

## Step 1: Create a New Project

1. Open **Visual Studio**.
2. Create a new **.NET Web API / Minimal API** project.
3. Open **Solution Explorer** and verify that the project has been created successfully.

## Step 2: Install Required NuGet Packages

Install the following NuGet packages:

| Package                                 | Purpose                                  |
| --------------------------------------- | ---------------------------------------- |
| `Microsoft.EntityFrameworkCore`         | Entity Framework Core                    |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | PostgreSQL database provider             |
| `Microsoft.EntityFrameworkCore.Tools`   | EF Core migrations and database commands |
| `Microsoft.EntityFrameworkCore.Design`  | EF Core design-time features             |

You can install them from:

**Solution Explorer → Right-click Project → Manage NuGet Packages**

## Step 3: Add Scalar API Documentation

Install the Scalar package then add 
app.MapScalarApiReference();

### Open Scalar Automatically When Running the Project

To automatically open Scalar when you start the project from the terminal, add the following inside the development environment condition:

## Step 4: Create the Database Context

Create a **Data** folder in the project.

Inside the `Data` folder, create:  ApplicationDbContext.cs

Add your entity `DbSet`s inside the context when you create your models.

For example:

```csharp
public DbSet<Student> Students { get; set; }
```

### Add Connection String in `appsettings.json`

Add your PostgreSQL connection string:

### Register `ApplicationDbContext` in `Program.cs`

## Step 5: Create the Database Using Package Manager Console

Open:

**Tools → NuGet Package Manager → Package Manager Console**

Then run:

```powershell
Update-Database
```

If you have not created a migration yet, first run:

```powershell
Add-Migration InitialCreate
```

Then:

```powershell
Update-Database
```

### What Happens?

`Update-Database` reads the connection string from `appsettings.json` and connects to PostgreSQL.

It then creates/updates the database specified in:

```json
"Database=YourDatabase"
```

It also creates the required tables based on your EF Core models and migrations.
---

step 6: create a entity inside model foleder lie student ,teacher entolmet departet 
add them into appicaitndbcontex inside data Dbset<.
step 7: add migratin update databse >