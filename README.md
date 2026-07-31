# HireKarlo — AI-Powered Career Platform

An AI career copilot that analyzes your resume against target JDs, tracks jobs, **drafts tailored applications for your review** (never auto-submits), builds a personalized 6-month roadmap, and keeps you connected to dream companies via referrals and interview-experience digests.

> **📦 Project Status: Complete Prototype (v1.0)**  
> This repository is a **finished, working prototype** demonstrating the full architecture. Core AI features are implemented; some external integrations are scaffolded for future API keys. See [Implementation Status](#-implementation-status) for details.

---

## 🔧 Tech Stack (Current)

The codebase supports **two AI configurations**:

### Option A: Free Tier (Default for Render deployment)
| Component | Technology | Cost |
|-----------|------------|------|
| **LLM** | Groq (Llama 3.3 70B) | FREE - 30 req/min |
| **Embeddings** | HuggingFace Inference API | FREE |
| **Vector Store** | In-Memory (InMemoryVectorStore) | FREE |
| **Database** | PostgreSQL (Render) | FREE - 256MB |
| **Hosting** | Render | FREE |

### Option B: Azure (Production)
| Component | Technology | Cost |
|-----------|------------|------|
| **LLM** | Azure OpenAI (GPT-4) | Pay-per-use |
| **Embeddings** | Azure OpenAI (ada-002) | Pay-per-use |
| **Vector Store** | Azure AI Search | Free tier available |
| **Database** | Azure SQL | Pay-per-use |
| **Hosting** | Azure App Service | Free tier available |

The code auto-detects which to use based on configuration.

---

## ✅ Implementation Status

### Controllers (API Endpoints) - All Have Real Code

| Controller | Endpoints | Lines | Status |
|------------|-----------|-------|--------|
| `AuthController` | Register, Login, Google, LinkedIn OAuth | ~200 | ✅ Implemented |
| `ResumesController` | Upload, Parse, ATS Score, Tailor | ~180 | ✅ Implemented |
| `JobsController` | Search, Match, Details | ~120 | ✅ Implemented |
| `ApplicationsController` | Kanban board, CRUD, Stage updates | 165 | ✅ Implemented |
| `DreamCompaniesController` | CRUD, Priority, Job tracking toggle | 270 | ✅ Implemented |
| `ContactsController` | CRUD, AI-drafted referral messages, Follow-ups | 320 | ✅ Implemented |
| `MockInterviewController` | Start, Answer, Feedback | ~150 | ✅ Implemented |
| `LearningPathController` | Company/Skill/Interview paths, Quizzes | ~180 | ✅ Implemented |
| `ChatController` | AI career assistant chat | ~80 | ✅ Implemented |
| `AdvancedAIController` | Outcome prediction, Explainable ATS, Trajectory | ~200 | ✅ Implemented |
| `LinkedInOptimizerController` | Profile optimization | ~100 | ✅ Implemented |
| `NewsletterController` | Subscribe, Unsubscribe | ~60 | ⚠️ Partial (see below) |

### AI Services - Real Implementations

| Service | File | Lines | What It Does |
|---------|------|-------|--------------|
| `RAGOrchestrator` | RAGOrchestrator.cs | 277 | Match reports, Interview digests, Project recommendations |
| `GroqService` | GroqService.cs | 200 | LLM completions via Groq (free) with **rate-limit retry + exponential backoff** |
| `HuggingFaceEmbeddingService` | HuggingFaceEmbeddingService.cs | 80 | Embeddings via HuggingFace (free) |
| `PostgresVectorStore` | PostgresVectorStore.cs | 160 | **Persistent** vector search (survives Render cold starts) |
| `InMemoryVectorStore` | InMemoryVectorStore.cs | 100 | In-memory vector search (fallback) |
| `AzureOpenAIService` | AzureOpenAIService.cs | ~150 | LLM via Azure (paid option) |
| `AdvancedAIService` | AdvancedAIService.cs | ~400 | Outcome prediction, Keyword radar, Trajectory |

### Scaffolded (Ready for API Keys)

| Feature | Status | Notes |
|---------|--------|-------|
| Newsletter digest generation | ⚠️ Subscribe/unsubscribe only | Scheduler scaffolded in `JobIngestionFunction.cs` |
| Real email sending | ⚠️ Logging only | `EmailService.cs` ready for SendGrid key |
| Job data ingestion | ⚠️ Timer exists | `JobIngestionFunction.cs` ready for Adzuna key |
| Greenhouse/Lever job ingestion | ⚠️ Fields exist | Entity has `GreenhouseBoardToken`, `LeverCompanyId` |

### Out of Scope (v1)

| Feature | Status |
|---------|--------|
| Mobile app (MAUI) | 🚫 Not planned |
| Push notifications | 🚫 Not planned |
| Chrome extension | 🚫 Not planned |

> **Note on Contacts**: Contacts/referrals are manually entered by the user. We deliberately do NOT pull from LinkedIn's connections API to avoid compliance concerns.

---

## 🏗️ Architecture

```
HireKarlo/
├── src/
│   ├── Core/
│   │   ├── HireKarlo.Domain/           # Entities (User, Resume, Job, Match, Contact, etc.)
│   │   ├── HireKarlo.Application/      # Interfaces (IOpenAIService, IEmbeddingService, etc.)
│   │   └── HireKarlo.Shared/           # Shared DTOs
│   │
│   ├── Infrastructure/
│   │   ├── HireKarlo.Infrastructure/
│   │   │   ├── AI/
│   │   │   │   ├── GroqService.cs              # FREE: Llama 3.3 70B + rate-limit backoff
│   │   │   │   ├── HuggingFaceEmbeddingService.cs  # FREE: Embeddings
│   │   │   │   ├── InMemoryVectorStore.cs      # FREE: Vector search (non-persistent)
│   │   │   │   ├── AzureOpenAIService.cs       # PAID: GPT-4 (optional)
│   │   │   │   ├── RAGOrchestrator.cs          # Core RAG logic
│   │   │   │   └── AdvancedAIService.cs        # Advanced AI features
│   │   │   ├── Auth/AuthService.cs             # JWT + Google/LinkedIn OAuth
│   │   │   └── Services/                       # Learning, MockInterview, LinkedIn, Email
│   │   │
│   │   └── HireKarlo.Persistence/
│   │       ├── HireKarloDbContext.cs           # EF Core (SQL Server or PostgreSQL)
│   │       ├── PostgresVectorStore.cs          # FREE: Persistent vector search (survives cold starts)
│   │       └── Repositories/                    # All repository implementations
│   │
│   ├── Presentation/
│   │   ├── HireKarlo.Api/                      # ASP.NET Core API (14 controllers)
│   │   └── HireKarlo.Web/                      # Blazor Web App (PWA ready)
│   │
│   ├── Services/
│   │   ├── HireKarlo.AtsEngine/                # ATS scoring logic
│   │   └── HireKarlo.ResumeService/            # PDF/DOCX parsing + generation
│   │
│   └── Functions/
│       └── HireKarlo.JobIngestion/             # Azure Functions (job fetch timer)
│
└── render.yaml                                  # Render deployment config
```

---

## 🚀 Quick Start

### Option 1: Run Locally (Free)

```bash
# 1. Clone
git clone https://github.com/K-riti/HireKarlo.git
cd HireKarlo

# 2. Get free API keys
# - Groq: https://console.groq.com (free)
# - HuggingFace: https://huggingface.co/settings/tokens (free)

# 3. Configure (create appsettings.Development.json in src/Presentation/HireKarlo.Api/)
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HireKarlo;Trusted_Connection=True;"
  },
  "Groq": {
	"ApiKey": "gsk_your_key_here",
	"Model": "llama-3.3-70b-versatile"
  },
  "HuggingFace": {
	"ApiKey": "hf_your_key_here"
  }
}

# 4. Run
cd src/Presentation/HireKarlo.Api
dotnet ef database update
dotnet run
```

### Option 2: Deploy to Render (Free)

1. Fork this repo
2. Go to [render.com](https://render.com) → New → Blueprint
3. Select your fork → Render detects `render.yaml`
4. Add environment variables:
   - `Groq__ApiKey`: Your Groq key
   - `HuggingFace__ApiKey`: Your HuggingFace key
5. Deploy!

Your URLs:
- Web: `https://hirekarlo-web.onrender.com`
- API: `https://hirekarlo-api.onrender.com`

---

## 📚 API Reference

### Authentication
```
POST /api/auth/register         # Email/password signup
POST /api/auth/login            # Email/password login
POST /api/auth/google           # Google OAuth
POST /api/auth/linkedin         # LinkedIn OAuth
```

### Resumes & ATS
```
POST /api/resumes/upload        # Upload resume
POST /api/resumes/{id}/ats-score    # Get ATS score with explanation
POST /api/resumes/{id}/tailor   # Generate tailored version for a JD
```

### Job Tracking
```
GET  /api/applications          # List all applications
GET  /api/applications/kanban   # Get kanban board view
POST /api/applications          # Create application
POST /api/applications/{id}/draft   # AI-draft cover letter + application message (for review)
PATCH /api/applications/{id}/stage  # Update stage (Applied → Interview → Offer)
```

### Dream Companies & Contacts
```
GET  /api/dreamcompanies        # List dream companies
POST /api/dreamcompanies        # Add company to track
GET  /api/contacts              # List contacts
POST /api/contacts/{id}/draft-message   # AI-generate referral message
```

### Advanced AI
```
POST /api/advancedai/predict-outcome    # Predict application success
POST /api/advancedai/explainable-ats    # Detailed ATS breakdown
POST /api/advancedai/keyword-radar      # Keyword gap analysis
POST /api/advancedai/skill-trajectory   # "Learn X → Y% match increase"
POST /api/advancedai/career-roadmap     # 6-month plan
```

### Mock Interview
```
POST /api/mockinterview/start   # Start interview session
POST /api/mockinterview/answer  # Submit answer for evaluation
GET  /api/mockinterview/feedback    # Get STAR-method feedback
```

---

## ⚠️ Known Limitations

| Area | Status | Notes |
|------|--------|-------|
| **Groq rate limits** | ✅ Handled | Exponential backoff with retry (30 req/min free tier) |
| **Vector store persistence** | ✅ Solved | PostgresVectorStore survives Render cold starts |
| **Job data** | ⚠️ Scaffolded | `JobIngestionFunction.cs` exists; needs Adzuna API key to fetch real jobs |
| **Email sending** | ⚠️ Scaffolded | `EmailService.cs` logs only; needs SendGrid API key for real emails |
| **Newsletter digest** | ⚠️ Partial | Subscribe/unsubscribe works; no scheduled job sends content |

### Required Environment Variables (Render Dashboard)

To make this fully functional, ensure these are set in your Render service:

| Variable | Required | Source |
|----------|----------|--------|
| `Groq__ApiKey` | ✅ Yes | [console.groq.com](https://console.groq.com) (free) |
| `HuggingFace__ApiKey` | ✅ Yes | [huggingface.co/settings/tokens](https://huggingface.co/settings/tokens) (free) |
| `SendGrid__ApiKey` | Optional | For real email sending |
| `Adzuna__AppId` / `Adzuna__ApiKey` | Optional | For real job data ingestion |

---

## 🗺️ Roadmap

### ✅ Completed (v1.0)
- [x] Rate-limit retry logic for Groq with exponential backoff
- [x] PostgresVectorStore for persistent embeddings (survives cold starts)
- [x] Full API implementation (14 controllers, 60+ endpoints)
- [x] Blazor WebAssembly frontend with Kanban board
- [x] OAuth integration (Google, LinkedIn)
- [x] RAG-based job matching and resume tailoring
- [x] Mock interview with STAR-method feedback
- [x] Advanced AI features (outcome prediction, skill trajectory)

### 🚫 Not Planned (Out of Scope for v1)
The following were considered but are **intentionally excluded** from this prototype:

| Feature | Reason |
|---------|--------|
| Newsletter digest scheduler | Requires paid email service + cron infrastructure |
| Mobile app (MAUI) | Separate project scope |
| Push notifications | Requires additional infrastructure |
| Greenhouse/Lever integration | Requires enterprise API access |
| Chrome extension | Separate project scope |

> **Note**: The scaffolding for job ingestion (`JobIngestionFunction.cs`) and email (`EmailService.cs`) exists if you want to extend this yourself with API keys.

---

## 📄 License

Proprietary. All rights reserved.
