namespace FelizServerless.Server

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Microsoft.Azure.Cosmos
open FelizServerless
open System
open Thoth.Json.Net
open FelizServerless.Server.Cosmos

module ToDo =

    type UserId with
        static member Encode(id: UserId) = Encode.string id.Value

        static member Decode path json =
            Decode.string path json |> Result.map UserId

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

    let extraCoders =
        Extra.empty
        |> Extra.withCustom UserId.Encode UserId.Decode

    let inline private autoEncoder<'T> =
        Encode.Auto.generateEncoderCached<'T> (CamelCase, extraCoders, true)

    let inline private autoDecoder<'T> =
        Decode.Auto.generateDecoderCached<'T> (CamelCase, extraCoders)
    // Using camelCase for JSON fields to be consistent with Google's style guide.
    // PascalCase appears uncommon for JSON naming

    type ToDoItem with
        static member Encode = autoEncoder<ToDoItem>
        static member Decode = autoDecoder<ToDoItem>

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
                        fun item ->
                            task {
                                let! container = container ()
                                let! itemResp = container.CreateItemAsync(item)
                                return Ok(itemResp.Resource.Id)
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
                    fun item ->
                        fun item ->
                            task {
                                let! container = container ()
                                let! _ = container.DeleteItemAsync(item.Id.ToString(), PartitionKey(userId.Value))
                                return Ok(item.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess (CosmosToDoItem.From item)

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
