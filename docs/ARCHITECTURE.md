# HireKarlo Architecture & Technical Design

## System Overview

```
┌─────────────────────────────────────────────────────┐
│          USER INTERFACES                            │
│    Web (Blazor) | VS Code | Browser Extensions     │
└────────────────────┬────────────────────────────────┘
					 │ (gRPC / REST)
		┌────────────▼────────────┐
		│   API Gateway & Auth    │
		│   (ASP.NET Core 9)      │
		│   JWT / OAuth2          │
		└────────────┬────────────┘
					 │
	┌────────────────┼────────────────────┐
	│                │                    │
	▼                ▼                    ▼
┌──────────┐   ┌─────────────┐   ┌─────────────────┐
│ Diagnosis│   │  Opportunity│   │  Referral      │
│ Engine   │   │  Matching   │   │  Intelligence  │
└──┬───────┘   └──┬──────────┘   └────┬────────────┘
   │              │                   │
   └──────────────┼───────────────────┘
				  │
		┌─────────▼──────────┐
		│  Service Layer     │
		│  (Dream Companies, │
		│   Interviews,      │
		│   Skill ROI)       │
		└─────────┬──────────┘
				  │
	┌─────────────┼──────────────┐
	│             │              │
	▼             ▼              ▼
┌──────────┐ ┌──────────┐ ┌──────────────┐
│PostgreSQL│ │  Redis   │ │  File Store  │
│+pgvector │ │ (Cache)  │ │ (Resumes)    │
└──────────┘ └──────────┘ └──────────────┘
```

---

## Core Modules

### 1. Resume Service (`HireKarlo.ResumeService`)
**Purpose**: Parse and extract resume data

**Capabilities**:
- PDF/DOCX parsing (DocumentFormat.OpenXml)
- LLM-based skill extraction (Groq/Azure OpenAI)
- Experience level detection
- Vector embedding generation

**Database Tables**:
- `Resumes` (id, user_id, filename, text, uploaded_at)
- `Skills` (id, resume_id, skill_name, years, embedding)

---

### 2. ATS Engine (`HireKarlo.AtsEngine`)
**Purpose**: Match jobs to resumes and score

**Algorithms**:
- TF-IDF similarity scoring
- Skill gap analysis
- Experience level matching
- Semantic search (pgvector)

**Output**: Match score (0-100%), confidence %, gap analysis

---

### 3. Application Layer (`HireKarlo.Application`)
**Purpose**: Business logic orchestration

**Key Services**:
- `IOpportunityRadarService` — Daily dashboard ranking
- `IDreamCompanyIntelligenceService` — Target company analysis
- `IReferralIntelligenceService` — Referral discovery
- `IInterviewDigestService` — Interview prep aggregation
- `ISkillROIService` — Skill learning path optimization

**Repository Pattern**:
```csharp
IUnitOfWork
├─ IOpportunityMatchRepository
├─ IDreamCompanyMatchRepository
├─ IReferralTargetRepository
├─ ISkillGapRecommendationRepository
└─ ICareerProgressRepository
```

---

### 4. API Layer (`HireKarlo.Api`)
**Purpose**: REST endpoint exposure

**Endpoints**:

```
POST   /api/v1/auth/login
POST   /api/v1/auth/register
POST   /api/v1/resumes (upload resume)
GET    /api/v1/resumes/{id}
DELETE /api/v1/resumes/{id}

GET    /api/v1/opportunities (daily radar)
GET    /api/v1/opportunities/{id}
POST   /api/v1/opportunities/{id}/apply

GET    /api/v1/dream-companies
POST   /api/v1/dream-companies
GET    /api/v1/dream-companies/{id}/intelligence

GET    /api/v1/referrals
POST   /api/v1/referrals/score
GET    /api/v1/referrals/{id}/outreach

GET    /api/v1/interviews/{companyId}/prep
POST   /api/v1/interviews/{id}/progress
```

---

### 5. Web App (`HireKarlo.Web` + `HireKarlo.Web.Client`)
**Purpose**: Blazor WebAssembly frontend

**Key Pages**:
- Login / Register
- Upload Resume
- Opportunity Radar Dashboard
- Dream Company Analysis
- Referral Outreach
- Interview Prep
- Settings / Profile

