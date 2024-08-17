module Meg.Schema

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
