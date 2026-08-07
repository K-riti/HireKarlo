# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

---

## [2.0.0] - 2026-08-07

### 🎯 What's New: Opportunity Radar

HireKarlo transforms from a prototype resume-analysis tool into an **AI Career Operating System** with real-time opportunity discovery at its core.

### ✨ Major Features

#### **Opportunity Radar** (New)
- Daily job recommendations ranked 0-100% match
- Intelligent job board scraping and semantic matching
- Smart filtering (only recommend 70%+ matches)
- Detailed opportunity analysis with skill gaps

#### **Dream Company Intelligence** (New)
- Analyze target companies vs. your resume
- Personalized skill learning paths
- "Gap to Target" analysis with ROI estimates
- Referral discovery within target companies

#### **Referral Intelligence** (New)
- Auto-discover qualified referrals
- Skill + experience + network scoring
- AI-generated personalized outreach messages
- Referral pipeline tracking

#### **Interview Digest** (New)
- Aggregate company-specific interview prep
- Blind, LeetCode, Levels.fyi integration
- Interview question patterns by role
- Topic-based study recommendations

#### **Skill ROI Engine** (New)
- "Learn skill X → gain Y% match improvement"
- Learning path prioritization
- Competency progression tracking
- 6-month career roadmap

### 🏗️ Architecture Changes

#### **New Services (Application Layer)**
- `IOpportunityRadarService` — Daily opportunity discovery & ranking
- `IDreamCompanyIntelligenceService` — Target company analysis
- `IReferralIntelligenceService` — Referral scoring & outreach
- `IInterviewDigestService` — Interview prep aggregation
- `ISkillROIService` — Learning optimization

#### **New Repositories (Data Layer)**
- `IOpportunityMatchRepository` — Job/resume matches
- `IDreamCompanyMatchRepository` — Company matches
- `IReferralTargetRepository` — Referral contacts
- `ISkillGapRecommendationRepository` — Skill gaps
- `ICareerProgressRepository` — Progress tracking

#### **New API Endpoints**
```
GET    /api/opportunities                    # Daily radar
GET    /api/opportunities/{id}               # Details
POST   /api/opportunities/{id}/apply         # Track application
GET    /api/dream-companies/{id}/intelligence
POST   /api/referrals/{id}/outreach         # Generate message
GET    /api/interviews/{companyId}/prep     # Interview guide
```

### 📚 Documentation Refactor

#### **Removed (11 redundant files)**
- `ARCHITECTURE_COMPLETE.md`
- `CAREER_OS_STRATEGY.md`
- `EXECUTIVE_BRIEF.md`
- `PHASE_1_COMPLETE.md`, `PHASE_1_COMPLETION_REPORT.md`, `PHASE_1_SUMMARY.md`
- `PHASE_2_PLAN.md`
- `PRODUCT_FLOW.md`
- `QUICK_REFERENCE.md`
- `README_NEW.md`, `README_OPERATING_SYSTEM.md`
- `RELEASE_CHECKLIST.md`

#### **Added (Single Source of Truth)**
- `README.md` — Concise overview (533 → 150 lines)
- `RELEASES.md` — Phase-based roadmap + release strategy
- `docs/ARCHITECTURE.md` — Complete system design
- `docs/PHASE_2.md` — Opportunity Radar implementation details
- `docs/ABOUT.md` — GitHub About section

### 🚀 Deployment

#### **New Deployment Options**
- Docker image available: `hirekarlo:2.0.0`
- Render (free tier) with PostgreSQL + Redis
- GitHub Actions CI/CD pipeline
- Automated testing on every push

#### **Database Migrations**
```bash
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api
```

### 📊 Release Strategy

**Version Roadmap**:
- **v1.0** ✅ Resume Intelligence (Complete)
- **v2.0** 🔄 Opportunity Radar (Current)
- **v3.0** 📋 VS Code + Browser Extensions (May 2025)
- **v3.1** 📋 NuGet + NPM SDKs (June 2025)

**Phase Release Structure**:
- Pre-release checks (tests, code review, security)
- Automated deployment via GitHub Actions
- Database migrations automatically applied
- Release notes + Docker image publish

### 🔄 Breaking Changes

**Documentation Structure**:
- Old: 20+ scattered markdown files
- New: Centralized in `README.md`, `RELEASES.md`, `/docs` folder
- Action: Update bookmarks to point to [RELEASES.md](RELEASES.md)

**Release Strategy**:
- Old: Multiple README versions
- New: Phase-based (v1.0 → v4.0) with clear dates

### 🐛 Bug Fixes

- Fixed vector store persistence on Render cold starts (PostgresVectorStore)
- Improved Groq API rate-limit handling with exponential backoff
- Better error handling for job board scraping failures

### 📈 Performance

- Reduced markdown file clutter (11 files consolidated)
- Faster repo navigation (single-source-of-truth docs)
- Clearer CI/CD pipeline (GitHub Actions)

### 🎓 For Recruiters

Demonstrates:
- ✅ Complex business logic (opportunity matching algorithms)
- ✅ Real-time data aggregation (job board scraping)
- ✅ AI/ML integration (semantic matching, LLM embeddings)
- ✅ Multi-service architecture (.NET layered services)
- ✅ Database optimization (pgvector semantic search)
- ✅ DevOps (Docker, CI/CD, GitHub Actions)
- ✅ Product thinking (5 core services solving real user problems)

### 🙏 Credits

- **Job Matching**: Semantic search via pgvector + Groq LLM
- **Referral Intelligence**: Network analysis + AI scoring
- **Interview Prep**: Public data aggregation from Blind, LeetCode, Levels.fyi
- **Frontend**: Blazor WebAssembly with real-time updates

### 📞 Support

- **Issues**: [GitHub Issues](https://github.com/K-riti/HireKarlo/issues)
- **Discussions**: [GitHub Discussions](https://github.com/K-riti/HireKarlo/discussions)

---

## [1.0.0] - 2026-01-15

### Phase 1: Resume Intelligence ✅

**Initial Release**

- Resume PDF/DOCX parsing
- LLM-based skill extraction (Groq)
- Experience level detection
- Vector embeddings (HuggingFace + pgvector)
- REST API endpoints (Auth, Resumes)
- Blazor WebAssembly frontend
- OAuth integration (Google, LinkedIn)

---

## Upcoming

### [3.0.0] - May 2025
- VS Code Extension
- Chrome & Firefox Extensions
- Local-first offline mode

### [3.1.0] - June 2025
- NuGet SDK (.NET)
- NPM SDK (JavaScript)
- CLI Tool
- REST API v2
- Enterprise support

---

**See [RELEASES.md](RELEASES.md) for detailed phase roadmap and deployment strategy.**
