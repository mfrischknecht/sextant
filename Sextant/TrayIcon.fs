namespace Sextant

open System
open System.Windows

module TrayIcon =
    open System.Windows.Forms
    open System.Reflection
    open System.Resources
    open System.Drawing

    type TrayIcon() =
        let app = System.Windows.Application.Current
        let icon = 
            let executingAssembly = Assembly.GetExecutingAssembly ()
            let resources = ResourceManager("ApplicationResources", executingAssembly)

            new NotifyIcon (
                Text    = "Sextant",
                Icon    = (resources.GetObject("TrayIcon") :?> Icon),
                Visible = true)

        do
            app.Exit.Add (fun _ ->
                icon.Visible <- false
                icon.Icon    <- null )

        interface IDisposable with
            member this.Dispose () =
                icon.Dispose()
