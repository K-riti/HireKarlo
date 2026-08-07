# HireKarlo — AI Career Operating System

> An AI-powered platform that helps engineers discover high-match opportunities, understand skill gaps, build learning roadmaps, find referrals, and ace interviews.

[![Version](https://img.shields.io/badge/version-2.0.0--dev-blue)](https://github.com/K-riti/HireKarlo/releases)
[![.NET](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com)
[![License](https://img.shields.io/badge/license-MIT-green)](LICENSE)

## ?? What is HireKarlo?

**Career Operating System** — Not a browser extension or job board aggregator.

Instead of browsing thousands of jobs aimlessly:

| Feature | What It Does |
|---------|-------------|
| **?? Opportunity Radar** | Daily job recommendations ranked 0-100% match |
| **?? Dream Company Intelligence** | Analyze target companies + personalized learning paths |
| **?? Referral Intelligence** | Auto-discover qualified referrals + message generation |
| **?? Interview Digest** | Company-specific interview prep (Blind, LeetCode, Levels.fyi) |
| **?? Skill ROI Engine** | \"Learn X skill ? gain Y% match improvement\" |

**Status**: v2.0.0-dev (Phase 2: Opportunity Radar)

---

## ??? Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor WebAssembly (.NET 9) |
| **API** | ASP.NET Core 9 |
| **Database** | PostgreSQL + pgvector |
| **Cache** | Redis |
| **LLM** | Groq (Llama 3.3) — **free** |
| **Embeddings** | HuggingFace — **free** |

---

## ?? Quick Start

### Local Development

\\\ash
# Clone repo
git clone https://github.com/K-riti/HireKarlo.git
cd HireKarlo

# Get free API keys
# 1. Groq: https://console.groq.com (30 req/min free)
# 2. HuggingFace: https://huggingface.co/settings/tokens

# Setup database
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api

# Run API
dotnet run --project src/Presentation/HireKarlo.Api

# Open browser
# Web app: http://localhost:5000
# Swagger: http://localhost:5000/swagger
\\\

### Deploy to Render (Free Tier)

1. Fork this repo
2. Go to [render.com](https://render.com) ? New ? Blueprint
3. Select your fork (auto-detects \ender.yaml\)
4. Add environment variables:
   - \Groq__ApiKey=gsk_...\
   - \HuggingFace__ApiKey=hf_...\
5. Deploy!

**Your URLs**:
- Web: \https://hirekarlo-web.onrender.com\
- API: \https://hirekarlo-api.onrender.com\

---

## ?? Documentation

| Document | Purpose |
|----------|---------|
| [.\release/ROADMAP.md](.release/ROADMAP.md) | Phase-based version roadmap (v1.0 ? v4.0) |
| [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) | System design + data models |
| [docs/PHASE_2.md](docs/PHASE_2.md) | Opportunity Radar implementation guide |
| [docs/ABOUT.md](docs/ABOUT.md) | Recruiter-friendly overview |

---

## ?? Core Services

### Phase 1: Resume Intelligence ?
- Resume PDF/DOCX parsing
- LLM skill extraction
- Vector embeddings (pgvector)

### Phase 2: Opportunity Radar ?? (Current)
- Daily job opportunity scoring
- Smart matching algorithm
- Dream company analysis
- Referral intelligence
- Interview prep aggregation

### Phase 3: Extensions ?? (May 2025)
- VS Code Extension
- Chrome & Firefox Extensions

### Phase 4: SDKs ?? (June 2025)
- NuGet SDK (.NET)
- NPM SDK (JavaScript)
- CLI Tool

---

## ?? Packages & Distribution

### Current Release (v2.0.0)

**Available on**:
- ? GitHub releases page (v2.0.0 tag)
- ? Docker: \docker pull hirekarlo:2.0.0\ (coming soon)
- ? Source code: GitHub repo

**How it comes**:
- GitHub Release with all artifacts
- Tagged in git: \2.0.0\
- Deployment-ready via GitHub Actions CI/CD

### Future Packages

| Package | Version | Type | Status |
|---------|---------|------|--------|
| **NuGet** | v3.1.0 | \.NET SDK\ | ?? Planned Jun 2025 |
| **npm** | v3.1.0 | \JavaScript SDK\ | ?? Planned Jun 2025 |
| **VS Code Extension** | v3.0.0 | \Extension\ | ?? Planned May 2025 |
| **Chrome Extension** | v3.0.0 | \Extension\ | ?? Planned May 2025 |

See [.release/ROADMAP.md](.release/ROADMAP.md) for detailed roadmap.

---

## ?? What This Demonstrates

### Engineering
- ? Layered architecture (clean separation)
- ? Dependency injection
- ? Vector semantic search (pgvector)
- ? LLM integration (RAG pipeline)

### Product
- ? Clear roadmap (4 phases over 12 months)
- ? Feature prioritization
- ? User-centric design
- ? Multi-platform strategy

### DevOps
- ? Docker containerization
- ? GitHub Actions CI/CD
- ? Database migrations
- ? Free-tier scalability

---

## ?? Contributing

1. Fork the repo
2. Create feature branch: \git checkout -b feature/amazing-feature\
3. Commit: \git commit -m 'feat: Add amazing feature'\
4. Push: \git push origin feature/amazing-feature\
5. Open Pull Request

---

## ?? License

MIT — see [LICENSE](LICENSE) file

---

## ?? Links

- **Issues**: [GitHub Issues](https://github.com/K-riti/HireKarlo/issues)
- **Discussions**: [GitHub Discussions](https://github.com/K-riti/HireKarlo/discussions)
- **Release Notes**: [.release/](.release/)
- **Architecture**: [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)

---

**Built with ?? for engineers building their careers.**
