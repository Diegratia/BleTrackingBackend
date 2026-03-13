# Alur Kerja SaaS Licensing & Module Management

## Alur Utama Aktivasi Lisensi

```
┌─────────────┐         ┌─────────────┐         ┌──────────────┐
│   Customer  │         │   System     │         │ License      │
│  (IT Admin) │         │   Admin      │         │ Generator    │
└──────┬──────┘         └──────┬──────┘         └──────┬───────┘
       │                      │                       │
       │  1. Request License  │                       │
       │─────────────────────>│                       │
       │                      │                       │
       │                      │  2. Generate License   │
       │                      │──────────────────────>│
       │                      │                       │
       │                      │  3. License File       │
       │                      │<──────────────────────│
       │                      │                       │
       │  4. Send License File│                       │
       │<─────────────────────│                       │
       │                      │                       │
       │  5. Activate License │                       │
       │─────────────────────>│                       │
       │                      │                       │
       │                      │  6. License Active      │
       │<─────────────────────│                       │
       │                      │                       │
```

### Penjelasan Alur Aktivasi:

1. **Request License** - Customer mengirim Machine ID dan informasi perusahaan
2. **Generate License** - System Admin membuat license file menggunakan LicenseGenerator
3. **License File** - File lisensi (.txt) dibuat dengan tanda tangan digital
4. **Send License File** - License file dikirim ke customer via email
5. **Activate License** - Customer mengaktifkan menggunakan CLI atau API
6. **License Active** - Sistem memvalidasi dan mengaktifkan lisensi

---

## Alur Validasi Lisensi (Runtime)

```
┌──────────────┐     ┌──────────────┐     ┌──────────────┐
│   Client     │     │   API        │     │  License     │
│  Application │     │  Service     │     │   Service    │
└──────┬───────┘     └──────┬───────┘     └──────┬───────┘
       │                     │                     │
       │  1. API Request      │                     │
       │────────────────────>│                     │
       │                     │                     │
       │                     │  2. Check License    │
       │                     │────────────────────>│
       │                     │                     │
       │                     │  3. License Info      │
       │                     │<────────────────────│
       │                     │                     │
       │                     │  4. Is License Valid? │
       │                     │──────┐              │
       │                     │      │              │
       │                     │<─────┘              │
       │                     │                     │
       │  5. Response        │                     │
       │<────────────────────│                     │
       │                     │                     │
```

### Penjelasan Alur Validasi:

1. **API Request** - Client mengirim request dengan JWT token
2. **Check License** - API memeriksa status lisensi dari database
3. **License Info** - Mengambil informasi lisensi (tipe, expiry, features)
4. **Is Valid?** - Memvalidasi:
   - Apakah lisensi sudah kadaluarsa?
   - Apakah application status active?
   - Apakah fitur yang diminta tersedia?
5. **Response** - Mengembalikan response sesuai validasi

---

## Alur Fitur Active Directory Sync

### 1. Konfigurasi AD Sync

```
┌─────────────┐         ┌─────────────┐         ┌──────────────┐
│   Admin     │         │   AD API     │         │  Database    │
│  Dashboard  │         │  Endpoint    │         │              │
└──────┬──────┘         └──────┬──────┘         └──────┬───────┘
       │                      │                       │
       │  1. Configure AD    │                       │
       │────────────────────>│                       │
       │                      │                       │
       │                      │  2. Check Feature     │
       │                      │────────────────────>│
       │                      │                       │
       │                      │  3. Is AD Module On?  │
       │                      │<────────────────────│
       │                      │                       │
       │     Yes              │                       │
       │<─────────────────────│                       │
       │                      │                       │
       │  4. Save Config      │                       │
       │────────────────────>│                       │
       │                      │  5. Store Config       │
       │                      │────────────────────>│
       │                      │                       │
       │  6. Config Saved     │                       │
       │<────────────────────│                       │
```

### 2. Proses Sync AD (Manual)

