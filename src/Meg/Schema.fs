module Meg.Schema

open Meg.Providers

// Defining the possible field types in the schema
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
    | Empty

// Defining the possible constraints for fields
type FieldConstraint =
    | PrimaryKey
    | NotNull
    | Unique
    | Check of string // A check constraint expression
    | Default of obj // Default value for the field
    | AutoIncrement // Auto-incrementing field
    | ForeignKey of (string * string)

// Defining the operations that can be performed on columns
type ColumnOperation =
    | Create
    | Modify
    | Remove
    | Rename

// Defining the structure of a database column
type Column =
    { Name: string
      Type: FieldType
      Constraints: FieldConstraint list
      Operation: ColumnOperation }

// Defining the operations that can be performed on tables
type TableOperation =
    | Create
    | Alter
    | Drop

// Defining the structure of a database table
type Table =
    { Name: string
      Columns: Column list
      Operation: TableOperation }

// Function to get SQL type based on the field type and SQL provider
let getSchemaMigrationValue (fieldType: FieldType) (provider: SqlProvider) =
    let fieldToSqlMap =
        match provider with
        | SqlProvider.PostgreSQL ->
            [ FieldType.Id, "INTEGER"
              FieldType.Guid, "UUID"
              FieldType.Integer, "INTEGER"
              FieldType.Float, "FLOAT"
              FieldType.Numeric, "NUMERIC"
              FieldType.Boolean, "BOOLEAN"
              FieldType.String, "VARCHAR(255)"
              FieldType.Text, "TEXT"
              FieldType.Binary, "BYTEA"
              FieldType.Array, "JSONB"
              FieldType.Record, "JSONB"
              FieldType.Date, "DATE"
              FieldType.Time, "TIME"
              FieldType.DateTime, "TIMESTAMP"
              FieldType.DateTimeTz, "TIMESTAMPTZ"
              FieldType.Empty, "" ]
        | SqlProvider.MySql ->
            [ FieldType.Id, "INTEGER"
              FieldType.Guid, "CHAR(36)"
              FieldType.Integer, "INT"
              FieldType.Float, "FLOAT"
              FieldType.Numeric, "DECIMAL"
              FieldType.Boolean, "TINYINT(1)"
              FieldType.String, "VARCHAR(255)"
              FieldType.Text, "TEXT"
              FieldType.Binary, "BLOB"
              FieldType.Array, "JSON"
              FieldType.Record, "JSON"
              FieldType.Date, "DATE"
              FieldType.Time, "TIME"
              FieldType.DateTime, "TIMESTAMP"
              FieldType.DateTimeTz, "DATETIME"
              FieldType.Empty, "" ]
        | SqlProvider.MSSQL ->
            [ FieldType.Id, "INT"
              FieldType.Guid, "UNIQUEIDENTIFIER"
              FieldType.Integer, "INT"
              FieldType.Float, "FLOAT"
              FieldType.Numeric, "NUMERIC"
              FieldType.Boolean, "BIT"
              FieldType.String, "NVARCHAR(255)"
              FieldType.Text, "NTEXT"
              FieldType.Binary, "VARBINAY(MAX)"
              FieldType.Array, "NVARCHAR(MAX)"
              FieldType.Record, "NVARCHAR(MAX)"
              FieldType.Date, "DATE"
              FieldType.Time, "TIME"
              FieldType.DateTime, "DATETIME2"
              FieldType.DateTimeTz, "DATETIMEOFFSET"
              FieldType.Empty, "" ]
        | SqlProvider.SQLite ->
            [ FieldType.Id, "INTEGER"
              FieldType.Guid, "TEXT"
              FieldType.Integer, "INTEGER"
              FieldType.Float, "REAL"
              FieldType.Numeric, "REAL"
              FieldType.Boolean, "INTEGER"
              FieldType.String, "TEXT"
              FieldType.Text, "TEXT"
              FieldType.Binary, "BLOB"
              FieldType.Array, "TEXT"
              FieldType.Record, "TEXT"
              FieldType.Date, "TEXT"
              FieldType.Time, "TEXT"
              FieldType.DateTime, "TEXT"
              FieldType.DateTimeTz, "TEXT"
              FieldType.Empty, "" ]

    match List.tryFind (fun (ft, _) -> ft = fieldType) fieldToSqlMap with
    | Some(_, sqlType) -> sqlType
    | None -> ""

