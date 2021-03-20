[<RequireQualifiedAccess>]
module FelizServerless.HazopProjectView

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons

type private Msg = HazopProject.Msg

[<ReactComponent>]
let private ProjectView (project: HazopProject.ProjectViewState) dispatch =
    Mui.container [
        Mui.textField [
            textField.label "Title"
            textField.defaultValue project.Title
            textField.onChange (Msg.TitleChanged >> dispatch)
        ]
        Html.div []
        Mui.textField [
            textField.label "Description"
            textField.defaultValue project.Description
            textField.onChange (Msg.DescriptionChanged >> dispatch)
            textField.multiline true
        ]
        HazopCompanyView.View project.Company (Msg.CompanyView >> dispatch)
    ]

[<ReactComponent>]
let View (state: HazopProject.State) dispatch =
    Mui.container [
        Mui.grid [
            grid.container true
            grid.alignContent.spaceBetween
            grid.spacing._2
            grid.children [
                Mui.gridItem [
                    Mui.typography [
                        typography.variant.h4
                        typography.children "Edit Project Details"
                    ]
                ]
                if state.IsDirty then
                    Mui.gridItem [
                        Mui.button [
                            button.variant.contained
                            button.color.primary
                            button.children "Save"
                            button.endIcon (saveIcon [])
                            prop.onClick (fun _ -> Msg.Save |> dispatch)
                        ]
                    ]

                    Mui.gridItem [
                        Mui.button [
                            button.variant.contained
                            button.color.secondary
                            button.children "Revert"
                            button.endIcon (undoIcon [])
                            prop.onClick (fun _ -> Msg.Revert |> dispatch)
                        ]
                    ]
            ]
        ]
        match state with
        | Working state -> ProjectView(state.Current) dispatch
        | Pending state ->
            Mui.dialog [
                dialog.disableBackdropClick true
                dialog.open' true
                dialog.children [
                    Mui.dialogTitle "Loading..."
                ]
            ]

            ProjectView(state.Current) dispatch

        | EditorError (state, err) ->
            Mui.dialog [
                dialog.disableBackdropClick true
                dialog.open' true
                dialog.children [
                    Mui.dialogTitle "Error"
                    Mui.dialogContentText err
                    Mui.button [
                        button.variant.contained
                        button.color.secondary
                        button.children "Revert"
                        button.endIcon (undoIcon [])
                        prop.onClick (fun _ -> Msg.Revert |> dispatch)
                    ]
                ]
            ]

            ProjectView(state.Current) dispatch
    ]
