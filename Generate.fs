module Meg.Generate

open Meg.Config

let genMigrations
    (
        migrationName: string,
        tableName: string,
        provider: SqlProvider,
        schemaDefinitions: string list option
    ) =
    printfn "Migration: %s" migrationName
    printfn "Table: %s" tableName
    printfn "Provider: %A" provider

    let schemaDefinitions =
        match schemaDefinitions with
        | Some schemDefinitions -> schemDefinitions |> String.concat (", ")
        | None -> ""

    printfn "Schema: %s" schemaDefinitions
