namespace Sextant

open System
open System.Drawing
open System.Windows.Controls
open System.Windows.Media

open Sextant.Overlay
open Sextant.Rectangle
open Sextant.NativeWindow
open Sextant.WindowThumbnail
open Sextant.JumpCodes

module Thumbnails =
    type JumpCodeLabel() as this =
        inherit Label()

        let mutable jumpCode = JumpCode ""
        let mutable userInput = UserInput.None

        let text  = TextBlock ()
        let correctRun   = System.Windows.Documents.Run ()
        let incorrectRun = System.Windows.Documents.Run ()
        let untypedRun   = System.Windows.Documents.Run ()

        do
            let green   = (byte 100, byte 255, byte 100) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            let yellow  = (byte 255, byte 255, byte   0) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            let red     = (byte 255, byte   0, byte   0) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            let black   = (byte   0, byte   0, byte   0) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            let monospace = "Consolas" |> Windows.Media.FontFamily

            this.HorizontalAlignment <- Windows.HorizontalAlignment.Center
            this.VerticalAlignment   <- Windows.VerticalAlignment  .Center
            this.BorderBrush         <- yellow
            this.BorderThickness     <- 1.0 |> Windows.Thickness
            this.Background          <- black
            this.Content             <- text

            correctRun.FontSize           <- 36.0
            correctRun.FontFamily         <- monospace
            correctRun.Foreground         <- green
            correctRun.Background         <- correctRun.Foreground.Clone ()
            correctRun.Background.Opacity <- 0.1
            text.Inlines.Add(correctRun)

            incorrectRun.FontSize           <- 36.0
            incorrectRun.FontFamily         <- monospace
            incorrectRun.Foreground         <- red
            incorrectRun.Background         <- incorrectRun.Foreground.Clone ()
            incorrectRun.Background.Opacity <- 0.1
            text.Inlines.Add(incorrectRun)

            untypedRun.FontSize           <- 36.0
            untypedRun.FontFamily         <- monospace
            untypedRun.Foreground         <- yellow
            untypedRun.Background         <- untypedRun.Foreground.Clone ()
            untypedRun.Background.Opacity <- 0.1
            text.Inlines.Add(untypedRun)

        member this.JumpCode 
            with get ()    = jumpCode
            and  set value =
                jumpCode      <- value
                userInput <- UserInput.None
                correctRun  .Text <- ""
                incorrectRun.Text <- ""
                untypedRun.Text <- value.Value

        member this.UserInput
            with get ()    = userInput
            and  set value =
                userInput <- value

                let correct, incorrect, untyped = value |> JumpCodes.diff jumpCode
                correctRun.Text   <- correct
                incorrectRun.Text <- incorrect
                untypedRun.Text   <- untyped

        member this.IsMatch = jumpCode.Value.Length <> 0 && jumpCode.Value = userInput.Value

    type ThumbnailForm(window) as this =
        inherit WindowOverlay(window)

        let mutable thumbnail:Thumbnail option = None

        let updateThumbnail () = 
            if thumbnail.IsNone then
                let thumb = this.CoveredWindow |> WindowThumbnail.create (this |> Window.fromWPF)
                thumbnail <- thumb |> Option.ofResult

            let dstRect = Rectangle.None |> withSize (this.ActualWidth,this.ActualHeight)
            thumbnail.Value.Properties <- 
                { Properties.None with 
                    Visible     = Some true
                    Destination = Some dstRect }

        do
            this.IsVisibleChanged.Add (fun _ -> updateThumbnail ())
            this.SizeChanged.Add (fun _ -> updateThumbnail ())

            let black = (byte 0, byte 0, byte 0) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            this.Background <- black

            this.Closed.Add (fun _ ->
                if thumbnail.IsSome then
                    (thumbnail.Value :> IDisposable).Dispose ()
                    thumbnail <- None )

        member this.IsInitialized = thumbnail.IsSome

    type ThumbnailOverlayForm(covered:Window) as this =
        inherit WindowOverlay(covered)

        let titleLabel = Label ()
        let codeLabel  = JumpCodeLabel ()
        let image      = Image ()

        let mutable title      = ""
        let mutable windowCode = JumpCode ""
        let mutable typedCode  = UserInput.None

        do
            let white = (byte 255, byte 255, byte 255) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            let transparentBlack = (byte 100, byte 0, byte 0, byte 0) |> Windows.Media.Color.FromArgb |> Windows.Media.SolidColorBrush

            this.Background <- transparentBlack

            let grid = System.Windows.Controls.Grid ()
            grid.HorizontalAlignment <- Windows.HorizontalAlignment.Stretch
            grid.VerticalAlignment   <- Windows.VerticalAlignment  .Stretch
            this.AddChild(grid)

            titleLabel.Content                    <- ""
            titleLabel.HorizontalAlignment        <- Windows.HorizontalAlignment.Stretch
            titleLabel.VerticalAlignment          <- Windows.VerticalAlignment  .Stretch
            titleLabel.HorizontalContentAlignment <- Windows.HorizontalAlignment.Center
            titleLabel.VerticalContentAlignment   <- Windows.VerticalAlignment  .Top
            titleLabel.FontSize                   <- 36.0
            titleLabel.Padding                    <- (0.0, 25.0, 0.0, 0.0) |> Windows.Thickness
            titleLabel.Foreground                 <- white

            image.Width               <- 50.0
            image.Height              <- 50.0
            image.Margin              <- 20.0 |> System.Windows.Thickness
            image.Stretch             <- Stretch.UniformToFill
            image.HorizontalAlignment <- Windows.HorizontalAlignment.Left
            image.VerticalAlignment   <- Windows.VerticalAlignment  .Top

            grid.Children.Add titleLabel |> ignore
            grid.Children.Add codeLabel  |> ignore
            grid.Children.Add image      |> ignore

        let mutable icon = None
        let mutable wpfIcon = None
        member this.Icon
            with get () = icon
            and set newIcon =
                icon <- newIcon
                wpfIcon <- icon |> Option.map WPF.convertBitmap

                this.Dispatcher.Invoke (fun _ ->
                    image.Source <- wpfIcon |> Option.defaultValue null)

        member this.WindowTitle 
            with get () = title
            and set value = 
                title <- value
                titleLabel.Content <- value

        member this.WindowCode
            with get () = windowCode
            and set value =
                windowCode         <- value
                codeLabel.JumpCode <- value

        member this.TypedCode
            with get () = typedCode
            and set value =
                typedCode           <- value
                codeLabel.UserInput <- value

        member this.IsMatch = codeLabel.IsMatch
