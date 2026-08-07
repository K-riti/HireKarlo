# HireKarlo - Product Flow & User Journey

## 🎯 The Core User Flow (Keeps Users Coming Back)

### Step 1: Upload Resume ✅
```
User uploads resume (PDF/DOCX)
	 ↓
System parses skills, experience, education
	 ↓
Stores in searchable format (vector embeddings)
```

**Blazor Component:** `ResumeUploadComponent.razor`
- Drag-drop upload
- Real-time parsing progress
- Show extracted skills for review

---

### Step 2: Pick Dream Companies
```
User selects target companies
├─ Adobe
├─ Atlassian
├─ Databricks
├─ Juspay
└─ + Add More...
```

**Blazor Component:** `DreamCompanyPickerComponent.razor`
- Search + autocomplete company names
- Show company info (logo, industry, size)
- Allow multiple selections
- Set role preference (optional)

**Why This Matters:**
- Personalizes all downstream features
- Users feel in control
- Powers Dream Company Intelligence

---

### Step 3: Generate Intelligence Dashboard
```
System analyzes and generates:

📊 Match Scores
├─ Adobe: 84%
├─ Atlassian: 78%
├─ Databricks: 93%
└─ Juspay: 65%
	 ↓
💡 Skill Gaps
├─ Missing Python (8 weeks → +11% improvement)
├─ Missing Terraform (3 weeks → +8% improvement)
└─ Missing Apache Kafka (6 weeks → +7% improvement)
	 ↓
🤝 Referral Targets
├─ Sarah Chen (92% match) at Adobe
├─ John Smith (87% match) at Atlassian
└─ + 5 more high-quality referrals
	 ↓
🎓 Interview Digest
├─ Most Asked Topics at Adobe
├─ System Design Areas
└─ Behavioral Themes
	 ↓
📡 Open Opportunities
├─ Adobe Platform Engineer (91% match)
├─ Atlassian SRE (87% match)
└─ + New opportunities added daily
```

**Blazor Components:**
- `DreamCompanyMatchDashboard.razor` — Shows match scores
- `SkillGapPanel.razor` — Displays gaps + learning paths
- `ReferralPanel.razor` — Shows referral targets
- `InterviewPrepPanel.razor` — Interview digest preview
- `OpportunityRadarPanel.razor` — Daily opportunities

---

### Step 4: Track Progress (Keeps Coming Back 📍)

```
LAST MONTH
┌──────────────────┐
│  Avg Match: 72%  │
│  Apps: 8         │
│  Interviews: 2   │
└──────────────────┘

TODAY
┌──────────────────┐
│  Avg Match: 84%  │
│  Apps: 0 (new)   │
│  Interviews: 0   │
└──────────────────┘

PROGRESS
┌──────────────────────────┐
│        📈 +12%           │ ← Match improvement
│   Match quality rising   │
│  Learn 2 key skills      │
│  Improve interview prep  │
└──────────────────────────┘
```

**Blazor Component:** `ProgressTrackingComponent.razor`
- Month-over-month comparison
- Match % trend chart
- Application velocity
- Interview conversion rate
- Motivation messaging

---

## 🏠 Dashboard Layout (Blazor Page)

### CareerDashboard.razor

