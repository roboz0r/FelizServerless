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
open FuncEngJwt
open Saturn.AzureFunctions
open Saturn.Controller
open Saturn
open FelizServerless
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open HeyRed.Mime

module Saturn =

    let staticController =
        let filePath fileName = Path.Combine("public", fileName)

        controller {
            index
                (fun ctx ->
                    let fileName = "index.html"
                    Controller.appDirFile ctx (filePath fileName) (MimeTypesMap.GetMimeType fileName))

            show
                (fun ctx (fileName: string) ->
                    Controller.appDirFile ctx (filePath fileName) (MimeTypesMap.GetMimeType fileName))

        }

    let errorHandler (ex: Exception): Giraffe.Core.HttpHandler =
        controller {
            index (fun ctx -> Controller.text ctx ex.Message)

            show
                (fun ctx id ->
                    id
                    |> sprintf "%s, %s" ex.Message
                    |> Controller.text ctx)
        }

    let notFoundHandler =
        controller {
            index (fun ctx -> Controller.text ctx "Not found")

            show
                (fun ctx id ->
                    id
                    |> sprintf "Not found, %s"
                    |> Controller.text ctx)
        }

    let counterImpl: ICounter =
        {
            Init = fun _ -> async { return 0 }
            InitValue = fun i -> async { return i }
        }

    let routeNoType typeName methodName = $"/{methodName}"

    let defaultHandler impl: Giraffe.Core.HttpHandler =
        Remoting.createApi ()
        |> Remoting.fromValue impl
        |> Remoting.withRouteBuilder routeNoType
        |> Remoting.buildHttpHandler

    let contextHandler impl: Giraffe.Core.HttpHandler =
        Remoting.createApi ()
        |> Remoting.fromContext impl
        |> Remoting.withRouteBuilder routeNoType
        |> Remoting.buildHttpHandler

    let counterHandler = defaultHandler counterImpl

    let claimsImpl (ctx: HttpContext): IClaims =
        {
            GetClaims = fun _ -> async { return ctx |> tokenFromCtx |> Result.bind validateToken }
        }

    let claimsHandler = contextHandler claimsImpl

    let mainRouter =
        router {
            forward "/IClaims" claimsHandler
            forward "/ICounter" counterHandler
            forward "" staticController
        }

    [<FunctionName("SaturnRouter")>]
    let saturnRouter
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, Route = "{*any}")>] req: HttpRequest,
            log: ILogger,
            context: ExecutionContext
        ) =
        log.LogInformation("Request path: " + req.Path.Value)

        match req.Path.Value with
        | "/api" ->
            req
            |> azureFunction {
                // If request lacks trailing slash then request path to files in html aren't resolved properly
                use_router (controller { index (fun ctx -> Controller.redirect ctx "/api/") })
               }
        | _ ->

            let svcs = req.HttpContext.RequestServices

            req.HttpContext.RequestServices <-
                { new IServiceProvider with
                    member __.GetService(t: Type) =
                        if t = typeof<ExecutionContext> then box context
                        else svcs.GetService t
                }

            req
            |> azureFunction {
                host_prefix "/api"
                logger log
                use_router mainRouter
                error_handler errorHandler
                not_found_handler notFoundHandler
               }
