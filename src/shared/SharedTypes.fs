namespace FelizServerless

open System

//https://docs.microsoft.com/en-us/dotnet/api/microsoft.azure.cosmos.container.createitemasync?view=azure-dotnet#exceptions
[<RequireQualifiedAccess>]
type CosmosError =
    | BadRequest of string
    | Forbidden of string
    | Conflict of string
    | RequestEntityTooLarge of string
    | TooManyRequests of string
    | Other of string
    | Multiple of string list

type ServerError =
    | AuthError of JwtError
    | DBError of CosmosError

type ICounter =
    {
        Init: unit -> Async<int>
        InitValue: int -> Async<int>
    }

type IClaims =
    {
        GetClaims: unit -> Async<Result<Claims2, JwtError>>
    }

type UserId =
    | UserId of string
    member this.Value = 
        let (UserId id) = this
        id

type ToDoId = Guid

type ToDoItem =
    {
        Id: ToDoId
        UserId: UserId
        Description: string
        Completed: bool
    }
    // static member sample = 
    //     let x = 
    //         {
    //             Id = System.Guid.Empty
    //             UserId = UserId "some Id"
    //             Description = ""
    //             Completed = false
    //         }
type IToDoItem =
    {
        List: unit -> Async<Result<List<ToDoItem>, ServerError>>
        Add: ToDoItem -> Async<Result<ToDoId, ServerError>>
        Update: ToDoItem -> Async<Result<ToDoId, ServerError>>
        Delete: ToDoItem -> Async<Result<ToDoId, ServerError>>
        GetItem: ToDoId -> Async<Result<ToDoItem, ServerError>>
    }
