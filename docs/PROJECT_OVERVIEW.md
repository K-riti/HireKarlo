# HireKarlo — Complete Project Overview

## 🎯 Why HireKarlo?

**Problem**: Engineers spend 10-20 hours/week on job search—browsing job boards, copy-pasting applications, negotiating salary with no data. Most job boards optimize for volume (quantity), not quality.

**Solution**: HireKarlo is an **AI Career Operating System** that replaces passive browsing with intelligent discovery:
- **Daily Opportunity Radar**: See only high-match jobs (0-100% relevance) instead of thousands
- **Smart Matching**: Analyze jobs against your resume using vector embeddings (pgvector) + LLM
- **Personalized Learning**: Skill gap analysis → learn X, gain Y% match improvement
- **Referral Intelligence**: Find employees at target companies, auto-generate outreach messages
- **Interview Prep**: Aggregate company-specific practice from Blind, LeetCode, Levels.fyi

Unlike job boards, HireKarlo **puts the engineer in control**: see why you match, what's missing, and decide to apply.

---

## 🏗️ Architecture & Tech Stack

### **Frontend Layer** — Blazor WebAssembly (.NET 9)
- **Why**: Type-safe C# in browser, PWA-capable, offline support
- **What**: Interactive dashboard, resume upload, opportunity browsing
- **Tech**: Blazor WebAssembly, SignalR (real-time notifications), MudBlazor (UI components)

### **API Layer** — ASP.NET Core 9 (REST + GraphQL ready)
- **Why**: Native .NET ecosystem, built-in dependency injection, OpenAPI/Swagger
- **What**: Job matching, opportunity ranking, referral scoring, user profiles
- **Tech**: ASP.NET Core 9, OpenAPI 3.0, Middleware (logging, auth, error handling)

### **Database Layer** — PostgreSQL 16 + pgvector
- **Why**: ACID compliance, native JSON, pgvector extension for semantic search
- **What**: Store resumes, opportunities, user preferences, interactions
- **Tables**: Users, Resumes, Opportunities, JobMatches, Referrals, InterviewPrep, SkillGaps
- **Indexing**: Full-text search on job descriptions, vector similarity on embeddings

### **AI/ML Layer** — Free Tier
- **LLM**: Groq API (Llama 3.3, 30 req/min free) — Resume parsing, skill extraction, job analysis
- **Embeddings**: HuggingFace (free) — Convert job descriptions + resume → vectors for similarity matching
- **RAG**: Retrieval-Augmented Generation for personalized recommendations

### **Caching & Performance** — Redis
- **Why**: Sub-millisecond access for opportunity rankings, match scores, user preferences
- **What**: Cache job boards daily, opportunity rankings, user embeddings

### **Infrastructure** — Docker + Render + GitHub Actions
- **Deployment**: Render.com (free tier) — API, web, database, Redis
- **CI/CD**: GitHub Actions — Build, test, publish on tag push
- **Monitoring**: Sentry (errors), LogRocket (frontend)

---

## 📋 Core Features (Current & Planned)

### ✅ **Phase 1: Resume Intelligence** (Complete v1.0)
- Upload resume (PDF/DOCX)
- LLM extracts skills, experience, education
- Store embeddings in pgvector
- REST API for resume management

### 🚀 **Phase 2: Opportunity Radar** (In Progress - v2.0)
- Daily job board scraping (LinkedEdin, Indeed, Levels.fyi, YC, etc.)
- Match scoring algorithm: `(Skill Match × 0.5) + (Experience × 0.3) + (Location × 0.2)`
- Opportunity dashboard with ranked list
- Dream company analysis + skill gaps
- Referral discovery (find employees at target companies)
- Interview prep aggregation (Blind discussions, LeetCode solutions)

### 📅 **Phase 3: Extensions** (May 2025)
- VS Code Extension — Browse jobs without leaving editor
- Chrome & Firefox Extensions — Job board enhancement
- Notifications for high-match opportunities

