[<RequireQualifiedAccess>]
module FelizServerless.GuidewordSetEditor

open System
open FelizServerless.Hazop

type State =
    {
        Id: GuidewordSetId
        Name: string
        Guidewords: GuidewordEditor.State list
        GuidewordIndex: Set<string>
        DeviationIndex: Set<string>
    }
    member this.ToGuidewordSet() : GuidewordSet =
        {
            Id = this.Id
            Name = this.Name
            Guidewords =
                this.Guidewords
                |> List.map (fun x -> x.ToGuideword())

        }

    static member OfGuidewordSet(gwSet: GuidewordSet) : State =

        let gwIx =
            gwSet.Guidewords
            |> List.map (fun x -> x.Guideword)
            |> Set
            |> Set.remove ""

        let devIx =
            gwSet.Guidewords
            |> List.choose (fun x -> x.Deviation)
            |> Set
            |> Set.remove ""

        let createGuideword =
            GuidewordEditor.State.OfGuideword gwIx devIx

        {
            Id = gwSet.Id
            Name = gwSet.Name
            Guidewords = gwSet.Guidewords |> List.map createGuideword
            GuidewordIndex = gwIx
            DeviationIndex = devIx
        }

let private indexGuidewords (guidewords: GuidewordEditor.State list) =
    guidewords
    |> List.map (fun x -> x.Guideword)
    |> Set
    |> Set.remove ""

let private indexDeviations (guidewords: GuidewordEditor.State list) =
    guidewords
    |> List.map (fun x -> x.Deviation)
    |> Set
    |> Set.remove ""

let init gwSet = State.OfGuidewordSet gwSet

type Msg =
    | NameChanged of string
    | AddGuideword
    | GuidewordChanged of GuidewordId * GuidewordEditor.Msg

let update msg state : State =
    match msg with
    | NameChanged setName -> { state with Name = setName }
    | AddGuideword ->
        let blankGuideword : GuidewordEditor.State =
            {
                Id = GuidewordId(Guid.NewGuid())
                Order = float state.Guidewords.Length
                Guideword = ""
                Deviation = ""
                GuidewordIndex = state.GuidewordIndex
                DeviationIndex = state.DeviationIndex
            }

        { state with
            Guidewords = state.Guidewords @ [ blankGuideword ]
        }

    | GuidewordChanged (id, msg) ->
        let newGuideword =
            GuidewordEditor.update msg (state.Guidewords |> List.find (fun x -> x.Id = id))

        let newGWList =
            state.Guidewords
            |> List.map (fun x -> if x.Id = id then newGuideword else x)
            |> List.sortBy (fun x -> x.Order)
            |> List.mapi
                (fun i x ->
                    let f = float i

                    if f = x.Order then
                        x
                    else
                        { x with Order = f })


        { state with
            Guidewords = newGWList
            GuidewordIndex = indexGuidewords newGWList
            DeviationIndex = indexDeviations newGWList
        }
