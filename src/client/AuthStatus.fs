[<RequireQualifiedAccess>]
module FelizServerless.AuthStatus

open System
open Fetch
open Fable.Auth0
open Fable.Remoting.Client

type AuthStatusState = 
    | HasError of Error
    | AuthenticatedNoToken of Global.IUser
    | Authenticated of Global.IUser * JWToken
    | AuthWDetails of  Global.IUser * JWToken * Auth0.IUserDetails
    | Loading
    | Anonymous

type State =
    {
        AuthState: AuthState
        UserDetails: Auth0.IUserDetails option
        AnchorEl: Browser.Types.Element option
        Scopes: string list
        Token: JWToken option
    }
    member this.AuthStatus = 
        match this.AuthState, this.Token, this.UserDetails with
        | Fable.Auth0.Authenticated user, None, None -> AuthenticatedNoToken user
        | Fable.Auth0.Authenticated user, Some token, None -> Authenticated (user, token)
        | Fable.Auth0.Authenticated user, Some token, Some details -> AuthWDetails (user, token, details)
        | Fable.Auth0.Authenticated _, None, Some _ -> failwith "Should never happen"
        | Fable.Auth0.HasError e, _, _ -> HasError e
        | Fable.Auth0.Loading, _, _ -> Loading
        | Fable.Auth0.Anonymous, _, _ -> Anonymous

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
        AuthState = Fable.Auth0.Anonymous
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
        | Fable.Auth0.Authenticated _ -> { state with AuthState = x }
        | _ ->
            { state with
                AuthState = x
                UserDetails = None
                Token = None
            }
    | SetAnchorEl x -> { state with AnchorEl = x }
    | SetToken x -> { state with Token = Some x }

type Auth0Headers(accessToken) =
    member __.Authorization = $"Bearer {accessToken}"

    [<CompiledName("Content-Type")>]
    member __.ContentType = "application/json"

    interface IHttpRequestHeaders
