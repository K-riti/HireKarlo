# HireKarlo — AI-Powered Career Platform

An AI career copilot that analyzes your resume against target JDs, tracks a daily-refreshed feed of 90%+ matched jobs, auto-drafts tailored applications, builds a personalized 6-month roadmap, and keeps you plugged into your dream companies via referrals and interview-experience digests.

---

## 🏗️ System Architecture

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                              HireKarlo Platform                                   │
├─────────────────────────────────────────────────────────────────────────────────┤
│                                                                                   │
│  ┌────────────────────────┐      ┌─────────────────────────────────────────┐     │
│  │   PRESENTATION LAYER   │      │           CORE DOMAIN LAYER             │     │
│  │                        │      │                                         │     │
│  │  ┌──────────────────┐  │      │  ┌─────────────┐  ┌─────────────────┐   │     │
│  │  │  Blazor Web App  │  │      │  │   Entities  │  │  Value Objects  │   │     │
│  │  │    (PWA Ready)   │  │      │  │  • User     │  │  • MatchReport  │   │     │
│  │  │  • Dashboard     │  │      │  │  • Resume   │  │  • GapAnalysis  │   │     │
│  │  │  • Resumes       │  │      │  │  • Job      │  │  • AtsScore     │   │     │
│  │  │  • Jobs          │◄────────►  │  • Match    │  │  • Skills       │   │     │
│  │  │  • Mock Interview│  │      │  │  • Contact  │  │  • Roadmap      │   │     │
│  │  │  • Learning      │  │      │  │  • Roadmap  │  │  • Interview    │   │     │
│  │  │  • AI Chat       │  │      │  │  • Digest   │  │    Questions    │   │     │
│  │  │  • LinkedIn      │  │      │  └─────────────┘  └─────────────────┘   │     │
│  │  └──────────────────┘  │      │                                         │     │
│  │                        │      │  ┌─────────────────────────────────────┐│     │
│  │  ┌──────────────────┐  │      │  │      APPLICATION INTERFACES        ││     │
│  │  │   REST API       │  │      │  │  • IOpenAIService                  ││     │
│  │  │  (ASP.NET Core)  │  │      │  │  • IEmbeddingService               ││     │
│  │  │  • AuthController│  │      │  │  • IVectorStoreService             ││     │
│  │  │  • ResumeController        │  │  • IResumeParser/Generator         ││     │
│  │  │  • JobController │  │      │  │  • IAtsScorer                      ││     │
│  │  │  • AdvancedAI    │  │      │  │  • ILearningPathService            ││     │
│  │  │  • ChatController│  │      │  │  • IMockInterviewService           ││     │
│  │  │  • LearnController         │  │  • ILinkedInOptimizer              ││     │
│  │  └──────────────────┘  │      │  └─────────────────────────────────────┘│     │
│  └────────────────────────┘      └─────────────────────────────────────────┘     │
│                                                                                   │
├─────────────────────────────────────────────────────────────────────────────────┤
│                             INFRASTRUCTURE LAYER                                  │
│  ┌─────────────────────────────────────────────────────────────────────────────┐ │
│  │                           AI / RAG Services                                  │ │
│  │  ┌───────────────────┐  ┌──────────────────┐  ┌───────────────────────────┐ │ │
│  │  │  Azure OpenAI     │  │  Embedding       │  │   Azure AI Search         │ │ │
│  │  │  (GPT-4/GPT-4o)   │  │  Service         │  │   (Vector Store)          │ │ │
│  │  │  • Completions    │  │  • ada-002       │  │   • Semantic Search       │ │ │
│  │  │  • JSON Mode      │  │  • Vectorization │  │   • HNSW Index            │ │ │
│  │  │  • Streaming      │  │  • Similarity    │  │   • Hybrid Retrieval      │ │ │
│  │  └───────────────────┘  └──────────────────┘  └───────────────────────────┘ │ │
│  │                                                                              │ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐│ │
│  │  │                         RAG Orchestrator                                 ││ │
│  │  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐  ││ │
│  │  │  │ Semantic Match  │  │ Interview Digest│  │   Project Recommender   │  ││ │
│  │  │  │ • Resume→JD     │  │ • RAG Retrieval │  │   • Skill Gap → Project │  ││ │
│  │  │  │ • Gap Analysis  │  │ • LLM Summary   │  │   • Personalized Ideas  │  ││ │
│  │  │  │ • LLM Reasoning │  │ • Grounding     │  │   • Impact Scoring      │  ││ │
│  │  │  └─────────────────┘  └─────────────────┘  └─────────────────────────┘  ││ │
│  │  └─────────────────────────────────────────────────────────────────────────┘│ │
│  │                                                                              │ │
│  │  ┌─────────────────────────────────────────────────────────────────────────┐│ │
│  │  │                      Advanced AI Service                                 ││ │
│  │  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐  ││ │
│  │  │  │ Outcome         │  │ Explainable ATS │  │ Trajectory Simulator    │  ││ │
│  │  │  │ Predictor       │  │ • Why scores    │  │ • Month-by-month plan   │  ││ │
│  │  │  │ • History-based │  │ • Issue fixes   │  │ • Milestone tracking    │  ││ │
│  │  │  │ • Pattern learn │  │ • Quick wins    │  │ • Risk analysis         │  ││ │
│  │  │  └─────────────────┘  └─────────────────┘  └─────────────────────────┘  ││ │
│  │  │  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────────────┐  ││ │
│  │  │  │ Keyword Radar   │  │ Career Roadmap  │  │ Resume Tailoring        │  ││ │
│  │  │  │ • Trend analysis│  │ • 6-month plan  │  │ • JD-specific rewrites  │  ││ │
│  │  │  │ • Coverage %    │  │ • Weekly sched  │  │ • Before/after score    │  ││ │
│  │  │  │ • Actions       │  │ • Resources     │  │ • Keywords added        │  ││ │
│  │  │  └─────────────────┘  └─────────────────┘  └─────────────────────────┘  ││ │
│  │  └─────────────────────────────────────────────────────────────────────────┘│ │
│  └─────────────────────────────────────────────────────────────────────────────┘ │
│                                                                                   │
│  ┌───────────────────┐  ┌──────────────────┐  ┌──────────────────────────────┐   │
│  │   Persistence     │  │  External APIs   │  │    Azure Functions           │   │
│  │  • Azure SQL      │  │  • Adzuna Jobs   │  │    (Job Ingestion)           │   │
│  │  • EF Core        │  │  • SendGrid      │  │    • Timer Triggers          │   │
│  │  • Repositories   │  │  • Blob Storage  │  │    • Daily Job Refresh       │   │
│  └───────────────────┘  └──────────────────┘  └──────────────────────────────┘   │
│                                                                                   │
└─────────────────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Data Flow Diagrams

