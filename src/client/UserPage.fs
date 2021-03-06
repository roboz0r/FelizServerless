[<RequireQualifiedAccess>]
module FelizServerless.UserPage

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Feliz.MaterialUI
open Fable.Remoting.Client
open Elmish
open Fable.Core.JS

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

let userApi = AuthStatus.createAuthenticatedApi<IClaims>

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
            Cmd.OfAsync.either x.GetClaims () SetClaims (fun err -> SetClaims (Error (OtherJwtError err.Message)))

        { state with ClaimsApi = Some x }, cmd
    | NoMsg -> state, Cmd.none

[<ReactComponent>]
let View (state: State) dispatch =
    match state.AuthStatus.UserDetails, state.Claims with
    | Some userDetails, None ->
        Html.div [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            Mui.typography ($"No Token")
        ]
    | Some userDetails, Some (Ok claims) ->

        let claims' =
            [
                $"AuthorizedParty : {claims.AuthorizedParty}"
                $"Expiration : {claims.Expiration}"
                $"IssuedAt : {claims.IssuedAt}"
                $"Issuer : {claims.Issuer}"
                $"Scopes : {claims.Scopes}"
                $"Subject : {claims.Subject}"
                $"Audience ; {claims.Audience}"
            ]

        Html.div [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            yield! claims' |> List.map Mui.typography
        ]

    | Some userDetails, Some (Error err) ->

        let errMsg =
            match err with
            | InvalidTokenParts -> "Invalid Token Parts"
            | SignatureVerification (_) -> "Signature Verification"
            | TokenExpired (_) -> "Token Expired"
            | OtherJwtError s -> s

        Html.div [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            Mui.typography errMsg
        ]

    | None, _ ->
        Html.div [
            Mui.typography ("Please log in using the button at the top right.")
        ]
