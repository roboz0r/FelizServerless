namespace FelizServerless

type ICounter =
    {
        Init: unit -> Async<int>
        InitValue: int -> Async<int>
    }
