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


type ColumnSchemaInput =
    { Name: string
      Type: string
      Properties: string option
      References: string option
      Provider: SqlProvider }

type ColumnSchemaDefinition =
    { SchemaInput: ColumnSchemaInput
      SchemaFieldType: SchemaFieldType
      MigrationValue: string
      DbPrimitive: string }


let quoteIdentifier (identifier: string) (provider: SqlProvider) =
    match provider with
    | SqlProvider.MSSQL -> $"[{identifier}]"
    | SqlProvider.MySql -> $"`{identifier}`"
    | _ ->
        let quote = "\""
        $"{quote}{identifier}{quote}"

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
        | SchemaArray -> ("NVARCHAR(MAX)", $"NVARCHAR(MAX) CHECK (ISJSON({columnName})>0)")
        | SchemaRecord -> ("NVARCHAR(MAX)", $"NVARCHAR(MAX) CHECK (ISJSON({columnName})>0)")
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


let getSchemaFieldMapping (fieldType: SchemaFieldType) (schemaInput: ColumnSchemaInput) : ColumnSchemaDefinition =

    let columnProperties =
        match schemaInput.Properties with
        | Some p -> $" {p}"
        | None -> ""

    match fieldType with
    | SchemaId ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaGuid ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaInteger ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaFloat ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaNumeric ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaBoolean ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaString ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaText ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaBinary ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaArray ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaRecord ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaDate ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaTime ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaUtcDateTime ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }
    | SchemaDateTimeTz ->
        let (providerSchemaPrimitive, providerSchemaMigrationValue) =
            getSchemaMigrationValue fieldType schemaInput.Provider schemaInput.Name

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue
          DbPrimitive = providerSchemaPrimitive }

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
    | "datetime" -> Some SchemaUtcDateTime
    | "datetimetz" -> Some SchemaDateTimeTz
    | _ -> None

let sampleInput (t: string) (provider: SqlProvider) =
    { Name = "YOUR_COLUMN_NAME"
      Type = t
      Properties = None
      References = None
      Provider = provider }

