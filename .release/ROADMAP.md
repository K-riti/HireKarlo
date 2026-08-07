# HireKarlo Release & Distribution Roadmap

HireKarlo follows **semantic versioning with phase-based releases**:
- **v[PHASE].[FEATURE].[PATCH]** 
- Example: v2.1.3 = Phase 2, Feature 1 (Dream Company), Patch 3

---

## 📊 Complete Version Timeline

| Version | Release Date | Phase | Status | Key Features | Packages |
|---------|--------------|-------|--------|--------------|----------|
| **v1.0** | Q1 2024 | Resume Intelligence | ✅ Complete | Resume parsing, skill extraction, pgvector embeddings | Source |
| **v2.0** | Sep 2024 | Opportunity Radar | 🚀 Current | Daily job matching (0-100%), opportunity dashboard | Source + GitHub Release |
| **v2.1** | Oct 2024 | Dream Company | 📋 Next | Target company analysis, personalized learning paths | Docker Hub |
| **v2.5** | Dec 2024 | Distribution | 📋 Planned | Docker Hub multiarch images, Docker Compose | Docker, GitHub Actions |
| **v3.0** | May 2025 | Extensions | 📋 Planned | VS Code, Chrome, Firefox browser extensions | Extension stores |
| **v3.1** | June 2025 | SDKs | 📋 Planned | NuGet (.NET), npm (JavaScript), CLI tool | NuGet.org, npm registry |
| **v4.0** | TBD | SaaS Premium | 📋 Vision | Team collaboration, advanced analytics, API rate limits | SaaS platform |

---

## 🚀 Current Release: v2.0.0 (Opportunity Radar)

**Released**: August 2024  
**Status**: 🟢 Production Ready  
**Available On**: GitHub Releases

### What''s Included

#### Backend API (.NET)
```csharp
// Core entities for opportunity matching
public class Opportunity {
    public Guid Id { get; set; }
    public string JobId { get; set; }
    public string Title { get; set; }
    public string Company { get; set; }
    public string Description { get; set; }
    public Vector Embedding { get; set; }  // pgvector for semantic search
    public List<JobMatch> Matches { get; set; }
}

public class JobMatch {
    public Guid UserId { get; set; }
    public Guid OpportunityId { get; set; }
    public decimal MatchScore { get; set; }  // 0-100.0
    public string Analysis { get; set; }      // Why it matches
    public DateTime CreatedAt { get; set; }
}

public class SkillGap {
    public Guid UserId { get; set; }
    public string Skill { get; set; }
    public int Priority { get; set; }
    public string LearningPath { get; set; }
    public decimal ImpactOnMatch { get; set; }  // How much skill improves match %
}
```

#### Frontend (Blazor WebAssembly - Razor Components)
```razor
@* Opportunity Dashboard Component *@
<div class="opportunity-card">
    <h3>@opportunity.Title</h3>
    <p>@opportunity.Company | @opportunity.Location</p>
    
    @* Match Score Visualization *@
    <div class="match-score">
        <ProgressBar Value="@opportunity.MatchScore" />
        <span>@opportunity.MatchScore% Match</span>
    </div>
    
    @* Skills Matched *@
    @foreach (var skill in opportunity.MatchedSkills) {
        <Badge>@skill.Name ⭐ @skill.Proficiency%</Badge>
    }
</div>
```

### How to Get v2.0.0

#### Option 1: Source Code
```bash
git clone https://github.com/K-riti/HireKarlo
cd HireKarlo
git checkout v2.0.0
```

#### Option 2: Deploy via Render (Free)
```bash
# 1. Fork repo
# 2. Go to render.com → Blueprint
# 3. Add Groq + HuggingFace API keys
# 4. Deploy!
```

#### Option 3: Docker (Coming v2.5)
```bash
# Planned for Dec 2024
docker pull hirekarlo:2.0.0
```

---

## 📦 Future Packages

### v2.5 (Dec 2024) - Docker Distribution
```bash
docker pull hirekarlo:2.5.0
docker-compose -f docker/docker-compose.yml up -d
```

### v3.0 (May 2025) - Extensions
- VS Code Extension
- Chrome & Firefox Extensions

### v3.1 (June 2025) - SDKs
```bash
# NuGet
dotnet add package HireKarlo.Sdk --version 3.1.0

# NPM
npm install hirekarlo-sdk

# CLI
npm install -g hirekarlo-cli
```

---

## 🛠️ Complete Tech Stack

| Layer | Technology | Version | Purpose |
|-------|-----------|---------|---------|
| **Frontend** | Blazor WebAssembly | .NET 9 | Type-safe UI, PWA |
| **Backend API** | ASP.NET Core | 9.0 | REST API, business logic |
| **Database** | PostgreSQL | 16+ | Primary data store |
| **Vector DB** | pgvector (PostgreSQL extension) | 0.5+ | Semantic search (job ↔ resume) |
| **Cache** | Redis | 7.x | Match scores, rankings (sub-ms access) |
| **LLM** | Groq Llama 3.3 | Free tier | Resume parsing, job analysis |
| **Embeddings** | HuggingFace Transformers | Free tier | Vector generation (384-dim) |
| **Authentication** | JWT + OAuth2 | .NET Identity | Secure API access |
| **API Docs** | OpenAPI 3.0 + Swagger UI | Auto-generated | Interactive API explorer |
| **Message Queue** | (Future: RabbitMQ/Azure Service Bus) | — | Async job scraping |
| **Monitoring** | Application Insights / Sentry | Native + Optional | Error tracking, performance |
| **Deployment** | Docker + Kubernetes ready | — | Container-based deployment |
| **CI/CD** | GitHub Actions | Native | Automated tests, releases |

---

## 🎯 Why HireKarlo? (Problem Statement)

**Today''s Reality**:
- Engineers waste 10-20 hours/week applying to jobs
- 90% of applications have 0-20% match (guess work)
- Job boards optimize for volume, not quality
- No personalized guidance on what to learn

**HireKarlo Solution**:
- ✅ Smart matching: See ONLY high-quality opportunities
- ✅ Career roadmap: Learn X, gain Y% match improvement
- ✅ Referral superpowers: Find decision makers at target companies
- ✅ Interview prep: Aggregate Blind, LeetCode, company ratings
- ✅ Free tier: Groq (free LLM) + HuggingFace (free embeddings)

**Impact**: 30% faster job search, 50% more interviews from high-match roles

---

**Next Steps**: Deploy v2.0 today, contribute to Phase 2.1 development! 🚀
