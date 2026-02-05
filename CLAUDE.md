# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Essential Context

This is a **BLE Tracking Backend** system - a .NET 8.0 microservices architecture for real-time tracking using Bluetooth Low Energy beacons. The system has 30+ services with shared business logic, repositories, and controllers.

**Key documentation** (read these first):

- `PROJECT_GUIDE.md` - Comprehensive architecture and coding standards (in Indonesian)
- `REFACTORING_GUIDE.md` - Current refactoring patterns and repository standards
- `docker-compose.yml` - Service port mappings and dependencies

**Important**: This codebase uses **manual projection** for read operations, NOT AutoMapper projections from repositories. See REFACTORING_GUIDE.md section 8 for the ProjectToRead pattern.

---

## Development Commands

### Building & Running

```bash
# Build entire solution
dotnet build BleTrackingBackend.sln

# Build specific service
dotnet build Services.API/Auth/Auth.csproj

# Run all services via Docker
docker-compose up -d

# Run specific services
docker-compose up -d auth mst-building

# View logs
docker-compose logs -f auth

# Stop all services
docker-compose down
```

### Database Operations

```bash
# Add migration for specific service
dotnet ef migrations add [MigrationName] --project Shared/Repositories/Repositories.csproj --startup-project Services.API/[Service]/[Service].csproj

# Update database
dotnet ef database update --project Shared/Repositories/Repositories.csproj --startup-project Services.API/[Service]/[Service].csproj
```

---

## Architecture Overview

### Service Structure

The codebase follows a **vertical slice architecture** with shared layers:

```
Services.API/[ServiceName]/     # 30+ standalone API services (Auth, MstBuilding, Patrol, etc.)
├── Program.cs                    # Service entry point with DI configuration
├── [Service].csproj              # Project file
├── Dockerfile                    # Container definition
└── appsettings.json              # Configuration

Shared/                           # Shared business logic and data layers
├── BusinessLogic.Services/       # Service layer with interfaces and implementations
│   ├── Interface/                # Service interfaces (IAuthService, IMstBuildingService, etc.)
│   ├── Implementation/           # Service implementations (AuthService, MstBuildingService, etc.)
│   ├── Extension/                # Extension methods for DI, auth, swagger, etc.
│   └── Background/               # Background services (MQTT, jobs)
├── Repositories/                 # Data access layer
│   ├── Repository/               # Repository implementations
│   └── DbContexts/               # EF Core DbContext
├── Entities.Models/              # Domain entities
├── Web.API.Controllers/          # Shared controllers
├── Shared.Contracts/             # DTOs and filters
│   ├── Read/                     # Read DTOs for projections (EntityRead.cs)
│   └── enum.cs                   # Shared enums
├── Data.ViewModels/              # Create/Update DTOs and validators
└── Helpers.Consumer/             # Utility classes (BaseModel, MQTT, email, etc.)
```

### Key Architectural Patterns

**1. Manual Projection (ProjectToRead)**

- Repositories return Read DTOs via manual projection, NOT entities
- Example: `MstBuildingRepository.FilterAsync()` returns `(List<MstBuildingRead> Data, int Total, int Filtered)`
- Do NOT use AutoMapper in repositories - use explicit `Select()` projections
- See `Shared/Repositories/Repository/MstBuildingRepository.cs` for reference

**2. Filter DTOs with JsonElement for ID fields**

- For ID filters that support both single Guid and Guid array, use `JsonElement` type
- Use `ExtractIds(filter.IdField)` helper from `BaseRepository` to handle both cases
- Example: `public JsonElement CategoryId { get; set; }` in Filter DTO

**3. Service Direct Return Pattern**

- If repository returns Read DTO, service should return it directly WITHOUT mapper
- Only use mapper for Create/Update operations that return full DTOs
- Example from UserService: `return user; // Direct return from repository`

**4. BaseEntityQuery Pattern**

- All repositories should have `BaseEntityQuery()` method for multi-tenancy
- Applies ApplicationId filtering and status != 0 checks
- Used as starting point for all queries

**5. Audit Trail with BaseService**

- Services inherit from `BaseService` to get audit helpers
- `SetCreateAudit(entity)`, `SetUpdateAudit(entity)`, `SetDeleteAudit(entity)`
- Inject `IAuditEmitter` and call `Created()`, `Updated()`, `Deleted()` after DB operations
- Use `UsernameFormToken` property from BaseService for current username

**6. Multi-Tenancy with IApplicationEntity**

- Entities implement `IApplicationEntity` interface for tenant filtering
- `BaseRepository.ApplyApplicationIdFilter()` handles filtering
- System admins bypass ApplicationId checks

**7. Authorization with MinLevel**

- Controllers use `[MinLevel(LevelPriority.SuperAdmin)]` attribute
- Custom `MinLevelHandler` enforces role hierarchy
- JWT token contains `level` claim with integer value

---

## When Creating/Refactoring Services

Follow the checklist in `REFACTORING_GUIDE.md`. Key points:

