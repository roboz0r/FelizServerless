[<AutoOpen>]
module Extensions

open System
open Fable.Core
open Fable.Core.JsInterop

[<RequireQualifiedAccess>]
module StaticFile =

    /// Function that imports a static file by it's relative path.
    let inline import (path: string) : string = importDefault<string> path

[<RequireQualifiedAccess>]
module Config =
    /// Returns the value of a configured variable using its key.
    /// Retursn empty string when the value does not exist
    [<Emit("process.env[$0] ? process.env[$0] : ''")>]
    let variable (key: string) : string = jsNative

    /// Tries to find the value of the configured variable if it is defined or returns a given default value otherwise.
    let variableOrDefault (key: string) (defaultValue: string) =
        let foundValue = variable key

        if String.IsNullOrWhiteSpace foundValue then
            defaultValue
        else
            foundValue

// Stylesheet API
// let private stylehsheet = Stylesheet.load "./fancy.css"
// stylesheet.["fancy-class"] which returns a string
module Stylesheet =

    type IStylesheet =
        [<Emit "$0[$1]">]
        abstract Item : className: string -> string

    /// Loads a CSS module and makes the classes within available
    let inline load (path: string) = importDefault<IStylesheet> path

module String =
    /// Returns an empty string "" if the option is None otherwise returns the string
    let OfOption =
        function
        | Some s -> s
        | None -> ""

    /// Shortens a string to the `length` specified. Adds trailing "..." if the string was shortened.
    let truncate length (s: String) =
        if s.Length <= length then
            s
        else
            s.[0..length] + "..."

module JSON =
    let stringify o =
        Fable.Core.JS.JSON.stringify (o, Unchecked.defaultof<_>, 4)
