# Analytics Enhancement Proposals
**Proposal Date:** 2025-02-08
**Service:** Analytics (User Journey)
**Status:** Ready for Review

---

## 📊 Executive Summary

Dengan adanya User Journey & Security Check yang sudah diimplementasikan, berikut adalah proposal fitur analytics tambahan yang akan memberikan nilai bisnis yang signifikan.

---

## 🎯 Top Priority (Immediate Business Value)

### 1. Area Dwell Time Analysis ⭐⭐⭐⭐⭐

**Tujuan:** Menganalisis berapa lama orang tinggal di setiap area

**Use Cases:**
- Identifikasi bottleneck (area dengan dwell time terlalu lama)
- Optimasi layout (area sepi vs ramai)
- Planning kapasitas area
- Deteksi anomali (orang tinggal terlalu lama di area sensitif)

**API Endpoint:**
```
POST /api/Analytics/dwell-time
Request: { from, to, areaId, buildingId, floorId }
Response: {
  areaName: string,
  avgDwellTimeMinutes: number,
  minDwellTimeMinutes: number,
  maxDwellTimeMinutes: number,
  medianDwellTimeMinutes: number,
  percentiles: { p25, p50, p75, p90, p95 },
  visitorCount: number,
  memberCount: number,
  hourlyDistribution: [ { hour, avgTime } ]
}
```

**Database Query:**
```sql
-- Group by area, calculate session durations
SELECT
    ma.name AS area_name,
    AVG(DATEDIFF(minute, MIN(t.trans_time), MAX(t.trans_time))) AS avg_dwell_minutes,
    COUNT(DISTINCT COALESCE(v.id, m.id)) AS person_count
FROM tracking_transaction_YYYYMMDD t
JOIN floorplan_masked_area ma ON t.floorplan_masked_area_id = ma.id
LEFT JOIN card c ON t.card_id = c.id
LEFT JOIN visitor v ON c.visitor_id = v.id
LEFT JOIN mst_member m ON c.member_id = m.id
WHERE t.trans_time BETWEEN @from AND @to
GROUP BY ma.id, ma.name
ORDER BY avg_dwell_minutes DESC;
```

**Complexity:** Medium
**Estimation:** 2-3 hari

---

### 2. Peak Hours Analysis ⭐⭐⭐⭐⭐

**Tujuan:** Mengetahui jam-jam tersibuk untuk setiap area

**Use Cases:**
- Resource planning (security guard scheduling)
- HVAC optimization (adjust based on occupancy)
- Cafe/restock scheduling
- Meeting room optimization

**API Endpoint:**
```
POST /api/Analytics/peak-hours
Request: { from, to, areaId, buildingId, granularity: 'hour'|'dayOfWeek' }
Response: {
  areaName: string,
  peakData: [
    { period: "09:00", visitorCount: 45, memberCount: 120, total: 165 },
    { period: "10:00", visitorCount: 52, memberCount: 115, total: 167 },
    ...
  ],
  quietHours: ["06:00", "07:00", "22:00"],
  busiestHour: { period: "14:00", count: 245 }
}
```

**Visualization Idea:** Heatmap calendar (hour of day vs day of week)

**Complexity:** Low
**Estimation:** 1-2 hari

---

### 3. Visitor vs Member Behavior Comparison ⭐⭐⭐⭐⭐

**Tujuan:** Membandingkan pola perjalanan visitor vs member

**Use Cases:**
- Security planning (visitor yang menyimpang dari rute normal)
- Facility optimization (area mana yang sering digunakan member)
- Understanding user journey differences

**API Endpoint:**
```
POST /api/Analytics/visitor-member-comparison
Request: { from, to, buildingId }
Response: {
  commonPaths: {
    visitorOnly: [ { path, count } ],
    memberOnly: [ { path, count } ],
    both: [ { path, visitorCount, memberCount, ratio } ]
  },
  areaUsage: {
    visitorTopAreas: [ { areaName, visitorCount } ],
    memberTopAreas: [ { areaName, memberCount } ],
    overlapAreas: [ { areaName, visitorCount, memberCount } ]
  },
  dwellTimeComparison: {
    visitorAvg: number,
    memberAvg: number,
    difference: number
  }
}
```

**Complexity:** Medium
**Estimation:** 2-3 hari

---

