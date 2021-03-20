module FelizServerless.Drawer

open Feliz
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
