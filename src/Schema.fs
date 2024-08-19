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
    | ForeignKey of string // Points to another table's primary key
    | Check of string // A check constraint expression
    | Default of obj // Default value for the field
    | AutoIncrement // Auto-incrementing field

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
let getSchemaConstraintValue (schemaConstraint: FieldConstraint) =
    match schemaConstraint with
    | PrimaryKey -> "PRIMARY KEY"
    | NotNull -> "NOT NULL"
    | Unique -> "UNIQUE"

// Generate SQL string representation of a column configuration
let columnToString (col: Column) (sqlProvider: SqlProvider) =
    let constraints =
        col.Constraints |> List.map getSchemaConstraintValue |> String.concat " "

    sprintf "%s %s" (getSchemaMigrationValue col.Type sqlProvider) constraints

// Generate SQL statement for altering columns
let columnAlterString (col: Column) (sqlProvider: SqlProvider) =
    let quoteChar = Providers.getQuoteChar sqlProvider

    match col.Operation with
    | ColumnOperation.Create ->
        Some(sprintf "ADD COLUMN %s%s%s %s" quoteChar col.Name quoteChar (columnToString col sqlProvider))
    | ColumnOperation.Remove -> Some(sprintf "DROP COLUMN %s%s%s" quoteChar col.Name quoteChar)
    | ColumnOperation.Modify ->
        Some(sprintf "MODIFY COLUMN %s%s%s %s" quoteChar col.Name quoteChar (columnToString col sqlProvider))
    | _ -> None

// let columnCreateString (col: Column) (sqlProvider: SqlProvider) =
//     let quoteChar = Providers.getQuoteChar sqlProvider
//     sprintf "ADD COLUMN %s%s%s %s" quoteChar col.Name quoteChar
// Generate SQL statement for table operations
let tableOperationToString (sqlProvider: SqlProvider) (table: Table) =
    let quoteChar = Providers.getQuoteChar sqlProvider

    match table.Operation with
    | TableOperation.Create ->
        let columnsSql =
            table.Columns
            |> List.rev
            |> List.map (fun col -> columnToString col sqlProvider)
            |> String.concat ", "

        sprintf "CREATE TABLE %s%s%s (%s);" quoteChar table.Name quoteChar columnsSql
    | TableOperation.Alter ->
        let alterStatements =
            table.Columns
            |> List.choose (fun col -> columnAlterString col sqlProvider)
            |> String.concat ", "

        sprintf "ALTER TABLE %s%s%s %s;" quoteChar table.Name quoteChar alterStatements
    | TableOperation.Drop -> sprintf "DROP TABLE %s%s%s;" quoteChar table.Name quoteChar

    | _ -> failwith "Unsupported table operation"

// Convert a table definition to its SQL equivalent
let toSql (sqlProvider: SqlProvider) (table: Table) =
    tableOperationToString sqlProvider table
