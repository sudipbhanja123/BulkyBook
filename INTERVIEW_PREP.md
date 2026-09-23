# .NET Interview Preparation Guide (Mid-Level)
## Focus: Razor Pages & ASP.NET Core Web Development

---

## Table of Contents
1. [ASP.NET Core & Razor Pages](#aspnet-core--razor-pages)
2. [C# Advanced Concepts](#c-advanced-concepts)
3. [Entity Framework Core](#entity-framework-core)
4. [Async/Await & Performance](#asyncawait--performance)
5. [SOLID Principles](#solid-principles)
6. [Design Patterns](#design-patterns)
7. [Dependency Injection & Middleware](#dependency-injection--middleware)
8. [Security & Best Practices](#security--best-practices)

---

## ASP.NET Core & Razor Pages

### Q1: What's the difference between Razor Pages and MVC?
**Answer:**
- **Razor Pages**: Page-focused, files contain both view logic and handler methods in code-behind (`PageModel`). Follows page-centric routing.
- **MVC**: Controller-Model-View separation. Multiple views can use the same controller. More separation of concerns.
- **When to use**: Razor Pages for simpler applications, MVC for complex apps with many views sharing logic.

```csharp
// Razor Page - ProductsModel.cshtml.cs
public class ProductsModel : PageModel
{
	public List<Product> Products { get; set; }

	public async Task OnGetAsync()
	{
		Products = await _context.Products.ToListAsync();
	}
}
```

---

### Q2: Explain the ASP.NET Core request pipeline
**Answer:**
The request goes through middleware in **order**:
1. App starts → Middleware configured in `Program.cs`
2. Request enters → Exception Handling → HTTPS Redirect
3. Static Files → Routing → Authentication → Authorization
4. Endpoint Execution (Controller/PageModel handler)
5. Response exits → Same middleware in REVERSE order

```csharp
// Program.cs - Pipeline order matters!
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseExceptionHandler("/Error");  // Must be early
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();  // Before Authorization
app.UseAuthorization();
app.MapRazorPages();
app.Run();
```

---

### Q3: What are route constraints and why use them?
**Answer:**
Route constraints validate route parameters before reaching the handler.

```csharp
// Route constraint examples - in PageRoute or MapPageRoute
[PageRoute("/product/{id:int}")]  // Must be integer
[PageRoute("/category/{slug:regex(^[a-z0-9-]+$)}")]  // Regex validation

// Common constraints: int, guid, string, bool, datetime
```

**Why use them?**
- Avoid passing invalid data to handlers
- Return 404 early instead of 400 errors
- Cleaner code (less validation in handler)

---

### Q4: Difference between `UseRouting()` and `MapRazorPages()`?
**Answer:**
- `UseRouting()`: Matches the current request to a route but **doesn't execute** it
- `MapRazorPages()` (or similar): **Registers** Razor Pages as endpoints and must be called after `UseRouting()`
- Middleware between them can access route info via `HttpContext.GetRouteData()`

```csharp
app.UseRouting();
// Middleware here runs AFTER routing is determined
app.UseAuthorization();  // Can use route info
app.MapRazorPages();  // Executes the matched page
```

---

## C# Advanced Concepts

### Q5: What is the difference between `ref`, `out`, and `in` parameters?
**Answer:**

| Keyword | Purpose | Must Initialize? | Used For |
|---------|---------|------------------|----------|
| `ref` | Pass by reference | YES (caller) | Read/write existing value |
| `out` | Return multiple values | NO (callee sets) | Returning results |
| `in` | Pass by reference (read-only) | YES (caller) | Performance (large structs) |

```csharp
// ref - caller provides value
void Increment(ref int x) { x++; }
int num = 5;
Increment(ref num);  // num is now 6

// out - method returns values
bool TryParse(string input, out int result)
{
	result = 0;
	if (int.TryParse(input, out var parsed))
	{
		result = parsed;
		return true;
	}
	return false;
}

// in - read-only reference (performance)
void Process(in LargeStruct data) { /* read-only */ }
```

---

### Q6: Explain `IEnumerable` vs `IQueryable`
**Answer:**

| Feature | IEnumerable | IQueryable |
|---------|-------------|-----------|
| Namespace | System.Collections | System.Linq |
| Execution | LINQ to Objects (in-memory) | Translated to SQL/other |
| When filtered | Loads all data, filters in-memory | Filters **before** database query |
| Performance | Slower for large datasets | Faster (remote filtering) |

```csharp
// IEnumerable - BAD - loads all 1M products, then filters
IEnumerable<Product> products = _context.Products;  // SELECT * executed
var expensive = products.Where(p => p.Price > 100);  // In-memory filtering

// IQueryable - GOOD - filters at database level
IQueryable<Product> products = _context.Products;  // Not executed yet
var cheap = products.Where(p => p.Price > 100);  // SELECT WHERE executed
var list = cheap.ToList();  // Now fetches results
```

---

### Q7: What are nullable reference types and why use them?
**Answer:**
Nullable reference types (C# 8+) let you explicitly mark when a reference **can be null**.

```csharp
#nullable enable

public class Product
{
	public string Name { get; set; }  // NOT nullable - must have value
	public string? Description { get; set; }  // Nullable - can be null
}

// Compiler warns you about potential null issues
public string GetName(Product p)
{
	return p.Name;  // Safe - Name is non-nullable
}

public string GetDescription(Product p)
{
	return p.Description;  // WARNING! Could be null
}
```

**Why use them:**
- Catch null reference exceptions **at compile-time**, not runtime
- Self-documenting code (readers know which values can be null)
- Reduces bugs

---

## Entity Framework Core

### Q8: What are the three ways to configure Entity Framework Core?
**Answer:**

1. **Fluent API** (Most powerful, recommended for complex scenarios)
```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
	modelBuilder.Entity<Product>()
		.HasKey(p => p.Id);
	modelBuilder.Entity<Product>()
		.Property(p => p.Name)
		.HasMaxLength(100)
		.IsRequired();
	modelBuilder.Entity<Product>()
		.HasMany(p => p.Reviews)
		.WithOne(r => r.Product);
}
```

2. **Data Annotations** (Simpler, but less flexible)
```csharp
public class Product
{
	[Key]
	public int Id { get; set; }

	[Required]
	[MaxLength(100)]
	public string Name { get; set; }
}
```

3. **Convention** (Default EF behavior)
- Properties named `Id` or `{EntityName}Id` become primary key
- Non-nullable types are `Required`
- `DbSet<T>` properties become tables

---

### Q9: Difference between `.ToList()` and `.AsNoTracking()`?
**Answer:**

```csharp
// .ToList() - Loads AND tracks changes
var products = _context.Products.ToList();  // Executed now
products[0].Name = "New Name";
await _context.SaveChangesAsync();  // Saves changes

// .AsNoTracking() - Loads but doesn't track (read-only, faster)
var products = _context.Products.AsNoTracking().ToList();
products[0].Name = "New Name";
await _context.SaveChangesAsync();  // NO changes saved (not tracked)
```

**Use Case:**
- `ToList()`: When you need to modify entities
- `AsNoTracking()`: Read-only queries, dashboard displays, reports (faster memory usage)

---

### Q10: What's the N+1 query problem and how to fix it?
**Answer:**
**Problem:** Multiple queries instead of one, especially with relationships.

```csharp
// BAD - N+1 Query Problem
var products = _context.Products.ToList();  // 1 query
foreach (var product in products)
{
	var reviews = _context.Reviews
		.Where(r => r.ProductId == product.Id)
		.ToList();  // N more queries (each iteration)
}
// Total: 1 + N queries

// GOOD - Use Include() for eager loading
var products = _context.Products
	.Include(p => p.Reviews)  // Single JOIN query
	.ToList();
foreach (var product in products)
{
	var reviews = product.Reviews;  // No query, in-memory
}
// Total: 1 query

// Alternative - Projection
var data = _context.Products
	.Select(p => new
	{
		p.Name,
		ReviewCount = p.Reviews.Count
	})
	.ToList();  // 1 query, only needed data
```

---

## Async/Await & Performance

### Q11: Why use async/await? When NOT to use it?
**Answer:**
**Use async/await when:**
- I/O operations: network calls, database queries, file I/O
- Freeing up threads for other work
- Improving application responsiveness

```csharp
// Good - I/O bound operation
public async Task<List<Product>> GetProductsAsync()
{
	return await _context.Products.ToListAsync();  // Thread not blocked
}

// Good - Multiple concurrent operations
public async Task<(List<Product>, List<Category>)> GetDataAsync()
{
	var productsTask = _context.Products.ToListAsync();
	var categoriesTask = _context.Categories.ToListAsync();
	await Task.WhenAll(productsTask, categoriesTask);  // Parallel!
	return (productsTask.Result, categoriesTask.Result);
}
```

**DON'T use async/await when:**
- Pure CPU-bound work (calculations, algorithms)
- Just wrapping synchronous code in `Task.FromResult()`

```csharp
// BAD - False async (wastes resources)
public async Task<int> CalculateAsync(int x)
{
	await Task.Delay(0);  // Unnecessary
	return x * x;  // Pure CPU work
}

// GOOD - Synchronous CPU work
public int Calculate(int x) => x * x;
```

---

### Q12: What's the difference between `.Result` and `await`?
**Answer:**

```csharp
// .Result - Blocking (can deadlock!)
public void BadFoo()
{
	var data = GetDataAsync().Result;  // Blocks thread, waits for result
}

// await - Non-blocking (recommended)
public async Task GoodFoo()
{
	var data = await GetDataAsync();  // Yields thread, continues after async work
}
```

**Critical issue:** Using `.Result` on UI/ASP.NET can cause **deadlock**. Always `await`.

---

## SOLID Principles

### Q13: Explain SOLID principles with examples
**Answer:**

**S - Single Responsibility Principle**
```csharp
// BAD - User class does too much
public class User
{
	public void Register() { /* DB logic */ }
	public void SendEmail() { /* Email logic */ }
	public void LogActivity() { /* Log logic */ }
}

// GOOD - Separated concerns
public class UserService { public void Register() { } }
public class EmailService { public void SendEmail() { } }
public class AuditService { public void LogActivity() { } }
```

**O - Open/Closed Principle** (Open for extension, closed for modification)
```csharp
// BAD - Modify method every time we add payment type
public class PaymentProcessor
{
	public void Process(string type)
	{
		if (type == "Credit") { /* ... */ }
		else if (type == "Paypal") { /* ... */ }
	}
}

// GOOD - Extend without modifying
public interface IPaymentProcessor
{
	void Process(Order order);
}
public class CreditCardProcessor : IPaymentProcessor { }
public class PayPalProcessor : IPaymentProcessor { }
```

**L - Liskov Substitution Principle** (Derived classes must be substitutable)
```csharp
// BAD - Derived class breaks contract
public abstract class Bird
{
	public abstract void Fly();
}
public class Penguin : Bird
{
	public override void Fly() { throw new NotImplementedException(); }  // Violates!
}

// GOOD - Correct inheritance
public abstract class Bird { }
public class FlyingBird : Bird { public virtual void Fly() { } }
public class Penguin : Bird { }  // Doesn't inherit Fly
```

**I - Interface Segregation Principle** (Clients shouldn't depend on methods they don't use)
```csharp
// BAD - Fat interface
public interface IRepository
{
	void Create(T item);
	void Read(int id);
	void Update(T item);
	void Delete(int id);
	void Backup();  // Not all repos need backup
}

// GOOD - Segregated interfaces
public interface IReadable<T> { T Read(int id); }
public interface IWritable<T> { void Create(T item); void Update(T item); }
public interface IBackupable { void Backup(); }
```

**D - Dependency Inversion Principle** (Depend on abstractions, not concretions)
```csharp
// BAD - Depends on concrete class
public class OrderService
{
	private readonly SqlDatabase _db = new();  // Tightly coupled
	public void PlaceOrder(Order order) { _db.Save(order); }
}

// GOOD - Depends on interface
public class OrderService
{
	private readonly IRepository<Order> _repository;
	public OrderService(IRepository<Order> repository) { _repository = repository; }
	public void PlaceOrder(Order order) { _repository.Save(order); }
}
```

---

## Design Patterns

### Q14: What are the most important design patterns for .NET web development?

**Answer:**

**1. Repository Pattern** (Abstract data access)
```csharp
public interface IRepository<T>
{
	Task<T> GetByIdAsync(int id);
	Task<List<T>> GetAllAsync();
	Task AddAsync(T entity);
	Task UpdateAsync(T entity);
	Task DeleteAsync(int id);
}

public class ProductRepository : IRepository<Product>
{
	public async Task<Product> GetByIdAsync(int id) =>
		await _context.Products.FindAsync(id);
}
```

**2. Unit of Work Pattern** (Manage transactions)
```csharp
public interface IUnitOfWork
{
	IRepository<Product> Products { get; }
	IRepository<Category> Categories { get; }
	Task<int> SaveChangesAsync();
}

public class UnitOfWork : IUnitOfWork
{
	private readonly ApplicationDbContext _context;
	public IRepository<Product> Products { get; }
	public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();
}
```

**3. Dependency Injection Pattern** (Manage object creation)
```csharp
// In Program.cs
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

// Use in PageModel
public class ProductModel : PageModel
{
	private readonly IProductService _service;  // Injected
	public ProductModel(IProductService service) { _service = service; }
}
```

**4. Factory Pattern** (Create objects)
```csharp
public interface INotificationFactory
{
	INotification CreateNotification(NotificationType type);
}

public class NotificationFactory : INotificationFactory
{
	public INotification CreateNotification(NotificationType type) =>
		type switch
		{
			NotificationType.Email => new EmailNotification(),
			NotificationType.SMS => new SmsNotification(),
			_ => throw new ArgumentException()
		};
}
```

---

## Dependency Injection & Middleware

### Q15: What are the three lifetimes in ASP.NET Core DI?
**Answer:**

| Lifetime | Duration | Use Case |
|----------|----------|----------|
| `Transient` | New instance every time | Stateless services |
| `Scoped` | Same for one HTTP request | DbContext, repositories |
| `Singleton` | Single instance for app lifetime | Configuration, caches, logging |

```csharp
// In Program.cs
builder.Services.AddTransient<IEmailService, EmailService>();  // New each time
builder.Services.AddScoped<IRepository<Product>, ProductRepository>();  // Per request
builder.Services.AddSingleton<IConfigService, ConfigService>();  // Shared

// Scoped is default for DbContext
builder.Services.AddDbContext<ApplicationDbContext>();  // Scoped
```

**Common mistakes:**
```csharp
// WRONG - Scoped service injected into Singleton
builder.Services.AddSingleton<MyService>(sp => new MyService(sp.GetRequiredService<IScoped>()));
// Problem: IScoped instance is captured for lifetime

// RIGHT - Get from service provider
builder.Services.AddSingleton<MyService>(sp =>
{
	return new MyService(() => sp.CreateScope().ServiceProvider.GetRequiredService<IScoped>());
});
```

---

### Q16: How do you write custom middleware?
**Answer:**

```csharp
// Custom middleware class
public class RequestLoggingMiddleware
{
	private readonly RequestDelegate _next;

	public RequestLoggingMiddleware(RequestDelegate next)
	{
		_next = next;
	}

	public async Task InvokeAsync(HttpContext context, ILogger<RequestLoggingMiddleware> logger)
	{
		logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");

		// Call next middleware
		await _next(context);

		logger.LogInformation($"Response: {context.Response.StatusCode}");
	}
}

// In Program.cs
app.UseMiddleware<RequestLoggingMiddleware>();
```

**Key points:**
- Constructor receives `RequestDelegate next`
- `Invoke` (or `InvokeAsync`) receives `HttpContext` + dependencies
- Must call `await _next(context)`
- Order matters - registered early runs first on request, last on response

---

## Security & Best Practices

### Q17: How do you prevent SQL Injection with Entity Framework?
**Answer:**

```csharp
// DANGEROUS - String concatenation
var id = "1 OR 1=1";  // Attacker input
var products = _context.Products
	.FromSql($"SELECT * FROM Products WHERE Id = {id}")  // SQL Injection!
	.ToList();

// SAFE - Parameterized queries with EF Core
var products = await _context.Products
	.FromSql($"SELECT * FROM Products WHERE Id = {id}")  // Parameters auto-created
	.ToListAsync();

// BEST - Use LINQ (no raw SQL)
var products = await _context.Products
	.Where(p => p.Id == int.Parse(id))  // Validated & parameterized
	.ToListAsync();
```

**Rule:** Always use `FromSql()` with interpolation (auto-parameterized), or better yet, use LINQ.

---

### Q18: Cross-Site Scripting (XSS) Prevention
**Answer:**

```html
<!-- DANGEROUS - Raw HTML output -->
<p>@Model.UserComment</p>  <!-- If comment has <script>, it runs! -->

<!-- SAFE - Encoded by default -->
<p>@Model.UserComment</p>  <!-- Safe in Razor Pages -->

<!-- For APIs returning JSON, beware: -->
public class CommentDto
{
	public string Text { get; set; }  // JSON escapes < > automatically
}
```

**Prevention:**
1. Razor Pages encode by default ✓
2. Validate/sanitize user input server-side
3. Use `HtmlEncoder` for manual encoding
4. Set CSP (Content Security Policy) headers

---

### Q19: What is CORS and when you need it?
**Answer:**
**CORS** = Cross-Origin Resource Sharing. Allows requests from different domains.

```csharp
// In Program.cs - Allow specific origins
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowFrontend", policy =>
	{
		policy.WithOrigins("https://example.com")
			  .AllowAnyMethod()
			  .AllowAnyHeader();
	});
});

app.UseCors("AllowFrontend");
```

**When needed:**
- Frontend (different domain) calls your API
- Mobile app calls backend API
- Not needed: Same domain requests

---

### Q20: Best practice - Validation patterns
**Answer:**

```csharp
// 1. Model-level validation (Data Annotations)
public class Product
{
	[Required]
	[StringLength(100, MinimumLength = 3)]
	public string Name { get; set; }

	[Range(0.01, 10000)]
	public decimal Price { get; set; }

	[EmailAddress]
	public string ContactEmail { get; set; }
}

// 2. Page-level validation (PageModel)
public class CreateModel : PageModel
{
	[BindProperty]
	[Required]
	public Product Product { get; set; }

	public async Task<IActionResult> OnPostAsync()
	{
		// ModelState checked automatically
		if (!ModelState.IsValid)
		{
			return Page();  // Redisplay with errors
		}

		await _repository.AddAsync(Product);
		return RedirectToPage("Index");
	}
}

// 3. Business logic validation (Service layer)
public class ProductService
{
	public async Task<Result> CreateProductAsync(Product product)
	{
		if (product.Price < 0)
			return Result.Failure("Price cannot be negative");

		if (await _repository.NameExistsAsync(product.Name))
			return Result.Failure("Product name already exists");

		await _repository.AddAsync(product);
		return Result.Success();
	}
}
```

---

## Quick Reference - Common Interview Red Flags

❌ **Avoid these in interviews:**
- Using `.Result` instead of `await`
- Forgetting to `await` `IQueryable` queries
- Mixing UI/data code
- Not using dependency injection
- Ignoring security (SQL injection, XSS)
- Blocking calls in async methods

✅ **Always mention:**
- "I'd write tests for this"
- "I'd handle exceptions appropriately"
- "I'd use dependency injection"
- "Consider async/await for I/O"
- "I'd validate input"

---

## Interview Tips

1. **Think aloud** - Explain your reasoning, not just answers
2. **Ask clarifying questions** - "Do we need to handle X case?"
3. **Consider edge cases** - Null values, empty collections, errors
4. **Optimization** - "First make it work, then make it fast"
5. **Trade-offs** - "This approach is simple but..." or "Performance-wise..."
6. **Show humility** - "I'm not 100% sure, but..." is better than guessing

---

Good Luck! 🚀
