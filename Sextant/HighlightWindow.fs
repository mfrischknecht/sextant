namespace Sextant

open System
open System.Diagnostics
open System.Windows
open System.Windows.Media
open System.Windows.Media.Animation

open Sextant.Rectangle
open Sextant.NativeWindow
open Sextant.Process
open Sextant.Workspace
open Sextant.Overlay
open System.Windows.Threading

module HighlightWindow =
    type WindowHighlight(window) as this =
        inherit WindowOverlay(window)

        do
            let brush = (byte 200, byte 0, byte 0, byte 0) |> Windows.Media.Color.FromArgb |> Windows.Media.SolidColorBrush
            this.Background <- brush

            NameScope.SetNameScope(this, NameScope ())
            this.RegisterName("brush",brush)

            let animation = ColorAnimation ()
            animation.From <- Colors.Green       |> Nullable
            animation.To   <- Colors.Transparent |> Nullable
            animation.Duration <- TimeSpan.FromMilliseconds(500.0) |> Duration
            animation.AutoReverse <- false
            animation.Completed.Add (fun _ -> this.Close ())

            let storyboard = Storyboard ()
            storyboard.Children.Add animation
            Storyboard.SetTargetName     (storyboard, "brush")
            Storyboard.SetTargetProperty (storyboard, SolidColorBrush.ColorProperty |> PropertyPath)

            this.IsVisibleChanged.Add (fun _ ->
                if this.IsVisible then
                    // let self = this |> Window.fromWPF
                    // window |> placeAbove self |> ignore
                    this.UpdateOverlay () |> ignore
                    // this.Topmost <- true
                    // this.ShowInTaskbar <- true
                    storyboard.Begin this
                    ) |> ignore

            this.Show ()
            this.Activate () |> ignore

    let highlight window =
        let overlay = WindowHighlight window
        () |> ignore