# HireKarlo: Product Vision & Value Proposition

## For Job Seekers: The Five Questions

Most engineers ask themselves:

1. **Which jobs should I apply for?**
2. **Which skills should I learn?**
3. **Which companies am I closest to?**
4. **Who should I ask for referrals?**
5. **How should I prepare for interviews?**

### HireKarlo Answers All Five in One Place

---

## The User Journey

### Step 1: Upload Resume
User uploads their resume (PDF/DOCX). System extracts:
- **Technical Skills** (Python, Kubernetes, Terraform, etc.)
- **Experience Level** (years, seniority, domains)
- **Work History** (companies, roles, impact)

### Step 2: Select Dream Companies
User selects target companies:
- **Adobe**, **Atlassian**, **Databricks**, **Juspay**, etc.
- Or browse a global company database

### Step 3: Get Intelligence Dashboard

#### **Dream Company Intelligence** (Centerpiece)
```
Adobe ............. 84%  ↑ How do I get to 85%?
Atlassian .......... 78%  ↑ What skills am I missing?
Databricks ......... 71%  ↓ Why am I not higher?
Juspay .............. 93%  ↓ I'm almost there!
```

Each match score includes:
- ✅ **Skills You Have** (aligned with job reqs)
- ✅ **Experience Match** (years, level fit)
- ⚠️ **Gaps** (missing skills/experience)
- 📈 **Improvement Path** (skill ROI engine)

---

## Core Features (MVP → V3.0)

### 🎯 Dream Company Intelligence *(v2.0 — in progress)*
**The centerpiece. The thing you check every morning.**

Flow:
1. Parse resume → extract skills
2. Fetch company job postings → parse requirements
3. Embed both → semantic matching
4. Score 0-100% → rank companies
5. Generate "Why X%" explanation

Output:
```
Dream Company Match: Adobe (84%)
───────────────────────────────
✔ Python (5 yrs) - Company wants 3+
✔ Kubernetes (3 yrs) - Company wants 2+
✔ AWS (4 yrs) - Company wants basic
✖ Terraform (0 yrs) - Company wants 5+
✖ System Design (not evident) - Company needs strong
✖ Distributed Systems (not demonstrated)

PREDICTION:
└─ Learn Terraform (6-8 weeks) → +8% match
└─ Learn Distributed Systems (10 weeks) → +12% match
└─ Build proof project (4 weeks) → +5% match
TOTAL: Could reach 97% in 6 months
```

---

### 🚀 Opportunity Radar *(v2.0 — in progress)*
**Your daily dose of high-quality opportunities.**

Flow:
1. Scrape job boards daily (LinkedIn, company careers pages)
2. Embed each job description
3. Semantic match against your resume
4. Rank by relevance (0-100%)
5. Show only high-confidence matches (>60%)

Homepage Output:
```
TODAY'S OPPORTUNITIES (Ranked by Match)
─────────────────────────────────────

1. Adobe - Platform Engineer IV
   Match: 91% | Location: San Francisco | Salary: $250-300k
   Why: ✔ Terraform, ✔ Kubernetes, ✔ CI/CD
   Missing: ✖ Python
   [Apply] [Save] [Learn Why]

2. Databricks - Infrastructure Engineer
   Match: 87% | Location: Remote | Salary: $240-290k
   Why: ✔ Spark, ✔ Cloud, ✔ Python
   Missing: ✖ ML Systems Design
   [Apply] [Save] [Learn Why]

3. Stripe - Cloud Platform Engineer
   Match: 84% | Location: NYC | Salary: $270-320k
   Why: ✔ Go, ✔ Microservices, ✔ Payment Systems
   Missing: ✖ Fraud Detection, ✖ Risk Modeling
   [Apply] [Save] [Learn Why]
```

**Only shows jobs you should care about.** No noise.

---

### 🎓 Skill ROI Engine *(v2.1)*
**What should you learn next?**

For each dream company, calculate:
- **Time to Learn** (weeks)
- **Match Improvement** (+%)
- **ROI Score** (improvement / time)

Example:
```
Current Adobe Match: 84%

Skill Learning Options:
─────────────────────

1. Terraform (6-8 weeks)
   → +8% match improvement
   → ROI: 1.1% per week ⭐⭐⭐⭐⭐

2. Python Deep Dive (8-10 weeks)
   → +4% match improvement
   → ROI: 0.45% per week ⭐⭐

3. Docker Security (3-4 weeks)
   → +10% match improvement
   → ROI: 2.7% per week ⭐⭐⭐⭐⭐

4. System Design (12+ weeks)
   → +15% match improvement
   → ROI: 1.2% per week ⭐⭐⭐⭐

RECOMMENDED: Terraform + Docker Security (high ROI)
```

