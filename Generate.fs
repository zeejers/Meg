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
      MigrationValue: string }

let getSchemaFieldMapping (fieldType: SchemaFieldType) : SchemaFieldTypeMapping =
    match fieldType with
    | SchemaId ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PInt
          MigrationValue = "INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY" }
    | SchemaGuid ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PUuid
          MigrationValue = "UUID DEFAULT uuid_generate_v4() PRIMARY KEY" }
    | SchemaInteger ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PInt
          MigrationValue = "INT" }
    | SchemaFloat ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PDoublePrecision
          MigrationValue = "FLOAT" }
    | SchemaNumeric ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PNumeric
          MigrationValue = "NUMERIC" }
    | SchemaBoolean ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PBoolean
          MigrationValue = "BOOLEAN" }
    | SchemaString ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PVarchar
          MigrationValue = "VARCHAR(255)" }
    | SchemaText ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PText
          MigrationValue = "TEXT" }
    | SchemaBinary ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PBytea
          MigrationValue = "BYTEA" }
    | SchemaArray ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PJsonb
          MigrationValue = "JSONB NOT NULL DEFAULT '[]'" }
    | SchemaRecord ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PJsonb
          MigrationValue = "JSONB NOT NULL DEFAULT '{}'" }
    | SchemaDate ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PDate
          MigrationValue = "DATE" }
    | SchemaTime ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PTime
          MigrationValue = "TIME" }
    | SchemaUtcDateTime ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PTimestampWithoutTimeZone
          MigrationValue = "TIMESTAMP WITHOUT TIME ZONE DEFAULT (now() at time zone 'utc')" }
    | SchemaDateTimeTz ->
        { SchemaFieldType = fieldType
          PostgresDbType = PostgresDbType.PTimestampWithTimeZone
          MigrationValue = "TIMESTAMP WITH TIME ZONE" }

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



let extractSchemaDefintion (input: string) =
    match input.Split(":") with
    | [| fieldName; fieldType |] -> (fieldName, fieldType)
    | _ ->
        failwithf
            "Failed to parse input schema definition for field: %s. Please make sure your definition follows the convention of fieldname:fieldtype"
            input

let genSqlLine (fieldName: string) (schemaMapping: SchemaFieldTypeMapping) =
    let quote = "\""
    $"\t{quote}{fieldName}{quote} {schemaMapping.MigrationValue}"

let mkdirSafe dirPath =
    match Directory.Exists(dirPath) with
    | true -> ()
    | false ->
        Directory.CreateDirectory(dirPath) |> ignore
        ()

let genMigrations
    (migrationName: string)
    (tableName: string)
    (provider: SqlProvider)
    (schemaDefinitions: string list)
    (outputDir: string)
    =

    let sql =
        schemaDefinitions
        |> List.map extractSchemaDefintion
        |> List.map (fun (x, y) ->
            match (parseSchemaFieldType y) with
            | Some t ->
                let schemaFieldTypeMapping = getSchemaFieldMapping t
                genSqlLine x schemaFieldTypeMapping
            | None -> failwithf "Unable to parse input schema definition for %s:%s" x y)
        |> String.concat (",\n")

    let sql = $"CREATE TABLE {tableName} (\n{sql}\n);"
    let unixEpochCalc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    mkdirSafe outputDir
    let filePath = Path.Combine([| outputDir; $"{unixEpochCalc}_{migrationName}.SQL" |])
    File.WriteAllBytes(filePath, sql |> Encoding.UTF8.GetBytes)
    printfn $"Wrote migration {filePath}"
