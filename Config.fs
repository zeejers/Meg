module Meg.Config

open Meg.Providers
open System


type MegConfig =
    { DB_MIGRATION_CONNECTION_STRING: string
      DB_INITIAL_CONNECTION_STRING: string
      DB_PROVIDER: SqlProvider
      MIGRATION_DIRECTORY: string }

let parseSqlProvider (providerName: string) =
    match providerName.ToUpper() with
    | "POSTGRESQL" -> SqlProvider.PostgreSQL
    | "MSSQL" -> SqlProvider.MSSQL
    | "MYSQL" -> SqlProvider.MySql
    | "SQLITE" -> SqlProvider.SQLite
    | _ -> failwith $"Unsupported SQL Provider: {providerName}"

let DB_INITIAL_CONNECTION_STRING =
    "Server=localhost; Port=54322; Database=postgres; User Id=postgres; Password=postgres;"

let DB_MIGRATION_CONNECTION_STRING =
    "Server=localhost; Port=54322; Database=postgres; User Id=postgres; Password=postgres;"

let Defaults =
    { DB_MIGRATION_CONNECTION_STRING =
        System.Environment.GetEnvironmentVariable("DB_MIGRATION_CONNECTION_STRING")
        |> Option.ofObj
        |> Option.defaultValue (DB_MIGRATION_CONNECTION_STRING)
      DB_INITIAL_CONNECTION_STRING =
        System.Environment.GetEnvironmentVariable("DB_INITIAL_CONNECTION_STRING")
        |> Option.ofObj
        |> Option.defaultValue (DB_INITIAL_CONNECTION_STRING)
      DB_PROVIDER =
        System.Environment.GetEnvironmentVariable("DB_PROVIDER")
        |> Option.ofObj
        |> Option.map (fun p -> parseSqlProvider (p))
        |> Option.defaultValue (SqlProvider.PostgreSQL)
      MIGRATION_DIRECTORY =
        System.Environment.GetEnvironmentVariable("MIGRATION_DIRECTORY")
        |> Option.ofObj
        |> Option.defaultValue ("Migrations") }
