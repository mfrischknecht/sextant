#r "Sextant.exe"

open Sextant
open Sextant.App
open Sextant.ContextMenu
open Sextant.Hotkeys
open Sextant.JumpTargets
open Sextant.NativeWindow
open Sextant.Process

let init (app:Sextant) =
    "Starting `rc.fsx`..." |> Log.info |> Log.log

    // app.Menu <- [
    //     MenuEntry ("Hi there! :)", (fun _ -> "Woot" |> Log.info |> Log.log))
    // ]

    let ignoredProcesses = [
      "TextInputHost"
      "WinStore.App"
      "Calculator"
      "SystemSettings"
      "ApplicationFrameHost"
      "PaintStudio.View"
      "Video.UI"
    ]
    
    let processName (window: Window) =
      window.Process
      |> Result.map name
      |> Option.ofResult
      |> Option.defaultValue ""
    
    let isIgnored (window: Window) =
      ignoredProcesses |> List.contains (processName window)
      
    let findWindows () =
      JumpTargets.findWindows ()
      |> Array.filter (isIgnored >> not)
      
    let printWindows () =
      findWindows ()
      |> Seq.iter (fun w -> w |> processName |> Log.info |> Log.log)
      
    app.Hotkeys <- [

        ( (Key.VK_P, Modifier.Alt + Modifier.Ctrl), (fun _ -> printWindows () ) )

        ( (Key.VK_TAB, Modifier.Ctrl),
          (fun _ -> Modes.enterMode (findWindows >> OverlayMode.start) ) )

        ( (Key.VK_TAB, Modifier.Ctrl + Modifier.Shift),
          (fun _ -> Modes.enterMode (findWindows >> GridMode.start) ) )

    ]