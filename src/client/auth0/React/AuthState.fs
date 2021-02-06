module Fable.Auth0.AuthState
// fsharplint:disable
open System
open Fable.Core
open Fable.Core.JS
open Fable.Auth0.Global

type Error =
    abstract member Error: string

/// The auth state which, when combined with the auth methods, make up the return object of the `useAuth0` hook.
[<AllowNullLiteral>]
type IAuthState =
    abstract error: Error option with get, set
    abstract isAuthenticated: bool with get, set
    abstract isLoading: bool with get, set
    abstract user: IUser option with get, set

type AuthState =
    | HasError of Error
    | Authenticated of IUser
    | Loading
    | Anonymous

let OfJsObj (authState: IAuthState) =
    let x = authState

    match x.isLoading, x.error, x.isAuthenticated, x.user with
    | true, _, _, _ -> Loading
    | _, Some e, _, _ -> HasError e
    | _, _, true, Some u -> Authenticated u
    | _ -> Anonymous
