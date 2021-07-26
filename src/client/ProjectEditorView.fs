[<RequireQualifiedAccess>]
module FelizServerless.HazopProjectView

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons

type private Msg = HazopProject.Msg
type private SubPage = HazopProject.SubPage
type private SubPageMsg = HazopProject.SubPageMsg

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
let private HeaderView (isDirty: bool) dispatch =
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
            if isDirty then
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

// [<ReactComponent>]
// let private MenuButtonsView (state: HazopProject.ProjectViewState) dispatch =
//     // TODO Disable buttons on a dirty state?
//     Mui.grid [
//         grid.container true
//         grid.children [
//             Mui.gridItem [
//                 Mui.list (
//                     [
//                         Mui.typography [
//                             typography.variant.h6
//                             typography.children "Hazop Setup"
//                         ]
//                         Mui.button [
//                             button.children "Guideword Setup"
//                             button.disabled false
//                             prop.onClick
//                                 (fun _ ->
//                                     SubPageMsg.GuidewordSetEditorMsg
//                                     |> Msg.OpenSubPage
//                                     |> dispatch)
//                         ]
//                         Mui.button [
//                             button.children "Systems Setup"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Nodes Setup"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Risk Ranking Setup"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Documents Setup"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Node Documents Setup"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                     ]
//                     |> List.map (fun x -> Mui.listItem [ x ])
//                 )
//             ]
//             Mui.gridItem [
//                 Mui.list (
//                     [
//                         Mui.typography [
//                             typography.variant.h6
//                             typography.children "Hazop Study"
//                         ]
//                         Mui.button [
//                             button.children "Hazop Study"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Recommendation Tracking"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                     ]
//                     |> List.map (fun x -> Mui.listItem [ x ])
//                 )
//             ]
//             Mui.gridItem [
//                 Mui.list (
//                     [
//                         Mui.typography [
//                             typography.variant.h6
//                             typography.children "Hazop Reports"
//                         ]
//                         Mui.button [
//                             button.children "Attendees"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Minutes of Meeting"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Recommendations"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                         Mui.button [
//                             button.children "Recomm. Worksheets"
//                             button.disabled true
//                             prop.onClick (fun _ -> ())
//                         ]
//                     ]
//                     |> List.map (fun x -> Mui.listItem [ x ])
//                 )
//             ]
//         ]
//     ]




[<ReactComponent>]
let View (state: HazopProject.State) dispatch =
    let isDirty = state.IsDirty

    match state with
    | Working state ->
        match state.Current.SubPage with
        | None -> 


            Mui.container [
                yield HeaderView isDirty dispatch
                // match state with
                // | Clean state' -> yield MenuButtonsView state' dispatch
                // | _ -> ()
                yield
                    Mui.grid [
                        Mui.gridItem [
                            ProjectView(state.Current) dispatch
                        ]
                    ]
            ]
        | Some (SubPage.GuidewordSetEditor gwSetState) -> GuidewordSetsEditorView.View gwSetState (Msg.GuidewordSetsChanged >> dispatch)

    | Pending state ->
        Mui.container [
            Mui.loadingDialog

            ProjectView(state.Current) dispatch
        ]
    | EditorError (state, err) ->
        Mui.container [
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
