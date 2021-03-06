module FelizServerless.App

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Feliz.MaterialUI

type private AuthState = Fable.Auth0.AuthState.AuthState

type State =
    {
        CurrentUrl: string list
        Counter: Counter.State
        ShowDrawer: bool
        ToDoList: ToDoList.State
        AuthState: AuthStatus.State
        UserPage: UserPage.State
    }

type Msg =
    | UrlChanged of string list
    | ToggleDrawer
    | Counter of Counter.Msg
    | ToDo of ToDoList.Msg
    | AuthStatus of AuthStatus.Msg
    | UserPage of UserPage.Msg

let init () =
    let counter, cmd = Counter.init ()

    let authState =
        AuthStatus.init [
            Scope.ReadCurrentUser
        ]

    {
        CurrentUrl = Router.currentUrl ()
        Counter = counter
        ShowDrawer = true
        ToDoList = ToDoList.init ()
        AuthState = authState
        UserPage = UserPage.init authState
    },
    cmd |> Cmd.map Counter

let update msg state =
    match msg with
    | UrlChanged segments -> { state with CurrentUrl = segments }, Cmd.none
    | ToggleDrawer ->
        { state with
            ShowDrawer = not state.ShowDrawer
        },
        Cmd.none
    | Counter msg ->
        let x = Counter.update msg state.Counter
        { state with Counter = x }, Cmd.none
    | ToDo msg ->
        let todo, cmd = ToDoList.update msg state.ToDoList
        { state with ToDoList = todo }, Cmd.map ToDo cmd
    | AuthStatus msg ->
        let auth = AuthStatus.update msg state.AuthState

        let page, cmd =
            UserPage.update (UserPage.SetAuthStatus auth) state.UserPage

        let page, cmd2 =
            match auth.Token with
            | Some token -> 
                let msg = UserPage.SetApi(UserPage.userApi token)
                UserPage.update msg page
            | None -> page, Cmd.none

        let userCmds = Cmd.map UserPage (Cmd.batch [ cmd; cmd2 ])

        let todo, cmd = 
            match auth.Token, auth.AuthState with
            | Some token, AuthState.Authenticated user -> 
                let userId = UserId (Auth0.UniqueId user.sub.Value)
                let msg = ToDoList.SetApi(ToDoList.toDoApi token, userId)
                ToDoList.update msg state.ToDoList
            | _ -> ToDoList.update ToDoList.ClearApi state.ToDoList

        { state with
            AuthState = auth
            UserPage = page
            ToDoList = todo
        }, Cmd.batch [ userCmds; (Cmd.map ToDo cmd) ]
        
    | UserPage msg ->
        let page, cmd = UserPage.update msg state.UserPage
        { state with UserPage = page }, Cmd.map UserPage cmd


let drawerWidth = 240

let useStyles: unit -> _ =
    Styles.makeStyles
        (fun styles theme ->
            {|
                root = styles.create [ style.display.flex ]
                appBar =
                    styles.create [
                        style.zIndex (theme.zIndex.drawer + 1)
                    ]
                menuButton =
                    styles.create [
                        style.marginRight (theme.spacing (2))
                    ]
                title = styles.create [ style.flexGrow 1 ]
            |})

//    + theme.transitions.create (
//        [| "margin"; "width" |],
//        { new TransitionOptions with
//            member __.delay = 0
//            member __.duration = theme.transitions.duration.leavingScreen
//            member __.easing = theme.transitions.easing.sharp }
//    )
//    appBarShift =
//        styles.create [
//            style.width (length.perc (100 - drawerWidth))
//            style.marginLeft drawerWidth
//        ]

// https://stackoverflow.com/questions/56432167/how-to-style-components-using-makestyles-and-still-have-lifecycle-methods-in-mat
// https://cmeeren.github.io/Feliz.MaterialUI/#usage/themes
let myTheme =
    Styles.createMuiTheme (
        [
            theme.overrides.muiCard.root [
                style.padding 10
                style.minWidth 150
                style.outlineStyle.auto
                style.outlineColor "primary"
            ]
        ]
    )

[<ReactComponent>]
let Router () =
    let state, dispatch = React.useElmish (init, update)
    let styles = useStyles ()

    React.router [
        router.onUrlChanged (UrlChanged >> dispatch)
        router.children [
            Mui.themeProvider [
                themeProvider.theme myTheme
                themeProvider.children [
                    Html.div [
                        prop.className styles.root
                        prop.children [
                            Mui.appBar [
                                appBar.position.fixed'
                                prop.className styles.appBar
                                appBar.children [
                                    Mui.toolbar [
                                        Mui.iconButton [
                                            iconButton.edge.start
                                            prop.className styles.menuButton
                                            iconButton.color.inherit'
                                            prop.ariaLabel "menu"
                                            prop.children [
                                                Fable.MaterialUI.Icons.menuIcon []
                                            ]
                                            prop.onClick (fun _ -> ToggleDrawer |> dispatch)
                                        ]
                                        Mui.typography [
                                            typography.variant.h6
                                            prop.className styles.title

                                            match state.CurrentUrl with
                                            | [] -> "Home"
                                            | [ "users" ] -> "Users"
                                            | [ "users"; Route.Int userId ] -> (sprintf "User ID %d" userId)
                                            | [ "ToDo" ] -> "To Do List"
                                            | _ -> "Not found"
                                            |> typography.children
                                        ]
                                        AuthStatus.LogIn state.AuthState (AuthStatus >> dispatch)
                                    ]
                                ]
                            ]

                            Drawer.Drawer state.ShowDrawer

                            Html.main [
                                Mui.toolbar []
                                match state.CurrentUrl with
                                | [] ->
                                    Html.div [
                                        Counter.Counter state.Counter (Msg.Counter >> dispatch)
                                    ]
                                | [ "users" ] -> UserPage.View state.UserPage dispatch
                                | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
                                | [ "DevEnv" ] -> DevEnv.View()
                                | [ "ToDo" ] -> ToDoList.View state.ToDoList (ToDo >> dispatch)
                                | _ -> Html.h1 "Not found"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
