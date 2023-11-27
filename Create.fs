module Meg.Create

open Npgsql
open System
open Meg.Providers

let create (connectionString: string, databaseName: string, provider: Meg.Providers.SqlProvider) =
    let sqlContext = SqlContext.Create(provider, connectionString)
    let dbExistsQuery = sqlContext.GetDatabaseExistsQuery(databaseName)
    printfn $"Executing db command: {dbExistsQuery}"

    let dbExists =
        try
            match sqlContext.ExecuteScalar(dbExistsQuery) with
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
