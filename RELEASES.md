# Release Strategy & Phase Roadmap

HireKarlo follows a phase-based release structure with semantic versioning: **v[PHASE].[FEATURE].[PATCH]**

---

## 🎯 Version Plan

| Version | Phase | Status | Focus | Release Target |
|---------|-------|--------|-------|-----------------|
| v1.x | Resume Intelligence | ✅ Complete | Resume parsing, skill extraction, embedding | Jan 2025 |
| **v2.0** | **Opportunity Radar** | 🔄 In Progress | Job matching, daily dashboard, ML scoring | Feb 2025 |
| v2.1 | Dream Company Intelligence | 📋 Planned | Target analysis, learning paths, gap detection | Mar 2025 |
| v2.2 | Referral + Interview Digest | 📋 Planned | Referral discovery, interview prep RAG | Apr 2025 |
| v3.0 | Extensions Release | 📋 Planned | VS Code + Browser extension | May 2025 |
| v3.1 | SDK Package Release | 📋 Planned | .NET NuGet + JavaScript NPM | Jun 2025 |

---

## 📦 Phase 1: Resume Intelligence ✅

**Release**: v1.0.0  
**Status**: Complete  
**Deliverables**:
- Resume PDF/DOCX parsing
- LLM-based skill extraction
- Experience level detection
- Vector embeddings (pgvector)
- REST API endpoints

**Package Formats**:
- ✅ Docker image: `hirekarlo:1.0.0`
- ✅ API deployment: Render (free tier)
- ✅ Database: PostgreSQL 15+

**Deploy Command**:
```bash
docker pull hirekarlo:1.0.0
docker run -e DATABASE_URL=... -p 5000:5000 hirekarlo:1.0.0
```

---

## 🎯 Phase 2: Opportunity Radar & Intelligence

**Release**: v2.0.0 (in progress)  
**Status**: 🔄 Active Development  
**Target Date**: February 2025  

### Features
- ✅ Job board scraping (LinkedIn, Wellfound, etc.)
- ✅ Match scoring algorithm (0-100%)
- ✅ Daily opportunity dashboard (Blazor)
- ✅ Dream Company analysis
- ✅ Learning path generation
- ✅ Skill gap detection
- 🔄 Referral discovery
- 🔄 Interview prep aggregation

### Deployment Options
```bash
# Option A: Docker (any server)
docker build -t hirekarlo:2.0.0 .
docker push your-registry/hirekarlo:2.0.0

# Option B: Render (free)
git push origin main
# Auto-deploys via GitHub Actions → .github/workflows/release.yml

# Option C: Azure Container Registry
az acr build --registry $ACR_NAME --image hirekarlo:2.0.0 .
```

### Package Formats
- **Docker**: `hirekarlo:2.0.0`
- **API**: REST endpoints (Swagger docs)
- **Release Notes**: See v2.0.0 tag on GitHub

### Database Migrations
```bash
dotnet ef database update --startup-project src/Presentation/HireKarlo.Api \
  --project src/Infrastructure/HireKarlo.Persistence
```

---

## 🚀 Phase 3: Extensions & Multi-Channel Distribution

**Release**: v3.0.0  
**Status**: 📋 Planned  
**Target Date**: May 2025  

### Packages
- **VS Code Extension**: `vscode:market hirekarlo` (Visual Studio Marketplace)
- **Chrome Extension**: Chrome Web Store
- **Firefox Add-on**: Firefox Add-ons
- **NPM Package**: `npm install hirekarlo-sdk`
- **CLI**: `npm install -g hirekarlo-cli`

### Installation

**VS Code**:
```
Command Palette → Install Extensions → Search "HireKarlo"
```

**Chrome/Firefox**:
```
Visit store → Search "HireKarlo" → Add to Browser
```

**NPM**:
```bash
npm install hirekarlo-sdk
```

### Release Artifacts
- GitHub Release: Full source + docs
- VS Code Marketplace VSIX
- Chrome Web Store CRX
- Firefox Add-ons XPI
- NPM registry

