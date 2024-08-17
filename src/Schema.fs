module Meg.Schema

open Meg.Providers

type FieldType =
    | Id
    | Guid
    | Integer
    | Float
    | Numeric
    | Boolean
    | String
    | Text
    | Binary
    | Array
    | Record
    | Date
    | Time
    | DateTime
    | DateTimeTz

type FieldConstraint =
    | PrimaryKey
    | NotNull
    | Unique

type Column =
    { Name: string
      Type: FieldType
      Constraints: FieldConstraint list }

type Table = { Name: string; Columns: Column list }

let getSchemaMigrationValue (fieldType: FieldType) (provider: SqlProvider) =
    match provider with
    | SqlProvider.PostgreSQL ->
        match fieldType with
        | FieldType.Id -> "INTEGER"
        | FieldType.Guid -> "UUID"
        | FieldType.Integer -> "INTEGER"
        | FieldType.Float -> "FLOAT"
        | FieldType.Numeric -> "NUMERIC"
        | FieldType.Boolean -> "BOOLEAN"
        | FieldType.String -> "VARCHAR(255)"
        | FieldType.Text -> "TEXT"
        | FieldType.Binary -> "BYTEA"
        | FieldType.Array -> "JSONB"
        | FieldType.Record -> "JSONB"
        | FieldType.Date -> "DATE"
        | FieldType.Time -> "TIME"
        | FieldType.DateTime -> "TIMESTAMP"
        | FieldType.DateTimeTz -> "TIMESTAMPTZ"
    // ... other cases for Postgres
    | SqlProvider.MySql ->
        match fieldType with
        | FieldType.Id -> "INTEGER"
        | FieldType.Guid -> "CHAR(36)"
        | FieldType.Integer -> "INT"
        | FieldType.Float -> "FLOAT"
        | FieldType.Numeric -> "DECIMAL"
        | FieldType.Boolean -> "TINYINT()"
        | FieldType.String -> "VARCHAR(255)"
        | FieldType.Text -> "TEXT"
        | FieldType.Binary -> "BLOB"
        | FieldType.Array -> "JSON"
        | FieldType.Record -> "JSON"
        | FieldType.Date -> "DATE"
        | FieldType.Time -> "TIME"
        | FieldType.DateTime -> "TIMESTAMP"
        | FieldType.DateTimeTz -> "DATETIME"

    // ... other cases for MySQL
    | SqlProvider.MSSQL ->
        match fieldType with
        | FieldType.Id -> "INT"
        | FieldType.Guid -> "UNIQUEIDENTIFIER"
        | FieldType.Integer -> "INT"
        | FieldType.Float -> "FLOAT"
        | FieldType.Numeric -> "NUMERIC"
        | FieldType.Boolean -> "BIT"
        | FieldType.String -> "NVARCHAR(255)"
        | FieldType.Text -> "NTEXT"
        | FieldType.Binary -> "VARBINAY(MAX)"
        | FieldType.Array -> "NVARCHAR(MAX)"
        | FieldType.Record -> "NVARCHAR(MAX)"
        | FieldType.Date -> "DATE"
        | FieldType.Time -> "TIME"
        | FieldType.DateTime -> "DATETIME2"
        | FieldType.DateTimeTz -> "DATETIMEOFFSET"
    | SqlProvider.SQLite ->
        match fieldType with
        | FieldType.Id -> "INTEGER"
        | FieldType.Guid -> "TEXT"
        | FieldType.Integer -> "INTEGER"
        | FieldType.Float -> "REAL"
        | FieldType.Numeric -> "REAL"
        | FieldType.Boolean -> "INTEGER"
        | FieldType.String -> "TEXT"
        | FieldType.Text -> "TEXT"
        | FieldType.Binary -> "BLOB"
        | FieldType.Array -> "TEXT"
        | FieldType.Record -> "TEXT"
        | FieldType.Date -> "TEXT"
        | FieldType.Time -> "TEXT"
        | FieldType.DateTime -> "TEXT"
        | FieldType.DateTimeTz -> "TEXT"

let getSchemaConstraintValue =
    function
    | PrimaryKey -> "PRIMARY KEY"
    | NotNull -> "NOT NULL"
    | Unique -> "UNIQUE"

let columnToString (col: Column) (sqlProvider: SqlProvider) =
    let constraints =
        col.Constraints |> List.map getSchemaConstraintValue |> String.concat " "

    let quoteChar = Providers.getQuoteChar sqlProvider
    sprintf "%s%s%s %s %s" quoteChar col.Name quoteChar (getSchemaMigrationValue col.Type sqlProvider) constraints

let toSql (table: Table) (sqlProvider: SqlProvider) =
    let columnsSql =
        table.Columns
        |> List.rev
        |> List.map (fun col -> columnToString col sqlProvider)
        |> String.concat ", "

    let quoteChar = Providers.getQuoteChar sqlProvider
    sprintf "CREATE TABLE %s%s%s (%s);" quoteChar table.Name quoteChar columnsSql
