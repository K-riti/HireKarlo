# HireKarlo Phase 3.1: Feature Implementation Roadmap

## Overview
Phase 3.1 focuses on implementing the core AI Career OS features identified in the product vision:
1. **Opportunity Radar** - Daily job discovery and ranking
2. **Referral Intelligence** - Tech-aware employee matching and outreach
3. **Skill ROI Engine** - Learning path optimization
4. **Interview Digest** - Company-specific preparation aggregation

---

## Phase 3.1 Prerequisites

### Domain Entities to Create
- [ ] `EmployeeProfile` - Represents employees at target companies (for referral targeting)
  - Name, JobTitle, Company, LinkedIn URL, GitHub, Skills, Experience, Location
  - ImportedFrom (LinkedIn, Internal Directory, etc)
  - LastUpdated timestamp

- [ ] Extend `OpportunityMatch` with missing properties:
  - MatchedSkills[] - Which skills from resume matched
  - MissingSkills[] - Which required skills are missing
  - Explanation - AI-generated reason for the match
  - Status - (New, Viewed, Interested, Applied, Rejected, etc)

- [ ] Extend `Resume` with missing properties:
  - YearsOfExperience - Derived from work history
  - Location - User's current/preferred location

- [ ] Extend `JobListing` with:
  - RequiredSkills[] - Parsed from job description
  - Url - Link to original posting

### Repository Interfaces to Create (in IRepositories.cs)
- [ ] `IEmployeeProfileRepository : IRepository<EmployeeProfile>`
  - `GetByCompanyAsync(companyName, ct)` - Get all employees at a company
  - `GetByLinkedInUrlAsync(profileUrl, ct)` - Get or create from LinkedIn URL
  - `UpdateAsync(profile, ct)` - Sync LinkedIn data

- [ ] Extend `IJobListingRepository`:
  - `GetRecentJobsAsync(days, limit, ct)` - Last N days of new postings

### Service Interfaces (Already Defined)
- ✓ `IOpportunityRadarService` - Already defined in application
- ✓ `IReferralIntelligenceService` - Already defined in application  
- ✓ `ISkillROIEngine` - Already defined in application (new)
- ✓ DTOs created - `OpportunitiesAndReferralsDtos.cs`

---

## Feature Implementation Details

### 1. Opportunity Radar Service

**Purpose**: Daily discovery of relevant job opportunities

**Methods to Implement**:
```csharp
// Core discovery
FindNewOpportunitiesAsync(userId) // Daily job feed scan
FindOpportunitiesForCompanyAsync(userId, dreamCompanyId) // Company-specific
GenerateOpportunityExplanationAsync(userId, jobListingId, dreamCompanyId) // AI explanation
GetOpportunityDetailsAsync(userId, opportunityMatchId) // Full details

// User actions
UpdateOpportunityStatusAsync(opportunityMatchId, status) // Track user interest
SendOpportunityRadarNotificationAsync(userId, opportunities) // Email/push digest
GetOpportunitiesByCompanyAsync(userId) // Dashboard view
ScheduleOpportunityRadarAsync(userId) // Background job scheduling
```

**Implementation Approach**:
1. Query active `JobListing` records from aggregated feeds
2. Load user `Resume` and `DreamCompany` preferences
3. For each job:
   - Use `IEmbeddingService` to vectorize job description
   - Compare against resume embedding using cosine similarity
   - Score: Skills (50%) + Experience (25%) + Location (15%) + Salary (10%)
4. Filter matches > 60%
5. Generate AI explanation using context window with resume/job + matching details
6. Store as `OpportunityMatch` entities
7. Queue notification if top 3 matches found

**Dependencies**:
- `IEmbeddingService.GetOrCreateEmbeddingAsync()` - Vector embedding (must add method if missing)
- `IMatchPercentageService.CalculateMatchPercentageAsync()` - Scoring helper
- `IOpenAIService.CompleteAsync()` - AI explanations
- Job aggregator adapter (LinkedIn API, Indeed RSS, etc) - Phase 3.2

