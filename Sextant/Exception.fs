namespace Sextant

open System
open System.Diagnostics.CodeAnalysis

module Exception =
    [<SuppressMessageAttribute("Hints","*")>]
    let chain (ex:Exception) =
        seq {
            let mutable e = ex
            while e <> null do 
                yield e }
