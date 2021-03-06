module FelizServerless.Server.Cosmos

open System
open Microsoft.Azure.Cosmos
open FSharp.Control.Tasks.V2.ContextInsensitive
open FelizServerless

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
