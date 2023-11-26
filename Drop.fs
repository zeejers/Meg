module Meg.Drop

open Npgsql
open System

open Meg.Providers

let dropPostgresql (connectionString: string, databaseName: string, provider: SqlProvider) =
    let sqlContext = SqlContext.Create(provider, connectionString)
    let dbExistsQuery = $"SELECT 1 FROM pg_database WHERE datname='{databaseName}'"
    printfn $"Executing db command: {dbExistsQuery}"

    let dbExists =
        try
            match sqlContext.ExecuteQuery(dbExistsQuery) with
            | null -> false
            | result ->
                printfn "QUERY RESULT: %A" result
                true
        with ex ->
            printfn "QUERY RESULT ERR: %A" ex

            false

    if dbExists then
        printfn $"Dropping DB {databaseName}"
        let createDbCommand = $"DROP DATABASE \"{databaseName}\""
        sqlContext.ExecuteNonQuery(createDbCommand) |> ignore
        printfn "Database '%s' dropped." databaseName
    else
        printfn "Database '%s' doesn't exist" databaseName

let drop (connectionString: string, databaseName: string, provider: SqlProvider) =
    match provider with
    | SqlProvider.PostgreSQL -> dropPostgresql (connectionString, databaseName, provider)
    | _ -> printfn $"Sory, {provider} is not yet implemented."
