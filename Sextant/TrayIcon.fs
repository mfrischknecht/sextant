namespace Sextant

open System
open System.Windows

module TrayIcon =
    open System.Windows.Forms
    open System.Reflection
    open System.Resources
    open System.Drawing

    type TrayIcon() =
        let icon = new NotifyIcon()

        do
            let app = System.Windows.Application.Current
            let executingAssembly = Assembly.GetExecutingAssembly ()
            let resources = ResourceManager("ApplicationResources", executingAssembly)

            icon.Text <- "Sextant"
            icon.Icon <- resources.GetObject("TrayIcon") :?> Icon
            icon.Visible <- true

            app.Exit.Add (fun _ ->
                icon.Visible <- false
                icon.Icon <- null )

        interface IDisposable with
            member this.Dispose () =
                icon.Dispose()
