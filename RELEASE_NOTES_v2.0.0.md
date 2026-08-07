# GitHub Release Notes: v2.0.0

**Copy this entire content to GitHub → Releases → Create Release**

---

## 🚀 HireKarlo v2.0.0 — AI Career Operating System

**Release Date**: August 7, 2026

### Phase 2: Opportunity Radar

HireKarlo evolves from a prototype resume analyzer into a **Career Operating System** that discovers, ranks, and helps you apply to high-match opportunities daily.

---

## ✨ What's New

### 📡 Opportunity Radar (The Centerpiece)
Wake up → Open dashboard → See your top 10 job opportunities ranked by match % (0-100%). No more browsing job boards aimlessly.

```
Match Score = (Skill TF-IDF × 0.5) + (Experience Level × 0.3) + (Location × 0.2)
Only recommend jobs ≥70% match
Daily refresh from 100+ job boards
```

### 🎯 Dream Company Intelligence
- Analyze how well you fit target companies
- Auto-generate personalized 6-month skill roadmaps
- Track "days to reach X% match for [company]"
- Identify the top 3 skills to learn for maximum ROI

### 🤝 Referral Intelligence
- Auto-discover qualified referrals at target companies
- Score referrals by: tech stack match (40%) + experience (30%) + network activity (20%) + response rate (10%)
- AI-generate personalized outreach messages
- Track referral pipeline (messaged → responded → referred)

### 🎓 Interview Digest
- Aggregate company-specific interview data from Blind, LeetCode, Levels.fyi
- Group questions by topic (System Design, Behavioral, Coding)
- Identify past question patterns by role
- Generate personalized study plans

### 📈 Skill ROI Engine
- "Learning Python → +5% match to Google"
- Prioritized learning paths (highest impact first)
- 6-month career roadmap with milestones
- Competency progression tracking

---

## 🏗️ Architecture

**New Core Services**:
- `OpportunityRadarService` — Daily opportunity discovery
- `DreamCompanyIntelligenceService` — Target company analysis
- `ReferralIntelligenceService` — Referral discovery & scoring
- `InterviewDigestService` — Interview prep aggregation
- `SkillROIService` — Learning path optimization

**New API Endpoints** (15+ endpoints):
```
GET    /api/opportunities                 # Daily radar
GET    /api/dream-companies/{id}/intelligence
POST   /api/referrals/score               # Referral scoring
GET    /api/interviews/{companyId}/prep   # Interview guide
```

**Tech Stack**:
- Backend: ASP.NET Core 9
- Frontend: Blazor WebAssembly
- Database: PostgreSQL + pgvector
- AI: Groq (Llama 3.3 70B) — free tier
- Embeddings: HuggingFace — $0

---

## 📚 Documentation Overhaul

**Why**: Previous version had 20+ scattered markdown files, causing:
- Contribution noise (every doc update = 1 commit)
- Confusion about what's current vs. deprecated
- Recruiter confusion (multiple competing READMEs)

**Solution**: Single source of truth
- **README.md** — Concise overview + quick start (150 lines, was 533)
- **RELEASES.md** — Phase roadmap + deployment strategy (v1.0 → v4.0)
- **docs/ARCHITECTURE.md** — System design + data models
- **docs/PHASE_2.md** — Opportunity Radar implementation details
- **CHANGELOG.md** — Version history

---

## 🚀 Deployment

### Quick Start (Local)
```bash
git clone https://github.com/K-riti/HireKarlo.git
cd HireKarlo

# Get free API keys
# 1. Groq: https://console.groq.com (30 req/min free)
# 2. HuggingFace: https://huggingface.co/settings/tokens

# Setup database
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api

# Run
dotnet run --project src/Presentation/HireKarlo.Api
# Browse: http://localhost:5000
```

