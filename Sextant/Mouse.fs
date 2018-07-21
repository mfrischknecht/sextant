namespace Sextant

open System
open System.Diagnostics

open Sextant.NativeErrors
open Sextant.NativeAPI

module Mouse =
    let pos () =
        let mutable pos = NativeAPI.Mouse.Point ()
        let result = NativeAPI.Mouse.GetCursorPos(&pos)
        if result then Ok (pos.x |> float, pos.y |> float)
        else Error (NativeError.Last |> annotate "Unable to query mouse cursor position")

    let setPos (x:float, y:float) =
        let result = NativeAPI.Mouse.SetCursorPos (x |> int, y |> int)
        if result then Ok ()
        else Error (NativeError.Last |> annotate "Unable to change mouse cursor position")