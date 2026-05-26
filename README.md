# HelpDesk Lite

Internal support ticketing system.

## **Epic 1: Authentication & User Access**

**Project location:** `/mnt/my_ntfs_drive/Career/iCareer/Technical Phase/HelpDesk Lite/`

### Stack

| Layer      | Technology                                        |
| ---------- | ------------------------------------------------- |
| Backend    | ASP.NET Core Web API (.NET 10)                    |
| ORM        | Entity Framework Core + PostgreSQL                |
| Auth       | JWT + RBAC (Employee, SupportAgent, ManagerAdmin) |
| Frontend   | React + TypeScript + Vite + TailwindCSS           |
| State      | React Query + Context API                         |
| Validation | FluentValidation                                  |
| Passwords  | ASP.NET Identity `PasswordHasher`                 |

### Quick start

#### 1. PostgreSQL

Ensure PostgreSQL is running, then create the database (default dev credentials are in `appsettings.Development.json`):

```bash
sudo systemctl start postgresql   # Linux, if needed
createdb -U postgres helpdesk_lite
```

Or in `psql`:

```sql
CREATE DATABASE helpdesk_lite;
```

**Configuration** — edit `backend/src/HelpDeskLite.Api/appsettings.Development.json`:

| Setting                               | Default (development)                                                               |
| ------------------------------------- | ----------------------------------------------------------------------------------- |
| `ConnectionStrings:DefaultConnection` | `Host=localhost;Port=5432;Database=helpdesk_lite;Username=postgres;Password=123456` |
| `Jwt:SigningKey`                      | Dev-only signing key (min 32 characters)                                            |

Production secrets should use environment variables or user secrets, not committed JSON.

#### 2. Database migration

```bash
cd backend
dotnet ef database update -p src/HelpDeskLite.Infrastructure -s src/HelpDeskLite.Api
```

**Create new migrations:**

```bash
dotnet ef migrations add <Name> -p src/HelpDeskLite.Infrastructure -s src/HelpDeskLite.Api
```

EF Core reads the connection string from the API startup project (`appsettings.Development.json` when `ASPNETCORE_ENVIRONMENT=Development`).

#### 3. Run API

```bash
cd backend/src/HelpDeskLite.Api
dotnet run
```

- Swagger: https://localhost:5001/swagger
- Health: https://localhost:5001/api/health

#### 4. Run frontend

```bash
cd frontend
npm install
npm run dev
```

Open http://localhost:5173 (API URL defaults to `https://localhost:5001` in `frontend/src/api/client.ts`).

### Epic 2: Ticket Submission

Employees can submit tickets at **http://localhost:5173/tickets/new** with:

- Subject, category, description, priority
- Optional file attachments (configurable size/type limits in `FileStorage` settings)
- Live knowledge base suggestions while typing the description

**API endpoints:**

| Method | Path                                          | Description                           |
| ------ | --------------------------------------------- | ------------------------------------- |
| POST   | `/api/tickets`                                | Create ticket (`multipart/form-data`) |
| GET    | `/api/categories`                             | List active categories                |
| GET    | `/api/knowledgebase/suggestions?description=` | KB article suggestions                |

After adding Epic 2 schema, run:

```bash
cd backend
dotnet ef database update -p src/HelpDeskLite.Infrastructure -s src/HelpDeskLite.Api
```

#### Seed users (Development)

| Email                   | Password     | Role         |
| ----------------------- | ------------ | ------------ |
| employee@helpdesk.local | Employee123! | Employee     |
| agent@helpdesk.local    | Agent123!    | SupportAgent |
| admin@helpdesk.local    | Admin123!    | ManagerAdmin |

#### API examples

##### Login

```bash
curl -k -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@helpdesk.local","password":"Employee123!"}'
```

##### Response:

```json
{
  "success": true,
  "data": {
    "accessToken": "eyJ...",
    "expiresAt": "2026-05-21T21:00:00Z",
    "user": {
      "id": "...",
      "email": "employee@helpdesk.local",
      "fullName": "Demo Employee",
      "role": "Employee"
    }
  }
}
```

##### Get tickets (with token)

