namespace FelizServerless.Hazop

open System
open FelizServerless

type ProjectId =
    | ProjectId of Guid
    member this.Value =
        let (ProjectId id) = this
        id

type DocumentId =
    | DocumentId of Guid
    member this.Value =
        let (DocumentId id) = this
        id

type SystemId =
    | SystemId of Guid
    member this.Value =
        let (SystemId id) = this
        id

type PersonId =
    | PersonId of Guid
    member this.Value =
        let (PersonId id) = this
        id

type NodeId =
    | NodeId of Guid
    member this.Value =
        let (NodeId id) = this
        id

type GuidewordSetId =
    | GuidewordSetId of Guid
    member this.Value =
        let (GuidewordSetId id) = this
        id

type GuidewordId =
    | GuidewordId of Guid
    member this.Value =
        let (GuidewordId id) = this
        id

// TODO Make Immutable array once Block added to F# / Fable
// Ref https://github.com/fsharp/fslang-suggestions/issues/619

type Image =
    {
        Data: array<byte>
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
        Country: string
    }
    static member Empty =
        {
            Line1 = ""
            Line2 = ""
            City = ""
            State = ""
            Postcode = ""
            Country = ""
        }

type Document =
    {
        Id: DocumentId
        ProjectId: ProjectId
        DocNo: string
        Title: string
        Rev: string
    }

[<RequireQualifiedAccess>]
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
        Logo: Image option
        Phone: string
        Address: Address option
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


type ProjectSummary =
    {
        Id: ProjectId
        Title: string
        Description: string
    }
    

type HazopError = HazopError of string

type IHazopProject =
    {
        List: unit -> Async<Result<Project list, HazopError>>
        ListSummary: unit -> Async<Result<ProjectSummary list, HazopError>>
        Add: Project -> Async<Result<ProjectId * ProjectId, HazopError>>
        Update: Project -> Async<Result<ProjectId, HazopError>>
        Delete: ProjectId -> Async<Result<ProjectId, HazopError>>
        GetItem: ProjectId -> Async<Result<Project, HazopError>>
    }
