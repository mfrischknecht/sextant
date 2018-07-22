namespace Sextant

open System.Diagnostics

open Sextant.Rectangle
open Sextant.NativeWindow
open Sextant.Process
open Sextant.Workspace

module JumpTargets =
    let currentProcess = Process.GetCurrentProcess ()
    let ignoredClasses = [ "Progman" ]

    let findWindows () =
        let isIn seq item = 
            seq |> Seq.contains item

        let filterWindows =
            Seq.filter isVisible 
            >> Seq.filter hasTitle 
            >> Seq.filter (windowClass >> (isIn ignoredClasses) >> (not))
            >> Seq.filter (windowBounds >> Result.defaultValue Rectangle.None >> area >> ((<) 0.0))
            >> Seq.filter (fun w ->
                let proc = w.Process |> Option.ofResult
                let pid = proc |> Option.map id        |> Option.defaultValue -1
                let sid = proc |> Option.map sessionId |> Option.defaultValue -1
                let keep = pid <> currentProcess.Id && sid = currentProcess.SessionId
                keep)
            >> Array.ofSeq

        currentWorkspace ()
        |> Result.map windows
        |> Option.ofResult
        |> Option.defaultValue [| |]
        |> filterWindows
        |> Array.ofSeq

    //<summary>Orders windows by their distance from the desktop's point of origin.</summary>
    //This function will use additional (static) window-specific information
    //(process id, window handle) to guarantee a stable order even if two windows
    //have the same position and their position is swapped in the input sequence.
    //This is useful, since findWindows' order is determined by the z-position
    //of the respective windows.
    let orderByPosition windows =
        windows 
        |> Seq.sortByDescending (fun window -> 
            let topLeft = 
                Desktop.getBounds() 
                |> Rectangle.topLeft

            let dist = 
                window 
                |> NativeWindow.clientBounds 
                |> Result.map Rectangle.topLeft 
                |> Result.map (Geometry.distanceSquaredTo topLeft)
                |> Result.defaultValue infinity

            let pid = 
                window.Process
                |> Result.map (fun p -> p.Id)
                |> Result.defaultValue 0

            let handle = window |> NativeWindow.handle

            (dist, pid, handle.ToInt64()))

    let activate window =
        let result =
            use sync = window |> synchronize
            let minimized = window |> isMinimized

            Ok ()
            |> Result.bind (fun _ -> 
                if minimized then window |> restore
                else Ok () ) //If the window is not minimized, we don't need to use `restore` (this would also un-maximize them)
            |> Result.bind (fun _ -> window |> toForeground)
            |> Result.bind (fun _ -> window |> activate    )
            |> Result.bind (fun _ -> 
                if not minimized then window |> focus
                else Ok () ) //Restored windows are focused anyway, but `focus` will fail

        result
        |> Result.bind (fun _ -> 
            if window |> isMinimized then 
                window |> restoreBounds
            else 
                window |> windowBounds )
        |> Result.bind (Rectangle.center >> Mouse.setPos)