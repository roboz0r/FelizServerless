module FelizServerless.DevEnv

open Feliz
open Feliz.UseElmish
open Elmish
open Feliz.MaterialUI

type Software =
    {
        Name: string
        Description: string
        Link: string
    }

let allSoftware =
    [
        {
            Name = "Azure Functions Core Tools"
            Description = "Provides cli commands for azure functions 'func'.  "
            Link = "https://github.com/Azure/azure-functions-core-tools#installing"
        }
        {
            Name = "Azure CLI"
            Description = "Provides ability to log in to azure via cli 'az'. Needed for Azure functions publishing."
            Link = "https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows?tabs=azure-cli"
        }
        {
            Name = "Everything"
            Description = "Indexes local drives and provies and actually decent search experience on windows."
            Link = "https://www.voidtools.com/downloads/"
        }
        {
            Name = "Git"
            Description = "Version Management"
            Link = "https://www.git-scm.com/download/win"
        }
        {
            Name = ".Net Core SDK"
            Description = ""
            Link = "https://dotnet.microsoft.com/download/dotnet-core/"
        }
        {
            Name = "Visual Studio Code"
            Description = "Primary code editor. Enhanced with extensions."
            Link = "https://code.visualstudio.com/"
        }
        {
            Name = "Node js"
            Description = "Tools for installing and running javascript"
            Link = "https://nodejs.org/en/download/"
        }
        {
            Name = "Notepad++"
            Description =
                "Lightweight text editor. Useful for quick edits when you dont need to load up a vscode session."
            Link = "https://notepad-plus-plus.org/downloads/"
        }
        {
            Name = "Paint.Net"
            Description = "Image editing tool"
            Link = "https://www.getpaint.net/download.html#download"
        }
        {
            Name = "Visual Studio Community Edition"
            Description = "Image editing tool"
            Link = "https://visualstudio.microsoft.com/vs/community/"
        }
    ]


let private renderSoftware (x: Software) =
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
        yield! allSoftware |> List.map renderSoftware
    ]
