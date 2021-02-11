[<RequireQualifiedAccess>]
module FelizServerless.UserPage

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Feliz.MaterialUI
open Fable.Remoting.Client
open Elmish

type State =
    {
        AuthStatus: AuthStatus.State
        Claims: Result<Map<string, obj>, JwtError> option
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
    | SetClaims of Result<Map<string, obj>, JwtError> option
    | SetApi of IClaims

let userApi (JWToken token) =
    Remoting.createApi ()
    |> Remoting.withAuthorizationHeader $"Bearer {token}"
    |> Remoting.withRouteBuilder (fun typeName methodName -> $"/api/{typeName}/{methodName}")
    |> Remoting.buildProxy<IClaims>


let update msg state =
    match msg with
    | SetAuthStatus x ->
        { state with
            AuthStatus = x
            Claims = None
            ClaimsApi = None
        },
        Cmd.none
    | SetClaims x -> { state with Claims = x }, Cmd.none
    | SetApi x ->
        let cmd =
            Cmd.OfAsync.perform x.GetClaims () (Some >> SetClaims)

        { state with ClaimsApi = Some x }, cmd

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
                for KeyValue (k, v) in claims -> $"{k} : {string v}"
            ]

        Html.div [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            yield! claims' |> List.map (fun x -> Mui.typography x)
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
