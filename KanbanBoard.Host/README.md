# Kanban Board

> ASP.NET Core • Entity Framework Core • SQL Server • SignalR • Angular 21 • Angular Material
> .NET 10 | Clean Architecture | Real-Time Collaboration | JWT Auth | Drag and Drop

---

## Purpose of the Application

The Kanban Board is a real time collaborative project management application that allows multiple users to manage tasks across customisable columns. Users can create boards, invite members, and drag cards between columns — with all changes instantly visible to every connected user via WebSockets.

Built to demonstrate real time systems engineering, clean layered architecture, board level role based access control, and a responsive Angular Material dashboard with drag and drop functionality.

---

## Tech Stack

### Backend

- ASP.NET Core (.NET 10)
- Entity Framework Core — ORM and database migrations
- SQL Server — relational database
- SignalR — real time WebSocket communication
- AutoMapper v16 — DTO ↔ entity mapping
- Serilog — structured logging with console and file sinks
- ASP.NET Identity (AddIdentityCore) — user management and password hashing
- JWT Bearer Authentication
- Board level role based access control (Admin, Member)
- Swashbuckle / Swagger — API documentation and testing UI

### Frontend

- Angular 21 — standalone component architecture
- Angular Material 21 — UI component library
- Angular CDK — drag and drop between columns
- @microsoft/signalr — SignalR client for real time updates
- Reactive Forms — form state management and validation
- JWT interceptor — automatic token attachment on all HTTP requests
- Route guards — authentication and page protection

---

## Solution Structure

### Backend

```
KanbanBoard/
├── Host/                        ← Controllers, Middleware, Program.cs
├── Application/                 ← Services, Interfaces, DTOs, Mapping
├── Domain/                      ← Entities (no dependencies)
└── Infrastructure/              ← Repositories, DbContext, Identity, Hubs
```

### Frontend

```
kanban-client/src/app/
├── core/
│   ├── services/                ← AuthService, SignalRService
│   ├── interceptors/            ← authInterceptor, errorInterceptor
│   └── guards/                  ← authGuard
├── features/
│   ├── auth/
│   │   ├── login/               ← Login page
│   │   └── register/            ← Register page
│   ├── boards/
│   │   ├── boards/              ← Board list page
│   │   └── board-form/          ← Create board dialog
│   └── board-detail/
│       ├── board-detail/        ← Board detail with drag and drop
│       ├── column-form/         ← Create and edit column dialog
│       └── card-form/           ← Create and edit card dialog
└── shared/
    ├── models/                  ← TypeScript interfaces mirroring backend DTOs
    ├── navigation/              ← Sidebar navigation shell
    └── confirm-dialog/          ← Reusable confirmation dialog
```

---

## Domain Model

### Entities

- **Board** — top level container representing a project. Owned by a user, shared with members.
  - `CreatedBy` stores the user ID of the creator as a string to match ASP.NET Identity's string typed Id
  - Has many `Column` entities and many `BoardMember` entities

- **Column** — a swimlane on the board (e.g. To Do, In Progress, Done)
  - `Position` integer determines left to right order
  - Has many `Card` entities
  - Names must be unique within a board — enforced at service layer

- **Card** — an individual task within a column
  - `ColumnId` foreign key determines which column the card belongs to
  - Moving a card between columns updates only `ColumnId`
  - Ordered by `CreatedAt` within a column

- **BoardMember** — join table representing a user's membership on a board
  - Composite primary key on `{ BoardId, UserId }` — a user can only appear once per board
  - `Role` property carries the user's permission level on that specific board
  - Roles: `Admin` (manage board structure) or `Member` (manage cards only)

- **AppUserIdentity** — extends IdentityUser with FirstName and LastName
  - Lives in Infrastructure — Application layer has no dependency on Identity

---

## Architecture

### Host
- Exposes HTTP endpoints via controllers
- Global exception middleware maps exceptions to HTTP status codes
- JWT authentication and authorisation configured in middleware pipeline
- SignalR hub mapped at `/hubs/kanban`

### Application
- Contains all business logic and permission checking
- Services verify board membership before every operation
- `userId` passed into every service method — extracted from JWT claims in controllers
- `IKanbanNotificationService` abstraction keeps SignalR out of Application layer

