module Meg.Migrate

open System.IO
open Npgsql.FSharp
open Npgsql
open Meg.Providers

let runSqlScript (sqlContext: SqlContext, migrationFilePath: string) =
    let sql = File.ReadAllText(migrationFilePath)
    let migrationVersion = System.IO.Path.GetFileNameWithoutExtension(migrationFilePath)
    printfn "Running migration '%s':\n %s" migrationVersion sql
    // let existingMigrationEntry = sqlContext.
    let queryResult = sqlContext.ExecuteNonQuery(sql)
    let timestamp = System.DateTime.UtcNow.ToString("o")

    let insertSchemaMigrationResult =
        sqlContext
            .Table<SchemaMigration>(sqlContext.migrationsTableName)
            .Insert(
                { inserted_at = timestamp
                  version = migrationVersion },
                sqlContext.GetConnection()
            )
        |> Async.AwaitTask
        |> Async.RunSynchronously

    ()

let runMigrations (connectionString: string, directoryPath: string, provider: SqlProvider) =
    let sqlFiles = Directory.GetFiles(directoryPath)
    let sqlContext = Meg.Providers.SqlContext.Create(provider, connectionString)

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
        |> Array.iter (fun migrationFilePath -> runSqlScript (sqlContext, migrationFilePath))
