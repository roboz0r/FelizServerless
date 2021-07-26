[<RequireQualifiedAccess>]
module FelizServerless.CounterView

open Feliz
open Feliz.MaterialUI

type private Msg = Counter.Msg

[<ReactComponent>]
let Counter (state: Counter.State) dispatch =
    match state.Count with
    | Resolved count ->
        Html.div [
            Html.h1 count
            Mui.button [
                prop.text "Increment"
                prop.onClick (fun _ -> dispatch Msg.Increment)
                button.color.primary
                button.variant.outlined
            ]

            Mui.button [
                prop.text "Decrement"
                prop.onClick (fun _ -> dispatch Msg.Decrement)
                button.color.primary
                button.variant.outlined
            ]
        ]
    | HasNotStartedYet ->
        Html.div [
            Mui.typography "Counter not started."
        ]
    | FirstLoad -> Html.div [ Mui.typography "Loading..." ]
    | InProgress _ -> Html.div [ Mui.typography "Loading..." ]
