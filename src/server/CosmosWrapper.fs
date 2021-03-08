[<AutoOpen>]
module FelizServerless.Server.Cosmos.Extensions

open System
open Microsoft.Azure.Cosmos
open FSharp.Control.Tasks.V2.ContextInsensitive
open FelizServerless
open System.Net

type DbContainerReq =
    {
        Database: string
        Container: string
        PartitionKeyPath: string
    }

type CosmosClient with
    member this.CreateDatabaseIfNotExistsAsyncResult(id, ?throughput: int, ?requestOptions, ?cancellationToken) =
        task {
            try
                let! dbResp =
                    this.CreateDatabaseIfNotExistsAsync(
                        id,
                        ?throughput = throughput,
                        ?requestOptions = requestOptions,
                        ?cancellationToken = cancellationToken
                    )

                return Ok dbResp
            with ex -> return Error(CosmosError.Other ex.Message)

        }

type Database with
    member this.CreateContainerIfNotExistsAsyncResult
        (
            containerProperties: ContainerProperties,
            ?throughput: int,
            ?requestOptions: RequestOptions,
            ?cancellationToken: Threading.CancellationToken
        ) =
        task {
            try
                let! containerResp =
                    this.CreateContainerIfNotExistsAsync(
                        containerProperties = containerProperties,
                        ?throughput = throughput,
                        ?requestOptions = requestOptions,
                        ?cancellationToken = cancellationToken
                    )

                return Ok containerResp
            with ex -> return Error(CosmosError.Other ex.Message)

        }

    member this.CreateContainerIfNotExistsAsyncResult
        (
            containerProperties: ContainerProperties,
            throughput: ThroughputProperties,
            ?requestOptions: RequestOptions,
            ?cancellationToken: Threading.CancellationToken
        ) =
        task {
            try
                let! containerResp =
                    this.CreateContainerIfNotExistsAsync(
                        containerProperties = containerProperties,
                        throughputProperties = throughput,
                        ?requestOptions = requestOptions,
                        ?cancellationToken = cancellationToken
                    )

                return Ok containerResp
            with ex -> return Error(CosmosError.Other ex.Message)

        }

let getContainerAsync (client: CosmosClient) (req: DbContainerReq) =
    task {
        let! db = client.CreateDatabaseIfNotExistsAsync(req.Database)

        let props =
            ContainerProperties(req.Container, req.PartitionKeyPath)

        let! containerResp = db.Database.CreateContainerIfNotExistsAsync(props)
        return containerResp.Container
    }

let handleCosmosException (ex: CosmosException) =
    match ex.StatusCode with
    | HttpStatusCode.BadRequest -> CosmosError.BadRequest ex.Message
    | HttpStatusCode.Forbidden -> CosmosError.Forbidden ex.Message
    | HttpStatusCode.Conflict -> CosmosError.Conflict ex.Message
    | HttpStatusCode.RequestEntityTooLarge -> CosmosError.RequestEntityTooLarge ex.Message
    | HttpStatusCode.TooManyRequests -> CosmosError.TooManyRequests ex.Message
    | _ -> CosmosError.Other ex.Message
