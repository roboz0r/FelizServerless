[<RequireQualifiedAccess>]
module FelizServerless.Auth0

open Fable.Core.JsInterop
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

type IUserMetaData =
    interface
    end

type IIdentity =
    [<EmitProperty("connection")>] //"Username-Password-Authentication"
    abstract Connection: string

    [<EmitProperty("provider")>] //"auth0"
    abstract Provider: string

    [<EmitProperty("user_id")>] //"600297bd0cadb70069b2e324"
    abstract UserId: string

    [<EmitProperty("isSocial")>]
    abstract IsSocial: bool

type IUserDetails =
    [<EmitProperty("created_at")>]
    abstract CreatedAt: DateTime

    [<EmitProperty("email")>]
    abstract Email: string

    [<EmitProperty("email_verified")>]
    abstract EmailVerified: bool

    [<EmitProperty("identities")>]
    abstract Identities: IIdentity array

    [<EmitProperty("name")>] // "mental.blindfolds.on@gmail.com",
    abstract Name: string

    [<EmitProperty("nickname")>] // "mental.blindfolds.on",
    abstract Nickname: string

    [<EmitProperty("picture")>] // "https://s.gravatar.com/avatar/ef828f63e7083c1c8fcfb09471552af3?s=480&r=pg&d=https%3A%2F%2Fcdn.auth0.com%2Favatars%2Fme.png",
    abstract Picture: string

    [<EmitProperty("updated_at")>] // "2021-02-06T01:09:38.364Z",
    abstract UpdatedAt: DateTime

    [<EmitProperty("user_id")>] // "auth0|600297bd0cadb70069b2e324",
    abstract UserId: string

    [<EmitProperty("user_metadata")>]
    abstract UserMetadata: IUserMetaData

    [<EmitProperty("last_ip")>] // "121.200.4.122",
    abstract LastIp: string

    [<EmitProperty("last_login")>]
    abstract LastLogin: DateTime

    [<EmitProperty("logins_count")>]
    abstract LoginsCount: int
