[<RequireQualifiedAccess>]
module FelizServerless.ToDoList

open System
open Elmish

type ToDoError = { Id: ToDoId; Message: string }

type State =
    {
        Items: Deferred<Result<ToDoItem, ToDoError>, ToDoItem> list
        NewItemText: string
        NewItemId: ToDoId
        ToDoApi: IToDoItem option
        UserId: UserId option
    }

let init () =
    {
        Items = []
        NewItemText = ""
        NewItemId = Guid.NewGuid()
        ToDoApi = None
        UserId = None
    }

type Msg =
    | Add of ToDoItem
    | ResolveAdd of Result<ToDoId * ToDoId, ToDoError>
    | Update of ToDoItem
    | Resolve of Result<ToDoItem, ToDoError>
    | Remove of ToDoId
    | ResolveRemove of Result<ToDoId, ToDoError>
    | NewItemChanged of string
    | SetApi of IToDoItem * UserId
    | ClearApi
    | SetItems of ToDoItem list
    | RefreshList

let toDoApi =
    AuthStatus.createAuthenticatedApi<IToDoItem>

let update msg state : State * Cmd<_> =

    let mapError id =
        Result.mapError (fun (x: ServerError) -> { Id = id; Message = sprintf "%A" x })

    match msg with
    | Add item ->

        { state with
            Items = (InProgress item) :: state.Items
            NewItemText = ""
            NewItemId = Guid.NewGuid()
        },
        match state.ToDoApi with
        | Some api -> Cmd.OfAsync.perform api.Add item (mapError item.Id >> ResolveAdd)
        | None -> Cmd.none
    | ResolveAdd res ->
        { state with
            Items =
                state.Items
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
            Items =
                state.Items
                |> List.map
                    (fun x ->
                        match x with
                        | Resolved (Ok x) when x.Id = item.Id -> InProgress item
                        | Resolved (Error x) when x.Id = item.Id -> InProgress item
                        | _ -> x)
        },
        match state.ToDoApi with
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
            Items =
                state.Items
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
            Items =
                state.Items
                |> List.choose
                    (fun x ->
                        match x with
                        | Resolved (Ok x) when x.Id = id ->
                            sendCmd <- true
                            Some(InProgress x)
                        | Resolved (Error x) when x.Id = id -> None
                        | _ -> Some x)
        },
        match state.ToDoApi, sendCmd with
        | Some api, true ->
            Cmd.OfAsync.perform
                api.Delete
                id
                (Result.mapError (fun x -> { Id = id; Message = (sprintf "%A" x) })
                 >> ResolveRemove)

        | _ -> Cmd.none

    | NewItemChanged s -> { state with NewItemText = s }, Cmd.none
    | SetApi (api, userId) ->
        { state with
            ToDoApi = Some api
            UserId = Some userId
            Items = []
            NewItemText = ""
            NewItemId = Guid.NewGuid()
        },

        Cmd.OfAsync.perform
            api.List
            ()
            (function
            | Ok x -> SetItems x
            | Error err ->
                Fable.Core.JS.console.log (sprintf "Error getting ToDoItem List: %A" err)
                SetItems [])
    | ClearApi -> init (), Cmd.none
    | SetItems items ->
        let items' = items |> List.map (Ok >> Resolved)
        { state with Items = items' }, Cmd.none
    | ResolveRemove resp ->
        let items =
            state.Items
            |> List.choose
                (fun x ->
                    match x, resp with
                    | InProgress x, Error y when x.Id = y.Id -> Some(Resolved(Error y))
                    | InProgress x, Ok id when x.Id = id -> None
                    | _ -> Some x)

        { state with Items = items }, Cmd.none
    | RefreshList ->
        { state with Items = [] },
        match state.ToDoApi with
        | Some api ->
            Cmd.OfAsync.perform
                api.List
                ()
                (function
                | Ok x -> SetItems x
                | Error err ->
                    Fable.Core.JS.console.log (sprintf "Error getting ToDoItem List: %A" err)
                    SetItems [])
        | None -> Cmd.none
