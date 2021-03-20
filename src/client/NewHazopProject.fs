[<RequireQualifiedAccess>]
module FelizServerless.NewHazopProject

open System
open FelizServerless.Hazop

type State =
    {
        Open: bool
        Id: ProjectId
        Title: string
        Description: string
    }

let init () =
    {
        Open = false
        Id = ProjectId(Guid.NewGuid())
        Title = ""
        Description = ""
    }

type Msg =
    | OpenDialog
    | CloseDialog
    | Submit
    | Cancel
    | TitleChanged of string
    | DescChanged of string
    | Reset

let update msg state : State =
    match msg with
    | OpenDialog -> { state with Open = true }
    | CloseDialog -> { state with Open = false }
    | Submit -> init ()
    | Cancel -> init ()
    | TitleChanged s -> { state with Title = s }
    | DescChanged s -> { state with Description = s }
    | Reset -> { init () with Open = true }
