[<RequireQualifiedAccess>]
module FelizServerless.ToDoListView

open System
open Feliz
open Fable.MaterialUI.Icons
open Feliz.MaterialUI

type private Msg = ToDoList.Msg

[<ReactComponent>]
let ToDoView (item: ToDoItem) dispatch =
    Mui.card [
        card.children [
            Mui.typography item.Description
            if not item.Completed then
                Mui.button [
                    prop.onClick
                        (fun _ ->
                            { item with Completed = true }
                            |> Msg.Update
                            |> dispatch)
                    button.children [ doneIcon [] ]
                ]
            Mui.button [
                prop.onClick (fun _ -> item.Id |> Msg.Remove |> dispatch)
                button.children [ deleteIcon [] ]
            ]
        ]
    ]

[<ReactComponent>]
let View (state: ToDoList.State) dispatch =

            
    match state.ToDoApi with
    | Some _ ->
        match state.Items with
        | Resolved (Ok items)
        | InProgress (Ok items) ->
            Html.div [
                if Deferred.inProgress state.Items then
                    Mui.loadingDialog
                Mui.typography [
                    typography.variant.h4
                    typography.children "To Do List"
                ]
                Mui.list (
                    items
                    |> List.map (fun i -> Mui.listItem [ (ToDoView i dispatch) ])
                )

                Mui.textField [
                    prop.key (state.NewItemId.ToString()) //Changing key after submit causes the element to rerender (and hence reset to default value)
                    textField.label "Add item"
                    textField.variant.filled
                    textField.defaultValue state.NewItemText
                    textField.onChange (Msg.NewItemChanged >> dispatch)
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
                                |> Msg.Add
                                |> dispatch)
                    button.disabled (String.IsNullOrWhiteSpace state.NewItemText)
                    button.children (addIcon [])
                ]
            ]
        | Resolved (Error err)
        | InProgress (Error err) ->
            Html.div [
                if Deferred.inProgress state.Items then Mui.loadingDialog
                Mui.typography [
                    typography.variant.h4
                    typography.children "To Do List"
                ]
                Mui.typography [
                    typography.color.error
                    typography.children err.Message
                ]
            ]
        | HasNotStartedYet ->
            Html.div [
                Mui.typography [
                    typography.variant.h4
                    typography.children "To Do List"
                ]
                Mui.button [
                    button.children "Load items"
                    prop.onClick (fun _ -> Msg.RefreshList |> dispatch)
                ]
            ]
        | FirstLoad ->
            Html.div [
                Mui.typography [
                    typography.variant.h4
                    typography.children "To Do List"
                ]
                Mui.button [
                    button.children "Loading..."
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
