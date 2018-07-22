namespace Sextant

open System
open System.Windows.Forms
open System.Reflection

open Sextant.NativeWindow
open Sextant.WPF
open Sextant.Hotkeys

module App =

    type IndicatorForm =
        { OriginalWindow:Window
          Form:Form }

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        let executingAssembly = Assembly.GetExecutingAssembly ()

        use trayIcon = 
            use stream = executingAssembly.GetManifestResourceStream("Sextant.Resources.TrayIcon.ico")
            let icon  = new System.Drawing.Icon (stream)
            new NotifyIcon(Icon = icon)

        let mutable hotkeys = None

        let app = System.Windows.Application ()

        app.AsyncDispatch (fun _ ->
            Process.exitIfAlreadyRunning()
            trayIcon.Visible <- true

            let keybindings = [ 
                  ( (Key.VK_TAB, Modifier.Ctrl), 
                    (fun _ -> Modes.enterMode OverlayMode.start ) )

                  ( (Key.VK_TAB, Modifier.Ctrl ||| Modifier.Shift),
                    (fun _ -> Modes.enterMode GridMode.start ) )
                ]

            hotkeys <- 
                Windows.Window ()
                |> Window.fromWPF 
                |> Hotkeys.register keybindings 
                |> Option.Some

            () ) |> ignore

        app.Exit.Add (fun _ ->
            trayIcon.Visible <- false 
            trayIcon.Icon <- null )

        app.Run ()
