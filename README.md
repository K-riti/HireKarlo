# HireKarlo — AI Career Operating System

> An AI-powered platform helping engineers discover high-match opportunities, understand skill gaps, build learning roadmaps, find referrals, and ace interviews.

[![Latest Release](https://img.shields.io/badge/Release-v2.0.0-blue?logo=github)](https://github.com/K-riti/HireKarlo/releases/tag/v2.0.0)
[![.NET 9](https://img.shields.io/badge/.NET-9.0-purple)](https://dotnet.microsoft.com)
[![Phase 2 Active](https://img.shields.io/badge/Phase-2%3A%20Opportunity%20Radar-brightgreen)](#phase-2-opportunity-radar-current)
[![MIT License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## What is HireKarlo?

**Career Operating System for engineers** — Not a browser extension or job board aggregator.

| Feature | What It Does |
|---------|-------------|
| **🎯 Opportunity Radar** | Daily job recommendations ranked 0-100% match |
| **🏢 Dream Company Intelligence** | Analyze target companies + personalized skill paths |
| **🤝 Referral Intelligence** | Auto-discover qualified referrals + message generation |
| **💼 Interview Digest** | Company-specific interview prep (Blind, LeetCode, Levels.fyi) |
| **📈 Skill ROI Engine** | "Learn X skill → gain Y% match improvement" |

**Current Release**: [v2.0.0 on GitHub Releases](https://github.com/K-riti/HireKarlo/releases/tag/v2.0.0)

---

## Tech Stack

| Layer | Technology |
| --- | --- |
| **Frontend** | Blazor WebAssembly (.NET 9) |
| **API** | ASP.NET Core 9 |
| **Database** | PostgreSQL + pgvector |
| **Cache** | Redis |
| **LLM** | Groq (Llama 3.3) — free tier |
| **Embeddings** | HuggingFace — free tier |

---

## Getting Started

### Local Development

```bash
# Clone repo
git clone https://github.com/K-riti/HireKarlo.git
cd HireKarlo

# Get free API keys
# 1. Groq: https://console.groq.com (30 req/min free)
# 2. HuggingFace: https://huggingface.co/settings/tokens

# Setup database
dotnet ef database update -p src/Infrastructure/HireKarlo.Persistence -s src/Presentation/HireKarlo.Api

# Run API backend
dotnet run --project src/Presentation/HireKarlo.Api

# Access web app: http://localhost:5000
# Swagger API docs: http://localhost:5000/swagger
```

### Deploy to Render (Free Tier)

1. [Fork this repo](https://github.com/K-riti/HireKarlo/fork)
2. Go to [render.com](https://render.com) → New → Blueprint
3. Select your fork (auto-detects `render.yaml`)
4. Add environment variables:
   - `Groq__ApiKey=gsk_...`
   - `HuggingFace__ApiKey=hf_...`
5. Deploy!

**Result**:
- Web: `https://hirekarlo-web.onrender.com`
- API: `https://hirekarlo-api.onrender.com`

---

## Releases & Packages

### v2.0.0 (Current) — Opportunity Radar

**Available**:
- ✅ [GitHub Release v2.0.0](https://github.com/K-riti/HireKarlo/releases/tag/v2.0.0) with complete release notes
- ✅ Source code on GitHub (MIT licensed)
- ✅ Deployment-ready via GitHub CI/CD

**How to get it**:
1. Download source from [Release Page](https://github.com/K-riti/HireKarlo/releases/tag/v2.0.0)
2. Clone the repo: `git clone https://github.com/K-riti/HireKarlo.git`
3. Deploy locally or to Render (see above)

### Future Packages (Roadmap)

| Version | Package | Type | Timeline |
|---------|---------|------|----------|
| v2.5.0 | Docker Hub | Container | Q4 2024 |
| v3.0.0 | Chrome/Firefox | Browser Extension | May 2025 |
| v3.0.0 | VS Code | Extension | May 2025 |
| v3.1.0 | NuGet | .NET SDK | June 2025 |
| v3.1.0 | npm | JavaScript SDK | June 2025 |

📍 See [`.release/ROADMAP.md`](.release/ROADMAP.md) for detailed phase breakdown.

---

## Documentation

| Resource | Purpose |
|----------|---------|
| [`docker/`](docker/) | Docker files (Dockerfile, docker-compose) |
| [`.release/ROADMAP.md`](.release/ROADMAP.md) | Version roadmap (v1.0 → v4.0) + package distribution plan |
| [`.release/CHANGELOG.md`](.release/CHANGELOG.md) | Release history |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | System design + data models |
| [`docs/PHASE_2.md`](docs/PHASE_2.md) | Opportunity Radar implementation guide |
| [`docs/ABOUT.md`](docs/ABOUT.md) | Recruiter-friendly overview |

---

## Features by Phase

### Phase 1: Resume Intelligence ✅
- Resume PDF/DOCX parsing
- LLM skill extraction
- Vector embeddings (pgvector)
- REST API

### Phase 2: Opportunity Radar 🚀 (Current)
- Daily job opportunity scoring
- Smart matching algorithm
- Dream company analysis
- Referral intelligence
- Interview prep aggregation

### Phase 3: Extensions 📅 
- VS Code Extension
- Chrome & Firefox Extensions

### Phase 4: SDKs 📅 
- NuGet SDK (.NET)
- NPM SDK (JavaScript)
- CLI Tool

---

## Contributing

1. Fork the repo
2. Create feature branch: `git checkout -b feature/amazing-feature`
3. Commit: `git commit -m 'feat: Add amazing feature'`
4. Push: `git push origin feature/amazing-feature`
5. Open Pull Request

---

## License

MIT License — see [LICENSE](LICENSE)

---

## Links

- 📰 [GitHub Issues](https://github.com/K-riti/HireKarlo/issues)
- 💬 [Discussions](https://github.com/K-riti/HireKarlo/discussions)
- 🏷️ [Releases](https://github.com/K-riti/HireKarlo/releases)
- 📚 [Architecture](docs/ARCHITECTURE.md)
- 🗺️ [Roadmap](.release/ROADMAP.md)

---

**Built with ❤️ for engineers building their careers.**