```
┌─────────────┐         ┌─────────────┐         ┌──────────┐    ┌──────────┐
│   Admin     │         │   AD API     │         │    AD    │    │ Database │
│  Dashboard  │         │  Endpoint    │         │  Server  │    │          │
└──────┬──────┘         └──────┬──────┘         └────┬─────┘    └────┬─────┘
       │                      │                     │             │
       │  1. Trigger Sync     │                     │             │
       │────────────────────>│                     │             │
       │                      │                     │             │
       │                      │  2. Validate Feature│             │
       │                      │──────┐              │             │
       │                      │      │              │             │
       │                      │<─────┘              │             │
       │                      │                     │             │
       │                      │  3. Get AD Users     │             │
       │                      │────────────────────>│             │
       │                      │                     │             │
       │                      │  4. User List        │             │
       │                      │<────────────────────│             │
       │                      │                     │             │
       │                      │  5. Create/Update    │             │
       │                      │    Members           │             │
       │                      │────────────────────────────────────>│
       │                      │                     │             │
       │                      │  6. Update Sync Status│             │
       │                      │────────────────────────────────────>│
       │                      │                     │             │
       │  7. Sync Result      │                     │             │
       │<────────────────────│                     │             │
```

### 3. Proses Sync AD (Background/otomatis)

```
┌──────────────────┐         ┌──────────────────┐
│ Background       │         │   Database       │
│ Service          │         │                  │
│  (Runs every     │         │                  │
│   1 minute)      │         │                  │
└──────┬───────────┘         └──────┬───────────┘
       │                            │
       │  1. Check due syncs       │
       │───────────────────────────>│
       │                            │
       │  2. Get configs to sync    │
       │<───────────────────────────│
       │                            │
       │  3. For each config:       │
       │     - Check if due         │
       │     - Check if enabled      │
       │     - Check feature flag   │
       │                            │
       │  4. Trigger sync if due    │
       │                            │
       │  5. Update sync status     │
       │───────────────────────────>│
       │                            │
       │  6. Wait 1 minute          │
       │────────────────────────────│
       │                            │
       ▼                            ▼
```

---

## Alur SSO Login

```
┌─────────────┐     ┌─────────────┐     ┌──────────────┐     ┌──────────────┐
│   User      │     │   Browser   │     │   API        │     │  Feature     │
│  (Windows)  │     │             │     │  Service     │     │  Service     │
└──────┬──────┘     └──────┬──────┘     └──────┬───────┘     └──────┬───────┘
       │                   │                   │                    │
       │  1. Click Login   │                   │                    │
       │──────────────────>│                   │                    │
       │                   │                   │                    │
       │                   │  2. Send Request  │                    │
       │                   │   with Windows    │                    │
       │                   │   Credentials     │                    │
       │                   │──────────────────>│                    │
       │                   │                   │                    │
       │                   │                   │  3. Check SSO      │
       │                   │                   │    Feature         │
       │                   │                   │──────────────────>│
       │                   │                   │                    │
       │                   │                   │  4. Is Enabled?    │
       │                   │                   │<──────────────────│
       │                   │                   │                    │
       │                   │      Yes          │                    │
       │                   │<──────────────────│                    │
       │                   │                   │                    │
       │                   │  5. Validate User  │                    │
       │                   │──────────────────>│                    │
       │                   │                   │                    │
       │                   │  6. Create JWT     │                    │
       │                   │<──────────────────│                    │
       │                   │                   │                    │
       │  7. JWT Token     │                   │                    │
       │<──────────────────│                   │                    │
       │                   │                   │                    │
```

---

## Alur Cek Feature Flag di Kode

### 1. Di Controller (Aplikasi biasa)

```csharp
[HttpGet("export")]
[MinLevel(LevelPriority.PrimaryAdmin)]
public async Task<IActionResult> ExportReport()
{
    // 1. Inject IFeatureService
    // 2. Cek apakah fitur reporting aktif
    if (!await _featureService.IsFeatureEnabledAsync("core.reporting", AppId))
    {
        return Forbid("Reporting module is not enabled");
    }

    // 3. Lanjutkan export data
    // ...
}
```

