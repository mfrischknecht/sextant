#r "Sextant.exe"

open Sextant
open Sextant.App
open Sextant.ContextMenu
open Sextant.Hotkeys

"Foobar"    |> Log.info |> Log.log
"Hi there!" |> Log.info |> Log.log

let init (app:Sextant) =
    // app.Menu <- [
    //     MenuEntry ("Woot...", (fun _ -> "Woot" |> Log.info |> Log.log))
    // ]

    app.Hotkeys <- [
        ( (Key.VK_TAB, Modifier.Ctrl), 
          (fun _ -> Modes.enterMode OverlayMode.start ) )

        ( (Key.VK_TAB, Modifier.Ctrl ||| Modifier.Shift),
          (fun _ -> Modes.enterMode GridMode.start ) )
    ]
