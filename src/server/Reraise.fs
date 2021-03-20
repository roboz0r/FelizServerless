[<AutoOpen>]
/// Allows computation expressions to reraise exceptions
/// http://www.fssnip.net/k1/title/Reraise-everywhere
module FelizServerless.Server.Reraise

open System.Reflection

let internal clone (e: 'T :> exn) =
    let bf =
        System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()

    use m = new System.IO.MemoryStream()
    bf.Serialize(m, e)
    m.Position <- 0L
    bf.Deserialize m :?> 'T

let internal remoteStackTraceField =
    let getField name =
        typeof<System.Exception>.GetField (name, BindingFlags.Instance ||| BindingFlags.NonPublic)

    match getField "remote_stack_trace" with
    | null ->
        match getField "_remoteStackTraceString" with
        | null -> failwith "a piece of unreliable code has just failed."
        | f -> f
    | f -> f

let inline internal reraise' (e: #exn) =
    // clone the exception to avoid mutation side-effects
    let e' = clone e
    remoteStackTraceField.SetValue(e', e'.StackTrace + System.Environment.NewLine)
    raise e'
