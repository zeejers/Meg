#r "bin/Debug/net8.0/Meg.dll"

open Meg.Migrate
open Meg.Expressions
open Meg.Schema
open Meg.Providers

let migration () =
    """
    CREATE TABLE TODOS (
    Id SERIAL PRIMARY KEY,
    Name varchar(255),
    Text TEXT
    )
    """


let up () =

    let table =
        create_table {
            table "Users"
            addColumn "id" Id [ PrimaryKey; NotNull ]
            addColumn "name" String [ NotNull ]
            addColumn "created_at" DateTime [ NotNull ]
        }
        |> toSql SqlProvider.PostgreSQL

    let modified_todos =
        alter_table {
            table "Todos"
            removeColumn "Jj"
            modifyColumn "Jojo" Integer []
            addColumn "Koko" String [ NotNull ]
        }
        |> toSql SqlProvider.PostgreSQL

    let dropped_todos = drop_table { table "Toos" } |> toSql SqlProvider.PostgreSQL
    printfn "SQL: %s" table
    printfn "SQL2: %s" modified_todos
    printfn "SQL3: %s" dropped_todos
// let m = migration ()
// let conn = System.Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
// let sqlContext = Meg.Providers.SqlContext.Create(Meg.Providers.PostgreSQL, conn)

// Meg.Migrate.runSqlScript (sqlContext, m, None)

up ()
