namespace FelizServerless.Server

open System
open System.IO
open Microsoft.AspNetCore.Mvc
open Microsoft.Azure.WebJobs
open Microsoft.Azure.WebJobs.Extensions.Http
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.IdentityModel.Tokens
open System.Security.Claims
open Saturn.AzureFunctions

module Jwt =

    [<Literal>]
    let Domain = "funceng.au.auth0.com"

    [<Literal>]
    let Audience = "https://funceng.au.auth0.com/api/v2/"

    // This works for Saturn server but FunctionBuilder doesnt have all the properties of ApplicationBuilder
    // type Saturn.AzureFunctions.FunctionBuilder with
    //     [<CustomOperation("use_token_authentication")>]
    //     member __.UseTokenAuthentication(state: FunctionState) =
    //         let middleware (app: IApplicationBuilder) = app.UseAuthentication()

    //         let service (s: IServiceCollection) =
    //             s
    //                 .AddAuthentication(fun options ->
    //                     options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
    //                     options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
    //                 .AddJwtBearer(fun options ->
    //                     options.Authority <- sprintf "https://%s/" Domain
    //                     options.Audience <- Audience

    //                     options.TokenValidationParameters <-
    //                         TokenValidationParameters(NameClaimType = ClaimTypes.NameIdentifier))
    //             |> ignore

    //             s

    //         { state with
    //             ServicesConfig = service :: state.ServicesConfig
    //             AppConfigs = middleware :: state.AppConfigs
    //             CookiesAlreadyAdded = true
    //         }
