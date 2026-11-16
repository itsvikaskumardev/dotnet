# **LINQ: Method Syntax vs Query Syntax (EF Core)**

## **1. LINQ kya hota hai?**

LINQ ka full form hai **Language Integrated Query**.

LINQ C# mein data ko query, filter, sort aur select karne ka way provide karta hai.

EF Core mein hum LINQ ka use karke database se data retrieve karte hain.

LINQ likhne ke 2 main ways hain:

1. Method Syntax
2. Query Syntax

Dono ka purpose same hai, bas likhne ka tarika different hai.

## **2. Method Syntax**

**Method Syntax** mein hum methods ka use karke database query likhte hain.

Common methods:

- `Where()` – Data filter karna.
- `Select()` – Required columns select karna.
- `OrderBy()` – Ascending order mein sort karna.
- `OrderByDescending()` – Descending order mein sort karna.
- `FirstOrDefault()` – First matching record ya null.
- `ToList()` – Records ko list mein convert karna.
- `Join()` – Multiple tables ko join karna.
- `Include()` – Related entities load karna.

### **Example: Pending Orders fetch karna**

```
var orders = await context.WmsOrders
    .Where(o => o.Status == "Pending")
    .OrderByDescending(o => o.CreatedDate)
    .ToListAsync();
```

**Explanation:**

1. `context.WmsOrders` – WmsOrders table access ki.
2. `Where()` – Sirf Pending orders filter kiye.
3. `OrderByDescending()` – Latest orders pehle show honge.
4. `ToListAsync()` – Database se records fetch karke list return ki.

### **Kab use karein?**

- Normal database queries.
- Filtering and sorting.
- Pagination (`Skip`, `Take`).
- `Include()` and `ThenInclude()`.
- API mein DTO projection using `Select()`.

## **3. Query Syntax**

**Query Syntax** mein hum SQL jaisa syntax use karke LINQ queries likhte hain.

Common keywords:

- `from` – Kis table se data lena hai.
- `where` – Data filter karna.
- `join` – Tables ko join karna.
- `orderby` – Data sort karna.
- `select` – Required data select karna.
- `group` – Data grouping karna.

### **Example: Pending Orders fetch karna**

```
var orders = await (
    from o in context.WmsOrders

    where o.Status == "Pending"

    orderby o.CreatedDate descending

    select o

).ToListAsync();
```

**Explanation:**

1. `from o` – WmsOrders table se data liya.
2. `where` – Sirf Pending orders filter kiye.
3. `orderby` – Latest orders pehle.
4. `select o` – Complete order select kiya.
5. `ToListAsync()` – Records fetch kiye.

### **Kab use karein?**

- Multiple tables ke complex JOINs.
- Jab SQL jaisa readable structure chahiye.
- Complex filtering and grouping.
- Reporting queries.

## **4. Method Syntax vs Query Syntax**

Dono se same query likh sakte hain.

**Method Syntax:**

```
var orders = await context.WmsOrders
    .Where(o => o.Status == "Pending")
    .ToListAsync();
```

**Query Syntax:**

```
var orders = await (
    from o in context.WmsOrders
    where o.Status == "Pending"
    select o
).ToListAsync();
```

### **Difference**

| Method Syntax                                  | Query Syntax                      |
| ---------------------------------------------- | --------------------------------- |
| Methods use hote hain                          | SQL-like keywords use hote hain   |
| `.Where()`                                     | `where`                           |
| `.Select()`                                    | `select`                          |
| `.OrderBy()`                                   | `orderby`                         |
| `.Join()`                                      | `join`                            |
| Method chaining hoti hai                       | Query clauses use hote hain       |
| Normal EF Core queries mein commonly preferred | Complex JOINs mein often readable |

## **5. Kab konsa use karna chahiye?**

| Situation                    | Recommended                      |
| ---------------------------- | -------------------------------- |
| Simple WHERE condition       | Method Syntax                    |
| OrderBy / OrderByDescending  | Method Syntax                    |
| Skip / Take (Pagination)     | Method Syntax                    |
| FirstOrDefault / Any / Count | Method Syntax                    |
| Include / ThenInclude        | Method Syntax                    |
| Simple JOIN                  | Both                             |
| Multiple complex JOINs       | Query Syntax often easier        |
| DTO Projection               | Method Syntax commonly preferred |

## **6. Important Points to Remember**

1. Method Syntax aur Query Syntax dono LINQ likhne ke ways hain.
2. Dono ka purpose same hai.
3. Dono ko EF Core SQL mein translate karta hai.
4. Performance inherently kisi ek syntax ki better nahi hoti.
5. Actual performance generated SQL, indexes aur database execution par depend karti hai.
6. Method Syntax mein kuch operations available hain jo Query Syntax mein directly nahi hote, jaise `FirstOrDefaultAsync()`, `ToListAsync()` etc.

**Final rule – Yaad rakhna**

Method Syntax

Normal EF Core queries ke liye ise primary approach rakho.

Query Syntax

Complex JOINs ke liye jab SQL-like structure zyada readable lage, tab use karo.

**Important:** Dono seekho, kyunki real-world codebase mein dono milenge.