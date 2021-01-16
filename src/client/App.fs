module FelizServerless.App

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Feliz.MaterialUI

type State = { CurrentUrl : string list
               Counter : Counter.State      }

type Msg = UrlChanged of string list

let init() = 
    let counter, cmd = Counter.init()
    { CurrentUrl = Router.currentUrl(); Counter = counter }, Cmd.batch [cmd]

let update (UrlChanged segments) state =
    { state with CurrentUrl = segments }, Cmd.none

[<ReactComponent>]
let Router () =
    let state, dispatch = React.useElmish (init, update)
    React.router [
        router.onUrlChanged (UrlChanged >> dispatch)

        router.children [
            match state.CurrentUrl with
            | [ ] -> 
                Html.div [ 
                    Html.h1 "Home"
                    Counter.Counter()
                    Mui.button [ 
                        prop.onClick (fun _ -> Router.navigate("users"))
                        prop.text "Users"
                        ]
                ]
            | [ "users" ] -> 
                Html.div [ 
                    Html.h1 "Users page"
                    Mui.button [ 
                        prop.onClick (fun _ -> Router.navigate(""))
                        prop.text "Home"
                        ]
                ]
            | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
            | _ -> Html.h1 "Not found"
        ]
    ]
