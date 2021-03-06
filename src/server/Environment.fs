module FelizServerless.Server.Environment

open System

let private getEnvVar key =
    Environment.GetEnvironmentVariable(key, EnvironmentVariableTarget.Process)
    |> function
    | null -> failwithf "Environment variable '%s' not found." key
    | x -> x

let PrimConnString () = getEnvVar ("FuncEng_PrimConnString")

let PrimKey () = getEnvVar ("FuncEng_PrimKey")

let SecConnString () = getEnvVar ("FuncEng_SecConnString")

let SecKey () = getEnvVar ("FuncEng_SecKey")

let DBUrl () = getEnvVar ("FunEng_DBUrl")
