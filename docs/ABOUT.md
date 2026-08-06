# Career Operating System for Engineers

HireKarlo is an AI-powered platform helping engineers discover high-match opportunities, understand skill gaps, build learning roadmaps, find referrals, and ace interviews.

**Current Release**: v2.0.0-dev (Phase 2: Opportunity Radar)

## Quick Links
- 📖 **README**: Full project overview and quick start
- 🚀 **RELEASES.md**: Version history and phase roadmap
- 🏗️ **docs/ARCHITECTURE.md**: System design and technical guide
- 📋 **docs/PHASE_2.md**: Current implementation roadmap

## Tech Stack
- **Backend**: ASP.NET Core 9, C#
- **Frontend**: Blazor WebAssembly
- **Database**: PostgreSQL + pgvector
- **AI**: Groq (Llama 3.3) + HuggingFace embeddings
- **Deployment**: Docker, Render, Azure

## Core Features
1. **Opportunity Radar** — Daily job recommendations ranked by match
2. **Dream Company Intelligence** — Target company analysis & skill paths
3. **Referral Intelligence** — Auto-discover & score referrals
4. **Interview Digest** — Company-specific prep aggregation
5. **Skill ROI Engine** — Optimize learning for career growth

## For Recruiters
HireKarlo demonstrates:
- ✅ Complex backend architecture (layered .NET services)
- ✅ AI/ML capabilities (RAG, embeddings, LLMs)
- ✅ Database design (PostgreSQL, pgvector, optimization)
- ✅ API design (REST, auth, rate limiting)
- ✅ Frontend development (Blazor WebAssembly)
- ✅ DevOps (Docker, CI/CD pipelines)
- ✅ Product thinking (feature prioritization, user value)
- ✅ Multi-platform distribution (web, extension, SDK)

## Deployment
```bash
# Local development
docker-compose up
dotnet run --project src/Presentation/HireKarlo.Api

# Production (via tag)
git tag v2.0.0
git push origin v2.0.0  # Auto-deploys to Render/Azure
```

## Status
- **Phase 1** ✅ Complete (Resume Intelligence)
- **Phase 2** 🔄 In Progress (Opportunity Radar)
- **Phase 3** 📋 Planned (VS Code + Browser Extensions)
- **Phase 4** 📋 Planned (NuGet + NPM SDKs)

---

**Built for engineers building their careers.** | MIT License
