[<AutoOpen>]
module FelizServerless.Utils

open Fable.Core

[<Emit("btoa($0)")>]
let toBase64String (bytes: byte []) : string = jsNative

[<Emit("atob($0)")>]
let fromBase64String (string: string) : byte [] = jsNative
