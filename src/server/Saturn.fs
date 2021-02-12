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

module Saturn =

    [<Literal>]
    let MimeCss = "text/css"

    [<Literal>]
    let MimeHtml = "text/html"

    let port = 7071

    let inline logger (ctx: HttpContext) = ctx.Items.["TraceWriter"] :?> ILogger

    let staticController (context: ExecutionContext) =
        let filePath fileName =
            Path.Combine(context.FunctionAppDirectory, "public", fileName)
            |> Path.GetFullPath

        controller {
            index (fun ctx -> Controller.file ctx (filePath "index.html"))

            show
                (fun ctx (id: string) ->
                    (logger ctx).LogTrace $"Retrieving file {id}"

                    if id.EndsWith(".css") then
                        ctx.Response.ContentType <- MimeCss

                    Controller.file ctx (filePath id))
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
            GetClaims =
                fun _ ->
                    async {
                        return
                            ctx
                            |> tokenFromCtx
                            |> Result.bind validateToken
                    }
        }

    let claimsHandler = contextHandler claimsImpl

    let mainRouter context =
        router {
            forward "/IClaims" claimsHandler
            forward "/ICounter" counterHandler
            forward "" (staticController context)
        }

    /// Catch all http function
    /// Note will only capture a url with a maximum of 5 segments. Seemingly no way to make it capture all http requests.
    /// Should consider splitting to more functions if the routing gets confusing or expensive to construct
    [<FunctionName("SaturnRouter")>]
    let saturnRouter
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, Route = "{seg1?}/{seg2?}/{seg3?}/{seg4?}/{seg5?}")>] req: HttpRequest,
            log: ILogger,
            context: ExecutionContext
        ) =
        log.LogInformation req.Path.Value

        req
        |> azureFunction {
            host_prefix "/api"
            use_router (mainRouter context)
            logger log
            error_handler errorHandler
            not_found_handler notFoundHandler
           }