## 🔥 High Priority (Security & Safety)

### 4. Real-Time Occupancy Monitoring ⭐⭐⭐⭐

**Tujuan:** Menampilkan jumlah orang di setiap area secara real-time

**Use Cases:**
- Overcrowding detection (social distancing)
- Emergency response (know where people are)
- Space utilization tracking
- Meeting room availability

**API Endpoint:**
```
GET /api/Analytics/real-time-occupancy
Query: ?buildingId={id}
Response: {
  timestamp: datetime,
  areas: [
    {
      areaId: guid,
      areaName: string,
      currentOccupancy: number,
      capacity: number,
      occupancyPercentage: number,
      status: "Normal" | "Crowded" | "Over Capacity",
      people: [ { personId, type, name, durationMinutes } ]
    }
  ],
  summary: {
    totalPeople: number,
    totalVisitors: number,
    totalMembers: number
  }
}
```

**Background Service:** SignalR hub untuk push updates real-time

**Complexity:** High (requires SignalR/WebSocket)
**Estimation:** 3-4 hari

---

### 5. Security Incident Dashboard ⭐⭐⭐⭐

**Tujuan:** Dashboard semua pelanggaran security zone

**Use Cases:**
- Security team monitoring
- Audit trail
- Pattern analysis (area mana yang sering dilanggar)
- Investigasi incident

**API Endpoint:**
```
POST /api/Analytics/security-incidents
Request: { from, to, buildingId, severity: 'All'|'High'|'Critical' }
Response: {
  incidents: [
    {
      id: guid,
      timestamp: datetime,
      personType: "Visitor" | "Member",
      personName: string,
      violationType: string,
      areaName: string,
      fromZone: string,
      toZone: string,
      severity: "High" | "Critical",
      resolved: boolean
    }
  ],
  summary: {
    totalIncidents: number,
    bySeverity: { high: number, critical: number },
    byArea: [ { areaName, count } ],
    byViolationType: [ { type, count } ]
  }
}
```

**DataTable Integration:** Paginated, sortable, filterable

**Complexity:** Medium
**Estimation:** 2-3 hari

---

### 6. Anomalous Behavior Detection ⭐⭐⭐⭐

**Tujuan:** Deteksi perilaku yang tidak wajar secara otomatis

**Use Cases:**
- Security threat detection
- Theft prevention (linggar terlalu lama di area valuables)
- Health monitoring (orang tidak bergerak lama)
- Tailgating detection (multiple people using same card)

**API Endpoint:**
```
POST /api/Analytics/anomalies
Request: { from, to, type: 'All'|'LongDwell'|'UnusualPath'|'AfterHours' }
Response: {
  anomalies: [
    {
      id: guid,
      timestamp: datetime,
      personId: guid,
      personName: string,
      anomalyType: string,
      description: string,
      severity: "Low" | "Medium" | "High",
      details: { ... },
      confidence: number
    }
  ]
}
```

**Anomaly Types:**
1. **Long Dwell:** Tinggal > 2x median di area tertentu
2. **Unusual Path:** Path yang jarang terjadi (< 5%)
3. **After Hours:** Aktivitas di area sensitif di luar jam kerja
4. **Rapid Movement:** Berpindah area terlalu cepat (mungkin tailgating)
5. **Repeated Access:** Bolak-balik ke area yang sama dalam waktu singkat

**Complexity:** High (machine learning/statistical analysis)
**Estimation:** 4-5 hari

---

## 📈 Medium Priority (Business Intelligence)

### 7. Journey Flow Diagram (Sankey) ⭐⭐⭐⭐

**Tujuan:** Visualisasi aliran pergerakan orang antar area

**Use Cases:**
- Understanding traffic patterns
- Identifying bottleneck paths
- Wayfinding optimization
- Marketing (people flow to promotional areas)

**API Endpoint:**
```
POST /api/Analytics/journey-flow
Request: { from, to, buildingId, minFlow: 5 }
Response: {
  nodes: [
    { id: guid, name: "Lobby", type: "Public" },
    { id: guid, name: "Banking", type: "Secure" }
  ],
  links: [
    { source: guid, target: guid, value: 45, percentage: 23.5 }
  ]
}
```

**Frontend Library:** D3.js Sankey Diagram atau ECharts

**Complexity:** Medium
**Estimation:** 2-3 hari

