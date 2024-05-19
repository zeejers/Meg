![Meg](https://raw.githubusercontent.com/zeejers/Meg/master/images/MEG250.png)

# Meg - Dotnet Migration Tool

Meg is an extremely simple migration command line tool that is inspired by conventions of tools such as rake and ecto. Support for Postgresql, MSSQL, MySql, and SQLite.

- `create` - Creates initial database by name.
- `drop` - Drops a database by name.
- `reset` - Drops and creates a database by name.
- `migrate` - Runs migrations in the specified migration folder. Defaults to folder "Migrations". Migrations are just SQL scripts.
- `gen` - Generates a migration via meg DSL.
- `env` - Print the meg environment to see what your default database connection strings, migration folder, and db provider are.

## Install

```bash
# Global
dotnet tool install --global meg
```

```bash
# Local
dotnet new tool-manifest
dotnet tool install meg
```

## QuickStart

```bash
meg create -d my_new_db -c "Server=localhost;User Id=postgres;Password=postgres;Database=postgres;Port=5432;"

#> Executing db command: SELECT 1 FROM pg_database WHERE datname='my_new_db'
#> Creating DB my_new_db because it doesn't exist yet
#> Database 'my_new_db' created.

meg gen migration AddUsersTable users Id:Guid Name:String Email:String SubscriptionId:Guid:References:Subscriptions:Id

#> Wrote migration Migrations/1716128697_AddUsersTable.SQL


meg migrate -d my_new_db -c "Server=localhost;User Id=postgres;Password=postgres;Database=postgres;Port=5432;"

# > Running Query from Script:
CREATE TABLE "users" (
	"Id" UUID DEFAULT uuid_generate_v4() PRIMARY KEY,
	"Name" VARCHAR(255),
	"Email" VARCHAR(255),
	"SubscriptionId" UUID,
	FOREIGN KEY ("SubscriptionId") REFERENCES Subscriptions("Id")
);
# > Query Result: -1
```

### Env Vars

You can supplement command line usage with some ENV vars in your project to reduce how often you have to enter params in the command line. Having these set will allow them to be used as defaults.

```bash
DB_CONNECTION_STRING # The connection string used by 'create', 'drop' and 'migrate' commands.
DB_PROVIDER # postgres | mssql | mysql | sqlite
MIGRATION_DIRECTORY # directory of your migrations, defaults to "./Migrations"
```

Example using direnv .envrc

```bash
export DB_CONNECTION_STRING="Host=localhost;Database=postgres;Username=postgres;Password=postgres;"
export DB_PROVIDER=postgres # postgres | mssql | mysql | sqlite
export MIGRATION_DIRECTORY=Migrations # directory of your migrations, defaults to "./Migrations"
```

Then you can just call create / migrate without having to specify your connection string.

```bash
meg create -d my_new_db

#> Executing db command: SELECT 1 FROM pg_database WHERE datname='my_new_db'
#> Creating DB my_new_db because it doesn't exist yet
#> Database 'my_new_db' created.

meg migrate -d my_new_db

# > Running Query from Script:
# > CREATE TABLE TODOS (
# >    Id SERIAL PRIMARY KEY,
# >    Name varchar(255),
# >    Text TEXT
# > )
# > Query Result: -1
```

#### Meg Env

```bash
meg env # utility command to print your meg env
# > DB_CONNECTION_STRING: Server=localhost;Port=54322;Database=postgres;User Id=postgres;Password=postgres;
# > DB_PROVIDER: PostgreSQL
# > MIGRATION_DIRECTORY: Migrations
```

# Usage

## Create

```bash
$USAGE: meg create [--help] [--db-name <db name>] [--connection-string <connection string>]
                  [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --db-name, -d <db name>
                          Specify the name of the database to create.
    --connection-string, -c <connection string>
                          Specify the database connection string for the Admin database. Must be able to create DBs
                          with the permissions of the user.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

## Drop

```bash
$USAGE: meg drop [--help] [--db-name <db name>] [--connection-string <connection string>]
                [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --db-name, -d <db name>
                          Specify the name of the database to create.
    --connection-string, -c <connection string>
                          Specify the database connection string for the Admin database. Must be able to create DBs
                          with the permissions of the user.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

## Reset

Drop then create the specified DB

```bash
$USAGE: meg reset [--help] [--db-name <db name>] [--connection-string <connection string>]
[--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --db-name, -d <db name>
                          Specify the name of the database to reset.
    --connection-string, -c <connection string>
                          Specify the database connection string for the Admin database. Must be able to create DBs
                          with the permissions of the user.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

## Migrate

Migrations will only run if they have not already been run based on entries that exist in your schema_migrations table. Migrations are unique by filename, and are executed in alphabetical order ascending. Migrations are SQL scripts that are read from your configured migration directory, either set by the env var `MIGRATION_DIRECTORY` or by cli option `--migration-directory`.

The schema_migrations table will be automatically created the first time you run `meg migrate` successfully.

```bash
$USAGE: meg migrate [--help] [--db-name <db name>] [--connection-string <connection string>] [--migration-directory <migration directory>]
                   [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:
    --db-name, -d <db name>
                          Specify the name of the database to run migrations for.
    --connection-string, -c <connection string>
                          Specify the database connection string for the database. Must be able to create and update
                          tables  with the permissions of the user.
    --migration-directory, -i <migration directory>
                          Specify the directory that contains your order-named migration .SQL files.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

## Gen Migrations

You can generate migrations using a convenient, limited DSL. The output is a SQL script which is written to your migrations directory, either set by the env var `MIGRATION_DIRECTORY` or by cli option `--migration-directory`. You can modify the output SQL after its generated if desired. All SQL is generated assuming you are CREATING a new table.

Each DB provider has different field representations that result from the DSL input. The `gen migration` command will generate field mappings based on the `DB_PROVIDER` which you have set either as an env var or passed in as the `--provider`. To see the definition of all these mappings, set `DB_PROVIDER` and the `meg gen migration --help` command will then update with the corresponding resulting schema mappings. 

Example command with break down:
`meg gen migration AddPostsTable posts Id:Guid Name:String UserId:Guid:References:Users:Id`

`meg gen migration` - command base
`AddPostsTable` - migration file name
`posts` - table name
`Id:Guid Name:String UserId:Guid:References:Users:Id` - schema definition

> ⚠️ **WARNING:** If you are using Guid field with PostgreSQL, make sure you've installed the required extension to support generating uuids using `CREATE EXTENSION IF NOT EXISTS "uuid-ossp";`.


```bash
$USAGE: meg gen migration [--help] [--provider <postgresql|mssql|mysql|sqlite>]
                         [--migration-directory <migration directory>]
                         [<string>...]

SCHEMA DEFINITION:

    <string>...           Specify the schema definition of the migration in
                          this format:
                          -----------
                          meg gen migration AddUsersTable users Id:Id
                          Name:String Description:Text
                          -----------
                          List of all schema types for PostgreSQL (see your
                          provider by setting DB_PROVIDER) :
                          Id              an integer autoincrementing primary
                          key. Maps to INTEGER.
                          Guid            Sane default (uuid PK) for dotnet
                          guids. Maps to UUID.
                          Integer         Maps to INTEGER.
                          Float           Maps to FLOAT.
                          Numeric         Maps to NUMERIC.
                          Boolean         Maps to BOOLEAN.
                          String          Maps to VARCHAR(255).
                          Text            Maps to TEXT.
                          Binary          Maps to BYTEA.
                          Array           Maps to JSONB.
                          Record          Maps to JSONB.
                          Date            Maps to DATE.
                          Time            Maps to TIME.
                          DateTime        Timestamp without TZ. Maps to
                          TIMESTAMP.
                          DateTimeTz      Timestamp with TZ. Maps to TIMESTAMP.
                          -- References --
                          You can include references using
                          [ColumnName]:[ColumnType]:References:[ReferenceTableNa
                          me]:[ReferenceColumnName]
                          For example
                          -----------
                          user_id:int:references:users:id
                          -----------
                          -- Additional Properties --
                          You can include additional properties to a non
                          reference field using a similar syntax
                          [ColumnName]:[ColumnType]:[ADDITIONAL PROPERTIES]
                          For example
                          -----------
                          "username:string:unique not null"
                          -----------


OPTIONS:

    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --migration-directory, -o <migration directory>
                          The output directory to write migrations to. When not
                          specified, the resulting SQL is written to env var
                          MIGRATION_DIRECTORY value, defaults to 'Migrations'
    --help                display this list of options.

```