[<RequireQualifiedAccess>]
module FelizServerless.AuthStatusView

open Feliz
open Feliz.MaterialUI
open Fable.Auth0
open Fable.Auth0.AuthState
open Fable.Core.JsInterop
open Browser.Dom
open Fetch

type private Msg = AuthStatus.Msg

/// Provides the ability to log in to the app.
/// Adapted from: https://auth0.com/docs/quickstart/spa/react
/// Docs https://auth0.github.io/auth0-react/
[<ReactComponent>]
let LogIn (state: AuthStatus.State) dispatch =

    // Auth0 Hooks
    let auth0 = useAuth0 ()
    let authState = AuthState.OfJsObj(auth0 :> IAuthState)

    if state.AuthState <> authState then
        dispatch (Msg.SetAuthState authState)

    //  https://auth0.com/docs/quickstart/spa/react/02-calling-an-api
    React.useEffect (
        (fun _ ->
            // Get User Details
            match state.AuthState with
            | Authenticated user ->
                promise {
                    let domain = Auth0.Domain

                    try
                        let! accessToken =
                            auth0.getAccessTokenSilently (
                                jsOptions<Global.IGetTokenSilentlyOptions>
                                    (fun x ->
                                        x.audience <- Some $"https://{domain}/api/v2/"
                                        x.redirect_uri <- Some window.location.href
                                        x.scope <- AuthStatus.allScopes state.Scopes)
                            )

                        dispatch (Msg.SetToken(JWToken accessToken))


                        let userDetailsByIdUrl =
                            $"https://{domain}/api/v2/users/{user.sub}"

                        let header : IHttpRequestHeaders =
                            !!{|
                                  Authorization = $"Bearer {accessToken}"
                              |}

                        let! userDetailsResponse = fetch userDetailsByIdUrl [ Headers(header) ]
                        let! (userDetails: Auth0.IUserDetails) = !!(userDetailsResponse.json ())

                        dispatch (Msg.SetUserDetails(userDetails))
                    with ex -> console.log (sprintf "Error getting user details: %s" ex.Message)
                }
                |> Promise.start
            | _ -> ()),
        [| state.AuthState :> obj |]
    )

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
                button.color.inherit'
                button.disabled true
            ]
        ]
    | HasError e ->
        Html.div [
            Mui.button [
                prop.onClick (fun e -> dispatch (Msg.SetAuthState Anonymous))
                button.color.inherit'
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
                button.color.inherit'
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
