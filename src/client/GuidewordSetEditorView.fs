[<RequireQualifiedAccess>]
module FelizServerless.GuidewordSetEditorView

open Feliz
open Feliz.MaterialUI
open Fable.MaterialUI.Icons

type private Msg = GuidewordSetEditor.Msg

[<ReactComponent>]
let View (state: GuidewordSetEditor.State) dispatch =
    Mui.paper [
        Mui.textField [
            prop.key $"Name{state.Id.Value}"
            textField.label "Set Name"
            textField.defaultValue "state.Name"
            textField.onChange (Msg.NameChanged >> dispatch)
        ]
        Mui.iconButton [
            iconButton.children [ addCircleIcon [] ]
            prop.onClick (fun _ -> dispatch Msg.AddGuideword)
        ]
        yield!
            state.Guidewords
            |> List.map
                (fun guideword ->
                    GuidewordEditorView.View guideword (fun msg -> dispatch (Msg.GuidewordChanged(guideword.Id, msg))))
    ]
