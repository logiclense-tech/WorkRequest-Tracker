# Work Request Tracker

A focused assignment implementation using ASP.NET Core Web API, EF Core, SQL Server, and React.

## Structure
- `backend/WorkRequestTracker.Api` — .NET 8 Web API and EF Core
- `frontend` — React + Vite client
- `sql/schema.sql` — SQL Server schema and indexes

## Prerequisites
- .NET 8 SDK
- Node.js 20+ and npm
- SQL Server (or update the connection string to your local SQL Server instance)

## Run the API
1. Open `backend/WorkRequestTracker.Api/appsettings.json` and set `ConnectionStrings:WorkRequests` for your SQL Server.
2. From `backend/WorkRequestTracker.Api` run:
   ```bash
   dotnet restore
   dotnet run
   ```
3. The API uses `EnsureCreated` for a quick local demo. For a maintained application, use EF Core migrations (`dotnet ef migrations add InitialCreate`, then `dotnet ef database update`) instead. Swagger is available in Development.

## Run the React frontend
From `frontend`:
```bash
npm install
npm run dev
```
Vite proxies `/api` to `https://localhost:7086`. If your API uses another URL/port, update `vite.config.js` and `API_BASE` in `src/App.jsx`.

## API
- `GET /api/work-requests?status=&search=&page=1&pageSize=10`
- `GET /api/work-requests/{id}`
- `POST /api/work-requests`
- `PATCH /api/work-requests/{id}/status`
- `POST /api/work-requests/{id}/notes`

## Notes
- The assignment implementation intentionally keeps scope small.
- Authentication/authorization, audit history, formal status transition rules, automated test projects, and production deployment configuration are not included.
- Review the code, run it locally, and adjust configuration before submission. Do not claim tests or cloud deployment that you have not personally performed.