```
┌─────────────────────────────────────────────────────────┐
│  HireKarlo — Career Operating System                    │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  Welcome back! Your AI is finding opportunities...     │
│                                                          │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  📡 OPPORTUNITY RADAR (Today)                           │
│  ┌────────────────────────────────────────────────────┐│
│  │ 12 New Opportunities Discovered                    ││
│  │                                                    ││
│  │ 🔥 Adobe Platform Engineer                        ││
│  │    Match: 91%  |  🎯 Dream Company  |  [Details] ││
│  │                                                    ││
│  │ ✅ Atlassian Backend Engineer                      ││
│  │    Match: 87%  |  [Apply Now]  [Bookmark]       ││
│  │                                                    ││
│  │ 👍 BrowserStack QA Lead                           ││
│  │    Match: 82%  |  [Learn Gaps]                   ││
│  │                                                    ││
│  │ [→ View All 12 Opportunities]                     ││
│  └────────────────────────────────────────────────────┘│
│                                                          │
├──────────────────  ─────────────────────────────────────┤
│                                                          │
│  YOUR INTELLIGENCE DASHBOARD                           │
│  ┌──────────────────┬──────────────────┬──────────────┐│
│  │  Dream Companies │  Skill Gaps      │ Referrals    ││
│  ├──────────────────┼──────────────────┼──────────────┤│
│  │ Adobe:    84%    │ Python: 8 weeks  │ Sarah Chen   ││
│  │ Atlassian: 78%   │  +11% match      │  92% score   ││
│  │ Databricks: 93%  │                  │              ││
│  │ Microsoft: 71%   │ Terraform: 3w    │ John Smith   ││
│  │                  │  +8% match       │  87% score   ││
│  │ [Edit Targets]   │                  │              ││
│  │                  │ [Learn Path]     │ [Message]    ││
│  └──────────────────┴──────────────────┴──────────────┘│
│                                                          │
├──────────────────  ─────────────────────────────────────┤
│                                                          │
│  📊 PROGRESS TRACKING                                   │
│  ┌────────────────────────────────────────────────────┐│
│  │  Last Month: 72%  →  Today: 84%  📈 +12%         ││
│  │                                                    ││
│  │  Match Quality Over Time:                         ││
│  │  ████████░  (improving)                           ││
│  │                                                    ││
│  │  Applications: 8  →  Interviews: 2  →  Offers: 0 ││
│  │  Conversion: 25%                                  ││
│  │                                                    ││
│  │  [Detailed Analytics]                             ││
│  └────────────────────────────────────────────────────┘│
│                                                          │
├──────────────────  ─────────────────────────────────────┤
│                                                          │
│  🎓 INTERVIEW PREP (Adobe Platform Engineer)           │
│  ┌────────────────────────────────────────────────────┐│
│  │  Most Asked Topics:                                ││
│  │  • Kubernetes (94%)                                ││
│  │  • CI/CD Design (87%)                              ││
│  │  • Incident Response (81%)                         ││
│  │                                                    ││
│  │  [Get Full Study Guide]                            ││
│  └────────────────────────────────────────────────────┘│
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## 📱 Mobile/Responsive Considerations

```csharp
// Blazor mobile-first design
@page "/career-dashboard"
@using HireKarlo.Web.Components
@inherits ComponentBase

<div class="dashboard-container">

	@if (opportunities == null)
	{
		<LoadingSpinner Message="Finding opportunities..." />
	}
	else
	{
		<OpportunityRadarSection 
			Opportunities="opportunities"
			OnViewAll="ShowAllOpportunities"
			OnApply="ApplyToJob" />

		<DreamCompanyMatchSection
			Companies="dreamCompanies"
			OnEditTargets="EditDreamCompanies" />

		<ProgressTrackingSection
			CurrentMatch="statistics.AvgThisMonth"
			PreviousMatch="statistics.AvgLastMonth"
			Trend="statistics.Trend" />

		<InterviewPrepSection
			CompanyName="topCompany"
			Topics="topicsList" />
	}
</div>

@code {
	private List<OpportunityDto> opportunities;
	private List<DreamCompanyMatchDto> dreamCompanies;
	private RadarStatisticsDto statistics;

	protected override async Task OnInitializedAsync()
	{
		// Load all dashboard data
		opportunities = await OpportunityRadarService
			.GetDailyOpportunitiesAsync(CurrentUser.Id);

		dreamCompanies = await DreamCompanyService
			.GetAllCompanyMatchesAsync(CurrentUser.Id);

		statistics = await OpportunityRadarService
			.GetRadarStatsAsync(CurrentUser.Id);
	}

	private async Task ApplyToJob(string jobId)
	{
		// Log interaction + navigate to detailed analysis
		await OpportunityRadarService
			.LogOpportunityInteractionAsync(
				CurrentUser.Id, 
				jobId, 
				OpportunityInteractionType.Applied);

		NavigateTo($"/opportunity/{jobId}");
	}
}
```

---

## 🔄 Daily/Weekly User Loop

### What Brings Users Back Daily?

1. **🎯 New Opportunities Card**
   - "12 new matches today"
   - See best matches first
   - Takes 2 minutes to review

2. **📈 Progress Bar**
   - Shows match % improvement
   - Motivational messaging
   - "Keep learning, you're at 84%!"

3. **🔔 Smart Notifications**
   - High-match opportunity found (90%+)
   - Dream company job posted
   - Referral responded

### What They Do Weekly?

1. **Apply to 1-2 strong opportunities**
   - Confidence from match score
   - Referral suggestions
   - Interview prep preview

2. **Message referrals**
   - Auto-generated messages
   - Track responses

3. **Review skill gaps**
   - Prioritize by ROI
   - Start learning top skills

4. **Study interview prep**
   - Company-specific guide
   - Practice problems

---

## ⚡ Key Product Moments

### Moment 1: Profile Complete
```
✅ Resume uploaded
✅ Dream companies selected
✅ Review extracted skills

