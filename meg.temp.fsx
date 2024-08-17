#r "bin/Debug/net8.0/Meg.dll"

open Meg.Migrate
open Meg.Expressions.CreateTable
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

    let s = toSql table SqlProvider.PostgreSQL
    printfn $"sql: {s}"
// let m = migration ()
// let conn = System.Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
// let sqlContext = Meg.Providers.SqlContext.Create(Meg.Providers.PostgreSQL, conn)

// Meg.Migrate.runSqlScript (sqlContext, m, None)

up ()
