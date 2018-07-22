namespace Sextant

open System
open System.Windows
open System.Windows.Controls
open System.Windows.Forms
open System.Windows.Media

open Sextant.Rectangle
open Sextant.Overlay
open Sextant.NativeWindow
open Sextant.WindowThumbnail
open Sextant.Modes
open Sextant.Thumbnails
open Sextant.JumpCodes
open Sextant.HighlightWindow

module GridMode =
    let layout (width, height) numCells =
        let width  = Math.Max (1.0, width )
        let height = Math.Max (1.0, height)

        let configurations =
            Seq.init numCells ((+) 1)
            |> Seq.map (fun rows ->
                let cols = int (Math.Ceiling ((float numCells)/(float rows)))
                let w = width  / (float cols)
                let h = height / (float rows)
                let ratio = Math.Max (w/h,h/w)
                (ratio, (cols, rows)) )

        configurations |> Seq.minBy fst |> snd

    let centerIn dst src =
        let dstW, dstH = dst |> size
        let srcW, srcH = src |> size
        let x = (dst |> topLeft |> fst) + ((dstW - srcW) / 2.0)
        let y = (dst |> topLeft |> snd) + ((dstH - srcH) / 2.0)
        src |> moveTo (x,y)

    let scale s bounds = 
        let srcW, srcH = bounds |> size
        let w = srcW * s
        let h = srcH * s
        bounds |> withSize (w,h)

    let scaleDown dstSize src =
        let dstW, dstH = dstSize
        let srcW, srcH = src |> size
        let s = Math.Min(dstW / srcW, dstH / srcH)
        let s = Math.Min(s, 1.0)
        src |> scale s

    let scaleTo dstSize src =
        let dstW, dstH = dstSize
        let srcW, srcH = src |> size

        let dstR = dstW / dstH
        let srcR = srcW / srcH

        let s = 
            if srcR > dstR then 
                dstW / srcW
            else 
                dstH / srcH

        src |> scale s

    let permutations fast slow =
        seq {
            for s in slow do
                for f in fast do
                    yield f,s }

    type CellData = { Window:Window; Code:JumpCode;  }

    type Thumbnails(monitor) as this =
        inherit MonitorOverlay(monitor)
        let self = this |> Window.fromWPF
        let mutable thumbnails = [| |]

        let cellBounds (cols,rows) =
            let width  = Math.Max (this.Width,  this.ActualWidth )
            let height = Math.Max (this.Height, this.ActualHeight)
            let cellWidth  = width  / (cols |> float)
            let cellHeight = height / (rows |> float)
            let padding = 0.05 * Math.Max (cellWidth, cellHeight)
            let baseBounds = Rectangle.None |> withSize (cellWidth,cellHeight)

            let cellPositions = 
                permutations (Seq.init cols id) (Seq.init rows id) 
                |> Seq.map (fun (x,y) -> 
                    ( x |> float |> ((*) cellWidth), 
                      y |> float |> ((*) cellHeight) ) )

            cellPositions 
            |> Seq.map (fun pos -> 
                baseBounds |> moveTo pos |> pad padding)
            |> Array.ofSeq

        let placeIn cellBounds thumb =
            let bounds =
                if thumb |> sourceWindow |> isMinimized then
                    //DWM thumbs for minimized windows have a very small maximum size.
                    //Try to center them as good as possible anyway.
                    Rectangle.None 
                    |> withSize (200.0, 200.0) 
                    |> scaleDown (cellBounds |> size) 
                    |> centerIn cellBounds
                else
                    thumb 
                    |> sourceWindow 
                    |> NativeWindow.windowBounds |> Result.defaultValue Rectangle.None
                    |> scaleTo (cellBounds |> size)
                    |> centerIn cellBounds

            thumb.Properties <-
                { Properties.None with
                      Visible     = Some true
                      Destination = Some bounds }

        do
            let transparentBlack = (byte 200, byte 0, byte 0, byte 0) |> Windows.Media.Color.FromArgb |> Windows.Media.SolidColorBrush
            this.Background <- transparentBlack

        member this.Update layout windows =
            thumbnails |> Options.filterSome |> Seq.iter Disposable.dispose
            thumbnails <- windows |> Seq.map ((WindowThumbnail.create self) >> Option.ofResult) |> Array.ofSeq

            let cellBounds = layout |> cellBounds |> Array.ofSeq
            Seq.zip thumbnails cellBounds
            |> Seq.iter (fun (thumb,bounds) ->
                if thumb.IsSome then
                    thumb.Value |> placeIn bounds)

    type CellControl() as this =
        inherit Border()
        let mutable (data:CellData option) = None

        let grid = 
            Grid (
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment   = VerticalAlignment  .Stretch)
        let titleLabel = 
            Windows.Controls.Label (
                Content                    = "",
                HorizontalAlignment        = Windows.HorizontalAlignment.Stretch,
                VerticalAlignment          = Windows.VerticalAlignment  .Stretch,
                HorizontalContentAlignment = Windows.HorizontalAlignment.Center,
                VerticalContentAlignment   = Windows.VerticalAlignment  .Top,
                FontSize                   = 36.0,
                Padding                    = ((0.0, 25.0, 0.0, 0.0) |> Windows.Thickness),
                Foreground                 = Brushes.White)

        let codeLabel  = JumpCodeLabel ()
        let image = 
            Image (
                Width               = 50.0,
                Height              = 50.0,
                Margin              = (20.0 |> System.Windows.Thickness),
                Stretch             = Windows.Media.Stretch.UniformToFill,
                HorizontalAlignment = Windows.HorizontalAlignment.Left,
                VerticalAlignment   = Windows.VerticalAlignment  .Top )

        do
            let white = (byte 255, byte 255, byte 255) |> Windows.Media.Color.FromRgb |> Windows.Media.SolidColorBrush
            this.BorderBrush <- white
            this.Child <- grid

            grid.Children.Add titleLabel |> ignore
            grid.Children.Add codeLabel  |> ignore
            grid.Children.Add image      |> ignore

        member this.Data
            with get ()    = data
            and  set value = 
                data <- value
                match value with
                | Some data ->
                    grid.Visibility    <- Visibility.Visible
                    titleLabel.Content <- data.Window |> title
                    codeLabel.JumpCode <- data.Code
                    image.Source <- 
                        data.Window 
                        |> NativeWindow.icon 
                        |> Option.map WPF.convertBitmap 
                        |> Option.defaultValue null

                | _ -> 
                    grid.Visibility <- Visibility.Hidden

        member this.ApplyUserInput input =
            data
            |> Option.bind (fun data -> 
                codeLabel.UserInput <- input
                if codeLabel.IsMatch then Some data
                else None)

    type Labels(monitor) as this =
        inherit MonitorOverlay(monitor)

        let grid  = System.Windows.Controls.Grid ()
        let mutable cells = [| |]
        do
            let transparent = (byte 1, byte 0, byte 0, byte 0) |> Windows.Media.Color.FromArgb  |> Windows.Media.SolidColorBrush
            this.Background <- transparent
            this.AddChild grid

        member this.Update (cols,rows) data =
            grid.ColumnDefinitions.Clear()
            grid.RowDefinitions   .Clear()

            let width  = Math.Max(this.ActualWidth,  this.Width )
            let height = Math.Max(this.ActualHeight, this.Height)

            let cellWidth  = cols |> float |> ((/) 1.0) |> ((*) width ) |> GridLength
            for i in 1..cols do 
                let column = ColumnDefinition ()
                column.Width <- cellWidth
                grid.ColumnDefinitions.Add column

            let cellHeight = rows |> float |> ((/) 1.0) |> ((*) height) |> GridLength
            for i in 1..rows do
                let row = RowDefinition ()
                row.Height <- cellHeight
                grid.RowDefinitions.Add row

            cells <-
                cells 
                |> Seq.cat  (Seq.initInfinite (ignore >> CellControl))
                |> Seq.take (cols*rows)
                |> Array.ofSeq

            let cellPositions = permutations (Seq.init cols id) (Seq.init rows id) 
            for (x,y), cell in Seq.zip cellPositions cells do
                Grid.SetColumn (cell, x) |> ignore
                Grid.SetRow    (cell, y) |> ignore

            grid.Children.Clear ()
            cells |> Seq.iter (grid.Children.Add >> ignore)

            let data = 
                data 
                |> Seq.map Option.Some
                |> Seq.cat (Seq.initInfinite (fun _ -> None)) 
                |> Seq.take (cols*rows)

            for data, cell in Seq.zip data cells do
                cell.Data <- data

        member this.ApplyUserInput input =
            cells 
            |> Seq.map (fun cell -> cell.ApplyUserInput input) 
            |> Seq.filter Option.isSome 
            |> Seq.map Option.get
            |> Seq.tryHead

    type Overlay() as this =
        inherit DesktopOverlay()

        let mutable windows    = [| |]
        let mutable userInput  = UserInput.None
        let mutable closed = false
        let monitors = Monitor.getVisibleMonitors()
        let thumbOverlays = monitors |> Seq.map (fun m -> m, Thumbnails m) |> Map
        let labelOverlays = monitors |> Seq.map (fun m -> m, Labels     m) |> Map

        let updateInput input =
            userInput <- input

            monitors
            |> Seq.map (fun monitor ->
                let labels = labelOverlays.[monitor]
                labels.ApplyUserInput input)
            |> Options.filterSome
            |> Seq.tryHead

        let update () =
            userInput |> updateInput |> ignore
            windows   <- 
                JumpTargets.findWindows () 
                |> JumpTargets.orderByPosition 
                |> Array.ofSeq

            let numWindows = windows |> Seq.length
            let data =
                windows
                |> Seq.mapi (fun i w -> { Window = w; Code = JumpCodes.Code numWindows i })
                |> Seq.groupBy (fun d -> d.Window |> monitor)
                |> Map

            for monitor in monitors do
                let thumbs = thumbOverlays.[monitor]
                let labels = labelOverlays.[monitor]
                let data = data.[monitor]
                let windows = data |> Seq.map (fun d -> d.Window) |> Array.ofSeq
                let monitorSize = monitor |> Monitor.bounds |> size 
                let layout  = windows |> Seq.length |> layout monitorSize
                windows |> thumbs.Update layout
                data    |> labels.Update layout

        let eventWindowBlacklist = [
                thumbOverlays |> Map.values |> Seq.map Window.fromWPF |> Array.ofSeq
                labelOverlays |> Map.values |> Seq.map Window.fromWPF |> Array.ofSeq
                [| this |> Window.fromWPF |]
            ] 
        let eventWindowBlacklist = eventWindowBlacklist |> Seq.collect id |> Array.ofSeq

        let windowEventHandler =  
            Handler<_> (fun sender (window, event) ->
                if not (eventWindowBlacklist |> Seq.contains window) then
                    // let isTopWindow = window |> parent |> Option.ofResult |> Option.isNone
                    // let windowOfInterest = windows |> Seq.contains window
                    // match isTopWindow && windowOfInterest, event with
                    match event with
                    | WindowEvent.EVENT_OBJECT_HIDE 
                    | WindowEvent.EVENT_OBJECT_SHOW 
                    | WindowEvent.EVENT_OBJECT_LOCATIONCHANGE -> 
                        update ()
                        if not this.IsFocused then
                            this.Activate () |> ignore
                            this |> Window.fromWPF |> focus |> ignore
                    | _ -> () )

        do
            let transparent = (byte 1, byte 0, byte 0, byte 0) |> Windows.Media.Color.FromArgb |> Windows.Media.SolidColorBrush
            this.Background <- transparent

            for monitor in monitors do
                let thumbs = thumbOverlays.[monitor]
                let labels = labelOverlays.[monitor]
                thumbs.UpdateOverlay () |> ignore
                labels.UpdateOverlay () |> ignore

            this.IsVisibleChanged.Add (fun _ -> 
                if this.IsVisible then
                    this.Topmost <- true
                    let self = this |> Window.fromWPF
                    for monitor in monitors do
                        let thumbnails = thumbOverlays.[monitor]
                        let labels     = labelOverlays.[monitor]

                        labels.Show () |> ignore
                        labels |> Window.fromWPF |> placeBelow self |> ignore 

                        thumbnails.Show () |> ignore
                        thumbnails |> Window.fromWPF |> placeBelow (labels |> Window.fromWPF)  |> ignore

                    this.Dispatcher.InvokeAsync (fun _ ->
                        this.Activate () |> ignore 
                        WindowEvent.AddHandler windowEventHandler
                        ) |> ignore

                else
                    WindowEvent.RemoveHandler windowEventHandler
                    for thumb, labels in Seq.zip thumbOverlays labelOverlays do
                        thumb .Value.Visibility <- Visibility.Hidden
                        labels.Value.Visibility <- Visibility.Hidden )

            this.Closed.Add (fun _ ->
                for thumb, labels in Seq.zip thumbOverlays labelOverlays do
                    thumb .Value.Close ()
                    labels.Value.Close () )

            this.KeyDown.Add (function
                | None -> ()
                | CloseOverlay _ -> this.Close()
                | CodeKey key -> 
                    key |> apply userInput |> updateInput
                    |> function
                        | Some data ->
                            this.Close()

                            data.Window |> JumpTargets.activate
                            |> Result.onError (Log.Entry.ofNativeError >> Log.log)
                            |> ignore

                            data.Window |> highlight

                        | _ -> () )
            update ()

        interface Mode with
            member this.Exit () =
                if not closed then
                    closed <- true
                    WindowEvent.RemoveHandler windowEventHandler
                    this.Close ()

    let start() =
        let mode = Overlay ()
        mode.UpdateOverlay () |> ignore
        mode.Show ()
        Some (mode :> Mode)