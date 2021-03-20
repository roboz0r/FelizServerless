[<RequireQualifiedAccess>]
module FelizServerless.AuthStatus

open System
open Fetch
open Fable.Auth0
open Fable.Auth0.AuthState
open Fable.Remoting.Client

type State =
    {
        AuthState: AuthState
        UserDetails: Auth0.IUserDetails option
        AnchorEl: Browser.Types.Element option
        Scopes: string list
        Token: JWToken option
    }

type Msg =
    | SetUserDetails of Auth0.IUserDetails
    | SetAuthState of AuthState
    | SetAnchorEl of Browser.Types.Element option
    | SetToken of JWToken


let inline createAuthenticatedApi<'T> (JWToken token) =
    Remoting.createApi ()
    |> Remoting.withAuthorizationHeader $"Bearer {token}"
    |> Remoting.withRouteBuilder (fun typeName methodName -> $"/api/{typeName}/{methodName}")
    |> Remoting.buildProxy<'T>

let init scopes =
    {
        AuthState = Anonymous
        UserDetails = None
        AnchorEl = None
        Scopes = scopes
        Token = None
    }

let update msg state =
    match msg with
    | SetUserDetails x -> { state with UserDetails = Some x }
    | SetAuthState x ->
        match x with
        | Authenticated _ -> { state with AuthState = x }
        | _ ->
            { state with
                AuthState = x
                UserDetails = None
                Token = None
            }
    | SetAnchorEl x -> { state with AnchorEl = x }
    | SetToken x -> { state with Token = Some x }


let stringOrEmpty =
    function
    | Some s -> s
    | None -> ""

let allScopes x =
    match x with
    | [] -> None
    | x -> Some(String.Join(" ", x))

type AuthStatusState = { mutable ShowMenu: bool }

type Auth0Headers(accessToken) =
    member __.Authorization = $"Bearer {accessToken}"

    [<CompiledName("Content-Type")>]
    member __.ContentType = "application/json"

    interface IHttpRequestHeaders
