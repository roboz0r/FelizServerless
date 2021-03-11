module FelizServerless.Json 

open FelizServerless.Hazop

#if FABLE_COMPILER
open Thoth.Json
#else
open Thoth.Json.Net
#endif

    type UserId with
        static member Encode(id: UserId) = Encode.string id.Value

        static member Decode path json =
            Decode.string path json |> Result.map UserId

    type ProjectId with
        static member Encode(id: ProjectId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map ProjectId

    type DocumentId with
        static member Encode(id: DocumentId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map DocumentId

    type SystemId with
        static member Encode(id: SystemId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map SystemId

    type PersonId with
        static member Encode(id: PersonId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map PersonId

    type NodeId with
        static member Encode(id: NodeId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map NodeId

    type GuidewordSetId with
        static member Encode(id: GuidewordSetId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map GuidewordSetId

    type GuidewordId with
        static member Encode(id: GuidewordId) = Encode.guid id.Value

        static member Decode path json =
            Decode.guid path json |> Result.map GuidewordId

    let extraCoders =
        Extra.empty
        |> Extra.withCustom UserId.Encode UserId.Decode
        |> Extra.withCustom ProjectId.Encode ProjectId.Decode
        |> Extra.withCustom DocumentId.Encode DocumentId.Decode
        |> Extra.withCustom SystemId.Encode SystemId.Decode
        |> Extra.withCustom PersonId.Encode PersonId.Decode
        |> Extra.withCustom NodeId.Encode NodeId.Decode
        |> Extra.withCustom GuidewordSetId.Encode GuidewordSetId.Decode
        |> Extra.withCustom GuidewordId.Encode GuidewordId.Decode

