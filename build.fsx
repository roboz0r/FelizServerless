#r "paket:
nuget FSharp.Core 4.7.0
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

Target.initEnvironment ()

let dev = "development"
let prod = "production"

let clientRel = "./src/Client"

let rootPath = Path.getFullName "./"
let clientPath = Path.getFullName clientRel
let serverPath = Path.getFullName "./src/Server"

let run path args workingDir = 
    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    RawCommand (path, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let npm args workingDir =
    let path =
        match ProcessUtils.tryFindFileOnPath "npm" with
        | Some path -> path
        | None ->
            "npm was not found in path. Please install it and make sure it's available from your path. https://www.npmjs.com/get-npm" 
            |> failwith

    run path args workingDir


let func args workingDir =
    let path =
        match ProcessUtils.tryFindFileOnPath "func" with
        | Some path -> path
        | None ->
            "Azure Functions was not found in path. Please install it and make sure it's available from your path. https://github.com/Azure/azure-functions-core-tools" 
            |> failwith

    run path args workingDir

let az args =
    let path =
        match ProcessUtils.tryFindFileOnPath "az" with
        | Some path -> path
        | None ->
            "Azure CLI was not found in path. Please install it and make sure it's available from your path. https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli" 
            |> failwith

    run path args rootPath

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

let start args = 
    let result = Shell.Exec ("pwsh", "--Command start " + args, rootPath) 
    if result <> 0 then failwithf "'start %s' failed." args

// Target.create "Clean" (fun _ -> 
//     !! "src/**/bin" 
//     ++ "src/**/obj" 
//     |> Shell.cleanDirs)

// Target.create "Build" (fun _ ->
//     !! "src/**/*.*proj" 
//     -- "src/**/.fable/**"
//     |> Seq.iter (DotNet.build id))

Target.create "CleanDist" (fun _ -> 
    !! "dist/" 
    |> Shell.cleanDirs)

Target.create "CleanDevServer" (fun _ ->
    !! "src/server/bin/output"
    |> Shell.cleanDirs)

Target.create "CopyPublic" (fun _ -> 
    Shell.copyDir "./dist" "./public" (fun _ -> true)
)

Target.create "FableDev" (fun _ -> 
    dotnet (sprintf "fable %s --run webpack --mode=%s" clientRel dev) rootPath
)

Target.create "FableProd" (fun _ -> 
    dotnet (sprintf "fable %s --run webpack --mode=%s" clientRel prod) rootPath
)

Target.create "FuncStart" (fun _ -> 
    async {   
        do! Async.Sleep 10000 
        start "http://localhost:7071/api/" 
    } |> Async.Start
    func "start" serverPath
)

Target.create "Watch" (fun _ -> 
    [ 
        async { dotnet (sprintf "fable watch %s --run webpack serve --mode=%s" clientRel dev) rootPath }
        // async {func "start" serverPath} 
        // https://stackoverflow.com/a/63753889/14134059
        // This allows dotnet watch to be used with Azure functions
        async { dotnet "watch msbuild /t:RunFunctions" serverPath }
        async {   
            do! Async.Sleep 10000 
            start "http://localhost:8080" 
        }
    ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "WatchFunc" (fun _ -> 
    [ 
        // async { dotnet (sprintf "fable %s --run webpack --mode=%s" clientRel dev) rootPath }
        // async {func "start" serverPath} 
        // https://stackoverflow.com/a/63753889/14134059
        // This allows dotnet watch to be used with Azure functions
        async { dotnet "watch msbuild /t:RunFunctions" serverPath }
        async {   
            do! Async.Sleep 10000 
            start "http://localhost:7071/api/" 
        }
    ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "Publish" (fun _ -> 
    try    
        func "azure functionapp publish FuncEng" serverPath
    with _ -> 
        //If publish fails once try login then try again
        az "login"
        func "azure functionapp publish FuncEng" serverPath
)

// "Clean" 
//     ==> "Build" 

"CleanDist"
    ==> "CleanDevServer"
    ==> "CopyPublic"
    ==> "FableDev"

"CleanDist"
    ==> "FableProd"
    ==> "Publish"

"CleanDist"
    ==> "CleanDevServer"
    ==> "CopyPublic"
    ==> "Watch"

"CleanDist"
    ==> "CleanDevServer"
    ==> "CopyPublic"
    ==> "FableProd"
    ==> "WatchFunc"

"CleanDist"
    ==> "FableProd"
    ==> "FuncStart"

Target.runOrDefault "Watch"
