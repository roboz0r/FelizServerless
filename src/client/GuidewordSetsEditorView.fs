[<RequireQualifiedAccess>]
module FelizServerless.GuidewordSetsEditorView

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons

type private Msg = GuidewordSetsEditor.Msg

[<ReactComponent>]
let View (state: GuidewordSetsEditor.State) dispatch =
    Mui.grid [
        grid.container true
        grid.children [
            Mui.gridItem [
                Mui.list [
                    for gwSet in state.GuidewordSets do
                        Mui.listItem [
                            Mui.button [
                                button.children gwSet.Name
                                prop.onClick (fun _ -> gwSet |> Msg.SelectGuidewordSet |> dispatch)
                            ]
                        ]
                ]
            ]
            match state.Selected with
            | Some selected ->
                Mui.gridItem [
                    GuidewordSetEditorView.View selected (Msg.SetChanged >> dispatch)
                ]
            | None ->
                Mui.gridItem [
                    Mui.paper [
                        Mui.typography "Select a guideword set..."
                    ]
                ]
        ]
    ]