### Domain
- Plain C# entities
- Repository interfaces defined here — Application depends on these abstractions
- No framework dependencies

### Infrastructure
- Repository implementations backed by EF Core
- `KanbanDbContext` inheriting from `IdentityDbContext`
- `AddIdentityCore` used instead of `AddIdentity` to prevent default auth scheme override
- `KanbanHub` — SignalR hub managing board group membership
- `KanbanNotificationService` — implements `IKanbanNotificationService`, broadcasts events via `IHubContext`
- `IDesignTimeDbContextFactory` enables EF Core migrations without bootstrapping the full application

---

## Role Based Access Control

Roles in this application are **board specific** — not global. A user can be Admin on one board and Member on another.

| Role | Permissions |
|------|-------------|
| **Admin** | Create, rename and delete columns. Delete the board. All card operations. |
| **Member** | Create, edit, move and delete cards only. Cannot modify board structure. |

Permission checking happens in the service layer on every operation:

```csharp
var membership = await boardMemberRepository.GetByBoardAndUser(boardId, userId);
if (membership == null || membership.Role != "Admin")
    throw new UnauthorizedAccessException("Only board Admins can create columns.");
```

The JWT token carries no role claim — roles are data, not claims. The `userId` from the token is used to look up the user's role on the specific board being accessed.

---

## Real Time Architecture

SignalR enables instant updates across all connected users viewing the same board.

### How it works

```
1. User opens a board
   → Angular establishes WebSocket connection to /hubs/kanban
   → Calls JoinBoard(boardId) — added to SignalR group for that board

2. User moves a card
   → Angular calls POST /api/cards/{id}/move
   → CardService updates database
   → KanbanNotificationService broadcasts CardMoved to board group
   → All connected users receive the event instantly
   → Angular updates local board state — card moves on screen
   → No page refresh needed

3. User navigates away
   → Angular calls LeaveBoard(boardId)
   → WebSocket connection closed
   → User removed from board group
```

### SignalR Events

| Event | Payload | Trigger |
|-------|---------|---------|
| `CardMoved` | `CardResponseDto` | Card moved between columns |
| `CardCreated` | `CardResponseDto` | New card created |
| `CardDeleted` | `cardId, columnId` | Card deleted |

### JWT Authentication for SignalR

WebSockets cannot send custom headers so SignalR passes the JWT token as a query parameter. The JWT middleware is configured to read tokens from query strings for hub requests:

```csharp
opt.Events = new JwtBearerEvents
{
    OnMessageReceived = context =>
    {
        var accessToken = context.Request.Query["access_token"];
        var path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken) &&
            path.StartsWithSegments("/hubs/kanban"))
        {
            context.Token = accessToken;
        }
        return Task.CompletedTask;
    }
};
```

---

## API Endpoints

### Auth

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Register a new user |
| `POST` | `/api/auth/login` | Authenticate and receive JWT token |

### Boards

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/boards` | Get all boards for the current user |
| `GET` | `/api/boards/{id}` | Get a board with all columns and cards |
| `POST` | `/api/boards` | Create a new board |
| `PUT` | `/api/boards/{id}` | Rename a board *(Admin only)* |
| `DELETE` | `/api/boards/{id}` | Delete a board and all its data *(Admin only)* |

### Columns

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/boards/{boardId}/columns` | Add a column to a board *(Admin only)* |
| `PUT` | `/api/columns/{id}` | Rename a column *(Admin only)* |
| `DELETE` | `/api/columns/{id}` | Delete a column and all its cards *(Admin only)* |

