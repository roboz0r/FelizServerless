module FelizServerless.Main

open Feliz
open Browser.Dom
open Fable.Core.JsInterop
open Fable.Auth0
open Fable.Auth0.Provider

importAll "./styles/global.scss"

let auth0ProviderOptions =
    jsOptions<IAuth0ProviderOptions>
        (fun x ->
            x.domain <- "funceng.au.auth0.com"
            x.clientId <- "p4dJdVxaclOlk7YRqj8tYulBifQGlb6s"
            x.redirectUri <- Some(window.location.origin)
            x.audience <- Some "https://funceng.azurewebsites.net/"
            x.scope <- Some "read:current_user update:current_user_metadata"
            x.children <- Some(App.Router()))

// TODO https://auth0.com/docs/quickstart/spa/react/02-calling-an-api

[<ReactComponent>]
let AuthProvider () =
    (Auth0Provider auth0ProviderOptions) :> ReactElement

ReactDOM.render (AuthProvider(), document.getElementById "feliz-app")