### 📦 **Phase 4: SDKs & Distribution** (June 2025)
- **NuGet SDK** (.NET) — Integrate HireKarlo into .NET apps
- **npm SDK** (JavaScript) — Use for web apps
- **CLI Tool** — Command-line job search
- **Docker Hub** — Pre-built container images

---

## 🛠️ Code Structure

```
src/
├── Core/
│   ├── HireKarlo.Domain/          # Entities (User, Resume, Opportunity, JobMatch)
│   ├── HireKarlo.Application/     # Services, DTOs, Repositories abstraction
│   └── HireKarlo.Crosscutting/    # Logging, exceptions, utilities
├── Infrastructure/
│   └── HireKarlo.Persistence/     # EF Core, database migrations, repositories
├── Presentation/
│   ├── HireKarlo.Api/             # ASP.NET Core REST API
│   └── HireKarlo.Web/             # Blazor WebAssembly frontend
└── Tests/
	├── HireKarlo.Application.Tests/
	└── HireKarlo.Api.Tests/
```

### **Key Entities**
```csharp
public class Resume : BaseEntity {
	public Guid UserId { get; set; }
	public string RawText { get; set; }
	public List<SkillEntity> Skills { get; set; }
	public Vector Embedding { get; set; }  // pgvector
}

public class Opportunity : BaseEntity {
	public string JobId { get; set; }
	public string Title { get; set; }
	public string Company { get; set; }
	public string Description { get; set; }
	public Vector Embedding { get; set; }  // pgvector
	public List<JobMatch> Matches { get; set; }
}

public class JobMatch : BaseEntity {
	public Guid UserId { get; set; }
	public Guid OpportunityId { get; set; }
	public decimal MatchScore { get; set; }  // 0-100
	public string Analysis { get; set; }     // Why it matches
}
```

---

## 📊 Deployment & Release Timeline

| Version | Release | Phase | Features | Packages |
|---------|---------|-------|----------|----------|
| **v1.0** | Q1 2024 | Resume | PDF parsing, skill extraction | — |
| **v2.0** | Sep 2024 | Radar (Current) | Job matching, dashboard, referrals | Source code |
| **v2.5** | Q4 2024 | Docker Hub | Container images | Docker, GitHub Releases |
| **v3.0** | May 2025 | Extensions | VS Code, Chrome, Firefox | Extension stores |
| **v3.1** | June 2025 | SDKs | NuGet, npm, CLI | NuGet, npm |
| **v4.0** | TBD | SaaS | Subscription model, analytics | — |

### **Current Status (v2.0)** 
- ✅ Core API built (Phase 1 + Phase 2 foundation)
- 🚀 Opportunity Radar in development
- ✅ Deployed to Render (free tier)
- 📦 Ready for NuGet/Docker distribution (Phase 3+)

---

## 🚀 How to Run

### Local Development
```bash
git clone https://github.com/K-riti/HireKarlo
cd HireKarlo

# Setup database
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api

# Run API
dotnet run --project src/Presentation/HireKarlo.Api
# Web: http://localhost:5000
# Swagger: http://localhost:5000/swagger

# Run tests
dotnet test HireKarlo.slnx
```

### Docker Deployment
```bash
docker-compose -f docker/docker-compose.yml up -d
```

### Deploy to Render
```bash
# 1. Fork repo
# 2. render.com → Blueprint
# 3. Add Groq + HuggingFace API keys
# 4. Deploy!
# Results:
# - API: https://hirekarlo-api.onrender.com
# - Web: https://hirekarlo-web.onrender.com
# - DB: Managed PostgreSQL
```

---

## 📈 Value Proposition

- **For Engineers**: Save 10+ hours/week on job search, 30% faster interviews (better prep)
- **For Recruiters**: Better candidate-job fit (using HireKarlo SDK)
- **For Communities**: Free tier (Groq + HuggingFace), giving engineers superpowers

---

**Built with ❤️ by engineers, for engineers.**
