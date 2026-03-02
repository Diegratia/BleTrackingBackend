# Alur Lengkap Fitur Evacuation

## Overview

Fitur Evacuation adalah sistem untuk memantau dan mengelola proses evakuasi darurat. Sistem terdiri dari 3 komponen utama:

1. **CMS (.NET Backend)** - Manajemen evakuasi, API, konfirmasi
2. **Engine (mqtt-bleapp)** - Deteksi real-time orang di assembly point, publish live count
3. **Frontend** - Tampilan live status (aplikasi terpisah)

---

## Arsitektur

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              FRONTEND (React)                                │
│  - Subscribe MQTT: evacuation/status/{appId}                                 │
│  - Display live count: Required/Evacuated/Remaining                         │
│  - Poll API CMS untuk detail & confirm                                      │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │ MQTT Subscribe
                                    │
┌─────────────────────────────────────────────────────────────────────────────┐
│                            ENGINE (mqtt-bleapp)                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  evacuationService.js                                               │    │
│  │  - Subscribe MQTT: evacuation/trigger/+/complete/+/cancel/+         │    │
│  │  - Track orang di assembly point (in-memory Map)                    │    │
│  │  - totalRequired = latestBeaconState.size                           │    │
│  │  - Publish live count ke Frontend (per 5 detik)                     │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  evacuationStorage.js                                               │    │
│  │  - Buffer detections → Flush ke DB (per 5 detik)                    │    │
│  │  - MERGE ke evacuation_transactions table                           │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
                                    ▲
                                    │ MQTT Publish
                                    │
┌─────────────────────────────────────────────────────────────────────────────┐
│                              CMS (.NET Backend)                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  EvacuationAlertController / Service                                │    │
│  │  - POST /api/evacuation-alert (trigger)                             │    │
│  │  - PUT /api/evacuation-alert/{id}/complete                          │    │
│  │  - PUT /api/evacuation-alert/{id}/cancel                            │    │
│  │  - GET /api/evacuation-alert/{id}/summary                           │    │
│  │  - Publish MQTT trigger → Engine                                    │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  EvacuationTransactionController / Service                          │    │
│  │  - GET /api/evacuation-transaction (list all)                       │    │
│  │  - PUT /api/evacuation-transaction/{id}/confirm                     │    │
│  │  - GET /api/evacuation-alert/{id}/person-status                    │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## Flow Lengkap: Start → Finish

### Tahap 1: TRIGGER EVACUATION

| Step | Komponen | Aksi | Detail |
|------|----------|------|--------|
| 1.1 | Frontend | User klik "Trigger Evacuation" | SuperAdmin memulai evakuasi |
| 1.2 | Frontend → CMS | `POST /api/evacuation-alert` | Kirim payload: `{ assemblyPointIds, status, notes }` |
| 1.3 | CMS | `EvacuationAlertService.CreateAsync()` | - Validasi tidak ada evakuasi aktif<br>- Create record `evacuation_alerts`<br>- Status = "Active" |
| 1.4 | CMS → Engine | MQTT Publish | Topic: `evacuation/trigger/{appId}`<br>Payload: `{ evacuationAlertId, status: "Active", triggeredAt, applicationId }` |
| 1.5 | Engine | `handleEvacuationTrigger()` | - Set `activeEvacuation` state<br>- `totalRequired = latestBeaconState.size`<br>- Start summary interval (5s) |

---

### Tahap 2: DETEKSI REAL-TIME (Berjalan terus selama Active)

