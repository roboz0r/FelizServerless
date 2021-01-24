namespace FelizServerless.Server

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging

module Server =
    // Define a nullable container to deserialize into.
    [<AllowNullLiteral>]
    type NameContainer() =
        member val Name = "" with get, set

    // For convenience, it's better to have a central place for the literal.
    [<Literal>]
    let Name = "name"

    [<FunctionName("server")>]
    let run
        ([<HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)>] req: HttpRequest)
        (log: ILogger)
        =
        async {
            log.LogInformation("F# HTTP trigger function processed a request.")

            let nameOpt =
                if req.Query.ContainsKey(Name) then
                    Some(req.Query.[Name].[0])
                else
                    None

            use stream = new StreamReader(req.Body)
            let! reqBody = stream.ReadToEndAsync() |> Async.AwaitTask

            let data =
                JsonConvert.DeserializeObject<NameContainer>(reqBody)

            let name =
                match nameOpt with
                | Some n -> n
                | None ->
                    match data with
                    | null -> ""
                    | nc -> nc.Name

            let responseMessage =
                if (String.IsNullOrWhiteSpace(name)) then
                    "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response."
                else
                    "Hello, "
                    + name
                    + ". This HTTP triggered function executed successfully."

            return OkObjectResult(responseMessage) :> IActionResult
        }
        |> Async.StartAsTask


    // The RequestObjects are new since I last looked at functions.
    // Previously I used something like
    // let r = new HttpResponseMessage()
    // r.StatusCode <- HttpStatusCode.OK
    // r.Content <- new StreamContent(file)
    // r.Content.Headers.ContentType <- Headers.MediaTypeHeaderValue("text/html")

    [<Literal>]
    let MIMEJSON = "application/json"

    let serveStaticContent (log: ILogger) (context: ExecutionContext) (fileName: string) =
        let filePath =
            Path.Combine(context.FunctionAppDirectory, "public", fileName)
            |> Path.GetFullPath

        try
            let file = new FileStream(filePath, FileMode.Open)

            log.LogInformation
            <| sprintf "File found: %s" filePath

            OkObjectResult file :> ObjectResult
        with _ ->
            let msg = sprintf "File not found: %s" filePath
            log.LogError msg
            BadRequestObjectResult msg :> ObjectResult

    [<FunctionName("serveStatic")>]
    let serveStatic
        (
            [<HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "{staticFile?}")>] req: HttpRequest,
            log: ILogger,
            context: ExecutionContext
        ) =
        log.LogInformation "Serving static content"

        match req.Path with
        | s when s.Value = "/api/" -> "index.html" |> serveStaticContent log context
        | s ->
            s.Value.Replace("/api/", "")
            |> serveStaticContent log context
