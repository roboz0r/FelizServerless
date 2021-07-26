[<RequireQualifiedAccess>]
module FelizServerless.AuthStatus

open System
open Fetch
open Fable.Remoting.Client

type State =
    {
        AuthState: Fable.Auth0.AuthState
        UserDetails: Auth0.IUserDetails option
        AnchorEl: Browser.Types.Element option
        Scopes: string list
        Token: JWToken option
    }
    member this.AuthStatus =
        match this.AuthState, this.Token, this.UserDetails with
        | Fable.Auth0.Authenticated user, None, None -> Auth0.PreAuthenticated user
        | Fable.Auth0.Authenticated user, Some token, None ->
            Auth0.Authenticated({ User = user; Details = None }, token)
        | Fable.Auth0.Authenticated user, Some token, Some details ->
            Auth0.Authenticated({ User = user; Details = Some details }, token)
        | Fable.Auth0.Authenticated _, None, Some _ -> failwith "Should never happen"
        | Fable.Auth0.HasError e, _, _ -> Auth0.HasError e
        | Fable.Auth0.Loading, _, _ -> Auth0.Loading
        | Fable.Auth0.Anonymous, _, _ -> Auth0.Anonymous

type Msg =
    | SetUserDetails of Auth0.IUserDetails
    | SetAuthState of Fable.Auth0.AuthState
    | SetAnchorEl of Browser.Types.Element option
    | SetToken of JWToken

let inline createAuthenticatedApi<'T> (JWToken token) =
    Remoting.createApi ()
    |> Remoting.withAuthorizationHeader $"Bearer {token}"
    |> Remoting.withRouteBuilder (fun typeName methodName -> $"/api/{typeName}/{methodName}")
    |> Remoting.buildProxy<'T>

let init scopes =
    {
        AuthState = Fable.Auth0.Anonymous
        UserDetails = None
        AnchorEl = None
        Scopes = scopes
        Token = None
    }

let update msg state =
    match msg with
    | SetUserDetails x -> { state with UserDetails = Some x }
    | SetAuthState auth ->
        match auth with
        | Fable.Auth0.Authenticated _ -> { state with AuthState = auth }
        | _ ->
            { state with
                AuthState = auth
                UserDetails = None
                Token = None
            }
    | SetAnchorEl x -> { state with AnchorEl = x }
    | SetToken x -> { state with Token = Some x }
