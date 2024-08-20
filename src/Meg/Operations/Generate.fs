module Meg.Generate

open Meg.Providers
open Meg.Schema
open Meg.Expressions
open System.IO
open System
open System.Text

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




type ColumnSchemaInput =
    { Name: string
      Type: string
      Properties: string option
      References: string option
      Provider: SqlProvider }

type ColumnSchemaDefinition =
    { SchemaInput: ColumnSchemaInput
      SchemaFieldType: FieldType
      MigrationValue: string }


let getSchemaFieldMapping (fieldType: FieldType) (schemaInput: ColumnSchemaInput) : ColumnSchemaDefinition =

    let columnProperties =
        match schemaInput.Properties with
        | Some p -> $" {p}"
        | None -> ""

    match fieldType with
    | FieldType.Id ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Guid ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Integer ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Float ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Numeric ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Boolean ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.String ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Text ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Binary ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Array ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Record ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Date ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.Time ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.DateTime ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | FieldType.DateTimeTz ->
        let providerSchemaMigrationValue =
            getSchemaMigrationValue fieldType schemaInput.Provider

        let providerSchemaMigrationValue =
            $"{providerSchemaMigrationValue}{columnProperties}"

        { SchemaInput = schemaInput
          SchemaFieldType = fieldType
          MigrationValue = providerSchemaMigrationValue }
    | _ -> failwith "Unsupported field type."

let parseSchemaFieldType (inputType: string) : option<FieldType> =
    match inputType.ToLower() with
    | "id" -> Some FieldType.Id
    | "guid" -> Some FieldType.Guid
    | "uuid" -> Some FieldType.Guid
    | "integer" -> Some FieldType.Integer
    | "int" -> Some FieldType.Integer
    | "float" -> Some FieldType.Float
    | "numeric" -> Some FieldType.Numeric
    | "bool" -> Some FieldType.Boolean
    | "boolean" -> Some FieldType.Boolean
    | "string" -> Some FieldType.String
    | "text" -> Some FieldType.Text
    | "binary" -> Some FieldType.Binary
    | "array" -> Some FieldType.Array
    | "record" -> Some FieldType.Record
    | "date" -> Some FieldType.Date
    | "time" -> Some FieldType.Time
    | "timestamp" -> Some FieldType.DateTime
    | "utcdatetime" -> Some FieldType.DateTime
    | "datetime" -> Some FieldType.DateTime
    | "datetimetz" -> Some FieldType.DateTimeTz
    | _ -> None

let sampleInput (t: string) (provider: SqlProvider) =
    { Name = "YOUR_COLUMN_NAME"
      Type = t
      Properties = None
      References = None
      Provider = provider }

let schemaDefintionCliUsageDescriptions (provider: SqlProvider) =
    let helpId = getSchemaFieldMapping FieldType.Id (sampleInput "id" provider)
    let helpGuid = getSchemaFieldMapping FieldType.Guid (sampleInput "guid" provider)

    let helpInteger =
        getSchemaFieldMapping FieldType.Integer (sampleInput "int" provider)

    let helpFloat = getSchemaFieldMapping FieldType.Float (sampleInput "float" provider)

    let helpNumeric =
        getSchemaFieldMapping FieldType.Numeric (sampleInput "numeric" provider)

    let helpBoolean =
        getSchemaFieldMapping FieldType.Boolean (sampleInput "boolean" provider)

    let helpString =
        getSchemaFieldMapping FieldType.String (sampleInput "string" provider)

    let helpText = getSchemaFieldMapping FieldType.Text (sampleInput "text" provider)

    let helpBinary =
        getSchemaFieldMapping FieldType.Binary (sampleInput "binary" provider)

    let helpArray = getSchemaFieldMapping FieldType.Array (sampleInput "array" provider)

    let helpRecord =
        getSchemaFieldMapping FieldType.Record (sampleInput "record" provider)

    let helpDate = getSchemaFieldMapping FieldType.Date (sampleInput "date" provider)
    let helpTime = getSchemaFieldMapping FieldType.Time (sampleInput "time" provider)

    let helpDateTime =
        getSchemaFieldMapping FieldType.DateTime (sampleInput "datetime" provider)

    let helpDateTimetz =
        getSchemaFieldMapping FieldType.DateTimeTz (sampleInput "datetimetz" provider)


    $"""
Id              an integer autoincrementing primary key. Maps to {helpId.MigrationValue}.
Guid            Sane default (uuid PK) for dotnet guids. Maps to {helpGuid.MigrationValue}.
Integer         Maps to {helpInteger.MigrationValue}.
Float           Maps to {helpFloat.MigrationValue}.
Numeric         Maps to {helpNumeric.MigrationValue}.
Boolean         Maps to {helpBoolean.MigrationValue}.
String          Maps to {helpString.MigrationValue}.
Text            Maps to {helpText.MigrationValue}.
Binary          Maps to {helpBinary.MigrationValue}.
Array           Maps to {helpArray.MigrationValue}.
Record          Maps to {helpRecord.MigrationValue}.
Date            Maps to {helpDate.MigrationValue}.
Time            Maps to {helpTime.MigrationValue}.
DateTime        Timestamp without TZ. Maps to {helpDateTime.MigrationValue}.
DateTimeTz      Timestamp with TZ. Maps to {helpDateTimetz.MigrationValue}.

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
        let quoteChar = getQuoteChar provider

        { Name = columnName
          Type = columnType
          Properties = None
          References = Some referenceTable
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
          References = Some referenceTable
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
          References = Some referenceTableWithField
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

let genMigrations
    (migrationName: string)
    (tableName: string)
    (provider: SqlProvider)
    (schemaDefinitionsInput: string list)
    (outputDir: string)
    =

    let schemaDefinitions: ColumnSchemaDefinition list =
        schemaDefinitionsInput
        |> List.map (fun inp ->
            let schemaInput = extractSchemaInput inp provider

            let schemaFieldType =
                match parseSchemaFieldType schemaInput.Type with
                | Some schemaFieldType -> schemaFieldType
                | None -> failwithf "Failed to parse schema field for input column type %s" schemaInput.Type

            getSchemaFieldMapping schemaFieldType schemaInput)

    let columns =
        schemaDefinitions
        |> List.map (fun sd ->
            let constraints =
                match sd.SchemaInput.References with
                | Some reference -> [ FieldConstraint.ForeignKey reference ]
                | None -> []

            (sd.SchemaInput.Name, sd.SchemaFieldType, constraints))

    let sql =
        create_table {
            table tableName
            addColumns columns
        }
        |> toSql provider

    let unixEpochCalc = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    mkdirSafe outputDir
    let filePath = Path.Combine([| outputDir; $"{unixEpochCalc}_{migrationName}.SQL" |])
    File.WriteAllBytes(filePath, sql |> Encoding.UTF8.GetBytes)
    printfn $"Wrote migration {filePath}"
