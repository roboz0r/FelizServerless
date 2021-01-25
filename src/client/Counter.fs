module FelizServerless.Counter

open Feliz
open Feliz.UseElmish
open Elmish
open Feliz.MaterialUI

type Msg =
    | Increment
    | Decrement

type State = { Count: int }

let init () = { Count = 0 }, Cmd.none

let update msg state =
    match msg with
    | Increment -> { state with Count = state.Count + 1 }, Cmd.none
    | Decrement -> { state with Count = state.Count - 1 }, Cmd.none

[<ReactComponent>]
let Counter state dispatch =

    Html.div [
        Html.h1 state.Count
        Mui.button [
            prop.text "Increment"
            prop.onClick (fun _ -> dispatch Increment)
            button.color.primary
            button.variant.outlined
        ]

        Mui.button [
            prop.text "Decrement"
            prop.onClick (fun _ -> dispatch Decrement)
            button.color.primary
            button.variant.outlined
        ]
    ]
