# HireKarlo — AI-Powered Career Platform

An AI career copilot that analyzes your resume against target JDs, tracks jobs, auto-drafts tailored applications, builds a personalized 6-month roadmap, and keeps you connected to dream companies via referrals and interview-experience digests.

> **⚠️ Honest Status**: This is a working prototype with real code. See [Implementation Status](#-implementation-status) for exactly what's built vs. planned.

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
| `NewsletterController` | Subscribe, Unsubscribe | ~60 | ✅ Implemented |

### AI Services - Real Implementations

| Service | File | Lines | What It Does |
|---------|------|-------|--------------|
| `RAGOrchestrator` | RAGOrchestrator.cs | 277 | Match reports, Interview digests, Project recommendations |
| `GroqService` | GroqService.cs | 160 | LLM completions via Groq (free) |
| `HuggingFaceEmbeddingService` | HuggingFaceEmbeddingService.cs | 80 | Embeddings via HuggingFace (free) |
| `InMemoryVectorStore` | InMemoryVectorStore.cs | 100 | Vector similarity search (free) |
| `AzureOpenAIService` | AzureOpenAIService.cs | ~150 | LLM via Azure (paid option) |
| `AdvancedAIService` | AdvancedAIService.cs | ~400 | Outcome prediction, Keyword radar, Trajectory |

### What's NOT Implemented Yet

| Feature | Status | Notes |
|---------|--------|-------|
| Mobile app (MAUI) | ❌ Not started | Future work |
| Push notifications | ❌ Not started | Future work |
| Greenhouse/Lever job ingestion | ⚠️ Fields exist, API not wired | Entity has `GreenhouseBoardToken`, `LeverCompanyId` |
| Real email sending | ⚠️ Service exists, needs SendGrid key | `EmailService.cs` implemented |

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
│   │   │   │   ├── GroqService.cs              # FREE: Llama 3.3 70B
│   │   │   │   ├── HuggingFaceEmbeddingService.cs  # FREE: Embeddings
│   │   │   │   ├── InMemoryVectorStore.cs      # FREE: Vector search
│   │   │   │   ├── AzureOpenAIService.cs       # PAID: GPT-4 (optional)
│   │   │   │   ├── RAGOrchestrator.cs          # Core RAG logic
│   │   │   │   └── AdvancedAIService.cs        # Advanced AI features
│   │   │   ├── Auth/AuthService.cs             # JWT + Google/LinkedIn OAuth
│   │   │   └── Services/                       # Learning, MockInterview, LinkedIn, Email
│   │   │
│   │   └── HireKarlo.Persistence/
│   │       ├── HireKarloDbContext.cs           # EF Core (SQL Server or PostgreSQL)
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

1. **Groq rate limits**: Free tier = 30 requests/minute. No backoff retry implemented yet.
2. **In-memory vector store**: Resets on restart. For production, consider PostgreSQL + pgvector.
3. **No real job data**: Job ingestion function exists but needs Adzuna API key to fetch real jobs.
4. **No email sending**: EmailService scaffold exists, needs SendGrid API key.

---

## 🗺️ Roadmap

- [ ] Add rate-limit retry logic for Groq
- [ ] Wire up Greenhouse/Lever job APIs
- [ ] Add pgvector for persistent vector store
- [ ] Mobile app (MAUI Blazor Hybrid)
- [ ] Push notifications
- [ ] Chrome extension for one-click apply

---

## 📄 License

Proprietary. All rights reserved.
