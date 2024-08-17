module Meg.Expressions.CreateTable

open Meg.Schema

type CreateTableBuilder() =
    member _.Yield(_) = { Name = ""; Columns = [] }

    [<CustomOperation("table")>]
    member _.Table(state: Table, name: string) = { state with Name = name }

    [<CustomOperation("addColumn")>]
    member _.AddColumn(state: Table, name: string, fieldType: FieldType, constraints: FieldConstraint list) =
        { state with
            Columns =
                { Name = name
                  Type = fieldType
                  Constraints = constraints }
                :: state.Columns }

let create_table = CreateTableBuilder()
