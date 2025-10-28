# Task Management Application

Complete task management application using **Angular 18**, **.NET 8**, **IdentityServer (Duende)** and **SQL Server LocalDB**.

## Table of Contents

- [Architecture](#architecture)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [Database](#database)
- [Getting Started](#getting-started)
- [Usage](#usage)
- [Testing](#testing)
- [Environments](#environments)
- [Troubleshooting](#troubleshooting)
- [Technologies](#technologies)

---

## Architecture
```
┌─────────────────────────────────────────────────────────────┐
│                    ANGULAR CLIENT (4200)                    │
│              • PrimeNG UI Components                        │
│              • OIDC Authentication                          │
│              • Reactive Forms                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      │ Authorization Code Flow + PKCE
                      ↓
┌─────────────────────────────────────────────────────────────┐
│                IDENTITY SERVER (5001)                       │
│              • Duende IdentityServer 6                      │
│              • JWT Token Generation                         │
│              • OpenID Connect                               │
└─────────────────────┬───────────────────────────────────────┘
                      │
                      │ Client Credentials (M2M)
                      │ User Validation
                      ↓
┌─────────────────────────────────────────────────────────────┐
│                TASK MANAGEMENT API (5002)                   │
│              • .NET 8 Web API                               │
│              • Entity Framework Core                        │
│              • JWT Authentication                           │
│              • SQL Server LocalDB                           │
└─────────────────────────────────────────────────────────────┘
```

---

## Prerequisites

### Required

- **Node.js** >= 18.x ([Download](https://nodejs.org/))
- **.NET SDK** >= 8.0 ([Download](https://dotnet.microsoft.com/download))
- **SQL Server LocalDB** (included with Visual Studio or [standalone download](https://go.microsoft.com/fwlink/?linkid=866658))
- **Angular CLI** >= 18.x
- **Git**

### Optional

- **Visual Studio 2022** or **VS Code**
- **Azure Data Studio** or **SSMS** (for database management)
- **Postman** (for API testing)

### Prerequisites Verification
```bash
# Node.js
node --version
# Expected result: v18.x.x or higher

# NPM
npm --version
# Expected result: 9.x.x or higher

# .NET SDK
dotnet --version
# Expected result: 8.0.x

# SQL Server LocalDB
sqllocaldb info
# Expected result: MSSQLLocalDB

# Angular CLI
ng version
# Expected result: 18.x.x
```


## Installation

### 1. Clone the Repository
```bash
git clone https://github.com/your-username/ng-dotnet-task-management.git
cd ng-dotnet-task-management
```

### 2. Backend Installation (.NET)

#### a) IdentityServer
```bash
cd server/IdentityServer

# Restore NuGet packages
dotnet restore

# Build
dotnet build
```

**Installed Packages:**
- `Duende.IdentityServer` (6.3.7)
- `Serilog.AspNetCore` (8.0.0)
- `IdentityModel` (6.2.0)

#### b) Task Management API
```bash
cd ../TaskManagement.API

# Restore NuGet packages
dotnet restore

# Build
dotnet build
```

**Installed Packages:**
- `Microsoft.EntityFrameworkCore.SqlServer` (8.0.0)
- `Microsoft.EntityFrameworkCore.Design` (8.0.0)
- `Microsoft.EntityFrameworkCore.Tools` (8.0.0)
- `BCrypt.Net-Next` (4.0.3)

### 3. Frontend Installation (Angular)
```bash
cd ../../client

# Install dependencies
npm install
```

**Main Packages:**
- `@angular/core` (18.x)
- `angular-oauth2-oidc` (17.x)
- `primeng` (17.x)
- `primeicons` (7.x)

---

## Configuration

### 1. SQL Server LocalDB Configuration

#### Start LocalDB
```powershell
# Check instances
sqllocaldb info

# If MSSQLLocalDB doesn't exist, create instance
sqllocaldb create MSSQLLocalDB

# Start instance
sqllocaldb start MSSQLLocalDB

# Verify instance is running
sqllocaldb info MSSQLLocalDB
```

**Expected Result:**
```
Name:               MSSQLLocalDB
Version:            17.0.925.4
State:              Running
```

### 2. IdentityServer Configuration

**File:** `server/IdentityServer/appsettings.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Duende.IdentityServer": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**No modification needed** - In-memory configuration via `Config.cs`.

### 3. Task Management API Configuration

**File:** `server/TaskManagement.API/appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  },
  "AllowedHosts": "*"
}
```

**File:** `server/TaskManagement.API/appsettings.Development.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TaskManagementDB_Dev;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

### 4. Angular Client Configuration

**File:** `client/src/environments/environment.ts`
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:5002/api',
  oidc: {
    issuer: 'https://localhost:5001',
    redirectUri: 'http://localhost:4200/auth/callback',
    postLogoutRedirectUri: 'http://localhost:4200',
    clientId: 'angular-client',
    scope: 'openid profile email taskapi offline_access',
    responseType: 'code',
    showDebugInformation: true,
    requireHttps: false
  }
};
```

**File:** `client/src/environments/environment.prod.ts`
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://your-production-api.com/api',
  oidc: {
    issuer: 'https://your-production-identityserver.com',
    redirectUri: 'https://your-production-app.com/auth/callback',
    postLogoutRedirectUri: 'https://your-production-app.com',
    clientId: 'angular-client',
    scope: 'openid profile email taskapi offline_access',
    responseType: 'code',
    showDebugInformation: false,
    requireHttps: true
  }
};
```

---

## Database

### 1. Database Creation

#### Option A: Automated PowerShell Script (Recommended)

Create `server/setup-database.ps1`:
```powershell
Write-Host "Setting up database..." -ForegroundColor Green

# 1. Check LocalDB
Write-Host "`nChecking LocalDB..." -ForegroundColor Yellow
$localDbRunning = sqllocaldb info MSSQLLocalDB | Select-String "Running"

if (!$localDbRunning) {
    Write-Host "LocalDB not started. Starting..." -ForegroundColor Yellow
    sqllocaldb start MSSQLLocalDB
}
Write-Host "LocalDB active" -ForegroundColor Green

# 2. Navigate to project
Set-Location -Path "TaskManagement.API"

# 3. Build
Write-Host "`nBuilding project..." -ForegroundColor Yellow
dotnet build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build error" -ForegroundColor Red
    exit 1
}
Write-Host "Build successful" -ForegroundColor Green

# 4. Remove old database (optional)
Write-Host "`nCleaning up..." -ForegroundColor Yellow
dotnet ef database drop --force 2>$null
Remove-Item -Path "Migrations" -Recurse -Force -ErrorAction SilentlyContinue

# 5. Create migration
Write-Host "`nCreating migration..." -ForegroundColor Yellow
dotnet ef migrations add InitialCreate
if ($LASTEXITCODE -ne 0) {
    Write-Host "Migration error" -ForegroundColor Red
    exit 1
}
Write-Host "Migration created" -ForegroundColor Green

# 6. Apply migration
Write-Host "`nApplying migration..." -ForegroundColor Yellow
dotnet ef database update
if ($LASTEXITCODE -ne 0) {
    Write-Host "Update error" -ForegroundColor Red
    exit 1
}
Write-Host "Database created and seeded!" -ForegroundColor Green

# 7. Display info
Write-Host "`nConnection information:" -ForegroundColor Cyan
Write-Host "Server: (localdb)\mssqllocaldb" -ForegroundColor White
Write-Host "Database: TaskManagementDB_Dev" -ForegroundColor White
Write-Host "`nTest accounts:" -ForegroundColor Cyan
Write-Host "john@example.com / password" -ForegroundColor White
Write-Host "alice@example.com / password" -ForegroundColor White

Write-Host "`nSetup completed!" -ForegroundColor Green
```

**Execution:**
```powershell
cd server
.\setup-database.ps1
```

#### Option B: Manual Commands
```bash
cd server/TaskManagement.API

# Create migration
dotnet ef migrations add InitialCreate

# Apply migration (create database)
dotnet ef database update
```

### 2. Verify Database

#### With Azure Data Studio

1. **Download:** https://aka.ms/azuredatastudio
2. **Connect:**
   - Server: `(localdb)\mssqllocaldb`
   - Authentication: Windows Authentication
   - Database: `TaskManagementDB_Dev`

3. **Execute:**
```sql
   -- View tables
   SELECT * FROM INFORMATION_SCHEMA.TABLES;
   
   -- View users
   SELECT * FROM Users;
   
   -- View tasks
   SELECT * FROM Tasks;
```

#### With SSMS (SQL Server Management Studio)

1. Server name: `(localdb)\mssqllocaldb`
2. Authentication: Windows Authentication
3. Connect

### 3. Seed Data (Test Data)

The database is automatically seeded on API startup via `DbInitializer.cs`.

**Created Users:**
- `john@example.com` / `password`
- `alice@example.com` / `password`

**Created Tasks:**
- 3 tasks for John
- 2 tasks for Alice

---

## Getting Started

### Option A: Manual Start (Development)

#### Terminal 1: IdentityServer
```bash
cd server/IdentityServer
dotnet run --urls "https://localhost:5001"
```

**Expected Result:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5001
```

#### Terminal 2: Task Management API
```bash
cd server/TaskManagement.API
dotnet run --urls "https://localhost:5002"
```

**Expected Result:**
```
Database initialized successfully
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:5002
```

#### Terminal 3: Angular Client
```bash
cd client
ng serve
```

**Expected Result:**
```
Browser application bundle generation complete.
Angular Live Development Server is listening on localhost:4200
Compiled successfully.
```

### Option B: PowerShell Script (Recommended)

Create `start-all.ps1` at root:
```powershell
Write-Host "Starting Task Management Application" -ForegroundColor Green

# Function to start a process
function Start-Service {
    param(
        [string]$Name,
        [string]$Path,
        [string]$Command
    )
    
    Write-Host "Starting $Name..." -ForegroundColor Yellow
    
    $process = Start-Process -FilePath "powershell" `
        -ArgumentList "-NoExit", "-Command", "cd '$Path'; $Command" `
        -PassThru
    
    Write-Host "$Name started (PID: $($process.Id))" -ForegroundColor Green
    
    return $process
}

# Start services
$identityServer = Start-Service `
    -Name "IdentityServer" `
    -Path "$PSScriptRoot\server\IdentityServer" `
    -Command "dotnet run --urls 'https://localhost:5001'"

Start-Sleep -Seconds 3

$taskApi = Start-Service `
    -Name "Task Management API" `
    -Path "$PSScriptRoot\server\TaskManagement.API" `
    -Command "dotnet run --urls 'https://localhost:5002'"

Start-Sleep -Seconds 3

$angular = Start-Service `
    -Name "Angular Client" `
    -Path "$PSScriptRoot\client" `
    -Command "ng serve"

Write-Host "`nAll services started!" -ForegroundColor Green
Write-Host "`nURLs:" -ForegroundColor Cyan
Write-Host "  IdentityServer:  https://localhost:5001" -ForegroundColor White
Write-Host "  Task API:        https://localhost:5002/swagger" -ForegroundColor White
Write-Host "  Angular Client:  http://localhost:4200" -ForegroundColor White

Write-Host "`nTest accounts:" -ForegroundColor Cyan
Write-Host "  john@example.com / password" -ForegroundColor White
Write-Host "  alice@example.com / password" -ForegroundColor White

Write-Host "`nPress any key to stop all services..." -ForegroundColor Yellow
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

# Stop services
Write-Host "`nStopping services..." -ForegroundColor Red
Stop-Process -Id $identityServer.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $taskApi.Id -Force -ErrorAction SilentlyContinue
Stop-Process -Id $angular.Id -Force -ErrorAction SilentlyContinue

Write-Host "All services stopped" -ForegroundColor Green
```

**Execution:**
```powershell
.\start-all.ps1
```

---

## Usage

### 1. Access the Application

Open in browser: **http://localhost:4200**

### 2. Sign In

1. Click on **"Sign in with Identity Server"**
2. You will be redirected to IdentityServer (https://localhost:5001)
3. Enter credentials:
   - Email: `john@example.com`
   - Password: `password`
4. Automatic redirection to application with authentication

### 3. Manage Tasks

#### Create a Task
- Click on **"New Task"**
- Fill in the form
- Click on **"Save"**

#### Edit a Task
- Click on **"Edit"** icon
- Modify fields
- Click on **"Save"**

#### Mark as Completed
- Click on the checkbox

#### Delete a Task
- Click on **"Delete"** icon
- Confirm deletion

#### Filter Tasks
- **All**: Display all tasks
- **Pending**: Display incomplete tasks
- **Completed**: Display completed tasks
- **Overdue**: Display overdue tasks

#### Search
- Use search bar to filter by title or description

### 4. Sign Out

Click on user icon then **"Logout"**

---

## Testing

### 1. Test with Swagger (Task API)

**URL:** https://localhost:5002/swagger

#### Authentication

1. Obtain a token:
```bash
   curl -X POST https://localhost:5001/connect/token \
     -H "Content-Type: application/x-www-form-urlencoded" \
     -d "grant_type=password" \
     -d "client_id=angular-client" \
     -d "username=john@example.com" \
     -d "password=password" \
     -d "scope=openid profile email taskapi" \
     --insecure
```

2. In Swagger:
   - Click **"Authorize"**
   - Enter: `Bearer {your_access_token}`
   - Click **"Authorize"**

#### Test Endpoints

- `GET /api/tasks` - List tasks
- `POST /api/tasks` - Create a task
- `GET /api/tasks/{id}` - Get a task
- `PUT /api/tasks/{id}` - Update a task
- `DELETE /api/tasks/{id}` - Delete a task
- `GET /api/tasks/statistics` - Statistics

### 2. Test with PowerShell

Create `test-api.ps1`:
```powershell
Write-Host "Testing Task Management API" -ForegroundColor Green

# 1. Login
$loginResponse = Invoke-RestMethod `
    -Uri "https://localhost:5001/connect/token" `
    -Method Post `
    -ContentType "application/x-www-form-urlencoded" `
    -Body "grant_type=password&client_id=angular-client&username=john@example.com&password=password&scope=openid profile email taskapi" `
    -SkipCertificateCheck

$token = $loginResponse.access_token
Write-Host "Token obtained" -ForegroundColor Green

$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

# 2. Get tasks
$tasks = Invoke-RestMethod `
    -Uri "https://localhost:5002/api/tasks" `
    -Method Get `
    -Headers $headers `
    -SkipCertificateCheck

Write-Host "Found $($tasks.Count) tasks" -ForegroundColor Green

# 3. Create a task
$newTask = @{
    title = "Test Task"
    description = "Test from PowerShell"
    dueDate = (Get-Date).AddDays(7).ToString("yyyy-MM-ddTHH:mm:ss")
} | ConvertTo-Json

$created = Invoke-RestMethod `
    -Uri "https://localhost:5002/api/tasks" `
    -Method Post `
    -Headers $headers `
    -Body $newTask `
    -SkipCertificateCheck

Write-Host "Task created: $($created.id)" -ForegroundColor Green

# 4. Delete task
Invoke-RestMethod `
    -Uri "https://localhost:5002/api/tasks/$($created.id)" `
    -Method Delete `
    -Headers $headers `
    -SkipCertificateCheck

Write-Host "Task deleted" -ForegroundColor Green
Write-Host "`nTests completed successfully!" -ForegroundColor Green
```

**Execution:**
```powershell
.\test-api.ps1
```

### 3. Unit Tests (Optional)
```bash
# Backend tests
cd server/TaskManagement.API.Tests
dotnet test

# Frontend tests
cd client
ng test
```

---

## Environments

### Development

**Backend:**
- IdentityServer: `https://localhost:5001`
- Task API: `https://localhost:5002`
- Database: `TaskManagementDB_Dev`

**Frontend:**
- Angular: `http://localhost:4200`

**Configuration:**
```bash
# Use development environment
export ASPNETCORE_ENVIRONMENT=Development  # Linux/Mac
$env:ASPNETCORE_ENVIRONMENT="Development"  # PowerShell
```

### Production

**Backend:**
- Modify `appsettings.Production.json`
- Configure production URLs
- Use real SQL Server database
- Configure SSL/TLS
- Configure secrets (Azure Key Vault, etc.)

**Frontend:**
- Modify `environment.prod.ts`
- Production build:
```bash
  ng build --configuration production
```

**Deployment:**
```bash
# Backend
dotnet publish -c Release -o ./publish

# Frontend
ng build --configuration production
# Files in: client/dist/
```

---

## Troubleshooting

### Issue: LocalDB won't start

**Symptom:**
```
A network-related or instance-specific error occurred
```

**Solution:**
```powershell
# Stop all instances
sqllocaldb stop MSSQLLocalDB

# Delete instance
sqllocaldb delete MSSQLLocalDB

# Recreate and start
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### Issue: SSL certificate error

**Symptom:**
```
SSL connection could not be established
```

**Solution:**
```bash
# Trust .NET development certificates
dotnet dev-certs https --trust
```

### Issue: Port already in use

**Symptom:**
```
Address already in use
```

**Solution:**
```powershell
# Find process
netstat -ano | findstr :5001

# Kill process
taskkill /PID <PID> /F
```

### Issue: Migration fails

**Symptom:**
```
Unable to create an object of type 'ApplicationDbContext'
```

**Solution:**
```bash
# Remove Migrations folder
rm -rf Migrations/

# Recreate migration
dotnet ef migrations add InitialCreate

# Apply
dotnet ef database update
```

### Issue: Angular - Module not found

**Symptom:**
```
Module 'primeng/button' not found
```

**Solution:**
```bash
# Remove node_modules and package-lock.json
rm -rf node_modules package-lock.json

# Reinstall
npm install
```

### Issue: CORS Error

**Symptom:**
```
Access to XMLHttpRequest blocked by CORS policy
```

**Solution:**

Check `Program.cs` (Task API):
```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("http://localhost:4200", "https://localhost:5001")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

### Issue: 401 Unauthorized on API

**Symptom:**
```
401 Unauthorized
```

**Solutions:**

1. **Check token:**
   - Token expired - Re-authenticate
   - Invalid token - Check OIDC configuration

2. **Check claims:**
```
   GET /api/users/debug/claims
```

3. **Check configuration:**
   - Authority in Task API = Issuer in IdentityServer
   - Correct audience

### Logs and Debugging

**Backend:**
```bash
# Increase log level
# In appsettings.Development.json
"Logging": {
  "LogLevel": {
    "Default": "Debug",
    "Microsoft.EntityFrameworkCore": "Information"
  }
}
```

**Frontend:**
```typescript
// In environment.ts
showDebugInformation: true  // Enable OIDC logs
```

---

## Technologies

### Backend

- **.NET 8.0** - Main framework
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 8.0** - ORM
- **SQL Server LocalDB** - Database
- **Duende IdentityServer 6** - OIDC authentication server
- **BCrypt.Net** - Password hashing
- **Serilog** - Logging
- **Swagger/OpenAPI** - API documentation

### Frontend

- **Angular 18** - SPA framework
- **TypeScript 5.x** - Language
- **PrimeNG 17** - UI components
- **PrimeIcons** - Icons
- **angular-oauth2-oidc** - OIDC client
- **RxJS** - Reactive programming

### Security

- **OpenID Connect (OIDC)** - Authentication protocol
- **OAuth 2.0** - Authorization protocol
- **JWT (JSON Web Tokens)** - Authentication tokens
- **PKCE** - Proof Key for Code Exchange
- **BCrypt** - Password hashing

---

## Useful Commands

### Backend
```bash
# Build
dotnet build

# Run
dotnet run

# Watch (auto-reload)
dotnet watch run

# Tests
dotnet test

# Migrations
dotnet ef migrations add <Name>
dotnet ef database update
dotnet ef database drop

# Publish
dotnet publish -c Release
```

### Frontend
```bash
# Serve
ng serve

# Build
ng build

# Production build
ng build --configuration production

# Tests
ng test

# Linter
ng lint

# Create component
ng generate component features/tasks/task-list
```

### Database
```bash
# LocalDB
sqllocaldb info
sqllocaldb start MSSQLLocalDB
sqllocaldb stop MSSQLLocalDB

# EF Core Migrations
dotnet ef migrations add InitialCreate
dotnet ef migrations list
dotnet ef migrations remove
dotnet ef database update
dotnet ef database drop --force
```

---

## Authors

- **Haroun NAKBI**

---

## Acknowledgments

- PrimeNG for UI components
- Duende Software for IdentityServer
- Microsoft for .NET and Entity Framework
- Angular Team


---

**Last updated:** October 2025