module Mime

open Fable.Core

[<Import("mime", "mime")>]
type private Mime() =
    class
        abstract member getType : string -> string option
        default this.getType(fileName: string) : string option = jsNative

        abstract member getExtension : string -> string option
        default this.getExtension(mimeType: string) : string option = jsNative

        // TODO For some reason calls to this don't add additional cases to the mime object
        // abstract member define : JS.Map<string, string seq> * bool -> unit
        // default this.define(typeMap, force) : unit = jsNative
    end

[<Emit("require('mime')")>]
let private mime' : Mime = jsNative

let private mime = mime'

let getType fileName = mime.getType fileName
let getExtension mimeType = mime.getExtension mimeType
// let define typeMap = mime.define(typeMap, false)
// let forceDefine typeMap = mime.define(typeMap, true)