### Deploy to Render (Free)
1. Fork this repo
2. Go to [render.com](https://render.com) → New → Blueprint
3. Select your fork (Blueprint detected via `render.yaml`)
4. Add environment variables:
   - `Groq__ApiKey=gsk_...`
   - `HuggingFace__ApiKey=hf_...`
5. Deploy!

**Your URLs**:
- Web: `https://hirekarlo-web.onrender.com`
- API: `https://hirekarlo-api.onrender.com`
- Swagger: `/swagger`

---

## 📊 Release Strategy

**Version Roadmap**:
| Version | Phase | Status | Date |
|---------|-------|--------|------|
| v1.0 | Resume Intelligence | ✅ Complete | Jan 2026 |
| **v2.0** | **Opportunity Radar** | 🔄 Current | Aug 2026 |
| v3.0 | VS Code + Browser Extensions | 📋 Planned | May 2027 |
| v3.1 | NuGet + NPM SDKs | 📋 Planned | Jun 2027 |

See **[RELEASES.md](https://github.com/K-riti/HireKarlo/blob/main/RELEASES.md)** for detailed roadmap.

---

## 🎓 For Recruiters

This project demonstrates:
- ✅ **Complex Backend Architecture** — Layered services, dependency injection, clean code
- ✅ **AI/ML Integration** — RAG pipeline, semantic search, LLM APIs
- ✅ **Database Design** — PostgreSQL, pgvector, query optimization
- ✅ **API Design** — REST, JWT auth, rate limiting, Swagger docs
- ✅ **Frontend Development** — Blazor WebAssembly, real-time updates
- ✅ **DevOps** — Docker, CI/CD (GitHub Actions), automated testing
- ✅ **Product Thinking** — Feature prioritization, user-centric design, multi-phase roadmap
- ✅ **Multi-Platform Distribution** — Web, extension, API, SDK strategy

---

## 🔄 Breaking Changes

### Documentation Structure
- **Old**: 20+ scattered markdown files (`ARCHITECTURE_COMPLETE.md`, `CAREER_OS_STRATEGY.md`, etc.)
- **New**: Centralized structure (`README.md`, `RELEASES.md`, `/docs` folder)
- **Action**: Update bookmarks to [RELEASES.md](https://github.com/K-riti/HireKarlo/blob/main/RELEASES.md) for roadmap

### Version Stability
- All v2.x will maintain backward compatibility with v2.0.0 API
- Breaking changes (if any) reserved for v3.0
- Deprecation notice 6 months in advance

---

## 🐛 Bug Fixes

- ✅ Fixed vector store persistence on Render cold starts (PostgresVectorStore now persists)
- ✅ Improved Groq API rate-limit handling (exponential backoff)
- ✅ Better job board scraping error handling

---

## 📦 Assets

### Download
- **Docker Image**: `docker pull hirekarlo:2.0.0` (when available on Docker Hub)
- **Source Code**: See Assets section below

### Docs
- 📖 [README.md](https://github.com/K-riti/HireKarlo/blob/main/README.md)
- 📖 [RELEASES.md](https://github.com/K-riti/HireKarlo/blob/main/RELEASES.md)
- 📖 [CHANGELOG.md](https://github.com/K-riti/HireKarlo/blob/main/CHANGELOG.md)
- 📖 [docs/ARCHITECTURE.md](https://github.com/K-riti/HireKarlo/blob/main/docs/ARCHITECTURE.md)
- 📖 [docs/PHASE_2.md](https://github.com/K-riti/HireKarlo/blob/main/docs/PHASE_2.md)

---

## 🙏 Thank You

Built with ❤️ for engineers building their careers.

**Questions?** Open an [issue](https://github.com/K-riti/HireKarlo/issues) or start a [discussion](https://github.com/K-riti/HireKarlo/discussions).

---

## 🔗 Links

- **GitHub**: [K-riti/HireKarlo](https://github.com/K-riti/HireKarlo)
- **Live Demo**: (Coming soon)
- **Issues**: [Report bugs](https://github.com/K-riti/HireKarlo/issues)
- **Discussions**: [Feature requests](https://github.com/K-riti/HireKarlo/discussions)

---

**v2.0.0 — Phase 2: Opportunity Radar** 🚀