"Perfect! Your AI is ready to find opportunities.
Check back often to see daily matches."
```

### Moment 2: First 90%+ Match
```
🔥 EXCEPTIONAL OPPORTUNITY FOUND

"Adobe Platform Engineer at 91% match!
You have all the core skills.
Apply in the next 24 hours — position is hot."
```

### Moment 3: Referral Found
```
🤝 PERFECT REFERRAL DISCOVERED

"Sarah Chen at Adobe (92% match referral score)
She has your exact tech stack and experience level.
Here's a personalized message ready to send."
```

### Moment 4: Month Progress
```
📈 YOUR GROWTH THIS MONTH

Last Month: 72% average match
Today: 84% average match
+12% improvement! 🎉

You're getting closer to your dream roles.
Keep learning. You're on track.
```

---

## 🎨 Design Principles

1. **Confidence Through Data**
   - Match scores aren't hidden
   - Show why matches work
   - Be honest about gaps

2. **Action-Oriented**
   - Every card suggests next step
   - "Apply Now" vs "Learn Gaps"
   - "Message Referral" vs save

3. **Motivational**
   - Show progress visually
   - Celebrate small wins
   - Monthly recaps

4. **Transparent Workflow**
   - Clear step-by-step process
   - No hidden calculations
   - Explainability matters

---

## 📊 Analytics to Track

```csharp
public class DashboardMetrics
{
	// User Engagement
	public int DailyActiveUsers { get; set; }
	public int TimeSpentOnDashboard { get; set; }
	public int OpportunitiesViewed { get; set; }

	// Conversion
	public int OpportunitiesAppliedTo { get; set; }
	public decimal ApplyConversionRate { get; set; } // Apply % of viewed
	public decimal ReferralOutreachRate { get; set; }

	// Quality
	public decimal AverageMatchScore { get; set; }
	public int InterviewScheduledRate { get; set; }
	public int OfferRate { get; set; }

	// Retention
	public int DaysSinceLastVisit { get; set; }
	public decimal MonthlyRetention { get; set; }
}
```

---

## 🚀 Implementation Priority

### Week 1-2: Opportunity Radar Core
- [ ] OpportunityRadarService (already designed ✅)
- [ ] OpportunityRadarDashboard.razor
- [ ] Daily opportunity scraping

### Week 3: Dream Company Integration
- [ ] Wire DreamCompanyIntelligenceService
- [ ] Show match scores on dashboard
- [ ] Skill gaps panel

### Week 4: Referral Integration
- [ ] Wire ReferralIntelligenceService
- [ ] Show referral candidates
- [ ] Message generation

### Week 5: Interview Prep
- [ ] Wire InterviewDigestService
- [ ] Show prep guide on dashboard

### Week 6: Progress Tracking
- [ ] Analytics accumulation
- [ ] Progress visualization
- [ ] Month-over-month comparison

---

**This flow keeps users coming back because:**
1. ✅ New opportunities daily (fresh reason to check)
2. ✅ Clear progress tracking (motivation)
3. ✅ Actionable insights (confidence)
4. ✅ Next steps obvious (low friction)
5. ✅ Community aspect (referrals + messaging)
