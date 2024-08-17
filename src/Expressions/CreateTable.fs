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

let columnTypeToString =
    function
    | FieldType.Integer -> "INT"
    | FieldType.String -> "VARCHAR(255)"
    | FieldType.Boolean -> "BOOLEAN"
    | FieldType.DateTime -> "DATETIME"
    | _ -> "NOT YET SUPPORTED"

let constraintToString =
    function
    | PrimaryKey -> "PRIMARY KEY"
    | NotNull -> "NOT NULL"
    | Unique -> "UNIQUE"

let columnToString (col: Column) =
    let constraints =
        col.Constraints |> List.map constraintToString |> String.concat " "

    sprintf "%s %s %s" col.Name (columnTypeToString col.Type) constraints

let tableToSql (table: Table) =
    let columnsSql =
        table.Columns |> List.rev |> List.map columnToString |> String.concat ", "

    sprintf "CREATE TABLE %s (%s);" table.Name columnsSql