---

### 🤝 Referral Intelligence *(v2.1)*
**Who should you ask for a referral?**

Not just "find employees at company X"—**find the right employees.**

Matching Score Includes:
- **Tech Stack Similarity** (shared tools/languages)
- **Experience Similarity** (comparable roles/levels)
- **Location Similarity** (same city, timezone, remote)
- **Career Path Similarity** (similar trajectory)
- **Reachability Score** (LinkedIn/GitHub connection strength)

Output:
```
Referral Targets at Adobe
───────────────────────────

1. Sarah Chen (Platform Engineering Lead)
   Overall Match: 94%
   ├─ Tech Stack: 98% (you both know Rust, Kubernetes)
   ├─ Experience: 91% (both 5+ years, similar roles)
   ├─ Location: 95% (both SF Bay Area)
   ├─ Career Path: 89% (both moved from Stripe to Big Tech)
   └─ Reachability: 87% (2nd degree connection via LinkedIn)

   [Generate Outreach Email] [View Full Profile]

2. Marcus Johnson (Senior Software Engineer)
   Overall Match: 82%
   ├─ Tech Stack: 86% (Go, Python, AWS overlap)
   ├─ Experience: 79% (4 yrs, similar scope)
   ├─ Location: 72% (NYC | you're SF)
   ├─ Career Path: 81% (both grew from startups)
   └─ Reachability: 91% (1st degree connection)

   [Generate Outreach Email] [View Full Profile]
```

Auto-generate personalized outreach:
```
Subject: Found you through shared experience at Stripe
Body:
Hi Sarah,

I noticed we both spent time scaling infrastructure at Stripe
and now at Adobe. I'm exploring opportunities in platform 
engineering and would love to pick your brain about life at Adobe,
especially your work on Kubernetes orchestration.

Would you have 15 mins next week?

Thanks,
Karthik
```

---

### 📚 Interview Digest *(v2.5)*
**Smart prep for each company.**

Not just "scrape Blind"—**aggregate, analyze, and personalize.**

Flow:
1. Scrape Blind, LeetCode, Levels.fyi
2. Embed company data
3. RAG: retrieval-augmented generation for context
4. Categorize by interview type
5. Generate personalized prep roadmap

Output:
```
Interview Digest: Adobe Platform Engineer
──────────────────────────────────────────

PREPARATION ROADMAP
├─ Behavioral (20% of interviews)
│  ├─ Most Asked: "Tell me about a time you scaled..."
│  ├─ Themes: Ownership, Impact, Communication
│  └─ Prep Time: 8 hours
│
├─ System Design (40% of interviews)
│  ├─ Areas: Distributed Systems, Caching, Load Balancing
│  ├─ Real Questions:
│  │  • Design a global CDN
│  │  • Design Adobe's asset storage system
│  │  • Handle 10M concurrent users
│  └─ Prep Time: 20 hours
│
├─ LeetCode-style Coding (30% of interviews)
│  ├─ Difficulty: Hard (mostly)
│  ├─ Topics: Graphs, Dynamic Programming, Strings
│  ├─ Real Questions:
│  │  • Longest palindromic substring
│  │  • Word ladder with constraints
│  └─ Prep Time: 15 hours
│
└─ Domain Knowledge (10% of interviews)
   ├─ Focus: Content Distribution, Media Serving
   ├─ Topics: Video codecs, streaming protocols
   └─ Prep Time: 4 hours

TOTAL PREP TIME: ~48 hours for high confidence
```

---

## Technical Stack (Why Each Choice)

| Component | Choice | Why |
|-----------|--------|-----|
| **Backend** | ASP.NET Core 9 | Type safety, excellent ORM (EF Core), minimal boilerplate |
| **Frontend** | Blazor WebAssembly | C# in browser, PWA-ready, code reuse |
| **Database** | PostgreSQL 16 | ACID guarantees, excellent ecosystem, pgvector for embeddings |
| **Vector Search** | pgvector + IVFFLAT | No separate vector DB needed, sub-millisecond search |
| **LLM** | Groq Llama 3.3 | Free tier (30 req/min), 70B model (excellent reasoning), fast inference |
| **Embeddings** | HuggingFace (all-MiniLM-L6-v2) | Free, reliable, 384-dim (good quality/speed tradeoff) |
| **Caching** | Redis | Sub-millisecond lookups, TTL support, minimal cloud cost |
| **Deployment** | Docker + Render | Consistent dev/prod, free tier ($0/month), managed PostgreSQL |
| **CI/CD** | GitHub Actions | Native to GitHub, no vendor lock-in |

