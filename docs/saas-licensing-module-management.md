# SaaS Licensing & Module Management System

## Overview

The SaaS Licensing & Module Management System provides a flexible licensing framework with tiered pricing and feature module management. This system enables:

- **License Tier Management** - Trial (7 days), Annual (1 year), and Perpetual licenses
- **Feature Flag System** - Enable/disable specific features per application
- **SaaS Modules** - Active Directory Sync and SSO as add-on features
- **Hybrid CLI + API** - Initial activation via CLI tool, runtime management via API

---

## Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                         Licensing Architecture                        │
├─────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌──────────────┐    ┌──────────────┐    ┌──────────────┐          │
│  │ License API  │    │   Feature    │    │ License      │          │
│  │  (Port 5032) │    │   Service    │    │ Checker CLI  │          │
│  └──────────────┘    └──────────────┘    └──────────────┘          │
│          │                  │                    │                    │
│          └──────────────────┴────────────────────┘                │
│                             │                                       │
│                             ▼                                       │
│                    ┌──────────────┐                                │
│                    │  DbContext   │                                │
│                    │ & Repository │                                │
│                    └──────────────┘                                │
│                             │                                       │
│                             ▼                                       │
│                    ┌──────────────────────────┐                    │
│                    │   MstApplication         │                    │
│                    │   - EnabledFeatures      │                    │
│                    │   - LicenseTier          │                    │
│                    │   - CustomerName         │                    │
│                    └──────────────────────────┘                    │
└─────────────────────────────────────────────────────────────────────┘
```

---

## License Types

| Type | Duration | Description |
|------|----------|-------------|
| **Trial** | 7 days | For evaluation and testing |
| **Annual** | 1 year | Standard annual subscription |
| **Perpetual** | Unlimited | Lifetime license with all features |

---

## Features

### Core Features (Base License)

| Feature Key | Display Name | Description |
|-------------|--------------|-------------|
| `core.masterData` | Master Data Management | Manage employees, visitors, buildings, floors, etc. |
| `core.tracking` | Real-Time Tracking | BLE-based position tracking |
| `core.monitoring` | Monitoring Dashboard | Live monitoring and map views |
| `core.alarm` | Alarm & Notification | Alert triggers and notifications |
| `core.patrol` | Patrol Management | Patrol routes and checkpoints |
| `core.reporting` | Reports & Analytics | Reports and data export |

### SaaS Modules (Add-ons)

| Feature Key | Display Name | Description |
|-------------|--------------|-------------|
| `saas.activeDirectory` | Active Directory Sync | Automatic employee synchronization from AD |
| `saas.sso` | Single Sign-On (SSO) | Windows authentication and SSO |

---

## License API Endpoints

### Get License Information
```http
GET /api/license/info
Authorization: Bearer {token}
Min-Level: PrimaryAdmin
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isValid": true,
    "message": "License is valid",
    "licenseType": "Annual",
    "licenseTier": "annual",
    "customerName": "PT. Example Company",
    "expirationDate": "2026-03-13",
    "daysRemaining": 365,
    "features": {
      "core": {
        "masterData": true,
        "tracking": true,
        "monitoring": true,
        "alarm": true,
        "patrol": true,
        "reporting": true
      },
      "saas": {
        "activeDirectory": true,
        "sso": true
      }
    }
  }
}
```

### Get Enabled Features
```http
GET /api/license/features
Authorization: Bearer {token}
Min-Level: PrimaryAdmin
```

**Response:**
```json
{
  "success": true,
  "data": {
    "core": {
      "core.masterData": {
        "key": "core.masterData",
        "displayName": "Master Data Management",
        "description": "Manage master data...",
        "isEnabled": true
      }
    },
    "saas": {
      "saas.activeDirectory": {
        "key": "saas.activeDirectory",
        "displayName": "Active Directory Sync",
        "description": "Automatic employee sync...",
        "isEnabled": true
      }
    }
  }
}
```

### Check Feature Enabled
```http
GET /api/license/feature/{featureKey}
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "featureKey": "saas.activeDirectory",
    "isEnabled": true
  }
}
```

### Activate License
```http
POST /api/license/activate
Authorization: Bearer {token}
Min-Level: SuperAdmin
Content-Type: application/json

{
  "licenseContent": "base64-encoded-license-file",
  "isBase64": true
}
```

### Validate License
```http
POST /api/license/validate
Authorization: Bearer {token}
Min-Level: SuperAdmin
Content-Type: application/json

