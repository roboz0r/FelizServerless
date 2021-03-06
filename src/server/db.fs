namespace FelizServerless.Server

open System
open Microsoft.Azure.Cosmos
open FelizServerless
open FSharp.Control.Tasks
open System.Net

type DbContainerReq =
    {
        Database: string
        Container: string
        PartitionKeyPath: string
    }

module FuncEngDB =

    let private endpointUrl = Environment.DBUrl()
    let private authorisationKey = Environment.PrimKey()
    let private clientOptions = CosmosClientOptions()

    // Static, shared client that can be shared between function calls recommended per
    // https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections#static-clients

    let client =
        new CosmosClient(endpointUrl, authorisationKey, clientOptions)

    let getContainerAsync req =
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
