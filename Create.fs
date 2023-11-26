module Meg.Create

open Npgsql
open System
open Meg.Providers

let createPostgresql (connectionString: string, databaseName: string, provider: SqlProvider) =
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

    if not dbExists then
        printfn $"Creating DB {databaseName} because it doesn't exist yet"
        let createDbCommand = $"CREATE DATABASE \"{databaseName}\""
        sqlContext.ExecuteNonQuery(createDbCommand) |> ignore
        printfn "Database '%s' created." databaseName
    else
        printfn "Database '%s' already exists" databaseName

let create (connectionString: string, databaseName: string, provider: Meg.Providers.SqlProvider) =
    match provider with
    | SqlProvider.PostgreSQL -> createPostgresql (connectionString, databaseName, provider)
    | _ -> printfn $"Sory, {provider} is not yet implemented."
