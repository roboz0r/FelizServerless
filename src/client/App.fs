[<RequireQualifiedAccess>]
module FelizServerless.App

open Feliz
open Feliz.Router
open Elmish

type private AuthState = Fable.Auth0.AuthState.AuthState

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
        HazopPage = HazopPage.init ()
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

        let userCmds =
            Cmd.map UserPage (Cmd.batch [ userCmd1; userCmd2 ])

        let todo, toDoCmd =
            match auth.Token, auth.AuthState with
            | Some token, AuthState.Authenticated user ->
                let userId = UserId(Auth0.UniqueId user.sub.Value)

                let msg =
                    ToDoList.SetApi(ToDoList.toDoApi token, userId)

                ToDoList.update msg state.ToDoList
            | _ -> ToDoList.update ToDoList.ClearApi state.ToDoList

        let hazop, hazopCmd =
            match auth.Token, auth.AuthState with
            | Some token, AuthState.Authenticated _ ->
                let msg =
                    HazopPage.SetApi(HazopPage.hazopApi token)

                HazopPage.update msg state.HazopPage
            | _ -> HazopPage.update HazopPage.ClearApi state.HazopPage

        { state with
            AuthState = auth
            UserPage = userPage
            ToDoList = todo
            HazopPage = hazop
        },
        Cmd.batch [
            userCmds
            (Cmd.map ToDo toDoCmd)
            (Cmd.map HazopPage hazopCmd)
        ]

    | UserPage msg ->
        let page, cmd = UserPage.update msg state.UserPage
        { state with UserPage = page }, Cmd.map UserPage cmd
    | HazopPage msg ->
        let page, cmd = HazopPage.update msg state.HazopPage
        { state with HazopPage = page }, Cmd.map HazopPage cmd
