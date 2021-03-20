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

