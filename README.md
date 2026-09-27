SmartInventoryAPI
---------------------

SmartInventoryAPI is an ASP.NET Core Web API project for managing users, products, shopping carts, and orders.

The project uses JWT authentication and role-based authorization to provide different access levels for Admin and Customer users.



Technologies Used
---------------------------

- C#
- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger / OpenAPI
- LINQ
- REST APIs
- xUnit


Project Features
========================

User Management
-------------------------

- User registration
- User login
- JWT token generation
- JWT authentication
- Role-based authorization
- Customer and Admin roles
- Authenticated user profile

Product Management
------------------------------

- Create product
- View products
- Update product
- Partially update product
- Delete product
- Product search
- Pagination
- Stock management
- Low-stock report

Cart Management
-------------------------------

- Add product to cart
- View carts
- Remove product from cart
- Quantity validation
- Stock validation
- Customer-specific cart access
 
 Order Management
 -----------------------------

- Place order
- Calculate order total
- Create order items
- Reduce product stock after order
- Clear cart after successful order
- View customer's own orders
- Admin can view all orders
- Search orders by product name
- View order by ID
- Update order status
- Cancel order
- Restore product stock when an order is cancelled

 Security
 ---------------------------------

- JWT authentication
- Role-based authorization
- Admin-only APIs
- Customer-specific order access
-  Customer-specific cart access
- Global exception handling
- Input validation

---

 Project Structure
 -----------------------------

text
SmartInventoryAPI
│
├── Controllers
│   ├── UserController.cs
│   ├── ProductController.cs
│   ├── CartController.cs
│   └── OrderController.cs
│
├── Data
│   └── AppDbContext.cs
│
├── DTOs
│   ├── LoginDto.cs
│   └── RegisterDto.cs
│
├── Middleware
│   └── ExceptionMiddleware.cs
│
├── Models
│   ├── User.cs
│   ├── Product.cs
│   ├── Cart.cs
│   ├── CartItem.cs
│   ├── Order.cs
│   └── OrderItem.cs
│
├── Migrations
│
├── Program.cs
├── appsettings.json
├── SmartInventoryAPI.csproj
├── SmartInventoryAPI.Tests.csproj
└── SmartInventoryAPI.slnx
