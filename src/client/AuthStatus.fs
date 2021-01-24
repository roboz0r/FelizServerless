module FelizServerless.AuthStatus

open Feliz
open Feliz.UseElmish
open Elmish
open Feliz.MaterialUI
open Fable.Auth0
open Fable.Core.JsInterop
open Browser.Dom

let stringOrEmpty =
    function
    | Some s -> s
    | None -> ""



type AuthStatusState = { mutable ShowMenu: bool }

/// Provides the ability to log in to the app.
/// Adapted from: https://auth0.com/docs/quickstart/spa/react
/// Docs https://auth0.github.io/auth0-react/
[<ReactComponent>]
let LogIn () =

    let auth0 = useAuth0 ()
    let anchorEl, setAnchorEl = React.useState None

    let redirectLoginOptions =
        jsOptions<Auth0Context.IRedirectLoginOptions> (fun x -> x.redirectUri <- Some window.location.href)

    let logoutOptions =
        jsOptions<Global.ILogoutOptions> (fun x -> x.returnTo <- Some window.location.href)

    match auth0.isLoading, auth0.isAuthenticated, auth0.user with
    | true, _, _ ->
        Html.div [
            Mui.button [
                prop.ariaControls "simple-menu"
                prop.text "Loading"
                button.color.inherit'
            ]
        ]
    | _, true, Some user ->

        let pic = stringOrEmpty user.picture

        Html.div [
            Mui.button [
                prop.text (stringOrEmpty user.name)
                prop.onClick (fun e -> setAnchorEl (Some(e.currentTarget :?> Browser.Types.Element)))
                button.color.inherit'
            ]

            Mui.menu [
                menu.anchorEl anchorEl
                menu.keepMounted true
                menu.open' anchorEl.IsSome
                menu.onClose (fun _ -> setAnchorEl None)

                menu.children [
                    Mui.menuItem [
                        menuItem.children "Logout"
                        prop.onClick
                            (fun _ ->
                                setAnchorEl None
                                auth0.logout logoutOptions)
                    ]
                ]

            // Html.img [ prop.src pic ]
            // Html.h2
            // Html.p (stringOrEmpty user.email)
            // prop.onClick (fun _ -> ignore ())
            ]

        // Html.img [ prop.src pic ]
        // Html.h2
        // Html.p (stringOrEmpty user.email)
        // prop.onClick (fun _ -> ignore ())
        ]
    | _, false, _ ->
        Html.div [
            Mui.button [
                prop.ariaControls "simple-menu"
                prop.text "Log In"
                // TODO Should deal with the possible error properly.
                prop.onClick
                    (fun _ ->
                        (auth0.loginWithRedirect redirectLoginOptions)
                        |> Promise.catchEnd (fun x -> printfn "%s" x.Message))
                button.color.inherit'
            ]
        ]
    | _ -> failwith "Invalid case"
