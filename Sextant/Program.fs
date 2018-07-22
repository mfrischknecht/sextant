namespace Sextant

open System
open System.Windows.Forms
open System.Reflection

open Sextant.NativeWindow
open Sextant.WPF
open Sextant.Hotkeys
open Sextant.ContextMenu

module App =

    type IndicatorForm =
        { OriginalWindow:Window
          Form:Form }

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        let app = System.Windows.Application ()
        let executingAssembly = Assembly.GetExecutingAssembly ()
        let logWindow = Log.LogWindow()

        Log.info "Setting up tray icon..." |> Log.log

        use trayIcon = 
            use stream = executingAssembly.GetManifestResourceStream("Sextant.Resources.TrayIcon.ico")
            let icon  = new System.Drawing.Icon (stream)
            new NotifyIcon(Icon = icon)

        logWindow.Closing.Add (fun e ->
            logWindow.Hide()
            e.Cancel <- true)

        trayIcon.DoubleClick.Add (fun _ ->
            logWindow.Show()
            logWindow.Activate() |> ignore
            logWindow.Focus() |> ignore)

        trayIcon.ContextMenu <- [
                MenuEntry ("Show Log...", (fun _ -> logWindow.Show()))
                Divider
                MenuEntry ("Exit",        (fun _ -> app.Shutdown()))
            ] 
            |> ContextMenu.create
            |> Result.unwrap "Unable to create tray menu"

        let mutable hotkeys = None

        app.AsyncDispatch (fun _ ->
            Process.exitIfAlreadyRunning()
            trayIcon.Visible <- true

            Log.info "Setting up global hotkeys..." |> Log.log

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
            Log.info "shutting down..." |> Log.log
            trayIcon.Visible <- false 
            trayIcon.Icon    <- null )

        app.Run ()
