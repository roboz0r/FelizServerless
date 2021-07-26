[<RequireQualifiedAccess>]
module FelizServerless.ToDoList

open System
open Elmish

let private console = Fable.Core.JS.console

type ToDoError = { Id: ToDoId; Message: string }

type State =
    {
        Items: Deferred<Result<ToDoItem list, ToDoError>>
        NewItemText: string
        NewItemId: ToDoId
        ToDoApi: IToDoItem option
        UserId: UserId option
    }

let init () =
    {
        Items = HasNotStartedYet
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
    let map = (List.map >> Result.map >> Deferred.map)

    let mapError id =
        Result.mapError (fun (x: ServerError) -> { Id = id; Message = sprintf "%A" x })

    match msg with
    | Add item ->
        match state.ToDoApi, state.Items with
        | Some api, Resolved (Ok items) ->

            { state with
                Items = InProgress(Ok(item :: items))
                NewItemText = ""
                NewItemId = Guid.NewGuid()
            },
            Cmd.OfAsync.perform api.Add item (mapError item.Id >> ResolveAdd)
        | _ -> state, Cmd.none
    | ResolveAdd result ->
        let items =
            match result with
            | Ok (oldId, newId) ->


                state.Items
                |> map
                    (fun item ->
                        if item.Id = oldId then
                            { item with Id = newId }
                        else
                            item)
            | Error err -> err |> Error |> Resolved

        { state with Items = items }, Cmd.none

    | Update item ->
        match state.ToDoApi, state.Items with
        | Some api, Resolved (Ok items) ->
            { state with
                Items =
                    items
                    |> List.map

                        (fun x -> if x.Id = item.Id then item else x)
                    |> Ok
                    |> InProgress
            },
            Cmd.OfAsync.perform
                api.Update
                item
                (mapError item.Id
                 >> Result.map (fun _ -> item)
                 >> Resolve)

        | _ -> state, Cmd.none
    | Resolve result ->
        match result, state.Items with
        | Ok item, InProgress (Ok items) ->
            { state with
                Items = Resolved(Ok items)
            },
            Cmd.none
        | Error err, _ ->
            { state with
                Items = Resolved(Error err)
            },
            Cmd.none
        | Ok item, _ ->
            { state with
                Items =
                    Resolved(
                        Error
                            {
                                Id = item.Id
                                Message = "Invalid state detected"
                            }
                    )
            },
            Cmd.none
    | Remove id ->
        match state.ToDoApi, state.Items with
        | Some api, Resolved (Ok items) ->
            { state with
                Items =
                    items
                    |> List.filter (fun x -> not (x.Id = id))
                    |> Ok
                    |> InProgress
            },
            Cmd.OfAsync.perform
                api.Delete
                id
                (Result.mapError (fun x -> { Id = id; Message = (sprintf "%A" x) })
                 >> ResolveRemove)

        | _ -> state, Cmd.none
    | NewItemChanged s -> { state with NewItemText = s }, Cmd.none
    | SetApi (api, userId) ->
        { state with
            ToDoApi = Some api
            UserId = Some userId
            Items = FirstLoad
            NewItemText = ""
            NewItemId = Guid.NewGuid()
        },
        Cmd.none

    //TODO Make it only call list if has focus
    // Cmd.OfAsync.perform
    //     api.List
    //     ()
    //     (function
    //     | Ok x -> SetItems x
    //     | Error err ->
    //         Fable.Core.JS.console.log (sprintf "Error getting ToDoItem List: %A" err)
    //         SetItems [])
    | ClearApi -> init (), Cmd.none
    | SetItems items ->
        let items' = items |> Ok |> Resolved
        { state with Items = items' }, Cmd.none
    | ResolveRemove result ->
        match result, state.Items with
        | Ok id, InProgress (Ok items) ->
            { state with
                Items = Resolved(Ok items)
            },
            Cmd.none
        | Error err, _ ->
            { state with
                Items = Resolved(Error err)
            },
            Cmd.none
        | Ok id, _ ->
            { state with
                Items =
                    Resolved(
                        Error
                            {
                                Id = id
                                Message = "Invalid state detected"
                            }
                    )
            },
            Cmd.none
    | RefreshList ->
        { state with Items = FirstLoad },
        match state.ToDoApi with
        | Some api ->
            Cmd.OfAsync.perform
                api.List
                ()
                (function
                | Ok x -> SetItems x
                | Error err ->
                    console.log (sprintf "Error getting ToDoItem List: %A" err)
                    SetItems [])
        | None -> Cmd.none
