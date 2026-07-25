using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NZWalks_ASP.NET_Core.Data;
using NZWalks_ASP.NET_Core.Models.Domain;
using NZWalks_ASP.NET_Core.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;

namespace NZWalks_ASP.NET_Core.Controllers
{
    // https:localhost:1234/api/
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize]
    public class RegionsController : ControllerBase
    {
        private readonly NZWalksDbContext dbContext;
        private readonly ILogger<RegionsController> logger;

        public RegionsController(NZWalksDbContext dbContext, ILogger<RegionsController> logger)
        {
            this.dbContext = dbContext;
            this.logger = logger;
        }

        // GET ALL  REGION (Get All Region )
        [HttpGet]
        //[Authorize(Roles = "Reader")]
        public async Task<IActionResult> GetAll()
        {

            logger.LogInformation("Get All Region Action Method was Invoked ");

            // Get data from the database (Domain Models)
            var regionsDomain = await dbContext.Regions.ToListAsync(); // With Entity Framework

            // Without Entitiy Framework we have to write a raw sql query :SELECT * FROM Regions;

            // Map Domain Models to DTOs
            // Never return Domain Models directly to the client.
            var regionsDto = new List<RegionDto>();

            foreach (var region in regionsDomain)
            {
                regionsDto.Add(new RegionDto
                {
                    Id = region.Id,
                    Code = region.Code,
                    Name = region.Name,
                    RegionImageUrl = region.RegionImageUrl
                });
            }

            //logger.LogInformation($"Finished Get All Region request with data :{JsonSerializer.Serialize{regionsDomai}");

            return Ok(regionsDto);
        }


        // GET SINGLE REGION (Get Region By ID)

        [HttpGet]
        [Route("{id:guid}")]
        //[Authorize(Roles = "Reader")]

        public async Task<IActionResult> GetById([FromRoute] Guid id)
        {
            // Get the region from the database
            var regionDomain = await dbContext.Regions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (regionDomain == null)
            {
                return NotFound();
            }

            // Map Domain Model to DTO
            var regionDto = new RegionDto
            {
                Id = regionDomain.Id,
                Code = regionDomain.Code,
                Name = regionDomain.Name,
                RegionImageUrl = regionDomain.RegionImageUrl
            };

            return Ok(regionDto);
        }

        // POST: Create a New Region

        [HttpPost]
        //[Authorize(Roles = "Writer")]

        public async Task<IActionResult> Create([FromBody] AddRegionRequestDto addRegionRequestDto)
        {

            // Map DTO to Domain Model
            var regionDomainModel = new Region
            {
                Code = addRegionRequestDto.Code,
                Name = addRegionRequestDto.Name,
                RegionImageUrl = addRegionRequestDto.RegionImageUrl
            };

            // Add the entity to the database
            await dbContext.Regions.AddAsync(regionDomainModel);

            // Save changes
            await dbContext.SaveChangesAsync();

            // Map Domain Model back to DTO
            var regionDto = new RegionDto
            {
                Id = regionDomainModel.Id,
                Code = regionDomainModel.Code,
                Name = regionDomainModel.Name,
                RegionImageUrl = regionDomainModel.RegionImageUrl
            };

            // Return HTTP 201 Created
            return CreatedAtAction(
                nameof(GetById),
                new { id = regionDto.Id },
                regionDto);
        }

        // PUT: Update a Region

        [HttpPut]
        [Route("{id:guid}")]
        //[Authorize(Roles = "Writer")]

        public async Task<IActionResult> Update(
            [FromRoute] Guid id,
            [FromBody] UpdateRegionRequestDto updateRegionRequestDto)
        {
            // Check if the region exists
            var regionDomainModel = await dbContext.Regions
                .FirstOrDefaultAsync(x => x.Id == id);

            if (regionDomainModel == null)
            {
                return NotFound();
            }

            // Map DTO to Domain Model
            regionDomainModel.Name = updateRegionRequestDto.Name;
            regionDomainModel.RegionImageUrl = updateRegionRequestDto.RegionImageUrl;

            // Save changes
            await dbContext.SaveChangesAsync();

            // Map Domain Model to DTO
            var regionDto = new RegionDto
            {
                Id = regionDomainModel.Id,
                Code = regionDomainModel.Code,
                Name = regionDomainModel.Name,
                RegionImageUrl = regionDomainModel.RegionImageUrl
            };

            return Ok(regionDto);
        }


        // DELETE: Delete a Region

        [HttpDelete]
        [Route("{id:guid}")]
        //[Authorize(Roles = "Writer")]

        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            // Find the region by Id
            var regionDomainModel = await dbContext.Regions.FindAsync(id);

            // Check if the region exists
            if (regionDomainModel == null)
            {
                return NotFound();
            }

            // Remove the region
            dbContext.Regions.Remove(regionDomainModel);

            // Save changes
            await dbContext.SaveChangesAsync();

            // Map Domain Model to DTO
            var regionDto = new RegionDto
            {
                Id = regionDomainModel.Id,
                Code = regionDomainModel.Code,
                Name = regionDomainModel.Name,
                RegionImageUrl = regionDomainModel.RegionImageUrl
            };

            // Return the deleted region
            return Ok(regionDto);
        }

    }
}

