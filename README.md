# HelpDesk Lite

Internal support ticketing system — **Epic 1: Authentication & User Access**

**Project location:** `/mnt/my_ntfs_drive/Career/iCareer/Technical Phase/HelpDesk Lite/`

## Stack

| Layer | Technology |
|-------|------------|
| Backend | ASP.NET Core Web API (.NET 10) |
| ORM | Entity Framework Core + PostgreSQL |
| Auth | JWT + RBAC (Employee, SupportAgent, ManagerAdmin) |
| Frontend | React + TypeScript + Vite + TailwindCSS |
| State | React Query + Context API |
| Validation | FluentValidation |
| Passwords | ASP.NET Identity `PasswordHasher` |

## Quick start

### 1. PostgreSQL

```bash
cd "/mnt/my_ntfs_drive/Career/iCareer/Technical Phase/HelpDesk Lite"
docker compose up -d
```

Or use your own PostgreSQL instance matching `.env.example`.

### 2. Environment

```bash
cp .env.example .env
# Edit Jwt__SigningKey (min 32 characters) and connection string if needed
```

### 3. Database migration

```bash
cd backend
export $(grep -v '^#' ../.env | xargs)   # or set vars manually on Windows

dotnet ef database update \
  -p src/HelpDeskLite.Infrastructure \
  -s src/HelpDeskLite.Api
```

**Create new migrations:**

```bash
dotnet ef migrations add <Name> \
  -p src/HelpDeskLite.Infrastructure \
  -s src/HelpDeskLite.Api
```

### 4. Run API

```bash
cd backend/src/HelpDeskLite.Api
dotnet run
```

- Swagger: https://localhost:5001/swagger
- Health: https://localhost:5001/api/health

### 5. Run frontend

```bash
cd frontend
cp .env.example .env
npm install
npm run dev
```

Open http://localhost:5173

## Seed users (Development)

| Email | Password | Role |
|-------|----------|------|
| employee@helpdesk.local | Employee123! | Employee |
| agent@helpdesk.local | Agent123! | SupportAgent |
| admin@helpdesk.local | Admin123! | ManagerAdmin |

## API examples

### Login

```bash
curl -k -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"employee@helpdesk.local","password":"Employee123!"}'
```

**Response:**

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

### Get tickets (with token)

```bash
TOKEN="<accessToken>"
curl -k https://localhost:5001/api/tickets \
  -H "Authorization: Bearer $TOKEN"
```

### Current user

```bash
curl -k https://localhost:5001/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

### Logout

```bash
curl -k -X POST https://localhost:5001/api/auth/logout \
  -H "Authorization: Bearer $TOKEN"
```

## Authorization test matrix

| User | Login | GET /api/tickets | GET /api/tickets/{otherUserTicket} |
|------|-------|------------------|-------------------------------------|
| Employee | OK | Own only | 403 |
| SupportAgent | OK | All | OK |
| ManagerAdmin | OK | All | OK |

## Solution structure

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

## Security notes

- Passwords are hashed with `PasswordHasher<User>` — never stored in plaintext
- JWT access tokens (default 15 minutes)
- Refresh token **entity and storage** are prepared; `/api/auth/refresh` returns 501 until next epic
- SSO: `IExternalAuthProvider` stub + `ExternalSubjectId` on `User`
- Use HTTPS in production; trust dev cert: `dotnet dev-certs https --trust`

## HTTPS dev certificate

```bash
dotnet dev-certs https --trust
```
