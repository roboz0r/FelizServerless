[<RequireQualifiedAccess>]
module FelizServerless.UserPage

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Feliz.MaterialUI

[<ReactComponent>]
let View (state: Auth0.IUserDetails option) dispatch =
    match state with
    | Some userDetails ->
        Html.div [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
        ]
    | None ->
        Html.div [
            Mui.typography ("Please log in using the button at the top right.")
        ]
