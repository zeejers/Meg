# MEG - DotNet Migration Tool (WIP)

Meg is an extremely simple migration command line tool that is inspired by conventions of tools such as rake and ecto. Support for Postgresql, MSSQL, MySql, and SQLite.

- `create` - Creates initial database by name.
- `drop` - Drops a database by name.
- `migrate` - Runs migrations in the specified migration folder. Defaults to folder "Migrations". Migrations are just SQL scripts.
- `gen` - Generates a migration via DSL. TODO.
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

meg migrate -c "Server=localhost;User Id=postgres;Password=postgres;Database=my_new_db;Port=5432;"

# > Running Query from Script:
# > CREATE TABLE TODOS (
# >    Id SERIAL PRIMARY KEY,
# >    Name varchar(255),
# >    Text TEXT
# > )
# > Query Result: -1
```

### Env Vars

You can supplement command line usage with some ENV vars in your project to reduce how often you have to enter params in the command line. Having these set will allow them to be used as defaults.

```bash
DB_INITIAL_CONNECTION_STRING # The connection string used by 'create' and 'drop' commands.
DB_MIGRATION_CONNECTION_STRING # The connection string for your created DB. Different from initial connection string because the admin db / user / password is almost definitely not the same as the app specific db / user / password
DB_PROVIDER # postgres | mssql | mysql | sqlite
MIGRATION_DIRECTORY # directory of your migrations, defaults to "./Migrations"
```

Example using direnv .envrc

```bash
export DB_INITIAL_CONNECTION_STRING="Host=localhost;Database=postgres;Username=postgres;Password=postgres;"
export DB_MIGRATION_CONNECTION_STRING="Host=localhost;Database=my_new_db;User Id=postgres;Password=postgres;"
export DB_PROVIDER=postgres # postgres | mssql | mysql | sqlite
export MIGRATION_DIRECTORY=Migrations # directory of your migrations, defaults to "./Migrations"
```

Then you can just call create / migrate without having to specify your connection string.

```bash
meg create -d my_new_db

#> Executing db command: SELECT 1 FROM pg_database WHERE datname='my_new_db'
#> Creating DB my_new_db because it doesn't exist yet
#> Database 'my_new_db' created.

meg migrate

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
# > DB_INITIAL_CONNECTION_STRING: Server=localhost;Port=54322;Database=postgres;User Id=postgres;Password=postgres;
# > DB_MIGRATION_CONNECTION_STRING: Server=localhost;Port=54322;Database=my_new_db;User Id=postgres;Password=postgres;
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

## Migrate

```bash
$USAGE: meg migrate [--help] [--connection-string <connection string>] [--migration-directory <migration directory>]
                   [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --connection-string, -c <connection string>
                          Specify the database connection string for the database. Must be able to create and update
                          tables  with the permissions of the user.
    --migration-directory, -i <migration directory>
                          Specify the directory that contains your order-named migration .SQL files.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```
