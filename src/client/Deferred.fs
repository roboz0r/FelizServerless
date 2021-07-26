namespace FelizServerless

type Deferred<'T> =
    | HasNotStartedYet
    | FirstLoad
    | InProgress of 'T
    | Resolved of 'T


[<RequireQualifiedAccess>]
module Deferred =

    /// Returns whether the `Deferred<'T>` value has been resolved or not.
    let resolved =
        function
        | HasNotStartedYet -> false
        | FirstLoad -> false
        | InProgress _ -> false
        | Resolved _ -> true

    /// Returns whether the `Deferred<'T>` value is in progress or not.
    let inProgress =
        function
        | HasNotStartedYet -> false
        | FirstLoad -> true
        | InProgress _ -> true
        | Resolved _ -> false

    /// Transforms the underlying value of the input deferred value when it exists from type to another
    let map (transform: 'T -> 'U) (deferred: Deferred<'T>) : Deferred<'U> =
        match deferred with
        | HasNotStartedYet -> HasNotStartedYet
        | FirstLoad -> FirstLoad
        | InProgress value -> InProgress (transform value)
        | Resolved value -> Resolved(transform value)

    /// Verifies that a `Deferred<'T>` value is resolved and the resolved data satisfies a given requirement.
    let exists (predicate: 'T -> bool) =
        function
        | HasNotStartedYet -> false
        | FirstLoad -> false
        | InProgress _ -> false
        | Resolved value -> predicate value

    /// Like `map` but instead of transforming just the value into another type in the `Resolved` case, it will transform the value into potentially a different case of the the `Deferred<'T>` type.
    // let bind (transform: 'T -> Deferred<'U>) (deferred: Deferred<'T>) : Deferred<'U> =
    //     match deferred with
    //     | HasNotStartedYet -> HasNotStartedYet
    //     | FirstLoad -> FirstLoad
    //     | InProgress x -> InProgress x
    //     | Resolved value -> transform value
