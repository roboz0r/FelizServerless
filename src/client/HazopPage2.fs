[<RequireQualifiedAccess>]
module FelizServerless.HazopPage2

open System
open Elmish
open Feliz.Router
open FelizServerless.Hazop

type HazopPageError = { Id: ProjectId; Message: string }

type State =
    {
        Api: IHazopProject option
        Projects: Deferred<Result<ProjectSummary list, HazopPageError>>
        NewProjectDialog: NewHazopProject.State
        SelectedProject: ProjectSummary option
    }

let init () =
    {
        Projects = HasNotStartedYet
        Api = None
        NewProjectDialog = NewHazopProject.init ()
        SelectedProject = None
    }

type NavigateToMsg = 
    | ProjectSetup of ProjectId
    | GuidewordSetup of ProjectId

type Msg =
    | Add of ProjectSummary
    | ResolveAdd of Result<ProjectId * ProjectId, HazopPageError>
    | Remove of ProjectId
    | ResolveRemove of Result<ProjectId, HazopPageError>
    | SetApi of IHazopProject
    | ClearApi
    | SetItems of ProjectSummary list
    | SetError of HazopPageError
    | RefreshList
    | SelectProject of ProjectSummary
    | NewProjectMsg of NewHazopProject.Msg
    | Navigation of NavigateToMsg

let hazopApi =
    AuthStatus.createAuthenticatedApi<IHazopProject>

let update msg state : State * Cmd<Msg> =
    let console = Fable.Core.JS.console

    let mapError id =
        Result.mapError (fun (HazopError err) -> { Id = id; Message = err })

    let listSummaryCmd api = 
        Cmd.OfAsync.perform
            api.ListSummary
            ()
            (function
            | Ok x -> 
                SetItems x
            | Error (HazopError err) ->
                console.log (sprintf "Error getting Project List: %s" err)
                SetError { Id = ProjectId Guid.Empty; Message = err } )

    match msg with
    | Add item ->
        match state.Api, state.Projects with 
        | Some api, Resolved (Ok projects) -> 
            let item = 
                {
                    Id = item.Id
                    Title = item.Title
                    Description = item.Description
                    Company = 
                        {
                            Name = ""
                            Logo = None
                            Phone = ""
                            Address = None
                        }
                    Documents = []
                    Systems = []
                    GuidewordSets = []
                }
            let cmd = Cmd.OfAsync.perform api.Add item ((mapError item.Id) >> ResolveAdd)
            { state with
                Projects = InProgress (Ok projects)
            }, cmd
        | _ -> state, Cmd.none
    | ResolveAdd result ->
        let projects = 
                match result with 
                | Ok (oldId, newId) -> 

                    let map = 
                        (List.map >> Result.map >> Deferred.map ) 

                    state.Projects
                    |> map
                        (fun project -> 
                            if project.Id = oldId then { project with Id = newId } else project )
                | Error err -> 
                    err |> Error |> Resolved

        { state with Projects = projects }, Cmd.none

    | Remove id ->
        match state.Api, state.Projects with 
        | Some api, Resolved (Ok projects) -> 
            let cmd = Cmd.OfAsync.perform api.Delete id ((mapError id) >> ResolveRemove)
            { state with
                Projects = InProgress (Ok projects)
            }, cmd
        | _ -> state, Cmd.none

    | ResolveRemove result ->
        let projects = 
            match result with 
            | Ok id -> 

                let filter = 
                    (List.filter >> Result.map >> Deferred.map ) 

                state.Projects
                |> filter
                    (fun project -> 
                        if project.Id = id then false else true )
            | Error err -> 
                err |> Error |> Resolved

        { state with Projects = projects }, Cmd.none

    | SetApi api ->
        { (init()) with
            Api = Some api
            Projects = FirstLoad
        }, (listSummaryCmd api)

    | ClearApi -> init (), Cmd.none
    | SetItems items ->
        let items' = items |> Ok |> Resolved
        { state with Projects = items' }, Cmd.none
    | SetError err -> 
        let err = err |> Error |> Resolved
        { state with Projects = err }, Cmd.none
    | RefreshList ->
        match state.Api with
        | Some api ->
            { (init()) with
                Api = Some api
                Projects = FirstLoad
            }, (listSummaryCmd api)
        | None -> init (), Cmd.none
    | NewProjectMsg msg ->
        let dialogState, cmd =
            match msg with
            | NewHazopProject.Msg.Submit ->
                let submitState = state.NewProjectDialog
                let newState =
                    NewHazopProject.update msg state.NewProjectDialog

                newState,
                match state.Api with
                | Some api ->
                    let project =
                        {
                            Id = submitState.Id
                            Title = submitState.Title
                            Description = submitState.Description
                        }

                    Cmd.ofMsg (Add project) 
                | None -> Cmd.none
            | _ -> NewHazopProject.update msg state.NewProjectDialog, Cmd.none

        { state with
            NewProjectDialog = dialogState
        },
        cmd
    | SelectProject project -> 
        { state with SelectedProject = Some project }, Cmd.none
    | Navigation navigationMsg -> 
        let navigate (ProjectId id) (pageName:string) = Cmd.navigate("Hazop", id.ToString(), pageName)
        match navigationMsg with
        | ProjectSetup projectId -> 
            state, navigate projectId "ProjectSetup"
        | GuidewordSetup projectId -> 
            state, navigate projectId "GuidewordSetup"
        