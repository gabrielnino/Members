# Autodesk Members Platform

[![.NET](https://img.shields.io/badge/.NET-8%2F9-blue)](https://dotnet.microsoft.com/)

A sample application to manage members, built with .NET 8/9, Entity Framework Core, SQLite, and Blazor WebAssembly.

## Table of Contents

- [Features](#features)
- [Prerequisites](#prerequisites)
- [Setup](#setup)
  - [Clone Repository](#clone-repository)
  - [Database Migration](#database-migration)
  - [Run API](#run-api)
  - [Run Client](#run-client)
- [API Documentation](#api-documentation)
- [Testing](#testing)
- [Docker](#docker)
- [Architecture](#architecture)
- [Caching Strategy](#caching-strategy)
- [Contact](#contact)

## Features

- **User CRUD**: Create, read, update, and delete users via a REST API.
- **Cursor Pagination**: Efficient paging with `PagedResult<T>`.
- **Caching**: In-memory caching for repeated read operations.
- **Blazor WebAssembly**: Interactive front-end for member management.
- **Swagger / OpenAPI**: Auto-generated API documentation and testing UI.

## Prerequisites

- [.NET 8 SDK or .NET 9 SDK](https://dotnet.microsoft.com/download)
- [SQLite](https://www.sqlite.org/download.html) (optional, for direct DB inspection)
- Git
- IDE or code editor (Visual Studio, VS Code)

## Setup

### Clone Repository

```bash
git clone https://github.com/gabrielnino/Autodesk.MembersPlatform.git
cd Autodesk.MembersPlatform
```

### Database Migration

Apply EF Core migrations:

```bash
cd Autodesk.Api
dotnet ef database update
```

### Run API

Start the REST API:

```bash
cd Autodesk.Api
dotnet run
```

> **URL:** `https://localhost:7134`

### Run Client

In a new terminal, launch the Blazor client:

```bash
cd Autodesk.Members.Portal
dotnet run
```

> **URL:** `https://localhost:5001`

## API Documentation

Open the Swagger UI in your browser:

```text
https://localhost:7134/swagger/index.html
```

## Testing

If unit tests exist, run:

```bash
cd <test-project-folder>
dotnet test
```

## Architecture

- **Domain** (`Autodesk.Domain`): Core entities (e.g., `User`).
- **Application**: Use-case logic and pagination.
- **Infrastructure**: EF Core implementations and error strategies.
- **Persistence**: `DataContext`, migrations, and table configuration.
- **API** (`Autodesk.Api`): ASP.NET Core controllers for user operations.
- **Portal** (`Autodesk.Members.Portal`): Blazor WebAssembly client.

## Caching Strategy

The `UserRead.GetUsersPage` method uses `IMemoryCache` to store paged results for 5 minutes, reducing database load on repeated queries.

## Contact

For questions or feedback, email: `gabrielnino@gmail.com`
