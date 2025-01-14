﻿open Meg.Create
open Meg.Drop
open Meg.Migrate
open Meg.Config
open Meg.Generate

open Argu

[<Literal>]
let VERSION = "2.3.0"

type CreateArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to create. Defaults to DB_NAME."
            | Connection_String _ ->
                "Specify the admin database connection string. Must be able to create DBs with the permissions of the user. Defaults to DB_CONNECTION_STRING."
            | Provider _ -> "Specify the database provider."


and DropArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to create. Defaults to DB_NAME."
            | Connection_String _ ->
                "Specify the admin database connection string. Must be able to create DBs with the permissions of the user. Defaults to DB_CONNECTION_STRING."
            | Provider _ -> "Specify the database provider. Defaults to DB_PROVIDER."

and ResetArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to reset. Defaults to DB_NAME."
            | Connection_String _ ->
                "Specify the admin database connection string. Must be able to create DBs with the permissions of the user. Defaults to DB_CONNECTION_STRING."
            | Provider _ -> "Specify the database provider."

and MigrateArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-i")>] Migration_Directory of migration_directory: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Connection_String _ ->
                "Specify the admin database connection string. Must be able to create and update tables with the permissions of the user. Defaults to DB_CONNECTION_STRING."
            | Migration_Directory _ ->
                "Specify the directory that contains your order-named migration .SQL files. Defaults to DB_MIGRATION_DIRECTORY."
            | Provider _ -> "Specify the database provider. Defaults to DB_PROVIDER."
            | Db_Name _ -> "Sepcify the database name. Defaults to DB_NAME."

// Example: meg gen migration add_users_table Users Name:String Id:Serial:Key
and GenMigrationArgs =
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider
    | [<AltCommandLine("-o")>] Migration_Directory of migration_directory: string
    | [<MainCommand>] Schema_Definition of string list

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Provider _ -> "Specify the database provider. Defaults to DB_PROVIDER."
            | Schema_Definition _ ->
                // let p = System.Environment.GetEnvironmentVariable("DB_PROVIDER")

                let provider = Defaults.DB_PROVIDER

                let schemaHelp = schemaDefintionCliUsageDescriptions (provider)

                $"Specify the schema definition of the migration in this format:\n-----------\nmeg gen migration AddUsersTable users Id:Id Name:String Description:Text\n-----------\nList of all schema types for {provider} (see your provider by setting DB_PROVIDER) :\n{schemaHelp}"
            | Migration_Directory _ ->
                "The output directory to write migrations to. When not specified, the resulting SQL is written to env var DB_MIGRATION_DIRECTORY value, defaults to 'Migrations'"

and GenArgs =
    | [<CliPrefix(CliPrefix.None)>] Migration of ParseResults<GenMigrationArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Migration _ -> "Generate a new migration providing a schema."

and MegArgs =
    | [<AltCommandLine("-v")>] Version
    | [<CliPrefix(CliPrefix.None)>] Env
    | [<CliPrefix(CliPrefix.None)>] Create of ParseResults<CreateArgs>
    | [<CliPrefix(CliPrefix.None)>] Drop of ParseResults<DropArgs>
    | [<CliPrefix(CliPrefix.None)>] Reset of ParseResults<ResetArgs>
    | [<CliPrefix(CliPrefix.None)>] Migrate of ParseResults<MigrateArgs>
    | [<CliPrefix(CliPrefix.None)>] Gen of ParseResults<GenArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Print the version."
            | Env -> "Print the meg environment."
            | Create _ -> "Create the initial database."
            | Drop _ -> "Drop the database."
            | Reset _ -> "Drop and recreate the database."
            | Migrate _ -> "Run migrations in the migrations directory"
            | Gen _ -> "Generate"

