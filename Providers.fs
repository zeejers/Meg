module Meg.Providers

open Npgsql

type SqlProvider =
    | PostgreSQL
    | MSSQL
    | MySql
    | SQLite

type SqlProviderConnection =
    | PostgreSQL of NpgsqlConnection
    | NotImplemented

type ISqlExecutionStrategy =
    abstract member CreateConnection: unit -> SqlProviderConnection
    abstract member ExecuteQuery: string -> obj
    abstract member ExecuteNonQuery: string -> obj


type PostgreSqlStrategy(connectionString: string) =
    interface ISqlExecutionStrategy with
        member this.CreateConnection() : SqlProviderConnection =
            try
                SqlProviderConnection.PostgreSQL(new NpgsqlConnection(connectionString))
            with ex ->
                failwith $"Unable to create SQL connection: {ex.Message}"

        member this.ExecuteQuery(query: string) =
            let connection =
                match (this :> ISqlExecutionStrategy).CreateConnection() with
                | PostgreSQL conn -> conn
                | _ -> failwith "Failed to execute query, unable to create connection."

            connection.Open()
            use command = new NpgsqlCommand(query, connection)
            let queryResult = command.ExecuteScalar()

            if (connection.State = System.Data.ConnectionState.Open) then
                connection.Close()

            queryResult


        member this.ExecuteNonQuery(query: string) =
            let connection =
                match (this :> ISqlExecutionStrategy).CreateConnection() with
                | PostgreSQL conn -> conn
                | _ -> failwith "Failed to execute query, unable to create connection."

            connection.Open()
            use command = new NpgsqlCommand(query, connection)
            let commandResult = command.ExecuteNonQuery()

            if (connection.State = System.Data.ConnectionState.Open) then
                connection.Close()

            commandResult


type MSSqlStrategy(connectionString: string) =
    interface ISqlExecutionStrategy with
        member _.ExecuteQuery(query: string) = ()
        member _.ExecuteNonQuery(query: string) = ()
        member _.CreateConnection() = SqlProviderConnection.NotImplemented


type MySqlStrategy(connectionString: string) =
    interface ISqlExecutionStrategy with
        member _.ExecuteQuery(query: string) = ()
        member _.ExecuteNonQuery(query: string) = ()
        member _.CreateConnection() = SqlProviderConnection.NotImplemented

type SQLiteStrategy(connectionString: string) =
    interface ISqlExecutionStrategy with
        member _.ExecuteQuery(query: string) = ()
        member _.ExecuteNonQuery(query: string) = ()
        member _.CreateConnection() = SqlProviderConnection.NotImplemented

type SqlContext(strategy: ISqlExecutionStrategy) =
    member this.ExecuteNonQuery(sql: string) = strategy.ExecuteNonQuery(sql)
    member this.ExecuteQuery(query: string) = strategy.ExecuteQuery(query)
    member this.CreateConnection() = strategy.CreateConnection()

    static member Create(provider: SqlProvider, connectionString: string) =
        match provider with
        | SqlProvider.PostgreSQL -> SqlContext(PostgreSqlStrategy(connectionString))
        | SqlProvider.MSSQL -> SqlContext(MSSqlStrategy(connectionString))
        | SqlProvider.MySql -> SqlContext(MySqlStrategy(connectionString))
        | SqlProvider.SQLite -> SqlContext(SQLiteStrategy(connectionString))