---

### 8. Area Popularity Ranking ⭐⭐⭐

**Tujuan:** Ranking area berdasarkan berbagai metrik

**Use Cases:**
- Lease planning (area premium untuk disewakan)
- Advertising placement
- Facility maintenance prioritization
- Expansion planning

**API Endpoint:**
```
POST /api/Analytics/area-popularity
Request: { from, to, metric: 'visits'|'dwellTime'|'uniqueVisitors' }
Response: {
  rankings: [
    {
      rank: 1,
      areaName: string,
      buildingName: string,
      floorName: string,
      totalVisits: number,
      uniqueVisitors: number,
      avgDwellTime: number,
      popularityScore: number,
      trend: "↑" | "↓" | "→"
    }
  ]
}
```

**Complexity:** Low
**Estimation:** 1-2 hari

---

### 9. Time-to-Destination Analysis ⭐⭐⭐

**Tujuan:** Berapa waktu rata-rata untuk mencapai area tertentu

**Use Cases:**
- Wayfinding kiosks (estimated time)
- Meeting planning (buffer time)
- Event scheduling
- Elevator/stairs optimization

**API Endpoint:**
```
POST /api/Analytics/time-to-destination
Request: { from, to, originAreaId, destinationAreaId }
Response: {
  originArea: string,
  destinationArea: string,
  avgTimeMinutes: number,
  medianTimeMinutes: number,
  percentiles: { p50, p75, p90, p95 },
  commonPaths: [ { path, count, avgTime } ]
}
```

**Complexity:** Medium
**Estimation:** 2 hari

---

## 📊 Low Priority (Nice to Have)

### 10. Cohort Analysis ⭐⭐⭐

**Tujuan:** Analisis perilaku berdasarkan kelompok

**Use Cases:**
- New vs repeat visitor behavior
- Department-based movement patterns
- Organization-level analytics

**Complexity:** High
**Estimation:** 3-4 hari

---

### 11. A/B Testing for Layout Changes ⭐⭐

**Tujuan:** Compare area usage before/after layout changes

**Complexity:** Medium
**Estimation:** 2-3 hari

---

### 12. Predictive Analytics ⭐⭐

**Tujuan:** Forecast future occupancy based on historical patterns

**Use Cases:**
- Crowd prediction
- Resource planning
- HVAC scheduling

**Complexity:** Very High (time series forecasting)
**Estimation:** 5-7 hari

---

## 🛠️ Technical Recommendations

### Database Indexes

```sql
-- Critical indexes for analytics queries
CREATE INDEX IX_tracking_transaction_trans_time
  ON tracking_transaction_YYYYMMDD(trans_time);

CREATE INDEX IX_tracking_transaction_area_time
  ON tracking_transaction_YYYYMMDD(floorplan_masked_area_id, trans_time);

CREATE INDEX IX_tracking_transaction_card_time
  ON tracking_transaction_YYYYMMDD(card_id, trans_time);

-- Composite index for common queries
CREATE INDEX IX_tracking_transaction_area_card_time
  ON tracking_transaction_YYYYMMDD(floorplan_masked_area_id, card_id, trans_time);
```

### Materialized Views (for heavy queries)

```sql
-- Pre-aggregated daily stats
CREATE VIEW vw_daily_area_stats AS
SELECT
    CAST(t.trans_time AS DATE) as stat_date,
    t.floorplan_masked_area_id,
    ma.name as area_name,
    COUNT(DISTINCT t.card_id) as unique_cards,
    COUNT(*) as total_detections,
    AVG(t.battery) as avg_battery
FROM tracking_transaction_YYYYMMDD t
JOIN floorplan_masked_area ma ON t.floorplan_masked_area_id = ma.id
GROUP BY CAST(t.trans_time AS DATE), t.floorplan_masked_area_id, ma.name;
```

### Caching Strategy

```
- Common Paths: Cache 15 minutes (changes slowly)
- Peak Hours: Cache 1 hour (historical data)
- Real-time Occupancy: No cache (live data)
- Dwell Time: Cache 30 minutes
- Security Incidents: No cache (security critical)
```

---

## 📅 Implementation Roadmap

### Phase 1: Quick Wins (Week 1)
1. ✅ User Journey & Security Check (DONE)
2. Peak Hours Analysis (1-2 days)
3. Area Dwell Time Analysis (2-3 days)
4. Area Popularity Ranking (1-2 days)

