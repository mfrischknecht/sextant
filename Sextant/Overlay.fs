namespace Sextant

open System

open Sextant.Rectangle
open Sextant.NativeWindow
open Sextant.Monitor

module Overlay =

    type SplashWindow() as this =
        inherit System.Windows.Window()

        do
            let transparent = (byte 0, byte 0, byte 0, byte 0) |> Windows.Media.Color.FromArgb |> Windows.Media.SolidColorBrush 
            this.Background <- transparent
            this.ResizeMode <- System.Windows.ResizeMode.NoResize
            this.WindowStyle <- Windows.WindowStyle.None
            this.AllowsTransparency <- true
            this.ShowInTaskbar <- false

        member this.BringToFront () =
            let visible = this.IsVisible
            if visible then
                this.Hide()
                this.Show()

    type Overlay(getBounds:unit -> Rectangle option) =
        inherit SplashWindow()
        member this.UpdateOverlay () =
            let bounds = getBounds ()
            match bounds with
            | Some rect ->
                this.Left   <- rect |> topLeft |> fst
                this.Top    <- rect |> topLeft |> snd
                this.Width  <- rect |> width
                this.Height <- rect |> height
                true

            | None -> false

    type WindowOverlay(covered:Window) =
        inherit Overlay(fun _ -> covered |> windowBounds |> Option.ofResult)
        member this.CoveredWindow = covered

    let coveredWindow (overlay:WindowOverlay) = overlay.CoveredWindow

    type DesktopOverlay() =
        inherit Overlay(fun () -> 
            Desktop.getBounds () |> Some)

    type MonitorOverlay(monitor) =
        inherit Overlay(fun () ->
            monitor |> bounds|> Some)

        member this.Monitor = monitor

    let coveredMonitor (overlay:MonitorOverlay) = overlay.Monitor