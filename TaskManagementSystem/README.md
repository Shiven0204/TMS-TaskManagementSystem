# Task Management System

A server-rendered ASP.NET Core MVC application for managing users, tasks, updates, files, reminders, and reports.

## Requirements

- .NET SDK 10.0.100 or later
- Visual Studio 2022 (17.14 or later) or VS Code with the C# Dev Kit

The project targets `net10.0` and uses Entity Framework Core 10.

## Run locally

From the directory containing `TaskManagementSystem.csproj`:

```powershell
dotnet restore
dotnet build
dotnet run
```

The development launch profile listens on:

- HTTP: http://localhost:5209
- HTTPS: https://localhost:7059

Open either address in a browser. The default local database is SQLite and is
created at `taskmanagement.db` on first start.

The application creates a development administrator when the database does not
already contain `admin2`:

| Username | Password |
| --- | --- |
| `admin2` | `admin1234` |

Change this password before using the application outside local development.

## Configuration

The default configuration in `appsettings.json` is:

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=taskmanagement.db"
  }
}
```

To use SQL Server, set `DatabaseProvider` to `SqlServer` and provide a valid
`ConnectionStrings:DefaultConnection` value. Environment variables use the
double-underscore form:

```powershell
$env:DatabaseProvider = "SqlServer"
$env:ConnectionStrings__DefaultConnection = "Server=localhost;Database=TaskManagementSystem;Trusted_Connection=True;TrustServerCertificate=True"
dotnet run
```

The SQL Server provider applies EF Core migrations at startup. SQLite uses
`EnsureCreated` to remain compatible with the existing local database file.
Delete `taskmanagement.db` only when you intentionally want to reset local
data.

## Features

- User registration, login, logout, and profile pages
- Role-aware task dashboard for administrators, team leads, and staff
- Task creation, editing, status tracking, reminders, updates, and file attachments
- Administrative user management, settings, and reports
- SQLite for zero-configuration local development, with SQL Server support