### Cards

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/columns/{columnId}/cards` | Create a card in a column |
| `PUT` | `/api/cards/{id}` | Update a card title or description |
| `POST` | `/api/cards/{id}/move` | Move a card to a different column |
| `DELETE` | `/api/cards/{id}` | Delete a card |

---

## Business Rules

- Column names must be unique within a board — enforced at service layer with case insensitive comparison
- A card cannot be moved to the column it already belongs to
- A card cannot be moved to a column on a different board
- Deleting a column cascades to delete all its cards
- Deleting a board cascades to delete all columns, cards, and member records
- The board creator is automatically added as Admin when the board is created
- Any registered user can create boards — there are no global application roles

---

## Frontend Features

### Drag and Drop
- Cards can be dragged between columns using Angular CDK
- Optimistic update — card moves instantly on screen before API response
- If the API call fails the board reloads to restore correct state
- `cdkDropListGroup` connects all columns automatically

### Real Time
- SignalR connection established when a board is opened
- Connection torn down when navigating away via `ngOnDestroy`
- Event listeners removed on destroy to prevent memory leaks
- `withAutomaticReconnect()` handles transient connection drops

### Error Handling
- Error interceptor transforms all HTTP errors into readable messages
- Backend error messages surfaced directly to the user
- Generic fallback messages for network and server errors

---

## Requirements to Run

- .NET 10 SDK
- SQL Server (local or remote instance)
- Node.js 22+ and npm
- Angular CLI 21 (`npm install -g @angular/cli`)
- Visual Studio 2022 (backend) and Visual Studio Code (frontend)

---

## Running the Application

### Backend

1. Update the connection string in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=KanbanBoardDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

2. Apply database migrations from the solution root:

```bash
dotnet ef migrations add InitialCreate --project KanbanBoard.Infrastructure --startup-project KanbanBoard.Host
dotnet ef database update --project KanbanBoard.Infrastructure --startup-project KanbanBoard.Host
```

3. Run the API:

```bash
dotnet run --project KanbanBoard.Host
```

4. Open Swagger UI:

```
https://localhost:{port}/swagger
```

### Frontend

1. Navigate to the Angular project:

```bash
cd kanban-client
```

2. Install dependencies:

```bash
npm install
```

3. Update the API and hub URLs in `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:{port}/api',
  hubUrl: 'https://localhost:{port}/hubs/kanban'
};
```

4. Start the development server:

```bash
ng serve
```

5. Open the application:

```
http://localhost:4200
```

---

## NuGet Packages

| Package | Purpose |
|---------|---------|
| `AutoMapper v16` | Entity to DTO mapping |
| `Microsoft.EntityFrameworkCore.SqlServer` | SQL Server EF Core provider |
| `Microsoft.EntityFrameworkCore.Tools` | EF Core migrations CLI |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | JWT Bearer authentication |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | ASP.NET Identity with EF Core |
| `Microsoft.AspNetCore.SignalR` | Real time WebSocket communication |
| `Serilog.AspNetCore` | Structured logging |
| `Serilog.Sinks.File` | Log output to file |
| `Swashbuckle.AspNetCore` | Swagger UI and API documentation |

---


## Key Engineering Concepts Demonstrated

### Backend
- Clean layered architecture — Host, Application, Domain, Infrastructure
- Repository pattern with interfaces in Domain — Application depends on abstractions
- Service layer owns all business logic and permission checking
- Board level RBAC — roles stored as data in `BoardMember`, checked at service layer
- Composite primary key on `BoardMember` — enforces one membership per user per board
- SignalR abstraction — `IKanbanNotificationService` keeps Infrastructure concerns out of Application
- JWT query string authentication — required for WebSocket connections
- `AddIdentityCore` pattern — prevents Identity overriding JWT as default auth scheme
- `IDesignTimeDbContextFactory` — decouples migrations from application startup
- Cascade deletes — columns cascade to cards, boards cascade to everything
- Ordered includes — columns ordered by Position, cards ordered by CreatedAt

### Frontend
- Standalone component architecture — no NgModules
- Feature based folder structure — code organised by domain
- Lazy loaded routes with preloading — fast navigation after initial load
- JWT interceptor — single point of token attachment
- Error interceptor — single point of HTTP error transformation
- SignalR service — manages WebSocket lifecycle and event subscriptions
- `ngOnDestroy` cleanup — event listeners removed, connection closed on navigation
- Angular CDK drag and drop with `cdkDropListGroup` — automatic cross-column connection
- Optimistic updates — UI reflects changes before API confirmation
- `ChangeDetectorRef` — manual change detection for async SignalR callbacks
- Reactive forms with inline validation

---

---

## Docker

The application is fully containerised using Docker and Docker Compose.

### Prerequisites
- Docker Desktop with virtualisation enabled in BIOS
- Or any Docker-compatible container runtime

### Services

| Service | Image | Port |
|---------|-------|------|
| `kanban-api` | .NET 10 ASP.NET Core | Internal only |
| `kanban-frontend` | Nginx + Angular build | 4300 |
| `sqlserver` | SQL Server 2022 Express | 1434 (dev only) |

### Running with Docker Compose

1. Copy the example environment file and fill in your values:
```bash
cp .env.example .env
```

2. Start all services:
```bash
docker compose up --build
```

3. Open the application:
```
http://localhost:4300
```

4. Stop all services:
```bash
docker compose down
```

### Architecture
```
Browser → localhost:4300
            ↓
          Nginx (kanban-frontend)
            ↓ /api/   →  kanban-api:8080
            ↓ /hubs/  →  kanban-api:8080 (SignalR WebSocket)
            ↓ /       →  Angular static files
