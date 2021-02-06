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

//TODO: Incorporate https://github.com/SAFE-Stack/SAFE-template/blob/master/Content/default/build.fsx

let rootPath = Path.getFullName "./"
let clientPath = Path.getFullName "./src/Client"
let serverPath = Path.getFullName "./src/Server"

let npm args workingDir =
    let npmPath =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. " +
            "See https://safe-stack.github.io/docs/quickstart/#install-pre-requisites for more info"
            |> failwith

    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    Command.RawCommand (npmPath, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let func args workingDir =
    let result = Shell.Exec ("func", args, workingDir) 
    if result <> 0 then failwithf "'func %s' failed in %s" args workingDir

Target.create "Clean" (fun _ -> 
    !! "src/**/bin" 
    ++ "src/**/obj" 
    |> Shell.cleanDirs)

Target.create "Build" (fun _ ->
    !! "src/**/*.*proj" 
    -- "src/**/.fable/**"
    |> Seq.iter (DotNet.build id))

Target.create "All" ignore

Target.create "Client-old" (fun _ ->
    DotNet.exec id "fable"
        "watch ./src/client --run webpack serve --mode=development --open firefox"
    |> ignore)


Target.create "Client" (fun _ ->
    dotnet "fable watch --run webpack serve --mode=development" clientPath
)


Target.create "AzureFunc" (fun _ ->
    let psi = System.Diagnostics.ProcessStartInfo()
    psi.FileName <- "pwsh"
    psi.Arguments <- "-Command func start"
    psi.WorkingDirectory <- serverPath
    psi.UseShellExecute <- true
    System.Diagnostics.Process.Start psi
    |> ignore

    // The below starts the process in the same window
    // This prevents the subsequent process step starting
    // Shell.Exec ("func", "start", "./src/server") |> ignore
)

Target.create "Watch" (fun _ -> 
    [ 
        async { dotnet "fable watch ./src/Client --run webpack serve --mode=development" rootPath }
        // async {func "start" serverPath} 
        // https://stackoverflow.com/a/63753889/14134059
        // This allows dotnet watch to be used with Azure functions
        async { dotnet "watch msbuild /t:RunFunctions" serverPath }
    ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

"Clean" 
    ==> "Build" 
    ==> "All"

"AzureFunc"
    ==> "Client"

Target.runOrDefault "All"
