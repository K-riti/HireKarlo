# HireKarlo: Complete Technical Deep Dive

## Executive Summary

HireKarlo is an **AI-powered Career Operating System** built with .NET 9 that solves the engineer job search problem through intelligent opportunity discovery, semantic matching, and personalized career guidance. Unlike traditional job boards that optimize for volume, HireKarlo uses machine learning to surface only high-quality opportunities (0-100% match scoring) alongside data-driven learning paths.

---

## Core Problem & Solution

### The Problem
Engineers waste **10-20 hours/week** browsing job boards with:
- **90% poor matches** (0-20% relevance to their skills)
- **1000+ irrelevant jobs daily** from generic aggregators
- **No guidance** on which skills to learn for better match rates
- **Manual referral sourcing** without qualification assessment
- **Duplicated interview prep** across companies

### HireKarlo's Solution
1. **Smart Matching Engine**: Analyze resume vs job descriptions using vector embeddings (pgvector) + semantic similarity search
2. **Daily Opportunity Radar**: Dashboard showing only high-match jobs (>60%) ranked 0-100%
3. **Personalized Learning Paths**: Calculate skill gaps, estimate learning time, predict match improvement
4. **Referral Intelligence**: Find qualified employees at target companies, auto-generate personalized outreach
5. **Interview Aggregation**: Collate company-specific prep from Blind, LeetCode, Levels.fyi using RAG

---

## Architecture Overview

### Frontend Layer: Blazor WebAssembly (.NET 9)

**Why Blazor?**
- Type-safe C# across browser and server (no TypeScript context switching)
- PWA capabilities (works offline, installable)
- Code reuse between frontend/backend
- Excellent tooling in Visual Studio

**Components:**
```csharp
// Opportunity Dashboard (Razor component)
@page "/dashboard"
@inject IOpportunityClient opportunityClient

<div class="opportunity-grid">
	@foreach (var opp in opportunities)
	{
		<OpportunityCard Opportunity="@opp" 
						OnApply="@(() => HandleApply(opp))" />
	}
</div>

// Match visualization
<MatchScoreChart MatchScore="@opp.MatchScore" 
				 MatchBreakdown="@opp.Analysis">
	Shows skill breakdown, why it matches, missing skills
</MatchScoreChart>
```

**Key Pages:**
- `/dashboard` - Daily opportunities ranked by match %
- `/matches/{jobId}` - Deep analysis (why matched, gaps, should apply?)
- `/skills/gaps` - Identified skill deficits with learning paths
- `/referrals/{companyId}` - Find employees, personalized messages
- `/interviews/{companyId}` - Aggregated interview prep

---

### Backend API: ASP.NET Core 9

**REST Endpoints (OpenAPI/Swagger documented):**

```csharp
// Resume Management
POST   /api/v1/resumes              // Upload PDF/DOCX
GET    /api/v1/resumes/{id}         // Get parsed resume
DELETE /api/v1/resumes/{id}         // Delete resume

// Job Opportunities
GET    /api/v1/opportunities        // Daily opportunities (paginated, sorted by match)
GET    /api/v1/opportunities/{id}   // Single opportunity details
POST   /api/v1/opportunities/analyze// Deep analysis for custom job description

// Match Scoring Algorithm
POST   /api/v1/matches/calculate    // Calculate match for arbitrary job
GET    /api/v1/matches/history      // User's match history

// Skill Management
GET    /api/v1/skills/gaps          // Identified gaps (ordered by impact)
POST   /api/v1/skills/learn/{skill} // Generate learning path (time, resources, impact)

// Referral Intelligence
GET    /api/v1/companies/{id}/referrals  // Find employees at company
POST   /api/v1/referrals/{id}/analyze    // Score person as referral
POST   /api/v1/referrals/{id}/message    // Generate personalized outreach

// Interview Prep
GET    /api/v1/interviews/{company} // Aggregate prep (Blind, LeetCode, Levels)

// User Profile
GET    /api/v1/users/me             // Current user details
POST   /api/v1/users/preferences    // Save preferences (target companies, etc)
```

**Core Service: Opportunity Matching**

```csharp
public class MatchService {
	// Algorithm: Weighted scoring across multiple dimensions
	public decimal CalculateMatch(Resume resume, Opportunity job) {
		var skillMatch = CalculateSkillOverlap(resume.Skills, job.RequiredSkills);
		var experienceMatch = CalculateExperienceLevel(resume.Years, job.LevelRequired);
		var locationMatch = CalculateLocation(resume.Location, job.Location);
		var compensationMatch = CalculateCompensation(resume.Expectations, job.Salary);

		// Weighted formula (tuned based on conversion data)
		return (skillMatch * 0.50) +      // Skills matter most
			   (experienceMatch * 0.25) +  // Experience level
			   (locationMatch * 0.15) +    // Flexibility to relocate
			   (compensationMatch * 0.10); // Salary alignment
	}

	// Uses pgvector for semantic similarity
	// Convert resume → 384-dim embedding
	// Convert job description → 384-dim embedding
	// Calculate cosine similarity (0-100%)
}
```

