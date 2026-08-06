# Phase 2 Implementation Guide: Opportunity Radar

**Status**: 🔄 In Progress  
**Target Completion**: February 2025  
**Release Version**: v2.0.0

---

## 🎯 Objectives

Transform HireKarlo from a resume-analysis tool into an **AI Career Operating System** with a real-time opportunity discovery dashboard.

### Key Metrics
- **Match Accuracy**: 90%+ (user feedback)
- **Daily Active Users**: 50+
- **Referral Callback Rate**: 60%+
- **Time to Apply**: <2 min via referral

---

## 📋 Deliverables Checklist

### Backend Services (Application Layer)
- [x] `OpportunityRadarService.cs` — Daily job ranking & dashboard
- [x] `DreamCompanyIntelligenceService.cs` — Target company analysis
- [x] `ReferralIntelligenceService.cs` — Referral discovery & scoring
- [x] `InterviewDigestService.cs` — Interview prep aggregation
- [ ] `SkillROIService.cs` — Learning path ROI calculation

### Data Layer (Repositories)
- [ ] `IOpportunityMatchRepository` — Job/resume match storage
- [ ] `IDreamCompanyMatchRepository` — Target company matches
- [ ] `IReferralTargetRepository` — Referral scoring cache
- [ ] `ISkillGapRecommendationRepository` — Gap analysis storage
- [ ] `ICareerProgressRepository` — User progress tracking

### API Endpoints (REST)
- [ ] `GET /api/v1/opportunities` — Daily radar dashboard
- [ ] `GET /api/v1/opportunities/{id}` — Opportunity details
- [ ] `POST /api/v1/opportunities/{id}/apply` — Track application
- [ ] `GET /api/v1/dream-companies/{id}/intelligence` — Analysis
- [ ] `GET /api/v1/referrals` — Referral list + scoring
- [ ] `POST /api/v1/referrals/{id}/outreach` — Message generation
- [ ] `GET /api/v1/interviews/{companyId}/prep` — Interview guide

### Blazor Web Components
- [ ] `Pages/Dashboard.razor` — Main opportunity radar
- [ ] `Components/OpportunityCard.razor` — Job card UI
- [ ] `Components/DreamCompanyCard.razor` — Target company card
- [ ] `Components/ReferralsList.razor` — Referral discovery UI
- [ ] `Components/InterviewPrepPanel.razor` — Interview guide
- [ ] `Components/SkillGapAnalysis.razor` — Gap visualization

### Database Migrations
- [ ] Add `Opportunities` table
- [ ] Add `DreamCompanies` table
- [ ] Add `Referrals` table (linking to companies & opportunities)
- [ ] Add `InterviewPrep` table
- [ ] Create pgvector indexes for semantic search
- [ ] Add foreign keys & constraints

### Testing
- [ ] Unit tests for scoring algorithms
- [ ] Integration tests for API endpoints
- [ ] E2E tests for dashboard flows (future)
- [ ] Load testing for 1000+ concurrent users (future)

### Documentation
- [ ] API Swagger docs
- [ ] User guide (how to use dashboard)
- [ ] Deployment guide (Phase 2)
- [ ] Architecture decisions (ADR)

---

## 🏗️ Implementation Order

### Week 1: Database & Repositories
1. Create migrations for new tables
2. Implement repository interfaces
3. Wire repositories into service layer
4. Test CRUD operations

**Files to create/modify**:
- `src/Infrastructure/HireKarlo.Persistence/Migrations/AddPhase2Tables.cs`
- `src/Infrastructure/HireKarlo.Persistence/Repositories/OpportunityMatchRepository.cs`
- `src/Infrastructure/HireKarlo.Persistence/Repositories/DreamCompanyMatchRepository.cs`
- `src/Infrastructure/HireKarlo.Persistence/Repositories/ReferralTargetRepository.cs`

### Week 2: Service Layer
1. Connect services to repositories
2. Implement scoring algorithms
3. Add caching layer (Redis)
4. Write unit tests

**Files to modify**:
- `src/Core/HireKarlo.Application/Services/OpportunityRadarService.cs`
- `src/Core/HireKarlo.Application/Services/DreamCompanyIntelligenceService.cs`
- `src/Core/HireKarlo.Application/Services/ReferralIntelligenceService.cs`

### Week 3: API Endpoints
1. Create controller endpoints
2. Add DTOs for request/response
3. Implement JWT auth
4. Generate Swagger docs

