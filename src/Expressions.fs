module Meg.Expressions

open Meg.Schema

type AlterTableBuilder() =
    member _.Yield(_) =
        { Name = ""
          Columns = []
          Operation = TableOperation.Alter }

    [<CustomOperation("table")>]
    member _.Table(state: Table, name: string) = { state with Name = name }

    [<CustomOperation("addColumn")>]
    member _.AddColumn(state: Table, name: string, fieldType: FieldType, constraints: FieldConstraint list) =
        { state with
            Columns =
                { Name = name
                  Type = fieldType
                  Constraints = constraints
                  Operation = ColumnOperation.Create }
                :: state.Columns }

    [<CustomOperation("removeColumn")>]
    member _.RemoveColumn(state: Table, name: string) =
        { state with
            Columns =
                { Name = name
                  Type = FieldType.Empty
                  Constraints = []
                  Operation = ColumnOperation.Remove }
                :: state.Columns }

    [<CustomOperation("modifyColumn")>]
    member _.ModifyColumn(state: Table, name: string, newFieldType: FieldType, newConstraints: FieldConstraint list) =
        { state with
            Columns =
                { Name = name
                  Type = newFieldType
                  Constraints = newConstraints
                  Operation = ColumnOperation.Modify }
                :: state.Columns }

let alter_table = AlterTableBuilder()

type CreateTableBuilder() =
    member _.Yield(_) =
        { Name = ""
          Columns = []
          Operation = TableOperation.Create }

    [<CustomOperation("table")>]
    member _.Table(state: Table, name: string) = { state with Name = name }

    [<CustomOperation("addColumn")>]
    member _.AddColumn(state: Table, name: string, fieldType: FieldType, constraints: FieldConstraint list) =
        { state with
            Columns =
                { Name = name
                  Type = fieldType
                  Constraints = constraints
                  Operation = ColumnOperation.Create }
                :: state.Columns }


let create_table = CreateTableBuilder()

type DropTableBuilder() =
    member _.Yield(_) =
        { Name = ""
          Columns = []
          Operation = TableOperation.Drop }

    [<CustomOperation("table")>]
    member _.Table(state: Table, name: string) = { state with Name = name }

let drop_table = DropTableBuilder()
