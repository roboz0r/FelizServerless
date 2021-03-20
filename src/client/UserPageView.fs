[<RequireQualifiedAccess>]
module FelizServerless.UserPageView

open Feliz
open Feliz.MaterialUI

[<ReactComponent>]
let View (state: UserPage.State) dispatch =
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
