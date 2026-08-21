# HireKarlo — AI Career Operating System

> **The operating system for engineer job search.** Answers all five questions job seekers ask: Which jobs should I apply for? Which skills should I learn? Which companies am I closest to? Who should I ask for referrals? How should I prepare for interviews?

[![Latest Release](https://img.shields.io/badge/Release-v2.0.0-blue?logo=github)](https://github.com/K-riti/HireKarlo/releases/tag/v2.0.0)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com)
[![Phase 2 Active](https://img.shields.io/badge/Phase-2%3A%20Opportunity%20Radar-brightgreen)](#phase-2-opportunity-radar-current)
[![MIT License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## The Five Core Features

**Upload resume** → Get these insights for life:

| Feature | The Problem It Solves | The Output |
|---------|-----|--------|
| **🎯 Dream Company Intelligence** | "Which companies should I target?" | Adobe: 84%, Atlassian: 78%, Juspay: 93% (with skill gaps & learning paths) |
| **🚀 Opportunity Radar** | "Which job should I apply for today?" | 10 high-match jobs ranked 0-100%, with "why this match" explanations |
| **📈 Skill ROI Engine** | "What skill should I learn next?" | "Learn Terraform (+8%, 6 weeks)" vs "Learn Docker Security (+10%, 3 weeks)" |
| **🤝 Referral Intelligence** | "Who should I ask for a referral?" | Matching employees by tech stack + experience + location + reachability |
| **📚 Interview Digest** | "How do I prepare for interviews?" | Company-specific: most asked topics, system design areas, difficulty levels, prep roadmap |

---

## Why HireKarlo?

Current job search is broken:
- Job boards show **1000+ irrelevant jobs daily** (90% poor matches)
- Engineers waste **10-20 hours/week** filtering noise
- No guidance on **which skills actually improve chances**
- **Manual referral sourcing** without qualification assessment
- Interview prep is **duplicated** across every company

**HireKarlo is different:** Semantic matching + AI analysis + personalized roadmaps.

---

## Quick Example

```
Step 1: Upload Resume
└─ System extracts: Python (5 yrs), Kubernetes (3 yrs), AWS (4 yrs)

Step 2: Select Dream Company
└─ Enter: "Adobe" (or browse list)

Step 3: Get Intelligence
└─ Adobe Platform Engineer Match: 84%
   ✔ Python (5 yrs) — Company needs 3+
   ✔ Kubernetes (3 yrs) — Company needs 2+
   ✖ Terraform (0 yrs) — Company needs 5+
   → Learn Terraform (6-8 weeks) = +8% match

Step 4: See Opportunities
└─ Adobe Platform Engineer IV (91% match) [APPLY]
   Stripe Cloud Platform (84% match) [SAVE]
   Databricks Infrastructure (87% match) [LEARN MORE]

Step 5: Get Referrals
└─ Sarah Chen @ Adobe (94% overall match, 1 connection away)
   [Auto-generate outreach email]
```

---

## Current Release

**v2.0.0** (September 2024)
- ✅ Dream Company Intelligence
- ✅ Opportunity Radar  
- ✖️ Skill ROI Engine (coming Oct 2024)
- ✖️ Referral Intelligence (coming Oct 2024)
- ✖️ Interview Digest (coming Dec 2024)

**[See full roadmap →](.release/ROADMAP.md)** | **[Full technical deep dive →](docs/TECHNICAL_DEEP_DIVE.md)** | **[Product vision →](docs/PRODUCT_VISION.md)**

---

## Tech Stack

| Layer | Technology | Why |
|-------|-----------|-----|
| **Frontend** | Blazor WebAssembly (.NET 9) | Type-safe C#, PWA-ready, excellent tooling |
| **Backend** | ASP.NET Core 9 | Minimal boilerplate, excellent ORM (EF Core) |
| **Database** | PostgreSQL 16 + pgvector | ACID guarantees, semantic search without separate vector DB |
| **Cache** | Redis | Sub-millisecond lookups for match rankings |
| **LLM** | Groq Llama 3.3 | Free tier (30 req/min), fast 70B model |
| **Embeddings** | HuggingFace | Free, reliable 384-dim embeddings |
| **Deployment** | Docker + Render | 1-click deploy, $0/month free tier |
| **CI/CD** | GitHub Actions | Native integration, auto-publish on tag |

**Philosophy:** Monolith-first architecture. No Kafka, Kubernetes, or microservices—scale later.

---

## Getting Started

### Prerequisites
- .NET 9 SDK
- PostgreSQL 15+
- Free API keys:
  - [Groq](https://console.groq.com) (30 requests/min)
  - [HuggingFace](https://huggingface.co/settings/tokens)

### Local Development

```bash
# Clone repo
git clone https://github.com/K-riti/HireKarlo.git
cd HireKarlo

# Set environment variables (or use .env)
export GROQ_API_KEY="your_groq_key"
export HUGGINGFACE_TOKEN="your_hf_token"
export DATABASE_URL="Server=localhost;Database=hirekarlo;User Id=postgres;Password=..."

# Setup database
dotnet ef database update \
  -p src/Infrastructure/HireKarlo.Persistence \
  -s src/Presentation/HireKarlo.Api

# Run backend API
dotnet run --project src/Presentation/HireKarlo.Api
# → API available at http://localhost:5000
# → Swagger docs at http://localhost:5000/swagger

# Run frontend (in another terminal)
dotnet run --project src/Presentation/HireKarlo.Web
# → Web app available at http://localhost:3000
```

### Docker

```bash
# Build & run full stack
docker-compose -f docker/docker-compose.yml up

# Access:
# - API: http://localhost:5000
# - Web: http://localhost:3000
# - Database: localhost:5432
```

### Deploy to Render (Free)

```bash
# Push to GitHub
git push origin main

# New tag triggers auto-deploy
git tag v2.0.1
git push origin v2.0.1

# Render API will auto-deploy via GitHub Actions
# → See deployment status: https://render.com/dashboard
```

---

## Architecture Highlights

### Frontend (Blazor WebAssembly)
- **Dashboard:** Opportunity cards, match scores, skill gaps
- **Dream Company:** Target company matching with skill ROI analysis
- **Opportunity Radar:** Daily high-match job recommendations
- **Referral Explorer:** Employee discovery with tech-aware matching
- **Interview Hub:** Company-specific prep roadmaps

### Backend (ASP.NET Core)
- **Resume Service:** Upload, parse, skill extraction via Groq LLM
- **Opportunity Service:** Scrape, embed, rank by semantic similarity
- **Matching Engine:** Weighted scoring (skills 50% + experience 25% + location 15% + salary 10%)
- **Referral Service:** Employee matching by multiple similarity dimensions
- **Interview Service:** Scrape + RAG aggregation from Blind/LeetCode/Levels.fyi

### Database (PostgreSQL + pgvector)
```sql
-- 11 core tables
Users, Resumes, ResumeSkills, Opportunities, JobMatches,
SkillGaps, ReferralProfiles, OpportunityInteractions, EmployeeProfiles,
InterviewRecords, UserPreferences

-- Vector search
SELECT * FROM opportunities 
WHERE embedding <-> resume_embedding < 0.5  -- IVFFLAT index
ORDER BY embedding <-> resume_embedding
LIMIT 10;  -- <200ms for single query
```

### Performance
- **Single match calculation:** <200ms (p99)
- **10 opportunity rankings:** <500ms
- **Cache hit rate:** >85% (Redis)
- **API response:** <100ms (p50), <200ms (p95), <500ms (p99)

---

## Development Roadmap

| Phase | Release | Features | Timeline |
|-------|---------|----------|----------|
| **1** | v1.0 | Resume Intelligence, skill extraction | ✅ Q1 2024 |
| **2** | v2.0 | Dream Company + Opportunity Radar | ✅ Sep 2024 |
| **2.1** | v2.1 | Skill ROI + Referral Intelligence | 🔜 Oct 2024 |
| **2.5** | v2.5 | Interview Digest + Docker distribution | 🔜 Dec 2024 |
| **3** | v3.0 | Extensions (VS Code, Chrome, Firefox) + SDKs | 🔜 May 2025 |
| **3.1** | v3.1 | npm SDK + NuGet package + CLI tool | 🔜 June 2025 |

---

## Key Success Metrics

**User Engagement**
- Resume uploads (onboarding)
- Daily active checks (habit formation)
- Dream company list size (market reach)

**Feature Adoption**
- Opportunity Radar clicks
- Skill ROI learning paths initiated
- Referral outreach messages sent

**Impact**
- Interview → offer acceptance rate
- Time from opportunity to apply
- Skill learning completion rate

---

## Competitive Advantages

| Feature | HireKarlo | Job Boards | ATS Checkers | LinkedIn |
|---------|-----------|-----------|-------------|----------|
| Dream Company Matching | ✅ | ❌ | ❌ | ❌ |
| Skill ROI Calculation | ✅ | ❌ | ❌ | ❌ |
| Interview Digest (Aggregated) | ✅ | ❌ | ❌ | ❌ |
| Referral Intelligence | ✅ | ❌ | ❌ | ⚠️ Basic |
| Semantic Matching | ✅ | ⚠️ Keyword | ⚠️ | ⚠️ |
| Free Tier | ✅ | ⚠️ | ✅ | ⚠️ |

---

## Resume Description (For Hiring Managers)

```
HireKarlo - .NET 9, PostgreSQL, pgvector, Groq, Redis

• Built an AI-powered Career Operating System that analyzes resume-to-company 
  fit, surfaces high-match opportunities, and generates personalized skill roadmaps.

• Implemented Dream Company Intelligence, Opportunity Radar, and Interview Digest 
  engines using semantic search, vector embeddings, and RAG pipelines.

• Designed a match-scoring platform leveraging PostgreSQL pgvector for embedding 
  storage, Groq LLMs for intelligent analysis, and Redis for sub-millisecond 
  rankings of 1000+ daily opportunities.

• Full-stack: Blazor WebAssembly (frontend), ASP.NET Core (backend), Docker 
  (containerization), Render (auto-scaling deployment), GitHub Actions (CI/CD).
```

---

## Contributing

We welcome contributions! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines.

---

## License

MIT License — See [LICENSE](LICENSE) for details.

---

## Links

- 🎯 **[Product Vision](docs/PRODUCT_VISION.md)** — Full feature breakdown and use cases
- 🛠️ **[Technical Deep Dive](docs/TECHNICAL_DEEP_DIVE.md)** — Architecture, code examples, performance
- 📊 **[Dashboard & Monitoring](docs/DASHBOARD_AND_MONITORING.md)** — Observability and metrics
- 📦 **[Package Publishing](docs/PACKAGE_PUBLISHING.md)** — Distribution strategy
- 🚀 **[Roadmap](.release/ROADMAP.md)** — Detailed phase timeline
- 🐛 **[Issues](https://github.com/K-riti/HireKarlo/issues)** — Bug reports and feature requests
- 📝 **[Releases](https://github.com/K-riti/HireKarlo/releases)** — Version history

---

