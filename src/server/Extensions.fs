[<AutoOpen>]
module FelizServerless.Server.Extensions

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Giraffe
open System.IO
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Hosting
open System
open FelizServerless

[<Literal>]
let ExecutionContextName = "ExecutionContext"

type HttpContext with

    /// <summary>
    /// Returns the Azure Functions Execution Context
    /// </summary>
    /// <returns>Returns a <see cref="Microsoft.Azure.WebJobs.ExecutionContext"/>.</returns>
    member ctx.GetExecutionContext() : ExecutionContext = ctx.GetService()

    member ctx.Logger = ctx.Items.["Logger"] :?> ILogger

    member ctx.GetWebHostEnvironment() : IWebHostEnvironment = ctx.GetService()

    member ctx.GetClaims() =
        ctx.Items.[FuncEngJwt.JwtClaims] :?> Result<Claims2, JwtError>

module Controller =
    /// <summary>
    /// Reads a file from disk and writes its contents to the body of the HTTP response.
    /// It also sets the HTTP header Content-Type and sets the Content-Length header accordingly.
    /// </summary>
    /// <param name="ctx">HttpContext</param>
    /// <param name="filePath">A relative or absolute file path to the file.</param>
    /// <param name="contentType">The MIME type for the file. See https://www.iana.org/assignments/media-types/media-types.xhtml</param>
    /// <returns>Task of Some HttpContext after writing to the body of the response.</returns>
    let appDirFile (ctx: HttpContext) (filePath: string) (contentType: string) =
        task {
            let fileStream =
                match Path.IsPathRooted filePath with
                | true ->
                    if File.Exists filePath then
                        FileInfo(filePath).OpenRead() :> Stream |> Some
                    else
                        None
                | false ->
                    let fileProvider =
                        ctx.GetWebHostEnvironment().WebRootFileProvider

                    let fileInfo = fileProvider.GetFileInfo(filePath)

                    if fileInfo.Exists then
                        fileInfo.CreateReadStream() |> Some
                    else
                        None

            match fileStream with
            | Some x ->
                use fileStream = x
                ctx.Response.ContentType <- contentType
                return! ctx.WriteStreamAsync false fileStream None None

            | None ->
                ctx.Response.StatusCode <- 404
                return! ctx.WriteTextAsync(sprintf "File not found: %s" filePath)
        }