```bash
TOKEN="<accessToken>"
curl -k https://localhost:5001/api/tickets \
  -H "Authorization: Bearer $TOKEN"
```

##### Current user

```bash
curl -k https://localhost:5001/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

##### Logout

```bash
curl -k -X POST https://localhost:5001/api/auth/logout \
  -H "Authorization: Bearer $TOKEN"
```

#### Authorization test matrix

| User         | Login | GET /api/tickets | GET /api/tickets/{otherUserTicket} |
| ------------ | ----- | ---------------- | ---------------------------------- |
| Employee     | OK    | Own only         | 403                                |
| SupportAgent | OK    | All              | OK                                 |
| ManagerAdmin | OK    | All              | OK                                 |

#### Solution structure

```
backend/
  src/
    HelpDeskLite.Api/           Controllers, Middleware, Authorization
    HelpDeskLite.Application/   DTOs, Interfaces, Validators
    HelpDeskLite.Domain/        Entities, Enums, Exceptions
    HelpDeskLite.Infrastructure/ EF Core, JWT, Services, Seed
frontend/
  src/
    api/        Axios client + React Query hooks
    auth/       AuthContext
    pages/      Login, Tickets
    components/ ProtectedRoute, Layout
```

#### Security notes

- Passwords are hashed with `PasswordHasher<User>` — never stored in plaintext
- JWT access tokens (default 15 minutes)
- Refresh token **entity and storage** are prepared; `/api/auth/refresh` returns 501 until next epic
- SSO: `IExternalAuthProvider` stub + `ExternalSubjectId` on `User`
- Use HTTPS in production; trust dev cert: `dotnet dev-certs https --trust`

#### HTTPS dev certificate

```bash
dotnet dev-certs https --trust
```

### Epic 3: Ticket Lifecycle Management

Support agents and managers can manage tickets through a full lifecycle on the ticket detail page (`/tickets/{id}`).

**Statuses:** New → Assigned → In Progress → Waiting for User → Resolved → Closed (invalid transitions blocked; managers can override).

**API endpoints:**

| Method | Path                         | Who                                             |
| ------ | ---------------------------- | ----------------------------------------------- |
| GET    | `/api/tickets/{id}`          | Owner (employee) or staff                       |
| GET    | `/api/tickets/{id}/history`  | Timeline (internal notes hidden from employees) |
| PATCH  | `/api/tickets/{id}/status`   | Support agent / manager                         |
| PATCH  | `/api/tickets/{id}/assign`   | Support agent / manager                         |
| POST   | `/api/tickets/{id}/comments` | All (employees: public only)                    |
| GET    | `/api/users/agents`          | Staff (assignee dropdown)                       |

Apply migration:

```bash
dotnet ef database update -p src/HelpDeskLite.Infrastructure -s src/HelpDeskLite.Api
```

**Test as agent:** `agent@helpdesk.local` / `Agent123!` — open a ticket, change status, assign, add internal/public comments.

### Epic 4: Dashboard & Visibility

Role-based dashboards with KPIs, searchable ticket queues, and manager reporting.

| Role          | Route                  | Features                                                     |
| ------------- | ---------------------- | ------------------------------------------------------------ |
| Employee      | `/dashboard`           | Open tickets, status overview, recent activity, quick create |
| Support agent | `/dashboard`, `/queue` | Queue KPIs, filters, search, sort, pagination, bulk assign   |
| Manager       | `/dashboard`, `/queue` | Workload, aging, resolution trends, delayed tickets          |

**API endpoints:**

| Method | Path                           | Who                     |
| ------ | ------------------------------ | ----------------------- |
| GET    | `/api/dashboard/employee`      | All authenticated users |
| GET    | `/api/dashboard/support-queue` | Support agent / manager |
| GET    | `/api/dashboard/manager`       | Manager only            |
| GET    | `/api/tickets/search`          | All (scoped by role)    |
| GET    | `/api/tickets/filter`          | Alias of search         |
| POST   | `/api/tickets/bulk-assign`     | Support agent / manager |
| GET    | `/api/reports/workload`        | Support agent / manager |
| GET    | `/api/reports/ticket-aging`    | Support agent / manager |

Dashboards refresh every 30 seconds via React Query polling.
