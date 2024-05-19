module Meg.Generate

open Meg.Providers
open System.IO
open System.Text
open System

// Reference to Ecto
// Ecto type            	Elixir type             Literal syntax in query
// :id          	        integer             	1, 2, 3
// :binary_id           	binary              	<<int, int, int, ...>>
// :integer         	    integer             	1, 2, 3
// :float           	    float               	1.0, 2.0, 3.0
// :boolean         	    boolean             	true, false
// :string          	    UTF-8 encoded string    "hello"
// :binary          	    binary              	<<int, int, int, ...>>
// {:array, inner_type}     list                	[value, value, value, ...]
// :map         	        map
// {:map, inner_type}       map
// :decimal         	    Decimal
// :date            	    Date
// :time            	    Time
// :time_usec           	Time
// :naive_datetime          NaiveDateTime
// :naive_datetime_usec     NaiveDateTime
// :utc_datetime            DateTime
// :utc_datetime_usec       DateTime

// Define supporting types
type PostgresDbType =
    | PUuid
    | PInt
    | PBigint
    | PNumeric
    | PDoublePrecision
    | PBoolean
    | PVarchar
    | PText
    | PBytea
    | PArray of PostgresDbType
    | PJsonb
    | PDate
    | PTime
    | PTimestampWithTimeZone
    | PTimestampWithoutTimeZone

type SchemaFieldType =
    | SchemaId
    | SchemaGuid
    | SchemaInteger
    | SchemaFloat
    | SchemaNumeric
    | SchemaBoolean
    | SchemaString
    | SchemaText
    | SchemaBinary
    | SchemaArray
    | SchemaRecord
    | SchemaDate
    | SchemaTime
    | SchemaUtcDateTime
    | SchemaDateTimeTz

type SchemaFieldTypeMapping =
    { SchemaFieldType: SchemaFieldType
      PostgresDbType: PostgresDbType
      MigrationValue: string
      MigrationPrimitive: string }

type ExtractedSchemaInput =
    { ColumnName: string
      ColumnType: string
      ColumnProperties: string option
      References: string option }

