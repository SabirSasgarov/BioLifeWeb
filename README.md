# BioLife

BioLife is an ASP.NET Core MVC e-commerce application for organic and grocery-style products. It includes a public storefront, product details, basket and checkout workflows, account management, reviews, subscribers, and an admin area for managing operational data.

## Tech Stack

- ASP.NET Core MVC (.NET 8)
- Entity Framework Core 8
- SQL Server
- ASP.NET Core Identity
- Google authentication
- Bootstrap, jQuery, Font Awesome

## Solution Structure

```text
BioLife
|-- BioLife.Domain          # Entities and shared domain types
|-- BioLife.Application     # Service interfaces and application registrations
|-- BioLife.Persistence     # EF Core DbContext, migrations, data seed, services
|-- BioLife.Infrastructure  # Infrastructure services such as email
|-- BioLife.MVC             # MVC controllers, views, models, static assets
```

## Main Features

- Product catalog with search, category filtering, sorting, and pagination
- Product details page with comments and ratings
- Basket and checkout flow
- User registration, login, email confirmation, password reset, and profile pages
- Google external login
- Subscriber collection and email sending
- Admin area for products, orders, users, subscribers, and reviews
- Seeded `Admin` and `Member` roles

## Requirements

- .NET 8 SDK
- SQL Server or SQL Server Express
- Visual Studio 2022, Rider, or VS Code
- EF Core tools, if you want to run migrations from the command line

Install EF tools if needed:

```bash
dotnet tool install --global dotnet-ef
```

## Configuration

The MVC project reads configuration from `BioLife.MVC/appsettings.json` and environment-specific files.

At minimum, configure:

- `ConnectionStrings:DefaultConnection`
- `EmailSettings:Host`
- `EmailSettings:Port`
- `EmailSettings:SenderEmail`
- `EmailSettings:Password`
- `Authentication:Google:ClientId`
- `Authentication:Google:ClientSecret`

For local development, prefer user secrets instead of committing credentials:

```bash
cd BioLife.MVC
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.;Database=BioLifeDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;"
dotnet user-secrets set "EmailSettings:Host" "smtp.gmail.com"
dotnet user-secrets set "EmailSettings:Port" "587"
dotnet user-secrets set "EmailSettings:SenderEmail" "your-email@example.com"
dotnet user-secrets set "EmailSettings:Password" "your-app-password"
dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
```

## Database Setup

Apply migrations from the solution root:

```bash
dotnet ef database update --project BioLife.Persistence --startup-project BioLife.MVC
```

When the application starts, it seeds:

- Roles: `Admin`, `Member`
- Default admin user: `admin@biolife.com`

Check `BioLife.Persistence/DataSeed.cs` for the seeded password and update it for real deployments.

## Run the Application

From the solution root:

```bash
dotnet restore
dotnet build
dotnet run --project BioLife.MVC
```

Then open the local URL printed by `dotnet run`, usually:

```text
https://localhost:5001
```

The admin area is available under:

```text
/Manage/Dashboard
```

## Common Commands

Build the solution:

```bash
dotnet build BioLife.sln
```

Add a new migration:

```bash
dotnet ef migrations add MigrationName --project BioLife.Persistence --startup-project BioLife.MVC
```

Update the database:

```bash
dotnet ef database update --project BioLife.Persistence --startup-project BioLife.MVC
```

Run the MVC app:

```bash
dotnet run --project BioLife.MVC
```

## Notes

- Admin controllers live under `BioLife.MVC/Areas/Manage`.
- Public storefront pages live mostly under `BioLife.MVC/Controllers` and `BioLife.MVC/Views`.
- Static assets are under `BioLife.MVC/wwwroot`.
- Do not commit real SMTP passwords, Google client secrets, or production connection strings.
