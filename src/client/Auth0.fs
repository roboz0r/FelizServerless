[<RequireQualifiedAccess>]
module FelizServerless.Auth0

open Fable.Core
open System

[<Literal>]
let Name = "FuncEng"

[<Literal>]
let Domain = "funceng.au.auth0.com"

[<Literal>]
let ClientId = "p4dJdVxaclOlk7YRqj8tYulBifQGlb6s"

[<Literal>]
let Audience = "https://funceng.au.auth0.com/api/v2/"

let UniqueId sub = $"https://{Domain}/%s{sub}"

type IUserMetaData =
    interface
    end

type IIdentity =
    [<EmitProperty("connection")>]
    abstract Connection : string

    [<EmitProperty("provider")>]
    abstract Provider : string

    [<EmitProperty("user_id")>]
    abstract UserId : string

    [<EmitProperty("isSocial")>]
    abstract IsSocial : bool

type IUserDetails =
    [<EmitProperty("created_at")>]
    abstract CreatedAt : DateTime

    [<EmitProperty("email")>]
    abstract Email : string

    [<EmitProperty("email_verified")>]
    abstract EmailVerified : bool

    [<EmitProperty("identities")>]
    abstract Identities : IIdentity array

    [<EmitProperty("name")>]
    abstract Name : string

    [<EmitProperty("nickname")>]
    abstract Nickname : string

    [<EmitProperty("picture")>]
    abstract Picture : string

    [<EmitProperty("updated_at")>]
    abstract UpdatedAt : DateTime

    [<EmitProperty("user_id")>]
    abstract UserId : string

    [<EmitProperty("user_metadata")>]
    abstract UserMetadata : IUserMetaData

    [<EmitProperty("last_ip")>]
    abstract LastIp : string

    [<EmitProperty("last_login")>]
    abstract LastLogin : DateTime

    [<EmitProperty("logins_count")>]
    abstract LoginsCount : int
