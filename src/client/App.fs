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
        HazopPage: Hazop.State
    }

type Msg =
    | UrlChanged of string list
    | ToggleDrawer
    | Counter of Counter.Msg
    | ToDo of ToDoList.Msg
    | AuthStatus of AuthStatus.Msg
    | UserPage of UserPage.Msg
    | HazopPage of Hazop.Msg

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
        HazopPage = Hazop.init()
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

        let userPage, userCmd1 =
            UserPage.update (UserPage.SetAuthStatus auth) state.UserPage

        let userPage, userCmd2 =
            match auth.Token with
            | Some token -> 
                let msg = UserPage.SetApi(UserPage.userApi token)
                UserPage.update msg userPage
            | None -> userPage, Cmd.none

        let userCmds = Cmd.map UserPage (Cmd.batch [ userCmd1; userCmd2 ])

        let todo, toDoCmd = 
            match auth.Token, auth.AuthState with
            | Some token, AuthState.Authenticated user -> 
                let userId = UserId (Auth0.UniqueId user.sub.Value)
                let msg = ToDoList.SetApi(ToDoList.toDoApi token, userId)
                ToDoList.update msg state.ToDoList
            | _ -> ToDoList.update ToDoList.ClearApi state.ToDoList

        let hazop, hazopCmd = 
            match auth.Token, auth.AuthState with
            | Some token, AuthState.Authenticated _ -> 
                let msg = Hazop.SetApi(Hazop.hazopApi token)
                Hazop.update msg state.HazopPage
            | _ -> Hazop.update Hazop.ClearApi state.HazopPage

        { state with
            AuthState = auth
            UserPage = userPage
            ToDoList = todo
            HazopPage = hazop
        }, Cmd.batch [ userCmds; (Cmd.map ToDo toDoCmd); (Cmd.map HazopPage hazopCmd) ]
        
    | UserPage msg ->
        let page, cmd = UserPage.update msg state.UserPage
        { state with UserPage = page }, Cmd.map UserPage cmd
    | HazopPage msg -> 
        let page, cmd = Hazop.update msg state.HazopPage
        { state with HazopPage = page }, Cmd.map HazopPage cmd


let drawerWidth = 240

type Styles = {
    Root:string
    AppBar:string
    MenuButton:string
    Title:string
}

let useStyles: unit -> _ =
    Styles.makeStyles
        (fun styles theme ->
            {
                Root = styles.create [ style.display.flex ]
                AppBar =
                    styles.create [
                        style.zIndex (theme.zIndex.drawer + 1)
                    ]
                MenuButton =
                    styles.create [
                        style.marginRight (theme.spacing (2))
                    ]
                Title = styles.create [ style.flexGrow 1 ]
            })

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
            theme.props.muiTextField [ 
                textField.margin.normal
                textField.variant.filled
            ]
            theme.overrides.muiPaper.rounded [
                style.padding 10
            ]
        ]
    )

[<ReactComponent>]
let private AppBar styles state dispatch = 
    Mui.appBar [
        appBar.position.fixed'
        prop.className styles.AppBar
        appBar.children [
            Mui.toolbar [
                Mui.iconButton [
                    iconButton.edge.start
                    prop.className styles.MenuButton
                    iconButton.color.inherit'
                    prop.ariaLabel "menu"
                    prop.children [
                        if state.ShowDrawer then Fable.MaterialUI.Icons.menuOpenIcon []
                        else Fable.MaterialUI.Icons.menuIcon []
                    ]
                    prop.onClick (fun _ -> ToggleDrawer |> dispatch)
                ]
                Mui.typography [
                    typography.variant.h6
                    prop.className styles.Title

                    match state.CurrentUrl with
                    | [] -> "Home"
                    | [ "users" ] -> "Users"
                    | [ "users"; Route.Int userId ] -> (sprintf "User ID %d" userId)
                    | [ "ToDo" ] -> "To Do List"
                    | [ "DevEnv"] -> "Development Environment"
                    | [ "Hazop"] -> "Hazop Study"
                    | _ -> "Not found"
                    |> typography.children
                ]
                AuthStatus.LogIn state.AuthState (AuthStatus >> dispatch)
            ]
        ]
    ]

[<ReactComponent>]
let Router () =
    let state, dispatch = React.useElmish (init, update)
    let styles = useStyles ()
    let drawerStyles = Drawer.useStyles()

    React.router [
        router.onUrlChanged (UrlChanged >> dispatch)
        router.children [
            Mui.themeProvider [
                themeProvider.theme myTheme
                themeProvider.children [
                    Html.div [
                        prop.className styles.Root
                        prop.children [
                            AppBar styles state dispatch

                            Drawer.Drawer drawerStyles state.ShowDrawer

                            Html.main [
                                Mui.toolbar [] //Empty toolbar to bump down the content below actual toolbar.
                                match state.CurrentUrl with
                                | [] ->
                                    Html.div [
                                        Counter.Counter state.Counter (Msg.Counter >> dispatch)
                                    ]
                                | [ "users" ] -> UserPage.View state.UserPage dispatch
                                | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
                                | [ "DevEnv" ] -> DevEnv.View()
                                | [ "ToDo" ] -> ToDoList.View state.ToDoList (ToDo >> dispatch)
                                | [ "Hazop" ] -> Hazop.View state.HazopPage (HazopPage >> dispatch)
                                | _ -> Html.h1 "Not found"
                            ]
                        ]
                    ]
                ]
            ]
        ]
    ]
