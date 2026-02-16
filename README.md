# Pantry Expiry Tracker

A simple web application to track pantry items and their expiration dates. Built with ASP.NET Core Web API, EF Core, and vanilla JavaScript.

## Features

- Add, view, and delete pantry items
- Track items by name, quantity, best-before date, opened status, and notes
- Filter items by name, opened status, and expiration date
- View items expiring within a specified number of days
- Toggle opened/closed status with one click
- Visual expiry status indicators (expired, expiring soon, good)
- Seed data on first run (10 sample items)
- RESTful API with proper validation and error handling

## Tech Stack

- **Backend**: ASP.NET Core 10.0 Web API
- **Database**: SQLite with Entity Framework Core 10.0
- **Frontend**: Plain HTML + JavaScript (no frameworks)

## Running Locally

### Prerequisites

- .NET 10.0 SDK

### Steps to Run

1. **Navigate to the project directory:**
   ```bash
   cd PantryTracker
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the application:**
   ```bash
   dotnet run
   ```

The application will start at a dynamic port (e.g., http://localhost:5000, http://localhost:5146, etc.). Check the console output for the exact URL.

**Access the app:**
- Frontend: http://localhost:{PORT}
- API: http://localhost:{PORT}/api/pantryitems

> **Note:** On first run, the application automatically creates the SQLite database (`pantry.db`) and seeds it with 10 sample items.

### To Stop the Server

Press `Ctrl+C` in the terminal where the server is running.

## API Endpoints

| Method | Endpoint                              | Description                                                           |
| ------ | ------------------------------------- | --------------------------------------------------------------------- |
| GET    | `/api/pantryitems`                    | List all items (supports `q`, `opened`, `expiresBefore` query params) |
| GET    | `/api/pantryitems/{id}`               | Get a specific item by ID                                             |
| POST   | `/api/pantryitems`                    | Create a new item                                                     |
| GET    | `/api/pantryitems/expiring?days=N`    | Get items expiring within N days                                      |
| PATCH  | `/api/pantryitems/{id}/toggle-opened` | Toggle the opened status                                              |
| DELETE | `/api/pantryitems/{id}`               | Delete an item                                                        |

## Data Model

### PantryItem

| Field      | Type   | Required | Validation         |
| ---------- | ------ | -------- | ------------------ |
| id         | int    | Yes      | Auto-generated     |
| name       | string | Yes      | Max 100 characters |
| quantity   | int    | Yes      | Must be >= 0       |
| bestBefore | date   | Yes      | Valid date         |
| isOpened   | bool   | Yes      | -                  |
| notes      | string | No       | Max 500 characters |

### Example POST Request

```json
{
  "name": "Organic Milk",
  "quantity": 1,
  "bestBefore": "2026-02-20",
  "isOpened": true,
  "notes": "Open - use within 5 days"
}
```

## Assumptions & Design Decisions

1. **Date Handling**: All dates are stored and compared as UTC (DateOnly). The frontend displays dates in the user's local timezone format.

2. **Expiring Items**: The `/expiring` endpoint returns items where `bestBefore <= today + N days`. This includes already-expired items.

3. **Database**: SQLite is used for simplicity and portability. The database file (`pantry.db`) is created automatically on first run.

4. **CORS**: CORS is configured to allow any origin for development purposes. In production, this should be restricted.

5. **Logging**: Structured logging is included using ASP.NET Core's built-in ILogger for debugging and monitoring.

## Learning Log

During this assignment, I learned and researched:

1. **.NET 10.0**: Explored the latest ASP.NET Core features including minimal APIs vs controllers, and chose controllers for better organization with multiple related endpoints.

2. **EF Core 10.0**: Learned about `EnsureCreated()` vs migrations for development, and how to use DateOnly type (newer .NET feature) for date-only values.

3. **Validation**: Discovered that `[Required]` and data annotations work automatically with controllers for model binding, but custom validation logic was needed for the record type used in POST requests.

4. **SQLite**: Learned that SQLite doesn't support all SQL Server features, but works great for this use case. The connection string format is simpler than I expected.

5. **Static Files**: Configured `UseDefaultFiles()` and `UseStaticFiles()` to serve the frontend from wwwroot without needing a separate server.

6. **Fetch API**: Used async/await patterns with the browser's fetch API, learning to handle different HTTP methods (GET, POST, PATCH, DELETE) and error responses.

7. **Date Handling in JavaScript**: Learned that `input type="date"` returns ISO format strings, and comparing dates requires normalizing timestamps to handle timezone differences.

## What I'd Improve Next

1. **Tests**: Add unit tests for controller logic and integration tests for API endpoints.

2. **Authentication**: Add user authentication so each user has their own pantry.

3. **Categories/Tags**: Add categorization for better organization (dairy, produce, canned goods, etc.).

4. **Email Notifications**: Add optional email alerts for items about to expire.

5. **Mobile App**: Create a React Native or PWA version for mobile use.

6. **Database Migrations**: Switch from `EnsureCreated()` to proper migrations for production schema management.

7. **Pagination**: Add pagination to the list endpoint for large datasets.
# pantry-tracker
