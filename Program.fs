open Meg.Create
open Meg.Drop
open Meg.Migrate
open Meg.Config

open Argu

[<Literal>]
let VERSION = "0.0.2"

type CreateArgs =
    | [<AltCommandLine("-d")>] Db_Name of db_name: string
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider

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
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Db_Name _ -> "Specify the name of the database to create."
            | Connection_String _ ->
                "Specify the database connection string for the Admin database. Must be able to create DBs with the permissions of the user."
            | Provider _ -> "Specify the database provider."



and MigrateArgs =
    | [<AltCommandLine("-c")>] Connection_String of connection_string: string
    | [<AltCommandLine("-i")>] Migration_Directory of migration_directory: string
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Connection_String _ ->
                "Specify the database connection string for the database. Must be able to create and update tables  with the permissions of the user."
            | Migration_Directory _ -> "Specify the directory that contains your order-named migration .SQL files."
            | Provider _ -> "Specify the database provider."

and GenMigrationArgs =
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider
    | [<CliPrefix(CliPrefix.None)>] Migration_Name
    | [<CliPrefix(CliPrefix.None)>] Table_Name
    | Schema_Definitions of string list

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Provider _ -> "Specify the database provider."
            | Migration_Name -> "Specify the name of the migration. Will be prefixed by current datetime."
            | Table_Name -> "Specify the name of the table."
            | Schema_Definitions _ ->
                "Definition of schema. Example... Id:serial:pk name:string description:text hobbies:jsonb"

and GenArgs =
    | [<CliPrefix(CliPrefix.None)>] Migration of ParseResults<GenMigrationArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Migration _ -> "Generate a new migration providing a schema."



and MegArgs =
    | Version
    | [<CliPrefix(CliPrefix.None)>] Create of ParseResults<CreateArgs>
    | [<CliPrefix(CliPrefix.None)>] Drop of ParseResults<DropArgs>
    | [<CliPrefix(CliPrefix.None)>] Migrate of ParseResults<MigrateArgs>
    | [<CliPrefix(CliPrefix.None)>] Gen of ParseResults<GenArgs>

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Print the version."
            | Create _ -> "Create the initial database."
            | Drop _ -> "Drop the database."
            | Migrate _ -> "Run migrations in the migrations directory"
            | Gen _ -> "Generate"





[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<MegArgs>(programName = "meg")
        let parseResult = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)

        match parseResult.GetSubCommand() with
        | Version -> printfn "%s" VERSION
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

            runMigrations (connString, migrationsDirectory, provider)
        | Gen args -> printfn "Generating schema definition %A" args

        0
    with ex ->
        // Prints Usage.
        printfn $"${ex.Message}"
        1