---

### Data Layer: PostgreSQL 16 + pgvector

**Schema (Core Tables):**

```sql
-- User with embedded resume data
CREATE TABLE Users (
	Id UUID PRIMARY KEY,
	Email VARCHAR unique,
	CreatedAt TIMESTAMP
);

-- Resume storage with vector embeddings
CREATE TABLE Resumes (
	Id UUID PRIMARY KEY,
	UserId UUID REFERENCES Users(Id),
	RawText TEXT,              -- Original PDF/DOCX text
	Embedding vector(384),      -- 384-dim sentence transformer embeddings
	ParsedAt TIMESTAMP,
	FOREIGN KEY (UserId) REFERENCES Users(Id)
);

-- Extracted skills (from LLM parsing)
CREATE TABLE ResumeSkills (
	Id UUID PRIMARY KEY,
	ResumeId UUID REFERENCES Resumes(Id),
	SkillName VARCHAR,
	Proficiency INT (0-100),   -- How well they know it
	YearsOfExperience INT
);

-- Job opportunities from scraped boards
CREATE TABLE Opportunities (
	Id UUID PRIMARY KEY,
	SourceJobId VARCHAR,       -- LinkedIn job ID, Indeed, etc
	Title VARCHAR,
	Company VARCHAR,
	Location VARCHAR,
	JobDescription TEXT,
	Embedding vector(384),     -- pgvector for semantic search
	ScrapedAt TIMESTAMP,
	ExpiresAt TIMESTAMP
);

-- Calculated match results
CREATE TABLE JobMatches (
	Id UUID PRIMARY KEY,
	UserId UUID REFERENCES Users(Id),
	OpportunityId UUID REFERENCES Opportunities(Id),
	MatchScore DECIMAL(5,2),   -- 0-100.00
	Analysis TEXT,             -- Why it matches
	SkillGaps JSONB,           -- Missing skills
	CalculatedAt TIMESTAMP,
	UNIQUE(UserId, OpportunityId)
);

-- Skill gaps identified by ML
CREATE TABLE SkillGaps (
	Id UUID PRIMARY KEY,
	UserId UUID REFERENCES Users(Id),
	SkillName VARCHAR,
	Priority INT (1-5),        -- How often needed in jobs
	LearningPath TEXT,         -- Resources + estimated time
	ImpactOnMatch DECIMAL(3,1),-- +5% match if learned
	UNIQUE(UserId, SkillName)
);

-- Referral profiles (employees at target companies)
CREATE TABLE ReferralProfiles (
	Id UUID PRIMARY KEY,
	CompanyId UUID,
	LinkedInUrl VARCHAR,
	Name VARCHAR,
	Title VARCHAR,
	Skills JSONB,
	ReferralScore DECIMAL(3,2),-- 0-100
	LastUpdated TIMESTAMP
);

-- Tracked user actions for analytics
CREATE TABLE OpportunityInteractions (
	Id UUID PRIMARY KEY,
	UserId UUID REFERENCES Users(Id),
	OpportunityId UUID REFERENCES Opportunities(Id),
	ActionType ENUM ('viewed', 'interested', 'applied', 'rejected', 'interviewed'),
	CreatedAt TIMESTAMP
);

-- Indexes for performance
CREATE INDEX idx_resume_embedding ON Resumes USING ivfflat(Embedding vector_cosine_ops);
CREATE INDEX idx_opportunity_embedding ON Opportunities USING ivfflat(Embedding vector_cosine_ops);
CREATE INDEX idx_user_matches ON JobMatches(UserId, MatchScore DESC);
```

---

### AI/ML Layer: Free Tier Stack

**LLM: Groq (Llama 3.3)**
- Free tier: 30 requests/minute
- Token limit: 405B total per model
- Use cases:
  - Resume skill extraction from PDF text
  - Job description analysis (requirements extraction)
  - Personalized outreach message generation

```python
# Example: Extract skills from resume text
response = groq_client.chat.completions.create(
	model="llama-3.3-70b-versatile",
	messages=[
		{"role": "system", "content": "Extract skills from resume. Return JSON."},
		{"role": "user", "content": resume_text}
	]
)
# Returns: { "skills": ["Python", "Kubernetes", "PostgreSQL", ...], "years": 5 }
```

**Embeddings: HuggingFace Transformers (sentence-transformers/all-MiniLM-L6-v2)**
- 384-dimensional vectors
- Fine-tuned on semantic similarity
- Free API tier
- Converts job descriptions + resume → comparable vectors
- Cosine similarity = match score

```python
# Semantic search example
from sentence_transformers import SentenceTransformer

model = SentenceTransformer('all-MiniLM-L6-v2')

resume_embedding = model.encode(resume_text)      # 384-dim vector
job_embedding = model.encode(job_description)     # 384-dim vector

# Cosine similarity (0-1, scaled to 0-100)
similarity = util.pytorch_cos_sim(resume_embedding, job_embedding)
match_score = similarity * 100  # e.g., 78.5%
```

