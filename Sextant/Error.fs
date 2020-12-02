namespace Sextant

open System
open System.Reflection

open Microsoft.FSharp.Core
open System.Diagnostics.CodeAnalysis

open Sextant.NativeErrors

module Error =
    type Source =
    | Text          of string
    | NativeError   of NativeError
    | Exception     of Exception
    | ExternalError of string * obj
    | Error         of string * Error
    | Multiple      of string * Error list

    and Error(source:Source) =
        let rec createMessage src : string =
            match src with
            | Text error -> error

            | NativeError error -> 
                sprintf "Native error: %s" error.ErrorText

            | Exception error -> 
                sprintf "Exception `%s`: %s\r\n%s" 
                    (error.GetType().Name) 
                    error.Message 
                    (error.StackTrace.ToString() |> Text.indent)

            | ExternalError (msg, error) ->
                sprintf "External error (%s): %s" (error.GetType().Name) msg

            | Error (msg, error) -> 
                error.Message 
                |> sprintf "Inner error: %s"
                |> Text.indent 
                |> sprintf "%s\r\n%s" msg

            | Multiple (msg, errors) ->
                let getSource (error:Error) = error.Source

                errors 
                |> Seq.map  (getSource >> createMessage >> Text.indent)
                |> Seq.mapi (sprintf "\r\nInner error #%i:\r\n%s")
                |> Seq.appendTo [ msg ]
                |> String.concat "\r\n"
        member this.Source = source
        member this.Message = source |> createMessage

    let ofText        = Text          >> Error
    let ofNativeError = NativeError   >> Error
    let ofException   = Exception     >> Error
    let ofExternal message error  = (message,error ) |> ExternalError   |> Error
    let combine    message errors = (message,errors) |> Source.Multiple |> Error
    let annotate   message error  = (message,error ) |> Source.Error    |> Error