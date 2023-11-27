# MEG - DotNet Migration Tool (WIP)

Meg is an extremely simple migration command line tool that is inspired by conventions of tools such as rake and ecto. Support for Postgresql, MSSQL, MySql, and SQLite.

- `create` - Creates initial database by name.
- `drop` - Drops a database by name.
- `migrate` - Runs migrations in the specified migration folder. Defaults to folder "Migrations". Migrations are just SQL scripts.
- `gen` - Generates a migration via DSL. TODO.


## Install

```bash
# Global
dotnet tool install --global meg

meg --help
meg create --help
meg migrate --help
meg drop --help
```

```bash
# Local
dotnet new tool-manifest
dotnet tool install meg
```

# Parameters

## Create

```bash
$USAGE: meg create [--help] [--dbname <db name>]
                  [--connectionstring <connection string>]
                  [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --dbname, -d <db name>
                          Specify the name of the database to create.
    --connectionstring, -c <connection string>
                          Specify the database connection string for the Admin
                          database. Must be able to create DBs with the
                          permissions of the user.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

## Drop

```bash
$USAGE: meg drop [--help] [--dbname <db name>]
                [--connectionstring <connection string>]
                [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --dbname, -d <db name>
                          Specify the name of the database to create.
    --connectionstring, -c <connection string>
                          Specify the database connection string for the Admin
                          database. Must be able to create DBs with the
                          permissions of the user.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

## Migrate

```bash
$USAGE: meg migrate [--help] [--connectionstring <connection string>]
                   [--migrationdirectory <migration directory>]
                   [--provider <postgresql|mssql|mysql|sqlite>]

OPTIONS:

    --connectionstring, -c <connection string>
                          Specify the database connection string for the
                          database. Must be able to create and update tables
                          with the permissions of the user.
    --migrationdirectory, -i <migration directory>
                          Specify the directory that contains your order-named
                          migration .SQL files.
    --provider, -p <postgresql|mssql|mysql|sqlite>
                          Specify the database provider.
    --help                display this list of options.
```

# Env Vars
You can supplement command line usage with some ENV vars in your project to reduce how often you have to enter params in the command line. Having these set will allow them to be used as defaults.

```bash
DB_CONNECTION_STRING
DB_NAME 
DB_PROVIDER
MIGRATION_DIRECTORY
```