1. **Create Filter DTO** in `Shared/Shared.Contracts/[Entity]Filter.cs`
   - Use `JsonElement` for ID fields that need to support both single/array
   - Inherit from `BaseFilter` if it exists

2. **Create Read DTO** in `Shared/Shared.Contracts/Read/[Entity]Read.cs`
   - Inherit from `BaseRead` for common audit fields
   - Include navigation properties (e.g., `GroupName` when including Group)

3. **Update Repository** in `Shared/Repositories/Repository/[Entity]Repository.cs`
   - Add `BaseEntityQuery()` with ApplicationId filtering
   - Add `ProjectToRead()` with manual projection using `Select()`
   - Add `FilterAsync()` returning `(List<[Entity]Read> Data, int Total, int Filtered)`
   - Use `ExtractIds(filter.IdField)` for ID filters
   - Keep `GetByIdEntityAsync()` for operations that need entity object

4. **Create/Update Service** in `Shared/BusinessLogic.Services/Implementation/[Entity]Service.cs`
   - Inherit from `BaseService`
   - Inject `IAuditEmitter` for audit logging
   - Use direct return for `GetByIdAsync`/`GetAllAsync` (no mapper)
   - Use `GetByIdEntityAsync` for update/delete operations
   - Call `SetCreateAudit`, `SetUpdateAudit`, `SetDeleteAudit` from BaseService
   - Call `audit.Created()`, `audit.Updated()`, `audit.Deleted()` after DB operations

5. **Update Controller** in `Shared/Web.API.Controllers/Controllers/[Entity]Controller.cs`
   - Use `[MinLevel(LevelPriority.[Role])]` for authorization
   - Use `ApiResponse.Success()`, `ApiResponse.Created()`, `ApiResponse.BadRequest()` for responses
   - Remove manual try-catch blocks (handled by middleware)
   - Add `POST /api/[entity]/filter` endpoint with `DataTablesProjectedRequest`

6. **Update Program.cs** in `Services.API/[Service]/Program.cs`
   - Use `EnvTryCatchExtension.LoadEnvWithTryCatch()` for env loading
   - Add `builder.UseSerilogExtension()` for Serilog
   - Add JSON options with enum converter and reference handler
   - Add `app.UseSerilogRequestLoggingExtension()` middleware
   - Register service and repository in DI
   - See `Services.API/Patrol/Program.cs` for reference
7. When refactoring services, always preserve and migrate existing audit emitters to the new structure before completing the refactoring.
   if not found create manual audit base on structure

---

## Critical Base Classes & Extensions

**BaseService** (`Shared/BusinessLogic.Services/Implementation/BaseService.cs`)

- `AppId` - Gets current ApplicationId from JWT token
- `UsernameFormToken` - Gets current username from JWT token
- `SetCreateAudit(entity)` - Sets CreatedBy, CreatedAt
- `SetUpdateAudit(entity)` - Sets UpdatedBy, UpdatedAt
- `SetDeleteAudit(entity)` - Sets UpdatedBy, UpdatedAt, Status = 0

**BaseRepository** (`Shared/Repositories/Repository/BaseRepository.cs`)

- `GetApplicationIdAndRole()` - Returns (ApplicationId?, isSystemAdmin)
- `ApplyApplicationIdFilter()` - Filters by ApplicationId (bypass for system admin)
- `ValidateApplicationIdAsync()` - Validates ApplicationId exists
- `ValidateApplicationIdForEntity()` - Validates entity belongs to user's ApplicationId
- `ExtractIds(JsonElement)` - Extracts single Guid or Guid array from JsonElement
- `CheckInvalidOwnershipAsync()` - Validates ownership for batch operations

**Extensions** (`Shared/BusinessLogic.Services/Extension/RootExtension/`)

- `EnvTryCatchExtension.LoadEnvWithTryCatch()` - Loads .env file with error handling
- `SerilogHostExtensions.UseSerilogExtension()` - Configures Serilog logging
- `SerilogAppExtensions.UseSerilogRequestLoggingExtension()` - Request logging middleware
- `AuthExtensions.AddJwtAuthExtension()` - JWT authentication setup
- `AuthorizationExtensions.AddAuthorizationNewPolicies()` - Authorization policies
- `DbContextExtensions.AddDbContextExtension()` - Database configuration
- `SwaggerExtensions.AddSwaggerExtension()` - Swagger setup

---

## Authentication & Authorization

**JWT Token Structure**

- Claims: `sub` (user id), `email`, `username`, `groupId`, `ApplicationId`, `groupName`, `role`, `level`
- `level` claim contains integer value of `LevelPriority` enum
- System = 0, SuperAdmin = 1, PrimaryAdmin = 2, Primary = 3, Secondary = 4, UserCreated = 5

**API Key Authentication**

- Integration login uses `MstIntegration.ApiKey` for authentication
- Only SuperAdmin and PrimaryAdmin can login via integration
- See `IAuthService.IntegrationLoginAsync()` in `Shared/BusinessLogic.Services/Interface/IAuthService.cs`

**MinLevel Authorization**

