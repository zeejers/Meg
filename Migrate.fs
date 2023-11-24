module Meg.Migrate

open FSharp.Data.Sql
open System.IO
open Npgsql.FSharp
open Npgsql
open Meg.Config


let runSqlScriptPostgresql (connectionString: string, filePath: string) = 
    let sql = File.ReadAllText(filePath)
    use connection = new NpgsqlConnection(connectionString)
    connection.Open()
    printfn "Running Query from Script:\n %s" sql
    use command = new NpgsqlCommand(sql, connection)
    let queryResult = command.ExecuteNonQuery()
    printfn "Query Result: %i" queryResult

let runMigrations (connectionString: string, directoryPath: string, provider: SqlProvider) =
    let sqlFiles = Directory.GetFiles(directoryPath)
    let sortedFiles = sqlFiles |> Array.sort

    sortedFiles 
    |> Array.filter(fun f -> System.IO.Path.GetExtension(f).ToLower() = ".sql")
    |> Array.iter(fun f -> 
        match provider with
        | SqlProvider.PostgreSQL ->
            runSqlScriptPostgresql(connectionString, f)
        | _ ->
            printfn $"Sorry, provider {provider} is not yet implemented."
        ) 

