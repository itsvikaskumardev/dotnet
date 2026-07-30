# ASP.NET Core Minimal API Project Roadmap

This project is built using **ASP.NET Core Minimal APIs** and follows a **feature-based architecture**. These notes are designed to help you understand every concept from the basics to advanced topics while building a real-world REST API.

Each section builds on the previous one, making it easy to learn and implement concepts step by step.

---

# Section 1: Introduction to Minimal APIs

## 1.1 Create the Project

Create a new ASP.NET Core Minimal API project.

Using the .NET CLI:

```bash
dotnet new web -n WebMinimalExample
```

Or create an **ASP.NET Core Empty** project using Visual Studio.

---

## 1.2 Project Structure

```
WebMinimalExample
│
├── Data
├── DTOs
├── Endpoints
├── Mapping
├── Migrations
├── Models
├── Program.cs
└── appsettings.json
```

### Folder Responsibilities

| Folder     | Purpose                           |
| ---------- | --------------------------------- |
| Models     | Domain models (database entities) |
| DTOs       | Data Transfer Objects             |
| Data       | EF Core DbContext                 |
| Endpoints  | Minimal API endpoint definitions  |
| Mapping    | AutoMapper profiles               |
| Migrations | Entity Framework Core migrations  |

---

## 1.3 What is a Minimal API?

A **Minimal API** is a lightweight approach to building REST APIs in ASP.NET Core without creating controllers.

Instead of writing:

```csharp
public class CategoryController : ControllerBase
{
}
```

You simply write:

```csharp
app.MapGet("/categories", () =>
{
    return Results.Ok();
});
```

This reduces boilerplate code while making APIs easier to build and maintain.

---

## 1.4 Why Use Minimal APIs?

| Minimal API                  | Controller API                    |
| ---------------------------- | --------------------------------- |
| Less code                    | More code                         |
| Lightweight                  | More structured                   |
| Faster startup               | Slightly slower startup           |
| Great for microservices      | Great for large MVC applications  |
| Uses `MapGet()`, `MapPost()` | Uses Controllers                  |
| Beginner-friendly            | Traditional ASP.NET Core approach |

---

## 1.5 Request Flow

```
Client
   │
   ▼
Minimal API Endpoint
   │
   ▼
Business Logic
   │
   ▼
DbContext
   │
   ▼
Database
```

---

## 1.6 Program.cs

Every ASP.NET Core application starts with **Program.cs**.

It is the application's entry point where we configure:

* Services
* Dependency Injection
* Database
* Authentication
* Authorization
* Middleware
* Routing
* API Endpoints

---

# Section 2: Scalar and Routing

In this section, we'll learn:

* Installing Scalar
* MapGet()
* MapPost()
* MapPut()
* MapDelete()
* Route Parameters
* Query Parameters
* Request Body
* IResult
* TypedResults
* Route Groups

Each topic includes:

* What it is
* Why we use it
* How it works
* Complete code examples
* Request flow
* Interview questions

---

# Section 3: Entity Framework Core

Topics covered:

* Installing EF Core packages
* Configuring DbContext
* Connection Strings
* PostgreSQL Integration
* Entity Classes
* Migrations
* Seed Data

---

# Section 4: Building API Endpoints

Build complete CRUD APIs while learning:

* Async Programming
* Validation
* IResult
* TypedResults
* Produces()
* Accepts()

---

# Section 5: DTOs and AutoMapper

### Flow

```
Client
   │
   ▼
Request DTO
   │
   ▼
AutoMapper
   │
   ▼
Domain Model
   │
   ▼
Database
```

Topics include:

* DTOs
* Request DTOs
* Response DTOs
* AutoMapper
* Mapping Profiles

---

# Section 6: Organizing Endpoints

Instead of placing every endpoint inside **Program.cs**, we'll organize them by feature.

```
Endpoints
│
├── CategoryEndpoints.cs
├── ProductEndpoints.cs
└── UserEndpoints.cs
```

Each feature remains modular, maintainable, and easier to scale.

---

# Section 7: Standard API Responses

Instead of returning inconsistent responses, every endpoint will return a common response structure.

Example:

```json
{
  "success": true,
  "message": "Category created successfully",
  "data": {}
}
```

This provides a consistent API experience for clients.

---

# Section 8: Authentication

Topics covered:

* User Entity
* User Registration
* Login
* JWT Authentication
* Claims
* Roles
* Authorization
* Scalar JWT Authentication

### Authentication Flow

```
Login
   │
   ▼
JWT Generated
   │
   ▼
Client Stores Token
   │
   ▼
Client Sends Token
   │
   ▼
API Validates Token
   │
   ▼
Protected Endpoint
```

---

# Section 9: Validation and File Upload

Topics include:

* FluentValidation
* Custom Validators
* Image Upload
* Saving Images
* Returning Image URLs

---

# Section 10: API Versioning

Support multiple API versions:

* v1
* v2
* v3

Versioning strategies:

* URL Versioning
* Query String Versioning
* Header Versioning

---

# Section 11: Endpoint Filters and CORS

Topics covered:

* Endpoint Filters
* Global Filters
* Logging
* Exception Handling
* CORS

### Request Flow

```
Request
   │
   ▼
Endpoint Filter
   │
   ▼
Endpoint
   │
   ▼
Response Filter
   │
   ▼
Client
```

---

# Section 12: ASP.NET Core Identity

Topics include:

* IdentityDbContext
* Identity Tables
* UserManager
* SignInManager
* Password Hashing
* Roles
* Claims
* JWT Authentication

### Identity Flow

```
Register
   │
   ▼
Identity User Created
   │
   ▼
Password Hashed
   │
   ▼
Saved to Database
   │
   ▼
Login
   │
   ▼
JWT Generated
   │
   ▼
Authorized API
```

---

# Learning Approach

Every topic in this roadmap follows the same structure:

1. What is it?
2. Why do we use it?
3. How does it work?
4. Step-by-step implementation
5. Complete code examples
6. Flow diagrams
7. Best practices
8. Common mistakes
9. Interview questions
10. Summary

This consistent format makes the notes easy to revise before interviews and practical to follow while building a complete ASP.NET Core Minimal API project from scratch.
