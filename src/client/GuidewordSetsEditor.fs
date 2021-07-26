[<RequireQualifiedAccess>]
module FelizServerless.GuidewordSetsEditor

open System
open FelizServerless.Hazop

type State =
    {
        Selected: GuidewordSetEditor.State option
        GuidewordSets: GuidewordSet list
    }
    member this.ToGuidewordSets() : GuidewordSet list =
        match this.Selected with
        | Some selected ->
            this.GuidewordSets
            |> List.map
                (fun gw ->
                    if gw.Id = selected.Id then
                        selected.ToGuidewordSet()
                    else
                        gw)
        | None -> this.GuidewordSets

    static member OfGuidewordSets gwSets : State =
        {
            Selected = None
            GuidewordSets = gwSets
        }

type Msg = 
    | SelectGuidewordSet of GuidewordSet
    | SetChanged of GuidewordSetEditor.Msg

let init gwSets = State.OfGuidewordSets gwSets

let update msg state : State = 
    match msg with
    | SelectGuidewordSet gwSet -> 
        { state with Selected = Some (GuidewordSetEditor.State.OfGuidewordSet gwSet); GuidewordSets = state.ToGuidewordSets() }
    | SetChanged msg -> 
        let selected = 
            state.Selected
            |> Option.map (fun selected -> GuidewordSetEditor.update msg selected)
        { state with Selected = selected }
