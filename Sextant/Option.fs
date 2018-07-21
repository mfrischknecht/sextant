namespace Sextant

module Option =
    let ofResult =
        function
            | Ok    value -> Some value
            | Error _     -> None

module Options =
    let filterSome seq = 
        seq 
        |> Seq.filter Option.isSome 
        |> Seq.map Option.get