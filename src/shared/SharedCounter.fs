namespace FelizServerless

type ICounter =
    {
        Init: unit -> Async<int>
        InitValue: int -> Async<int>
    }

type IClaims = 
    {
        GetClaims: unit -> Async<Result<Map<string, obj>, JwtError >>
    }