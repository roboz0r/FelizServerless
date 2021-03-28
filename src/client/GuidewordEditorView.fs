[<RequireQualifiedAccess>]
module FelizServerless.GuidewordEditorView

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons

type private Msg = GuidewordEditor.Msg

[<ReactComponent>]
let View (state: GuidewordEditor.State) dispatch =
    Mui.grid [
        grid.container true
        grid.children [
            Mui.gridItem [
                Mui.autocomplete [
                    autocomplete.options (state.GuidewordIndex |> Array.ofSeq)
                    autocomplete.getOptionLabel id
                    autocomplete.renderInput
                        (fun autoParams ->
                            Mui.textField [
                                prop.key $"G{state.Id.Value}"
                                textField.inputProps (List.ofArray autoParams.InputProps.felizProps)
                                textField.label "Guideword"
                                textField.defaultValue state.Guideword
                                textField.onChange (Msg.GuidewordChanged >> dispatch)
                            ])
                ]
            ]
            Mui.gridItem [
                Mui.autocomplete [
                    autocomplete.options (state.DeviationIndex |> Array.ofSeq)
                    autocomplete.getOptionLabel id
                    autocomplete.renderInput
                        (fun autoParams ->
                            Mui.textField [
                                prop.key $"D{state.Id.Value}"
                                textField.inputProps (List.ofArray autoParams.InputProps.felizProps)
                                textField.label "Deviation"
                                textField.defaultValue state.Guideword
                                textField.onChange (Msg.DeviationChanged >> dispatch)
                            ])
                ]
            ]
            Mui.gridItem [
                grid.xs._1
                grid.children [
                    Mui.list [
                        list.dense true
                        list.children [
                            Mui.iconButton [
                                iconButton.size.small
                                iconButton.children [
                                    keyboardArrowUpIcon []
                                ]
                                prop.onClick
                                    (fun _ ->
                                        (state.Order - 1.5)
                                        |> Msg.OrderChanged
                                        |> dispatch)
                            ]
                            Mui.iconButton [
                                iconButton.size.small
                                iconButton.children [
                                    keyboardArrowDownIcon []
                                ]
                                prop.onClick
                                    (fun _ ->
                                        (state.Order + 1.5)
                                        |> Msg.OrderChanged
                                        |> dispatch)
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
