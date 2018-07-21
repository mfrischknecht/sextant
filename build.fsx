#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

open System.Diagnostics.CodeAnalysis

let args = 
    System.Environment.GetCommandLineArgs() 
    |> Array.skipWhile (fun str -> str <> "--") 
    |> function
    | [| |] -> [| |]
    | x -> x |> Array.skip 1

let setParams (defaults:DotNet.BuildOptions) =
    if args |> Seq.contains "--debug" |> not then defaults
    else { defaults with Configuration = DotNet.BuildConfiguration.Debug }

Target.create "Clean" (fun _ ->
    !! "**/bin"
    ++ "**/obj"
    |> Shell.cleanDirs 
)

Target.create "NativeAPI" (fun _ ->
    !! "**/NativeAPI.csproj"
    |> Seq.iter (DotNet.build setParams)
)

Target.create "Sextant" (fun _ ->
    !! "**/Sextant.fsproj"
    |> Seq.iter (DotNet.build setParams)
)

Target.create "Build" ignore
Target.create "All" ignore
Target.create "RebuildAll" ignore

"NativeAPI" ==> "Sextant" ==> "Build" ==> "All"

"Clean" ?=> "NativeAPI"
"Clean" ?=> "Sextant"
"Clean" ?=> "Build"
"Clean" ==> "RebuildAll"
"Build" ==> "RebuildAll"

[<SuppressMessage("Hints","*")>]
let exceptionChain (ex:System.Exception) =
    seq { let mutable e = ex
          while e <> null do
            yield e 
            e <- e.InnerException}

let build () =
    try 
        Target.runOrDefaultWithArguments "All"
    with
    | ex -> 
        Trace.traceError "Build failed!"

        ex |> exceptionChain 
        |> Seq.iter (fun e -> 
            Trace.traceError e.Message)

if args |> Seq.contains "--watch" |> not then
    build ()

else
    Trace.trace "Setting up file system watcher..."

    use watcher = 
        !! "**/.*proj" ++ "**/*.fs" ++ "**/*.cs"
        |> ChangeWatcher.run (ignore >> build)

    Trace.trace "Starting initial build..."
    build ()

    System.Console.ReadLine() |> ignore