---

### 2. Referral Intelligence Service

**Purpose**: Find qualified referrers at target companies

**Methods to Implement**:
```csharp
// Discovery & analysis
FindReferralTargetsAsync(userId, dreamCompanyId) // Find employees at company
GetReferralTargetAsync(userId, referralTargetId) // Specific target details
CalculateSimilarityScoreAsync(userId, referralTargetId) // Education+skills+path match

// Outreach generation
GenerateOutreachStrategyAsync(userId, referralTargetId) // Personalized strategy
GenerateDraftMessageAsync(userId, referralTargetId) // Copy-paste ready message

// Tracking & follow-up
SetFollowUpReminderAsync(referralTargetId, date) // Schedule reminder
UpdateReferralStatusAsync(referralTargetId, status) // (NoAction, Contacted, Responded, Referred, Rejected)
SendReferralReminderNotificationsAsync(userId) // Daily reminders

// Data integration
IngestLinkedInReferralsAsync(userId, dreamCompanyId) // LinkedIn API integration
GetAllReferralTargetsAsync(userId) // Dashboard aggregation
```

**Implementation Approach**:
1. Query `EmployeeProfile` records for target company
2. Calculate multi-factor similarity:
   - Tech Stack: Jaccard similarity of skills (25%)
   - Experience: Years difference normalized (30%)
   - Location: City match or timezone proximity (15%)
   - Career Path: Vector embedding similarity of backgrounds (20%)
   - Reachability: LinkedIn graph distance (10%)
3. Filter by similarity > 50%
4. Rank by overall score
5. Generate contextual AI outreach strategy
6. Track status and follow-up in `ReferralTarget` entity

**LinkedIn Integration** (Phase 3.1):
- Implement `ILinkedInAdapter` to query employee directory
- Cache results in `EmployeeProfile` table
- Update weekly (or on-demand)

**Dependencies**:
- `IEmbeddingService` - Career path similarity
- `IOpenAIService` - Strategy/message generation
- LinkedIn API adapter - To be implemented
- Career path vector database - May use pgvector for embeddings

---

### 3. Skill ROI Engine

**Purpose**: Predict learning ROI and generate personalized learning paths

**Methods to Implement**:
```csharp
// Analysis
AnalyzeSkillsROIAsync(userId, dreamCompanyId) // ROI for this company
GetSkillROIAcrossCompaniesAsync(userId, skill) // Skill value across portfolio
GetRecommendedLearningPathAsync(userId, dreamCompanyId, targetMatchPercentage) // Learning sequence
```

**Implementation Approach**:
1. **Skill Frequency Analysis**:
   - Query all `JobListing` records for target company
   - Parse required skills from descriptions (use existing skill taxonomy)
   - Frequency count: which skills appear in 80% / 50% / 20% of roles

2. **Learning Time Estimation**:
   - Maintain skill library with estimated weeks (expert knowledge)
   - E.g., "Terraform basics: 4 weeks", "Kubernetes: 8 weeks"
   - Factor user's background (if already knows similar tech, reduce by 30%)

3. **Salary Impact**:
   - Aggregate salary data by skill requirement
   - "Rust required roles: avg $240k", "Python roles: avg $180k"
   - Salary uplift per skill = avg(with skill) - avg(without)

4. **ROI Calculation**:
   - ROI Score = (Salary Uplift × Frequency) / Learning Weeks
   - Example: Terraform: ($30k × 0.8) / 6 weeks = 4.0 points/week (high ROI)

5. **Learning Path Sequencing**:
   - Group by dependencies (Docker before Kubernetes)
   - Sort by ROI
   - Estimate total path time
   - Recommend learning resources

**DTOs** (Already defined in application):
- `SkillROIDto` - Individual skill ROI
- `SkillROIComparisonDto` - Skill across companies
- `LearningPathRecommendationDto` - Sequenced learning steps

**Dependencies**:
- Skill taxonomy/ontology (domain model)
- Job listing data with parsed skills
- Historical salary data (aggregated)

