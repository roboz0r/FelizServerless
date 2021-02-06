module FelizServerless.Main

open Feliz
open Browser.Dom
open Fable.Core.JsInterop
open Fable.Auth0
open Fable.Auth0.Provider
open FelizServerless.Scope

importAll "./styles/global.scss"

let auth0ProviderOptions =
    jsOptions<IAuth0ProviderOptions>
        (fun x ->
            x.domain <- Auth0.Domain
            x.clientId <- Auth0.ClientId
            x.redirectUri <- Some(window.location.origin)
            x.audience <- Some Auth0.Audience
            x.scope <- Some $"{ReadCurrentUser} {UpdateCurrentUserMetadata}"
            x.children <- Some(App.Router()))

[<ReactComponent>]
let AuthProvider () =
    (Auth0Provider auth0ProviderOptions) :> ReactElement

ReactDOM.render (AuthProvider(), document.getElementById "feliz-app")
