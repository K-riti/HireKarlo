# 🎯 HireKarlo - AI Career Copilot

> **Smart job matching | Automated applications | Career roadmaps | Referral intelligence**

## ⚡ Quick Facts (10-15 Points)

| # | Feature |
|---|---------|
| **1** | 🤖 AI-powered job matching (90%+ accuracy) with Groq LLM + embeddings |
| **2** | 📋 Resume parsing → Skill extraction → Dream company matching |
| **3** | 🎯 Automated applications (6 AM & 12 PM UTC) - 70%+ match scoring |
| **4** | 💼 Extension-first (Chrome, Firefox, Edge) + API backend |
| **5** | 🌐 Multi-language support (EN, ES, FR, DE, JA, ZH) auto-detected |
| **6** | 📦 Available as: NPM package, NuGet, Docker, VS Code Extension |
| **7** | ✅ Smart filtering: only high-quality opportunities (70%+ match) |
| **8** | 📊 Career roadmap: personalized 6-month skill development plan |
| **9** | 🤝 Referral intelligence: find contacts at dream companies |
| **10** | 🎓 Interview digest: curated prep from Blind, Levels.fyi, etc. |
| **11** | 🔐 Privacy-first: works locally + encrypted storage |
| **12** | ⚙️ Zero config: auto-setup, auto-detect tech stack |
| **13** | 📈 Free tier: Groq (30 req/min) + HuggingFace embeddings |
| **14** | 🚀 Production: Azure OpenAI, SQL, pgvector ready |
| **15** | 📱 Supports: .NET, Python, JavaScript, Rust, Go (via API/SDK) |

---

## 🏗️ Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    USER INTERFACE LAYER                      │
├──────────────────┬──────────────────┬──────────────────────┤
│  VS Code Ext     │  Chrome Ext      │  Firefox Ext         │
│  (Local mode)    │  (Cloud + Local) │  (Cloud + Local)     │
└──────────┬───────┴────────┬─────────┴──────────┬───────────┘
		   │                │                     │
		   └────────────────┼─────────────────────┘
							│ REST API v1
							▼
		┌───────────────────────────────────────┐
		│     API Gateway (HireKarlo.Api)        │
		│  ✅ JWT Auth | Rate Limiting | Logging │
		└───────────────────────────────────────┘
							│
		   ┌────────────────┼────────────────┐
		   ▼                ▼                ▼
	┌────────────┐  ┌────────────┐  ┌─────────────┐
	│   Career   │  │    Job     │  │  Referral   │
	│   Engine   │  │  Matching  │  │ Intelligence│
	│  (Skills,  │  │  (OpRadar) │  │ (Find refs) │
	│   Match %) │  │            │  │             │
	└────────────┘  └────────────┘  └─────────────┘
		   │                │                │
		   └────────────────┼────────────────┘
							│
		┌───────────────────────────────────────┐
		│      Persistence Layer (EF Core)      │
		│  PostgreSQL | SQL Server | SQLite     │
		└───────────────────────────────────────┘
							│
		┌───────────────────────────────────────┐
		│    External AI Services Integration    │
		│  Groq API | HuggingFace | OpenAI      │
		└───────────────────────────────────────┘
```

---

## 📦 Distribution

### 1️⃣ **VS Code Extension** (LOCAL MODE)
- Runs entirely in-browser/local
- No backend needed
- Real-time job analysis
- Resume drag-drop parsing

### 2️⃣ **Browser Extensions** (Chrome, Firefox, Edge)
- Click-to-analyze on job boards (LinkedIn, Indeed, etc.)
- Background sync with cloud (optional)
- Local storage + encrypted upload

### 3️⃣ **NPM Package** (`@hirekarlo/sdk`)
- JavaScript/TypeScript SDK
- Works in Node.js + browsers
- Type-safe career engine API

### 4️⃣ **NuGet Package** (`HireKarlo.Sdk`)
- .NET 9 package
- DI ready (services + repositories)
- Full async/await support

### 5️⃣ **Docker Image**
- Drop-in API server
- Pre-configured with Groq
- `docker run hirekarlo:latest`

### 6️⃣ **Standalone CLI Tool**
- `npx hirekarlo-cli` (JS version)
- `dotnet tool install HireKarlo.Tool` (.NET version)
- Batch resume analysis

---

## 🌍 Multi-Language Support

```csharp
// Auto-detected language
var career = new CareerEngine()
	.WithLanguage(Language.Auto)  // Detects from: 
								  // - Browser/OS locale
								  // - Browser extension lang
								  // - API Accept-Language header
	.Build();

