# **IQueryable and IEnumerable in .NET / EF Core**

## **1. IQueryable aur IEnumerable kya hain?**

Dono C# ke **interfaces** hain, jo collections aur data ke saath kaam karne ke liye use hote hain.

EF Core mein inka use database se data retrieve, filter aur process karne ke liye hota hai.

Simple language mein:

- **IQueryable:** Database se data fetch karne se pehle query build aur modify karne ke liye use hota hai.
- **IEnumerable:** Generally application memory mein already available data ko filter aur process karne ke liye use hota hai.

Dono LINQ queries likhne mein help karte hain, lekin EF Core mein inka behaviour different hota hai.

## **2. IQueryable kya hai?**

`IQueryable<T>` ek interface hai jo query ko represent karta hai. EF Core ke saath use karne par hum database ke liye query build kar sakte hain.

**Example:**

```
IQueryable<WmsOrder> orders = context.WmsOrders
    .Where(o => o.Status == "Pending");
```

Yahan:

- `context.WmsOrders` database ki WmsOrders entity query hai.
- `Where()` Pending orders ki condition add karta hai.
- `IQueryable<WmsOrder>` query ko represent karta hai.
- Abhi query execute nahi hui aur records fetch nahi hue.

Jab hum execution method call karenge:

```
var result = await orders.ToListAsync();
```

Tab EF Core SQL generate karke database se matching records fetch karega.

**SQL conceptually:**

```
SELECT *
FROM WmsOrders
WHERE Status = 'Pending';
```

### **IQueryable kyu use karte hain?**

1. Database level par filtering karne ke liye.
2. Unnecessary records ko application memory mein load hone se bachane ke liye.
3. Multiple conditions dynamically add karne ke liye.
4. Database queries ko efficiently build karne ke liye.

## **3. IEnumerable kya hai?**

`IEnumerable<T>` ek interface hai jo collection ke elements ko iterate aur process karne ke liye use hota hai.

Generally jab database se data fetch ho chuka hota hai, tab hum `IEnumerable` ka use karke application memory mein data filter kar sakte hain.

**Example:**

```
var orders = await context.WmsOrders
    .ToListAsync();

IEnumerable<WmsOrder> result = orders
    .Where(o => o.Status == "Pending");
```

Yahan:

- `ToListAsync()` ne database se saare orders fetch kar liye.
- Data application memory mein aa gaya.
- `IEnumerable<WmsOrder>` us collection ko represent kar raha hai.
- `Where()` ab memory mein Pending orders filter karega.

### **IEnumerable kyu use karte hain?**

1. In-memory collection ko filter karne ke liye.
2. Already fetched data ko process karne ke liye.
3. C# collections par LINQ operations perform karne ke liye.
4. Jab database query ki zaroorat nahi ho aur data already available ho.

## **4. IQueryable vs IEnumerable – Example with WMS**

Suppose tumhare database mein 10,000 WmsOrders hain aur humein sirf Pending orders chahiye.

**IQueryable approach:**

```
IQueryable<WmsOrder> query = context.WmsOrders
    .Where(o => o.Status == "Pending");

var orders = await query.ToListAsync();
```

- Filtering database mein hogi.
- Sirf matching Pending orders application memory mein aayenge.

**IEnumerable approach:**

```
var allOrders = await context.WmsOrders
    .ToListAsync();

IEnumerable<WmsOrder> query = allOrders
    .Where(o => o.Status == "Pending");
```

- Pehle saare 10,000 orders memory mein fetch honge.
- Uske baad Pending orders filter honge.

## **5. Memory mein kya difference hai?**

Ye dono approaches ka important difference hai.

**IQueryable**

Database-level filtering

**Database records**

10,000

**Matching Pending records**

100

Illustration: only matching records are retrieved by the filtered query.

**IEnumerable (after ToListAsync)**

In-memory filtering

**Records fetched into memory**

10,000

**Matching Pending records**

100

All 10,000 were fetched before filtering.

**Conclusion:** IQueryable generally memory-efficient hota hai jab database mein filtering karni ho, kyunki unnecessary records application memory mein load nahi hote.

## **6. Important: Deferred Execution vs Immediate Execution**

- **Deferred Execution:** Query immediately execute nahi hoti; jab result enumerate ya retrieve karte hain tab execute hoti hai.
- **Immediate Execution:** Query execute hoti hai aur result retrieve ya calculate hota hai.

Example:

```
// Deferred Execution
var query = context.WmsOrders
    .Where(o => o.Status == "Pending");

// Immediate Execution
var orders = await query.ToListAsync();
```

Important: `IQueryable` aur `IEnumerable` dono deferred execution support kar sakte hain. `IEnumerable` ka matlab automatically immediate execution nahi hota.

## **7. Difference Table**

| Feature         | IQueryable                           | IEnumerable                                        |
| --------------- | ------------------------------------ | -------------------------------------------------- |
| Type            | Interface                            | Interface                                          |
| Main purpose    | Query building                       | Collection processing                              |
| Filtering       | Database level (EF Core)             | In-memory (LINQ-to-Objects)                        |
| SQL translation | Supported by EF Core provider        | Subsequent operations SQL mein translate nahi hote |
| Execution       | Generally deferred                   | Depends on operation                               |
| Async methods   | EF Core methods like `ToListAsync()` | Directly `ToListAsync()` nahi                      |
| Best use        | Database queries                     | In-memory data                                     |

## **8. Final Notes – Remember**

**IQueryable**

Database se data fetch karne se pehle query build aur modify karne ke liye use hota hai. EF Core is query ko SQL mein translate kar sakta hai.

**IEnumerable**

Generally database se data fetch hone ke baad application memory mein available collection ko filter aur process karne ke liye use hota hai.

**Why use them?**

- IQueryable → Database-level filtering, less unnecessary data retrieval.
- IEnumerable → In-memory data processing.

**Memory difference:**

- IQueryable → Sirf required records retrieve kar sakte ho.
- IEnumerable → Agar pehle `ToListAsync()` se saara data fetch kar liya, toh woh data memory consume karega.

**Interview answer:** `IQueryable` is used to build queries that can be translated and executed by a database provider, whereas `IEnumerable` is used to iterate and process data in memory. In EF Core, IQueryable helps perform filtering at the database level, reducing unnecessary data retrieval.