### Resume Analysis & ATS Scoring Flow

```
┌──────────┐    ┌───────────────┐    ┌──────────────┐    ┌──────────────┐
│  Upload  │───►│ Resume Parser │───►│ ATS Scorer   │───►│ Match Report │
│  Resume  │    │ (PDF/DOCX)    │    │              │    │              │
└──────────┘    └───────────────┘    └──────────────┘    └──────────────┘
					   │                     │                  │
					   ▼                     ▼                  ▼
			   ┌───────────────┐    ┌──────────────┐    ┌──────────────┐
			   │ Extract Text  │    │ Keyword      │    │ Gap Analysis │
			   │ & Structure   │    │ Extraction   │    │ • Missing    │
			   └───────────────┘    │ via LLM      │    │ • Matching   │
					   │            └──────────────┘    │ • Suggestions│
					   ▼                   │            └──────────────┘
			   ┌───────────────┐           │
			   │ Generate      │◄──────────┘
			   │ Embeddings    │
			   │ (ada-002)     │
			   └───────────────┘
					   │
					   ▼
			   ┌───────────────┐
			   │ Store in      │
			   │ Vector Index  │
			   └───────────────┘
```

### RAG-Powered Interview Question Generation

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        RAG Interview Question Flow                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. QUERY FORMULATION                 2. VECTOR RETRIEVAL                   │
│  ┌──────────────────────┐             ┌──────────────────────┐              │
│  │ User: "Google SWE    │────────────►│ Azure AI Search      │              │
│  │ behavioral questions"│             │ • Embed query        │              │
│  └──────────────────────┘             │ • HNSW search        │              │
│                                       │ • Top-K results      │              │
│                                       └──────────────────────┘              │
│                                                  │                           │
│                                                  ▼                           │
│  3. CONTEXT RETRIEVAL                 ┌──────────────────────┐              │
│  ┌──────────────────────┐             │ Retrieved Documents  │              │
│  │ Reddit experiences   │◄────────────│ • Score > 0.5        │              │
│  │ LeetCode discussion  │             │ • Ranked by relevance│              │
│  │ Glassdoor reviews    │             └──────────────────────┘              │
│  │ (via Bing Search API)│                                                   │
│  └──────────────────────┘                                                   │
│           │                                                                  │
│           ▼                                                                  │
│  4. GROUNDED GENERATION               5. OUTPUT                             │
│  ┌──────────────────────┐             ┌──────────────────────┐              │
│  │ Azure OpenAI (GPT-4) │────────────►│ Questions with:      │              │
│  │ • System prompt with │             │ • Source attribution │              │
│  │   retrieved context  │             │ • Difficulty level   │              │
│  │ • "Generate questions│             │ • Sample answers     │              │
│  │   ONLY from context" │             │ • Follow-up tips     │              │
│  │ • Grounding check    │             │ • IsGrounded flag    │              │
│  └──────────────────────┘             └──────────────────────┘              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Application Outcome Prediction Flow

