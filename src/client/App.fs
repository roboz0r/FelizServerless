[<RequireQualifiedAccess>]
module FelizServerless.App

open Feliz
open Feliz.Router
open Elmish

let console = Fable.Core.JS.console

type private AuthState = Fable.Auth0.AuthState
module HazopPage = HazopPage2

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
        HazopProjectPage : HazopProjectPage.State option
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
        HazopProjectPage = None
    },
    Cmd.batch [ counterCmd; userCmd ]

let mapCmd (f:'TMsg -> 'UMsg) (state:'State, msg) = state, (Cmd.map f msg)

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
        console.log (sprintf "Auth status changed: %A" msg)

        let userPage, userCmd =
            let msg =
                UserPage.SetAuthStatus(newAuthState.AuthStatus)

            UserPage.update msg state.UserPage
            |> mapCmd UserPage

        match newAuthState.AuthStatus with
        | Auth0.Authenticated (user, token) ->

            let todo, toDoCmd =
                    let userId = UserId(user.UniqueId)
                    let api = ToDoList.toDoApi token
                    let msg = ToDoList.SetApi(api, userId)
                    ToDoList.update msg state.ToDoList
                    |> mapCmd ToDo 

            let hazop, hazopCmd =
                    let msg =
                        HazopPage.SetApi(HazopPage.hazopApi token)

                    HazopPage.update msg state.HazopPage
                    |> mapCmd HazopPage

            { state with
                AuthState = newAuthState
                UserPage = userPage
                ToDoList = todo
                HazopPage = hazop
            },
            Cmd.batch [
                userCmd
                toDoCmd
                hazopCmd
            ]
        | _ ->
            let todo, toDoCmd =
                    ToDoList.update ToDoList.ClearApi state.ToDoList
                    |> mapCmd ToDo

            let hazop, hazopCmd =
                    HazopPage.update HazopPage.ClearApi state.HazopPage
                    |> mapCmd HazopPage

            { state with
                AuthState = newAuthState
                UserPage = userPage
                ToDoList = todo
                HazopPage = hazop
            },
            Cmd.batch [
                userCmd
                toDoCmd
                hazopCmd
            ]


    | UserPage msg ->
        let page, cmd = UserPage.update msg state.UserPage
        { state with UserPage = page }, Cmd.map UserPage cmd
    | HazopPage msg ->
        match msg with
        | HazopPage.Msg.Navigation navMsg -> 
            match navMsg with
            | HazopPage.NavigateToMsg.ProjectSetup id -> failwith ""
            | HazopPage.NavigateToMsg.GuidewordSetup id -> failwith ""
        | _ -> 
            let page, cmd = HazopPage.update msg state.HazopPage
            { state with HazopPage = page }, Cmd.map HazopPage cmd