### 2. Di Middleware (Global check)

```csharp
public class FeatureFlagMiddleware
{
    public async Task InvokeAsync(HttpContext context, IFeatureService featureService)
    {
        var endpoint = context.Request.Path;
        var appId = GetApplicationId(context);

        // 1. Cek endpoint memerlukan fitur tertentu
        var requiredFeature = GetRequiredFeature(endpoint);

        if (requiredFeature != null)
        {
            // 2. Validasi fitur
            var isEnabled = await featureService.IsFeatureEnabledAsync(requiredFeature, appId);

            if (!isEnabled)
            {
                context.Response.StatusCode = 403;
                return;
            }
        }

        // 3. Lanjut ke endpoint berikutnya
        await _next(context);
    }
}
```

---

## Diagram State Transisi Lisensi

```
┌──────────────────────────────────────────────────────────────────┐
│                        License State Machine                       │
├──────────────────────────────────────────────────────────────────┤
│                                                                   │
│   ┌─────────┐    Activate    ┌──────────┐    Expire/Purge   ┌───┐  │
│   │  None   │───────────────>│  Active  │─────────────────>│In  │  │
│   └─────────┘                 └──────────┘                   │active│  │
│                                                                  │  │  │
│                                                                  │  └───┘
│                                                                  │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                   Active States                           │   │
│   ├─────────────┬─────────────┬─────────────┬──────────────┤   │
│   │    Trial    │   Annual    │  Perpetual  │  grace       │   │
│   │   (7 days)  │  (1 year)   │ (unlimited)  │  period      │   │
│   └─────────────┴─────────────┴─────────────┴──────────────┘   │
│                                                                  │
│   ┌─────────────────────────────────────────────────────────┐   │
│   │                 Feature States                            │   │
│   ├─────────────┬─────────────┬─────────────┬──────────────┤   │
│   │     On      │     Off     │   Cached    │   Refresh    │   │
│   │  (enabled)  │  (disabled) │  (memory)   │  (renew)     │   │
│   └─────────────┴─────────────┴─────────────┴──────────────┘   │
└──────────────────────────────────────────────────────────────────┘
```

---

## Alur Error Handling

### License Expired
```
API Request → LicenseService → Check Expiry → EXPIRED
                                                  │
                                                  ▼
                                      Return 403 Forbidden
                                      {
                                        "message": "License expired on 2025-03-13",
                                        "renewLink": "/api/license/renew"
                                      }
```

### Feature Not Enabled
```
API Request → FeatureService → Check Feature → NOT ENABLED
                                                   │
                                                   ▼
                                       Return 403 Forbidden
                                       {
                                         "message": "Active Directory module is not enabled",
                                         "featureKey": "saas.activeDirectory",
                                         "contactSupport": true
                                       }
```

### Machine ID Mismatch
```
License Activation → Validate Machine ID → MISMATCH
                                               │
                                               ▼
                                   Return 400 Bad Request
                                   {
                                     "message": "License is not for this machine",
                                     "expected": "abc123...",
                                     "current": "def456..."
                                   }
```

---

## Alur Cache Feature Flags

```
┌────────────────────────────────────────────────────────────────────┐
│                    Feature Flag Caching Strategy                   │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  1. First Request                                                 │
│     └──> Query Database → Get Enabled Features → Cache (5 min)    │
│                                                                    │
│  2. Subsequent Requests (within 5 minutes)                          │
│     └──> Return from Cache → Fast Response                         │
│                                                                    │
│  3. Cache Expiry                                                   │
│     └──> Remove from Cache → Next request queries Database        │
│                                                                    │
│  4. Manual Refresh (POST /api/license/feature/refresh)            │
│     └──> Clear Cache → Force Database Query                        │
│                                                                    │
│  5. License Activation                                             │
│     └──> Update Features → Clear Cache → New Features Apply        │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

---

## Alur Security Checks

### Multi-Layer Security
```
┌────────────────────────────────────────────────────────────────────┐
│                      Security Check Layers                         │
├────────────────────────────────────────────────────────────────────┤
│                                                                    │
│  Layer 1: JWT Authentication                                       │
│  └──> Validate token, extract user info & application ID           │
│                                                                    │
│  Layer 2: Authorization (MinLevel)                                  │
│  └──> Check user has required role (SuperAdmin, PrimaryAdmin, etc)│
│                                                                    │
│  Layer 3: License Validation                                        │
│  └──> Check license is valid and not expired                       │
│                                                                    │
│  Layer 4: Feature Flag Check                                       │
│  └──> Verify requested feature is enabled for this application    │
│                                                                    │
│  Layer 5: Resource Ownership                                       │
│  └──> Ensure user can only access their application's resources   │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