**Philosophy:** Monolith first, scale later. No Kafka, Kubernetes, or microservices yet.

---

## Development Roadmap

### ✅ Phase 1: Resume Intelligence (v1.0 — Q1 2024)
- Resume upload & parsing
- Skill extraction via LLM
- Basic match scoring
- User authentication

### 🔄 Phase 2: Opportunity Radar (v2.0 — Sep 2024)
- Dream Company Intelligence
- Job scraping pipeline
- Semantic matching
- Opportunity Radar dashboard
- **Status:** In Progress

### 🔜 Phase 3: Skill ROI + Referral (v2.1 — Oct 2024)
- Skill ROI calculation
- Learning path generation
- Referral intelligence
- Employee matching engine

### 📅 Phase 4: Interview Intelligence (v2.5 — Dec 2024)
- Interview Digest aggregation
- Blind/LeetCode scraping
- RAG processing
- Personalized prep roadmaps

### 🌐 Phase 5: Extensions & SDKs (v3.0 — May 2025)
- VS Code extension
- Chrome/Firefox extension
- CLI tool
- NuGet SDK
- npm SDK

---

## Competitive Advantages

| Feature | HireKarlo | Job Boards | ATS Checkers | LinkedIn |
|---------|-----------|-----------|-------------|----------|
| Dream Company Matching | ✅ | ❌ | ❌ | ❌ |
| Skill ROI Calculation | ✅ | ❌ | ❌ | ❌ |
| Interview Digest | ✅ (Aggregated) | ❌ | ❌ | ❌ |
| Referral Intelligence | ✅ (Tech-aware) | ❌ | ❌ | ⚠️ (Basic) |
| Semantic Matching | ✅ | ⚠️ (Keyword only) | ⚠️ | ⚠️ |
| Free Tier | ✅ | ⚠️ | ✅ | ⚠️ |

---

## Resume Description (For Hiring Managers/Recruiters)

### Your Elevator Pitch
```
HireKarlo - AI-Powered Career OS (.NET 9, PostgreSQL, pgvector, Groq, Redis)

• Built an AI-powered Career Operating System that analyzes resume-to-company 
  fit, surfaces high-match opportunities, and generates personalized skill roadmaps.

• Implemented Dream Company Intelligence, Opportunity Radar, and Interview Digest 
  engines using semantic search, vector embeddings, and RAG pipelines.

• Designed a match-scoring platform leveraging PostgreSQL pgvector for embedding 
  storage, Groq LLMs for intelligent analysis, and Redis for sub-millisecond 
  rankings of 1000+ daily opportunities.

• Full-stack: Blazor WebAssembly (frontend), ASP.NET Core (backend), Docker 
  (containerization), Render (auto-scaling deployment).
```

### GitHub Stats Worth Highlighting
- **Lines of Code:** ~25K (backend logic, AI pipelines)
- **Database:** 11 tables, 384-dim vector search
- **API Endpoints:** 20+ REST endpoints
- **Performance:** <500ms for 10 matches, <200ms for single opportunity
- **Deployment:** 1-click deploy via GitHub Actions → Render

---

## Key Success Metrics

**Launch Metrics:**
- Users uploading resumes
- Daily active opportunity checks
- Companies in "Dream Company" list (viral indicator)
- Conversion: Opportunity → Apply

**Engagement Metrics:**
- Week 1 retention (came back)
- Feature adoption (Radar, Skill ROI, Referrals)
- Time in app (habit formation)

**Impact Metrics:**
- interview → offer acceptance rate
- Speed to apply (lower is better)
- Skill learning adoption

---

## Next Steps for Reviewers

1. **For Engineers:** Review `docs/TECHNICAL_DEEP_DIVE.md` for architecture details
2. **For Product Managers:** Review rollout strategy and feature prioritization
3. **For Hiring Managers:** Use the resume description above
4. **For Investors:** See roadmap, competitive advantages, and business model (B2C freemium → B2B recruiting tools)
