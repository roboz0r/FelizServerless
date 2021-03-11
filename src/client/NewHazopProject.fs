[<RequireQualifiedAccess>]
module FelizServerless.NewHazopProject

open System
open Feliz
open Elmish
open Fable.MaterialUI.Icons
open Feliz.MaterialUI
open Fable.Remoting.Client
open FelizServerless.Hazop

type State =
    {
        Open: bool
        Id: ProjectId
        Title: string
        Description: string
    }

let init () =
    {
        Open = false
        Id = ProjectId(Guid.NewGuid())
        Title = ""
        Description = ""
    }

type Msg =
    | OpenDialog
    | CloseDialog
    | Submit
    | Cancel
    | TitleChanged of string
    | DescChanged of string
    | Reset

let update msg state : State =
    match msg with
    | OpenDialog -> { state with Open = true }
    | CloseDialog -> { state with Open = false }
    | Submit -> init ()
    | Cancel -> init ()
    | TitleChanged s -> { state with Title = s }
    | DescChanged s -> { state with Description = s }
    | Reset -> { init () with Open = true }

[<ReactComponent>]
let View state dispatch =
    Html.div [
        Mui.button [
            prop.onClick (fun _ -> dispatch OpenDialog)
            button.children [
                Mui.typography "Create New Project"
                addIcon []
            ]
        ]
        Mui.dialog [
            dialog.open' state.Open
            dialog.onClose (fun _ -> dispatch CloseDialog)
            dialog.maxWidth.lg
            dialog.children [
                Mui.dialogTitle "Create New Project"
                Mui.dialogContent [
                    Mui.textField [
                        textField.autoFocus true
                        textField.id $"Title{state.Id}"
                        textField.label "Title"
                        textField.onChange (TitleChanged >> dispatch)
                    ]
                    Html.p []
                    Mui.textField [
                        textField.multiline true
                        textField.fullWidth true
                        textField.id $"Desc{state.Id}"
                        textField.label "Project Description"
                        textField.onChange (DescChanged >> dispatch)
                    ]
                ]
                Mui.dialogActions [
                    Mui.button [
                        prop.onClick (fun _ -> dispatch Submit)
                        button.children "Submit"
                    ]
                    Mui.button [
                        prop.onClick (fun _ -> dispatch Reset)
                        button.children "Reset"
                    ]
                    Mui.button [
                        prop.onClick (fun _ -> dispatch Cancel)
                        button.children "Cancel"
                    ]
                ]
            ]
        ]
    ]
