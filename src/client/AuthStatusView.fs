[<RequireQualifiedAccess>]
module FelizServerless.AuthStatusView

open System
open Feliz
open Feliz.MaterialUI
open Fable.Auth0
open Fable.Core.JsInterop
open Browser.Dom
open Fetch

type private Msg = AuthStatus.Msg

/// Provides the ability to log in to the app.
/// Adapted from: https://auth0.com/docs/quickstart/spa/react
/// Docs https://auth0.github.io/auth0-react/
[<ReactComponent>]
let LogIn (state: AuthStatus.State) dispatch =

    let allScopes x =
        match x with
        | [] -> None
        | x -> Some(String.Join(" ", x))

    // Auth0 Hooks
    let auth0 = useAuth0 ()
    let authState = AuthState.OfJsObj(auth0 :> IAuthState)

    //  https://auth0.com/docs/quickstart/spa/react/02-calling-an-api
    React.useEffect (
        (fun _ ->
            // Get User Details
            if state.AuthState = authState then
                match state.AuthState with
                | Authenticated user ->
                    promise {
                        let domain = Auth0.Domain

                        try
                            let! tokenStr =
                                auth0.getAccessTokenSilently (
                                    jsOptions<Global.IGetTokenSilentlyOptions>
                                        (fun x ->
                                            x.audience <- Some $"https://{domain}/api/v2/"
                                            x.redirect_uri <- Some window.location.href
                                            x.scope <- allScopes state.Scopes)
                                )

                            let accessToken = JWToken tokenStr

                            match state.Token with
                            | None -> dispatch (Msg.SetToken(accessToken))
                            | Some token when token <> accessToken -> dispatch (Msg.SetToken(accessToken))
                            | Some _ ->

                                let userDetailsByIdUrl =
                                    $"https://{domain}/api/v2/users/{user.sub}"

                                let header : IHttpRequestHeaders =
                                    !!{|
                                          Authorization = $"Bearer {tokenStr}"
                                      |}

                                let! userDetailsResponse = fetch userDetailsByIdUrl [ Headers(header) ]
                                let! (userDetails: Auth0.IUserDetails) = !!(userDetailsResponse.json ())
                                dispatch (Msg.SetUserDetails(userDetails))
                        with ex -> console.log (sprintf "Error getting user details: %s" ex.Message)
                    }
                    |> Promise.start
                | _ -> ()
            else
                ()),
        [|
            state.AuthState :> obj
            state.Token :> _
        |]
    )

    if state.AuthState <> authState then
        dispatch (Msg.SetAuthState authState)

    let redirectLoginOptions =
        jsOptions<Auth0Context.IRedirectLoginOptions> (fun x -> x.redirectUri <- Some window.location.href)

    let logoutOptions =
        jsOptions<Global.ILogoutOptions> (fun x -> x.returnTo <- Some window.location.href)

    match state.AuthState with
    | Loading ->
        Html.div [
            Mui.button [
                prop.ariaControls "simple-menu"
                prop.text "Loading..."
                button.disabled true
            ]
        ]
    | HasError e ->
        Html.div [
            Mui.button [
                prop.onClick (fun e -> dispatch (Msg.SetAuthState Anonymous))
                button.children [
                    Mui.tooltip [
                        tooltip.title e.Error
                        tooltip.children (
                            Mui.typography [
                                typography.children "Retry"
                            ]
                        )
                    ]
                ]
            ]
        ]
    | Authenticated user ->
        let pic = String.OfOption user.picture

        Html.div [
            Mui.button [
                prop.text (String.OfOption user.name)
                prop.onClick (fun e -> dispatch (Msg.SetAnchorEl(Some(e.currentTarget :?> Browser.Types.Element))))
            ]

            Mui.menu [
                menu.anchorEl state.AnchorEl
                menu.keepMounted true
                menu.open' state.AnchorEl.IsSome
                menu.onClose (fun _ -> dispatch (Msg.SetAnchorEl None))

                menu.children [
                    Mui.menuItem [
                        Html.img [
                            prop.src pic
                            prop.width 50
                            prop.height 50
                        ]
                    ]
                    Mui.menuItem [
                        menuItem.children "Logout"
                        prop.onClick
                            (fun _ ->
                                dispatch (Msg.SetAnchorEl None)
                                auth0.logout logoutOptions)
                    ]
                ]
            ]
        ]
    | Anonymous ->
        Html.div [
            Mui.button [
                prop.ariaControls "simple-menu"
                prop.text "Log In"
                prop.onClick
                    (fun _ ->
                        auth0.loginWithRedirect redirectLoginOptions
                        |> Promise.catchEnd
                            (fun x -> console.log (sprintf "Error handling Auth0 Log In Click: %s" x.Message)))
                button.color.inherit'
            ]
        ]
