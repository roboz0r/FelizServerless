[<RequireQualifiedAccess>]
module FelizServerless.DevEnvView

open Feliz
open Feliz.MaterialUI

[<ReactComponent>]
let RenderSoftware (x: DevEnv.Software) =
    Html.div [
        Mui.typography [
            typography.variant.h6
            typography.children x.Name
        ]
        Mui.typography [
            typography.children [ x.Description ]
        ]
        Html.a [
            prop.text "Download"
            prop.href x.Link
        ]
    ]

[<ReactComponent>]
let View () =
    Html.div [
        Mui.typography [
            typography.variant.h4
            typography.children "Installed Packages"
        ]
        yield! DevEnv.allSoftware |> List.map RenderSoftware
    ]
