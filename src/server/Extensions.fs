[<AutoOpen>]
module FelizServerless.Server.Extensions

open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Microsoft.Azure.WebJobs
open Giraffe
open System.IO
open Microsoft.Extensions.Logging

[<Literal>]
let ExecutionContextName = "ExecutionContext"

type HttpContext with

    /// <summary>
    /// Returns the Azure Functions Execution Context
    /// </summary>
    /// <returns>Returns a <see cref="Microsoft.Azure.WebJobs.ExecutionContext"/>.</returns>
    member ctx.GetExecutionContext(): ExecutionContext = ctx.GetService()

    member ctx.Logger = ctx.Items.["Logger"] :?> ILogger


module Controller =

    /// <summary>
    /// Reads a file from disk and writes its contents to the body of the HTTP response.
    /// It also sets the HTTP header Content-Type and sets the Content-Length header accordingly.
    /// </summary>
    /// <param name="filePath">A relative or absolute file path to the file.</param>
    /// <param name="contentType">The MIME type for the file. See https://www.iana.org/assignments/media-types/media-types.xhtml</param>
    /// <returns>Task of Some HttpContext after writing to the body of the response.</returns>
    let appDirFile (ctx: HttpContext) (filePath: string) (contentType: string) =
        task {
            let filePath =
                match Path.IsPathRooted filePath with
                | true -> filePath
                | false -> Path.GetFullPath filePath

            ctx.Response.ContentType <- contentType

            let! file = File.ReadAllBytesAsync filePath
            return! ctx.WriteBytesAsync file
        }
