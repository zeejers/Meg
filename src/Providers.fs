module Meg.Providers

open System.Data
open System.Data.SqlClient
open MySql.Data.MySqlClient
open Dapper.FSharp
open Dapper.FSharp.PostgreSQL
open Dapper.FSharp.MSSQL
open Dapper.FSharp.MySQL
open Dapper.FSharp.SQLite
open Dapper
open Npgsql
open Microsoft.Data.Sqlite

// for MSSQL
Dapper.FSharp.MSSQL.OptionTypes.register ()

// for MySQL
Dapper.FSharp.MySQL.OptionTypes.register ()

// for PostgreSQL
Dapper.FSharp.PostgreSQL.OptionTypes.register ()

// for SQLite
Dapper.FSharp.SQLite.OptionTypes.register ()

type SqlProvider =
    | PostgreSQL
    | MSSQL
    | MySql
    | SQLite

type SqlProviderConnection = IDbConnection

type SchemaMigration =
    { version: string; inserted_at: string }

type ISqlExecutionStrategy =
    abstract member ExecuteScalar: string -> obj
    abstract member ExecuteNonQuery: string -> int
    abstract member GetConnection: unit -> SqlProviderConnection

type SqlExecutionStrategy(createConnection: unit -> SqlProviderConnection, provider: SqlProvider) =
    member val SqlProvider = provider with get

    interface ISqlExecutionStrategy with
        member this.ExecuteScalar(query: string) =
            use connection = createConnection ()
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- query
            command.ExecuteScalar()

        member this.ExecuteNonQuery(commandText: string) =
            use connection = createConnection ()
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- commandText
            command.ExecuteNonQuery()

        member this.GetConnection() = createConnection ()

type QuerySource<'T> =
    | Postgres of Dapper.FSharp.PostgreSQL.Builders.QuerySource<'T>
    | MSSQL of Dapper.FSharp.MSSQL.Builders.QuerySource<'T>
    | MySQL of Dapper.FSharp.MySQL.Builders.QuerySource<'T>
    | SQLite of Dapper.FSharp.SQLite.Builders.QuerySource<'T>

type ITable<'T> =
    abstract member QuerySource: QuerySource<'T>

    abstract member Select:
        SqlProviderConnection -> System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<'T>>

    abstract member Insert: 'T * SqlProviderConnection -> System.Threading.Tasks.Task<int>
    abstract member DeleteAll: SqlProviderConnection -> System.Threading.Tasks.Task<int>

