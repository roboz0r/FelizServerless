module FelizServerless.Drawer

open Feliz
open Feliz.Router
open Elmish
open Feliz.UseElmish
open Fable.MaterialUI.Icons
open Feliz.MaterialUI

let drawerWidth = 240

let useStyles: unit -> _ =
    Styles.makeStyles
        (fun styles theme ->
            {| hide = styles.create [ style.display.none ]
               drawer =
                   styles.create [
                       style.width drawerWidth
                       style.flexShrink 0
                       style.marginRight (theme.spacing (1))
                   ]
               drawerPaper =
                   styles.create [
                       style.width drawerWidth
                   ] |})

[<ReactComponent>]
let Drawer showDrawer =
    //TODO Figure out aligning the drawer to not cover App Bar
    // https://material-ui.com/components/drawers/#permanent-drawer

    let classes = useStyles ()

    Mui.drawer [
        prop.className (
            if showDrawer then
                classes.drawer
            else
                classes.hide
        )
        drawer.variant.permanent
        drawer.classes.paper classes.drawerPaper
        drawer.anchor.left
        drawer.children [
            Mui.toolbar []
            Mui.divider []
            Mui.list [
                Mui.listItem [
                    prop.onClick (fun _ -> Router.navigate (""))
                    listItem.button true
                    listItem.children [
                        Mui.listItemIcon [ homeIcon [] ]
                        Mui.listItemText "Home"
                    ]
                ]
                Mui.listItem [
                    prop.onClick (fun _ -> Router.navigate ("users"))
                    listItem.button true
                    listItem.children [
                        Mui.listItemIcon [ groupIcon [] ]
                        Mui.listItemText "Users"
                    ]
                ]
                Mui.listItem [
                    prop.onClick (fun _ -> Router.navigate ("DevEnv"))
                    listItem.button true
                    listItem.children [
                        Mui.listItemIcon [ groupIcon [] ]
                        Mui.listItemText "Development"
                    ]
                ]
            ]
        ]
    ]
