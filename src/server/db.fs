namespace FelizServerless.Server.Cosmos

open System
open Microsoft.Azure.Cosmos
open FelizServerless
open System.IO
open System.Text
open Thoth.Json.Net
open FelizServerless.Server
open FelizServerless


module FuncEngDB =

    let utf8 = new UTF8Encoding(false, true)

    let inline private encoder<'T> extraCoders = Encode.Auto.generateEncoderCached<'T> (CamelCase, extraCoders, false)
    let inline private decoder<'T> extraCoders = Decode.Auto.generateDecoderCached<'T> (CamelCase, extraCoders)

    let private serialiser = 
        let extraCoders = Json.extraCoders
        {
            new CosmosSerializer() with
                member __.FromStream<'T> (stream:Stream) = 
                    use stream = stream
                    use reader = new StreamReader(stream, utf8)
                    let json = reader.ReadToEnd()
                    Decode.unsafeFromString (decoder<'T> extraCoders) json

                member __.ToStream<'T> (input:'T) = 
                  let json = encoder extraCoders input
                  let streamPayload = new MemoryStream()
                  use streamWriter = new StreamWriter(streamPayload,
                                                      encoding = utf8,
                                                      bufferSize = 1024,
                                                      leaveOpen = true )
                  use writer = new Newtonsoft.Json.JsonTextWriter(streamWriter)
                  writer.Formatting <- Newtonsoft.Json.Formatting.None
                  json.WriteTo(writer)
                  writer.Flush()
                  streamWriter.Flush()
                  streamPayload.Position <- 0L
                  streamPayload :> Stream

        }

    let private endpointUrl = Server.Environment.DBUrl()
    let private authorisationKey = Server.Environment.PrimKey()
    let private clientOptions = CosmosClientOptions()
    clientOptions.Serializer <- serialiser

    // Static, shared client that can be shared between function calls recommended per
    // https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections#static-clients

    let client =
        new CosmosClient(endpointUrl, authorisationKey, clientOptions)