let getSchemaMigrationValue (fieldType: SchemaFieldType) (provider: SqlProvider) (columnName: string) =
    match provider with
    | SqlProvider.PostgreSQL ->
        match fieldType with
        | SchemaId -> ("INTEGER", "INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY")
        | SchemaGuid -> ("UUID", "UUID DEFAULT uuid_generate_v4() PRIMARY KEY")
        | SchemaInteger -> ("INTEGER", "INTEGER")
        | SchemaFloat -> ("FLOAT", "FLOAT")
        | SchemaNumeric -> ("NUMERIC", "NUMERIC")
        | SchemaBoolean -> ("BOOLEAN", "BOOLEAN")
        | SchemaString -> ("VARCHAR(255)", "VARCHAR(255)")
        | SchemaText -> ("TEXT", "TEXT")
        | SchemaBinary -> ("BYTEA", "BYTEA")
        | SchemaArray -> ("JSONB", "JSONB NOT NULL DEFAULT '[]'")
        | SchemaRecord -> ("JSONB", "JSONB NOT NULL DEFAULT '{}'")
        | SchemaDate -> ("DATE", "DATE")
        | SchemaTime -> ("TIME", "TIME")
        | SchemaUtcDateTime -> ("TIMESTAMP", "TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() at time zone 'utc')")
        | SchemaDateTimeTz -> ("TIMESTAMP", "TIMESTAMP WITH TIME ZONE")
    // ... other cases for Postgres
    | SqlProvider.MySql ->
        match fieldType with
        | SchemaId -> ("INTEGER", "INT AUTO_INCREMENT PRIMARY KEY")
        | SchemaGuid -> ("CHAR(36)", "CHAR(36) DEFAULT (UUID()) PRIMARY KEY")
        | SchemaInteger -> ("INT", "INT")
        | SchemaFloat -> ("FLOAT", "FLOAT")
        | SchemaNumeric -> ("DECIMAL", "DECIMAL")
        | SchemaBoolean -> ("TINYINT(1)", "TINYINT(1)")
        | SchemaString -> ("VARCHAR(255)", "VARCHAR(255)")
        | SchemaText -> ("TEXT", "TEXT")
        | SchemaBinary -> ("BLOB", "BLOB")
        | SchemaArray -> ("JSON", "JSON NOT NULL DEFAULT '[]'")
        | SchemaRecord -> ("JSON", "JSON NOT NULL DEFAULT '{}'")
        | SchemaDate -> ("DATE", "DATE")
        | SchemaTime -> ("TIME", "TIME")
        | SchemaUtcDateTime -> ("TIMESTAMP", "TIMESTAMP DEFAULT CURRENT_TIMESTAMP")
        | SchemaDateTimeTz -> ("DATETIME", "DATETIME")

    // ... other cases for MySQL
    | SqlProvider.MSSQL ->
        match fieldType with
        | SchemaId -> ("INT", "INT IDENTITY PRIMARY KEY")
        | SchemaGuid -> ("UNIQUEIDENTIFIER", "UNIQUEIDENTIFIER DEFAULT NEWID() PRIMARY KEY")
        | SchemaInteger -> ("INT", "INT")
        | SchemaFloat -> ("FLOAT", "FLOAT")
        | SchemaNumeric -> ("NUMERIC", "NUMERIC")
        | SchemaBoolean -> ("BIT", "BIT")
        | SchemaString -> ("NVARCHAR(255)", "NVARCHAR(255)")
        | SchemaText -> ("NTEXT", "NTEXT")
        | SchemaBinary -> ("VARBINARY(MAX)", "VARBINARY(MAX)")
        | SchemaArray -> ("NVARCHAR(MAX)", $"NVARCHAR(MAX) CHECK (ISJSON([{columnName}])>0)")
        | SchemaRecord -> ("NVARCHAR(MAX)", $"NVARCHAR(MAX) CHECK (ISJSON([{columnName}])>0)")
        | SchemaDate -> ("DATE", "DATE")
        | SchemaTime -> ("TIME", "TIME")
        | SchemaUtcDateTime -> ("DATETIME2", "DATETIME2 DEFAULT GETUTCDATE()")
        | SchemaDateTimeTz -> ("DATETIMEOFFSET", "DATETIMEOFFSET")
    | SqlProvider.SQLite ->
        match fieldType with
        | SchemaId -> ("INTEGER", "INTEGER PRIMARY KEY AUTOINCREMENT")
        | SchemaGuid ->
            ("TEXT",
             "TEXT PRIMARY KEY DEFAULT (lower(hex(randomblob(4))) || '-' || lower(hex(randomblob(2))) || '-4' || substr(lower(hex(randomblob(2))),2) || '-' || substr('89AB',abs(random()) % 4 + 1, 1) || substr(lower(hex(randomblob(2))),2) || '-' || lower(hex(randomblob(6))))")
        | SchemaInteger -> ("INTEGER", "INTEGER")
        | SchemaFloat -> ("REAL", "REAL")
        | SchemaNumeric -> ("REAL", "REAL")
        | SchemaBoolean -> ("INTEGER", $"INTEGER CHECK ({columnName} IN (0,1))")
        | SchemaString -> ("TEXT", "TEXT")
        | SchemaText -> ("TEXT", "TEXT")
        | SchemaBinary -> ("BLOB", "BLOB")
        | SchemaArray -> ("TEXT", "TEXT")
        | SchemaRecord -> ("TEXT", "TEXT")
        | SchemaDate -> ("TEXT", "TEXT")
        | SchemaTime -> ("TEXT", "TEXT")
        | SchemaUtcDateTime -> ("TEXT", "TEXT")
        | SchemaDateTimeTz -> ("TEXT", "TEXT")


