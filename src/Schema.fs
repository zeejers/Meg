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

let getSchemaMigrationValue (fieldType: FieldType) (provider: SqlProvider) (columnName: string) =
    match provider with
    | SqlProvider.PostgreSQL ->
        match fieldType with
        | FieldType.Id -> ("INTEGER", "INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY")
        | FieldType.Guid -> ("UUID", "UUID DEFAULT uuid_generate_v4() PRIMARY KEY")
        | FieldType.Integer -> ("INTEGER", "INTEGER")
        | FieldType.Float -> ("FLOAT", "FLOAT")
        | FieldType.Numeric -> ("NUMERIC", "NUMERIC")
        | FieldType.Boolean -> ("BOOLEAN", "BOOLEAN")
        | FieldType.String -> ("VARCHAR(255)", "VARCHAR(255)")
        | FieldType.Text -> ("TEXT", "TEXT")
        | FieldType.Binary -> ("BYTEA", "BYTEA")
        | FieldType.Array -> ("JSONB", "JSONB NOT NULL DEFAULT '[]'")
        | FieldType.Record -> ("JSONB", "JSONB NOT NULL DEFAULT '{}'")
        | FieldType.Date -> ("DATE", "DATE")
        | FieldType.Time -> ("TIME", "TIME")
        | FieldType.DateTime -> ("TIMESTAMP", "TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() at time zone 'utc')")
        | FieldType.DateTimeTz -> ("TIMESTAMP", "TIMESTAMP WITH TIME ZONE")
    // ... other cases for Postgres
    | SqlProvider.MySql ->
        match fieldType with
        | FieldType.Id -> ("INTEGER", "INT AUTO_INCREMENT PRIMARY KEY")
        | FieldType.Guid -> ("CHAR(36)", "CHAR(36) DEFAULT (UUID()) PRIMARY KEY")
        | FieldType.Integer -> ("INT", "INT")
        | FieldType.Float -> ("FLOAT", "FLOAT")
        | FieldType.Numeric -> ("DECIMAL", "DECIMAL")
        | FieldType.Boolean -> ("TINYINT(1)", "TINYINT(1)")
        | FieldType.String -> ("VARCHAR(255)", "VARCHAR(255)")
        | FieldType.Text -> ("TEXT", "TEXT")
        | FieldType.Binary -> ("BLOB", "BLOB")
        | FieldType.Array -> ("JSON", "JSON NOT NULL DEFAULT '[]'")
        | FieldType.Record -> ("JSON", "JSON NOT NULL DEFAULT '{}'")
        | FieldType.Date -> ("DATE", "DATE")
        | FieldType.Time -> ("TIME", "TIME")
        | FieldType.DateTime -> ("TIMESTAMP", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP")
        | FieldType.DateTimeTz -> ("DATETIME", "DATETIME")

    // ... other cases for MySQL
    | SqlProvider.MSSQL ->
        match fieldType with
        | FieldType.Id -> ("INT", "INT IDENTITY PRIMARY KEY")
        | FieldType.Guid -> ("UNIQUEIDENTIFIER", "UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY")
        | FieldType.Integer -> ("INT", "INT")
        | FieldType.Float -> ("FLOAT", "FLOAT")
        | FieldType.Numeric -> ("NUMERIC", "NUMERIC")
        | FieldType.Boolean -> ("BIT", "BIT")
        | FieldType.String -> ("NVARCHAR(255)", "NVARCHAR(255)")
        | FieldType.Text -> ("NTEXT", "NTEXT")
        | FieldType.Binary -> ("VARBINARY(MAX)", "VARBINARY(MAX)")
        | FieldType.Array -> ("NVARCHAR(MAX)", $"NVARCHAR(MAX) CHECK (ISJSON({columnName})>0)")
        | FieldType.Record -> ("NVARCHAR(MAX)", $"NVARCHAR(MAX) CHECK (ISJSON({columnName})>0)")
        | FieldType.Date -> ("DATE", "DATE")
        | FieldType.Time -> ("TIME", "TIME")
        | FieldType.DateTime -> ("DATETIME2", "DATETIME2 DEFAULT GETUTCDATE()")
        | FieldType.DateTimeTz -> ("DATETIMEOFFSET", "DATETIMEOFFSET")
    | SqlProvider.SQLite ->
        match fieldType with
        | FieldType.Id -> ("INTEGER", "INTEGER PRIMARY KEY AUTOINCREMENT")
        | FieldType.Guid ->
            ("TEXT",
             "TEXT PRIMARY KEY DEFAULT (lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))),2) || '-' || substr('89AB',abs(random()) % 4 + 1, 1) || substr(lower(hex(randomblob(2))),2) || '-' || lower(hex(randomblob(6))))")
        | FieldType.Integer -> ("INTEGER", "INTEGER")
        | FieldType.Float -> ("REAL", "REAL")
        | FieldType.Numeric -> ("REAL", "REAL")
        | FieldType.Boolean -> ("INTEGER", $"INTEGER CHECK ({columnName} IN (0,1))")
        | FieldType.String -> ("TEXT", "TEXT")
        | FieldType.Text -> ("TEXT", "TEXT")
        | FieldType.Binary -> ("BLOB", "BLOB")
        | FieldType.Array -> ("TEXT", "TEXT")
        | FieldType.Record -> ("TEXT", "TEXT")
        | FieldType.Date -> ("TEXT", "TEXT")
        | FieldType.Time -> ("TEXT", "TEXT")
        | FieldType.DateTime -> ("TEXT", "TEXT")
        | FieldType.DateTimeTz -> ("TEXT", "TEXT")