/*
 
 
 
 
 | `Find(id)`                                                                                                               | `FirstOrDefault(x => x.Id == id)`                                                                  |
| ------------------------------------------------------------------------------------------------------------------------ | -------------------------------------------------------------------------------------------------- |
| Searches by **primary key only**.                                                                                        | Searches using **any condition**.                                                                  |
| Very fast for primary key lookups.                                                                                       | Executes the LINQ query you provide.                                                               |
| Checks EF Core's **change tracker** first. If the entity is already loaded, it returns it without querying the database. | Always translates the LINQ query into SQL (unless the query is already being evaluated in memory). |
| Can only search by the entity's primary key.                                                                             | Can search by any property or combination of properties.                                           |
| Simpler syntax.                                                                                                          | More flexible syntax.                                                                              |

 
 
 -----------------------------------------------------------------------------

What is Task?
Yes. Task is the return type of an asynchronous method. 
A Task represents work that is currently running or will finish in the future.

Before async---------------

A normal method executes completely and returns the final result.Here, the return type is: IActionResult

It means:"This method will return an IActionResult immediately."

After async--------------

Now the return type is: Task<IActionResult>

This means: "This method will eventually return an IActionResult, but it may need to wait for an asynchronous operation to complete first."
 


-------------------------------------------------------------------------------------------------------------------------------------------------------------

Entity Framework (more commonly **Entity Framework Core** or **EF Core**) is **not** a controller, a function, or a design pattern.

It is an **ORM (Object-Relational Mapper)**.

Its job is to **convert C# objects into database records and database records into C# objects**.

Without an ORM, you write SQL manually. : SELECT * FROM Products;

With an ORM, you work with C# objects, and the ORM generates the SQL for you. Eg; var products = await dbContext.Products.ToListAsync();


and converts the results into a list of `Product` objects.





    ---

    # DbContext

    The main class in EF Core is `DbContext`.

    ```csharp
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }
    }
    ```

    Think of `DbContext` as your application's connection to the database.

    ---

# Insert Data

Instead of SQL:

```sql
INSERT INTO Products(Name, Price)
VALUES ('Laptop', 50000);
```

You write:

```csharp
var product = new Product
{
    Name = "Laptop",
    Price = 50000
};

dbContext.Products.Add(product);
await dbContext.SaveChangesAsync();
```

EF Core generates the `INSERT` statement automatically.

---

# Read Data

Instead of:

```sql
SELECT * FROM Products;
```

You write:

```csharp
var products = await dbContext.Products.ToListAsync();
```

---

# Update Data

Instead of:

```sql
UPDATE Products
SET Price = 60000
WHERE Id = 1;
```

You write:

```csharp
var product = await dbContext.Products.FindAsync(1);

product.Price = 60000;

await dbContext.SaveChangesAsync();
```

EF Core generates the `UPDATE` statement.

---

# Delete Data

Instead of:

```sql
DELETE FROM Products
WHERE Id = 1;
```

You write:

```csharp
var product = await dbContext.Products.FindAsync(1);

dbContext.Products.Remove(product);

await dbContext.SaveChangesAsync();
```

---

# How does it fit into an ASP.NET Core application?

```text
User
   │
HTTP Request
   │
Controller
   │
Service / CQRS Handler
   │
Entity Framework Core (DbContext)
   │
SQL Database
```

Notice that **Entity Framework is below the Controller**.

The controller never talks directly to the database. It usually calls a service or CQRS handler, which uses `DbContext` to access the database.

---

# Real-life analogy

Imagine a restaurant:

```text
Customer
     │
     ▼
Waiter (Controller)
     │
     ▼
Chef (Service / Handler)
     │
     ▼
Translator (Entity Framework)
     │
     ▼
Kitchen (Database)
```

* The **Controller** receives the request.
* The **Service/Handler** decides what should happen.
* **Entity Framework** translates C# operations into SQL.
* The **Database** stores the data.

---

# Is Entity Framework a design pattern?

**No.**

It is an **ORM framework** developed by Microsoft.

---

# Summary

| Term                        | What is it?            | Purpose                                                       |
| --------------------------- | ---------------------- | ------------------------------------------------------------- |
| **Controller**              | ASP.NET Core component | Receives HTTP requests and returns responses                  |
| **Entity**                  | C# class               | Represents a database table                                   |
| **DbContext**               | EF Core class          | Manages the database connection and tracks entities           |
| **Entity Framework Core**   | ORM framework          | Converts C# objects to SQL and SQL results back to C# objects |
| **SQL Server / PostgreSQL** | Database               | Stores the actual data                                        |

### One sentence to remember

> **Entity Framework Core is an ORM that lets you work with your database using C# objects instead of writing SQL for every operation.**

 
 
 
 */