---

## Contoh Use Case: Menambah User Baru dengan AD Sync

```
1. Admin mengkonfigurasi AD connection
   │
   ├── POST /api/ad/config
   │   └──> { server, domain, credentials, ... }
   │
2. Admin mengaktifkan AD sync
   │
   ├── POST /api/ad/toggle/{id}
   │   └──> { enabled: true }
   │
3. Background service mendeteksi config baru
   │
   └──> AdSyncBackgroundService (runs every 1 min)
       │
       └──> Sync interval tercapai → Trigger sync
           │
           ├── Connect ke LDAP server
           ├── Query user list
           ├── Create/update MstMember records
           └── Update sync status
```

---

## Summary Flow Diagram (End-to-End)

```
┌──────────────────────────────────────────────────────────────────────┐
│                        Complete Licensing Flow                        │
├──────────────────────────────────────────────────────────────────────┤
│                                                                        │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐          │
│  │ Customer │   │  License │   │ License  │   │  System  │          │
│  │   Server │   │Generator │   │ Checker  │   │  Admin   │          │
│  └─────┬────┘   └─────┬────┘   └─────┬────┘   └─────┬────┘          │
│        │               │               │               │              │
│        │ Generate      │               │               │              │
│        │ License──────>│               │               │              │
│        │               │               │               │              │
│        │  License File │               │               │              │
│        │<──────────────│               │               │              │
│        │               │               │               │              │
│        │  Activate     │               │               │              │
│        │  CLI/API──────>──────────────>│               │              │
│        │               │               │               │              │
│        │               │ Validate & Store│               │              │
│        │               │<───────────────│               │              │
│        │               │               │               │              │
│        │               │               │ Configure     │              │
│        │               │               │ Features──────>│              │
│        │               │               │               │              │
│        │  Application  │               │               │              │
│        │  Running      │               │               │              │
│        │               │               │               │              │
│        │  API Requests │               │               │              │
│        │  (with JWT)───>──────────────────────────────>│              │
│        │               │               │               │              │
│        │               │               │ Check         │              │
│        │               │               │ License &     │              │
│        │               │               │ Features──────>│              │
│        │               │               │               │              │
│        │  Response    │               │               │              │
│        │  (Allowed/Denied)             │               │              │
│        │<───────────────────────────────────────│               │              │
│        │               │               │               │              │
│                                                                        │
│  ┌──────────────────────────────────────────────────────────────────┐ │
│  │                     Database Storage                               │ │
│  ├──────────────────────────────────────────────────────────────────┤ │
│  │  mst_application                                                    │ │
│  │  ├── enabled_features: ["core.*", "saas.*"]                       │ │
│  │  ├── license_tier: "annual"                                       │ │
│  │  ├── customer_name: "PT. ABC"                                    │ │
│  │  └── license_machine_id: "abc123..."                               │ │
│  │                                                                    │ │
│  │  active_directory_configs                                          │ │
│  │  ├── server, domain, credentials...                                │ │
│  │  ├── sync_interval_minutes                                         │ │
│  │  └── last_sync_at, total_users_synced                              │ │
│  └──────────────────────────────────────────────────────────────────┘ │
│                                                                        │
└──────────────────────────────────────────────────────────────────────┘
```
