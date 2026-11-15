Neeche complete notes hain jo tum apni `EF-Core-Interceptors.md` file mein save kar sakte ho. Maine explanation beginner-friendly Hinglish mein rakhi hai, lekin concepts professional/technical terminology ke saath explain kiye hain, taaki tum interview aur actual WMS development dono mein use kar sako.

# EF Core Interceptors & Change Tracker

## 1. Interceptor kya hota hai?

**Interceptor** EF Core ka ek feature hai jo Entity Framework Core ke database operations ke execution ke different stages par intervene karke custom logic execute karne deta hai.

Simple language mein:

Jab EF Core koi operation perform karne wala hota hai, toh Interceptor us operation ke beech mein apna custom code execute kar sakta hai.

### Real-life example (WMS Order)

Suppose hum WMS mein ek Create Order API bana rahe hain.

Jab bhi koi user order create kare, toh humein database mein ye information maintain karni hai:

- CreatedDate: Order kab create hua?
- CreatedBy: Order kis user ne create kiya?
- ModifiedDate: Order last time kab update hua?
- ModifiedBy: Order last time kis user ne update kiya?

Normally humein har API mein manually ye properties set karni padti:

```
order.CreatedDate = DateTime.UtcNow;
order.CreatedBy = currentUser;
```

Lekin agar hum EF Core Interceptor use karte hain, toh ye logic ek common class mein likh sakte hain.

Ab jab bhi supported entity ke liye `SaveChanges()` ya `SaveChangesAsync()` execute hoga, interceptor automatically audit fields set kar dega.

**Main purpose:**

- Repeated code ko avoid karna.
- Common logic ko centralize karna.
- Audit fields automatically maintain karna.
- Database operations ko monitor ya customize karna.

---

## 2. EF Core Interceptors ke types

EF Core mein different types ke interceptors available hain. Ye different database operations ko intercept karte hain.

| Interceptor                 | Purpose                                                                                |
| --------------------------- | -------------------------------------------------------------------------------------- |
| SaveChangesInterceptor      | SaveChanges aur SaveChangesAsync operations intercept karta hai.                       |
| DbCommandInterceptor        | SQL commands execute hone ke different stages ko intercept karta hai.                  |
| DbConnectionInterceptor     | Database connection open/close hone ke operations intercept karta hai.                 |
| DbTransactionInterceptor    | Database transactions ko intercept karta hai.                                          |
| DbDataReaderInterceptor     | Database se results read karne ke operations intercept karta hai.                      |
| IMaterializationInterceptor | Database results se entity objects materialize hone ke process ko intercept karta hai. |

### Humne kaunsa Interceptor use kiya?

Humare WMS project mein:

`SaveChangesInterceptor`

use kiya gaya hai.

Kyunki humein database mein data save hone se pehle audit fields automatically set karni hain.

Isliye humne `SaveChangesInterceptor` inherit karke apni custom class banayi:

```
public class AuditableInterceptor : SaveChangesInterceptor
{
}
```

---

## 3. SaveChangesInterceptor kaise kaam karta hai?

SaveChangesInterceptor EF Core ke SaveChanges pipeline ke different stages par custom logic execute karne deta hai.

Ismein humne do important methods override kiye:

### 1. SavingChanges()

Ye method tab execute hota hai jab application synchronous `SaveChanges()` call karti hai.

```
public override InterceptionResult<int> SavingChanges(
    DbContextEventData eventData,
    InterceptionResult<int> result)
{
    // Custom logic

    return base.SavingChanges(eventData, result);
}
```

### 2. SavingChangesAsync()

Ye method tab execute hota hai jab application asynchronous `SaveChangesAsync()` call karti hai.

```
public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
    DbContextEventData eventData,
    InterceptionResult<int> result,
    CancellationToken cancellationToken = default)
{
    // Custom logic

    return base.SavingChangesAsync(
        eventData,
        result,
        cancellationToken);
}
```

**Difference:**

| SavingChanges               | SavingChangesAsync              |
| --------------------------- | ------------------------------- |
| Synchronous operation       | Asynchronous operation          |
| SaveChanges()               | SaveChangesAsync()              |
| Sync code execute karta hai | Async save pipeline ka part hai |

Humare WMS APIs mein generally `SaveChangesAsync()` use hota hai, isliye `SavingChangesAsync()` important hai.

