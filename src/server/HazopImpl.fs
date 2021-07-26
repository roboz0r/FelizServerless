namespace FelizServerless.Server

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Microsoft.Azure.Cosmos
open System
open FelizServerless
open FelizServerless.Hazop
open FelizServerless.Server.Cosmos

module HazopProject =

    module ProjectSummary =
        let summarise (project:Project):ProjectSummary = 
            {
                Id = project.Id
                Title = project.Title
                Description = project.Description
            }


    type CosmosUserId = string

    type UserProject =
        private
            {
                Id: ProjectId
                TypeName: string
                UserId: CosmosUserId
                Project: Project
            }

        static member OfProject (UserId userId) project =
            {
                Project = project
                TypeName = nameof UserProject
                Id = project.Id
                UserId = userId
            }

    let toDoContainerReq =
        {
            Database = "HazopDb"
            Container = "Hazop"
            PartitionKeyPath = "/userId"
        }

    let impl (client: CosmosClient) (ctx: HttpContext) : IHazopProject =

        let inline mapError res =
            Result.mapError (fun err -> HazopError(sprintf "%A" err)) res

        let inline tryProcess f =
            try
                f ()
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

                    return Error(DBError err) |> mapError
                }
            | :? CosmosException as ex ->
                async {
                    return
                        handleCosmosException ex
                        |> DBError
                        |> Error
                        |> mapError
                }

        let container () =
            getContainerAsync client toDoContainerReq

        match ctx.GetClaims() with
        | Ok claims ->
            let userId = UserId claims.UniqueId

            let listQry (container: Container) =
                let options =
                    CosmosLinqSerializerOptions(PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase)

                let qry =
                    let queryable =
                        container.GetItemLinqQueryable<UserProject>(true, linqSerializerOptions = options)

                    query {
                        for item in queryable do
                            where (
                                (item.UserId) = userId.Value
                                && item.TypeName = nameof UserProject
                            )

                            select item
                    }

                qry
                |> Seq.toList
                |> List.map (fun x -> x.Project)
                |> Ok


            let summaryQry (container: Container) =

                let options =
                    CosmosLinqSerializerOptions(PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase)

                let qry =
                    let queryable =
                        container.GetItemLinqQueryable<UserProject>(true, linqSerializerOptions = options)

                    query {
                        for item in queryable do
                            where (
                                (item.UserId) = userId.Value
                                && item.TypeName = nameof UserProject
                            )

                            select item
                    }

                qry
                |> Seq.toList
                |> List.map (fun x -> x.Project |> ProjectSummary.summarise)
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
                        |> tryProcess
                ListSummary =
                    fun _ ->
                        fun _ ->
                            task {
                                let! container = container ()
                                return summaryQry container
                            }
                            |> Async.AwaitTask
                        |> tryProcess
                Add =
                    fun project ->
                        fun _ ->
                            task {
                                let newProject =
                                    { project with
                                        Id = ProjectId(Guid.NewGuid())
                                    }

                                let userProject = UserProject.OfProject userId newProject

                                let! container = container ()
                                let! itemResp = container.CreateItemAsync(userProject, PartitionKey(userId.Value))
                                return Ok(project.Id, itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess

                Update =
                    fun project ->
                        fun _ ->
                            task {
                                let userProject = UserProject.OfProject userId project
                                let! container = container ()
                                let! itemResp = container.UpsertItemAsync(userProject, PartitionKey(userId.Value))
                                return Ok(itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess

                Delete =
                    fun id ->
                        fun _ ->
                            task {
                                let! container = container ()
                                let! _ = container.DeleteItemAsync(id.Value.ToString(), PartitionKey(userId.Value))
                                return Ok(id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess

                GetItem =
                    fun id ->
                        fun _ ->
                            task {
                                let! container = container ()
                                let! itemResp = container.ReadItemAsync(id.ToString(), PartitionKey(userId.Value))
                                return Ok itemResp.Resource
                            }
                            |> Async.AwaitTask
                        |> tryProcess
            }
        | Error err ->
            let inline returnError err =
                fun _ -> async { return Error(AuthError err) |> mapError }

            {
                List = returnError err
                ListSummary = returnError err
                Add = returnError err
                Update = returnError err
                Delete = returnError err
                GetItem = returnError err
            }
