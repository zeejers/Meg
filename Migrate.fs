module Meg.Migrate

open System.IO
open Npgsql.FSharp
open Npgsql
open Meg.Providers


let runSqlScript (connectionString: string, migrationFilePath: string, provider: SqlProvider) =
    let sql = File.ReadAllText(migrationFilePath)
    let sqlContext = Meg.Providers.SqlContext.Create(provider, connectionString)
    printfn "Running Query from Script:\n %s" sql
    let queryResult = sqlContext.ExecuteNonQuery(sql)
    printfn "Query Result: %A" queryResult

let runMigrations (connectionString: string, directoryPath: string, provider: SqlProvider) =
    let sqlFiles = Directory.GetFiles(directoryPath)
    let sortedFiles = sqlFiles |> Array.sort

    sortedFiles
    |> Array.filter (fun f -> System.IO.Path.GetExtension(f).ToLower() = ".sql")
    |> Array.iter (fun migrationFilePath -> runSqlScript (connectionString, migrationFilePath, provider))
