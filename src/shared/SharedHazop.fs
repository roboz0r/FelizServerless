namespace FelizServerless.Hazop

open System
open System.Collections.Immutable
open FelizServerless

type ProjectId =
    private
    | ProjectId of Guid
    member this.Value =
        let (ProjectId id) = this
        id

    static member OfGuid id = ProjectId id

type DocumentId =
    private
    | DocumentId of Guid
    member this.Value =
        let (DocumentId id) = this
        id

    static member OfGuid id = DocumentId id

type SystemId =
    private
    | SystemId of Guid
    member this.Value =
        let (SystemId id) = this
        id

    static member OfGuid id = SystemId id

type PersonId =
    private
    | PersonId of Guid
    member this.Value =
        let (PersonId id) = this
        id

    static member OfGuid id = PersonId id

type NodeId =
    private
    | NodeId of Guid
    member this.Value =
        let (NodeId id) = this
        id

    static member OfGuid id = NodeId id

type GuidewordSetId =
    private
    | GuidewordSetId of Guid
    member this.Value =
        let (GuidewordSetId id) = this
        id

    static member OfGuid id = GuidewordSetId id

type GuidewordId =
    private
    | GuidewordId of Guid
    member this.Value =
        let (GuidewordId id) = this
        id

    static member OfGuid id = GuidewordId id

type Image =
    {
        Data: ImmutableArray<byte>
        Filename: string
        MIMEType: string
    }

type Address =
    {
        Line1: string
        Line2: string
        City: string
        State: string
        Postcode: string
    }

type Document =
    {
        Id: DocumentId
        ProjectId: ProjectId
        DocNo: string
        Title: string
        Rev: string
    }

type NodeStatus =
    | NotStarted
    | InProgress
    | Completed
    | Merged of NodeId

type Node =
    {
        Id: NodeId
        SystemId: SystemId
        Name: string
        Description: string
        Status: NodeStatus
    }

type System =
    {
        Id: SystemId
        ProjectId: ProjectId
        Name: string
        Description: string
        Nodes: Node list
    }

type Person =
    {
        Id: PersonId
        ProjectId: ProjectId
        Name: string
        Position: string
    }

type Guideword =
    {
        Id: GuidewordId
        Order: int
        Guideword: string
        Deviation: string option
    }

type GuidewordSet =
    {
        Id: GuidewordSetId
        Name: string
        Guidewords: Guideword list
    }

type Company =
    {
        Name: string
        Logo: Image
        Phone: string
        Address: Address
    }

type Project =
    {
        Id: ProjectId
        Title: string
        Description: string
        Company: Company
        Documents: Document list
        Systems: System list
        GuidewordSets: GuidewordSet list
    }

type IHazopProject =
    {
        List: unit -> Async<Result<Project list, ServerError>>
        Add: Project -> Async<Result<ProjectId, ServerError>>
        Update: Project -> Async<Result<ProjectId, ServerError>>
        Delete: Project -> Async<Result<ProjectId, ServerError>>
        GetItem: ProjectId -> Async<Result<Project, ServerError>>
    }