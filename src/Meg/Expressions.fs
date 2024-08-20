module Meg.Expressions

open Meg.Schema

// Look at example for CE builders
// https://github.com/Dzoukr/Dapper.FSharp/blob/master/src/Dapper.FSharp/PostgreSQL/Builders.fs

type AlterTableBuilder() =

    member this.Yield(_) =
        { Name = ""
          Columns = []
          Operation = TableOperation.Alter }

    [<CustomOperation("table")>]
    member this.Table(state: Table, name: string) = { state with Name = name }

    [<CustomOperation("addColumn")>]
    member this.AddColumn(state: Table, name: string, fieldType: FieldType, constraints: FieldConstraint list) =
        { state with
            Columns =
                { Name = name
                  Type = fieldType
                  Constraints = constraints
                  Operation = ColumnOperation.Create }
                :: state.Columns }

    [<CustomOperation("removeColumn")>]
    member this.RemoveColumn(state: Table, name: string) =
        { state with
            Columns =
                { Name = name
                  Type = FieldType.Empty
                  Constraints = []
                  Operation = ColumnOperation.Remove }
                :: state.Columns }

    [<CustomOperation("modifyColumn")>]
    member this.ModifyColumn
        (state: Table, name: string, newFieldType: FieldType, newConstraints: FieldConstraint list)
        =
        { state with
            Columns =
                { Name = name
                  Type = newFieldType
                  Constraints = newConstraints
                  Operation = ColumnOperation.Modify }
                :: state.Columns }

    member this.Zero() = None

    member inline this.For(s: seq<'T>, [<InlineIfLambda>] f: 'T -> Table) = this.Yield(f)



let alter_table = AlterTableBuilder()

type CreateTableBuilder() =


    member this.Yield(_) =
        { Name = ""
          Columns = []
          Operation = TableOperation.Create }

    [<CustomOperation("table")>]
    member this.Table(state: Table, name: string) = { state with Name = name }

    [<CustomOperation("addColumn")>]
    member this.AddColumn(state: Table, name: string, fieldType: FieldType, constraints: FieldConstraint list) =
        { state with
            Columns =
                { Name = name
                  Type = fieldType
                  Constraints = constraints
                  Operation = ColumnOperation.Create }
                :: state.Columns }


    [<CustomOperation("addColumns")>]
    member this.AddColumns(state: Table, columns: (string * FieldType * FieldConstraint list) list) =
        let result =
            columns
            |> List.rev
            |> List.fold
                (fun acc (name, fieldType, fieldConstraints) ->
                    let kip = this.AddColumn(acc, name, fieldType, fieldConstraints)
                    kip)
                state

        result


let create_table = CreateTableBuilder()

type DropTableBuilder() =
    member this.Yield(_) =
        { Name = ""
          Columns = []
          Operation = TableOperation.Drop }


    [<CustomOperation("table")>]
    member this.Table(state: Table, name: string) = { state with Name = name }

let drop_table = DropTableBuilder()