**Total:** 4-7 days

### Phase 2: Security & Monitoring (Week 2-3)
5. Real-Time Occupancy Monitoring (3-4 days)
6. Security Incident Dashboard (2-3 days)
7. Anomalous Behavior Detection (4-5 days)

**Total:** 9-12 days

### Phase 3: Advanced Analytics (Week 4-5)
8. Visitor vs Member Comparison (2-3 days)
9. Journey Flow Diagram (2-3 days)
10. Time-to-Destination Analysis (2 days)

**Total:** 6-8 days

### Phase 4: Intelligence (Week 6+)
11. Cohort Analysis (3-4 days)
12. Predictive Analytics (5-7 days)

**Total:** 8-11 days

---

## 💡 Quick Implementation Template

Semua analytics endpoint mengikuti pattern ini:

```csharp
// 1. Create DTO
public class AreaDwellTimeRead { ... }

// 2. Repository method
public async Task<List<AreaDwellTimeRead>> GetDwellTimeAsync(filter) { ... }

// 3. Service method (direct return)
public async Task<List<AreaDwellTimeRead>> GetDwellTimeAsync(filter)
{
    return await _repository.GetDwellTimeAsync(filter);
}

// 4. Controller endpoint
[HttpPost("dwell-time")]
[MinLevel(LevelPriority.PrimaryAdmin)]
public async Task<IActionResult> GetDwellTime([FromBody] DwellTimeFilter filter)
{
    var result = await _service.GetDwellTimeAsync(filter);
    return ApiResponse.Success(result);
}
```

---

## 🎁 Bonus: Frontend Components

### DataTables.NET Integration

```javascript
// Common DataTable configuration for all analytics
$('.analytics-table').DataTable({
    processing: true,
    serverSide: true,
    ajax: {
        url: '/api/Analytics/dwell-time',
        type: 'POST',
        headers: { 'Authorization': 'Bearer ' + token }
    },
    columns: [
        { data: 'areaName', title: 'Area' },
        { data: 'avgDwellTimeMinutes', title: 'Avg Time (min)' },
        { data: 'visitorCount', title: 'Visitors' },
        { data: 'memberCount', title: 'Members' }
    ],
    order: [[1, 'desc']],
    dom: 'Bfrtip',
    buttons: ['excel', 'pdf', 'print']
});
```

---

## 📞 Next Steps

Diskusikan dengan stakeholder untuk memilih fitur mana yang:
1. Paling urgent (high business value)
2. Sesuai budget/time
3. Tersedia data yang cukup

**Rekomendasi Start:**
- **Quick Win:** Peak Hours + Dwell Time (mudah, high value)
- **Security Focus:** Security Incident Dashboard
- **Advanced:** Real-time Occupancy (requires SignalR setup)

---

## 📊 Summary Table

| Fitur | Priority | Complexity | Estimation | Business Value |
|-------|----------|------------|------------|----------------|
| Dwell Time Analysis | ⭐⭐⭐⭐⭐ | Medium | 2-3 hari | High |
| Peak Hours Analysis | ⭐⭐⭐⭐⭐ | Low | 1-2 hari | High |
| Visitor vs Member Comparison | ⭐⭐⭐⭐⭐ | Medium | 2-3 hari | High |
| Real-Time Occupancy | ⭐⭐⭐⭐ | High | 3-4 hari | High |
| Security Incident Dashboard | ⭐⭐⭐⭐ | Medium | 2-3 hari | High |
| Anomaly Detection | ⭐⭐⭐⭐ | High | 4-5 hari | Medium |
| Journey Flow Diagram | ⭐⭐⭐⭐ | Medium | 2-3 hari | Medium |
| Area Popularity Ranking | ⭐⭐⭐ | Low | 1-2 hari | Medium |
| Time-to-Destination | ⭐⭐⭐ | Medium | 2 hari | Medium |
| Cohort Analysis | ⭐⭐⭐ | High | 3-4 hari | Low |
| A/B Testing | ⭐⭐ | Medium | 2-3 hari | Low |
| Predictive Analytics | ⭐⭐ | Very High | 5-7 hari | Low |

**Total untuk semua fitur:** ~30-45 hari kerja

---

**End of Proposal**
