module Meg.Create
open Npgsql
open System
open Meg.Config



let createPostgresql (connectionString: string, databaseName: string) =
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
         
        if not dbExists then
            printfn $"Creating DB {databaseName} because it doesn't exist yet"
            let createDbCommand = $"CREATE DATABASE \"{databaseName}\""
            let createCmd = new NpgsqlCommand(createDbCommand, conn)
            createCmd.ExecuteNonQuery() |> ignore
            printfn "Database '%s' created." databaseName
        else 
            printfn "Database '%s' already exists" databaseName
        finally
            if conn.State = Data.ConnectionState.Open then
                conn.Close()

let create (connectionString: string, databaseName: string, provider: Meg.Config.SqlProvider) =
    match provider with
    | SqlProvider.PostgreSQL ->
        createPostgresql(connectionString, databaseName)
    | _ ->
        printfn $"Sory, {provider} is not yet implemented."