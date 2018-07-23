namespace Sextant

open System
open System.Runtime.InteropServices

module NativeErrors =
    [<Struct>]
    type NativeError (code:int, hresult:int, ?annotations:string list) =
        member this.Code    = code
        member this.HResult = hresult

        member this.Annotations = defaultArg annotations []
        member this.ErrorText = 
            let ex = hresult |> Marshal.GetExceptionForHR
            ex.Message

        static member Last =
            let code    = Marshal.GetLastWin32Error      ()
            let hresult = Marshal.GetHRForLastWin32Error ()
            NativeError (code, hresult)

    let (|Success|Failure|) (error:NativeError) =
        match error.Code with
        | 0 -> Success
        | _ -> Failure

    let errorCode (error:NativeError) = error.Code
    let hresult   (error:NativeError) = error.HResult

    let ``exception`` error = 
        let hresult = error |> hresult 
        let mutable ex = hresult |> Marshal.GetExceptionForHR
        for message in error.Annotations |> Seq.rev do
            ex <- new InvalidOperationException (message, ex)
        ex

    let text (error:NativeError) =
        let text = error.Annotations |> List.append [ error.ErrorText ] |> Array.ofList
        String.Join ("\n", text)

    let annotate message (error:NativeError) =
        NativeError (error.Code, error.HResult, message :: error.Annotations)
