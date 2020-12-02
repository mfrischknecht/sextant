namespace Sextant

open System
open System.Reflection

open Microsoft.FSharp.Core
open System.Diagnostics.CodeAnalysis
open FSharp.Compiler.SourceCodeServices

module Script =
    type Script private (assembly:System.Reflection.Assembly) =
        member this.Assembly = assembly
        member this.Module =
            assembly.GetTypes()
            |> Seq.filter (fun t -> 
                t.GetCustomAttributes<CompilationMappingAttribute>(true)
                |> Seq.map (fun a -> a.SourceConstructFlags)
                |> Seq.exists ((=) SourceConstructFlags.Module))
            |> Seq.head

        [<SuppressMessage("NameConvention","*")>]
        static member compile file =
            let checker = FSharpChecker.Create()
            let errors, exitCode, assembly = 
                checker.CompileToDynamicAssembly([| "-a"; file; "-o"; file |> sprintf "%s.dll" |], execute = None)
                |> Async.RunSynchronously

            assembly
            |> Option.map Script
            |> Result.nonEmptyOption
            |> Result.mapError (fun _ -> 
                errors 
                |> Seq.map (fun e -> Error.ofExternal e.Message e)
                |> List.ofSeq
                |> Error.combine "Script compilation failed")

    let private initialize (script:Script) =
        try
            let initMethods =
                script.Assembly.GetTypes()
                |> Seq.filter  (fun t -> t.Namespace <> null)
                |> Seq.filter  (fun t -> t.Namespace.Contains "StartupCode")
                |> Seq.filter  (fun t -> not t.IsGenericType)
                |> Seq.collect (fun t -> t.GetMethods(BindingFlags.Public ||| BindingFlags.Static))
                |> Seq.filter  (fun m -> 
                    [ m.Name = "main@"
                      not m.IsGenericMethod
                      m.GetParameters() |> Seq.isEmpty ] 
                    |> Seq.fold (&&) true)
                |> Array.ofSeq

            initMethods
            |> Seq.iter (fun m ->
                m.Invoke(null, [| |]) |> ignore)

            script |> Ok
        with
        | ``exception`` -> 
            ``exception`` 
            |> Error.ofException 
            |> Result.Error

    let load file =
        file 
        |> Script.compile
        |> Result.bind initialize
    