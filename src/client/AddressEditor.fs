[<RequireQualifiedAccess>]
module FelizServerless.HazopAddress

open FelizServerless.Hazop

type State = Address

let init address = address

type Msg =
    | Line1Changed of string
    | Line2Changed of string
    | CityChanged of string
    | StateChanged of string
    | PostcodeChanged of string
    | CountryChanged of string

let update msg (state: State) =
    match msg with
    | Line1Changed line1 -> { state with Line1 = line1 }
    | Line2Changed line2 -> { state with Line2 = line2 }
    | CityChanged city -> { state with City = city }
    | StateChanged state' -> { state with State = state' }
    | PostcodeChanged pc -> { state with Postcode = pc }
    | CountryChanged country -> { state with Country = country }