let schemaDefintionCliUsageDescriptions (provider: SqlProvider) =
    let helpId = getSchemaFieldMapping SchemaId (sampleInput "id" provider)
    let helpGuid = getSchemaFieldMapping SchemaGuid (sampleInput "guid" provider)
    let helpInteger = getSchemaFieldMapping SchemaInteger (sampleInput "int" provider)
    let helpFloat = getSchemaFieldMapping SchemaFloat (sampleInput "float" provider)

    let helpNumeric =
        getSchemaFieldMapping SchemaNumeric (sampleInput "numeric" provider)

    let helpBoolean =
        getSchemaFieldMapping SchemaBoolean (sampleInput "boolean" provider)

    let helpString = getSchemaFieldMapping SchemaString (sampleInput "string" provider)
    let helpText = getSchemaFieldMapping SchemaText (sampleInput "text" provider)
    let helpBinary = getSchemaFieldMapping SchemaBinary (sampleInput "binary" provider)
    let helpArray = getSchemaFieldMapping SchemaArray (sampleInput "array" provider)
    let helpRecord = getSchemaFieldMapping SchemaRecord (sampleInput "record" provider)
    let helpDate = getSchemaFieldMapping SchemaDate (sampleInput "date" provider)
    let helpTime = getSchemaFieldMapping SchemaTime (sampleInput "time" provider)

    let helpDateTime =
        getSchemaFieldMapping SchemaUtcDateTime (sampleInput "datetime" provider)

    let helpDateTimetz =
        getSchemaFieldMapping SchemaDateTimeTz (sampleInput "datetimetz" provider)


    $"""
Id              an integer autoincrementing primary key. Maps to {helpId.DbPrimitive}.
Guid            Sane default (uuid PK) for dotnet guids. Maps to {helpGuid.DbPrimitive}.
Integer         Maps to {helpInteger.DbPrimitive}.
Float           Maps to {helpFloat.DbPrimitive}.
Numeric         Maps to {helpNumeric.DbPrimitive}.
Boolean         Maps to {helpBoolean.DbPrimitive}.
String          Maps to {helpString.DbPrimitive}.
Text            Maps to {helpText.DbPrimitive}.
Binary          Maps to {helpBinary.DbPrimitive}.
Array           Maps to {helpArray.DbPrimitive}.
Record          Maps to {helpRecord.DbPrimitive}.
Date            Maps to {helpDate.DbPrimitive}.
Time            Maps to {helpTime.DbPrimitive}.
DateTime        Timestamp without TZ. Maps to {helpDateTime.DbPrimitive}.
DateTimeTz      Timestamp with TZ. Maps to {helpDateTimetz.DbPrimitive}.

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


let extractSchemaInput (input: string) (provider: SqlProvider) : ColumnSchemaInput =
    match input.Split(":") |> Array.toList with
    | [] ->
        failwithf
            "Failed to parse input schema definition for field: %s. Please make sure your definition follows the convention of fieldname:fieldtype"
            input
    | [ columnName; columnType ] ->
        { Name = columnName
          Type = columnType
          Properties = None
          References = None
          Provider = provider }
    | [ columnName; columnType; references; referenceField ] when references.ToUpper() = "REFERENCES" ->
        { Name = columnName
          Type = columnType
          Properties = None
          References = Some referenceField
          Provider = provider }
    | [ columnName; columnType; references; referenceTable; referenceField ] when references.ToUpper() = "REFERENCES" ->
        { Name = columnName
          Type = columnType
          Properties = None
          References = Some $"{quoteIdentifier referenceTable provider}({quoteIdentifier referenceField provider})"
          Provider = provider }
    // TODO: BE MORE CLEVER ABOUT THE PARSING OF 2nd TO LAST OR 3rd TO LAST BEING "REFERENCE" VALUE
    | columnName :: columnType :: additionalProperties when
        List.length additionalProperties >= 3
        && (List.item (List.length additionalProperties - 3) additionalProperties)
            .ToUpper() = "REFERENCES"
        ->
        // This is the scenario where additionalProperties includes a "references" entry, indicating that this schema definition is a reference field but also has additionalProperties that must be added to the line.
        let referenceTable =
            List.item (List.length additionalProperties - 2) additionalProperties

        let trimmedAdditionalProperties =
            List.take (List.length additionalProperties - 3) additionalProperties


        let referenceField = List.last additionalProperties

        { Name = columnName
          Type = columnType
          Properties = Some(trimmedAdditionalProperties |> String.concat " ")
          References = Some $"{quoteIdentifier referenceTable provider}({quoteIdentifier referenceField provider})"
          Provider = provider }
    | columnName :: columnType :: additionalProperties when
        List.length additionalProperties >= 2
        && (List.item (List.length additionalProperties - 2) additionalProperties)
            .ToUpper() = "REFERENCES"
        ->
        // This is the scenario where additionalProperties includes a "references" entry, indicating that this schema definition is a reference field but also has additionalProperties that must be added to the line.
        let referenceTableWithField = List.last additionalProperties

        let trimmedAdditionalProperties =
            List.take (List.length additionalProperties - 2) additionalProperties

        { Name = columnName
          Type = columnType
          Properties = Some(trimmedAdditionalProperties |> String.concat " ")
          References = Some $"{quoteIdentifier referenceTableWithField provider}"
          Provider = provider }

    | columnName :: columnType :: additionalProperties ->

        { Name = columnName
          Type = columnType
          Properties = Some(additionalProperties |> String.concat " ")
          References = None
          Provider = provider }
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

let genColumnSqlLine (schemaDefinition: ColumnSchemaDefinition) =
    let quotedColumnName =
        quoteIdentifier schemaDefinition.SchemaInput.Name schemaDefinition.SchemaInput.Provider

    match schemaDefinition.SchemaInput.References with
    | None -> $"\t{quotedColumnName} {schemaDefinition.MigrationValue}"
    | Some _ ->
        // If its a reference field, we need to only include the column and its propreties
        let properties =
            match schemaDefinition.SchemaInput.Properties with
            | None -> ""
            | Some properties -> $" {properties}"

        $"\t{quotedColumnName} {schemaDefinition.DbPrimitive}{properties}"

let genReferenceSqlLine (schemaDefinition: ColumnSchemaDefinition) =
    let quotedColumnName =
        quoteIdentifier schemaDefinition.SchemaInput.Name schemaDefinition.SchemaInput.Provider

    sprintf "\tFOREIGN KEY (%s) REFERENCES %s" quotedColumnName schemaDefinition.SchemaInput.References.Value

let genMigrations
    (migrationName: string)
    (tableName: string)
    (provider: SqlProvider)
    (schemaDefinitionsInput: string list)
    (outputDir: string)
    =

    let schemaDefinitions =
        schemaDefinitionsInput
        |> List.map (fun inp ->
            let schemaInput = extractSchemaInput inp provider

            let schemaFieldType =
                match parseSchemaFieldType schemaInput.Type with
                | Some schemaFieldType -> schemaFieldType
                | None -> failwithf "Failed to parse schema field for input column type %s" schemaInput.Type

            getSchemaFieldMapping schemaFieldType schemaInput)

    let sql =
        schemaDefinitions
        |> List.map (fun schemaDefinition ->
            match (parseSchemaFieldType schemaDefinition.SchemaInput.Type) with
            | Some t ->

                genColumnSqlLine schemaDefinition
            | None ->
                failwithf
                    "Unable to parse input schema definition for %s:%s"
                    schemaDefinition.SchemaInput.Name
                    schemaDefinition.SchemaInput.Type)
        |> String.concat (",\n")

    let referencesSql =
        schemaDefinitions
        |> List.filter (fun schemaDefinition -> schemaDefinition.SchemaInput.References.IsSome)
        |> List.map (fun schemaDefinition -> genReferenceSqlLine schemaDefinition)
        |> String.concat (",\n")

    let sql =
        match referencesSql with
        | "" -> sql
        | referencesSql -> $"{sql},\n{referencesSql}"

    let tableName = quoteIdentifier tableName provider
    let sql = $"CREATE TABLE {tableName} (\n{sql}\n);"
    let unixEpochCalc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    mkdirSafe outputDir
    let filePath = Path.Combine([| outputDir; $"{unixEpochCalc}_{migrationName}.SQL" |])
    File.WriteAllBytes(filePath, sql |> Encoding.UTF8.GetBytes)
    printfn $"Wrote migration {filePath}"
