module FelizServerless.AppView

open Feliz
open Feliz.Router
open Feliz.UseElmish
open Feliz.MaterialUI

type private State = App.State
type private Msg = App.Msg

let private useStyles : unit -> App.Styles =
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

// https://stackoverflow.com/questions/56432167/how-to-style-components-using-makestyles-and-still-have-lifecycle-methods-in-mat
// https://cmeeren.github.io/Feliz.MaterialUI/#usage/themes

let private myTheme =
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
                textField.fullWidth true
            ]
            theme.overrides.muiPaper.rounded [
                style.padding 10
            ]
            theme.props.muiButton [
                button.variant.contained
                button.color.primary
            ]
        ]
    )

[<ReactComponent>]
let AppBar (styles: App.Styles) (state: App.State) dispatch =
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
                        if state.ShowDrawer then
                            Fable.MaterialUI.Icons.menuOpenIcon []
                        else
                            Fable.MaterialUI.Icons.menuIcon []
                    ]
                    prop.onClick (fun _ -> Msg.ToggleDrawer |> dispatch)
                ]
                Mui.typography [
                    typography.variant.h6
                    prop.className styles.Title

                    match state.CurrentUrl with
                    | [] -> "Home"
                    | [ "users" ] -> "Users"
                    | [ "users"; Route.Int userId ] -> (sprintf "User ID %d" userId)
                    | [ "ToDo" ] -> "To Do List"
                    | [ "DevEnv" ] -> "Development Environment"
                    | [ "Hazop" ] -> "Hazop Study"
                    | _ -> "Not found"
                    |> typography.children
                ]
                AuthStatusView.LogIn state.AuthState (Msg.AuthStatus >> dispatch)
            ]
        ]
    ]

[<ReactComponent>]
let MainView (drawerStyles: Drawer.Styles) (styles: App.Styles) (state: App.State) dispatch =
    Mui.container [
        prop.className styles.Root
        prop.children [
            AppBar styles state dispatch
            DrawerView.Drawer drawerStyles state.ShowDrawer

            Html.main [
                //Empty toolbar to bump down the content below actual toolbar.
                Mui.toolbar []
                match state.CurrentUrl with
                | [] ->
                    Html.div [
                        CounterView.Counter state.Counter (Msg.Counter >> dispatch)
                    ]
                | [ "users" ] -> UserPageView.View state.UserPage dispatch
                | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
                | [ "DevEnv" ] -> DevEnvView.View()
                | [ "ToDo" ] -> ToDoListView.View state.ToDoList (Msg.ToDo >> dispatch)
                | [ "Hazop" ] -> HazopPageView.View state.HazopPage (Msg.HazopPage >> dispatch)
                | _ -> Html.h1 "Not found"
            ]
        ]
    ]

[<ReactComponent>]
let Router state dispatch =
    let styles = useStyles ()
    let drawerStyles = Drawer.useStyles ()

    React.router [
        router.onUrlChanged (Msg.UrlChanged >> dispatch)
        router.children [
            Mui.themeProvider [
                themeProvider.theme myTheme
                themeProvider.children [
                    let mainView =
                        MainView drawerStyles styles state dispatch

                    (AuthProvider.AuthProvider mainView)
                ]
            ]
        ]
    ]

// Use for Feliz.useElmish instead of Elmish
[<ReactComponent>]
let FelizRouter () =
    let state, dispatch = React.useElmish (App.init, App.update)
    Router state dispatch
