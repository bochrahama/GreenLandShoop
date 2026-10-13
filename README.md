
# 🌿 Green Land Shop — E-Commerce Backend API

A RESTful backend for **Green Land Shop**, an online store for nature-inspired products. Built with ASP.NET Core 8, Entity Framework Core and SQL Server.

## Features

- Product catalog with categories, search and price filtering
- User registration and login with JWT authentication
- Role-based authorization (Admin and Customer)
- Shopping cart for each user
- Transactional checkout: an order is fully created or fully rolled back
- Stock management with optimistic concurrency to prevent overselling
- Order history with price snapshots, so past orders never change
- Soft delete for products to preserve order history
- Input validation and global exception handling
- Unit and integration tests

## Tech Stack

- ASP.NET Core Web API (.NET 8)
- Entity Framework Core 8 (Code First, Migrations)
- SQL Server
- ASP.NET Core Identity and JWT Bearer
- Swagger / OpenAPI
- xUnit
- Git and GitHub

## Architecture

The project follows a layered architecture:

- **Controllers** handle HTTP requests, routing and authorization only.
- **Services** contain the business rules, such as stock validation and checkout.
- **Data access** uses a thin generic repository for simple CRUD, and `AppDbContext` directly for filtering, joins and transactions.
- **DTOs** separate the API contract from the domain model.
- **Middleware** handles exceptions globally.

## Getting Started

Prerequisites: .NET10 SDK, SQL Server (LocalDB) and the `dotnet-ef` tool.

run
```

Open `https://localhost:<port>/swagger` to explore the API.

Run the tests with:


# What I Learned

- Designing a relational database before writing code
- Structuring a layered application with dependency injection
- Implementing JWT authentication and role-based authorization
- Handling transactions and concurrency in checkout workflows
- Writing validation, DTO mapping and centralized error handling


Hamadate Bochra — Computer Science student, M'Hamed Bougara University of Boumerdès (UMBB)