**Components**:
- `OpportunityCard.razor` — Individual job card with match %
- `DashboardLayout.razor` — Main dashboard grid
- `ReferralsList.razor` — Referral discovery UI
- `InterviewPrepPanel.razor` — Company interview guide

---

## Data Models

### Resume
```csharp
class Resume {
	int Id,
	string UserId,
	string OriginalFilename,
	string ParsedText,
	Dictionary<string, float[]> SkillEmbeddings,
	DateTime CreatedAt
}
```

### Opportunity
```csharp
class Opportunity {
	int Id,
	string JobBoardId,
	string Title,
	string Company,
	string Description,
	int MatchScore (0-100),
	Dictionary<string, int> SkillGaps,
	bool IsApplied,
	DateTime CreatedAt
}
```

### DreamCompany
```csharp
class DreamCompany {
	int Id,
	string UserId,
	string Name,
	List<string> TargetRoles,
	int MatchScore,
	List<LearningPath> GapFills,
	List<Referral> Referrals,
	DateTime CreatedAt
}
```

---

## Deployment Architecture

### Development
- **OS**: Windows/Mac/Linux
- **Runtime**: .NET 9 SDK
- **Database**: Local PostgreSQL + pgvector extension
- **Cache**: Local Redis

### Staging
- **Host**: Render (free tier)
- **Database**: Neon PostgreSQL (free)
- **Cache**: Render Redis (free)
- **API**: `https://hirekarlo-api-staging.onrender.com`

### Production
- **Host**: Azure / Render (paid tier)
- **Database**: PostgreSQL 15+ with pgvector
- **Cache**: Redis 7+
- **CDN**: Cloudflare

---

## Infrastructure as Code

### Docker
See `Dockerfile` and `docker-compose.yml` in repo root.

```bash
docker-compose up -d
# Starts API, Web, PostgreSQL, Redis
```

### GitHub Actions
`.github/workflows/release.yml` — Tag-triggered multi-platform publish:
- Docker → Docker Hub / GitHub Container Registry
- NPM → npm registry
- NuGet → nuget.org
- Extensions → VS Code, Chrome, Firefox marketplaces

---

## Performance & Scalability

### Current Bottlenecks
- Vector search with 100k+ resumes (solved via pgvector index)
- LLM inference latency (solved via cached embeddings + Redis)
- Job board scraping rate limits (solved via scheduled batch jobs)

### Optimization Roadmap
- [ ] Implement Redis caching layer for dashboard (v2.1)
- [ ] Add ElasticSearch for full-text search (v2.2)
- [ ] Horizontal API scaling with load balancer (v3.0)
- [ ] Database read replicas for reporting (v3.1)

---

## Testing Strategy

| Layer | Framework | Target Coverage |
|-------|-----------|-----------------|
| Unit | xUnit | 80%+ |
| Integration | xUnit + Testcontainers | 60%+ |
| E2E | Playwright (future) | 40%+ |
| Load | k6 (future) | N/A |

**Run tests**:
```bash
dotnet test
# or specific project
dotnet test tests/HireKarlo.Application.Tests
```

---

## Security

- **Auth**: JWT tokens with 24h expiry
- **Database**: Encrypted connection strings (via GitHub Secrets)
- **Secrets**: Environment variables (no hardcoded values)
- **HTTPS**: Enforced in production
- **CORS**: Limited to known domains
- **Rate Limiting**: 100 req/min per IP (configurable)

---

## Monitoring & Logging

| Tool | Purpose |
|------|---------|
| Application Insights | Error tracking, performance |
| Serilog | Structured logging |
| OpenTelemetry | Tracing (future) |
| Prometheus | Metrics (future) |

**View logs**:
```bash
# Render
render logs --service hirekarlo-api

# Azure
az webapp log tail --resource-group HireKarlo --name hirekarlo-api
```

---

## Reference Documentation

- **.NET 9**: [Official Docs](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9)
- **Blazor**: [Official Docs](https://learn.microsoft.com/en-us/aspnet/core/blazor)
- **Entity Framework Core**: [Official Docs](https://learn.microsoft.com/en-us/ef/core/)
- **PostgreSQL + pgvector**: [pgvector Docs](https://github.com/pgvector/pgvector)
- **Groq API**: [API Docs](https://console.groq.com/docs)
- **Azure OpenAI**: [Service Docs](https://learn.microsoft.com/en-us/azure/cognitive-services/openai/)

---

**Last updated**: Phase 2 (v2.0.0-dev)