---

### 4. Interview Digest / Interview Intelligence Engine

**Purpose**: Aggregate company-specific interview prep from multiple sources

**Status**: Already partially implemented (see `InterviewDigestEntry`, `EmailDigestService`)

**To Complete**:
- [ ] Implement RAG-based content aggregation:
  - LeetCode problem sets for company
  - Blind interview experiences
  - Levels.fyi equivalency data
  - Company blogs, engineering posts

- [ ] Vector search of aggregated content
- [ ] Email digest generation
- [ ] Dashboard timeline view

---

## Implementation Milestones

### Week 1: Domain Model & Repository Setup
- Create `EmployeeProfile` entity + migration
- Extend `OpportunityMatch`, `Resume`, `JobListing` entities
- Implement repository interfaces
- Update DbContext

### Week 2: Opportunity Radar
- Implement `OpportunityRadarService`
- Integration with embedding service
- Daily background job scheduling
- Notification system

### Week 3: Referral Intelligence
- Implement `ReferralIntelligenceService`
- AI outreach strategy generation
- Similarity scoring algorithms
- Status tracking & reminders

### Week 4: Skill ROI Engine
- Implement `SkillROIEngine`
- Skill library & learning time database
- ROI calculation engine
- Learning path recommendation

### Week 5: LinkedIn Integration (Phase 3.2)
- LinkedIn API adapter
- Employee profile sync
- Data enrichment pipeline

### Week 6: Testing & Optimization
- Unit tests for all services
- Integration tests
- Performance tuning
- Dashboard integration

---

## Known Gaps & Phase 3.2 Planning

**Phase 3.1 Blockers** (Can work around with mocks):
- [ ] LinkedIn API access (requires approval)
- [ ] Job aggregator feed (can mock for testing)
- [ ] Embedding model choice (use OpenAI embeddings or local model?)

**Phase 3.2 Features** (To follow):
- [ ] LinkedIn employee directory ingestion
- [ ] Job feed aggregation (Indeed, LinkedIn, Greenhouse, Lever, etc)
- [ ] Vector database optimization (pgvector performance tuning)
- [ ] Background job retry policies
- [ ] Analytics dashboard (track match improvement over time)

---

## File References

**Created**:
- `src/Core/HireKarlo.Application/DTOs/CareerOS/OpportunitiesAndReferralsDtos.cs`
- `src/Core/HireKarlo.Application/Interfaces/Services/ISkillROIEngine.cs`

**To Be Created**:
- `src/Infrastructure/HireKarlo.Infrastructure/Services/OpportunityRadarService.cs`
- `src/Infrastructure/HireKarlo.Infrastructure/Services/ReferralIntelligenceService.cs`
- `src/Infrastructure/HireKarlo.Infrastructure/Services/SkillROIEngine.cs`
- `src/Infrastructure/HireKarlo.Infrastructure/Adapters/LinkedInAdapter.cs`
- `src/Infrastructure/HireKarlo.Infrastructure/Adapters/JobAggregatorAdapter.cs`

**Tests to Add**:
- `tests/HireKarlo.Application.Tests/Services/OpportunityRadarServiceTests.cs`
- `tests/HireKarlo.Application.Tests/Services/ReferralIntelligenceServiceTests.cs`
- `tests/HireKarlo.Application.Tests/Services/SkillROIEngineTests.cs`

---

## References

- **Embedding Service**: See `IEmbeddingService` in `src/Core/HireKarlo.Application/Interfaces/AI/`
- **Match Scoring**: See `IMatchPercentageService` in `src/Core/HireKarlo.Application/Interfaces/Services/`
- **OpenAI Integration**: See `IOpenAIService` for available methods
- **Job Aggregation**: Research LinkedIn API, Indeed RapidAPI, Greenhouse API
- **Background Jobs**: Implement via Hangfire (already in project)
- **Vector Database**: Use pgvector extension on PostgreSQL (already configured)