| Step | Komponen | Aksi | Detail |
|------|----------|------|--------|
| 2.1 | Engine Hardware | Beacon menerima sinyal | Reader detect beacon card |
| 2.2 | Engine | `realtimeService.js` | - Calculate position (x, y)<br>- Get person data dari `cardCache` |
| 2.3 | Engine | `processPersonForEvacuation()` | Untuk setiap posisi valid:<br>- Cek apakah di assembly point (`pointInPolygon`)<br>- Jika ya: tambah ke `evacuatedPersons` Map |
| 2.4 | Engine → Storage | `saveEvacuationDetections()` | Buffer detection records (max 100, flush per 5s) |
| 2.5 | Storage → DB | `MERGE evacuation_transactions` | - INSERT jika baru<br>- UPDATE `last_detected_at` jika existing |
| 2.6 | Engine → Frontend | MQTT Publish (per 5s) | Topic: `evacuation/status/{appId}`<br>Payload: `{ totalRequired, totalEvacuated, totalRemaining, status }` |
| 2.7 | Frontend | Display live count | Update UI dengan real-time data |

**Person Key Format**: `{personCategory}-{personId}`
- `0-{memberId}` = Member
- `1-{visitorId}` = Visitor
- `2-{securityId}` = Security

---

### Tahap 3: KONFIRMASI EVAKUASI (Opsional, selama Active)

| Step | Komponen | Aksi | Detail |
|------|----------|------|--------|
| 3.1 | Frontend | Security klik "Confirm" pada orang | Pilih person yang sudah diverifikasi |
| 3.2 | Frontend → CMS | `PUT /api/evacuation-transaction/{id}/confirm` | Payload: `{ personStatus: "Confirmed", confirmationNotes }` |
| 3.3 | CMS | `EvacuationTransactionService.ConfirmAsync()` | - UPDATE `evacuation_transactions`<br>- `person_status = 2` (Confirmed)<br>- Set `confirmed_by`, `confirmed_at` |

---

### Tahap 4: COMPLETE EVACUATION

| Step | Komponen | Aksi | Detail |
|------|----------|------|--------|
| 4.1 | Frontend | User klik "Complete Evacuation" | Semua orang sudah dievakuasi |
| 4.2 | Frontend → CMS | `POST /api/evacuation-alert/{id}/complete` | Payload: `{ completionNotes }` |
| 4.3 | CMS | `EvacuationAlertService.CompleteAsync()` | - UPDATE `evacuation_alerts` set `status = "Completed"`<br>- Hitung final summary |
| 4.4 | CMS → Engine | MQTT Publish | Topic: `evacuation/complete/{appId}`<br>Payload: `{ evacuationAlertId, status: "Completed", completedAt }` |
| 4.5 | Engine | `handleEvacuationComplete()` | - Stop summary interval<br>- Clear `activeEvacuation` state |
| 4.6 | Frontend | Tampilkan summary | Final evacuation report |

---

### Tahap 5: CANCEL EVACUATION (Opsional - jika false alarm)

| Step | Komponen | Aksi | Detail |
|------|----------|------|--------|
| 5.1 | Frontend | User klik "Cancel Evacuation" | False alarm atau dibatalkan |
| 5.2 | Frontend → CMS | `POST /api/evacuation-alert/{id}/cancel` | |
| 5.3 | CMS | `EvacuationAlertService.CancelAsync()` | UPDATE `evacuation_alerts` set `status = "Cancelled"` |
| 5.4 | CMS → Engine | MQTT Publish | Topic: `evacuation/cancel/{appId}` |
| 5.5 | Engine | `handleEvacuationCancel()` | Stop processing |

---

## MQTT Topics Summary

### Dari CMS → Engine
| Topic | Purpose | Payload |
|-------|---------|---------|
| `evacuation/trigger/{appId}` | Mulai evakuasi | `{ evacuationAlertId, status, triggeredAt, applicationId }` |
| `evacuation/complete/{appId}` | Selesai evakuasi | `{ evacuationAlertId, status, completedAt, completedBy }` |
| `evacuation/cancel/{appId}` | Batalkan evakuasi | `{ evacuationAlertId, status, cancelledAt }` |
| `engine/refresh/evacuation-assembly-points` | Refresh assembly point data | `{ floorplanId, assemblyPoints: [...] }` |

