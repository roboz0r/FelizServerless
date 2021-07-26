module FelizServerless.HazopPageView2

open System
open Feliz
open Feliz.Router
open Fable.MaterialUI.Icons
open Feliz.MaterialUI
open FelizServerless.Hazop

type private Msg = HazopPage2.Msg
type private NavigateToMsg = HazopPage2.NavigateToMsg

[<ReactComponent>]
let private ProjectSelectorView (selectedId: ProjectId option) (project: ProjectSummary) dispatch =
    Mui.listItem [
        listItem.button true
        listItem.selected (selectedId |> function | Some id' -> id' = project.Id | None -> false)
        listItem.children project.Title
        prop.onClick (fun _ -> project |> Msg.SelectProject |> dispatch)
    ]

[<ReactComponent>]
let private ProjectListView (newProjectDialog) (selectedId: ProjectId option) (projects: ProjectSummary list) dispatch =
        Mui.list (
            (Mui.listItem [
                NewHazopProjectView.View newProjectDialog (Msg.NewProjectMsg >> dispatch)
             ])
             :: Mui.divider []
             :: (projects
                |> List.map (fun project -> ProjectSelectorView selectedId project dispatch))
        )

[<ReactComponent>]
let private MenuButtonsView (project: ProjectSummary) dispatch =

    let navigate (pageName:string) = Router.navigate("Hazop", project.Id.Value.ToString(), pageName)

    Mui.grid [
        grid.container true
        grid.children [
            Mui.gridItem [
                Mui.list (
                    [
                        Mui.typography [
                            typography.variant.h6
                            typography.children "Hazop Setup"
                        ]
                        Mui.button [
                            button.children "Project Setup"
                            button.disabled false
                            prop.onClick
                                (fun _ ->
                                    // navigate "ProjectSetup")
                                    project.Id
                                    |> NavigateToMsg.ProjectSetup
                                    |> Msg.Navigation
                                    |> dispatch)
                        ]
                        Mui.button [
                            button.children "Guideword Setup"
                            button.disabled false
                            prop.onClick
                                (fun _ ->
                                    navigate "GuidewordSetup")
                                    // |> NavigateToMsg.GuidewordSetup
                                    // |> Msg.Navigation
                                    // |> dispatch)
                        ]
                        Mui.button [
                            button.children "Systems Setup"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Nodes Setup"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Risk Ranking Setup"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Documents Setup"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Node Documents Setup"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                    ]
                    |> List.map (fun x -> Mui.listItem [ x ])
                )
            ]
            Mui.gridItem [
                Mui.list (
                    [
                        Mui.typography [
                            typography.variant.h6
                            typography.children "Hazop Study"
                        ]
                        Mui.button [
                            button.children "Hazop Study"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Recommendation Tracking"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                    ]
                    |> List.map (fun x -> Mui.listItem [ x ])
                )
            ]
            Mui.gridItem [
                Mui.list (
                    [
                        Mui.typography [
                            typography.variant.h6
                            typography.children "Hazop Reports"
                        ]
                        Mui.button [
                            button.children "Attendees"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Minutes of Meeting"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Recommendations"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                        Mui.button [
                            button.children "Recomm. Worksheets"
                            button.disabled true
                            prop.onClick (fun _ -> ())
                        ]
                    ]
                    |> List.map (fun x -> Mui.listItem [ x ])
                )
            ]
        ]
    ]

[<ReactComponent>]
let private SelectedProjectView (project: ProjectSummary option) dispatch =

    match project with
    | Some project -> 
        Mui.paper [
            paper.children [
                Mui.typography [
                    typography.variant.h6
                    typography.children project.Title
                ]
                Mui.typography (project.Description)
                MenuButtonsView (project: ProjectSummary) dispatch
            ]
        ]
    | None -> 
        Mui.paper [
            paper.children [
                Mui.typography [
                    typography.variant.h6
                    typography.children "Select a project"
                ]]]


[<ReactComponent>]
let View (state: HazopPage2.State) dispatch =
    let headingText = "Hazop Recorder"

    match state.Api with
    | Some _ ->
        match state.Projects with
        | HasNotStartedYet ->
            Html.div [
                Mui.button [
                    button.children "Load Projects"
                    prop.onClick (fun _ -> Msg.RefreshList |> dispatch)
                ]
            ]
        | FirstLoad ->
            Html.div [
                Mui.loadingDialog
                Mui.button [
                    button.children "Load Projects"
                    prop.onClick (fun _ -> Msg.RefreshList |> dispatch)
                ]
            ]
        | InProgress (Ok projects)
        | Resolved (Ok projects) ->
            Html.div [
                if Deferred.inProgress state.Projects then
                    Mui.loadingDialog
                Mui.grid [
                    grid.container true
                    grid.children [
                        Mui.gridItem [
                            grid.xs._3
                            grid.children (ProjectListView state.NewProjectDialog (state.SelectedProject |> Option.map (fun x -> x.Id)) projects dispatch)
                        ]
                        Mui.gridItem [
                            grid.container true
                            grid.children (SelectedProjectView state.SelectedProject dispatch)
                    ]]
                ]
            ]
        | InProgress (Error error)
        | Resolved (Error error) ->
            Html.div [
                if Deferred.inProgress state.Projects then
                    Mui.loadingDialog
                Mui.button [
                    button.children "Reload Projects"
                    prop.onClick (fun _ -> Msg.RefreshList |> dispatch)
                ]
                Mui.typography [
                    typography.children error.Message
                    typography.color.error
                ]
            ]

    | None ->
        Html.div [
            Mui.typography [
                typography.variant.h4
                typography.children headingText
            ]
            Mui.typography [
                typography.color.error
                typography.children "Please log in to begin"
            ]
        ]
