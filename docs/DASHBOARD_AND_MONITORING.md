# Dashboard Tools & Monitoring

## 🎛️ Administration Dashboard

### Features
- **User Management**: View active users, registrations, retention metrics
- **Opportunity Metrics**: Jobs scraped/day, match distribution, top companies
- **Performance**: API response times, database queries, cache hit rate
- **AI Usage**: Groq API calls, HuggingFace embeddings, cost tracking
- **System Health**: Error logs, deployment status, disk usage

### Access
```
Admin URL: https://hirekarlo-api.onrender.com/admin
```

---

## 📊 Analytics Dashboard (Blazor)

### User Dashboard
```
Dashboard.razor
├── Opportunity Card
│   ├── Today's Jobs: 12 new
│   ├── Top Match: 98% - Senior Engineer @ Google
│   └── Action: [View Full List] [Apply]
├── Match Trends Chart
│   ├── 7-day average match score
│   └── Skill improvement graph
├── Referral Pipeline
│   ├── Contacted: 5
│   ├── Replied: 2
│   └── Interviews: 1
└── Skill Gaps
	├── Missing: Kubernetes, K8s best practices
	├── Estimated learning time: 2 weeks
	└── Impact: +5% average match
```

### Recruiter Dashboard (Future)
```
RecruiterDashboard.razor
├── Candidate Matches
│   ├── Filter by role, location, skills
│   ├── View HireKarlo match % for each
│   └── Export matched candidates
├── Job Performance
│   ├── Applications per posting
│   ├── Quality (using HireKarlo data)
│   └── Time-to-hire
└── Analytics
	├── Best job board sources
	├── Skill trends
	└── Salary insights
```

---

## 🔍 Monitoring & Observability

### **Logs** (Structured Logging)
```csharp
_logger.LogInformation("Opportunity matched", new {
	UserId = userId,
	OpportunityId = jobId,
	MatchScore = 87.5,
	Timestamp = DateTime.UtcNow
});
```

Logs go to:
- Console (local development)
- File (docker logs)
- Sentry (production errors)

### **Metrics** (Prometheus-ready)
```
# Metrics endpoint: https://hirekarlo-api.onrender.com/metrics

hirekarlo_opportunities_total{source="linkedin"} 4523
hirekarlo_matches_per_user{quantile="0.5"} 15
hirekarlo_api_request_duration_seconds_bucket{endpoint="/api/opportunities", le="0.1"} 1250
hirekarlo_groq_api_calls_total{model="llama-3.3"} 45230
hirekarlo_database_query_duration_seconds{operation="skill_match"} 0.045
```

### **Profiling** (Application Insights / Open Telemetry)
```csharp
using var activity = new Activity("CalculateMatchScore");
activity.Start();
try {
	// Match calculation
} finally {
	activity.Stop();
}
```

---

## 📈 Key Performance Indicators (KPIs)

| KPI | Target | Current | Tool |
|-----|--------|---------|------|
| API Response Time (p95) | < 200ms | 145ms | App Insights |
| Daily Opportunities Scraped | 1000+ | 850 | Prometheus |
| Average Match Score | 50-70% | 62% | Dashboard |
| User Retention (7-day) | > 40% | 35% | SQL Query |
| Error Rate | < 0.5% | 0.2% | Sentry |
| Groq API Cost/month | < $10 | $3.20 | Groq Dashboard |

---

## 🛠️ Developer Tools

### **Swagger UI**
```
https://hirekarlo-api.onrender.com/swagger
```

Available endpoints:
- `POST /api/resumes` — Upload and parse resume
- `GET /api/opportunities` — List opportunities for user
- `POST /api/matches/calculate` — Calculate match score
- `GET /api/referrals` — Find referrals at company
- `GET /api/admin/stats` — System statistics

### **Database Viewer** (pgAdmin)
```
URL: https://pgadmin.hirekarlo.onrender.com
Login: admin@hirekarlo.com / (see .env)
```

Query examples:
```sql
-- Top matching opportunities
SELECT o.title, o.company, COUNT(*) as match_count
FROM opportunities o
JOIN job_matches jm ON o.id = jm.opportunity_id
WHERE jm.match_score > 80
GROUP BY o.id ORDER BY match_count DESC;

-- User engagement
SELECT u.id, COUNT(jm.id) as opportunities_matched, 
	   COUNT(DISTINCT oi.id) as interactions
FROM users u
LEFT JOIN job_matches jm ON u.id = jm.user_id
LEFT JOIN opportunity_interactions oi ON u.id = oi.user_id
GROUP BY u.id ORDER BY interactions DESC;
```

### **Redis Commander** (Cache Monitoring)
```
URL: http://localhost:6379 (local)
Commands:
- KEYS hirekarlo:* — List all cache keys
- GET hirekarlo:opportunities:user:{id} — Cached opportunities
- TTL hirekarlo:match:* — Cache expiration
```

---

## 📱 Mobile Dashboard (Future)

Using Blazor Hybrid:
```csharp
// MobileApp.razor
<MobileOpportunityCard Opportunity="opp" />
<MobileMatchChart Data="matchTrends" />
<NotificationBell AlertCount="@alerts.Count()" />
```

Deploy as:
- iOS app (via MAUI)
- Android app (via MAUI)
- PWA (current web)

---

## 🔐 Security & Compliance Monitoring

- API Key Rotation: Every 30 days
- Database Encryption: At rest (Render managed)
- SSL/TLS: All endpoints (automatic via Render)
- GDPR Compliance: User data export, deletion
- Rate Limiting: 100 req/min per user (prevent abuse)

---

**Next**: Implement real-time notifications when high-match opportunities appear! 🚀
