# ToDoList API - ASP.NET Core Web API

A clean and maintainable .NET 8 Web API project structured using the Onion Architecture. This API allows users to manage their tasks (ToDo items) with support for CRUD operations, filtering, pagination, authentication, and validation.

---

## Project Architecture

This project follows the **Onion Architecture** principle for clean separation of concerns and testability.

```
 ToDoList
├── Api                   --> Presentation layer (Controllers, Middleware, Swagger)
├── Application           --> Business logic, DTOs, Interfaces, Services
├── Core                  --> Domain models and interfaces
├── Infrastructure        --> Data access implementations, EF Core, Repository pattern
├── Api.IntegrationTests  --> Integration tests for API endpoints
└── Application.UnitTests --> Unit tests for Application services
```

---

##  Features

-  User Registration & Login with JWT
-  Task Management (Create, Read, Update, Delete)
-  User Invitations with Role (Owner / Guest)
-  Filtering, Searching, and Pagination
-  FluentValidation for data validation
-  Global Error Handling
-  Unit & Integration Testing
-  Docker & Docker Compose setup
-  Swagger API Documentation
-  Logs stored using Serilog

---

##  Getting Started

### 1. Clone the repository

```bash
git clone https://gitlab.com/your-username/todolist-api.git
cd todolist-api
```

##  Run Without Docker (Local)
### 2. Configure the database (Local Development)

Edit `appsettings.json` in the `Api` project:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=ToDoListDB;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

### 3. Apply Migrations & Seed Data

```bash
cd Infrastructure
dotnet ef database update
```

### 4. Run the API

```bash
cd Api
dotnet run
```

### 5. Access Swagger UI

```
http://localhost/swagger/index.html
```

---


##  Running with Docker

### . Configure the database 

Edit `appsettings.json` in the `Api` project:

```json
"ConnectionStrings": {
     "DefaultConnection": "Server=db;Database=ToDoListDB;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;"
}
```bash
cd todolist/api
docker-compose up --build
```

Make sure Docker is installed and running. This will spin up the API with SQL Server.
No need for manual setup — the database will be created and seeded automatically.
---


## Authentication

To use protected endpoints, authenticate first:

- POST `/api/auth/token`
```json
{
  "username": "owner@elkood.com",
  "password": "P@ssw0rd"
}
```

Then use the token in the Authorization header:
```
Authorization: Bearer {your_token}
```

---

## Technologies Used

- ASP.NET Core 8
- Entity Framework Core
- Dependency injection
- SQL Server
- AutoMapper
- FluentValidation
- Swagger
- xUnit / Moq
- Docker

---