```
┌──────────────────┐    ┌──────────────────┐    ┌──────────────────────────┐
│ User's Historical│───►│ Pattern Analysis │───►│ Prediction Model         │
│ Applications     │    │                  │    │                          │
│ • Interview rate │    │ • Interview %    │    │ ┌──────────────────────┐ │
│ • Offer rate     │    │ • Offer %        │    │ │ Current Application  │ │
│ • Response times │    │ • Avg response   │    │ │ • Resume text        │ │
│ • Job types      │    │ • Success types  │    │ │ • JD text            │ │
│ • Outcomes       │    │ • Weak areas     │    │ │ • Semantic score     │ │
└──────────────────┘    └──────────────────┘    │ └──────────────────────┘ │
												│            │             │
												│            ▼             │
												│ ┌──────────────────────┐ │
												│ │ LLM Reasoning        │ │
												│ │ (Azure OpenAI)       │ │
												│ └──────────────────────┘ │
												│            │             │
												│            ▼             │
												│ ┌──────────────────────┐ │
												│ │ Prediction Output    │ │
												│ │ • Success probability│ │
												│ │ • Predicted outcome  │ │
												│ │ • Key factors        │ │
												│ │ • Risk factors       │ │
												│ │ • Improvement tips   │ │
												│ └──────────────────────┘ │
												└──────────────────────────┘
```

---

## ✅ Implementation Status

### Pure LLM Features (Azure OpenAI, no retrieval)

| Feature | Status | API Endpoint |
|---------|--------|--------------|
| Resume Tailoring per JD | ✅ Implemented | `POST /api/advancedai/tailor-resume` |
| ATS Score Explainability | ✅ Implemented | `POST /api/advancedai/explainable-ats` |
| 6-Month Career Roadmap | ✅ Implemented | `POST /api/advancedai/career-roadmap` |
| LinkedIn Profile Rewrite | ✅ Implemented | `POST /api/linkedin/optimize` |
| Mock Interview Questions | ✅ Implemented | `POST /api/mockinterview/questions` |
| Mock Interview Feedback | ✅ Implemented | `POST /api/mockinterview/evaluate` |
| AI Career Chat Assistant | ✅ Implemented | `POST /api/chat/send` |
| Skill Trajectory Simulation | ✅ Implemented | `POST /api/advancedai/skill-trajectory` |
| Keyword Radar Analysis | ✅ Implemented | `POST /api/advancedai/keyword-radar` |

