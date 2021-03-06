module FelizServerless.ToDoList

open System
open Feliz
open Elmish
open Fable.MaterialUI.Icons
open Feliz.MaterialUI
open Fable.Remoting.Client

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
    | Resolve of Result<ToDoItem, ToDoError>
    | ResolveRemove of Result<ToDoId, ToDoError>
    | Update of ToDoItem
    | Remove of ToDoItem
    | AddItemChanged of string
    | SetApi of IToDoItem * UserId
    | ClearApi
    | SetItems of ToDoItem list
    | RefreshList

let toDoApi =
    AuthStatus.createAuthenticatedApi<IToDoItem>

let update msg state : State * Cmd<_> =

    let resolveItem item =
        (function
        | Ok id -> Resolve(Ok item)
        | Error x ->
            {
                Id = item.Id
                Message = sprintf "%A" x
            }
            |> Error
            |> Resolve)

    match msg with
    | Add item ->
        { state with
            Items = (InProgress item) :: state.Items
            NewItemText = ""
            NewItemId = Guid.NewGuid()
        },
        match state.ToDoApi with
        | Some api -> Cmd.OfAsync.perform api.Add item (resolveItem item)
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
        | Some api -> Cmd.OfAsync.perform api.Update item (resolveItem item)

        | None -> Cmd.none

    | Remove item ->
        let mutable sendCmd = false

        { state with
            Items =
                state.Items
                |> List.map
                    (fun x ->
                        match x with
                        | Resolved (Ok x) when x.Id = item.Id ->
                            sendCmd <- true
                            InProgress x
                        | _ -> x)
                |> List.filter
                    (fun x ->
                        match x with
                        | Resolved (Error x) when x.Id = item.Id -> false
                        | _ -> true)
        },
        match state.ToDoApi, sendCmd with
        | Some api, true ->
            Cmd.OfAsync.perform
                api.Delete
                item
                (Result.mapError
                    (fun x ->
                        {
                            Id = item.Id
                            Message = (sprintf "%A" x)
                        })
                 >> ResolveRemove)

        | _ -> Cmd.none

    | AddItemChanged s -> { state with NewItemText = s }, Cmd.none
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
            |> List.filter
                (fun x ->
                    match x, resp with
                    | InProgress x, Ok id when x.Id = id -> false
                    | _ -> true)
            |> List.map
                (fun x ->
                    match x, resp with
                    | InProgress x, Error y when x.Id = y.Id -> Resolved(Error y)
                    | x, _ -> x

                    )

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

let private toDoView (item: Deferred<Result<ToDoItem, ToDoError>, ToDoItem>) dispatch =

    match item with
    | HasNotStartedYet ->
        Mui.card [
            card.children [
                Mui.typography "Not Started..."
            ]
        ]
    | InProgress item ->
        Mui.card [
            card.children [
                Mui.typography item.Description
                Mui.button [
                    button.disabled true
                    button.children [ syncIcon [] ]
                ]
            ]
        ]
    | Resolved (Ok item) ->
        Mui.card [
            card.children [
                Mui.typography item.Description
                if not item.Completed then
                    Mui.button [
                        prop.onClick
                            (fun _ ->
                                { item with Completed = true }
                                |> Update
                                |> dispatch)
                        button.children [ doneIcon [] ]
                    ]
                Mui.button [
                    prop.onClick (fun _ -> item |> Remove |> dispatch)
                    button.children [ deleteIcon [] ]
                ]
            ]
        ]
    | Resolved (Error err) ->
        Mui.card [
            card.children [
                Mui.typography err.Message
                Mui.button [
                    prop.onClick (fun _ -> RefreshList |> dispatch)
                    button.children [ deleteIcon [] ]
                ]
            ]
        ]

[<ReactComponent>]
let View state dispatch =

    match state.ToDoApi with
    | Some _ ->

        Html.div [
            Mui.typography [
                typography.variant.h4
                typography.children "To Do List"
            ]
            Mui.list (
                state.Items
                |> List.map (fun i -> Mui.listItem [ (toDoView i dispatch) ])
            )

            Mui.textField [
                prop.key (state.NewItemId.ToString()) //Changing key after submit causes the element to rerender (and hence reset to default value)
                textField.label "Add item"
                textField.variant.filled
                textField.defaultValue state.NewItemText
                textField.onChange (AddItemChanged >> dispatch)
            ]
            Mui.button [
                prop.onClick
                    (fun _ ->
                        if String.IsNullOrWhiteSpace state.NewItemText then
                            ()
                        else
                            {
                                Id = state.NewItemId
                                Description = state.NewItemText
                                Completed = false
                                UserId = state.UserId.Value
                            }
                            |> Add
                            |> dispatch)
                button.disabled (String.IsNullOrWhiteSpace state.NewItemText)
                button.children (addIcon [])
            ]
        ]
    | None ->
        Html.div [
            Mui.typography [
                typography.variant.h4
                typography.children "To Do List"
            ]
            Mui.typography [
                typography.color.error
                typography.children "Please log in to create a To Do list"
            ]
        ]
