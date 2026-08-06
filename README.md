# HireKarlo — AI Career Operating System

> An AI-powered platform helping engineers discover high-match opportunities, understand skill gaps, build learning roadmaps, find referrals, and ace interviews.

![Version](https://img.shields.io/badge/version-2.0.0--dev-blue)
![.NET](https://img.shields.io/badge/.NET-9.0-purple)
![Blazor](https://img.shields.io/badge/frontend-Blazor-blueviolet)
![License](https://img.shields.io/badge/license-MIT-green)

## ?? What is HireKarlo?

**Career Operating System for engineers** (not browser extension or job board aggregator)

| Feature | What It Does |
|---------|-------------|
| **?? Opportunity Radar** | Daily job recommendations ranked 0-100% match |
| **?? Dream Company Intelligence** | Analyze target companies + personalized skill paths |
| **?? Referral Intelligence** | Auto-discover qualified referrals + message generation |
| **?? Interview Digest** | Company-specific interview prep (Blind, LeetCode, Levels.fyi) |
| **?? Skill ROI Engine** | "Learn X skill ? gain Y% match increase" |

**Current Status**: v2.0.0-dev (Phase 2: Opportunity Radar in progress)

---

## ??? Tech Stack

| Layer | Technology |
|-------|-----------|
| **Frontend** | Blazor WebAssembly (.NET 9) |
| **API** | ASP.NET Core 9 |
| **Database** | PostgreSQL + pgvector |
| **Cache** | Redis |
| **LLM** | Groq (Llama 3.3) — free tier: \ |
| **Embeddings** | HuggingFace — \ |

---

## ?? Quick Start

### Local Development

\\\ash
# Clone repo
git clone https://github.com/K-riti/HireKarlo.git
cd HireKarlo

# Get free API keys
# - Groq: https://console.groq.com (free)
# - HuggingFace: https://huggingface.co/settings/tokens (free)

# Setup database
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api

# Run API
dotnet run --project src/Presentation/HireKarlo.Api

# Web app: http://localhost:5000
# Swagger: http://localhost:5000/swagger
\\\

### Deploy to Render (Free)

1. Fork this repo
2. Go to [render.com](https://render.com) ? New ? Blueprint
3. Select your fork (Blueprint detected via \ender.yaml\)
4. Add API keys: \Groq__ApiKey\, \HuggingFace__ApiKey\
5. Deploy!

**Your app URLs**:
- Web: \https://hirekarlo-web.onrender.com\
- API: \https://hirekarlo-api.onrender.com\

---

## ?? Documentation

| Page | Purpose |
|------|---------|
| **[RELEASES.md](RELEASES.md)** | Version history + phase-based roadmap |
| **[docs/ARCHITECTURE.md](docs/ARCHITECTURE.md)** | System design + data models |
| **[docs/PHASE_2.md](docs/PHASE_2.md)** | Current implementation roadmap |
| **[docs/ABOUT.md](docs/ABOUT.md)** | Recruiter-friendly summary |

---

## ?? Core Features (by Phase)

### ? Phase 1: Resume Intelligence (Complete)
- Resume PDF/DOCX parsing
- LLM-based skill extraction
- Vector embeddings (pgvector)
- REST API endpoints

### ?? Phase 2: Opportunity Radar (In Progress)
- Job board scraping & matching
- Daily dashboard with ranked opportunities
- Dream company analysis + skill paths
- Referral intelligence
- Interview prep aggregation

### ?? Phase 3: Extensions (Planned - May 2025)
- VS Code extension
- Chrome & Firefox extensions
- Local-first mode

### ?? Phase 4: SDKs (Planned - June 2025)
- NuGet SDK (.NET)
- NPM SDK (JavaScript)
- CLI tool
- REST API v2

---

## ?? Contributing

1. Fork the repo
2. Create a feature branch (\git checkout -b feature/AmazingFeature\)
3. Commit changes (\git commit -m 'feat: Add AmazingFeature'\)
4. Push to branch (\git push origin feature/AmazingFeature\)
5. Open a Pull Request

---

## ?? License

MIT License - see [LICENSE](LICENSE) file

---

**Built with ?? for engineers building their careers.**
