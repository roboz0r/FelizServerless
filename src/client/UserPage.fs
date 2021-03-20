[<RequireQualifiedAccess>]
module FelizServerless.UserPage

open Elmish

type State =
    {
        AuthStatus: AuthStatus.State
        Claims: Result<Claims2, JwtError> option
        ClaimsApi: IClaims option
    }

let init authStatus =
    {
        AuthStatus = authStatus
        Claims = None
        ClaimsApi = None
    }

type Msg =
    | SetAuthStatus of AuthStatus.State
    | SetClaims of Result<Claims2, JwtError>
    | SetApi of IClaims
    | NoMsg

let userApi =
    AuthStatus.createAuthenticatedApi<IClaims>

let update msg state =
    match msg with
    | SetAuthStatus x ->
        { state with
            AuthStatus = x
            Claims = None
            ClaimsApi = None
        },
        Cmd.none
    | SetClaims x -> { state with Claims = Some x }, Cmd.none
    | SetApi x ->
        let cmd =
            Cmd.OfAsync.either x.GetClaims () SetClaims (fun err -> SetClaims(Error(OtherJwtError err.Message)))

        { state with ClaimsApi = Some x }, cmd
    | NoMsg -> state, Cmd.none
