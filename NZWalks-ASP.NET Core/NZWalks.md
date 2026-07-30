# C# ASP.NET Core Web API Setup Guide

A step-by-step guide for creating an ASP.NET Core Web API using Entity Framework Core, PostgreSQL, JWT Authentication, API Versioning, Logging, and more.

---

## Table of Contents

1. Project Setup
2. Create an Entity
3. Install Required Packages
4. Understanding DbContext
5. Configure Database Connection
6. Dependency Injection
7. Entity Framework Core Migrations
8. Create API Controllers
9. DTO (Data Transfer Object)
10. Repository Pattern
11. JWT Authentication & ASP.NET Core Identity
12. Swagger JWT Authentication
13. Logging & Global Exception Handling
14. API Versioning

---

# 1. Project Setup

## Open the Project File

Open your `.csproj` file from **Solution Explorer**.

- Right-click the project
- Select **Edit Project File**

---

# 2. Create an Entity

Inside the **Models** folder, create your entity.

Example:

- Student
- Product
- Category
- Walk

---

# 3. Install Required NuGet Packages

Install the following packages:

| Package | Purpose |
|---------|----------|
| Microsoft.EntityFrameworkCore | EF Core |
| Npgsql.EntityFrameworkCore.PostgreSQL | PostgreSQL Provider |
| Microsoft.EntityFrameworkCore.Tools | Migrations |
| Microsoft.EntityFrameworkCore.Design | Design-time Features |

---

# 4. Understanding DbContext

`DbContext` acts as a bridge between your application and the database.

It is responsible for:

- Database connection
- Tracking entity changes
- CRUD operations
- Mapping entities to database tables

---

# 5. Configure Database Connection

Add a connection string inside **appsettings.json**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=studentdb;Username=postgres;Password=yourpassword"
  }
}
```

---

# 6. Dependency Injection (DI)

Dependency Injection helps build loosely coupled and testable applications.

## Register Services

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

## Register DbContext

```csharp
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));
```

## Benefits

- Loose coupling
- Better testing
- Easier maintenance
- Automatic dependency creation

---

# 7. Entity Framework Core Migrations

## Create Migration

```powershell
Add-Migration InitialCreate
```

Examples:

```powershell
AddStudentTable
CreateWalkTables
AddCategoryEntity
```

## Apply Migration

```powershell
Update-Database
```

## Migration Workflow

```text
Entity
    │
    ▼
Add-Migration
    │
    ▼
Migration Files
    │
    ▼
Update-Database
    │
    ▼
Database Created
```

---

# 8. Create API Controller

1. Right-click **Controllers**
2. Add → Controller
3. Select **API Controller - Empty**
4. Add GET, POST, PUT and DELETE actions

---

# 9. DTO (Data Transfer Object)

DTOs transfer only the required data between the client and server.

## Why Use DTOs?

- Hide sensitive information
- Reduce payload size
- Separate API contracts from domain models
- Improve security

### Data Flow

```text
Client
   │
   ▼
DTO
   │
   ▼
Controller
   │
   ▼
Domain Model
   │
   ▼
Database
```

---

# 10. Repository Pattern

The Repository Pattern separates business logic from data access.

### Request Flow

```text
Controller
    │
    ▼
Repository
    │
    ▼
DbContext
    │
    ▼
Database
```

## Benefits

- Loose coupling
- Better testing
- Easier maintenance
- Database abstraction
- Supports multiple database providers

---

# 11. JWT Authentication & ASP.NET Core Identity

## Install Packages

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer

dotnet add package Microsoft.IdentityModel.Tokens

dotnet add package System.IdentityModel.Tokens.Jwt

dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

## Configure JWT

Add JWT settings inside:

```
appsettings.json
```

Configure authentication in:

```
Program.cs
```

```csharp
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
    });
```

Register Identity

```csharp
builder.Services
    .AddIdentityCore<IdentityUser>()
    .AddEntityFrameworkStores<AppAuthDbContext>();
```

Enable middleware

```csharp
app.UseAuthentication();

app.UseAuthorization();
```

> Always call `UseAuthentication()` before `UseAuthorization()`.

## Identity Migration

```bash
dotnet ef migrations add AddingIdentityTables --context AppAuthDbContext
```

Update database

```bash
dotnet ef database update --context AppAuthDbContext
```

Identity automatically creates:

- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetRoleClaims
- AspNetUserLogins
- AspNetUserTokens

---

# 12. Configure Swagger for JWT

Register Swagger

```csharp
builder.Services.AddSwaggerGen();
```

Configure:

- OpenApiSecurityScheme
- AddSecurityDefinition()
- AddSecurityRequirement()

## Using Swagger

1. Run the application
2. Open Swagger UI
3. Click **Authorize**
4. Enter

```text
Bearer your_jwt_token
```

5. Call protected APIs

---

# 13. Logging & Global Exception Handling

## Logging

Inject ILogger

```csharp
private readonly ILogger<StudentController> _logger;
```

Example

```csharp
_logger.LogInformation("Fetching students");
```

## Serilog Packages

```bash
dotnet add package Serilog.AspNetCore

dotnet add package Serilog.Sinks.Console

dotnet add package Serilog.Sinks.File
```

---

## Global Exception Handling

Register services

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();
```

Enable middleware

```csharp
app.UseExceptionHandler();
```

---

# 14. API Versioning

API Versioning allows multiple versions of an API while maintaining backward compatibility.

## Install Package

```bash
dotnet add package Microsoft.AspNetCore.Mvc.Versioning
```

## Configure Versioning

```csharp
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
});
```

## Versioning Styles

### URL Versioning

```
/api/v1/students

/api/v2/students
```

### Query String Versioning

```
/api/students?api-version=1.0
```

### Header Versioning

```
api-version: 1.0
```

## Benefits

- Backward compatibility
- Multiple API versions
- Smooth migration
- Long-term API maintenance

---

# Next Steps

After completing this setup, you can continue with:

- CRUD APIs
- Validation
- AutoMapper
- Authorization
- Role-Based Authentication
- Refresh Tokens
- Pagination
- Filtering
- Sorting
- Minimal APIs
- Clean Architecture
- CQRS
- MediatR
- Unit Testing
- Docker
- Deployment

---

## Author

Created for learning **ASP.NET Core Web API** with:

- C#
- ASP.NET Core
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- Swagger
- API Versioning
- Serilog