
open Meg.Create
open Meg.Drop
open Meg.Migrate
open Meg.Config

open Argu

let [<Literal>] VERSION = "0.0.2" 
type CreateArgs =
    | [<AltCommandLine("-d")>] DbName of db_name: string
    | [<AltCommandLine("-c")>] ConnectionString of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider

    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | DbName _ -> "Specify the name of the database to create."
            | ConnectionString _-> "Specify the database connection string for the Admin database. Must be able to create DBs with the permissions of the user."
            | Provider _ -> "Specify the database provider."


and DropArgs =
    | [<AltCommandLine("-d")>] DbName of db_name: string
    | [<AltCommandLine("-c")>] ConnectionString of connection_string: string
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | DbName _ -> "Specify the name of the database to create."
            | ConnectionString _-> "Specify the database connection string for the Admin database. Must be able to create DBs with the permissions of the user."
            | Provider _ -> "Specify the database provider."



and MigrateArgs =
    | [<AltCommandLine("-c")>] ConnectionString of connection_string: string
    | [<AltCommandLine("-i")>] MigrationDirectory of migration_directory: string
    | [<AltCommandLine("-p")>] Provider of Meg.Config.SqlProvider
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | ConnectionString _ -> "Specify the database connection string for the database. Must be able to create and update tables  with the permissions of the user."
            | MigrationDirectory _-> "Specify the directory that contains your order-named migration .SQL files."
            | Provider _ -> "Specify the database provider."



and MegArgs =
    | Version
    | [<CliPrefix(CliPrefix.None)>] Create of ParseResults<CreateArgs>
    | [<CliPrefix(CliPrefix.None)>] Drop of ParseResults<DropArgs>
    | [<CliPrefix(CliPrefix.None)>] Migrate of ParseResults<MigrateArgs>
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> "Print the version."
            | Create _ -> "Create the initial database."
            | Drop _ -> "Drop the database."
            | Migrate _ -> "Run migrations in the migrations directory"



[<EntryPoint>]
let main argv =
    try
        let parser = ArgumentParser.Create<MegArgs>(programName = "meg")
        let parseResult = parser.ParseCommandLine(inputs = argv, raiseOnUsage = true)
        match parseResult.GetSubCommand() with
        | Version ->
            printfn "%s" VERSION
        | Create args ->
            let connString = args.TryGetResult(CreateArgs.ConnectionString) |> Option.defaultValue(Defaults.DB_CONNECTION_STRING)
            let dbName = args.GetResult(CreateArgs.DbName)
            let provider = args.TryGetResult(CreateArgs.Provider) |> Option.defaultValue(Defaults.DB_PROVIDER)
            create(connString, dbName, provider)
        | Drop args->
            let connString = args.TryGetResult(DropArgs.ConnectionString) |> Option.defaultValue(Defaults.DB_CONNECTION_STRING) 
            let dbName = args.GetResult(DropArgs.DbName)
            let provider = args.TryGetResult(DropArgs.Provider) |> Option.defaultValue(Defaults.DB_PROVIDER)
            drop(connString, dbName, provider)
        | Migrate args -> 
            let connString = args.TryGetResult(MigrateArgs.ConnectionString) |> Option.defaultValue(Defaults.DB_CONNECTION_STRING) 
            let provider = args.TryGetResult(MigrateArgs.Provider) |> Option.defaultValue(Defaults.DB_PROVIDER)
            let migrationsDirectory = args.TryGetResult(MigrateArgs.MigrationDirectory) |> Option.defaultValue(Defaults.MIGRATION_DIRECTORY)
            runMigrations(connString, migrationsDirectory, provider)
        0
    with ex ->
        // Prints Usage.
        printfn $"${ex.Message}"
        1
