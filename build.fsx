#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

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

Target.runOrDefaultWithArguments "All"