---

## 4. Change Tracker kya hota hai?

Change Tracker EF Core ka ek mechanism hai jo DbContext ke andar entities ko track karta hai.

Simple language mein:

**Change Tracker monitor karta hai ki entity new hai, update hui hai, delete hui hai ya usmein koi change nahi hua.**

### WMS Order example

Suppose database mein pehle se ek order available hai:

```
OrderNumber = ORD001
Status      = Pending
```

Hum EF Core se order fetch karte hain:

```
var order = await context.WmsOrders
    .FirstOrDefaultAsync(x => x.OrderNumber == "ORD001");
```

By default EF Core is entity ko track karta hai.

Ab initially entity ki state hogi:

```
EntityState.Unchanged
```

Ab hum status update karte hain:

```
order.Status = "Completed";
```

EF Core detect karega ki entity mein modification hua hai.

Ab entity ki state:

```
EntityState.Modified
```

Jab `SaveChangesAsync()` call hoga, EF Core UPDATE query generate karega.

### Change Tracker ka flow

1. Database se WmsOrder fetch hui.
2. EF Core ne entity ko track kiya.
3. Entity ki original values maintain ki.
4. Developer ne entity ki property update ki.
5. EF Core ne change detect kiya.
6. Entity ki state Modified ho gayi.
7. SaveChangesAsync call hua.
8. Interceptor ne entity ki state check ki.
9. Audit fields update hue.
10. Database mein changes save ho gaye.

---

## 5. EntityState kya hota hai?

EntityState batata hai ki EF Core ke according entity ki current tracking condition kya hai.

EF Core mein five important states hoti hain.

### 1. Added

Jab hum new entity ko DbContext mein add karte hain.

```
context.WmsOrders.Add(order);
```

State:

```
EntityState.Added
```

Meaning: New record database mein insert hona hai.

SQL operation: INSERT

### 2. Modified

Jab existing tracked entity ki properties change hoti hain.

```
order.Status = "Completed";
```

State:

```
EntityState.Modified
```

Meaning: Existing record update hona hai.

SQL operation: UPDATE

### 3. Deleted

Jab existing entity ko remove karte hain.

```
context.WmsOrders.Remove(order);
```

State:

```
EntityState.Deleted
```

Meaning: Entity delete mark ho gayi hai.

SQL operation: DELETE

### 4. Unchanged

Jab entity database se fetch hui ho aur usmein koi change detect nahi hua.

```
var order = await context.WmsOrders.FirstAsync();
```

State:

```
EntityState.Unchanged
```

Meaning: Entity tracked hai, lekin abhi koi modification nahi hua.

### 5. Detached

Jab entity current DbContext ke Change Tracker mein tracked nahi hai.

```
var order = new WmsOrder();

var state = context.Entry(order).State;
```

State:

```
EntityState.Detached
```

Meaning: Entity abhi context se attached nahi hai.

### EntityState comparison

| EntityState | Meaning        | SQL operation |
| ----------- | -------------- | ------------- |
| Added       | New entity     | INSERT        |
| Modified    | Updated entity | UPDATE        |
| Deleted     | Deleted entity | DELETE        |
| Unchanged   | No changes     | Normally none |
| Detached    | Not tracked    | None          |

Note: Added, Modified, Deleted states mein actual database operation tabhi hoga jab SaveChanges successfully execute hoga. Unchanged aur Detached entities normally save nahi hoti.

---

## 6. EntityEntry kya hota hai?

EntityEntry kisi particular entity ki tracking information access karne ke liye use hota hai.

Example:

```
var order = await context.WmsOrders.FirstAsync();

var entry = context.Entry(order);
```

Ab hum entity ki state check kar sakte hain:

```
entry.State;
```

Original aur current values access kar sakte hain:

```
var property = entry.Property(x => x.Status);

property.OriginalValue;

property.CurrentValue;

property.IsModified;
```

Example:

```
OriginalValue = Pending
CurrentValue  = Completed
IsModified    = True
```

### ChangeTracker vs EntityEntry

| ChangeTracker                             | EntityEntry                                         |
| ----------------------------------------- | --------------------------------------------------- |
| All tracked entities ko manage karta hai. | Particular entity ki tracking information deta hai. |
| Entries() method provide karta hai.       | State aur Property APIs provide karta hai.          |
| Multiple entities ke liye useful.         | Single entity ke liye useful.                       |

