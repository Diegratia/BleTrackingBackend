# Patrol Checkpoint Action & Validation - Integration Guide
*(For Node.js Tracking Engine)*

Dokumen ini menjelaskan integrasi dan mekanisme validasi terkait aksi *Patrol Checkpoint* yang dilakukan via Mobile App. 

Engine Tracking berbasis Node.js diharapkan memahami behavior ini agar `ArrivedAt` dan `LeftAt` dari Engine dapat sinkron secara teknis dengan API `checkpoint-action` baru di Backend .NET.

---

## 1. Core Concept: Status Lifecycle di `PatrolCheckpointLog`

Setiap kali Satpam memulai Sesi Patroli (`/start`), Backend akan membuat **semua Checkpoint Logs** berdasarkan rute dengan status awal:
*   `CheckpointStatus = 0` (AutoDetected)
*   `ArrivedAt = null`
*   `LeftAt = null`

Sistem mengandalkan **Engine Tracking Node.js** untuk memperbarui kapabilitas waktu hadir/perginya Security di suatu titik area:
1. Ketika Engine mendeteksi Satpam memasuki radius area Checkpoint `=>` Engine wajib mengisi timestamp `ArrivedAt`.
2. Ketika Engine mendeteksi Satpam keluar radius `=>` Engine wajib mengisi timestamp `LeftAt`.

## 2. Poin Integrasi: Anti-Cheating (Dwell Time & Live Location)

Ketika Mobile App mencoba men-submit laporan absen Checkpoint (via `POST /api/patrol-session/checkpoint-action`), Backend .NET akan melakukan *hard-validation* untuk memastikan status absensi tidak dimanipulasi:

### A. Validasi Live Location (Kehadiran Nyata)
Backend mengeksekusi pengecekan ketat pada baris log:
```sql
ArrivedAt IS NOT NULL AND LeftAt IS NULL
```
**Apa artinya bagi Tracking Engine?**
1. Engine **wajib secepat mungkin** meng-update `ArrivedAt` ketika Security terdeteksi memasuki Area Checkpoint. Jika Engine terlambat/delay, Mobile App tidak akan bisa submit (ditolak dengan error "Security hasn't arrived or has left").
2. Selama Satpam mengutak-atik form Case/Aman di Mobile App, **Engine TIDAK BOLEH men-set `LeftAt`**. Kalau sinyal GPS/BLE terpantul-pantul (*bouncing* / pindah area sebentar lalu balik lagi), form submit Checkpoint di HP akan mendadak tertolak apabila Engine keburu men-set `LeftAt`.
*Saran untuk Engine: Berikan Tolenransi (Buffer/Debounce)* beberapa detik sebelum memvonis `LeftAt` untuk mengakomodir jitter sinyal saat Satpam berdiri diam.

### B. Validasi Dwell Time (Waktu Tunggu Minimal)
Backend mewajibkan Satpam untuk **berada di Area tersebut minimal 30 detik** sebelum mereka diizinkan memencet tombol "Aman" atau "Ada Kasus" di Mobile App.
```csharp
if ((waktu_sekarang - ArrivedAt).TotalSeconds < 30) { reject() }
```
**Apa artinya bagi Tracking Engine?**
Sinkronisasi jam (NTP) antara server Node.js Engine (yang menyetor nilai timestamp `ArrivedAt`) dan server .NET Backend harus terkalibrasi ke `UTC` dengan presisi tinggi.

## 3. The "1-Action" Workflow for Checkpoints
Ketika Guard melakukan submit laporan ("Aman" atau "Ada Kasus"), .NET Backend menggunakan satu endpoint tunggal (`/api/patrol-session/checkpoint-action`):

Mobile App mengirim:
```json
{
  "patrolCheckpointLogId": "GUID",
  "patrolAreaId": "GUID",
  "patrolCheckpointStatus": 1, // 1 = Cleared (Aman) | 2 = HasCase
  "securityNote": "Aman terkendali",
  "caseDetails": null // Diisi Object jika PatrolCheckpointStatus = 2
}
```

*   Jika **1 (Cleared)** `=>` Backend men-set log menjadi `Status = 1` & `<ClearedAt>`.
*   Jika **2 (HasCase)** `=>` Backend men-set log `Status = 2` & `<ClearedAt>`, kemudian secara otomatis menggunakan payload `caseDetails` untuk menetaskan Tiket Case baru yang diikat paksa lokasinya (Auto Assignment) berdasarkan lokasi `PatrolAreaId` di checkpoint tersebut. Tidak butuh logic Case Standalone manual.

## 4. Mekanisme Missed Checkpoint (Terlewat)
Saat sesi patrol dihentikan (`/api/patrol-session/{id}/stop`), .NET Backend akan mencari semua sisa Checkpoint di database yang memiliki `Status == 0 (AutoDetected)` alias *belum pernah disentuh satpam*.

Sisa Checkpoint tersebut akan otomatis diblokir dengan *stempel akhir* berstatus **`3 = Missed`**.

---

**Prioritas Utama untuk Node.js Developer:** 
Tolong optimasikan algoritma deteksi `ArrivedAt` menjadi *real-time* dengan latensi terendah, dan gunakan buffer (e.g. 5-10 detik) saat mendeteksi `LeftAt` untuk mencegah Bug akibat koneksi *flapping* yang langsung membatalkan absensi di checkpoint.
