module FelizServerless.Counter

open Feliz
open Feliz.UseElmish
open Elmish
open Feliz.MaterialUI
open Fable.Remoting.Client

type Msg =
    | Increment
    | Decrement
    | SetValue of int

type State = { Count: Deferred<int> }

let counterApi =
    Remoting.createApi ()
    |> Remoting.withRouteBuilder (fun typeName methodName -> $"/api/{typeName}/{methodName}")
    |> Remoting.buildProxy<ICounter>

let init () =
    { Count = HasNotStartedYet }, (Cmd.OfAsync.perform counterApi.Init () SetValue)

let update msg state =
    match msg with
    | Increment ->
        { state with
            Count = state.Count |> Deferred.map ((+) 1)
        }
    | Decrement ->
        { state with
            Count = state.Count |> Deferred.map (fun i -> i - 1)
        }
    | SetValue i -> { state with Count = Resolved i }

[<ReactComponent>]
let Counter state dispatch =
    match state.Count with
    | Resolved count ->
        Html.div [
            Html.h1 count
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
    | HasNotStartedYet ->
        Html.div [
            Mui.typography "Counter not started."
        ]
    | InProgress -> Html.div [ Mui.typography "Loading..." ]
