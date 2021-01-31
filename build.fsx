#r "paket:
nuget FSharp.Core 5.0.0
nuget Fake.DotNet.Cli
nuget Fake.IO.FileSystem
nuget Fake.Core.Target //"
#load ".fake/build.fsx/intellisense.fsx"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
// open System.Diagnostics
// open System

Target.initEnvironment ()

Target.create "Clean" (fun _ -> 
    !! "src/**/bin" 
    ++ "src/**/obj" 
    |> Shell.cleanDirs)

Target.create "Build" (fun _ ->
    !! "src/**/*.*proj" 
    -- "src/**/.fable/**"
    |> Seq.iter (DotNet.build id))

Target.create "All" ignore

Target.create "Client" (fun _ ->
    DotNet.exec id "fable"
        "watch ./src/client --run webpack serve --mode=development --open firefox"
    |> ignore)

Target.create "AzureFunc" (fun _ ->
    let psi = System.Diagnostics.ProcessStartInfo()
    psi.FileName <- "pwsh"
    psi.Arguments <- "-Command func start" //-WorkingDirectory ./src/server
    psi.WorkingDirectory <- System.IO.Path.Combine [|__SOURCE_DIRECTORY__; "src"; "server" |]
    psi.UseShellExecute <- true
    System.Diagnostics.Process.Start psi |> ignore

    // The below starts the process in the same window
    // This prevents the subsequent process step starting
    // Shell.Exec ("func", "start", "./src/server") |> ignore
)

Target.create "Local" ignore

"Clean" 
    ==> "Build" 
    ==> "All"

"AzureFunc"
    ==> "Client"
    ==> "Local"

Target.runOrDefault "All"
