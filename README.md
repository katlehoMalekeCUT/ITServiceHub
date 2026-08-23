# ServiceHub IT

**ServiceHub IT** is an ASP.NET Core 8 MVC application designed to streamline IT help desk operations, including ticket management, asset management, maintenance, notifications, and reporting.

## Features

* 🔐 Authentication & role-based authorization
* 🎫 IT ticket management and assignment
* 💻 Asset management
* 🛠️ Maintenance management
* 🔔 Notifications
* 📚 Knowledge base
* 📊 Dashboard and reporting
* 🤖 Machine learning ticket prediction
* 📄 PDF and document processing
* 🗄️ Supabase backend integration
* 📱 Responsive Bootstrap 5 interface

## Screenshots

### Login

![Login](screenshots/login.png)

### Dashboard

![Dashboard](screenshots/dashboard.png)

### Ticket Management

![Tickets](screenshots/tickets.png)

### Asset Management

![Assets](screenshots/assets.png)

### Reports

![Reports](screenshots/reports.png)

## Tech Stack

* **Backend:** ASP.NET Core 8 MVC, C#
* **Frontend:** Razor Views, Bootstrap 5, JavaScript
* **Database:** Supabase
* **Machine Learning:** Microsoft ML.NET
* **Documents:** OpenXML, PDFPig

## Project Structure

```text
Controllers/     MVC controllers
Models/          Models and ViewModels
Views/           Razor views
Services/        Application and integration services
Repositories/    Data access
Interfaces/      Service and repository interfaces
DTOs/            Data transfer objects
Middleware/      Application middleware
wwwroot/         CSS, JavaScript and images
```

## Getting Started

### Requirements

* .NET 8 SDK
* Supabase project

### Run

```bash
dotnet restore
dotnet run
```

Open the application using the HTTPS URL displayed by ASP.NET Core.

### Configuration

Configure your Supabase credentials in `appsettings.json`:

```json
"Supabase": {
  "Url": "https://<your-project>.supabase.co",
  "ApiKey": "<your-api-key>",
  "RedirectUrl": "https://localhost:5001/Auth/Login"
}
```

> **Security:** Never commit real API keys or sensitive credentials to GitHub.

## License

Developed as an IT service management application using ASP.NET Core 8 MVC.
