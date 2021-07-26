[<AutoOpen>]
module MuiExtensions

open Feliz
open Feliz.MaterialUI

type Mui 
    with
    static member gridItem (children:seq<ReactElement>):ReactElement = 
        Mui.grid [
            grid.item true
            grid.children children
        ]

    static member gridItem (props:seq<IReactProperty>):ReactElement = 
        Mui.grid [
            grid.item true
            yield! props
        ]

    static member loadingDialog = 
            Mui.dialog [
                dialog.disableBackdropClick true
                dialog.open' true
                dialog.children [
                    Mui.dialogTitle "Loading..."
                ]
            ]