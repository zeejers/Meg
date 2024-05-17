open Meg.Create
open Meg.Drop
open Meg.Migrate
open Meg.Config
open Meg.Generate

open Argu

[<Literal>]
let VERSION = "2.0.0"

type CreateArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to create."
            | Connection_String _ ->
                "Specify the database connection string for the Admin database. Must be able to create DBs with the permissions of the user."
            | Provider _ -> "Specify the database provider."


and DropArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to create."
            | Connection_String _ ->
                "Specify the database connection string for the Admin database. Must be able to create DBs with the permissions of the user."
            | Provider _ -> "Specify the database provider."

and ResetArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to reset."
            | Connection_String _ ->
                "Specify the database connection string for the Admin database. Must be able to create DBs with the permissions of the user."
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
                "Specify the database connection string for the database. Must be able to create and update tables  with the permissions of the user."
            | Migration_Directory _ -> "Specify the directory that contains your order-named migration .SQL files."
            | Provider _ -> "Specify the database provider."
            | Db_Name _ -> "Sepcify the database name."

// Example: meg gen migration add_users_table Users Name:String Id:Serial:Key
and GenMigrationArgs =
    | [<AltCommandLine("-p")>] Provider of Meg.Providers.SqlProvider
    | [<MainCommand>] Schema_Definition of string list

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Provider _ -> "Specify the database provider."
            | Schema_Definition _ ->
                "Specify the schema definition of the migration. Format MigrationName TableName Id:Serial:Pk Name:String Description:Text"

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

        let dbName = args.GetResult(CreateArgs.Db_Name)

        let provider =
            args.TryGetResult(CreateArgs.Provider)
            |> Option.defaultValue (Defaults.DB_PROVIDER)

        create (connString, dbName, provider)
    | Drop args ->
        let connString =
            args.TryGetResult(DropArgs.Connection_String)
            |> Option.defaultValue (Defaults.DB_CONNECTION_STRING)

        let dbName = args.GetResult(DropArgs.Db_Name)

        let provider =
            args.TryGetResult(DropArgs.Provider)
            |> Option.defaultValue (Defaults.DB_PROVIDER)

        drop (connString, dbName, provider)
    | Reset args ->
        let connString =
            args.TryGetResult(ResetArgs.Connection_String)
            |> Option.defaultValue (Defaults.DB_CONNECTION_STRING)

        let dbName = args.GetResult(ResetArgs.Db_Name)

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
            |> Option.defaultValue (Defaults.MIGRATION_DIRECTORY)

        let dbName = args.GetResult(MigrateArgs.Db_Name)

        runMigrations (connString, migrationsDirectory, provider, dbName)
    | Gen args ->
        match args.GetSubCommand() with
        | Migration args ->
            printfn "Generating Schema Definition"

            let provider =
                args.TryGetResult(GenMigrationArgs.Provider)
                |> Option.defaultValue (Defaults.DB_PROVIDER)

            match args.GetResult(GenMigrationArgs.Schema_Definition) with
            | schemaName :: tableName :: fields ->
                printfn "Gen Migration Command: Schema: %A Table: %A definitions: %A" schemaName tableName fields
            // genMigrations (migrationName, tableName, provider, schemaDefinitions)
            | _ -> failwith "You must provide a schema name, table, and field definitions."
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
                "DB_CONNECTION_STRING=\"%s\"\nDB_PROVIDER=%s\nMIGRATION_DIRECTORY=%s"
                Defaults.DB_CONNECTION_STRING
                (Defaults.DB_PROVIDER |> string)
                Defaults.MIGRATION_DIRECTORY
        | _ -> runProgram (parseResult)




        0
    with ex ->
        // Prints Usage.
        printfn $"${ex.Message}"
        1
