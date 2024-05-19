module Meg.Config

open Meg.Providers

type MegConfig =
    { DB_CONNECTION_STRING: string
      DB_PROVIDER: SqlProvider
      MIGRATION_DIRECTORY: string }


let DB_CONNECTION_STRING =
    "Server=localhost; Port=54322; Database=postgres; User Id=postgres; Password=postgres;"

let Defaults =
    { DB_CONNECTION_STRING =
        System.Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
        |> Option.ofObj
        |> Option.defaultValue (DB_CONNECTION_STRING)
      DB_PROVIDER =
        System.Environment.GetEnvironmentVariable("DB_PROVIDER")
        |> Option.ofObj
        |> Option.map (fun p -> Providers.parseProvider (p))
        |> Option.defaultValue (SqlProvider.PostgreSQL)
      MIGRATION_DIRECTORY =
        System.Environment.GetEnvironmentVariable("MIGRATION_DIRECTORY")
        |> Option.ofObj
        |> Option.defaultValue ("Migrations") }
