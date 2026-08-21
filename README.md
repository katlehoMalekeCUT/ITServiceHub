# ServiceHub IT

ServiceHub IT is an ASP.NET Core 8 MVC application for IT help desk, asset management, ticketing, and reporting.

## Overview

This application includes:
- Authentication and authorization with cookie-based login
- Ticket, asset, department, maintenance, notification, and knowledge base management
- Dashboard views and reporting features
- Supabase integration for backend data access
- PDF processing and document import support
- Machine learning ticket prediction model
- Responsive Bootstrap 5 UI and standard MVC layout

## Prerequisites

- .NET 8 SDK installed
- A Supabase project with REST API access
- Optional: Visual Studio 2022/2023, Visual Studio Code, or another C# editor

## Configuration

The application reads Supabase settings from `appsettings.json`.

The required configuration section is:

```json
"Supabase": {
  "Url": "https://<your-supabase-project>.supabase.co",
  "ApiKey": "<your-supabase-api-key>",
  "RedirectUrl": "https://localhost:5001/Auth/Login"
}
```


## Run locally

1. Open a terminal in the project folder:

```powershell
cd "c:\Users\manue\OneDrive\Desktop\ServiceHub IT\ServiceHub IT"
```

2. Restore packages and run the app:

```powershell
dotnet restore
dotnet run
```

3. Open the web browser at:

```text
https://localhost:5001
```

The default application route is configured to `Auth/Login`.

## Project structure

- `Controllers/` - MVC controllers for pages and API actions
- `Models/` - domain models, view models, and configuration classes
- `Views/` - Razor views and shared layouts
- `Services/` - application services, Supabase integration, PDF processing, reporting, and ML
- `Repositories/` - data access and repository base classes
- `Interfaces/` - service and repository interfaces
- `DTOs/` - data transfer objects
- `Utilities/` - shared constants and helper code
- `Middleware/` - request logging and pipeline middleware
- `wwwroot/` - static assets, CSS, JS, and images

## Key packages

- `Supabase` for Supabase REST API integration
- `Microsoft.ML` for machine learning ticket prediction
- `DocumentFormat.OpenXml` for working with Office documents
- `UglyToad.PdfPig` for PDF parsing and processing

## Notes

- The app uses HTTPS and cookie authentication by default.
- Authorization is enabled globally, so users must sign in to access protected routes.
- If you change the local port or hostname, update `Supabase:RedirectUrl` accordingly.

## Troubleshooting

- If the app does not start, verify your .NET SDK installation with:

```powershell
dotnet --info
```

- If login or Supabase fails, confirm the `Supabase` section in `appsettings.json` is correct.
- If you use a different development port, update the redirect URL in configuration.
