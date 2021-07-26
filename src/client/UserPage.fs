[<RequireQualifiedAccess>]
module FelizServerless.UserPage

open Elmish

type State =
    {
        AuthStatus: Auth0.AuthStatusState
        Claims: Result<Claims2, JwtError> option
        ClaimsApi: IClaims option
    }

type Msg =
    | SetAuthStatus of Auth0.AuthStatusState
    | SetClaims of Result<Claims2, JwtError>

let userApi =
    AuthStatus.createAuthenticatedApi<IClaims>

let init authStatus =
    let claimsApi =
        authStatus |> Auth0.AuthStatusState.tryToken |> Option.map userApi

    {
        AuthStatus = authStatus
        Claims = None
        ClaimsApi = claimsApi

    },
    match claimsApi with
    | Some claimsApi ->
        Cmd.OfAsync.either 
            claimsApi.GetClaims 
            () 
            SetClaims 
            (fun err -> SetClaims(Error(OtherJwtError err.Message)))
    | None -> Cmd.none

let update msg state =
    match msg with
    | SetAuthStatus x ->    
        match state.AuthStatus, x with
        | Auth0.Authenticated (_, token), Auth0.Authenticated (_, token') when token = token' ->
             { state with AuthStatus = x }, Cmd.none
        | _ -> 
            init x
    | SetClaims x -> { state with Claims = Some x }, Cmd.none
