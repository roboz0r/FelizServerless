// ts2fable 0.7.1
module Fable.Auth0.AuthState

open System
open Fable.Core
open Fable.Core.JS
open Fable.Auth0.Global

type Error = Exception

// type User =
//     abstract picture: string with get, set
//     abstract name: string with get, set
//     abstract email: string with get, set
// obj option

/// The auth state which, when combined with the auth methods, make up the return object of the `useAuth0` hook.
[<AllowNullLiteral>]
type IAuthState =
    abstract error: Error option with get, set
    abstract isAuthenticated: bool with get, set
    abstract isLoading: bool with get, set
    abstract user: IUser option with get, set

// let [<Import("initialAuthState","@auth0/auth0-react/dist/auth-state")>] initialAuthState: IAuthState = jsNative
[<Import("initialAuthState", "@auth0/auth0-react")>]
let initialAuthState: IAuthState = jsNative
