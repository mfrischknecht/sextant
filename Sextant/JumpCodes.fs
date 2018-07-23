namespace Sextant

open System
open System.Text

module JumpCodes =

    let (|CloseOverlay|CodeKey|None|) (args:Windows.Input.KeyEventArgs) =
        if args.Key >= Windows.Input.Key.A && args.Key <= Windows.Input.Key.Z then CodeKey args.Key
        else
            match args.Key with
            | Windows.Input.Key.Back -> CodeKey args.Key
            | Windows.Input.Key.Escape -> CloseOverlay args.Key
            | _ -> None

    type JumpCode = JumpCode  of string with
        member this.Value =
            match this with
            | JumpCode code -> code

    let characters = [ 'a'; 's'; 'd'; 'f'; 'g'; 'h'; 'j'; 'k'; 'l' ]

    let Code numCodes index =
        let numCodes = Math.Max (numCodes, 2)
        let numChars = int (Math.Ceiling ( Math.Log(float numCodes) / Math.Log(float characters.Length)))
        let code = numChars |> StringBuilder
        let mutable i = index
        for j in 1..numChars do
            code.Append (characters.[i % characters.Length]) |> ignore
            i <- i / characters.Length
        JumpCode (code.ToString ())

    type UserInput = UserInput of string with
        static member None = UserInput ""

        member this.Value =
            match this with
            | UserInput input -> input

    let apply (input:UserInput) key =
        if key >= Windows.Input.Key.A && key <= Windows.Input.Key.Z then
            let builder = StringBuilder(input.Value,input.Value.Length+1)
            let char = key.ToString().ToLower()
            builder.Append char |> ignore
            UserInput (builder.ToString())
        elif key = Windows.Input.Key.Back then
            //Note: 0 means there will be 1 char left, -1 yields an empty string
            let ``end`` = Math.Max (input.Value.Length-2,-1) 
            UserInput input.Value.[0..``end``]
        else
            input

    let diff (code:JumpCode) (input:UserInput) =
        let code  = code .Value
        let input = input.Value

        let mutable matches = true
        let length = Math.Max (code.Length, input.Length)
        let correct   = length |> StringBuilder
        let incorrect = length |> StringBuilder
        let untyped   = length |> StringBuilder

        for i in 0..length-1 do
            match (code.Length > i, input.Length > i) with
                | true, true -> 
                    if not matches then
                        incorrect.Append (code.[i]) |> ignore
                    elif code.[i] <> input.[i] then
                        matches <- false
                        incorrect.Append (code.[i]) |> ignore
                    else
                        correct.Append (code.[i]) |> ignore

                | true, false ->
                    untyped.Append (code.[i]) |> ignore

                | false, true ->
                    incorrect.Append "?" |> ignore

                | _ -> ()

        (correct.ToString(), incorrect.ToString(), untyped.ToString())

    let equals (code:JumpCode) (input:UserInput) =
        let code  = code .Value
        let input = input.Value
        code = input