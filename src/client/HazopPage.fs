[<RequireQualifiedAccess>]
module FelizServerless.HazopPage

open Elmish
open FelizServerless.Hazop

type HazopPageError = { Id: ProjectId; Message: string }

type ProjectSubPage = 
    | ProjectEditor of HazopProject.State

type State =
    {
        Api: IHazopProject option
        Projects: Deferred<Result<Project, HazopPageError>, Project> list
        NewProjectDialog: NewHazopProject.State
        SubPage: ProjectSubPage option
    }


let init () =
    {
        Projects = []
        Api = None
        NewProjectDialog = NewHazopProject.init ()
        SubPage = None
    }

type Msg =
    | Add of Project
    | ResolveAdd of Result<ProjectId * ProjectId, HazopPageError>
    | Update of Project
    | Resolve of Result<Project, HazopPageError>
    | Remove of ProjectId
    | ResolveRemove of Result<ProjectId, HazopPageError>
    | NewItemChanged of string
    | SetApi of IHazopProject
    | ClearApi
    | SetItems of Project list
    | RefreshList
    | NewProjectMsg of NewHazopProject.Msg
    | ProjectEditorChanged of HazopProject.Msg
    | OpenSubPage of ProjectSubPage
    | CloseSubPages

let hazopApi =
    AuthStatus.createAuthenticatedApi<IHazopProject>

let update msg state : State * Cmd<_> =
    let console = Fable.Core.JS.console

    let mapError id =
        Result.mapError (fun (HazopError err) -> { Id = id; Message = err })

    match msg with
    | Add item ->
        { state with
            Projects = (InProgress item) :: state.Projects
        },
        match state.Api with
        | Some api -> Cmd.OfAsync.perform api.Add item ((mapError item.Id) >> ResolveAdd)
        | None -> Cmd.none
    | ResolveAdd res ->
        { state with
            Projects =
                state.Projects
                |> List.map
                    (fun x ->
                        match x, res with
                        | InProgress x, Ok (oldId, newId) when x.Id = oldId -> Resolved(Ok { x with Id = newId })
                        | InProgress x, Error y when x.Id = y.Id -> Resolved(Error y)
                        | _ -> x)
        },
        Cmd.none
    | Update item ->
        { state with
            Projects =
                state.Projects
                |> List.map
                    (fun x ->
                        match x with
                        | Resolved (Ok x) when x.Id = item.Id -> InProgress item
                        | Resolved (Error x) when x.Id = item.Id -> InProgress item
                        | _ -> x)
        },
        match state.Api with
        | Some api ->
            Cmd.OfAsync.perform
                api.Update
                item
                (mapError item.Id
                 >> Result.map (fun _ -> item)
                 >> Resolve)

        | None -> Cmd.none
    | Resolve item ->
        { state with
            Projects =
                state.Projects
                |> List.map
                    (fun x ->
                        match x, item with
                        | InProgress x, Ok y when x.Id = y.Id -> Resolved item
                        | InProgress x, Error y when x.Id = y.Id -> Resolved item
                        | _ -> x)
        },
        Cmd.none
    | Remove id ->
        let mutable sendCmd = false

        { state with
            Projects =
                state.Projects
                |> List.choose
                    (fun x ->
                        match x with
                        | Resolved (Ok x) when x.Id = id ->
                            sendCmd <- true
                            Some(InProgress x)
                        | Resolved (Error x) when x.Id = id -> None
                        | _ -> Some x)
        },
        match state.Api, sendCmd with
        | Some api, true ->
            Cmd.OfAsync.perform
                api.Delete
                id
                (Result.mapError (fun x -> { Id = id; Message = (sprintf "%A" x) })
                 >> ResolveRemove)

        | _ -> Cmd.none
    | ResolveRemove resp ->
        { state with
            Projects =
                state.Projects
                |> List.choose
                    (fun x ->
                        match x, resp with
                        | InProgress x, Error y when x.Id = y.Id -> Some(Resolved(Error y))
                        | InProgress x, Ok id when x.Id = id -> None
                        | _ -> Some x)
        },
        Cmd.none
    | NewItemChanged (_) -> failwith "Not Implemented"
    | SetApi api ->
        { state with
            Api = Some api
            Projects = []
        },

        Cmd.OfAsync.perform
            api.List
            ()
            (function
            | Ok x -> 
                SetItems x
            | Error err ->
                console.log (sprintf "Error getting Project List: %A" err)
                SetItems [])
    | ClearApi -> init (), Cmd.none
    | SetItems items ->
        let items' = items |> List.map (Ok >> Resolved)
        { state with Projects = items' }, Cmd.none
    | RefreshList ->
        { state with Projects = [] },
        match state.Api with
        | Some api ->
            Cmd.OfAsync.perform
                api.List
                ()
                (function
                | Ok x -> SetItems x
                | Error err ->
                    console.log (sprintf "Error getting Project List: %A" err)
                    SetItems [])
        | None -> Cmd.none
    | NewProjectMsg msg ->
        let newState, cmd =
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

                    Cmd.ofMsg (Add project) 
                | None -> Cmd.none
            | _ -> NewHazopProject.update msg state.NewProjectDialog, Cmd.none

        { state with
            NewProjectDialog = newState
        },
        cmd
    | ProjectEditorChanged (HazopProject.Msg.Save) -> 
        let newState = 
            match state.SubPage with
            | Some (ProjectEditor editorState) -> 
                let editorState = HazopProject.update HazopProject.Msg.Save editorState
                { state with SubPage = Some (ProjectEditor editorState) }
            | _ -> state
        let cmd = 
            match newState.Api, newState.SubPage with
            | Some api, Some (ProjectEditor projectEditor) -> 
                let project = (Editor.getCurrent projectEditor).ToProject()
                let updateHandler result = 
                    projectEditor
                    |> Editor.resolve (result |> Result.mapError (fun (HazopError err) -> err))
                    |> ProjectEditor
                    |> OpenSubPage

                Cmd.OfAsync.perform api.Update (project) updateHandler
            | None, _ -> 
                console.log "Received project editor message without valid api."
                Cmd.none
            | Some _ , _ -> 
                console.log "Received project editor message while project editor isn't Resolved."
                Cmd.none 
        newState, cmd

    | ProjectEditorChanged msg -> 
        let newState = 
            match state.SubPage with
            | Some (ProjectEditor editorState) -> 
                let editorState = HazopProject.update msg editorState
                { state with SubPage = Some (ProjectEditor editorState) }
            | _ -> state
        newState, Cmd.none
    | OpenSubPage subPage -> { state with SubPage = Some subPage }, Cmd.none
    | CloseSubPages ->  { state with SubPage = None }, Cmd.none
