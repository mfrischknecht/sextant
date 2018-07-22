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

        static let bg =
            "#C8000000" 
            |> Color.parse 
            |> Result.unwrap "Color parsing failed" 
            |> SolidColorBrush

        static let fromColor =
            "#FF2972e8"
            |> Color.parse 
            |> Result.unwrap "Color parsing failed" 
            |> Nullable

        static let toColor =
            "#002972e8"
            |> Color.parse 
            |> Result.unwrap "Color parsing failed" 
            |> Nullable

        static let duration = 500.0 |> TimeSpan.FromMilliseconds |> Duration

        do
            this.Background <- bg

            NameScope.SetNameScope(this, NameScope ())
            this.RegisterName("brush",bg)

            let animation = 
                ColorAnimation (
                    From        = fromColor, 
                    To          = toColor, 
                    Duration    = duration, 
                    AutoReverse = false)

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