namespace FelizServerless.Server

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Microsoft.Azure.Cosmos
open FelizServerless
open System
open FelizServerless.Server.Cosmos

module ToDo =

    type private CosmosToDoItem =
        {
            Id: ToDoId
            UserId: string
            Description: string
            Completed: bool
        }

    module private CosmosToDoItem =
        let From (item: ToDoItem) =
            let (UserId userId) = item.UserId

            {
                UserId = userId
                Id = item.Id
                Description = item.Description
                Completed = item.Completed
            }

        let To item : ToDoItem =
            {
                UserId = UserId item.UserId
                Id = item.Id
                Description = item.Description
                Completed = item.Completed
            }

    let toDoContainerReq =
        {
            Database = "ToDoList"
            Container = "ToDoItems"
            PartitionKeyPath = "/userId"
        }

    let toDoImpl (client: CosmosClient) (ctx: HttpContext) : IToDoItem =
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
                            | :? CosmosException as ex -> handleCosmosException ex
                            | ex -> ex.Message |> CosmosError.Other
                        | _ ->
                            [
                                for ex in exs.InnerExceptions do
                                    ex.Message
                            ]
                            |> CosmosError.Multiple

                    return Error(DBError err)
                }
            | :? CosmosException as ex -> async { return handleCosmosException ex |> DBError |> Error }

        let container () =
            getContainerAsync client toDoContainerReq

        match ctx.GetClaims() with
        | Ok claims ->
            let userId = UserId claims.UniqueId

            let listQry (container: Container) =
                let options = CosmosLinqSerializerOptions(PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase)
                let qry =
                    query {
                        for item in container.GetItemLinqQueryable<CosmosToDoItem>(true, linqSerializerOptions = options) do
                            where (item.UserId = userId.Value)
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
                        fun (item:CosmosToDoItem) ->
                            task {
                                let newId = Guid.NewGuid()
                                let newItem = { item with Id = newId }
                                let! container = container ()
                                let! itemResp = container.CreateItemAsync(newItem)
                                return Ok(item.Id, itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess (CosmosToDoItem.From item)

                Update =
                    fun item ->
                        fun item ->
                            task {
                                let! container = container ()
                                let! itemResp = container.UpsertItemAsync(item)
                                return Ok(itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess (CosmosToDoItem.From item)

                Delete =
                    fun id ->
                        fun id ->
                            task {
                                let! container = container ()
                                let! _ = container.DeleteItemAsync(id.ToString(), PartitionKey(userId.Value))
                                return Ok(id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess id

                GetItem =
                    fun id ->
                        fun id ->
                            task {
                                let! container = container ()
                                let! itemResp = container.ReadItemAsync(id.ToString(), PartitionKey(userId.Value))
                                return Ok (CosmosToDoItem.To itemResp.Resource)
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