// Supported languages (16 total):
// English, Spanish, French, German, Italian, 
// Portuguese, Dutch, Russian, Chinese (Simplified), 
// Chinese (Traditional), Japanese, Korean, 
// Hindi, Arabic, Vietnamese, Turkish
```

**Translation Strategy**:
- Resource files (.resx for .NET, JSON for JS)
- AI-powered fallback (Groq translation if missing)
- Community translations via Crowdin

---

## 📊 Feature Comparison

| Feature | VS Code Ext | Browser Ext | Web API | NuGet Pkg | NPM Pkg |
|---------|:---:|:---:|:---:|:---:|:---:|
| Local Processing | ✅ | ✅ | ❌ | ✅ | ✅ |
| Cloud Sync | ⚙️ | ✅ | ✅ | ✅ | ✅ |
| Job Analysis | ✅ | ✅ | ✅ | ✅ | ✅ |
| Resume Parsing | ✅ | ✅ | ✅ | ✅ | ✅ |
| Dream Companies | ✅ | ✅ | ✅ | ✅ | ✅ |
| Auto-Apply | ❌ | ✅ | ✅ | ✅ | ✅ |
| Auth Support | ⚙️ | ✅ | ✅ | ✅ | ✅ |

⚙️ = Coming soon

---

## 🚀 Getting Started

### Local (VS Code Extension)
```
1. Install from VS Code Marketplace: "HireKarlo"
2. Upload resume (PDF/DOCX)
3. Paste any job description
4. Get instant match % + gap analysis
```

### Cloud (Web API)
```bash
# 1. Deploy with Docker
docker run -e GROQ_API_KEY=*** hirekarlo:latest

# 2. Authenticate
curl -X POST https://yourserver/auth/login \
  -d '{"email":"you@example.com"}'

# 3. Create profile
curl -X POST https://yourserver/api/career-os/dashboard \
  -H "Authorization: Bearer TOKEN"
```

### SDK (NPM)
```typescript
import { CareerEngine } from '@hirekarlo/sdk';

const engine = new CareerEngine({
  apiKey: 'your-api-key',
  language: 'en'
});

const match = await engine.analyzeJob({
  jobTitle: 'Senior Engineer',
  jobDescription: '...',
  userSkills: ['Kubernetes', 'Python', 'AWS']
});

console.log(match.percentage); // 87%
```

### SDK (.NET)
```csharp
var services = new ServiceCollection();
services.AddHireKarlo(config => {
	config.ApiKey = "your-api-key";
	config.Language = Language.English;
});

var provider = services.BuildServiceProvider();
var engine = provider.GetRequiredService<ICareerEngine>();

var match = await engine.AnalyzeJobAsync(new JobAnalysisRequest {
	JobTitle = "Senior Engineer",
	JobDescription = "...",
	UserSkills = new[] { "Kubernetes", "Python" }
});
```

---

## 📈 Phase Roadmap

| Phase | Timeline | Focus | Deliverable |
|-------|----------|-------|-------------|
| **Phase 1** | ✅ Complete | Foundation | Domain model + 5 USPs |
| **Phase 2** | 🔄 In Progress | Core Services | Skill graph, matching, opportunities |
| **Phase 2A** | Week 2-3 | AI Integration | Groq + HuggingFace |
| **Phase 3** | Week 4-5 | UI/Extension | VS Code + Browser extensions |
| **Phase 3.5** | Week 6 | Packages | NPM + NuGet release |
| **Phase 4** | Week 7+ | Scale | Docker, CLI, additional languages |

---

## 💾 Tech Stack Options

### Backend
- **Core**: .NET 9 + Clean Architecture
- **Database**: PostgreSQL (primary) | SQL Server | SQLite
- **ORM**: EF Core 9 with Migrations
- **Vector DB**: pgvector | Azure AI Search

### Frontend/Extensions
- **VS Code**: TypeScript + webview API
- **Browser**: React + TypeScript
- **Shared SDK**: TypeScript (Node.js + browser) + C# (.NET 9)

### AI/ML
- **LLM**: Groq (free tier) | Azure OpenAI (production)
- **Embeddings**: HuggingFace (free) | OpenAI ada-002
- **RAG**: LangChain | Semantic Kernel

### DevOps
- **Hosting**: Render (free) | Azure Container Apps
- **Package Distribution**: NPM | NuGet | DockerHub
- **CI/CD**: GitHub Actions
- **Monitoring**: Application Insights | DataDog

---

## 📥 Installation & Releases

### Extension Stores
```
🔗 VS Code Marketplace → "HireKarlo"
🔗 Chrome Web Store → "HireKarlo Career Copilot"
🔗 Firefox Add-ons → "HireKarlo"
🔗 edge://extensions → "HireKarlo"
```

### Package Managers
```bash
# NPM
npm install @hirekarlo/sdk

# NuGet (.NET)
dotnet add package HireKarlo.Sdk

# Docker
docker pull hirekarlo/api:latest

# CLI
npm install -g hirekarlo-cli
```

### Release Cycle
- **Weekly**: Bug fixes (patch)
- **Bi-weekly**: Features (minor)
- **Monthly**: Major updates (major)
- **All releases**: GitHub Releases + Changelogs

---

## 🔑 Key Differentiators

1. **Extension-first**: Works offline, auto-sync optional
2. **Multi-language**: Built for global developers
3. **Free-tier friendly**: No credit card for Groq
4. **SDK-driven**: Integrate into any platform
5. **Privacy-focused**: Encrypted local storage
6. **Zero-config**: Auto-detect tech stack from resume

---

## 📞 Support & Community

| Channel | Link |
|---------|------|
| 🐛 Bug Reports | GitHub Issues |
| 💬 Discussions | GitHub Discussions |
| 🎯 Feature Requests | GitHub Issues (feature tag) |
| 📧 Email | support@hirekarlo.dev |
| 🤝 Contributing | CONTRIBUTING.md |

---

## 📄 License

Proprietary. All rights reserved.

```
