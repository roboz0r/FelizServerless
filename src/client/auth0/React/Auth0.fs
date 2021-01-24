[<AutoOpen>]
module Fable.Auth0.Auth0

open System
open Fable.Core
open Fable.Core.JS
open Fable.Core.JsInterop
open Feliz
open Auth0Context
open Provider

[<Import("useAuth0", "@auth0/auth0-react"); Hook>]
let useAuth0: unit -> IAuth0ContextInterface = jsNative

[<Import("Auth0Provider", "@auth0/auth0-react")>]
let Auth0Provider: (IAuth0ProviderOptions -> JSX.Element) = jsNative
