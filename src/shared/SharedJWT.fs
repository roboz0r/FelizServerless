namespace FelizServerless

open System
open System.Collections.Generic

#if FABLE_COMPILER
open Thoth.Json
open Fable.DateFunctions
#else
open Thoth.Json.Net
#endif

[<Struct>]
type JWToken = JWToken of string

type JwtError =
    | InvalidTokenParts
    | SignatureVerification of {| Expected: string; Received: string |}
    | TokenExpired of
        {| Expiration: DateTime option
           PayloadData: Map<string, obj>
           Expected: string
           Received: string |}
    | OtherJwtError of string

/// JWT Claims for Auth0
/// Refer to https://auth0.com/docs/tokens/json-web-tokens/json-web-token-claims
/// Also https://www.iana.org/assignments/jwt/jwt.xhtml#claims
type Claims2 =
    {
        Issuer: string
        Subject: string
        Expiration: DateTime
        IssuedAt: DateTime
        Scopes: Set<string>
        AuthorizedParty: string
        UniqueId: string
        Audience: string list
    }
    static member Decoder: Decoder<Claims2> =
        let unixEpoch =
#if FABLE_COMPILER
            DateTime(1970, 1, 1)
#else
            DateTime.UnixEpoch
#endif

        Decode.object
            (fun get ->
                let iss = get.Required.Field "iss" Decode.string
                let sub = get.Required.Field "sub" Decode.string
                let aud =
                    get.Optional.Field "aud"
                        (Decode.oneOf [ Decode.list Decode.string
                                        Decode.map (fun x -> [ x ]) Decode.string ])
                    |> Option.defaultValue []

                {
                    Issuer = iss
                    Subject = sub
                    Expiration =
                        unixEpoch
                        + TimeSpan.FromSeconds(float <| get.Required.Field "exp" Decode.int64)
                    IssuedAt =
                        unixEpoch
                        + TimeSpan.FromSeconds(float <| get.Required.Field "iat" Decode.int64)
                    Scopes =
                        (get.Required.Field "scope" Decode.string)
                            .Split(' ')
                        |> Set
                    AuthorizedParty = get.Required.Field "azp" Decode.string
                    UniqueId = iss + sub
                    Audience = aud
                })
    member this.HasScope scope = this.Scopes.Contains scope
