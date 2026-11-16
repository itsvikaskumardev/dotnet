# **EF Core Loading Techniques – Simple Hinglish Notes**

EF Core mein jab hum ek entity (table) ka data fetch karte hain, aur uske saath related tables ka data bhi chahiye hota hai, toh hum **Loading Techniques** use karte hain.

Mainly 3 types hoti hain:

1. Eager Loading – `Include()` / `ThenInclude()`
2. Lazy Loading
3. Explicit Loading

Inke alawa ek important approach hai **Projection –&#x20;**`Select()`, jo mostly API Read Operations mein useful hoti hai.

Chalo same WMS example se samajhte hain.

## **1. Sabse pehle relationship samjho**

Maan lo warehouse mein ek order hai.

**WmsOrder (Parent)**

OrderNumber: ORD-1001

One-to-Many

**WmsOrderItem (Child)**

Rice, Oil, Sugar

Many-to-One

**Product (Master Table)**

ProductName, SKU, Price

Relationships:

- One `WmsOrder` → Multiple `WmsOrderItem`
- One `WmsOrderItem` → One `Product`

Ab maan lo tumhe Order ke saath uske Items aur har Item ka Product Name bhi chahiye.

Yahi par loading techniques ka use hota hai.

## **2. Eager Loading – Include() / ThenInclude()**

**Meaning:** Jab hum main entity ke saath related entities ka data bhi ek hi query operation mein pehle se load karna chahte hain, toh Eager Loading use karte hain.

Iske liye `Include()` aur `ThenInclude()` use hote hain.

### **A. Include()**

`Include()` ka use related navigation property ko load karne ke liye hota hai.

Example:

```
var order = await _dbContext.WmsOrders
    .Include(o => o.Items)
    .FirstOrDefaultAsync(o => o.Id == id);
```

Yahan kya hua?

1. `WmsOrders` se order fetch kiya.
2. `Include(o => o.Items)` se us order ke saare related items bhi load kar liye.

Matlab:

- WmsOrder load
- WmsOrderItem load

### **B. ThenInclude()**

Jab related entity ke andar bhi kisi doosri related entity ka data chahiye ho, tab `ThenInclude()` use karte hain.

Example:

```
var order = await _dbContext.WmsOrders
    .Include(o => o.Items)
        .ThenInclude(i => i.Product)
    .FirstOrDefaultAsync(o => o.Id == id);
```

Iska flow:

1. `WmsOrders` → Main table.
2. `Include(o => o.Items)` → Order ke saare items.
3. `ThenInclude(i => i.Product)` → Har item ka Product bhi.

**Data loading flow**

1

**WmsOrder**

2

**Items**

3

**Product**

**Kab use karna chahiye?**

- Jab complete entity aur uski related entities chahiye.
- Jab Entity Framework ke through entity update karni ho.
- Jab Order ke saath Items, Product, Customer jaise related data chahiye.

**Yaad rakho:** `Include()` related entity load karta hai, aur `ThenInclude()` us related entity ke andar next relationship load karta hai.

## **3. Lazy Loading**

**Meaning:** Ismein related data automatically tab load hota hai jab tum us navigation property ko access karte ho.

Matlab pehle sirf Order load hoga. Jab tum `order.Items` access karoge, tab EF Core related Items load karega.

Example:

```
var order = await _dbContext.WmsOrders
    .FirstOrDefaultAsync(o => o.Id == id);

// Abhi sirf Order load hua.

// Jab Items access karoge:
var items = order.Items;

// Tab related Items load ho sakte hain.
```

Lazy Loading ke liye EF Core mein proxies configure karne padte hain.

Install:

```
dotnet add package Microsoft.EntityFrameworkCore.Proxies
```

Program.cs:

```
builder.Services.AddDbContext<WmsDbContext>(options =>
    options.UseLazyLoadingProxies()
           .UseNpgsql(connectionString));
```

Navigation properties ko `virtual` banana hota hai:

```
public virtual ICollection<WmsOrderItem> Items { get; set; }
    = new List<WmsOrderItem>();
```

Similarly:

```
public virtual Product Product { get; set; } = null!;
```

**Kab use karna chahiye?**

- Jab related data ki requirement pehle se clear na ho.
- Jab zaroorat padne par hi related data fetch karna ho.
- Small applications mein limited scenarios ke liye.

**Disadvantage – N+1 Query Problem**

Maan lo tumne 100 orders fetch kiye:

```
var orders = await _dbContext.WmsOrders.ToListAsync();

foreach (var order in orders)
{
    var items = order.Items;
}
```

Conceptually:

- 1 query → 100 Orders.
- 100 additional queries → Har Order ke Items.

Total = 101 queries.

Isse performance slow ho sakti hai.

**Important:** Lazy Loading ko production APIs mein carefully use karna chahiye, especially loops aur large datasets mein.

## **4. Explicit Loading**

**Meaning:** Ismein hum related data ko manually load karte hain, jab humein zaroorat hoti hai.

Lazy Loading mein navigation property access karne se data automatically load hota hai.

Lekin Explicit Loading mein hum khud EF Core ko bolte hain ki related data load karo.

Example:

```
var order = await _dbContext.WmsOrders
    .FirstOrDefaultAsync(o => o.Id == id);

// Manually Items load karo.
await _dbContext.Entry(order)
    .Collection(o => o.Items)
    .LoadAsync();
```

