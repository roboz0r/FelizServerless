namespace FelizServerless.Server

open FelizServerless
open System
open System.Collections.Generic
open System.Security.Cryptography
open JWT
open JWT.Algorithms
open JWT.Builder
open JWT.Exceptions
open Microsoft.AspNetCore.Http

module FuncEngJwt =

    [<Literal>]
    let Domain = "funceng.au.auth0.com"

    [<Literal>]
    let Audience = "https://funceng.au.auth0.com/api/v2/"

    [<Literal>]
    let Jwks =
        "https://funceng.au.auth0.com/.well-known/jwks.json"

    //  TODO Consider getting this from the above url as required
    /// https://tools.ietf.org/html/rfc7517
    /// https://auth0.com/docs/tokens/json-web-tokens/json-web-key-set-properties
    let publicKeys =
        [|
            {|
                Alg = "RS256"
                Kty = "RSA"
                Use = "sig"
                N =
                    "sgKsTFHIkmIWAewrZVN2BxcSDkkE-9v7O8fqSlfWqkpQaKs2O4K5hz9g7r58F6jb0TrRdrSj1lntvr9pnpM9R2bh5CLK4IrKvMR_x6YsAliHHhstQQWTi1kKSkilF9eGl8pObq4D-w5yvDotKobdyC4LNGqkq8VQeT-Bw12ib6CzTTlt3jiqmd0fAz8qADXgUl1s_IPK0bJDDppywWe1duMfSxgUYDGjN-mV1FXwIgyZhEgQnxm_3W3H8QJ_TJAKbmRB9qKb8YkDcTtwIdbowOCq0wC0jpPUKqn2dIYBXLEScYFEd4itndM7GomaPp2ww5c7zOOQ2nfjwOiCmwJnnw"
                E = "AQAB"
                Kid = "YLX2S9mcNGVkSWlfTflhG"
                X5t = "ZJdY63fWuPLFii9JO4KfGrcJcDc"
                X5c =
                    [
                        "MIIDAzCCAeugAwIBAgIJYnGc/nvwR1GJMA0GCSqGSIb3DQEBCwUAMB8xHTAbBgNVBAMTFGZ1bmNlbmcuYXUuYXV0aDAuY29tMB4XDTIxMDExNjA3MTQxNFoXDTM0MDkyNTA3MTQxNFowHzEdMBsGA1UEAxMUZnVuY2VuZy5hdS5hdXRoMC5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCyAqxMUciSYhYB7CtlU3YHFxIOSQT72/s7x+pKV9aqSlBoqzY7grmHP2DuvnwXqNvROtF2tKPWWe2+v2mekz1HZuHkIsrgisq8xH/HpiwCWIceGy1BBZOLWQpKSKUX14aXyk5urgP7DnK8Oi0qht3ILgs0aqSrxVB5P4HDXaJvoLNNOW3eOKqZ3R8DPyoANeBSXWz8g8rRskMOmnLBZ7V24x9LGBRgMaM36ZXUVfAiDJmESBCfGb/dbcfxAn9MkApuZEH2opvxiQNxO3Ah1ujA4KrTALSOk9QqqfZ0hgFcsRJxgUR3iK2d0zsaiZo+nbDDlzvM45Dad+PA6IKbAmefAgMBAAGjQjBAMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFJf3wdwqhZgUPx+Nti8sihefPjF6MA4GA1UdDwEB/wQEAwIChDANBgkqhkiG9w0BAQsFAAOCAQEAAjuQ2GpXbJWjS/HXsz9xtk5T8pLoXPw+GZo8UE9fuxotrzHnZpkiGogF73NPq7Ofll7DnVLkEx1xogqaY0Svbj/ble3VunSuW+mFdc9ZlpyAMutZBwQ6Sag0MFzSGVqTPrjVRscPAIAZc9vIvNCUvgjAbiHw928GP5CkmXbK7CDyLaXIT6Sug97HHJq1Q8tSN+kaXVv3adM2pIWASOIjFj4wpWQYuAmpwudQBEPXJQD98+XAXKxysFoNcBNilHl9cbvT1MlvUquCL/AmvjSJ1ixJGVeMipWyORB0PpuD81SbV89pKs/xVY8DeHgb6viVquchWIXHK0Kx/PfjlX1cWw=="
                    ]
            |}
            {|
                Alg = "RS256"
                Kty = "RSA"
                Use = "sig"
                N =
                    "tP-4jLQ_aMCMFMa3yqTkz9uZ2ttOVOlnhzqLEuhV42K4AHfkQP5f3lLY_yWka2lZZMU2dCA_t7bUBgBOzeNHCxHgij66SAjN4wijQfAc425k2aNrtpGJ3JfTJyp4x1Z_Dbh1Zn0FcFSEKV4Ba1QTavL7GwYg1gUYPAyRmFKKPpkUMIpE1IVOs91O15Mvf3qaF-cucdyrr0GER-QJTgkC9YbMDOYe8JjNZ9kiXd778Y3K8nwOAE3_Ek3DfbS4bc-19eQekafw63KBmdSLnJ8qAXGRfOtvqq5ICH0gAas8on49VU74d2ZB3Bj_peT2sRP4r1sQVnlMvtppEOfaWFDNew"
                E = "AQAB"
                Kid = "fghQYfeNjInKP1-LPQnfh"
                X5t = "c-R_gW1KalgLbCvX7s-ahWu-ijA"
                X5c =
                    [
                        "MIIDAzCCAeugAwIBAgIJDe70Fhp8YFqiMA0GCSqGSIb3DQEBCwUAMB8xHTAbBgNVBAMTFGZ1bmNlbmcuYXUuYXV0aDAuY29tMB4XDTIxMDExNjA3MTQxNFoXDTM0MDkyNTA3MTQxNFowHzEdMBsGA1UEAxMUZnVuY2VuZy5hdS5hdXRoMC5jb20wggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQC0/7iMtD9owIwUxrfKpOTP25na205U6WeHOosS6FXjYrgAd+RA/l/eUtj/JaRraVlkxTZ0ID+3ttQGAE7N40cLEeCKPrpICM3jCKNB8BzjbmTZo2u2kYncl9MnKnjHVn8NuHVmfQVwVIQpXgFrVBNq8vsbBiDWBRg8DJGYUoo+mRQwikTUhU6z3U7Xky9/epoX5y5x3KuvQYRH5AlOCQL1hswM5h7wmM1n2SJd3vvxjcryfA4ATf8STcN9tLhtz7X15B6Rp/DrcoGZ1IucnyoBcZF862+qrkgIfSABqzyifj1VTvh3ZkHcGP+l5PaxE/ivWxBWeUy+2mkQ59pYUM17AgMBAAGjQjBAMA8GA1UdEwEB/wQFMAMBAf8wHQYDVR0OBBYEFHFghBsNBd7nZqp3ca+N5v9o6cViMA4GA1UdDwEB/wQEAwIChDANBgkqhkiG9w0BAQsFAAOCAQEAAXzxAJveH4HThzuLzHgKJmBoYEaELk5U2AvwqODzOJrKa9PNMs4zmB1lZvj/wfWcm1vshB1TjWiF8AKBTLfDXt6O6tJKSwdM/E5V7Hu20YURz+VP8LFosWr2e2raDTdOvXLim4S0pBYOZf+33hwEJy887LOsoKNMsEOuL8gNvsV2l21tKgC3WpSTvdekkub+KZn/7SThyTlBXXBmDQagvi0DOHLA9dD0XaZWZitWCEIus+xp8t1k8wg86wgIcMzQhvE9C6sZzO78WVEn7q+wE0cgUp9tF+u4jpzoGlKIEbfnX5pNCoHP2vBRRj7uf4esRf7X3z4exBK4Bi4FQZ3O6g=="
                    ]
            |}
        |]

    let tokenFromCtx (ctx:HttpContext) = 
        match ctx.Request.Headers.TryGetValue "Authorization" with
        | true, x ->
            match x.Count with
            | 1 ->
                let x = x.[0]

                if x.StartsWith("Bearer ") then
                    Ok(x.[7..])
                else
                    Error(OtherJwtError "Authorization header must be of the format Authorization : Bearer <token>")
            | _ -> Error(OtherJwtError "More than one Authorization Header supplied.")
        | false, _ -> Error(OtherJwtError "Header must contain value with the format Authorization : Bearer <token>")

    /// Validates a JWT
    /// Refer to https://stackoverflow.com/a/64920468/14134059
    /// Returns the decoded claims or a decoding error
    let validateToken token =
        let urlEncoder = JwtBase64UrlEncoder()
        let rsaKey = RSA.Create()

        let rsa =
            RSAParameters(Modulus = urlEncoder.Decode(publicKeys.[0].N), Exponent = urlEncoder.Decode(publicKeys.[0].E))

        rsaKey.ImportParameters(rsa)

        try
            let claims =
                JwtBuilder()
                    .WithAlgorithm(RS256Algorithm(rsaKey))
                    .Audience(Audience)
                    .MustVerifySignature()
                    // .Decode(token)
                    .Decode<IDictionary<string, obj>>(token)

            Console.WriteLine(claims)

            claims :> seq<_>
            |> Seq.map (|KeyValue|)
            |> Map
            |> Ok
        with
        | :? InvalidTokenPartsException -> Error InvalidTokenParts
        | :? TokenExpiredException as ex ->
            Error(
                TokenExpired
                    {|
                        Expiration = Option.ofNullable ex.Expiration
                        PayloadData =
                            ex.PayloadData :> seq<_>
                            |> Seq.map (|KeyValue|)
                            |> Map
                        Expected = ex.Expected
                        Received = ex.Received
                    |}
            )
        | :? SignatureVerificationException as ex ->
            Error(
                SignatureVerification
                    {|
                        Expected = ex.Expected
                        Received = ex.Received
                    |}
            )
        | ex -> Error(OtherJwtError $"Exception thrown: {ex.Message}")
