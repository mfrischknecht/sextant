namespace Sextant

open System
open System.Runtime.InteropServices

open Sextant.Rectangle
open Sextant.NativeAPI

module Monitor =

    [<Struct>]
    type Monitor =
        { Id:IntPtr }

    let bounds monitor =
        let mutable info = Desktop.MonitorInfoEx ()
        info.Size <- Marshal.SizeOf(info)

        let success = Desktop.GetMonitorInfo (monitor.Id, &info)
        if not success then
            let errorCode = Marshal.GetHRForLastWin32Error ()
            let error = Marshal.GetExceptionForHR (errorCode)
            raise error

        { Left   = info.WorkArea.Left   |> float
          Right  = info.WorkArea.Right  |> float
          Top    = info.WorkArea.Top    |> float
          Bottom = info.WorkArea.Bottom |> float }

    type Monitor with member this.Bounds = bounds this

    let getVisibleMonitors () =
        let monitors = Desktop.GetMonitors()
        monitors
        |> Seq.map (fun h -> { Id = h.Key })
        |> Array.ofSeq

