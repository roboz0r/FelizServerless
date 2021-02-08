namespace FelizServerless.Server

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging
open Saturn.Application
open Jwt
open Saturn.AzureFunctions
open Saturn.Controller
open Saturn
open FelizServerless
open Fable.Remoting.Server
open Fable.Remoting.Giraffe

module Saturn =

    [<Literal>]
    let MimeCss = "text/css"

    let port = 7071

    let staticController (context: ExecutionContext) =
        let filePath fileName =
            Path.Combine(context.FunctionAppDirectory, "public", fileName)
            |> Path.GetFullPath

        controller {
            index (fun ctx -> Controller.file ctx (filePath "index.html"))

            show
                (fun ctx (id: string) ->
                    if id.EndsWith(".css") then
                        ctx.Response.ContentType <- MimeCss

                    Controller.file ctx (filePath id))
        }

    let customErrorHandler (ex: Exception): Giraffe.Core.HttpHandler =
        controller {
            index (fun ctx -> Controller.text ctx ex.Message)

            show
                (fun ctx id ->
                    id
                    |> sprintf "%s, %s" ex.Message
                    |> Controller.text ctx)

        }

    let customNotFoundHandler =
        controller {
            index (fun ctx -> Controller.text ctx "Not found")

            show
                (fun ctx id ->
                    id
                    |> sprintf "Not found, %s"
                    |> Controller.text ctx)
        }

    let counterImpl:ICounter = {
        Init = fun _ -> async { return 0 }
        InitValue = fun i -> async { return i }
    }

    let counterHandler : Giraffe.Core.HttpHandler = 
        Remoting.createApi()
        |> Remoting.fromValue counterImpl
        |> Remoting.buildHttpHandler


    let counterController = 
        controller {
            index (fun ctx -> Controller.json ctx 0 )
            show (fun ctx (id:string) -> 
                match Int32.TryParse id with
                | true, i -> Controller.json ctx i
                | false, _ -> Controller.json ctx -123 )

        }

    let mainRouter context = 
        router {
            forward "/ICounter" counterHandler // (staticController context)
            forward "" (staticController context)
        }

    let func log context =
        azureFunction {
            host_prefix "/api"
            // use_token_authentication
            use_router (staticController context)
            logger log
            error_handler customErrorHandler
            not_found_handler customNotFoundHandler
        }

    [<FunctionName("SaturnHelloWorld")>]
    let helloWorld
        (
            [<HttpTrigger(Extensions.Http.AuthorizationLevel.Anonymous, Route = "{route?}")>] req: HttpRequest,
            log: ILogger,
            context: ExecutionContext
        ) =
        func log context req
