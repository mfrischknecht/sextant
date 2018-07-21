﻿namespace Sextant

module Result =
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

