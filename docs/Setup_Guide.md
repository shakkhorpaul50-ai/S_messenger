# Setup Guide

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full edition)
- [Node.js](https://nodejs.org/) (optional, for live reload development)

## Setup Steps

### 1. Clone the Repository

```bash
git clone <repository-url>
cd messenger-webapp
```

### 2. Configure the Database Connection

Open `backend-csharp/MessengerAPI/appsettings.json` and update the `ConnectionStrings:DefaultConnection` value:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=MessengerDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 3. Set Up the Database

**Option A: Run the schema script**

Execute `database/schema.sql` against your SQL Server instance using SQL Server Management Studio (SSMS), Azure Data Studio, or the `sqlcmd` CLI.

```bash
sqlcmd -S (localdb)\\MSSQLLocalDB -d MessengerDb -i database/schema.sql -E
```

**Option B: Use EF Core migrations**

```bash
cd backend-csharp/MessengerAPI
dotnet ef database update
```

### 4. (Optional) Seed Test Data

Run `database/seed_data.sql` to insert sample users and messages:

```bash
sqlcmd -S (localdb)\\MSSQLLocalDB -d MessengerDb -i database/seed_data.sql -E
```

### 5. Run the Backend

```bash
cd backend-csharp/MessengerAPI
dotnet run
```

The API will start on `https://localhost:5001` and `http://localhost:5000`.

### 6. Serve the Frontend

The frontend is a static SPA. Serve it with any HTTP server:

```bash
# Using dotnet serve
dotnet serve -d frontend -p 3000

# Using Python
python -m http.server 3000 -d frontend

# Using npx live-server
npx live-server frontend --port=3000
```

### 7. Open the Application

Open your browser to `http://localhost:3000` (or whichever port you chose).

## Configuration

### JWT Settings

In `appsettings.json`, configure the JWT token settings:

```json
{
  "Jwt": {
    "Key": "your-super-secret-key-at-least-32-characters-long",
    "Issuer": "MessengerAPI",
    "Audience": "MessengerApp",
    "ExpirationMinutes": 60
  }
}
```

### Connection String

Update the `DefaultConnection` in `appsettings.json` to match your SQL Server instance.
