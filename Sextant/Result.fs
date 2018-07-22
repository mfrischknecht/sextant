namespace Sextant

module Result =
    let nonEmptyOption =
        function
            | Some x -> Ok x
            | None   -> Error ()

    let defaultValue ``default`` =
        function
            | Ok    value -> value
            | Error _     -> ``default``
        
    let defaultWith constructDefault =
        function
            | Ok    value -> value
            | Error _     -> constructDefault ()

    let onSuccess callback =
        function
            | Ok value ->
                callback value
                Ok value
            | Error err -> 
                Error err

    let onError callback =
        function
            | Ok value -> 
                Ok value
            | Error err -> 
                callback err
                Error err

    let tryWith fn =
        try
            let result = fn ()
            Ok result
        with
        | ex -> Error ex

    let unwrap text = 
        function
        | Ok x -> x
        | Error e -> sprintf "Logic error: unexpected error result: %s (Error: %A)" text e |> failwith

