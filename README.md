# Smart Inventory Management System

## Overview

Smart Inventory Management System is an ASP.NET Core MVC web application designed to manage products, categories, and customer orders in an organized inventory environment.

The system supports authentication, role-based access, product tracking, category management, order processing, and email-based account features.

---

## Features

* User registration and login using ASP.NET Identity
* Role-based authentication and authorization
* Product management (add, update, delete, search)
* Category management
* Order creation and tracking
* Inventory summary views
* Email integration for account confirmation and password reset
* Error handling with custom pages
* Logging with Serilog
* Unit testing support

---

## Technologies Used

* ASP.NET Core MVC
* C#
* Entity Framework Core
* PostgreSQL
* ASP.NET Identity
* Serilog
* Razor Pages

---

## Project Structure

```plaintext
Controllers/        -> Main controllers
Areas/              -> Product management and identity modules
Models/             -> Data models
Views/              -> Razor views
Data/               -> Database context and seed data
Services/           -> Email service and supporting services
Migrations/         -> Entity Framework migrations
UnitTests/          -> Unit testing files
wwwroot/            -> Static files (CSS, JS, images)
```

---

## Main Modules

### Product Management

* Add new products
* Update existing products
* Delete products
* Search products

### Category Management

* Add categories
* View category list

### Order Management

* Create orders
* View order summaries
* Track orders

### Authentication

* Register account
* Login / Logout
* Password reset
* Email confirmation

---

## Database Setup

Update your connection string in:

```json
appsettings.json
```

Example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=YourDatabaseName;Username=your_username;Password=your_password"
}
```

Run migrations:

```bash
dotnet ef database update
```

---

## Running the Project

```bash
dotnet restore
dotnet build
dotnet run
```

Then open:

```plaintext
https://localhost:5001
```

or

```plaintext
http://localhost:5000
```

---

## Logging

The project uses Serilog for logging.

Logs include:

* Timestamp
* User information
* Error details
* Request endpoint

---
