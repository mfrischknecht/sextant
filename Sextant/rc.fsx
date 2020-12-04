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

    let findWindows () =
        let windows =
            JumpTargets.findWindows ()
            |> Array.filter (fun w ->
                let processName =
                  w.Process
                  |> Result.map name
                  |> Option.ofResult
                  |> Option.defaultValue ""

                ignoredProcesses |> List.contains processName |> not)

        // windows
        // |> Seq.iter (fun w ->
        //     let processName =
        //       w.Process
        //       |> Result.map name
        //       |> Option.ofResult
        //       |> Option.defaultValue ""

        //     processName |> Log.info |> Log.log)

        windows

    app.Hotkeys <- [
        ( (Key.VK_TAB, Modifier.Ctrl),
          (fun _ ->
               let startMode = findWindows >> OverlayMode.start
               Modes.enterMode startMode ) )

        ( (Key.VK_TAB, Modifier.Ctrl ||| Modifier.Shift),
          (fun _ ->
               let startMode = findWindows >> GridMode.start
               Modes.enterMode startMode ) )
    ]
