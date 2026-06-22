# Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      Client Browser                      │
│  ┌───────────────────────────────────────────────────┐  │
│  │              Single Page Application              │  │
│  │  ┌──────────┐  ┌──────────┐  ┌────────────────┐  │  │
│  │  │  Auth UI  │  │  Chat UI │  │  SignalR Client │  │  │
│  │  └──────────┘  └──────────┘  └────────────────┘  │  │
│  └───────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────┘
                       │ HTTP / WebSocket
                       ▼
┌─────────────────────────────────────────────────────────┐
│                    ASP.NET Core Server                    │
│  ┌──────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │   REST   │  │  SignalR Hub │  │   JWT Auth        │  │
│  │ Controllers│  │  (ChatHub)   │  │   Middleware      │  │
│  └──────────┘  └──────────────┘  └───────────────────┘  │
│  ┌───────────────────────────────────────────────────┐  │
│  │              Service Layer (Business Logic)        │  │
│  └───────────────────────────────────────────────────┘  │
│  ┌───────────────────────────────────────────────────┐  │
│  │         Entity Framework Core (Data Access)        │  │
│  └───────────────────────────────────────────────────┘  │
└──────────────────────┬──────────────────────────────────┘
                       │ SQL
                       ▼
┌─────────────────────────────────────────────────────────┐
│                     SQL Server                           │
│  ┌──────────┐  ┌──────────────┐  ┌───────────────────┐  │
│  │  Users   │  │ Conversations│  │     Messages      │  │
│  └──────────┘  └──────────────┘  └───────────────────┘  │
│  ┌───────────────────────────────────────────────────┐  │
│  │                  Participants                      │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

## Frontend

A static Single Page Application (SPA) built with vanilla JavaScript. No framework dependencies.

- **HTTP Client:** `fetch()` API for REST calls
- **Real-time:** `@microsoft/signalr` library for WebSocket communication
- **Auth:** JWT tokens stored in memory/localStorage, attached via `Authorization: Bearer` header
- **Routing:** Hash-based client-side routing (`#/login`, `#/chat`, etc.)
- **Styling:** Plain CSS with a responsive layout

## Backend

ASP.NET Core 8 Web API with SignalR.

- **REST API:** Controllers with attribute routing (`[ApiController]`, `[Route("api/[controller]")]`)
- **Real-time Hub:** `ChatHub` extending `Hub<T>` with strongly-typed client interface
- **ORM:** Entity Framework Core 8 with SQL Server provider
- **Auth:** JWT Bearer token authentication, validated on every request
- **Validation:** Data annotations on DTOs and `[FromBody]` model binding
- **CORS:** Configured to allow the frontend origin

## Folder Structure

```
messenger-webapp/
├── backend-csharp/
│   └── MessengerAPI/
│       ├── Controllers/        # REST API controllers
│       ├── Hubs/               # SignalR hubs (ChatHub)
│       ├── Models/             # Entity classes (EF Core)
│       ├── DTOs/               # Request/response data transfer objects
│       ├── Services/           # Business logic services
│       ├── Data/               # DbContext and migrations
│       ├── Middleware/         # Custom middleware
│       └── Program.cs          # Application entry point
├── frontend/
│   ├── index.html              # Main HTML shell
│   ├── css/                    # Stylesheets
│   ├── js/                     # JavaScript modules
│   └── lib/                    # Third-party libraries (signalr)
├── database/
│   ├── schema.sql              # Full database schema
│   ├── seed_data.sql           # Test data
│   └── migrations/             # EF Core SQL migration scripts
├── docs/                       # Documentation files
├── .gitignore
└── README.md
```

## Data Flow: Sending a Message

1. User types a message and clicks send in the frontend.
2. The frontend calls `connection.invoke("SendMessage", { conversationId, content })` on the SignalR hub.
3. The `ChatHub.SendMessage` method on the server validates the user, saves the message to the database via EF Core, and broadcasts `MessageReceived` to all participants in the conversation group.
4. The sender and all other online participants receive the `MessageReceived` event and append the message to their chat UI.
5. If a user is offline, they will fetch missed messages via `GET /api/messages/conversation/{id}` when they come online.

## Authentication Flow

1. User registers or logs in via `POST /api/auth/login`.
2. Server validates credentials and returns a JWT token.
3. Frontend stores the token and includes it in all subsequent API requests via the `Authorization` header.
4. The SignalR connection also uses the token via `accessTokenFactory`.
5. The `JwtMiddleware` validates the token on every request and sets `HttpContext.User`.
6. When the token expires, the user is redirected to the login page.
