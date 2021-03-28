[<RequireQualifiedAccess>]
module FelizServerless.UserPageView

open Feliz
open Feliz.MaterialUI

[<ReactComponent>]
let View (state: UserPage.State) dispatch =
    match state.AuthStatus, state.Claims with
    | AuthStatus.AuthWDetails (user, _, userDetails), None ->
        Mui.container [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            Mui.typography ($"No Token")
        ]
    | AuthStatus.AuthWDetails (user, _, userDetails), Some (Ok claims) ->

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

        Mui.container [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            yield! claims' |> List.map Mui.typography
        ]

    | AuthStatus.AuthWDetails (user, _, userDetails), Some (Error err) ->

        let errMsg =
            match err with
            | InvalidTokenParts -> "Invalid Token Parts"
            | SignatureVerification (_) -> "Signature Verification"
            | TokenExpired (_) -> "Token Expired"
            | OtherJwtError s -> s

        Mui.container [
            Mui.typography ($"Name: {userDetails.Name}")
            Mui.typography ($"Email: {userDetails.Email}")
            Mui.typography errMsg
        ]

    | _, _ ->
        Mui.container [
            Mui.typography ("Please log in using the button at the top right.")
            Mui.typography ($"State:\n{JSON.stringify state}")
        ]
