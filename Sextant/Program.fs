namespace Sextant

open System
open System.Windows.Forms
open System.Reflection
open System.IO

open Microsoft.FSharp.Core
open Microsoft.FSharp.Compiler.SourceCodeServices

open Sextant.NativeWindow
open Sextant.WPF
open Sextant.Hotkeys
open Sextant.ContextMenu
open System.Resources

module App =
    type IndicatorForm =
        { OriginalWindow:Window
          Form:Form }

    type Keys = Hotkeys.HotkeyCombination
    type Callback = Keys -> Unit
    type Keybinding = Keys * Callback


    type Sextant() =
        let app = System.Windows.Application ()
        let executingAssembly = Assembly.GetExecutingAssembly ()
        let logWindow = Log.LogWindow()
        do logWindow.Closing.Add (fun e -> logWindow.Hide(); e.Cancel <- true)

        do "Setting up tray icon..." |> Log.info |> Log.log

        let trayIcon = 
            use stream = executingAssembly.GetManifestResourceStream("Sextant.Resources.TrayIcon.ico")
            let icon  = new System.Drawing.Icon (stream)
            new NotifyIcon(Icon = icon)

        let mutable publicMenu = [ ]
        let privateMenu = [
            Divider
            MenuEntry ("Show Log...", (fun _ -> logWindow.Show()))
            Divider
            MenuEntry ("Exit",        (fun _ -> app.Shutdown()))
        ]
        let setMenu entries =
            publicMenu <- entries |> List.ofSeq
            trayIcon.ContextMenu <-  
                publicMenu
                |> Seq.prependTo privateMenu
                |> ContextMenu.create
                |> Result.unwrap "Unable to create tray menu"

        let mutable hotkeys = None
        let setHotkeys (keyBindings:Keybinding seq)  =
            hotkeys |> Option.iter Disposable.dispose
            hotkeys <- 
                keyBindings
                |> Option.nonEmptySeq
                |> Option.map (fun keys ->
                    Windows.Window ()
                    |> Window.fromWPF
                    |> Hotkeys.register keys)
            ()

        let registerHotkeys keybindings =
            app.AsyncDispatch (fun _ ->
                Process.exitIfAlreadyRunning()
                trayIcon.Visible <- true
                () ) |> ignore

        do
            setMenu []
            app.Exit.Add (fun _ ->
                Log.info "shutting down..." |> Log.log
                trayIcon.Visible <- false 
                trayIcon.Icon    <- null)

        member this.Menu
            with get ()    = publicMenu |> Seq.ofList
            and  set value = setMenu value

        member this.Hotkeys
            with set combinations = setHotkeys combinations

        member this.Run() =
            try 
                trayIcon.Visible <- true
                app.Run () |> ignore |> Ok
            with 
            | ex -> 
                ex |> Error.ofException |> Error

    [<EntryPoint>]
    [<STAThread>]
    let main argv = 
        let app = Sextant()

        let rcFile = 
            Script.load "rc.fsx"
            |> Result.bind (fun rc -> 
                let publicStatic = BindingFlags.Public ||| BindingFlags.Static
                let ``module`` = rc.Module
                let method = ``module``.GetMethod("init",publicStatic)

                try
                    method.Invoke(null, [| app |]) |> ignore |> Ok
                with
                | ``exception`` -> 
                    ``exception`` 
                    |> Error.ofException 
                    |> Error)

        rcFile
        |> Result.bind     app.Run
        |> Result.mapError Log.Entry.ofError
        |> Result.onError  Log.log
        |> function
        | Ok    _ ->  0
        | Error _ -> -1