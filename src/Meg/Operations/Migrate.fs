module Meg.Migrate

open System.IO
open Npgsql.FSharp
open Npgsql
open Meg.Providers
open System.Text.RegularExpressions

let runSqlScript (sqlContext: SqlContext, sql: string, version: string option) =
    let queryResult = sqlContext.ExecuteNonQuery(sql)
    let timestamp = System.DateTime.UtcNow.ToString("o")

    let version =
        match version with
        | Some version -> version
        | None -> ""

    let insertSchemaMigrationResult =
        sqlContext
            .Table<SchemaMigration>(sqlContext.migrationsTableName)
            .Insert(
                { inserted_at = timestamp
                  version = version },
                sqlContext.GetConnection()
            )
        |> Async.AwaitTask
        |> Async.RunSynchronously

    ()

let readAndRunSqlScript (sqlContext: SqlContext, migrationFilePath: string) =
    let sql = File.ReadAllText(migrationFilePath)
    let migrationVersion = System.IO.Path.GetFileNameWithoutExtension(migrationFilePath)
    printfn "Running migration '%s':\n %s" migrationVersion sql
    // let existingMigrationEntry = sqlContext.
    runSqlScript (sqlContext, sql, Some migrationVersion)

    ()

let getTableConnectionString (connectionString: string, dbName: string) =
    // "Server=localhost; Port=54322; Database=postgres; User Id=postgres; Password=postgres;"
    let regex = new Regex("(Database=)[^;]+", RegexOptions.IgnoreCase)
    regex.Replace(connectionString, $"$1{dbName}")

let runMigrations (connectionString: string, directoryPath: string, provider: SqlProvider, dbName: string) =
    let sqlFiles = Directory.GetFiles(directoryPath)
    let connString = getTableConnectionString (connectionString, dbName)
    let sqlContext = Meg.Providers.SqlContext.Create(provider, connString)

    let createMigrationsTableCmd = sqlContext.CreateMigrationsTableCommand()

    let createMigrationsTableResult =
        sqlContext.ExecuteNonQuery(createMigrationsTableCmd)

    let sortedFiles = sqlFiles |> Array.sort

    let migrationsTable =
        sqlContext.Table<SchemaMigration>(sqlContext.migrationsTableName)

    let res =
        migrationsTable.Select(sqlContext.GetConnection())
        |> Async.AwaitTask
        |> Async.RunSynchronously

    let filteredMigrations =
        sortedFiles
        |> Array.filter (fun f ->
            let isSql = System.IO.Path.GetExtension(f).ToLower() = ".sql"
            let migrationVersion = System.IO.Path.GetFileNameWithoutExtension(f)

            let hasMigrated =
                res
                |> Seq.exists
                    (fun
                        { version = version
                          inserted_at = insertedAt } -> version = migrationVersion)

            isSql = true && not hasMigrated)

    if filteredMigrations.Length < 1 then
        printfn "Migrations up to date!"
    else
        filteredMigrations
        |> Array.iter (fun migrationFilePath -> readAndRunSqlScript (sqlContext, migrationFilePath))