// Function to map field constraints to their SQL equivalents
let getSchemaConstraintValue (schemaConstraint: FieldConstraint) (sqlProvider: SqlProvider) =
    // Function to map field constraints to their SQL equivalents based on the SQL provider
    match schemaConstraint with
    | PrimaryKey -> "PRIMARY KEY"
    | NotNull -> "NOT NULL"
    | Unique -> "UNIQUE"
    | ForeignKey(table) ->
        match sqlProvider with
        | SqlProvider.SQLite
        | SqlProvider.PostgreSQL
        | SqlProvider.MySql
        | SqlProvider.MSSQL -> $"FOREIGN KEY REFERENCES {table}(id)"
    | Check(expression) ->
        match sqlProvider with
        | SqlProvider.SQLite
        | SqlProvider.PostgreSQL
        | SqlProvider.MySql
        | SqlProvider.MSSQL -> $"CHECK ({expression})"
    | Default(value) ->
        let valueStr =
            match value with
            | :? string as s -> $"'{s}'"
            | :? int as i -> $"{i}"
            | :? float as f -> $"{f}"
            | _ -> failwith "Unsupported default value type"

        match sqlProvider with
        | SqlProvider.SQLite
        | SqlProvider.PostgreSQL
        | SqlProvider.MySql
        | SqlProvider.MSSQL -> $"DEFAULT {valueStr}"
    | AutoIncrement ->
        match sqlProvider with
        | SqlProvider.SQLite -> "AUTOINCREMENT"
        | SqlProvider.PostgreSQL -> "SERIAL"
        | SqlProvider.MySql -> "AUTO_INCREMENT"
        | SqlProvider.MSSQL -> "IDENTITY(1,1)"

let getInlineSchemaConstraintValue (fieldConstraint: FieldConstraint) (sqlProvider: SqlProvider) =
    match fieldConstraint with
    | FieldConstraint.PrimaryKey -> ""
    | _ -> getSchemaConstraintValue fieldConstraint sqlProvider

let getTableConstraintValue (fieldConstraint: FieldConstraint) (sqlProvider: SqlProvider) =
    match fieldConstraint with
    | FieldConstraint.PrimaryKey -> getSchemaConstraintValue fieldConstraint sqlProvider
    | _ -> ""

/// Generate SQL string representation of a column configuration, including the column primitive and constraints
let columnValue (col: Column) (sqlProvider: SqlProvider) =
    let constraints =
        col.Constraints
        |> List.map (fun c -> getInlineSchemaConstraintValue c sqlProvider)
        |> String.concat " "

    sprintf "%s %s" (getSchemaMigrationValue col.Type sqlProvider) constraints

/// Generate SQL statement for altering column
let columnAlterString (col: Column) (sqlProvider: SqlProvider) =
    let quoteChar = Providers.getQuoteChar sqlProvider

    match col.Operation with
    | ColumnOperation.Create ->
        Some(sprintf "ADD COLUMN %s%s%s %s" quoteChar col.Name quoteChar (columnValue col sqlProvider))
    | ColumnOperation.Remove -> Some(sprintf "DROP COLUMN %s%s%s" quoteChar col.Name quoteChar)
    | ColumnOperation.Modify ->
        Some(sprintf "MODIFY COLUMN %s%s%s %s" quoteChar col.Name quoteChar (columnValue col sqlProvider))
    | _ -> None

/// Generate SQL statement for creating column
let columnCreateString (col: Column) (sqlProvider: SqlProvider) =
    let quoteChar = Providers.getQuoteChar sqlProvider
    sprintf "ADD COLUMN %s%s%s %s" quoteChar col.Name quoteChar (columnValue col sqlProvider)

let tableConstraintString (col: Column) (sqlProvider: SqlProvider) =
    col.Constraints
    |> List.rev
    |> List.map (fun fieldConstraint -> getTableConstraintValue fieldConstraint sqlProvider)
    |> String.concat ", "

/// Generate SQL statement for table operations
let tableOperationToString (sqlProvider: SqlProvider) (table: Table) =
    let quoteChar = Providers.getQuoteChar sqlProvider

    match table.Operation with
    | TableOperation.Create ->
        let columnsSql =
            table.Columns
            |> List.rev
            |> List.map (fun col -> columnCreateString col sqlProvider)

        let tableConstraintsSql =
            table.Columns
            |> List.rev
            |> List.map (fun col -> tableConstraintString col sqlProvider)

        let tableSql = (columnsSql @ tableConstraintsSql) |> String.concat ", "

        sprintf "CREATE TABLE %s%s%s (%s);" quoteChar table.Name quoteChar tableSql
    | TableOperation.Alter ->
        let alterStatements =
            table.Columns
            |> List.choose (fun col -> columnAlterString col sqlProvider)
            |> String.concat ", "

        sprintf "ALTER TABLE %s%s%s %s;" quoteChar table.Name quoteChar alterStatements
    | TableOperation.Drop -> sprintf "DROP TABLE %s%s%s;" quoteChar table.Name quoteChar

    | _ -> failwith "Unsupported table operation"

/// Convert a table definition to its SQL equivalent
let toSql (sqlProvider: SqlProvider) (table: Table) =
    tableOperationToString sqlProvider table
