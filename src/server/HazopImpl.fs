namespace FelizServerless.Server

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.AspNetCore.Http
open Microsoft.Azure.Cosmos
open FelizServerless
open System.Net
open System
open FelizServerless.Hazop
open Thoth.Json.Net
open System.Collections.Immutable
open FelizServerless.Server.Cosmos

module HazopSerialiser =

    type UserId with
        static member Encode(id: UserId) = Encode.string id.Value

        static member Decode path json =
            Decode.string path json |> Result.map UserId

    type ProjectId with
        static member Encode(id: ProjectId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json
            |> Result.map ProjectId.OfGuid

    type DocumentId with
        static member Encode(id: DocumentId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json
            |> Result.map DocumentId.OfGuid

    type SystemId with
        static member Encode(id: SystemId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json
            |> Result.map SystemId.OfGuid

    type PersonId with
        static member Encode(id: PersonId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json
            |> Result.map PersonId.OfGuid

    type NodeId with
        static member Encode(id: NodeId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map NodeId.OfGuid

    type GuidewordSetId with
        static member Encode(id: GuidewordSetId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json
            |> Result.map GuidewordSetId.OfGuid

    type GuidewordId with
        static member Encode(id: GuidewordId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json
            |> Result.map GuidewordId.OfGuid

    type Image with
        static member Encode(image: Image) =
            Encode.object [ "data", image.Data |> Seq.map Encode.byte |> Encode.seq
                            "filename", Encode.string image.Filename
                            "mimeType", Encode.string image.MIMEType ]

        static member Decode =
            Decode.object
            <| fun get ->
                {
                    Data = ImmutableArray.CreateRange(get.Required.Field "data" (Decode.array Decode.byte))
                    Filename = get.Required.Field "filename" Decode.string
                    MIMEType = get.Required.Field "mimeType" Decode.string
                }

    let private extraCoders =
        Extra.empty
        |> Extra.withCustom UserId.Encode UserId.Decode
        |> Extra.withCustom ProjectId.Encode ProjectId.Decode
        |> Extra.withCustom DocumentId.Encode DocumentId.Decode
        |> Extra.withCustom SystemId.Encode SystemId.Decode
        |> Extra.withCustom PersonId.Encode PersonId.Decode
        |> Extra.withCustom NodeId.Encode NodeId.Decode
        |> Extra.withCustom GuidewordSetId.Encode GuidewordSetId.Decode
        |> Extra.withCustom GuidewordId.Encode GuidewordId.Decode
        |> Extra.withCustom Image.Encode Image.Decode

    let inline private autoEncoder<'T> =
        Encode.Auto.generateEncoderCached<'T> (CamelCase, extraCoders, true)

    let inline private autoDecoder<'T> =
        Decode.Auto.generateDecoderCached<'T> (CamelCase, extraCoders)

    type Project with
        static member Encode = autoEncoder<Project>
        static member Decode = autoDecoder<Project>

    type UserProject =
        {
            Id: ProjectId
            TypeName: string
            UserId: UserId
            Project: Project
        }
        static member Encode = autoEncoder<UserProject>
        static member Decode = autoDecoder<UserProject>

        static member OfProject userId project =
            {
                Project = project
                TypeName = nameof UserProject
                Id = project.Id
                UserId = userId
            }


module HazopProject =
    open HazopSerialiser

    let toDoContainerReq =
        {
            Database = "HazopDb"
            Container = "Hazop"
            PartitionKeyPath = "/userId"
        }

    let impl (client: CosmosClient) (ctx: HttpContext) : IHazopProject =
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
                let qry =
                    query {
                        for item in container.GetItemLinqQueryable<UserProject>(true) do
                            where (
                                item.UserId = userId
                                && item.TypeName = nameof UserProject
                            )

                            select item
                    }

                qry
                |> Seq.toList
                |> List.map (fun x -> x.Project)
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
                    fun project ->
                        fun userProject ->
                            task {
                                let! container = container ()
                                let! itemResp = container.CreateItemAsync(userProject)
                                return Ok(itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess (UserProject.OfProject userId project)

                Update =
                    fun project ->
                        fun userProject ->
                            task {
                                let! container = container ()
                                let! itemResp = container.UpsertItemAsync(userProject)
                                return Ok(itemResp.Resource.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess (UserProject.OfProject userId project)

                Delete =
                    fun project ->
                        fun userProject ->
                            task {
                                let! container = container ()

                                let! _ =
                                    container.DeleteItemAsync(
                                        userProject.Id.Value.ToString(),
                                        PartitionKey(userId.Value)
                                    )

                                return Ok(userProject.Id)
                            }
                            |> Async.AwaitTask
                        |> tryProcess (UserProject.OfProject userId project)

                GetItem =
                    fun id ->
                        fun id ->
                            task {
                                let! container = container ()
                                let! itemResp = container.ReadItemAsync(id.ToString(), PartitionKey(userId.Value))
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