**RAG (Retrieval-Augmented Generation) for Interview Prep:**
```csharp
// 1. Query external APIs (Blind, LeetCode, Levels.fyi)
var blindThreads = await blindClient.GetCompanyThreadsAsync(companyName);
var leetcodeProblems = await leetcodeClient.GetCompanyProblemsAsync(companyName);
var salaryData = await levelsClient.GetSalariesAsync(companyName, role);

// 2. Store in vector DB (pgvector)
foreach (var thread in blindThreads) {
	var embedding = embeddingService.Encode(thread.Content);
	await db.SaveInterviewContentAsync(companyName, "blind", embedding, thread);
}

// 3. Retrieve relevant context for user's role
var userRole = "Senior Engineer";
var relevantContext = await db.SemanticSearch(
	companyName: "Google",
	role: userRole,
	limit: 5
);

// 4. Generate guidance using LLM
var guidance = await groqClient.GenerateInterviewGuidanceAsync(
	company: "Google",
	role: userRole,
	context: relevantContext
);
```

---

### Deployment & Infrastructure

**Docker Containerization:**
- `docker/Dockerfile` - Main production image
- `docker/Dockerfile.api` - ASP.NET Core API service
- `docker/Dockerfile.web` - Blazor WebAssembly frontend
- `docker/docker-compose.yml` - Local dev environment

**Deployment Targets:**
1. **Render.com** (Free tier - current)
   - PostgreSQL managed database
   - Redis instance for caching
   - Auto-deploy on git push (GitHub Actions)
   - Blueprint configuration: `render.yaml`

2. **Docker Hub** (Planned v2.5)
   - Multiarch images (amd64, arm64, armv7)
   - Container registry for Kubernetes

3. **GitHub Container Registry** (ghcr.io)
   - Private images for teams
   - Auto-publish on release tags

**Caching Strategy (Redis):**
```csharp
// Cache match scores (prevent recalculation)
var cacheKey = $"match:{userId}:{jobId}";
var cachedScore = await cache.GetAsync<JobMatchDto>(cacheKey);
if (cachedScore == null) {
	cachedScore = CalculateMatch(user, job);
	await cache.SetAsync(cacheKey, cachedScore, TimeSpan.FromDays(7));
}

// Cache opportunity rankings (expensive to compute)
var rankingKey = $"opportunities:{userId}:today";
var cachedRankings = await cache.GetAsync<List<OpportunityDto>>(rankingKey);
// TTL: 1 hour (refreshes daily)
```

---

## Release Timeline & Distribution

| Version | Date | Phase | Distribution |
|---------|------|-------|---|
| **v1.0** | Q1 2024 | Resume Intelligence | Source code |
| **v2.0** | Sep 2024 | Opportunity Radar (Current) | GitHub Release |
| **v2.5** | Dec 2024 | Docker Distribution | Docker Hub, ghcr.io |
| **v3.0** | May 2025 | Extensions | VS Code, Chrome, Firefox |
| **v3.1** | June 2025 | SDKs | NuGet (.NET), npm (JS), CLI |

---

## Key Metrics & Success Indicators

**User Experience:**
- Resume upload → 10 matches: <500ms (cached)
- Match calculation: <200ms (99th percentile)
- Dashboard load: <1.5 seconds
- Search: <300ms

**Product:**
- Average match score: 55-70% (optimized distribution)
- Users viewing 5+ matches: 65% (engagement)
- Apply conversion: 12-15% (vs industry 3-5%)
- Interview callbacks: 30% from HireKarlo opportunities

**Infrastructure:**
- Uptime: 99.9% (Render SLA)
- API latency: p50 <100ms, p95 <200ms, p99 <500ms
- Database connections: <10 (managed pool)
- Cache hit rate: >85% (match scores)

---

## Technology Justifications

| Technology | Why |
|-----------|-----|
| **.NET 9 / Blazor** | Type-safe full-stack, excellent tooling, performance |
| **PostgreSQL + pgvector** | ACID compliance + semantic search (no separate vector DB) |
| **Groq API** | Free LLM tier (30 req/min), fast (70B model) |
| **HuggingFace** | Free embeddings, proven on similarity tasks |
| **Redis** | Sub-millisecond cache for opportunity rankings |
| **Docker** | Consistent dev/prod environment, easy scaling |
| **GitHub Actions** | Native CI/CD, auto-publish to NuGet/npm/Docker |
| **Render** | Free tier ($0/month), managed DB, auto-deploys |

---

## Contributing & Roadmap

**Phase 2.1 (Oct 2024):** Dream Company Intelligence
- Target company analysis
- Employee discovery + LinkedIn scraping
- Personalized learning paths

**Phase 3 (May 2025):** Extensions
- VS Code plugin (browse jobs in editor)
- Browser extensions (enhance job boards)

**Phase 4 (June 2025):** SDKs
- NuGet SDK for .NET developers
- npm SDK for JavaScript
- CLI tool for command-line users

---

**Start here:** Clone repo, run locally, deploy to Render → https://github.com/K-riti/HireKarlo
