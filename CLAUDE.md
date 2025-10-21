# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This repository contains **two separate ASP.NET Core MVC applications**:
- **GameSpace**: Main gaming forum platform (Admin backend focus)
- **GamiPort**: Public-facing portal

Both projects share a common architecture using ASP.NET Core 8.0 with Areas-based modular design.

## Common Development Commands

### Build and Run
```powershell
# Build GameSpace
cd GameSpace
dotnet build GameSpace.sln

# Run GameSpace
cd GameSpace
dotnet run --project GameSpace.csproj

# Build GamiPort
cd GamiPort
dotnet build GamiPort.sln

# Run GamiPort
cd GamiPort
dotnet run --project GamiPort.csproj
```

### Database Operations
**CRITICAL**: Do NOT use EF Migrations to modify schema. The database structure is managed manually via SQL Server.

```powershell
# Test database connection (from schema/ directory)
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E -Q "SELECT DB_NAME() AS CurrentDatabase"

# Export all database tables
sqlcmd -S tcp:DESKTOP-8HQIS1S\SQLEXPRESS,1433 -d GameSpacedatabase -E -Q "SET NOCOUNT ON;EXEC sp_MSforeachtable 'SELECT ''?'' AS TableName, * FROM ?'" -o "GameSpacedatabase_all_tables.txt"
```

## Architecture

### Dual DbContext Pattern
The projects use two separate DbContexts with clearly separated responsibilities:

1. **ApplicationDbContext** (`DefaultConnection`)
   - ASP.NET Identity user authentication only
   - Database: `aspnet-GameSpace-38e0b594-8684-40b2-b330-7fb94b733c73`

2. **GameSpacedatabaseContext** (`GameSpace` connection string)
   - ALL business logic and domain data
   - Database: `GameSpacedatabase`
   - Used by all Areas for business operations
   - 88 tables, ~17,063 data rows

**Key Principle**: All Areas share the SAME `GameSpacedatabaseContext`. Never create Area-specific DbContexts.

### Areas Structure

Both projects use ASP.NET Areas for modular organization:

**GameSpace** (Admin-focused):
- `Areas/Forum/` - Forum management
- `Areas/Identity/` - ASP.NET Identity pages
- `Areas/MemberManagement/` - Member management
- `Areas/MiniGame/` - Mini-game admin system (complex, requires special registration)
- `Areas/social_hub/` - Social hub features (complex, includes SignalR)

**GamiPort** (Public-facing):
- `Areas/Forum/` - Public forum
- `Areas/Identity/` - User authentication
- `Areas/Login/` - Login system
- `Areas/MemberManagement/` - Member features
- `Areas/MiniGame/` - Mini-game client features
- `Areas/OnlineStore/` - E-commerce frontend
- `Areas/social_hub/` - Social features

### Area Registration Patterns

**Simple Areas** (Forum, MemberManagement):
- No special registration needed
- Controllers only need `[Area("AreaName")]` attribute
- Inject shared `GameSpacedatabaseContext` via constructor

**Complex Areas** (MiniGame, social_hub):
- Use `ServiceExtensions.cs` pattern for centralized DI registration
- Register in `Program.cs` with extension method
- Example: `builder.Services.AddMiniGameServices(builder.Configuration);`

## MiniGame Area - Special Focus

The MiniGame Area is the most complex module in this project, implementing a complete game backend system.

### Core Modules
1. **User_Wallet** - Member points and coupon system
2. **UserSignInStats** - Daily check-in system
3. **Pet** - Pet raising system with 5 attributes
4. **MiniGame** - Adventure game records

### Key Business Rules

**Wallet System**:
- Points cannot be negative
- Three types of items: Points, Coupons (CPN-YYYYMM-XXXXXX), E-Vouchers (EV-TYPE-XXXX-XXXXXX)
- All transactions must use database transactions to prevent concurrency issues
- All changes logged in `WalletHistory` table

**Check-in System**:
- One check-in per day (Asia/Taipei timezone)
- Weekday: +20 points, +0 exp
- Weekend: +30 points, +200 exp
- 7-day streak: +40 points, +300 exp
- Full month: +200 points, +2000 exp, +1 coupon

**Pet System**:
- 5 attributes (hunger, mood, stamina, cleanliness, health): 0-100 range
- Level-up formula:
  - Level 1-10: EXP = 40 × level + 60
  - Level 11-100: EXP = 0.8 × level² + 380
  - Level ≥ 101: EXP = 285.69 × (1.06^level)
- Color change cost: 2000 points per change
- Daily decay at 00:00: hunger -20, mood -30, stamina -10, cleanliness -20

**Mini-Game System**:
- Daily limit: 3 games per day (resets at 00:00 Asia/Taipei)
- Pet health check required before adventure starts
- Three difficulty levels with increasing rewards

### MiniGame Area Database Tables

**Wallet & Coupons**:
- `User_Wallet`, `WalletHistory`
- `Coupon`, `CouponType`
- `EVoucher`, `EVoucherType`, `EVoucherToken`, `EVoucherRedeemLog`

**Check-in**:
- `UserSignInStats`, `SignInRule`

**Pet & Game**:
- `Pet`, `MiniGame`

**Admin Permissions**:
- `ManagerData`, `ManagerRole`, `ManagerRolePermission`