---

## 7. AuditableInterceptor mein ChangeTracker ka use kyun kiya?

Humare interceptor mein:

```
foreach (var entry in context.ChangeTracker.Entries())
{
    if (entry.State == EntityState.Added)
    {
        // CreatedDate and CreatedBy set
    }

    else if (entry.State == EntityState.Modified)
    {
        // ModifiedDate and ModifiedBy set
    }
}
```

Yahan hum `ChangeTracker.Entries()` se current DbContext ki tracked entities ki entries retrieve karte hain.

Phir foreach loop se har entity ki state check karte hain.

### Added state

Agar entity Added hai, toh iska matlab new record insert hone wala hai.

```
if (entry.State == EntityState.Added)
{
    entry.Entity.CreatedDate = DateTime.UtcNow;
    entry.Entity.CreatedBy = currentUser;
}
```

### Modified state

Agar entity Modified hai, toh iska matlab existing record update hone wala hai.

```
else if (entry.State == EntityState.Modified)
{
    entry.Entity.ModifiedDate = DateTime.UtcNow;
    entry.Entity.ModifiedBy = currentUser;
}
```

Is tarah audit fields automatically set ho jaate hain.

---

## 8. IAuditableEntity Interface kya hai?

Hum chahte hain ki sirf wahi entities audit hon jo audit mechanism support karti hain.

Isliye hum common interface banate hain.

```
public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }

    string? CreatedBy { get; set; }

    DateTime? ModifiedDate { get; set; }

    string? ModifiedBy { get; set; }
}
```

Ab WmsOrder implement karega:

```
public class WmsOrder : IAuditableEntity
{
    public Guid Id { get; set; }

    public string OrderNumber { get; set; }

    public DateTime CreatedDate { get; set; }

    public string? CreatedBy { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? ModifiedBy { get; set; }
}
```

**Interface ka purpose:**

Ye contract define karta hai ki implement karne wali entity mein ye audit properties honi chahiye.

Interface khud property ka actual storage provide nahi karta, isliye WmsOrder mein properties implement karni padti hain.

---

## 9. IHttpContextAccessor ka use kyun hota hai?

Audit fields mein humein current logged-in user ki ID chahiye.

Example:

```
Vikas User ID = 101
Rahul User ID = 102
```

Agar Vikas order create karta hai toh:

```
CreatedBy = 101
```

Agar Rahul order update karta hai toh:

```
ModifiedBy = 102
```

Isliye current request se logged-in user ki identity retrieve karni hoti hai.

`IHttpContextAccessor` current HTTP request ka HttpContext access karne deta hai.

```
var userId = _httpContextAccessor
    .HttpContext?
    .User
    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
```

Is user ID ko interceptor audit fields mein set karta hai.

---

## 10. Interceptor vs DbContext.SaveChangesAsync Override

Dono approaches SaveChanges ke time custom logic execute kar sakti hain.

| DbContext Override                             | Interceptor                                  |
| ---------------------------------------------- | -------------------------------------------- |
| DbContext class ke andar logic                 | Separate class mein logic                    |
| DbContext-specific implementation              | Multiple DbContexts mein reuse               |
| Simple application ke liye useful              | Cross-cutting logic ke liye useful           |
| Separate interceptor registration nahi chahiye | AddInterceptors() se register karna hota hai |

Large applications mein auditing aur logging jaise common concerns ke liye Interceptor useful hai.

---

## 11. Complete WMS Order flow

### Create Order API

Developer code:

```
var order = new WmsOrder
{
    Id = Guid.NewGuid(),
    OrderNumber = "ORD001",
    Status = "Pending"
};

context.WmsOrders.Add(order);

await context.SaveChangesAsync();
```

Internally:

1. WmsOrder object create hua.
2. Add() ne entity ko Change Tracker mein register kiya.
3. EntityState = Added.
4. SaveChangesAsync() call hua.
5. SavingChangesAsync() interceptor execute hua.
6. ChangeTracker.Entries() se tracked entities retrieve hui.
7. EntityState Added check hua.
8. CreatedDate aur CreatedBy automatically set hue.
9. EF Core INSERT query execute karta hai.
10. Database mein order save ho gaya.

### Update Order API

Developer code:

```
var order = await context.WmsOrders
    .FirstAsync(x => x.Id == id);

order.Status = "Completed";

await context.SaveChangesAsync();
```

Internally:

1. Database se existing WmsOrder fetch hui.
2. EF Core ne entity track ki.
3. Initial state Unchanged thi.
4. Status update hua.
5. EntityState Modified detect hui.
6. SaveChangesAsync() call hua.
7. Interceptor execute hua.
8. ModifiedDate aur ModifiedBy automatically set hue.
9. EF Core UPDATE query execute karta hai.
10. Database mein updated order save ho gaya.

---

## 12. Interceptor ko DbContext mein register kaise karte hain?

Interceptor class create karna enough nahi hai. Use EF Core ke DbContext ke saath register bhi karna hota hai.

```
builder.Services.AddScoped<AuditableInterceptor>();

builder.Services.AddDbContext<WmsDbContext>(
    (serviceProvider, options) =>
    {
        options.UseNpgsql(connectionString);

        options.AddInterceptors(
            serviceProvider.GetRequiredService<AuditableInterceptor>()
        );
    });
```

Important line:

```
options.AddInterceptors(
    serviceProvider.GetRequiredService<AuditableInterceptor>()
);
```

Ye EF Core ko batati hai ki WmsDbContext ke SaveChanges pipeline mein AuditableInterceptor use karna hai.

Ab har API mein interceptor manually call nahi karna padta.

---

## 13. Important points to remember

1. Interceptor = EF Core operations ke beech mein custom logic execute karna.
2. SaveChangesInterceptor = SaveChanges operations intercept karta hai.
3. ChangeTracker = Entity changes ko monitor karta hai.
4. EntityState = Entity ki current tracking condition batata hai.
5. EntityEntry = Specific entity ki tracking information provide karta hai.
6. Added = New entity insert hogi.
7. Modified = Existing entity update hogi.
8. Deleted = Entity delete mark hui hai.
9. Unchanged = Entity mein koi detected change nahi.
10. Detached = Entity tracked nahi hai.
11. IHttpContextAccessor = Current HTTP request se logged-in user ki identity access karna.
12. AddInterceptors() = Interceptor ko DbContext ke saath register karna.

**Important limitation:** Normal SaveChanges interceptor `ExecuteUpdateAsync()`, `ExecuteDeleteAsync()` aur direct SQL operations ko automatically intercept nahi karta, kyunki ye normal SaveChanges pipeline bypass karte hain.

---

## 14. Interview questions

**Q1. What is an EF Core Interceptor?**

An EF Core Interceptor allows us to intercept database-related operations and execute custom logic at different stages of their execution.

**Q2. Why did we use SaveChangesInterceptor?**

To automatically set audit fields such as CreatedDate, CreatedBy, ModifiedDate and ModifiedBy before saving entities.

**Q3. What is ChangeTracker?**

ChangeTracker is an EF Core mechanism that tracks entity states and detects changes in tracked entities.

**Q4. What is EntityState?**

EntityState represents the current tracking state of an entity, such as Added, Modified, Deleted, Unchanged or Detached.

**Q5. What is EntityEntry?**

EntityEntry provides tracking information and property-level access for a particular entity.

**Q6. Why use IHttpContextAccessor?**

To retrieve the currently authenticated user's information from the active HTTP request.

**Q7. What is the difference between SavingChanges and SavingChangesAsync?**

SavingChanges intercepts synchronous SaveChanges operations, whereas SavingChangesAsync intercepts asynchronous SaveChangesAsync operations.

**Q8. Do we need to call the interceptor manually in every API?**

No. Once registered with the DbContext using AddInterceptors(), EF Core invokes it automatically when the corresponding SaveChanges pipeline executes.

---

## 15. One-line summary

**ChangeTracker** entities ko track karta hai → **EntityState** batata hai ki entity Added/Modified hai → **Interceptor** us state ko check karta hai → Audit fields automatically set hoti hain → **SaveChangesAsync()** database mein changes save karta hai.

**Ek practical suggestion:** Notes padhne ke baad apne WMS project mein `AuditableInterceptor.cs` par breakpoint lagakar Create Order aur Update Order API execute karna. Tum khud dekh paoge ki entity `Added` se `Unchanged` aur update ke case mein `Unchanged` se `Modified` kaise hoti hai.