{
  "licenseContent": "base64-encoded-license-file",
  "isBase64": true
}
```

### Get Machine ID
```http
GET /api/license/machine-id
Authorization: Bearer {token}
Min-Level: SuperAdmin
```

**Response:**
```json
{
  "success": true,
  "data": {
    "machineId": "a1b2c3d4e5f6...",
    "instructions": "Provide this ID when requesting a license file"
  }
}
```

### Refresh Feature Cache
```http
POST /api/license/feature/refresh
Authorization: Bearer {token}
Min-Level: SuperAdmin
```

---

## Active Directory API Endpoints

### Get AD Configuration
```http
GET /api/ad/config
Authorization: Bearer {token}
Min-Level: PrimaryAdmin
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": "guid",
    "applicationId": "guid",
    "server": "ldap.example.com",
    "port": 389,
    "useSsl": true,
    "domain": "example.com",
    "serviceAccountDn": "CN=service,OU=Users,DC=example,DC=com",
    "searchBase": "OU=Users,DC=example,DC=com",
    "userObjectFilter": "(objectClass=user)",
    "syncIntervalMinutes": 60,
    "isEnabled": true,
    "autoCreateMembers": true
  }
}
```

### Save/Update AD Configuration
```http
POST /api/ad/config
Authorization: Bearer {token}
Min-Level: SuperAdmin
Content-Type: application/json

{
  "server": "ldap.example.com",
  "port": 389,
  "useSsl": true,
  "domain": "example.com",
  "serviceAccountDn": "CN=service,OU=Users,DC=example,DC=com",
  "serviceAccountPassword": "encrypted-password",
  "searchBase": "OU=Users,DC=example,DC=com",
  "syncIntervalMinutes": 60,
  "isEnabled": true,
  "autoCreateMembers": true,
  "defaultDepartmentId": "optional-guid"
}
```

### Trigger Manual Sync
```http
POST /api/ad/sync
Authorization: Bearer {token}
Min-Level: PrimaryAdmin
Content-Type: application/json

{
  "forceFullSync": false,
  "filter": "optional-ldap-filter"
}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "success": true,
    "message": "Sync completed",
    "usersSynced": 150,
    "usersCreated": 10,
    "usersUpdated": 140,
    "usersFailed": 0,
    "syncStartedAt": "2026-03-13T10:00:00Z",
    "syncCompletedAt": "2026-03-13T10:01:30Z",
    "durationSeconds": 90
  }
}
```

### Get Sync Status
```http
GET /api/ad/sync/status
Authorization: Bearer {token}
Min-Level: PrimaryAdmin
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isConfigured": true,
    "isEnabled": true,
    "lastSyncAt": "2026-03-13T10:00:00Z",
    "lastSyncStatus": "success",
    "lastSyncMessage": "Sync completed",
    "totalUsersSynced": 150,
    "syncIntervalMinutes": 60,
    "nextSyncAt": "2026-03-13T11:00:00Z"
  }
}
```

### Toggle AD Sync
```http
POST /api/ad/toggle/{id}
Authorization: Bearer {token}
Min-Level: SuperAdmin
Content-Type: application/json

{
  "enabled": true
}
```

### Test AD Connection
```http
POST /api/ad/test/{id}
Authorization: Bearer {token}
Min-Level: PrimaryAdmin
```

---

## SSO Feature Endpoints

### Check SSO Enabled
```http
GET /api/auth/sso/enabled
Authorization: Bearer {token}
```

**Response:**
```json
{
  "success": true,
  "data": {
    "isEnabled": true,
    "featureKey": "saas.sso",
    "featureName": "Single Sign-On (SSO)"
  }
}
```

### Windows SSO Login (Enhanced)
```http
GET /api/auth/login-sso
Authorization: Negotiate {windows-credentials}
```

**Behavior:**
- Checks if `saas.sso` feature is enabled for the application
- Returns 403 Forbidden if SSO module is not enabled
- Proceeds with Windows authentication if enabled

---

## Database Schema

### MstApplication (Modified)
```sql
ALTER TABLE mst_application ADD COLUMN enabled_features TEXT;
ALTER TABLE mst_application ADD COLUMN license_tier VARCHAR(50);
ALTER TABLE mst_application ADD COLUMN customer_name VARCHAR(255);
ALTER TABLE mst_application ADD COLUMN license_machine_id VARCHAR(255);
```

### ActiveDirectoryConfig (New Table)
```sql
CREATE TABLE active_directory_configs (
    id UNIQUEIDENTIFIER PRIMARY KEY,
    application_id UNIQUEIDENTIFIER NOT NULL,
    server VARCHAR(255) NOT NULL,
    port INT NOT NULL DEFAULT 389,
    use_ssl BIT NOT NULL DEFAULT 0,
    domain VARCHAR(255) NOT NULL,
    service_account_dn VARCHAR(512),
    service_account_password VARCHAR(512),
    search_base VARCHAR(255),
    user_object_filter VARCHAR(100) DEFAULT '(objectClass=user)',
    sync_interval_minutes INT NOT NULL DEFAULT 60,
    last_sync_at DATETIME2,
    last_sync_status VARCHAR(255),
    last_sync_message NVARCHAR(MAX),
    total_users_synced INT NOT NULL DEFAULT 0,
    is_enabled BIT NOT NULL DEFAULT 0,
    auto_create_members BIT NOT NULL DEFAULT 1,
    default_department_id UNIQUEIDENTIFIER,
    status INT NOT NULL DEFAULT 1,
    created_at DATETIME2 NOT NULL DEFAULT GETDATE(),
    updated_at DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT fk_ad_config_application FOREIGN KEY (application_id)
        REFERENCES mst_application(id),
    CONSTRAINT fk_ad_config_department FOREIGN KEY (default_department_id)
        REFERENCES mst_department(id)
);
```

---

## Usage Examples

### 1. Check if Feature is Enabled (C#)
```csharp
public class MyServiceController : ControllerBase
{
    private readonly IFeatureService _featureService;

