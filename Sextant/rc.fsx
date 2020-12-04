#r "Sextant.exe"

open Sextant
open Sextant.App
open Sextant.ContextMenu
open Sextant.Hotkeys
open Sextant.JumpTargets
open Sextant.NativeWindow
open Sextant.Process

open Sextant.NativeAPI

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
      |> Array.filter (fun w -> w.IsOnCurrentDesktop )
      
    let printWindows () =
      findWindows ()
      |> Seq.iter (fun w -> w |> processName |> Log.info |> Log.log)
      
    let numberKeys = [
      Key.VK_1; Key.VK_2; Key.VK_3; Key.VK_4; Key.VK_5;
      Key.VK_6; Key.VK_7; Key.VK_8; Key.VK_9; Key.VK_0 
    ]

    let desktopHotkeys =
      numberKeys
      |> List.map (fun key -> (key, Modifier.Alt + Modifier.Ctrl))
      |> List.mapi (fun i key -> (key, (fun _ -> VirtualDesktop.SwitchToDesktop i ) ) )
      
    app.Hotkeys <- [

        ( (Key.VK_P, Modifier.Alt + Modifier.Ctrl), (fun _ -> printWindows () ) )

        ( (Key.VK_TAB, Modifier.Ctrl),
          (fun _ -> Modes.enterMode (findWindows >> OverlayMode.start) ) )

        ( (Key.VK_TAB, Modifier.Ctrl + Modifier.Shift),
          (fun _ -> Modes.enterMode (findWindows >> GridMode.start) ) )

        ( (Key.VK_K, Modifier.Ctrl + Modifier.Alt),
          (fun _ -> VirtualDesktop.SwitchToNextDesktop () ) )

        ( (Key.VK_J, Modifier.Ctrl + Modifier.Alt),
          (fun _ -> VirtualDesktop.SwitchToNextDesktop () ) )

    ] |> List.append desktopHotkeys