let runProgram (parseResult: ParseResults<MegArgs>) =
    match parseResult.GetSubCommand() with
    | Create args ->
        let connString =
            args.TryGetResult(CreateArgs.Connection_String)
            |> Option.defaultValue (Defaults.DB_CONNECTION_STRING)

        let dbName =
            match args.TryGetResult(CreateArgs.Db_Name) with
            | Some value -> value
            | None ->
                match Defaults.DB_NAME with
                | None -> failwith "DB_NAME cannot be None if -d is not specified."
                | Some value -> value

        let provider =
            args.TryGetResult(CreateArgs.Provider)
            |> Option.defaultValue (Defaults.DB_PROVIDER)

        create (connString, dbName, provider)
    | Drop args ->
        let connString =
            args.TryGetResult(DropArgs.Connection_String)
            |> Option.defaultValue (Defaults.DB_CONNECTION_STRING)

        let dbName =
            match args.TryGetResult(DropArgs.Db_Name) with
            | Some value -> value
            | None ->
                match Defaults.DB_NAME with
                | None -> failwith "DB_NAME cannot be None if -d is not specified."
                | Some value -> value

        let provider =
            args.TryGetResult(DropArgs.Provider)
            |> Option.defaultValue (Defaults.DB_PROVIDER)

        drop (connString, dbName, provider)
    | Reset args ->
        let connString =
            args.TryGetResult(ResetArgs.Connection_String)
            |> Option.defaultValue (Defaults.DB_CONNECTION_STRING)

        let dbName =
            match args.TryGetResult(ResetArgs.Db_Name) with
            | Some value -> value
            | None ->
                match Defaults.DB_NAME with
                | None -> failwith "DB_NAME cannot be None if -d is not specified."
                | Some value -> value

        let provider =
            args.TryGetResult(ResetArgs.Provider)
            |> Option.defaultValue (Defaults.DB_PROVIDER)

        drop (connString, dbName, provider)
        create (connString, dbName, provider)
    | Migrate args ->
        let connString =
            args.TryGetResult(MigrateArgs.Connection_String)
            |> Option.defaultValue (Defaults.DB_CONNECTION_STRING)

        let provider =
            args.TryGetResult(MigrateArgs.Provider)
            |> Option.defaultValue (Defaults.DB_PROVIDER)

        let migrationsDirectory =
            args.TryGetResult(MigrateArgs.Migration_Directory)
            |> Option.defaultValue (Defaults.DB_MIGRATION_DIRECTORY)

        let dbName =
            match args.TryGetResult(MigrateArgs.Db_Name) with
            | Some value -> value
            | None ->
                match Defaults.DB_NAME with
                | None -> failwith "DB_NAME cannot be None if -d is not specified."
                | Some value -> value

        runMigrations (connString, migrationsDirectory, provider, dbName)
    | Gen args ->
        match args.GetSubCommand() with
        | Migration args ->

            let provider =
                args.TryGetResult(GenMigrationArgs.Provider)
                |> Option.defaultValue (Defaults.DB_PROVIDER)

            let outputDir =
                args.TryGetResult(GenMigrationArgs.Migration_Directory)
                |> Option.defaultValue (Defaults.DB_MIGRATION_DIRECTORY)

            match args.GetResult(GenMigrationArgs.Schema_Definition) with
            | schemaName :: tableName :: fields -> genMigrations schemaName tableName provider fields outputDir
            // genMigrations (migrationName, tableName, provider, schemaDefinitions)
            | _ -> failwith "You must provide a migration name, table name, and field definitions."
    | _ -> failwith "Invalid command provided."



[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<MegArgs>(programName = "meg")
        let parseResult = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

        match parseResult.GetAllResults() with
        | [ Version ] ->
            printfn "%s" VERSION
            ()
        | [ Env ] ->
            printfn
                "DB_CONNECTION_STRING=\"%s\"\nDB_PROVIDER=%s\nDB_MIGRATION_DIRECTORY=%s\nDB_NAME=%s"
                Defaults.DB_CONNECTION_STRING
                (Defaults.DB_PROVIDER |> string)
                Defaults.DB_MIGRATION_DIRECTORY
                (Defaults.DB_NAME |> Option.defaultValue "")
        | _ -> runProgram (parseResult)




        0
    with ex ->
        // Prints Usage.
        printfn $"${ex.Message}"
        1