let getSchemaFieldMapping
    (fieldType: SchemaFieldType)
    (provider: SqlProvider)
    (columnName: string option)
    (additionalProperties: string option)
    : SchemaFieldTypeMapping =
    let columnName =
        match columnName with
        | Some col -> col
        | None -> "YOUR_COLUMN_NAME"

    let additionalProperties =
        match additionalProperties with
        | Some p -> $" {p}"
        | None -> ""

    match fieldType with
    | SchemaId ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PInt
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaGuid ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PUuid
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaInteger ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PInt
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaFloat ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PDoublePrecision
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaNumeric ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PNumeric
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaBoolean ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PBoolean
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaString ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PVarchar
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaText ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PText
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaBinary ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PBytea
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaArray ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PJsonb
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaRecord ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PJsonb
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaDate ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PDate
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaTime ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PTime
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaUtcDateTime ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PTimestampWithoutTimeZone
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }
    | SchemaDateTimeTz ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType provider columnName

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{additionalProperties}"

        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PTimestampWithTimeZone
          MigrationValue = providerSchemaMigrationValue
          MigrationPrimitive = providerSchemaPrimitive }

let parseSchemaFieldType (inputType: string) : option<SchemaFieldType> =
    match inputType.ToLower() with
    | "id" -> Some SchemaId
    | "guid" -> Some SchemaGuid
    | "uuid" -> Some SchemaGuid
    | "integer" -> Some SchemaInteger
    | "int" -> Some SchemaInteger
    | "float" -> Some SchemaFloat
    | "numeric" -> Some SchemaNumeric
    | "bool" -> Some SchemaBoolean
    | "boolean" -> Some SchemaBoolean
    | "string" -> Some SchemaString
    | "text" -> Some SchemaText
    | "binary" -> Some SchemaBinary
    | "array" -> Some SchemaArray
    | "record" -> Some SchemaRecord
    | "date" -> Some SchemaDate
    | "time" -> Some SchemaTime
    | "timestamp" -> Some SchemaUtcDateTime
    | "utcdatetime" -> Some SchemaUtcDateTime
    | "datetimetz" -> Some SchemaDateTimeTz
    | _ -> None


let schemaDefintionCliUsageDescriptions (provider: SqlProvider) =
    let helpId = getSchemaFieldMapping SchemaId provider None None
    let helpGuid = getSchemaFieldMapping SchemaGuid provider None None
    let helpInteger = getSchemaFieldMapping SchemaInteger provider None None
    let helpFloat = getSchemaFieldMapping SchemaFloat provider None None
    let helpNumeric = getSchemaFieldMapping SchemaNumeric provider None None
    let helpBoolean = getSchemaFieldMapping SchemaBoolean provider None None
    let helpString = getSchemaFieldMapping SchemaString provider None None
    let helpText = getSchemaFieldMapping SchemaText provider None None
    let helpBinary = getSchemaFieldMapping SchemaBinary provider None None
    let helpArray = getSchemaFieldMapping SchemaArray provider None None
    let helpRecord = getSchemaFieldMapping SchemaRecord provider None None
    let helpDate = getSchemaFieldMapping SchemaDate provider None None
    let helpTime = getSchemaFieldMapping SchemaTime provider None None
    let helpDateTime = getSchemaFieldMapping SchemaUtcDateTime provider None None
    let helpDateTimetz = getSchemaFieldMapping SchemaDateTimeTz provider None None

    $"""
Id              an integer autoincrementing primary key. Maps to {helpId.MigrationPrimitive}.
Guid            Sane default (uuid PK) for dotnet guids. Maps to {helpGuid.MigrationPrimitive}.
Integer         Maps to {helpInteger.MigrationPrimitive}.
Float           Maps to {helpFloat.MigrationPrimitive}.
Numeric         Maps to {helpNumeric.MigrationPrimitive}.
Boolean         Maps to {helpBoolean.MigrationPrimitive}.
String          Maps to {helpString.MigrationPrimitive}.
Text            Maps to {helpText.MigrationPrimitive}.
Binary          Maps to {helpBinary.MigrationPrimitive}.
Array           Maps to {helpArray.MigrationPrimitive}.
Record          Maps to {helpRecord.MigrationPrimitive}.
Date            Maps to {helpDate.MigrationPrimitive}.
Time            Maps to {helpTime.MigrationPrimitive}.
DateTime        Timestamp without TZ. Maps to {helpDateTime.MigrationPrimitive}.
DateTimeTz      Timestamp with TZ. Maps to {helpDateTimetz.MigrationPrimitive}.

-- References --
You can include references using
[ColumnName]:[ColumnType]:References:[ReferenceTableName]:[ReferenceColumnName]
For example
-----------
user_id:int:references:users:id
-----------
-- Additional Properties --
You can include additional properties to a non reference field using a similar syntax
[ColumnName]:[ColumnType]:[ADDITIONAL PROPERTIES]
For example
-----------
"username:string:unique not null"
-----------
"""


