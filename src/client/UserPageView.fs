[<RequireQualifiedAccess>]
module FelizServerless.UserPageView

open Feliz
open Feliz.MaterialUI

[<ReactComponent>]
let View (state: UserPage.State) dispatch =
    match state.AuthStatus, state.Claims with
    | Auth0.Authenticated (user, _), None ->
        Mui.container [
            Mui.typography ($"Name: {user.Details |> Option.map (fun x -> x.Name) |> String.OfOption}")
            Mui.typography ($"Email: {user.Details |> Option.map (fun x -> x.Email) |> String.OfOption}")
            Mui.typography ($"Claims not loaded")
        ]
    | Auth0.Authenticated (user, _), Some (Ok claims) ->

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
            Mui.typography ($"Name: {user.Details |> Option.map (fun x -> x.Name) |> String.OfOption}")
            Mui.typography ($"Email: {user.Details |> Option.map (fun x -> x.Email) |> String.OfOption}")
            yield! claims' |> List.map Mui.typography
        ]

    | Auth0.Authenticated (user, _), Some (Error err) ->

        let errMsg =
            match err with
            | InvalidTokenParts -> "Invalid Token Parts"
            | SignatureVerification (_) -> "Signature Verification"
            | TokenExpired (_) -> "Token Expired"
            | OtherJwtError s -> s

        Mui.container [
            Mui.typography ($"Name: {user.Details |> Option.map (fun x -> x.Name) |> String.OfOption}")
            Mui.typography ($"Email: {user.Details |> Option.map (fun x -> x.Email) |> String.OfOption}")
            Mui.typography errMsg
        ]

    | _, _ ->
        Mui.container [
            Mui.typography ("Please log in using the button at the top right.")
            Mui.typography ($"State:\n{JSON.stringify state}")
        ]
