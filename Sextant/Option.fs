namespace Sextant

module Option =
    let ofResult =
        function
            | Ok    value -> Some value
            | Error _     -> None
    let nonEmptySeq =
        function
        | null -> None
        | seq when seq |> Seq.isEmpty -> None
        | seq -> Some seq

    let nonEmptyList =
        function
        | []   -> None
        | list -> Some list

    let nonEmptyArray =
        function
        | null  -> None
        | [| |] -> None
        | array -> Some array

    let nonEmptyString =
        function
        | null -> None
        | ""   -> None
        | str  -> Some str

module Options =
    let filterSome seq = 
        seq 
        |> Seq.filter Option.isSome 
        |> Seq.map Option.get