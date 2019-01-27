#load ".fake/build.fsx/intellisense.fsx"
open Fake.IO

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

Target.create "Clean" (fun _ ->
    !! "**/bin"
    ++ "**/obj"
    |> Shell.cleanDirs 
)


if args |> Seq.contains "--msbuild" then

    let setParams (defaults:MSBuildParams) =
        let debug = args |> Seq.contains "--debug"

        { defaults with 
            Verbosity = Some(Quiet)
            Targets = ["Build"]
            Properties = [
                "Optimize",      if debug then "False" else "True"
                "DebugSymbols",  if debug then "True"  else "False"
                "Configuration", if debug then "Debug" else "Release"
            ] }

    Target.create "NativeAPI" (fun _ ->
        let file = "NativeAPI/NativeAPI.csproj"

        Command.RawCommand("dotnet", Arguments.OfArgs ["restore"; file |> sprintf "%s"])
        |> CreateProcess.fromCommand
        |> Proc.run
        |> ignore

        MSBuild.build setParams file
    )

    Target.create "Sextant" (fun _ ->
        let file = "Sextant/Sextant.fsproj"

        Command.RawCommand("dotnet", Arguments.OfArgs ["restore"; file |> sprintf "%s"])
        |> CreateProcess.fromCommand
        |> Proc.run
        |> ignore

        MSBuild.build setParams file
    )

else //.NET Core

    let setParams (defaults:DotNet.BuildOptions) =
        if args |> Seq.contains "--debug" |> not then defaults
        else 
            { defaults with 
                Configuration = DotNet.BuildConfiguration.Debug }


    Target.create "NativeAPI" (fun _ ->
        "NativeAPI/NativeAPI.csproj" 
        |> DotNet.build setParams
    )

    Target.create "Sextant" (fun _ ->
        "Sextant/Sextant.fsproj"
        |> DotNet.build setParams
    )


Target.create "Build"      ignore
Target.create "All"        ignore
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
        Trace.trace "Building!"
        Target.runOrDefaultWithArguments "All"
    with
    | ex -> 
        Trace.traceError "Build failed!"

        ex |> exceptionChain 
        |> Seq.iter (fun e -> 
            Trace.traceError e.Message)

type Message =
    | StartBuild of System.DateTime
    | Exit

if args |> Seq.contains "--watch" |> not then
    build ()

else
    let now () = System.DateTime.Now

    let buildAgent = MailboxProcessor.Start (fun input ->
        async {
            Trace.trace "Build agent started."

            let mutable running = true
            let mutable lastBuild = System.DateTime.MinValue
            while running do
                let! message = input.Receive()
                match message with
                | Exit -> running <- false
                | StartBuild ts when lastBuild < ts-> 
                    lastBuild <- now()
                    build ()
                | _ -> () })

    let triggerBuild () = now () |> StartBuild |> buildAgent.Post

    Trace.trace "Starting initial build..."
    triggerBuild()

    Trace.trace "Setting up file system watcher..."
    use watcher = 
        !! "**/.*proj" ++ "**/*.fs" ++ "**/*.cs"
        |> ChangeWatcher.run (fun _ ->
            Trace.trace "File changes detected! Triggering Build..."
            triggerBuild())

    System.Console.ReadLine() |> ignore
    buildAgent.Post Exit
