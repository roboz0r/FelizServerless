[<RequireQualifiedAccess>]
module FelizServerless.HazopProject

open FelizServerless.Hazop

type ProjectViewState =
    {
        Id: ProjectId
        Title: string
        Description: string
        Company: HazopCompany.State
        Documents: Document list
        Systems: System list
        GuidewordSets: GuidewordSet list
    }
    member this.ToProject() : Project =
        {
            Id = this.Id
            Title = this.Title
            Description = this.Description
            Company = this.Company.ToCompany()
            Documents = this.Documents
            Systems = this.Systems
            GuidewordSets = this.GuidewordSets
        }

    static member OfProject(project: Project) : ProjectViewState =
        {
            Id = project.Id
            Title = project.Title
            Description = project.Description
            Company = HazopCompany.State.OfCompany project.Company
            Documents = project.Documents
            Systems = project.Systems
            GuidewordSets = project.GuidewordSets
        }

type State = Editor<ProjectViewState, string>


let init project : State =
    project
    |> ProjectViewState.OfProject
    |> Editor.create

type Msg =
    | TitleChanged of string
    | DescriptionChanged of string
    | CompanyChanged of HazopCompany.State
    | DocumentsChanged of Document list
    | SystemsChanged of System list
    | GuidewordSetsChanged of GuidewordSet list
    | Save //Intended as external message at the next level to bubble a request to save
    | CompanyView of HazopCompany.Msg
    | Revert

let update msg state : State =
    match msg with
    | TitleChanged title ->
        state
        |> Editor.map (fun project -> { project with Title = title })
    | DescriptionChanged desc ->
        state
        |> Editor.map (fun project -> { project with Description = desc })
    | CompanyChanged company ->
        state
        |> Editor.map (fun project -> { project with Company = company })
    | DocumentsChanged docs ->
        state
        |> Editor.map (fun project -> { project with Documents = docs })
    | SystemsChanged systems ->
        state
        |> Editor.map (fun project -> { project with Systems = systems })
    | GuidewordSetsChanged gwSets ->
        state
        |> Editor.map (fun project -> { project with GuidewordSets = gwSets })
    | CompanyView msg ->

        let updateCompany project =
            let newCompany = HazopCompany.update msg project.Company
            { project with Company = newCompany }

        state |> Editor.map updateCompany

    | Save _ -> state |> Editor.makePending
    | Revert -> state |> Editor.clean
