#r "src/Meg/bin/Debug/net8.0/Meg.dll"

open Meg.Expressions
open Meg.Schema
open Meg.Providers

let tab1 =
    create_table {
        table "Zach"
        addColumn "name" FieldType.String []
        addColumn "family_id" FieldType.String [ FieldConstraint.ForeignKey "id" ]
    }

let vv = tab1 |> toSql SqlProvider.PostgreSQL
