module Meg.Config
open System

type SqlProvider = 
    | PostgreSQL
    | MSSQL
    | MySql
    | SQLite


type MegConfig = {
    DB_CONNECTION_STRING: string;
    DB_NAME: string;
    DB_PROVIDER: SqlProvider
    MIGRATION_DIRECTORY: string;
}

let parseSqlProvider (providerName: string) =
    match providerName.ToUpper() with
    | "POSTGRESQL" -> PostgreSQL
    | "MSSQL" -> MSSQL
    | "MYSQL" -> MySql
    | "SQLITE" -> SQLite
    | _ -> failwith $"Unsupported SQL Provider: {providerName}"

let DB_CONNECTION_STRING = "Server=localhost; Port=54322; Database=postgres; User Id=postgres; Password=postgres;"

let Defaults = {
    DB_CONNECTION_STRING = System.Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") |> Option.ofObj |> Option.defaultValue(DB_CONNECTION_STRING)
    DB_NAME = System.Environment.GetEnvironmentVariable("DB_NAME") |> Option.ofObj |> Option.defaultValue("")
    DB_PROVIDER = System.Environment.GetEnvironmentVariable("DB_PROVIDER") |> Option.ofObj |> Option.map(fun p -> parseSqlProvider(p)) |> Option.defaultValue(PostgreSQL)
    MIGRATION_DIRECTORY = System.Environment.GetEnvironmentVariable("MIGRATION_DIRECTORY") |> Option.ofObj |> Option.defaultValue("Migrations")
}    