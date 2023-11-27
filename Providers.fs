module Meg.Providers

open System.Data
open System.Data.SqlClient
open MySql.Data.MySqlClient
open Npgsql
open Microsoft.Data.Sqlite

type SqlProvider =
    | PostgreSQL
    | MSSQL
    | MySql
    | SQLite

type SqlProviderConnection = IDbConnection

type ISqlExecutionStrategy =
    abstract member ExecuteScalar: string -> obj
    abstract member ExecuteNonQuery: string -> int

type SqlExecutionStrategy(createConnection: unit -> SqlProviderConnection, provider: SqlProvider) =
    member val SqlProvider = provider with get

    interface ISqlExecutionStrategy with
        member this.ExecuteScalar(query: string) =
            use connection = createConnection()
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- query
            command.ExecuteScalar()

        member this.ExecuteNonQuery(commandText: string) =
            use connection = createConnection()
            connection.Open()
            use command = connection.CreateCommand()
            command.CommandText <- commandText
            command.ExecuteNonQuery()

let createPostgreSqlConnection(connectionString: string) () : SqlProviderConnection =
    new Npgsql.NpgsqlConnection(connectionString) :> SqlProviderConnection

let createMSSqlConnection(connectionString: string) () : SqlProviderConnection =
    new System.Data.SqlClient.SqlConnection(connectionString) :> SqlProviderConnection

let createMySQLConnection(connectionString: string) () : SqlProviderConnection =
    new MySql.Data.MySqlClient.MySqlConnection(connectionString) :> SqlProviderConnection

let createSQLiteConnection(connectionString: string) () : SqlProviderConnection =
    new SqliteConnection(connectionString) :> SqlProviderConnection

// Other connection creation functions...

type SqlContext(strategy: ISqlExecutionStrategy) =
    member this.ExecuteScalar(query: string) = strategy.ExecuteScalar(query)
    member this.ExecuteNonQuery(command: string) = strategy.ExecuteNonQuery(command)
    member this.GetDatabaseExistsQuery(databaseName: string) =
        match strategy with
        | :? SqlExecutionStrategy as pg when pg.SqlProvider = SqlProvider.PostgreSQL -> 
            $"SELECT 1 FROM pg_database WHERE datname='{databaseName}'"
        | :? SqlExecutionStrategy as ms when ms.SqlProvider = SqlProvider.MSSQL -> 
            $"IF EXISTS(SELECT * FROM sys.databases WHERE name = '{databaseName}') SELECT 1 ELSE SELECT 0;"
        | :? SqlExecutionStrategy as my when my.SqlProvider = SqlProvider.MySql -> 
            $"SELECT EXISTS(SELECT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}');"
        | :? SqlExecutionStrategy as sq when sq.SqlProvider = SqlProvider.SQLite -> 
             "SELECT 1;"
            //  Probably can't do this.
        | _ ->
            failwith "Unknown provider"

    static member Create(provider: SqlProvider, connectionString: string) =
        let strategy = 
            match provider with
            | SqlProvider.PostgreSQL -> SqlExecutionStrategy(createPostgreSqlConnection(connectionString), SqlProvider.PostgreSQL)
            | SqlProvider.MSSQL -> SqlExecutionStrategy(createMSSqlConnection(connectionString), SqlProvider.MSSQL)
            | SqlProvider.MySql -> SqlExecutionStrategy(createMySQLConnection(connectionString), SqlProvider.MySql)
            | SqlProvider.SQLite -> SqlExecutionStrategy(createSQLiteConnection(connectionString), SqlProvider.SQLite)
            // Add more cases for other providers...
        new SqlContext(strategy)