    [HttpGet("export")]
    public async Task<IActionResult> ExportData()
    {
        // Check if reporting feature is enabled
        if (!await _featureService.IsFeatureEnabledAsync("core.reporting", AppId))
        {
            return Forbid("Reporting module is not enabled");
        }

        // Proceed with export...
    }
}
```

### 2. Get License Info (JavaScript/TypeScript)
```typescript
const response = await fetch('/api/license/info', {
    headers: {
        'Authorization': `Bearer ${token}`
    }
});

const { isValid, features, expirationDate } = await response.json();

if (!isValid) {
    // Show license expired message
} else if (!features.saas.activeDirectory) {
    // Disable AD sync button
}
```

### 3. Configure Active Directory (cURL)
```bash
curl -X POST https://api.example.com/api/ad/config \
  -H "Authorization: Bearer {token}" \
  -H "Content-Type: application/json" \
  -d '{
    "server": "ldap.company.com",
    "port": 389,
    "useSsl": true,
    "domain": "company.com",
    "serviceAccountDn": "CN=svc_account,OU=Service Accounts,DC=company,DC=com",
    "serviceAccountPassword": "P@ssw0rd",
    "searchBase": "OU=Employees,DC=company,DC=com",
    "syncIntervalMinutes": 60,
    "isEnabled": true,
    "autoCreateMembers": true
  }'
```

---

## Background Services

### AdSyncBackgroundService
- Runs every 1 minute to check for pending syncs
- Automatically triggers sync based on configured interval
- Only syncs when `saas.activeDirectory` feature is enabled
- Logs sync results for troubleshooting

---

## Security Considerations

1. **License Validation** - Public key embedded in application prevents tampering
2. **Machine Binding** - Licenses tied to specific machine ID
3. **Feature Enforcement** - Server-side validation prevents bypass
4. **Multi-Tenancy** - Each application has isolated feature set
5. **Audit Trail** - All license/feature changes logged

---

## Migration Guide

To enable the licensing system for existing applications:

1. **Run Database Migration**
   ```bash
   dotnet ef migrations add AddSaaSLicenseFeatures --project Shared/Repositories/Repositories.csproj --startup-project Services.API/License/License.csproj
   dotnet ef database update --project Shared/Repositories/Repositories.csproj --startup-project Services.API/License/License.csproj
   ```

2. **Generate License File** (using LicenseGenerator CLI)

3. **Activate License** (choose one):
   ```bash
   # Option A: CLI (for initial setup)
   LicenseChecker.exe activate license.txt

   # Option B: API (for admin dashboard)
   POST /api/license/activate
   ```

4. **Configure Features** (automatically set based on license tier)

---

## Troubleshooting

### License shows as expired
- Check `ApplicationExpired` column in `mst_application` table
- Verify system clock is accurate
- Check license tier (`trial` expires after 7 days)

### Feature not enabled after activation
- Call `/api/license/feature/refresh` to clear cache
- Verify feature key is in `EnabledFeatures` JSON
- Check license tier includes the feature

### AD Sync not working
- Verify `saas.activeDirectory` feature is enabled
- Check `is_enabled` flag in `active_directory_configs` table
- Review logs for LDAP connection errors
- Test connection using `/api/ad/test/{id}`

### SSO login returns 403
- Verify `saas.sso` feature is enabled for the application
- Check user has valid Windows credentials
- Ensure browser supports NTLM/Negotiate authentication

---

## Future Enhancements

- **OAuth/OIDC Providers** - Azure AD, Okta, Google Workspace
- **LDAP Group Mapping** - Map AD groups to application roles
- **License Transfer** - Move license between machines
- **Usage Analytics** - Track feature usage per tenant
- **Trial Extension** - Extend trial periods for evaluation
- **Module Marketplace** - In-app purchase of additional modules
