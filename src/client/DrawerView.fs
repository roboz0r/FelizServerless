module FelizServerless.DrawerView

open Feliz
open Feliz.Router
open Fable.MaterialUI.Icons
open Feliz.MaterialUI
open FelizServerless.Drawer

[<ReactComponent>]
let MenuItem (currentUrl: string list, path: string, displayText: string, icon: ReactElement) =
    Mui.listItem [
        prop.onClick (fun _ -> Router.navigate (path))
        listItem.button true
        listItem.selected (
            match currentUrl with
            | [ x ] when x = path -> true
            | _ -> false
        )
        listItem.children [
            Mui.listItemIcon [ icon ]
            Mui.listItemText displayText
        ]
    ]

[<ReactComponent>]
let Drawer (styles: Styles) showDrawer =
    let currentUrl = Router.currentUrl ()

    Mui.drawer [
        prop.className (
            if showDrawer then
                styles.Drawer
            else
                styles.Hide
        )
        drawer.variant.permanent
        drawer.classes.paper styles.DrawerPaper
        drawer.anchor.left
        drawer.children [
            Mui.toolbar []
            Mui.divider []
            Mui.list [
                Mui.listItem [
                    prop.onClick (fun _ -> Router.navigate (""))
                    listItem.button true
                    listItem.selected (
                        match currentUrl with
                        | []
                        | [ "" ] -> true
                        | _ -> false
                    )
                    listItem.children [
                        Mui.listItemIcon [ homeIcon [] ]
                        Mui.listItemText "Home"
                    ]
                ]
                MenuItem(currentUrl, "users", "Users", (groupIcon []))
                MenuItem(currentUrl, "DevEnv", "Development", (groupIcon []))
                MenuItem(currentUrl, "ToDo", "To Do List", (assignmentIcon []))
                MenuItem(currentUrl, "Hazop", "Hazop Study", (Html.none))
            ]
        ]
    ]
