[<RequireQualifiedAccess>]
module FelizServerless.AuthStatus

open Fable.Core
open Feliz
open Feliz.UseElmish
open Elmish
open Feliz.MaterialUI
open Fable.Auth0
open Fable.Auth0.AuthState
open Fable.Core.JsInterop
open Browser.Dom
open Fetch

type State =
    {
        AuthState: AuthState
        UserDetails: Auth0.IUserDetails option
        AnchorEl: Browser.Types.Element option
    }

type Msg =
    | SetUserDetails of Auth0.IUserDetails option
    | SetAuthState of AuthState
    | SetAnchorEl of Browser.Types.Element option

let init =
    {
        AuthState = Loading
        UserDetails = None
        AnchorEl = None
    }

let update msg state =
    match msg with
    | SetUserDetails x -> { state with UserDetails = x }
    | SetAuthState x ->
        match x with
        | Authenticated _ -> { state with AuthState = x }
        | _ ->
            { state with
                AuthState = x
                UserDetails = None
            }
    | SetAnchorEl x -> { state with AnchorEl = x }


let stringOrEmpty =
    function
    | Some s -> s
    | None -> ""

type AuthStatusState = { mutable ShowMenu: bool }

type Auth0Headers(accessToken) =
    member __.Authorization = $"Bearer {accessToken}"

    [<CompiledName("Content-Type")>]
    member __.ContentType = "application/json"

    interface IHttpRequestHeaders

/// Provides the ability to log in to the app.
/// Adapted from: https://auth0.com/docs/quickstart/spa/react
/// Docs https://auth0.github.io/auth0-react/
[<ReactComponent>]
let LogIn state dispatch =

    // Auth0 Hooks
    let auth0 = useAuth0 ()
    let authState = AuthState.OfJsObj(auth0 :> IAuthState)

    if state.AuthState <> authState then
        dispatch (SetAuthState authState)

    React.useEffect (
        (fun _ ->
            // Get User Details
            promise {
                let domain = Auth0.Domain

                try
                    let user =
                        auth0.user
                        |> Option.defaultWith (fun _ -> failwith "No user on auth0 object")

                    let! accessToken =
                        auth0.getAccessTokenSilently (
                            jsOptions<Global.IGetTokenSilentlyOptions>
                                (fun x ->
                                    x.audience <- Some $"https://{Auth0.Domain}/api/v2/"
                                    x.scope <- Some Scope.ReadCurrentUser)
                        )

                    let userDetailsByIdUrl =
                        $"https://{Auth0.Domain}/api/v2/users/{user.sub}"

                    let header: IHttpRequestHeaders =
                        !!{|
                              Authorization = $"Bearer {accessToken}"
                          |}

                    let! userDetailsResponse = fetch userDetailsByIdUrl [ Headers(header) ]
                    let! userDetails = userDetailsResponse.json ()

                    dispatch (SetUserDetails(Some !!userDetails))
                with ex ->
                    console.log (sprintf "Error getting user metadata: %s" ex.Message)
                    dispatch (SetUserDetails None)
            }
            |> Promise.start),
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
                prop.text "Loading"
                button.color.inherit'
            ]
        ]
    | HasError e ->
        Html.div [
            Mui.button [
                prop.text ("Error. Reset")
                prop.onClick (fun e -> dispatch (SetAuthState Anonymous))
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
        let pic = stringOrEmpty user.picture

        Html.div [
            Mui.button [
                prop.text (stringOrEmpty user.name)
                prop.onClick (fun e -> dispatch (SetAnchorEl(Some(e.currentTarget :?> Browser.Types.Element))))
                button.color.inherit'
            ]

            Mui.menu [
                menu.anchorEl state.AnchorEl
                menu.keepMounted true
                menu.open' state.AnchorEl.IsSome
                menu.onClose (fun _ -> dispatch (SetAnchorEl None))

                menu.children [
                    Mui.menuItem [
                        menuItem.children "Logout"
                        prop.onClick
                            (fun _ ->
                                dispatch (SetAnchorEl None)
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
