[<RequireQualifiedAccess>]
module FelizServerless.NewHazopProjectView

open Feliz
open Fable.MaterialUI.Icons
open Feliz.MaterialUI

type private Msg = NewHazopProject.Msg

[<ReactComponent>]
let View (state: NewHazopProject.State) dispatch =
    Html.div [
        Mui.button [
            prop.onClick (fun _ -> dispatch Msg.OpenDialog)
            button.children "New Project"
            button.endIcon (addIcon [])
        ]
        Mui.dialog [
            dialog.open' state.Open
            dialog.onClose (fun _ -> dispatch Msg.CloseDialog)
            dialog.maxWidth.lg
            dialog.children [
                Mui.dialogTitle "Create New Project"
                Mui.dialogContent [
                    Mui.textField [
                        textField.autoFocus true
                        textField.id $"Title{state.Id}"
                        textField.label "Title"
                        textField.onChange (Msg.TitleChanged >> dispatch)
                    ]
                    Html.p []
                    Mui.textField [
                        textField.multiline true
                        textField.fullWidth true
                        textField.id $"Desc{state.Id}"
                        textField.label "Project Description"
                        textField.onChange (Msg.DescChanged >> dispatch)
                    ]
                ]
                Mui.dialogActions [
                    Mui.button [
                        prop.onClick (fun _ -> dispatch Msg.Submit)
                        button.children "Submit"
                    ]
                    Mui.button [
                        prop.onClick (fun _ -> dispatch Msg.Reset)
                        button.children "Reset"
                    ]
                    Mui.button [
                        prop.onClick (fun _ -> dispatch Msg.Cancel)
                        button.children "Cancel"
                    ]
                ]
            ]
        ]
    ]
