[<RequireQualifiedAccess>]
module FelizServerless.GuidewordEditor

open System
open FelizServerless.Hazop

type State =
    {
        Id: GuidewordId
        Order: float
        Guideword: string
        Deviation: string
        GuidewordIndex: Set<string>
        DeviationIndex: Set<string>
    }
    member this.ToGuideword() = 
        {
            Id = this.Id
            Order = int this.Order
            Guideword = this.Guideword
            Deviation = 
                match this.Deviation with
                | "" -> None
                | s -> Some s
        }
    static member OfGuideword gwIx devIx (guideword:Guideword)  =
        {
            Id = guideword.Id
            Order = float guideword.Order
            Guideword = guideword.Guideword
            Deviation = String.OfOption guideword.Deviation
            GuidewordIndex = gwIx
            DeviationIndex = devIx
        }


let init gwSet devSet guideword = State.OfGuideword gwSet devSet guideword

type Msg =
    | OrderChanged of float
    | GuidewordChanged of string
    | DeviationChanged of string

let update msg state : State =
    match msg with
    | OrderChanged order -> { state with Order = order }
    | GuidewordChanged guideword -> { state with Guideword = guideword }
    | DeviationChanged deviation -> { state with Deviation = deviation }