type PostgresTable<'T>(tableName: string) =
    let dapperQuerySource = Dapper.FSharp.PostgreSQL.Builders.Table.table'<'T> tableName

    interface ITable<'T> with
        member this.QuerySource = Postgres(dapperQuerySource)

        member this.Select(conn: SqlProviderConnection) =
            Dapper.FSharp.PostgreSQL.Builders.select {
                for e in dapperQuerySource do
                    selectAll
            }
            |> conn.SelectAsync<'T>

        member this.Insert(item: 'T, conn: SqlProviderConnection) =
            Dapper.FSharp.PostgreSQL.Builders.insert {
                into dapperQuerySource
                value item
            }
            |> conn.InsertAsync

        member this.DeleteAll(conn: SqlProviderConnection) =
            Dapper.FSharp.PostgreSQL.Builders.delete {
                for e in dapperQuerySource do
                    deleteAll
            }
            |> conn.DeleteAsync


type MSSQLTable<'T>(tableName: string) =
    let dapperQuerySource = Dapper.FSharp.MSSQL.Builders.Table.table'<'T> tableName

    interface ITable<'T> with
        member this.QuerySource =
            MSSQL(Dapper.FSharp.MSSQL.Builders.Table.table'<'T> tableName)

        member this.Select(conn: SqlProviderConnection) =
            Dapper.FSharp.MSSQL.Builders.select {
                for e in dapperQuerySource do
                    selectAll
            }
            |> conn.SelectAsync<'T>

        member this.Insert(item: 'T, conn: SqlProviderConnection) =
            Dapper.FSharp.MSSQL.Builders.insert {
                into dapperQuerySource
                value item
            }
            |> conn.InsertAsync

        member this.DeleteAll(conn: SqlProviderConnection) =
            Dapper.FSharp.MSSQL.Builders.delete {
                for e in dapperQuerySource do
                    deleteAll
            }
            |> conn.DeleteAsync



type MySQLTable<'T>(tableName: string) =
    let dapperQuerySource = Dapper.FSharp.MySQL.Builders.Table.table'<'T> tableName

    interface ITable<'T> with
        member this.QuerySource =
            MySQL(Dapper.FSharp.MySQL.Builders.Table.table'<'T> tableName)

        member this.Select(conn: SqlProviderConnection) =
            Dapper.FSharp.MySQL.Builders.select {
                for e in dapperQuerySource do
                    selectAll
            }
            |> conn.SelectAsync<'T>

        member this.Insert(item: 'T, conn: SqlProviderConnection) =
            Dapper.FSharp.MySQL.Builders.insert {
                into dapperQuerySource
                value item
            }
            |> conn.InsertAsync

        member this.DeleteAll(conn: SqlProviderConnection) =
            Dapper.FSharp.MySQL.Builders.delete {
                for e in dapperQuerySource do
                    deleteAll
            }
            |> conn.DeleteAsync



type SQLiteTable<'T>(tableName: string) =
    let dapperQuerySource = Dapper.FSharp.SQLite.Builders.Table.table'<'T> tableName

    interface ITable<'T> with
        member this.QuerySource =
            SQLite(Dapper.FSharp.SQLite.Builders.Table.table'<'T> tableName)

        member this.Select(conn: SqlProviderConnection) =
            Dapper.FSharp.SQLite.Builders.select {
                for e in dapperQuerySource do
                    selectAll
            }
            |> conn.SelectAsync<'T>

        member this.Insert(item: 'T, conn: SqlProviderConnection) =
            Dapper.FSharp.SQLite.Builders.insert {
                into dapperQuerySource
                value item
            }
            |> conn.InsertAsync

        member this.DeleteAll(conn: SqlProviderConnection) =
            Dapper.FSharp.SQLite.Builders.delete {
                for e in dapperQuerySource do
                    deleteAll
            }
            |> conn.DeleteAsync


let parseProvider (p: string) =
    match p.ToLower() with
    | "postgresql" -> SqlProvider.PostgreSQL
    | "mssql" -> SqlProvider.MSSQL
    | "mysql" -> SqlProvider.MySql
    | "sqlite" -> SqlProvider.SQLite
    | _ -> SqlProvider.PostgreSQL

let createPostgreSqlConnection (connectionString: string) () : SqlProviderConnection =
    new Npgsql.NpgsqlConnection(connectionString) :> SqlProviderConnection

let createMSSqlConnection (connectionString: string) () : SqlProviderConnection =
    new System.Data.SqlClient.SqlConnection(connectionString) :> SqlProviderConnection

let createMySQLConnection (connectionString: string) () : SqlProviderConnection =
    new MySql.Data.MySqlClient.MySqlConnection(connectionString) :> SqlProviderConnection

let createSQLiteConnection (connectionString: string) () : SqlProviderConnection =
    new SqliteConnection(connectionString) :> SqlProviderConnection

type SqlContext(strategy: ISqlExecutionStrategy) =
    member val migrationsTableName = "schema_migrations"
    member val migrationsTableColumnDefinitions = [ "version", "VARCHAR(255)"; "inserted_at", "VARCHAR(255)" ]
    member this.ExecuteScalar(query: string) = strategy.ExecuteScalar(query)
    member this.ExecuteNonQuery(command: string) = strategy.ExecuteNonQuery(command)
    member this.GetConnection() = strategy.GetConnection()

    member this.Table<'T>(tableName) =
        match strategy with
        | :? SqlExecutionStrategy as pg when pg.SqlProvider = SqlProvider.PostgreSQL ->
            PostgresTable(tableName) :> ITable<'T>
        | :? SqlExecutionStrategy as ms when ms.SqlProvider = SqlProvider.MSSQL -> MSSQLTable(tableName) :> ITable<'T>
        | :? SqlExecutionStrategy as my when my.SqlProvider = SqlProvider.MySql -> MySQLTable(tableName) :> ITable<'T>
        | :? SqlExecutionStrategy as sq when sq.SqlProvider = SqlProvider.SQLite -> SQLiteTable(tableName) :> ITable<'T>
        | _ -> failwith "Unknown provider"

    member this.GetDatabaseExistsQuery(databaseName: string) =
        match strategy with
        | :? SqlExecutionStrategy as pg when pg.SqlProvider = SqlProvider.PostgreSQL ->
            $"SELECT 1 FROM pg_database WHERE datname='{databaseName}'"
        | :? SqlExecutionStrategy as ms when ms.SqlProvider = SqlProvider.MSSQL ->
            $"IF EXISTS(SELECT * FROM sys.databases WHERE name = '{databaseName}') SELECT 1 ELSE SELECT 0;"
        | :? SqlExecutionStrategy as my when my.SqlProvider = SqlProvider.MySql ->
            $"SELECT EXISTS(SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}');"
        | :? SqlExecutionStrategy as sq when sq.SqlProvider = SqlProvider.SQLite -> "SELECT 1;"
        //  Probably can't do this.
        | _ -> failwith "Unknown provider"

    member this.CreateTableCommand(tableName, columns) =
        let columnDefinitions =
            columns
            |> List.map (fun (name, dataType) -> $"{name} {dataType}")
            |> String.concat (", ")

        match strategy with
        | :? SqlExecutionStrategy as pg when pg.SqlProvider = SqlProvider.PostgreSQL ->
            sprintf "CREATE TABLE IF NOT EXISTS %s (%s);" tableName columnDefinitions
        | :? SqlExecutionStrategy as ms when ms.SqlProvider = SqlProvider.MSSQL ->
            sprintf
                "IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '%s') CREATE TABLE %s (%s);"
                tableName
                tableName
                columnDefinitions
        | :? SqlExecutionStrategy as my when my.SqlProvider = SqlProvider.MySql ->
            sprintf "CREATE TABLE IF NOT EXISTS %s (%s) ENGINE=InnoDB DEFAULT CHARSET=utf8;" tableName columnDefinitions

        | :? SqlExecutionStrategy as sq when sq.SqlProvider = SqlProvider.SQLite ->
            sprintf "CREATE TABLE IF NOT EXISTS %s (%s);" tableName columnDefinitions
        | _ -> failwith "Unknown provider"

    member this.CreateMigrationsTableCommand() =
        this.CreateTableCommand(this.migrationsTableName, this.migrationsTableColumnDefinitions)

    static member Create(provider: SqlProvider, connectionString: string) =
        let strategy =
            match provider with
            | SqlProvider.PostgreSQL ->
                SqlExecutionStrategy(createPostgreSqlConnection (connectionString), SqlProvider.PostgreSQL)
            | SqlProvider.MSSQL -> SqlExecutionStrategy(createMSSqlConnection (connectionString), SqlProvider.MSSQL)
            | SqlProvider.MySql -> SqlExecutionStrategy(createMySQLConnection (connectionString), SqlProvider.MySql)
            | SqlProvider.SQLite -> SqlExecutionStrategy(createSQLiteConnection (connectionString), SqlProvider.SQLite)
        // Add more cases for other providers...
        new SqlContext(strategy)
