namespace Sextant

open System
open System.Runtime.InteropServices

open Sextant.Rectangle
open Sextant.NativeAPI

module Desktop =

    let getBounds () =
        let left   = Desktop.GetSystemMetrics(Desktop.SystemMetric.SM_XVIRTUALSCREEN ) |> float
        let top    = Desktop.GetSystemMetrics(Desktop.SystemMetric.SM_YVIRTUALSCREEN ) |> float
        let width  = Desktop.GetSystemMetrics(Desktop.SystemMetric.SM_CXVIRTUALSCREEN) |> float
        let height = Desktop.GetSystemMetrics(Desktop.SystemMetric.SM_CYVIRTUALSCREEN) |> float
        Rectangle.create (left,top) (width,height)