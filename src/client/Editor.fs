namespace FelizServerless

type EditorState<'T> =
    | Clean of 'T
    | Dirty of {| Clean: 'T; Current: 'T |}

    member this.Current =
        match this with
        | Clean x -> x
        | Dirty x -> x.Current

type Editor<'T, 'TError> =
    | Working of EditorState<'T>
    | Pending of EditorState<'T>
    | EditorError of EditorState<'T> * 'TError
    member this.IsDirty =
        match this with
        | Working (Dirty _) -> true
        | _ -> false

    member this.IsClean =
        match this with
        | Working (Clean _) -> true
        | _ -> false

module Editor =
    let create clean = Working(Clean clean)

    let map f x =
        match x with
        | Working (Clean state) ->
            let newState = f state

            if newState = state then
                Working(Clean newState)
            else
                Working(Dirty {| Clean = state; Current = newState |})
        | Working (Dirty state) ->
            let newState = f state.Current

            if newState = state.Clean then
                Working(Clean newState)
            else
                Working(Dirty {| state with Current = newState |})
        | Pending state -> Pending state
        | EditorError (state, err) -> EditorError(state, err)

    let resolve result editor =
        match editor, result with
        | Pending (Dirty state), Ok _ -> Working(Clean state.Current)
        | Pending (Clean state), Ok _ -> Working(Clean state)
        | Pending state, Error err -> EditorError(state, err)
        | Working state, _ -> Working state
        | EditorError (state, err), _ -> EditorError(state, err)

    let makePending editor =
        match editor with
        | Working x
        | Pending x -> Pending x
        | EditorError (x, err) -> Pending x

    let clean editor =
        let clean' =
            function
            | Clean x -> Clean x
            | Dirty x -> Clean x.Clean

        match editor with
        | Working x -> Working(clean' x)
        | Pending x -> Working(clean' x)
        | EditorError (x, err) -> Working(clean' x)

    let getCurrent editor =
        let getCurrent' =
            function
            | Clean x -> x
            | Dirty x -> x.Current

        match editor with
        | Working x -> (getCurrent' x)
        | Pending x -> (getCurrent' x)
        | EditorError (x, err) -> (getCurrent' x)

    let makeError err editor =
        match editor with
        | Working x -> EditorError(x, err)
        | Pending x -> EditorError(x, err)
        | EditorError (x, _) -> EditorError(x, err)
