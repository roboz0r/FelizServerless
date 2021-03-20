module FelizServerless.HazopPageView

open System
open Feliz
open Fable.MaterialUI.Icons
open Feliz.MaterialUI
open FelizServerless.Hazop

type private Msg = HazopPage.Msg

[<ReactComponent>]
let ProjectView (project: Deferred<Result<Project, HazopPage.HazopPageError>, Project>) dispatch =

    let maxLength len (s: String) =
        if s.Length <= len then
            s
        else
            s.[0..len] + "..."

    match project with
    | HasNotStartedYet ->
        Mui.gridItem [
                Mui.paper [
                    paper.children [
                        Mui.typography "Loading..."
                    ]
                ]
            ]
    | InProgress project ->
        Mui.gridItem [
                Mui.paper [
                    paper.children [
                        Mui.typography [
                            typography.variant.h6
                            typography.children project.Title
                        ]
                        Mui.typography "Loading..."
                    ]
                ]
            ]
    | Resolved (Ok project) ->
        Mui.gridItem [
                Mui.paper [
                    paper.children [
                        Mui.typography [
                            typography.variant.h6
                            typography.children project.Title
                        ]
                        Mui.button [
                            button.color.primary
                            button.children (editIcon [])
                            prop.onClick
                                (fun _ ->
                                    project
                                    |> HazopProject.init
                                    |> HazopPage.ProjectEditor
                                    |> Msg.OpenSubPage
                                    |> dispatch)
                        ]
                        Mui.typography (maxLength 200 project.Description)
                    ]
                ]
            ]
    | Resolved (Error err) ->
        Mui.gridItem [
                Mui.paper [
                    paper.children [
                        Mui.typography err.Message
                        Mui.button [
                            prop.onClick (fun _ -> dispatch Msg.RefreshList)
                            button.children [
                                Mui.typography "Refresh List"
                            ]
                        ]
                    ]
                ]
            ]

[<ReactComponent>]
let View (state: HazopPage.State) dispatch =
    let headingText = "Hazop Recorder"

    match state.Api, state.SubPage with
    | Some _, None ->
        Mui.container [
            Mui.typography [
                typography.variant.h4
                typography.children headingText
            ]
            NewHazopProjectView.View state.NewProjectDialog (Msg.NewProjectMsg >> dispatch)
            Mui.grid (
                state.Projects
                |> List.map (fun project -> ProjectView project dispatch)
            )

        ]
    | Some _, Some (HazopPage.ProjectEditor projEditor) ->
        Mui.container [
            Mui.breadcrumbs [
                Mui.link [
                    prop.onClick (fun _ -> dispatch Msg.CloseSubPages)
                    link.children "Hazop Home"
                ]
                Mui.typography "Project"
            ]
            HazopProjectView.View projEditor (Msg.ProjectEditorChanged >> dispatch)
        ]
    | None, _ ->
        Mui.container [
            Mui.typography [
                typography.variant.h4
                typography.children headingText
            ]
            Mui.typography [
                typography.color.error
                typography.children "Please log in to begin"
            ]
        ]
