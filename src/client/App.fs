[<RequireQualifiedAccess>]
module FelizServerless.App

open Feliz
open Feliz.Router
open Elmish

let console = Fable.Core.JS.console

type private AuthState = Fable.Auth0.AuthState

type Styles =
    {
        Root: string
        AppBar: string
        MenuButton: string
        Title: string
    }

type State =
    {
        CurrentUrl: string list
        Counter: Counter.State
        ShowDrawer: bool
        ToDoList: ToDoList.State
        AuthState: AuthStatus.State
        UserPage: UserPage.State
        HazopPage: HazopPage.State
    }

type Msg =
    | UrlChanged of string list
    | ToggleDrawer
    | Counter of Counter.Msg
    | ToDo of ToDoList.Msg
    | AuthStatus of AuthStatus.Msg
    | UserPage of UserPage.Msg
    | HazopPage of HazopPage.Msg

let init () =
    let counter, counterCmd = Counter.init ()
    let counterCmd = counterCmd |> Cmd.map Counter

    let authState =
        AuthStatus.init [
            Scope.ReadCurrentUser
        ]

    let userPage, userCmd = UserPage.init authState.AuthStatus
    let userCmd = userCmd |> Cmd.map UserPage

    {
        CurrentUrl = Router.currentUrl ()
        Counter = counter
        ShowDrawer = true
        ToDoList = ToDoList.init ()
        AuthState = authState
        UserPage = userPage
        HazopPage = HazopPage.init ()
    },
    Cmd.batch [ counterCmd; userCmd ]

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
        let newAuthState = AuthStatus.update msg state.AuthState
        console.log (sprintf "%A" msg)
        match state.AuthState.AuthStatus, newAuthState.AuthStatus with
        | AuthStatus.AuthenticatedNoToken _, AuthStatus.Authenticated (user, token) ->
            let userPage, userCmd2 =
                let msg =
                    UserPage.SetAuthStatus(newAuthState.AuthStatus)

                UserPage.update msg state.UserPage

            let userCmds = Cmd.map UserPage userCmd2

            let todo, toDoCmd =
                match newAuthState.Token, newAuthState.AuthState with
                | Some token, AuthState.Authenticated user ->
                    let userId = UserId(Auth0.UniqueId user.sub.Value)

                    let msg =
                        ToDoList.SetApi(ToDoList.toDoApi token, userId)

                    ToDoList.update msg state.ToDoList
                | _ -> ToDoList.update ToDoList.ClearApi state.ToDoList

            let hazop, hazopCmd =
                match newAuthState.Token, newAuthState.AuthState with
                | Some token, AuthState.Authenticated _ ->
                    let msg =
                        HazopPage.SetApi(HazopPage.hazopApi token)

                    HazopPage.update msg state.HazopPage
                | _ -> HazopPage.update HazopPage.ClearApi state.HazopPage

            { state with
                AuthState = newAuthState
                UserPage = userPage
                ToDoList = todo
                HazopPage = hazop
            },
            Cmd.batch [
                userCmds
                (Cmd.map ToDo toDoCmd)
                (Cmd.map HazopPage hazopCmd)
            ]
        | _ ->
            let userPage, userCmd2 =
                let msg =
                    UserPage.SetAuthStatus(newAuthState.AuthStatus)

                UserPage.update msg state.UserPage

            let userCmds = Cmd.map UserPage userCmd2

            { state with
                AuthState = newAuthState
                UserPage = userPage
            },
            userCmds


    | UserPage msg ->
        let page, cmd = UserPage.update msg state.UserPage
        { state with UserPage = page }, Cmd.map UserPage cmd
    | HazopPage msg ->
        let page, cmd = HazopPage.update msg state.HazopPage
        { state with HazopPage = page }, Cmd.map HazopPage cmd
