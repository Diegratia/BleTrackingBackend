# Implementation Plan: Alarm Analytics Optimization (Rute 2)

Dokumen ini memuat langkah-langkah optimalisasi (refactoring) sistem *Analytics* dari yang awalnya terlalu bergantung pada query berat di `AlarmRecordTracking` menuju tabel master kejadian `AlarmTriggers`.

## 1. Modifikasi Entity Model (`AlarmTriggers.cs`)
Tabel `AlarmTriggers` saat ini tidak membawa data `MaskedArea` karena sejak awal dikhususkan di `AlarmRecordTracking`. Penambahan relasi area sangat diperlukan:

Edit file: `Shared/Entities.Models/AlarmTriggers.cs`
```csharp
// Tambahkan Field / Kolom baru
[Column("floorplan_masked_area_id")]
public Guid? FloorplanMaskedAreaId { get; set; }

// Tambahkan Relasi Entity
public FloorplanMaskedArea FloorplanMaskedArea { get; set; }
```

## 2. Execute Code First Migration (Entity Framework)
Karena skema database berubah, jalankan Command EF Core berikut untuk push pembaruan *entity* ke database backend.

```bash
# Membuat file migrasi dari Shared Directory
dotnet ef migrations add AddMaskedAreaToAlarmTriggers -p Shared/Repositories -o Repositories/Migrations

# Menerapkan pada database target
dotnet ef database update -p Shared/Repositories
```

## 3. Modifikasi Business Logic (Pembuatan Trigger)
Cari di mana object `AlarmTriggers` pertama kali di-inisialisasi (di-_insert_). Hal ini biasanya berada di Message Consumer (MQTT / Kafka) atau Service seperti `AlarmTriggerService`.
- **Action**: Pastikan saat *Alarm* terpancing, service mengambil `FloorplanMaskedAreaId` dari data *Positioning/Beacon* pengunjung di detik yang bersangkutan, dan assign id tersebut ke dalam `AlarmTriggers` yang hendak disave.

## 4. Refactoring `AlarmAnalyticsIncidentRepository.cs`
Pindahkan logika query seluruh *metrics* dari _record tracking_ yang redundan dan memonopoli RAM karena proses `Distinct()` ke data trigger yang _streamlined_.

- **Target Berubah**: Semua `_context.AlarmRecordTrackings.AsNoTracking()` diganti ke `_context.AlarmTriggers.AsNoTracking()`.
- **Method yang terdampak**:
  - `GetAreaDailySummaryAsync`: Menghilangkan `Distinct()` dan bisa langsung _group_ `AlarmTriggers` via koneksi FK `FloorplanMaskedArea`.
  - `GetDailySummaryAsync`: Menghitung _event_ asli per hari, bukan menebak-nebak log pengunjung via `Distinct()`.
  - `GetStatusSummaryAsync`: Mengagregasi status secara langsung.
  - `GetVisitorSummaryAsync`, `GetBuildingSummaryAsync`, `GetHourlyStatusSummaryAsync`.

## 5. Backfill Data Historis (Opsional / Direkomendasikan)
Agar dashboard _Analytics_ tidak kosong (*blank*) untuk tanggal-tanggal historis lalu atau statistik bulanan sebelum *refactor* ini naik ke Production, jalankan *Script SQL* _one-time_ untuk menarik area dari history tracking untuk menetapkan lokasi default tiap kejadian *Trigger* yang lama.

*Query Referensi:*
```sql
UPDATE [AlarmTriggers]
SET floorplan_masked_area_id = (
    SELECT TOP 1 floorplan_masked_area_id 
    FROM [AlarmRecordTracking] 
    WHERE [AlarmRecordTracking].alarm_triggers_id = [AlarmTriggers].id
    ORDER BY timestamp ASC
)
WHERE floorplan_masked_area_id IS NULL;
```

## 6. Validasi Endpoint Analytics
Setelah refactoring di-deploy, bandingkan metrik API:
1. Hitung durasi response time dari Endpoint Analytics saat diuji stress-test. Peningkatan kecepatan query seharusnya naik 5x - 30x lebih cepat tergantung kepadatan log.
2. Bandingkan _accuracy_ (kecocokan) total log hari ini vs status summary per hari tersebut.
