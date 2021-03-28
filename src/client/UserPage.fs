[<RequireQualifiedAccess>]
module FelizServerless.UserPage

open Elmish

type State =
    {
        AuthStatus: AuthStatus.AuthStatusState
        Claims: Result<Claims2, JwtError> option
        ClaimsApi: IClaims option
    }

type Msg =
    | SetAuthStatus of AuthStatus.AuthStatusState
    | SetClaims of Result<Claims2, JwtError>
    // | NoMsg

let userApi =
    AuthStatus.createAuthenticatedApi<IClaims>

let init authStatus =
    let claimsApi =
        match authStatus with
        | AuthStatus.Authenticated (_, token) -> userApi token |> Some
        | AuthStatus.AuthWDetails (_, token, _) -> userApi token |> Some
        | _ -> None

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
        | AuthStatus.Authenticated (_, token), AuthStatus.Authenticated (_, token') when token = token' ->
             { state with AuthStatus = x }, Cmd.none
        | AuthStatus.Authenticated (_, token), AuthStatus.AuthWDetails (_, token', _) when token = token' ->
             { state with AuthStatus = x }, Cmd.none
        | AuthStatus.AuthWDetails (_, token, _), AuthStatus.Authenticated (_, token') when token = token' ->
             { state with AuthStatus = x }, Cmd.none
        | AuthStatus.AuthWDetails (_, token, _), AuthStatus.AuthWDetails (_, token', _) when token = token' ->
             { state with AuthStatus = x }, Cmd.none
        | _ -> 
            init x
    | SetClaims x -> { state with Claims = Some x }, Cmd.none
    // | NoMsg -> state, Cmd.none
