module FelizServerless.Counter

open Elmish
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
    { Count = HasNotStartedYet }, Cmd.none // (Cmd.OfAsync.perform counterApi.Init () SetValue)

    //TODO Only perform command if has focus

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