let extractSchemaInput (input: string) =
    match input.Split(":") |> Array.toList with
    | [] ->
        failwithf
            "Failed to parse input schema definition for field: %s. Please make sure your definition follows the convention of fieldname:fieldtype"
            input
    | [ columnName; columnType ] ->
        { ColumnName = columnName
          ColumnType = columnType
          ColumnProperties = None
          References = None }
    | [ columnName; columnType; references; referenceField ] when references.ToUpper() = "REFERENCES" ->
        { ColumnName = columnName
          ColumnType = columnType
          ColumnProperties = None
          References = Some referenceField }
    | [ columnName; columnType; references; referenceTable; referenceField ] when references.ToUpper() = "REFERENCES" ->
        { ColumnName = columnName
          ColumnType = columnType
          ColumnProperties = None
          References = Some $"{referenceTable}({referenceField})" }
    | columnName :: columnType :: additionalProperies ->
        { ColumnName = columnName
          ColumnType = columnType
          ColumnProperties = Some(additionalProperies |> String.concat " ")
          References = None }
    | _ ->
        failwithf
            "Failed to parse input schema definition for field: %s. Please make sure your definition follows the convention of fieldname:fieldtype"
            input

let mkdirSafe dirPath =
    match Directory.Exists(dirPath) with
    | true -> ()
    | false ->
        Directory.CreateDirectory(dirPath) |> ignore
        ()

let genColumnSqlLine (fieldName: string) (schemaMapping: SchemaFieldTypeMapping) =
    let quote = "\""
    $"\t{quote}{fieldName}{quote} {schemaMapping.MigrationValue}"


let genForeignKeySqlLine schemaInput =
    sprintf "\tFOREIGN KEY (\"%s\") REFERENCES %s" schemaInput.ColumnName schemaInput.References.Value

let genMigrations
    (migrationName: string)
    (tableName: string)
    (provider: SqlProvider)
    (schemaDefinitions: string list)
    (outputDir: string)
    =

    let extractedSchemaInputs = schemaDefinitions |> List.map extractSchemaInput

    let sql =
        extractedSchemaInputs
        |> List.map (fun extractedSchemaInput ->
            match (parseSchemaFieldType extractedSchemaInput.ColumnType) with
            | Some t ->
                let schemaFieldTypeMapping =
                    getSchemaFieldMapping
                        t
                        provider
                        (Some extractedSchemaInput.ColumnName)
                        extractedSchemaInput.ColumnProperties

                genColumnSqlLine extractedSchemaInput.ColumnName schemaFieldTypeMapping
            | None ->
                failwithf
                    "Unable to parse input schema definition for %s:%s"
                    extractedSchemaInput.ColumnName
                    extractedSchemaInput.ColumnType)
        |> String.concat (",\n")

    let referencesSql =
        extractedSchemaInputs
        |> List.filter (fun schemaInput -> schemaInput.References.IsSome)
        |> List.map genForeignKeySqlLine
        |> String.concat (",\n")

    let sql =
        match referencesSql with
        | "" -> sql
        | referencesSql -> $"{sql},\n{referencesSql}"

    let sql = $"CREATE TABLE {tableName} (\n{sql}\n);"
    let unixEpochCalc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    mkdirSafe outputDir
    let filePath = Path.Combine([| outputDir; $"{unixEpochCalc}_{migrationName}.SQL" |])
    File.WriteAllBytes(filePath, sql |> Encoding.UTF8.GetBytes)
    printfn $"Wrote migration {filePath}"