**Files to create**:
- `src/Presentation/HireKarlo.Api/Controllers/OpportunitiesController.cs`
- `src/Presentation/HireKarlo.Api/Controllers/DreamCompaniesController.cs`
- `src/Presentation/HireKarlo.Api/Controllers/ReferralsController.cs`
- `src/Presentation/HireKarlo.Api/DTOs/OpportunityDto.cs`
- `src/Presentation/HireKarlo.Api/DTOs/DreamCompanyAnalysisDto.cs`

### Week 4: Blazor Frontend
1. Create dashboard layout
2. Build individual components
3. Implement client-side caching
4. Add error handling & loading states

**Files to create**:
- `src/Presentation/HireKarlo.Web/Pages/Dashboard.razor`
- `src/Presentation/HireKarlo.Web.Client/Components/OpportunityCard.razor`
- `src/Presentation/HireKarlo.Web.Client/Components/DashboardLayout.razor`
- `src/Presentation/HireKarlo.Web.Client/Services/OpportunityService.cs`

### Week 5: Polish & Testing
1. Load testing & optimization
2. Security audit
3. Integration testing
4. Documentation & deployment guide

---

## 🔑 Key Design Decisions

### 1. **Opportunity Matching Algorithm**
```
Match Score = (Skill TF-IDF × 0.5) + (Experience Level × 0.3) + (Location × 0.2)
Range: 0-100%
Threshold: Apply only if ≥70% match
```

### 2. **Referral Scoring**
```
Referral Score = (Tech Stack Match × 0.4) + 
				 (Years Experience × 0.3) + 
				 (Network Activity × 0.2) + 
				 (Response Rate × 0.1)
```

### 3. **Caching Strategy**
- Dashboard (user) → Redis 6h
- Job board data → Redis 2h
- Referral scores → Redis 24h
- Company intelligence → Redis 7d

### 4. **Job Board Integration**
Currently scrape:
- LinkedIn Jobs API (if available)
- Wellfound
- GitHub Jobs
- HackerNews Who's Hiring
- AngelList

**Rate limiting**: 10 jobs/req, 5 min between runs

---

## 📊 Data Flow Diagram

```
1. USER UPLOADS RESUME
   ↓
2. RESUME SERVICE extracts skills
   ↓
3. DAILY BATCH JOB: Job boards → Scrape 100+ jobs
   ↓
4. MATCHING ENGINE scores each job
   ↓
5. OPPORTUNITY RADAR stores top 50 matches
   ↓
6. DASHBOARD queries Redis cache
   ↓
7. USER SEES: Job cards ranked by match %
   ↓
8. USER CLICKS JOB → See:
   - Why matched? (skill analysis)
   - What's missing? (gap analysis)
   - Who can refer? (referral intel)
   - How to prepare? (interview prep)
```

---

## 🚀 Deployment Checklist

### Pre-Deployment
- [ ] All tests passing
- [ ] Code review approved
- [ ] Database migrations tested
- [ ] Performance benchmarks OK
- [ ] Security scan passed
- [ ] Documentation complete

### Deployment (Single Command)
```bash
# Create GitHub Release tag
git tag v2.0.0
git push origin v2.0.0

# GitHub Actions auto-runs:
# 1. Build Docker image
# 2. Run tests
# 3. Push to registry
# 4. Deploy to Render/Azure
# 5. Run DB migrations
```

### Post-Deployment
- [ ] Smoke tests in production
- [ ] Monitor error logs
- [ ] Check API response times
- [ ] Verify dashboard loads
- [ ] Test end-to-end flow

---

## 📚 Reference Files

| File | Purpose |
|------|---------|
| `.github/workflows/release.yml` | Auto-deploy on tag |
| `docker-compose.yml` | Local dev environment |
| `package.json` | Build & release scripts |
| `RELEASES.md` | Version history |
| `docs/ARCHITECTURE.md` | System design |

---

## 🎯 Success Criteria

By end of Phase 2, HireKarlo should:
1. ✅ Score any job against user's resume (0-100%)
2. ✅ Display 10+ daily opportunities in dashboard
3. ✅ Identify skill gaps with learning recommendations
4. ✅ Suggest relevant referrals (70%+ accuracy)
5. ✅ Aggregate company-specific interview prep
6. ✅ Support 1000+ concurrent users
7. ✅ Deploy via single `git tag` command

**Portfolio Value**: Demonstrates full-stack AI product shipping with Blazor + .NET microservices.

---

**Next Phase**: Phase 3 (v3.0.0) — VS Code & Browser extension distribution
