module FelizServerless.AuthProvider

open Feliz
open Browser.Dom
open Fable.Core.JsInterop
open Fable.Auth0
open Fable.Auth0.Provider
open FelizServerless.Scope

let private auth0ProviderOptions children =
    jsOptions<IAuth0ProviderOptions>
        (fun x ->
            x.children <- Some children
            x.domain <- Auth0.Domain
            x.clientId <- Auth0.ClientId
            x.redirectUri <- Some window.location.href
            x.audience <- Some Auth0.Audience
            x.scope <- Some $"{ReadCurrentUser} {UpdateCurrentUserMetadata}")

[<ReactComponent>]
let AuthProvider children =
    let x = auth0ProviderOptions children
    (Auth0Provider x) :> ReactElement
