module FelizServerless.ToDoList

open System
open Feliz
open Feliz.UseElmish
open Elmish
open Fable.MaterialUI.Icons
open Feliz.MaterialUI

type ToDoId = Guid

type ToDoItem =
    {
        Id: ToDoId
        Description: string
        Completed: bool
    }

type State =
    {
        Items: ToDoItem list
        NewItemText: string
        NewItemId: ToDoId
    }

let init () =
    {
        Items = []
        NewItemText = ""
        NewItemId = Guid.NewGuid()
    }

type Msg =
    | Add of ToDoItem
    | Complete of ToDoId
    | Remove of ToDoId
    | RemoveMany of ToDoId list
    | AddItemChanged of string

let update msg state =
    match msg with
    | Add item ->
        { state with
            Items = item :: state.Items
            NewItemText = ""
            NewItemId = Guid.NewGuid()
        }
    | Complete id ->
        let items =
            state.Items
            |> List.map
                (fun x ->
                    if x.Id = id then
                        { x with Completed = true }
                    else
                        x)

        { state with Items = items }
    | Remove id ->
        let items =
            state.Items |> List.filter (fun x -> (x.Id <> id))

        { state with Items = items }
    | RemoveMany ids ->
        let items =
            state.Items
            |> List.filter (fun x -> not (List.contains x.Id ids))

        { state with Items = items }
    | AddItemChanged s -> { state with NewItemText = s }

// TODO Stop using the hook api 
// https://stackoverflow.com/questions/56432167/how-to-style-components-using-makestyles-and-still-have-lifecycle-methods-in-mat
// https://cmeeren.github.io/Feliz.MaterialUI/#usage/themes
let useStyles: unit -> _ =
    Styles.makeStyles
        (fun styles theme ->
            {|
                hide = styles.create [ style.display.none ]
                card =
                    styles.create [
                        style.padding 10
                        style.minWidth 150
                        style.outlineStyle.ridge
                        style.outlineColor "primary"
                    ]
            |})

let private toDoView item dispatch =

    let styles = useStyles ()

    Mui.card [
        prop.className styles.card
        card.children [
            Mui.typography item.Description
            Mui.button [
                prop.onClick (fun _ -> item.Id |> Complete |> dispatch)
                button.children [ doneIcon [] ]
            ]
            Mui.button [
                prop.onClick (fun _ -> item.Id |> Remove |> dispatch)
                button.children [ deleteIcon [] ]
            ]
        ]
    ]

[<ReactComponent>]
let View state dispatch =
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
                (fun e ->
                    if String.IsNullOrWhiteSpace state.NewItemText then
                        ()
                    else
                        {
                            Id = state.NewItemId
                            Description = state.NewItemText
                            Completed = false
                        }
                        |> Add
                        |> dispatch)
            button.disabled (String.IsNullOrWhiteSpace state.NewItemText)
            button.children (addIcon [])
        ]
    ]