- Use `[MinLevel(LevelPriority.SuperAdmin)]` attribute on controller/action
- Enforces minimum role level (higher number = lower priority)
- Example: `[MinLevel(LevelPriority.PrimaryAdmin)]` allows PrimaryAdmin (2), SuperAdmin (1), System (0)

---

## Important File Locations

**Configuration**

- `.env` - Environment variables (not committed)
- `appsettings.json` - Application settings
- `docker-compose.yml` - Service orchestration
- `nginx.conf` - API Gateway configuration

**Key Shared Files**

- `Shared/Helpers/Consumer/BaseModelWithTimeApp.cs` - Base entity with audit fields (Status is int, not enum)
- `Shared/Helpers/Consumer/IApplicationEntity.cs` - Multi-tenancy interface
- `Shared/BusinessLogic.Services/Extension/AuditEmitter/IAuditEmitter.cs` - Audit logging interface
- `Shared/DataViewModels/ResponseHelper/ApiResponse.cs` - Standard API response helper

**Service-Specific**

- Each service has its own Program.cs, Dockerfile, and appsettings.json
- Service naming: `Services.API/[ServiceName]/[ServiceName].csproj`
- Port mapping defined in `docker-compose.yml` and `.env` file

---

## Common Patterns & Anti-Patterns

**DO:**

- Use manual projection in repositories with `Select()` to Read DTOs
- Use `JsonElement` for ID filters that support both single and array
- Use `ExtractIds()` helper from BaseRepository for ID filtering
- Use direct return pattern when repository returns Read DTO
- Use `GetByIdEntityAsync()` when you need the entity object (update/delete)
- Call `SetCreateAudit`, `SetUpdateAudit`, `SetDeleteAudit` from BaseService
- Inject `IAuditEmitter` and call audit methods after DB operations
- Use `UsernameFormToken` from BaseService for current username
- Use `MinLevel` attribute for authorization instead of `[Authorize]`
- Use `ApiResponse` helper for all controller responses

**DON'T:**

- Don't use AutoMapper in repository projections
- Don't return entities from services (use Read DTOs or full DTOs)
- Don't use mapper when repository already returns Read DTO (direct return)
- Don't inject IAuditEmitter without calling Created/Updated/Deleted
- Don't manually set CreatedBy/UpdatedAt (use BaseService helpers)
- Don't use manual try-catch in controllers (handled by middleware)
- Don't use `[Authorize]` attribute (use MinLevel instead)
- Don't forget to register services and repositories in Program.cs

---

## Testing

**Current State**: No test projects exist in the codebase.

**Recommendation**: Add test projects following .NET conventions:

- `Tests/Unit/[Service].Tests.csproj` for unit tests
- `Tests/Integration/[Service].Tests.csproj` for integration tests
- Use xUnit, NUnit, or MSTest
- Use Moq for mocking
- Test coverage should aim for 80%+ for business logic

---

## Environment Variables (.env file)

Key variables (see `.env` file for complete list):

```bash
# Database
DB_HOST=localhost
DB_PORT=1433
DB_NAME=BleTrackingDb
DB_USER=sa
DB_PASSWORD=P@ssw0rd

# JWT
JWT_KEY=your-secret-key
JWT_ISSUER=TrackingBleAuth
JWT_AUDIENCE=TrackingBleAPI

# MQTT
MQTT_HOST=localhost
MQTT_PORT=1883
MQTT_USERNAME=bio_mqtt
MQTT_PASSWORD=P@ssw0rd

# Service Ports (example)
AUTH_PORT=5001
MST_BUILDING_PORT=5010
PATROL_PORT=5020
```

---

## Status Field Changes (Important)

**Recent Changes**:

- User entity now uses `int Status` from `BaseModelWithTimeApp` instead of `StatusActive` enum
- Status = 1 means Active, Status = 0 means Inactive
- UserRead DTO uses `StatusEmployee` enum (not int)
- When filtering by Status in UserRepository: `Where(u => u.Status != 0)` filters out inactive users
- When setting status: `user.Status = 1` for active, `user.Status = 0` for inactive

**Note**: Different entities may use different status representations:

- User uses `int Status` (from BaseModelWithTimeApp)
- UserRead uses `StatusEmployee` enum
- Always check the entity definition before refactoring

---

## Troubleshooting

**Service won't start:**

- Check `.env` file exists and has correct values
- Verify port is not already in use
- Check `docker-compose logs -f [service]`

**Database connection issues:**

- Verify SQL Server is running
- Check connection string in `.env`
- Ensure database exists: `BleTrackingDb`

**Migration issues:**

- Use specific startup project: `--startup-project Services.API/[Service]/[Service].csproj`
- Ensure DbContext is in `Shared/Repositories/Repositories.csproj`

**Authentication failures:**

- Check JWT_KEY in `.env` matches between services
- Verify token hasn't expired (default 15 minutes)
- Check user Status = 1 and IsEmailConfirmation = 1

**MQTT connection issues:**

- Verify MQTT broker is running
- Check MQTT_HOST and MQTT_PORT in `.env`
- See `docker-compose logs -f mqttbroker`