### MiniGame Service Registration

The MiniGame Area uses a centralized service registration pattern in `Areas/MiniGame/config/ServiceExtensions.cs`:

```csharp
public static class ServiceExtensions
{
    public static IServiceCollection AddMiniGameServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // NOTE: DbContext is registered in Program.cs, NOT here
        // All services use the shared GameSpacedatabaseContext

        services.AddScoped<IMiniGameAdminService, MiniGameAdminService>();
        services.AddScoped<IUserWalletService, UserWalletService>();
        // ... 30+ service registrations

        return services;
    }
}
```

Registered in `Program.cs` with:
```csharp
builder.Services.AddMiniGameServices(builder.Configuration);
```

### Permission Control

Role-based access control (RBAC) using three tables:
- `ManagerRolePermission` - defines permissions per role
- `ManagerRole` - maps managers to roles
- `ManagerData` - manager account data

Permission fields:
- `AdministratorPrivilegesManagement` - Admin platform management
- `UserStatusManagement` - User management (required for Wallet, Check-in, MiniGame)
- `ShoppingPermissionManagement` - Shopping management
- `MessagePermissionManagement` - Message management
- `Pet_Rights_Management` - Pet management (required for Pet module)
- `customer_service` - Customer service

Controllers use: `[Authorize(AuthenticationSchemes = "AdminCookie", Policy = "AdminOnly")]`

## Database Guidelines

### Critical Rules
1. **Schema Authority**: The database in SQL Server is the single source of truth
2. **NO Migrations**: Never use Entity Framework Migrations to modify the schema
3. **Read-Only Queries**: Use `AsNoTracking()` for all read operations
4. **Transactions**: All write operations that modify points/coupons/wallet must use transactions
5. **UTF-8 Encoding**: All files should use UTF-8 (prefer no BOM for code, with BOM for documentation)

### Connection Strings
Located in `appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=aspnet-GameSpace-...",
    "GameSpace": "Data Source=(local)\\SQLEXPRESS01;Initial Catalog=GameSpacedatabase;..."
  }
}
```

## Development Workflow

### Team Boundaries
- Work is divided by Areas
- **Strict rule**: Only modify files within your assigned Area
- **Exception**: `Program.cs` can be modified ONLY to add necessary registration code
- Do not modify shared layouts, vendor files, or other Areas

### Code Organization
- Controllers: `Areas/{AreaName}/Controllers/`
- Services: `Areas/{AreaName}/Services/`
- Models: `Areas/{AreaName}/Models/`
- Views: `Areas/{AreaName}/Views/`
- Config: `Areas/{AreaName}/config/` (for complex Areas)

### Service Lifecycle
- Use `AddScoped` for most business services
- Use `AddSingleton` for stateless utilities and configuration
- Use `AddTransient` for lightweight, per-request services

## Routing

Standard Area routing is configured in `Program.cs`:
```csharp
// Area route (handles all Areas automatically)
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
```

Controllers only need `[Area("AreaName")]` attribute - do NOT add `[Route]` attributes.

## Testing

### Health Check
```
GET /healthz/db
Expected: {"status":"ok"}
```

### Test Accounts (from schema documentation)
**Admin accounts**:
- zhang_zhiming_01 / AdminPass001@ (Full permissions)
- li_xiaohua_02 / SecurePass002# (User & Forum management)
- wang_meiling_03 / StrongPwd003! (Shopping & Pet management)

**Member account**:
- dragonknight88 / Password001@

## Key Technical Patterns

### Dependency Injection
Always inject dependencies via constructor, never create instances manually:
```csharp
public class MyController : Controller
{
    private readonly GameSpacedatabaseContext _context;

    public MyController(GameSpacedatabaseContext context)
    {
        _context = context;
    }
}
```

### Authentication Schemes
- Admin backend: `AdminCookie` authentication scheme
- Claims-based authorization with `IsManager=true` claim
- Policy-based authorization: `[Authorize(Policy = "AdminOnly")]`

### SignalR Integration (social_hub)
```csharp
// In Program.cs
builder.Services.AddSignalR();

// Hub mapping
app.MapHub<ChatHub>("/social_hub/chatHub", opts => {
    opts.Transports = HttpTransportType.WebSockets
        | HttpTransportType.ServerSentEvents
        | HttpTransportType.LongPolling;
});
```

## Documentation References

Comprehensive documentation is available in the `schema/` directory:
- `README_合併版.md` - Complete project specification (MiniGame Admin focus)
- `Area註冊架構說明.md` - Area registration architecture guide
- `MiniGame_Area_完整描述文件.md` - Complete MiniGame Area specification
- `SQL_Server_連線操作完整手冊_AI適用.md` - SQL Server connection guide
- `MiniGame_Area_資料庫完整結構文件_2025-10-21.md` - Database structure documentation

## Special Notes

1. **Language**: User interface uses Traditional Chinese (zh-TW), but code identifiers, file names, and SQL/CLI keywords must remain in English
2. **File Limits**: Keep commits small - max 3 files or 400 lines per batch
3. **UI Frameworks**:
   - Admin backend uses SB Admin template (do not modify vendor files)
   - Public frontend uses Bootstrap-based design (reference `index.txt`)
4. **No Generic Documentation**: Do not create generic dev guides, best practices docs, or TODO files unless explicitly required
