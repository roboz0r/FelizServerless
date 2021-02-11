namespace FelizServerless

open System
open System.Collections.Generic

#if FABLE_COMPILER
open Fable.DateFunctions
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
type Claims(claims: IDictionary<string, obj>) =
    let unixEpoch = 
#if FABLE_COMPILER 
        DateTime(1970,1,1)
#else
        DateTime.UnixEpoch 
#endif   
    let aud =
        (claims.["aud"] :?> string).Split(',') |> List

    let azp = claims.["azp"] :?> string

    let exp = 

        unixEpoch + TimeSpan.FromSeconds(claims.["exp"] :?> int64 |> float)

    let iat = 
        unixEpoch + TimeSpan.FromSeconds(claims.["iat"] :?> int64 |> float)

    let iss = claims.["iss"] :?> string

    let scopes =
        (claims.["scope"] :?> string).Split(' ') |> Set

    let sub = claims.["sub"] :?> string

    member __.Item key = claims.[key]
    member __.TryGetValue key = claims.TryGetValue key
    member __.Issuer = iss
    member __.Subject = sub
    member __.Audience = aud
    member __.Expiration = exp
    member __.IssuedAt = iat
    member __.Scopes = scopes
    member __.HasScope scope = scopes.Contains scope
    member __.AuthorizedParty = azp