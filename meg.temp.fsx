#r "bin/Debug/net8.0/Meg.dll"
open Meg.Migrate

let migration () =
    """
    CREATE TABLE TODOS (
    Id SERIAL PRIMARY KEY,
    Name varchar(255),
    Text TEXT
    )
    """


let up () =

    let m = migration ()
    let conn = System.Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
    let sqlContext = Meg.Providers.SqlContext.Create(Meg.Providers.PostgreSQL, conn)
    Meg.Migrate.runSqlScript (sqlContext, m, None)