Ab agar har Item ka Product bhi load karna hai:

```
foreach (var item in order.Items)
{
    await _dbContext.Entry(item)
        .Reference(i => i.Product)
        .LoadAsync();
}
```

### **Collection() vs Reference()**

| Method         | Meaning                                 |
| -------------- | --------------------------------------- |
| `Collection()` | Multiple related records load karta hai |
| `Reference()`  | Single related record load karta hai    |

Example:

- `Collection(o => o.Items)` → Multiple Items.
- `Reference(i => i.Product)` → One Product.

**Kab use karna chahiye?**

- Jab initially related data ki zaroorat nahi ho.
- Jab kisi condition ke basis par related data load karna ho.
- Jab data loading par manual control chahiye.

## **5. Projection – Select()**

Ye ek bahut important concept hai, especially Backend API development mein.

**Meaning:** Projection ka matlab hai database se sirf wahi columns aur data fetch karna jo humein chahiye, aur unhe DTO ya custom object mein convert karna.

Example:

```
var order = await _dbContext.WmsOrders
    .Where(o => o.Id == id)
    .Select(o => new WmsOrderResponseDto
    {
        OrderNumber = o.OrderNumber,
        Status = o.Status,

        Items = o.Items.Select(i => new WmsOrderItemResponseDto
        {
            ProductName = i.Product.ProductName,
            SKU = i.Product.SKU,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalAmount = i.Quantity * i.UnitPrice

        }).ToList(),

        OrderTotal = o.Items.Sum(i => i.Quantity * i.UnitPrice)
    })
    .FirstOrDefaultAsync();
```

### **Yahan kya hua?**

- `Where()` → Particular order filter kiya.
- `Select()` → Custom DTO banaya.
- `i.Product.ProductName` → Related Product ka name select kiya.
- `i.Quantity` → Quantity select ki.
- `Sum()` → Total order amount calculate kiya.

**Important:** Projection mein related data lene ke liye `Include()` zaroori nahi hota. EF Core navigation properties ko SQL mein translate karke required data fetch kar sakta hai.

### **Projection vs Include**

| Include                          | Projection                               |
| -------------------------------- | ---------------------------------------- |
| Complete entities load karta hai | Required data select karta hai           |
| `Include()` use hota hai         | `Select()` use hota hai                  |
| Entity graph milta hai           | DTO/custom object milta hai              |
| Entity update ke liye useful     | Read APIs ke liye useful                 |
| Extra columns load ho sakte hain | Unnecessary columns avoid kar sakte hain |



Recommended

**Meri recommendation:** WMS ki GET APIs mein, jaise Order Details, Order Listing, Picklist Listing, mostly Projection prefer karo.



## **6. All Loading Techniques – Quick Comparison**

| Technique        | Data kab load hota hai?               | Method          |
| ---------------- | ------------------------------------- | --------------- |
| Eager Loading    | Main query ke saath                   | `Include()`     |
| Nested Eager     | Nested relationship ke saath          | `ThenInclude()` |
| Lazy Loading     | Property access par                   | Automatic       |
| Explicit Loading | Manually bolne par                    | `LoadAsync()`   |
| Projection       | Query execute hote waqt selected data | `Select()`      |

## **7. Real WMS project mein kab kya use karna chahiye?**

| Scenario                                       | Recommended              |
| ---------------------------------------------- | ------------------------ |
| Order Details GET API                          | Projection               |
| Order Listing API                              | Projection               |
| Order + Items + Product entities chahiye       | Include + ThenInclude    |
| Order update karna hai with related entities   | Include (if needed)      |
| Related data baad mein conditionally chahiye   | Explicit Loading         |
| Automatically property access par data chahiye | Lazy Loading (carefully) |

## **8. Interview ke liye yaad rakhna**

- **Eager Loading:** Related data pehle se load karna using `Include()` and `ThenInclude()`.
- **Lazy Loading:** Related data tab load karna jab navigation property access hoti hai.
- **Explicit Loading:** Related data ko manually `LoadAsync()` se load karna.
- **Projection:** `Select()` se sirf required data ko DTO mein fetch karna.

**One-line trick to remember**

- Eager = Pehle hi load.
- Lazy = Jab zaroorat pade tab automatic load.
- Explicit = Khud manually load.
- Projection = Sirf required data load.

## **9. Khud ko test karo**

**EF Core Loading Quiz**

4/4 answered

**1. Order ke saath Items aur har Item ka Product load karna hai. Kya use karoge?**

Lazy Loading

Include + ThenInclude

Explicit Loading

**Correct!**

Include Items ko aur ThenInclude har Item ke Product ko load karega.

**2. Sirf ProductName, SKU aur Quantity API response mein chahiye. Best approach?**

Projection

Lazy Loading

Explicit Loading

**Correct!**

Select() se sirf required data DTO mein fetch kar sakte ho.

**3. Related Items ko manually load karna hai. Kya use karoge?**

Include()

LoadAsync()

ThenInclude()

**Correct!**

Entry(order).Collection(o => o.Items).LoadAsync() explicit loading hai.

**4. 100 orders ke loop mein Lazy Loading se kya problem ho sakti hai?**

N+1 Query Problem

Migration Error

Foreign Key Error

**Correct!**

Har order ke liye additional query execute ho sakti hai.

Check Answers

# **4/4 Correct**

Try Again