module FelizServerless.App

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Feliz.MaterialUI


type State =
    {
        CurrentUrl: string list
        Counter: Counter.State
        ShowDrawer: bool
        ToDoList: ToDoList.State
    }

type Msg =
    | UrlChanged of string list
    | ToggleDrawer
    | Counter of Counter.Msg
    | ToDo of ToDoList.Msg

let init () =
    let counter, cmd = Counter.init ()

    {
        CurrentUrl = Router.currentUrl ()
        Counter = counter
        ShowDrawer = true
        ToDoList = ToDoList.init ()
    },
    Cmd.batch [ cmd ]

let update msg state =
    match msg with
    | (UrlChanged segments) -> { state with CurrentUrl = segments }, Cmd.none
    | ToggleDrawer ->
        { state with
            ShowDrawer = not state.ShowDrawer
        },
        Cmd.none
    | Counter msg ->
        let x, cmd = Counter.update msg state.Counter
        { state with Counter = x }, cmd
    | ToDo msg ->
        let todo = ToDoList.update msg state.ToDoList
        { state with ToDoList = todo }, Cmd.none


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
            |}
            )

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


[<ReactComponent>]
let Router () =
    let state, dispatch = React.useElmish (init, update)
    let classes = useStyles ()

    React.router [
        router.onUrlChanged (UrlChanged >> dispatch)
        router.children [
            Html.div [
                prop.className classes.root
                prop.children [
                    Mui.appBar [
                        appBar.position.fixed'
                        prop.className classes.appBar
                        appBar.children [
                            Mui.toolbar [
                                Mui.iconButton [
                                    iconButton.edge.start
                                    prop.className classes.menuButton
                                    iconButton.color.inherit'
                                    prop.ariaLabel "menu"
                                    prop.children [
                                        Fable.MaterialUI.Icons.menuIcon []
                                    ]
                                    prop.onClick (fun _ -> ToggleDrawer |> dispatch)
                                ]
                                Mui.typography [
                                    typography.variant.h6
                                    prop.className classes.title

                                    match state.CurrentUrl with
                                    | [] -> "Home"
                                    | [ "users" ] -> "Users"
                                    | [ "users"; Route.Int userId ] -> (sprintf "User ID %d" userId)
                                    | [ "ToDo" ] -> "To Do List"
                                    | _ -> "Not found"
                                    |> typography.children
                                ]
                                AuthStatus.LogIn()
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
                        | [ "users" ] ->
                            Html.div [
                                Mui.typography "Nothing to see here"
                            ]
                        | [ "users"; Route.Int userId ] -> Html.h1 (sprintf "User ID %d" userId)
                        | [ "DevEnv" ] -> DevEnv.View()
                        | [ "ToDo" ] -> ToDoList.View state.ToDoList (ToDo >> dispatch)
                        | _ -> Html.h1 "Not found"
                    ]
                ]
            ]
        ]
    ]
