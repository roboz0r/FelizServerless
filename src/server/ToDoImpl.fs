namespace FelizServerless.Server

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Microsoft.Azure.Cosmos
open FelizServerless
open System.Net
open Newtonsoft.Json
open System

module ToDo =

    let private parseGuid s = System.Guid.ParseExact(s, "D")

    // Using camelCase for JSON fields to be consistent with Google's style guide.
    // PascalCase appears uncommon for JSON naming

    [<CLIMutable>]
    type private CosmosToDoItem =
        {
            [<JsonProperty(PropertyName = "id")>]
            Id: string
            [<JsonProperty(PropertyName = "userId")>]
            UserId: string
            [<JsonProperty(PropertyName = "description")>]
            Description: string
            [<JsonProperty(PropertyName = "completed")>]
            Completed: bool
        }

    module private CosmosToDoItem =
        let From (item: ToDoItem) =
            let (UserId userId) = item.UserId

            {
                UserId = userId
                Id = item.Id.ToString()
                Description = item.Description
                Completed = item.Completed
            }

        let To item : ToDoItem =
            {
                UserId = UserId item.UserId
                Id = parseGuid item.Id
                Description = item.Description
                Completed = item.Completed
            }

    let toDoContainerReq =
        {
            Database = "ToDoList"
            Container = "ToDoItems"
            PartitionKeyPath = "/userId"
        }

    let toDoImpl (ctx: HttpContext) : IToDoItem =
        let inline returnError err =
            fun _ -> async { return Error(AuthError err) }

        let inline tryProcess item f =
            try
                f item
            with
            | :? AggregateException as exs ->
                async {
                    let err =
                        match exs.InnerExceptions.Count with
                        | 1 ->
                            match exs.InnerException with
                            | :? CosmosException as ex -> FuncEngDB.handleCosmosException ex
                            | ex -> ex.Message |> CosmosError.Other
                        | _ ->
                            [
                                for ex in exs.InnerExceptions do
                                    ex.Message
                            ]
                            |> CosmosError.Multiple

                    return Error(DBError err)
                }
            | :? CosmosException as ex ->
                async {
                    return
                        FuncEngDB.handleCosmosException ex
                        |> DBError
                        |> Error
                }

        let container () =
            FuncEngDB.getContainerAsync toDoContainerReq

        match ctx.GetClaims() with
        | Ok claims ->
            let userId = claims.UniqueId

            let listQry (container: Container) =
                let qry =
                    query {
                        for item in container.GetItemLinqQueryable<CosmosToDoItem>(true) do
                            where (item.UserId = userId)
                            select item
                    }

                qry
                |> Seq.toList
                |> List.map CosmosToDoItem.To
                |> Ok

            {
                List =
                    fun _ ->
                        fun _ ->
                            task {
                                let! container = container ()
                                return listQry container
                            }
                            |> Async.AwaitTask
                        |> tryProcess ()

                Add =
                    fun item ->
                        fun item ->
                            task {
                                let item = CosmosToDoItem.From item
                                let! container = container ()
                                let! itemResp = container.CreateItemAsync(item)
                                return Ok(parseGuid itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess item

                Update =
                    fun item ->
                        fun item ->
                            task {
                                let item = CosmosToDoItem.From item
                                let! container = container ()
                                let! itemResp = container.UpsertItemAsync(item)
                                return Ok(parseGuid itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess item

                Delete =
                    fun item ->
                        fun item ->
                            task {
                                let item = CosmosToDoItem.From item
                                let! container = container ()

                                let! _ = container.DeleteItemAsync(item.Id, PartitionKey(item.UserId))

                                return Ok(parseGuid item.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess item

                GetItem =
                    fun id ->
                        fun id ->
                            task {
                                let! container = container ()
                                let! itemResp = container.ReadItemAsync(id.ToString(), PartitionKey(userId))
                                return Ok itemResp.Resource
                            }
                            |> Async.AwaitTask
                        |> tryProcess id
            }
        | Error err ->
            {
                List = returnError err
                Add = returnError err
                Update = returnError err
                Delete = returnError err
                GetItem = returnError err
            }