---

## 📚 Phase 4: SDKs & Enterprise APIs

**Release**: v3.1.0  
**Status**: 📋 Planned  
**Target Date**: June 2025  

### Package Formats

#### NuGet (.NET SDK)
```bash
dotnet add package HireKarlo.Sdk
```

**Usage**:
```csharp
var client = new HireKarloClient("your-api-key");
var opportunities = await client.GetOpportunitiesAsync();
```

#### NPM (JavaScript SDK)
```bash
npm install hirekarlo-sdk
```

**Usage**:
```javascript
const client = new HireKarloClient("your-api-key");
const opportunities = await client.getOpportunities();
```

#### REST API (OpenAPI)
```
GET  /api/v1/opportunities
GET  /api/v1/dream-companies
POST /api/v1/referrals/score
GET  /api/v1/interviews/{companyId}/prep
```

Full docs: `/swagger` endpoint

---

## 📋 Release Checklist by Phase

### Pre-Release (Every Phase)
- [ ] All tests passing (`dotnet test`)
- [ ] Code review (2+ approvals)
- [ ] Security scan (GitHub DevSecOps)
- [ ] Performance tested (Lighthouse, stress test)
- [ ] Documentation complete
- [ ] CHANGELOG.md updated
- [ ] Version bumped in all projects

### Release (Publish Day)
- [ ] Git tag: `git tag v2.0.0`
- [ ] GitHub Release via Actions
- [ ] Docker build & push
- [ ] NuGet publish
- [ ] NPM publish
- [ ] VS Code Marketplace
- [ ] Browser extension stores
- [ ] Render/Azure deployment

### Post-Release (Day 1-7)
- [ ] Smoke tests in production
- [ ] Monitor error rates
- [ ] Customer feedback collection
- [ ] Hotfix channel ready
- [ ] Release notes published
- [ ] Social announcement (optional)

---

## 🔐 API Versioning Strategy

| Version | Status | Sunset Date |
|---------|--------|-------------|
| v1 | Deprecated | Dec 2025 |
| v2 | Current | - |
| v3 | Future | - |

**Backwards Compatibility**
- Additive changes always supported
- Breaking changes announce 6 months in advance
- Deprecated endpoints return `410 Gone` before removal

---

## 📊 Distribution Channels by Phase

```
v1.0 (Phase 1)
└─ Docker only

v2.0 (Phase 2)
├─ Docker
└─ REST API

v3.0 (Phase 3)
├─ Docker
├─ REST API
├─ VS Code Extension
├─ Chrome/Firefox Extension
└─ NPM CLI

v3.1 (Phase 4)
├─ All above
├─ NuGet SDK
└─ JavaScript SDK
```

---

## 💾 Database Schema Versions

| Version | Migration | Status | Date |
|---------|-----------|--------|------|
| 1.0 | Initial schema (resume, jobs, matches) | Complete | Jan 2025 |
| 2.0 | Dream companies, referrals, vectors | In Progress | Feb 2025 |
| 2.1 | Interview data aggregation | Planned | Mar 2025 |
| 3.0 | Multi-user/team support | Planned | May 2025 |

**Auto-migration on deploy**:
```bash
# Render / Azure will auto-run this on startup
dotnet ef database update
```

---

## 🎓 Recommended Upgrade Path

### Individual Users
```
v1.0 (try app) → v2.0 (use dashboard) → v3.0 (install extension)
```

### Enterprise/Teams
```
v1.0 → v2.0 → v3.1 (integrate SDK into internal tools)
```

### Developers
```
v1.0 → v2.0 → v3.1 (use NuGet + NPM SDKs for custom integrations)
```

---

## 📞 Support

- **Issues**: Report bugs at [GitHub Issues](https://github.com/K-riti/HireKarlo/issues)
- **Discussions**: Feature requests at [GitHub Discussions](https://github.com/K-riti/HireKarlo/discussions)
- **Security**: Report at security@hirekarlo.dev (future)

---

**Built for engineers. Released with intention.**
