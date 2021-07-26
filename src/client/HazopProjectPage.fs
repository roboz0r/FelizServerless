[<RequireQualifiedAccess>]
module FelizServerless.HazopProjectPage

open System
open Elmish
open Feliz.Router
open FelizServerless.Hazop

type HazopProjectError = HazopProjectError of string

type State<'T, 'TMsg when 'T : (member ToProject : unit -> Project)> =
    {
        Id: ProjectId
        ProjectPage: Deferred<'T>
        Api: IHazopProject
    }

let init id api =
    {
        Id = id
        ProjectPage = HasNotStartedYet
        Api = api
    }

type Msg<'TMsg> = 
    | PageMsg of 'TMsg
    | Save

let update pageUpdate msg state : State<'T, 'TMsg> * Cmd<Msg<'TMsg>> = 
    match msg with
    | PageMsg msg -> 
        let projectPage = 
            state.ProjectPage |> Deferred.map (pageUpdate msg)
        {state with ProjectPage = projectPage }, Cmd.none
    | Save -> failwith "Not implemented"
        // TODO Write this module as non-generic then see if it can be made generic 
        // match state.ProjectPage with
        // | Deferred.Resolved project -> 
        //     let project = project.ToProject()
        //     state, 
        //         Cmd.OfAsync.perform 
        //             state.Api.Update 
        //             project

        