```

The API container is not exposed directly — all traffic enters through Nginx. WebSocket connections for SignalR are proxied through the `/hubs/` location block with the correct `Upgrade` and `Connection` headers to enable the WebSocket handshake. SQL Server is only reachable internally by the API container.

### Note on Virtualisation

Docker requires hardware virtualisation to be enabled in BIOS. If Docker Desktop shows a virtualisation error enable `Intel Virtualization Technology` or `SVM Mode` in your BIOS settings.

---

## Kubernetes

Kubernetes manifests are provided in the `k8s/` folder for deployment to a Kubernetes cluster.

### Prerequisites
- Minikube — local Kubernetes cluster
- kubectl — Kubernetes CLI
- Hardware virtualisation enabled in BIOS

### Manifest Structure
```
k8s/
├── secrets.yaml                      ← JWT key and DB password (base64 encoded)
├── configmap.yaml                    ← Non-sensitive configuration
├── sqlserver-deployment.yaml         ← SQL Server pod and persistent volume
├── sqlserver-service.yaml            ← Internal SQL Server endpoint
├── kanban-api-deployment.yaml        ← API pod with health probes
├── kanban-api-service.yaml           ← Internal API endpoint
├── kanban-frontend-deployment.yaml   ← Nginx + Angular pod
├── kanban-frontend-service.yaml      ← Frontend endpoint (NodePort for dev)
└── ingress.yaml                      ← External traffic routing
```

### Deploying to Minikube

1. Start Minikube:
```bash
minikube start
```

2. Enable the Ingress addon:
```bash
minikube addons enable ingress
```

3. Apply all manifests:
```bash
kubectl apply -f k8s/
```

4. Get the Minikube IP:
```bash
minikube ip
```

5. Add to your hosts file (`C:\Windows\System32\drivers\etc\hosts`):
```
<minikube-ip>  kanban.local
```

6. Open the application:
```
http://kanban.local
```

### Health Checks

The API exposes a `/health` endpoint used by Kubernetes probes:
```
Liveness probe  → restarts pod if unhealthy
Readiness probe → removes pod from load balancer if not ready
```

### Scaling

To scale the API to multiple replicas:
```bash
kubectl scale deployment kanban-api --replicas=3
```

Kubernetes automatically load balances requests across all healthy replicas.

### Challenges and Design Decisions

**Secrets management** — sensitive values are stored as Kubernetes Secrets encoded in base64 and never appear in plain text in any manifest file. In a production environment these would be managed by a dedicated secrets manager such as Azure Key Vault or AWS Secrets Manager.

**Persistent storage** — SQL Server uses a PersistentVolumeClaim to ensure database files survive pod restarts and redeployments.

**Health probes** — liveness and readiness probes use the `/health` endpoint to give Kubernetes visibility into application health enabling automatic recovery from failures.

**SignalR WebSocket support** — the Nginx Ingress is configured with extended proxy timeouts of 3600 seconds to keep SignalR WebSocket connections alive for the duration of a user session. Without this Nginx would close connections after its default 60 second timeout, forcing SignalR to fall back to long polling.

**NodePort vs Ingress** — the frontend service uses NodePort for local Minikube development. In production this would be replaced with ClusterIP and all external traffic would route through the Ingress controller.

## Future Improvements

- Card assignments — assign cards to specific board members
- Due dates and labels on cards
- Board member management UI — invite and remove members
- Pagination on board and card endpoints
- Board level activity feed
- Docker containerisation
- Kubernetes deployment
- Redis backplane for SignalR — required when scaling to multiple API instances
- NgRx state management for production scale client side state
- Unit and integration test suite
