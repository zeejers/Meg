module Meg.Drop
open Npgsql
open System

open Meg.Config

let dropPostgresql (connectionString: string, databaseName: string) =
    use conn = new NpgsqlConnection(connectionString)
    try
        conn.Open()
        let dbExistsQuery = $"SELECT 1 FROM pg_database WHERE datname='{databaseName}'"
        printfn $"Executing db command: {dbExistsQuery}"
        use cmd = new NpgsqlCommand(dbExistsQuery, conn)
        let dbExists =
            try
                match cmd.ExecuteScalar() with
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
            let createCmd = new NpgsqlCommand(createDbCommand, conn)
            createCmd.ExecuteNonQuery() |> ignore
            printfn "Database '%s' dropped." databaseName
        else 
            printfn "Database '%s' doesn't exist" databaseName
        finally
            if conn.State = Data.ConnectionState.Open then
                conn.Close()

let drop (connectionString: string, databaseName: string, provider: SqlProvider) = 
    match provider with
    | SqlProvider.PostgreSQL ->
        dropPostgresql(connectionString, databaseName)
    | _ ->
        printfn $"Sory, {provider} is not yet implemented."