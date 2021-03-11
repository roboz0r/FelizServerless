module FelizServerless.Drawer

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Fable.MaterialUI.Icons
open Feliz.MaterialUI

type Styles =
    {
        Hide: string
        Drawer: string
        DrawerPaper: string
    }

let useStyles : unit -> _ =
    let drawerWidth = 240

    Styles.makeStyles
        (fun styles theme ->
            {
                Hide = styles.create [ style.display.none ]
                Drawer =
                    styles.create [
                        style.width drawerWidth
                        style.flexShrink 0
                        style.marginRight (theme.spacing (1))
                    ]
                DrawerPaper =
                    styles.create [
                        style.width drawerWidth
                    ]
            })

let private menuItem (currentUrl: string list, path: string, displayText: string, icon: ReactElement) =
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
                menuItem (currentUrl, "users", "Users", (groupIcon []))
                menuItem (currentUrl, "DevEnv", "Development", (groupIcon []))
                menuItem (currentUrl, "ToDo", "To Do List", (assignmentIcon []))
                menuItem (currentUrl, "Hazop", "Hazop Study", (Html.none))
            ]
        ]
    ]