### RAG Features (Retrieval + Generation)

| Feature | Status | Description |
|---------|--------|-------------|
| Resume-JD Semantic Matching | ✅ Implemented | Embed both → retrieve → LLM reasons over context |
| Interview Experience Digest | ✅ Implemented | Retrieve relevant posts → LLM summarizes grounded content |
| Project Gap Recommender | ✅ Implemented | Retrieve project ideas → LLM matches to user's gaps |
| Contextual Interview Questions | ✅ Implemented | Retrieve real questions → LLM generates similar grounded questions |

### Infrastructure Components

| Component | Status | Technology |
|-----------|--------|------------|
| Vector Store Setup | ✅ Configured | Azure AI Search with HNSW index |
| Embedding Pipeline | ✅ Implemented | `EmbeddingService` using text-embedding-ada-002 |
| Azure OpenAI Integration | ✅ Implemented | `AzureOpenAIService` with GPT-4 |
| RAG Orchestration Layer | ✅ Implemented | `RAGOrchestrator` class |

### Differentiating AI Features

| Feature | Status | Unique Value |
|---------|--------|--------------|
| Application Outcome Predictor | ✅ Implemented | Trains on YOUR history—nobody else does this |
| Explainable ATS Score | ✅ Implemented | Shows WHY, not just the number |
| Keyword Radar | ✅ Implemented | Visual gap chart with prioritized actions |
| Skill Trajectory Simulator | ✅ Implemented | "Learn X → match rate increases Y%" |
| Project Gap Recommender | ✅ Implemented | Personalized project suggestions to close gaps |

---

## 📁 Project Structure