### Dari Engine → Frontend
| Topic | Purpose | Payload |
|-------|---------|---------|
| `evacuation/status/{appId}` | Live count update | `{ EvacuationAlertId, Status, Timestamp, TotalRequired, TotalEvacuated, TotalConfirmed, TotalRemaining }` |

---

## Database Tables

### `evacuation_alerts`
| Column | Description |
|--------|-------------|
| `id` | PK |
| `alert_status` | Active, Paused, Completed, Cancelled |
| `total_required` | Jumlah orang yang harus dievakuasi |
| `total_evacuated` | Jumlah terdeteksi di assembly point |
| `total_confirmed` | Jumlah sudah dikonfirmasi |
| `total_remaining` | total_required - total_evacuated |
| `triggered_by` | Username yang trigger |
| `completed_by` | Username yang complete |
| `application_id` | Tenant ID |

### `evacuation_transactions`
| Column | Description |
|--------|-------------|
| `id` | PK |
| `evacuation_alert_id` | FK → evacuation_alerts |
| `evacuation_assembly_point_id` | FK → assembly point |
| `person_category` | 0=Member, 1=Visitor, 2=Security |
| `member_id` | FK → mst_member (nullable) |
| `visitor_id` | FK → visitor (nullable) |
| `security_id` | FK → mst_security (nullable) |
| `card_id` | FK → card (nullable) |
| `person_status` | 0=Remaining, 1=Evacuated, 2=Confirmed |
| `detected_at` | Waktu deteksi pertama |
| `last_detected_at` | Waktu deteksi terakhir |
| `confirmed_by` | Username yang konfirmasi |
| `confirmed_at` | Waktu konfirmasi |
| `confirmation_notes` | Catatan konfirmasi |
| `application_id` | Tenant ID |

---

## Key Files Reference

### CMS (.NET Backend)
| File | Purpose |
|------|---------|
| `Shared/BusinessLogic.Services/Implementation/EvacuationAlertService.cs` | Core evacuation logic |
| `Shared/BusinessLogic.Services/Implementation/EvacuationTransactionService.cs` | Transaction & confirmation logic |
| `Shared/Web.API.Controllers/Controllers/EvacuationAlertController.cs` | Evacuation API endpoints |
| `Shared/Web.API.Controllers/Controllers/EvacuationTransactionController.cs` | Transaction API endpoints |
| `Shared/Repositories/Repository/EvacuationAlertRepository.cs` | DB operations for alerts |
| `Shared/Repositories/Repository/EvacuationTransactionRepository.cs` | DB operations for transactions |
| `Shared/BusinessLogic.Services/Background/IMqttPubQueue.cs` | MQTT publishing interface |

### Engine (mqtt-bleapp)
| File | Purpose |
|------|---------|
| `src/services/evacuationService.js` | Evacuation detection & MQTT handling |
| `src/modules/storage/evacuationStorage.js` | DB write buffer for transactions |
| `src/services/realtimeService.js` | Integration with tracking loop |
| `src/cache/occupantCounter.js` | `latestBeaconState` for totalRequired |

---

## Person Status Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                                                                 │
│   0 = REMAINING        (belum terdeteksi di assembly point)      │
│         │                                                     │
│         │ (terdeteksi oleh Engine di assembly point)           │
│         ▼                                                     │
│   1 = EVACUATED        (otomatis oleh Engine)                   │
│         │                                                     │
│         │ (security konfirmasi manual)                         │
│         ▼                                                     │
│   2 = CONFIRMED        (manual oleh user via CMS)               │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Summary Count Formulas

| Field | Source | Formula |
|-------|--------|---------|
| `totalRequired` | Engine | `latestBeaconState.size` (real-time detected beacons) |
| `totalEvacuated` | Engine | `activeEvacuation.evacuatedPersons.size` (in-memory count) |
| `totalConfirmed` | CMS | Count dari `evacuation_transactions` where `person_status = 2` |
| `totalRemaining` | Engine | `totalRequired - totalEvacuated` |
