namespace Sextant

open System
open System.Diagnostics.CodeAnalysis
open System.Collections.Generic

open Sextant.Rectangle
open Sextant.NativeAPI
open Sextant.Overlay
open Sextant.NativeWindow
open Sextant.Thumbnails
open Sextant.JumpCodes
open Sextant.WindowThumbnail
open Sextant.Process
open Sextant.Modes
open Sextant.HighlightWindow

module OverlayMode =

    type Child = 
        { Window:Window
          Thumbnail:ThumbnailForm
          Overlay:ThumbnailOverlayForm }

        [<SuppressMessage("NameConventions","*")>]
        static member forWindow (window:Window) =
            let id = window.Handle.ToInt64()
            let t  = window |> ThumbnailForm
            let o = t |> Window.fromWPF |> ThumbnailOverlayForm
            { Window = window; Thumbnail = t; Overlay = o }

    let window    (child:Child) = child.Window
    let thumbnail (child:Child) = child.Thumbnail
    let overlay   (child:Child) = child.Overlay

    let createChildren windows = 
        windows 
        |> Seq.map (fun w -> 
            w, w |> Child.forWindow) 
        |> Map.ofSeq

    let updateChildren oldChildren (windows:Window[]) =
        let deleted = 
            oldChildren |> Map.keys
            |> Seq.except windows 
            |> Seq.map (fun w -> oldChildren |> Map.find w)
            |> Array.ofSeq

        let newLookup =
            let getOrConstructChild window =
                oldChildren
                |> Map.tryFind window
                |> Option.defaultWith (fun () -> 
                    window |> Child.forWindow)

            windows
            |> Seq.map (fun window -> window, window |> getOrConstructChild)
            |> Map.ofSeq

        (newLookup, deleted)

    type MainOverlay() as this =
        inherit DesktopOverlay()

        let mutable ignoreDeactivates = false
        let mutable children = Seq.empty |> createChildren
        let mutable typedCode = UserInput.None

        let updateThumbnails (windows:Window[]) =
            ignoreDeactivates <- true
            let ``new``, deleted = updateChildren children windows

            for child in deleted do 
                child.Overlay  .Close()
                child.Thumbnail.Close()

            children <- ``new``

            children
            |> Map.values
            |> Seq.filter (fun child -> not child.Thumbnail.IsInitialized)
            |> Seq.iter (fun child ->
                child.Thumbnail.Left   <- float -1000
                child.Overlay  .Left   <- float -1000
                child.Thumbnail.Width  <- float 10
                child.Overlay  .Width  <- float 10
                child.Thumbnail.Height <- float 10
                child.Overlay  .Height <- float 10 )

            let jumpTargets =
                windows 
                |> Seq.map (fun w -> children |> Map.find w)
                |> Array.ofSeq

            let windowIndexes =
                let windowIndices =
                    jumpTargets
                    |> Seq.map (fun t -> t.Window)
                    |> JumpTargets.orderByPosition
                    |> Seq.mapi (fun i w -> (w.Handle.ToInt64(),i))
                    |> dict

                jumpTargets |> Array.map (fun t -> windowIndices.[t.Window.Handle.ToInt64()])

            jumpTargets |> Seq.iteri (fun i child -> 
                let procName = 
                    child.Window.Process 
                    |> Option.ofResult
                    |> Option.map name
                    |> Option.defaultValue ""

                let index = windowIndexes.[i]
                child.Overlay.WindowCode  <- (JumpCodes.Code windows.Length index)
                child.Overlay.WindowTitle <- sprintf "%s\n%s" child.Window.Title procName
                child.Overlay.Icon        <- child.Window.Icon )

            //"Build" the overlays of the jump targets from the top, so
            //there aren't too many visual artefacts (the other way around, 
            //there'd be windows from the bottom popping up).
            this.Dispatcher.InvokeAsync (fun _ ->
                let mutable lastWindow = this |> Window.fromWPF
                let desktopBounds = Desktop.getBounds ()
                jumpTargets 
                |> Seq.iter (fun child ->
                    let success =
                        child.Thumbnail.UpdateOverlay() &&
                        child.Overlay  .UpdateOverlay()

                    if success then
                        //Move the overlays off-screen first in order to avoid them flashing up over other content
                        child.Thumbnail.Left <- desktopBounds.Left - child.Thumbnail.Width - 100.0
                        child.Overlay  .Left <- desktopBounds.Left - child.Overlay  .Width - 100.0

                        child.Overlay.Show ()
                        child.Overlay |> Window.fromWPF |> placeBelow lastWindow |> ignore
                        lastWindow <- child.Overlay |> Window.fromWPF

                        child.Thumbnail.Show()
                        child.Thumbnail |> Window.fromWPF |> placeBelow lastWindow |> ignore
                        lastWindow <- child.Thumbnail |> Window.fromWPF

                        //Move the overlays back into place
                        child.Thumbnail.UpdateOverlay() |> ignore
                        child.Overlay  .UpdateOverlay() |> ignore )

                this.Activate () |> ignore
                this.Topmost <- true
                ignoreDeactivates <- false ) |> ignore


        let updateCode newCode =
            typedCode <- newCode

            for child in children |> Map.values do
                child.Overlay.TypedCode <- typedCode

            children 
            |> Map.tryPick (fun _ child -> 
                let code = child.Overlay.WindowCode
                match code.Value = typedCode.Value with
                | true  -> Some child
                | false -> None)
            |> Option.map (fun child ->
                this.Close()

                child |> window |> JumpTargets.activate 
                |> Result.onError (Log.Entry.ofNativeError >> Log.log) 
                |> ignore 
                
                child |> window |> highlight )

            |> ignore

        do
            //Avoid a click-through behavior on transparent backgrounds
            let bg = System.Windows.Media.Color.FromArgb (byte 1, byte 0, byte 0, byte 0)
            this.Background <- bg |> System.Windows.Media.SolidColorBrush

            this.Closing.Add (fun _ -> ignoreDeactivates <- true)

            this.Deactivated.Add (fun _ -> 
                if not ignoreDeactivates then
                    this.Close())

            this.Closed.Add (fun _ ->
                children 
                |> Map.values
                |> Seq.rev
                |> Seq.iter (fun child ->
                    child.Thumbnail.Hide()
                    child.Overlay  .Hide()
                    child.Thumbnail.Close()
                    child.Overlay  .Close() )

                children <- Seq.empty |> Map )

            this.KeyDown.Add (function
                | None -> ()
                | CloseOverlay _ -> this.Close()
                | CodeKey key -> 
                    let newCode = key |> apply typedCode
                    updateCode newCode)

        member this.UpdateThumbnails windows = 
            updateThumbnails windows

        interface Mode with
            member this.Exit () =
                this.Close ()
    
    let start() =
        let mode = MainOverlay ()

        let windows = JumpTargets.findWindows () |> Array.ofSeq

        mode.Show ()
        mode.UpdateThumbnails windows
        mode.Activate() |> ignore 
        Some (mode :> Mode)