```
HireKarlo/
├── src/
│   ├── Core/
│   │   ├── HireKarlo.Domain/           # Entities, Value Objects
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   ├── Resume.cs
│   │   │   │   ├── JobListing.cs
│   │   │   │   ├── Match.cs
│   │   │   │   ├── Application.cs
│   │   │   │   ├── LearningPath.cs
│   │   │   │   └── InterviewDigestEntry.cs
│   │   │   └── ValueObjects/
│   │   │       ├── MatchReport.cs
│   │   │       └── GapAnalysis.cs
│   │   │
│   │   ├── HireKarlo.Application/      # Interfaces, DTOs
│   │   │   └── Interfaces/
│   │   │       ├── AI/
│   │   │       │   └── IAIServices.cs  # IOpenAIService, IEmbeddingService, IVectorStoreService
│   │   │       ├── Services/
│   │   │       │   ├── IResumeService.cs
│   │   │       │   ├── ILearningPathService.cs
│   │   │       │   ├── IMockInterviewService.cs
│   │   │       │   └── ILinkedInOptimizerService.cs
│   │   │       └── Repositories/
│   │   │
│   │   └── HireKarlo.Shared/           # Cross-cutting DTOs
│   │
│   ├── Infrastructure/
│   │   ├── HireKarlo.Infrastructure/
│   │   │   ├── AI/
│   │   │   │   ├── AzureOpenAIService.cs    # GPT-4 completions
│   │   │   │   ├── EmbeddingService.cs      # ada-002 embeddings
│   │   │   │   ├── AzureAISearchService.cs  # Vector store
│   │   │   │   ├── RAGOrchestrator.cs       # RAG pipeline
│   │   │   │   └── AdvancedAIService.cs     # Advanced AI features
│   │   │   ├── Auth/
│   │   │   │   └── AuthService.cs           # Google/LinkedIn OAuth
│   │   │   ├── Services/
│   │   │   │   ├── LearningPathService.cs
│   │   │   │   ├── MockInterviewService.cs
│   │   │   │   ├── LinkedInOptimizerService.cs
│   │   │   │   └── EmailDigestService.cs
│   │   │   └── External/
│   │   │       ├── JobFetchService.cs       # Adzuna integration
│   │   │       └── EmailService.cs          # SendGrid
│   │   │
│   │   └── HireKarlo.Persistence/
│   │       ├── HireKarloDbContext.cs
│   │       ├── Configurations/              # EF Core entity configs
│   │       ├── Repositories/
│   │       └── Migrations/
│   │
│   ├── Presentation/
│   │   ├── HireKarlo.Api/
│   │   │   ├── Controllers/
│   │   │   │   ├── AuthController.cs
│   │   │   │   ├── ResumesController.cs
│   │   │   │   ├── JobsController.cs
│   │   │   │   ├── AdvancedAIController.cs  # Advanced AI endpoints
│   │   │   │   ├── MockInterviewController.cs
│   │   │   │   ├── LearningPathController.cs
│   │   │   │   ├── ChatController.cs
│   │   │   │   └── LinkedInController.cs
│   │   │   └── Program.cs
│   │   │
│   │   └── HireKarlo.Web/
│   │       ├── HireKarlo.Web/              # Blazor Server host
│   │       │   └── Components/
│   │       │       ├── Layout/
│   │       │       │   └── MainLayout.razor
│   │       │       └── Pages/
│   │       │           └── Home.razor
│   │       │
│   │       └── HireKarlo.Web.Client/       # Blazor WASM client
│   │           ├── Pages/
│   │           │   ├── Resumes.razor
│   │           │   ├── Jobs.razor
│   │           │   ├── MockInterview.razor
│   │           │   ├── Learning.razor
│   │           │   ├── Chat.razor
│   │           │   ├── LinkedIn.razor
│   │           │   ├── Applications.razor
│   │           │   ├── Login.razor
│   │           │   └── Register.razor
│   │           ├── Services/
│   │           │   ├── ApiClient.cs
│   │           │   └── AuthStateProvider.cs
│   │           └── wwwroot/
│   │               ├── manifest.json        # PWA manifest
│   │               └── service-worker.js
│   │
│   ├── Services/
│   │   ├── HireKarlo.AtsEngine/            # ATS scoring engine
│   │   └── HireKarlo.ResumeService/        # Resume parser + generator
│   │
│   └── Functions/
│       └── HireKarlo.JobIngestion/         # Azure Functions for job ingestion
│
├── tests/
│   └── HireKarlo.Tests/
│
├── README.md
├── DEPLOYMENT.md
└── HireKarlo.slnx
```

---

## 🔧 Tech Stack

| Layer | Technology | Purpose |
|-------|------------|---------|
| **Frontend** | Blazor Web App (WASM) | Interactive SPA with PWA support |
| **API** | ASP.NET Core 9.0 | REST API with JWT auth, rate limiting |
| **AI/LLM** | Azure OpenAI (GPT-4) | All LLM completions, chat, reasoning |
| **Embeddings** | text-embedding-ada-002 | Vectorizing resumes, JDs, content |
| **Vector Store** | Azure AI Search | HNSW index for semantic search |
| **Database** | Azure SQL + EF Core | Relational data, migrations |
| **Blob Storage** | Azure Blob Storage | Resume files, generated documents |
| **Email** | SendGrid | Notifications, weekly digests |
| **Auth** | JWT + Google/LinkedIn OAuth | Social login support |
| **Scheduled Jobs** | Azure Functions | Daily job ingestion |
| **Hosting** | Azure App Service | Free tier available |

---

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK
- Azure SQL Database (or LocalDB for development)
- Azure OpenAI resource with GPT-4 deployment
- Azure AI Search resource (for vector store)

### Configuration

Create `appsettings.Development.json` in the API project:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=HireKarlo;Trusted_Connection=True;"
  },
  "Jwt": {
	"Key": "your-jwt-secret-key-minimum-32-characters",
	"Issuer": "HireKarlo",
	"Audience": "HireKarlo"
  },
  "AzureOpenAI": {
	"Endpoint": "https://your-resource.openai.azure.com/",
	"ApiKey": "your-api-key",
	"DeploymentName": "gpt-4",
	"EmbeddingDeploymentName": "text-embedding-ada-002"
  },
  "AzureAISearch": {
	"Endpoint": "https://your-search.search.windows.net",
	"ApiKey": "your-api-key",
	"IndexName": "hirekarlo-vectors"
  }
}
```

### Run Locally

```bash
# Apply database migrations
cd src/Presentation/HireKarlo.Api
dotnet ef database update

