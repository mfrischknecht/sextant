namespace Sextant

module Modes =
    type Mode =
        abstract member Exit: unit -> unit

    let mutable private mode:Mode option = None

    let currentMode () = mode
    let exitMode () =
        match mode with
        | Some m -> m.Exit()
        | _ -> ()
        mode <- None

    let enterMode activateNextMode =
        exitMode ()
        mode <- activateNextMode ()

