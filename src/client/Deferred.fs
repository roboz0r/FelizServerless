namespace FelizServerless

type Deferred<'T, 'U> =
    | HasNotStartedYet
    | InProgress of 'U
    | Resolved of 'T


[<RequireQualifiedAccess>]
module Deferred =

    /// Returns whether the `Deferred<'T>` value has been resolved or not.
    let resolved =
        function
        | HasNotStartedYet -> false
        | InProgress _ -> false
        | Resolved _ -> true

    /// Returns whether the `Deferred<'T>` value is in progress or not.
    let inProgress =
        function
        | HasNotStartedYet -> false
        | InProgress _ -> true
        | Resolved _ -> false

    /// Transforms the underlying value of the input deferred value when it exists from type to another
    let map (transform: 'T -> 'U) (deferred: Deferred<'T, 'V>) : Deferred<'U, 'V> =
        match deferred with
        | HasNotStartedYet -> HasNotStartedYet
        | InProgress x -> InProgress x
        | Resolved value -> Resolved(transform value)

    /// Verifies that a `Deferred<'T>` value is resolved and the resolved data satisfies a given requirement.
    let exists (predicate: 'T -> bool) =
        function
        | HasNotStartedYet -> false
        | InProgress _ -> false
        | Resolved value -> predicate value

    /// Like `map` but instead of transforming just the value into another type in the `Resolved` case, it will transform the value into potentially a different case of the the `Deferred<'T>` type.
    let bind (transform: 'T -> Deferred<'U, 'V>) (deferred: Deferred<'T, 'V>) : Deferred<'U, 'V> =
        match deferred with
        | HasNotStartedYet -> HasNotStartedYet
        | InProgress x -> InProgress x
        | Resolved value -> transform value