# Run the API
dotnet run

# In another terminal, run the Web app
cd src/Presentation/HireKarlo.Web/HireKarlo.Web
dotnet run
```

API will be at: `https://localhost:7001`  
Web app will be at: `https://localhost:7002`

---

## 📚 API Endpoints Reference

### Authentication
```
POST /api/auth/register          # Email/password registration
POST /api/auth/login             # Email/password login
POST /api/auth/google            # Google OAuth login
POST /api/auth/linkedin          # LinkedIn OAuth login
POST /api/auth/refresh           # Refresh JWT token
```

### Advanced AI (New!)
```
POST /api/advancedai/predict-outcome      # Application outcome prediction
POST /api/advancedai/explainable-ats      # Detailed ATS score breakdown
POST /api/advancedai/keyword-radar        # Keyword trend analysis
POST /api/advancedai/skill-trajectory     # Skill development simulation
POST /api/advancedai/career-roadmap       # 6-month career plan
POST /api/advancedai/tailor-resume        # JD-specific resume tailoring
POST /api/advancedai/interview-questions  # RAG-powered interview questions
```

### Resume & ATS
```
GET  /api/resumes                 # List user's resumes
POST /api/resumes/upload          # Upload new resume
POST /api/resumes/{id}/ats-score  # Get ATS score for resume
POST /api/resumes/{id}/tailor     # Generate tailored version
```

### Jobs
```
GET  /api/jobs/search             # Search job listings
GET  /api/jobs/{id}               # Get job details
GET  /api/jobs/{id}/match         # Get match score with user's resume
```

### Mock Interview
```
POST /api/mockinterview/start     # Start new interview session
POST /api/mockinterview/answer    # Submit answer for evaluation
GET  /api/mockinterview/feedback  # Get session feedback
```

### Learning Paths
```
POST /api/learning/company        # Generate company-specific path
POST /api/learning/skill          # Generate skill-based path
POST /api/learning/interview      # Generate interview-pattern path
GET  /api/learning/active         # Get current active path
POST /api/learning/quiz           # Generate quiz
POST /api/learning/quiz/submit    # Submit quiz answers
```

### AI Chat
```
POST /api/chat/send               # Send message to AI assistant
POST /api/chat/stream             # Stream response (SSE)
```

---

## 🔐 Security

- JWT-based authentication with refresh tokens
- Google and LinkedIn OAuth 2.0 support
- Rate limiting on API endpoints
- CORS configured for allowed origins
- All sensitive data in Azure Key Vault (production)

---

## 🎯 How It All Works Together

### 1. User Onboarding
```
User Signs Up → Upload Resume → Parse & Vectorize → Store in SQL + Vector Index
```

### 2. Job Discovery
```
Daily Job Fetch (Azure Functions) → Match Against User Resumes → 
Score > 70% → Notify User → Show on Dashboard
```

### 3. Resume Optimization Loop
```
User selects Job → Get ATS Score → See exact gaps → 
Tailor Resume with AI → New higher score → Apply
```

### 4. Interview Prep
```
Select Target Company → RAG retrieves real experiences → 
AI generates grounded questions → Practice with Mock Interview → 
Get STAR-method feedback → Track weak areas
```

### 5. Career Growth
```
AI analyzes skill gaps → Recommends projects → Generates learning path → 
Skill trajectory shows progress → 6-month roadmap guides actions
```

---

## 📈 Roadmap

- [ ] Mobile app (MAUI Blazor Hybrid)
- [ ] Push notifications for job alerts
- [ ] Chrome extension for one-click apply
- [ ] More job sources (Greenhouse, Lever APIs)
- [ ] Interview scheduling integration
- [ ] Team/enterprise features

---

## 📄 License

This project is proprietary. All rights reserved.
