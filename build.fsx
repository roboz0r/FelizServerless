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
// open System.Diagnostics
// open System

Target.initEnvironment ()

//TODO: Incorporate https://github.com/SAFE-Stack/SAFE-template/blob/master/Content/default/build.fsx

let dev = "development"
let prod = "production"

let clientRel = "./src/Client"

let rootPath = Path.getFullName "./"
let clientPath = Path.getFullName clientRel
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

let az args = 
    let result = Shell.Exec ("az", args, rootPath) 
    if result <> 0 then failwithf "'az %s' failed." args

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
    func "start" serverPath
)


Target.create "Watch" (fun _ -> 
    [ 
        async { dotnet (sprintf "fable watch %s --run webpack serve --mode=%s" clientRel dev) rootPath }
        // async {func "start" serverPath} 
        // https://stackoverflow.com/a/63753889/14134059
        // This allows dotnet watch to be used with Azure functions
        async { dotnet "watch msbuild /t:RunFunctions" serverPath }
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
    ==> "FableProd"
    ==> "FuncStart"

Target.runOrDefault "All"
