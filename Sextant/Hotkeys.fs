namespace Sextant

open System
open System.Collections.Generic
open System.Diagnostics.CodeAnalysis
open System.Threading
open System.Runtime.InteropServices
open System.Windows.Interop

open Sextant.NativeAPI
open Sextant.NativeErrors
open Sextant.NativeWindow

module Hotkeys =
    type Modifier = Keyboard.HotkeyModifier
    type Key      = Keyboard.HotkeyKey

    let mutable private staticId = 0
    let private generateId () = Interlocked.Increment (&staticId)

    type private HotkeyEvent private (id,window,key,modifiers) =
        let event = Event<_> ()
        let hook = HwndSourceHook(fun _ message param1 _ _ ->
            if message = Keyboard.WM_HOTKEY && param1.ToInt32() = id then
                event.Trigger (key, modifiers) 
            IntPtr.Zero )
        let source = 
            let tmp = window |> handle |> HwndSource.FromHwnd
            tmp.AddHook hook
            tmp

        member private this.ID = id
        member private this.Window = window
        member private this.Source = source
        member this.Event = event

        [<SuppressMessage("NameConventions","*")>]
        static member register (key,modifiers) window =
            let id = generateId ()
            let success = Keyboard.RegisterHotKey (window |> handle, id, modifiers, key)
            if success then 
                (id,window,key,modifiers) |> HotkeyEvent |> Result.Ok
            else
                Error (NativeError.Last |> annotate "Unable to register global hotkey")

        [<SuppressMessage("NameConventions","*")>]
        static member unregister (event:HotkeyEvent) =
            let success = Keyboard.UnregisterHotKey (event.Window |> handle, event.ID)
            if success then
                event.Source.Dispose ()
                () |> Result.Ok
            else 
                NativeError.Last |> annotate "Failed to unregister a global hotkey" |> Result.Error

    type HotkeyHandler(window) =
        let mutable observers = [| |] |> Map

        member this.Unregister keyCombination =
            observers 
            |> Map.tryFind keyCombination
            |> function
                | Some event ->
                    event |> HotkeyEvent.unregister |> Result.onError (text >> Console.Error.WriteLine) |> ignore
                    observers <- observers |> Map.remove keyCombination
                | _ -> ()

        member this.Register keyCombination callback =
            window 
            |> HotkeyEvent.register keyCombination
            |> Result.map (fun event ->
                event.Event.Publish.Add callback
                observers <- observers |> Map.add keyCombination event
                { new IDisposable with 
                      member self.Dispose () =
                          this.Unregister keyCombination })

        interface IDisposable with 
            member this.Dispose () =
                let keys = observers |> Map.keys
                keys |> Seq.iter this.Unregister

    let register callbacks window =
        let handler = new HotkeyHandler (window)
        callbacks
        |> Seq.map (fun (keys,callback) -> 
            handler.Register keys callback 
            |> Result.onError (text >> Console.Error.WriteLine))
        |> Seq.iter ignore
        handler