# HireKarlo v2.0 - Quick Reference Guide

## 🎯 What Changed?

### Strategic Pivot
```
❌ OLD THINKING: "Build a browser extension"
✅ NEW THINKING: "Build an AI Career Operating System"
				 (Extensions are just one distribution channel)
```

### Core Realization
The **missing piece** was **Opportunity Radar** — the daily dashboard that ties everything together and keeps users engaged.

---

## 📊 The 5-Feature Platform

```
				 USER OPENS APP
					  ↓
			  ┌────────────────────┐
			  │ OPPORTUNITY RADAR  │ ← CENTERPIECE
			  │ (Daily Dashboard)  │
			  └────────────┬───────┘
						   ↓
		┌──────────┬──────────┬──────────┬──────────┐
		↓          ↓          ↓          ↓          ↓

	Dream Co   Referral   Interview   Skill ROI   Stats
	Match      Intelligence Digest    Engine     Progress
	(Why?)     (Who?)       (What?)    (Learn?)   (Growth?)
```

**User Sees:** One dashboard. Click on any card → full analysis.

---

## 🚀 Implementation Order (What to Build First)

### **Week 1-2: Opportunity Radar** 🔴 START HERE
- [ ] Wire `OpportunityRadarService` into DI
- [ ] Job board scraper (LinkedIn, Indeed basic)
- [ ] Score jobs against resume
- [ ] Build Blazor dashboard layout
- [ ] Show top 10 with match %

### **Week 3-4: Dream Company Integration**
- [ ] Wire `DreamCompanyIntelligenceService`
- [ ] Add company match cards to dashboard
- [ ] Show skill gaps panel

### **Week 5-6: Referral Integration**
- [ ] Wire `ReferralIntelligenceService`
- [ ] LinkedIn integration (basic)
- [ ] Show referral candidates
- [ ] Test message generation

### **Week 7-8: Interview Digest**
- [ ] Wire `InterviewDigestService`
- [ ] Setup Blind/Levels.fyi scrapers
- [ ] RAG pipeline for aggregation
- [ ] Interview prep panel

### **Week 9-10: Dashboard Polish**
- [ ] Responsive design
- [ ] Dark mode
- [ ] Loading states
- [ ] Error handling

### **Week 11-12: Analytics + Deploy**
- [ ] Progress tracking graph
- [ ] Month-over-month comparison
- [ ] Deploy to Render + Neon
- [ ] GitHub Pages for marketing site

---

## 💾 Code Files Ready to Use

| File | Lines | Status | What to Do |
|------|-------|--------|-----------|
| `OpportunityRadarService.cs` | 900+ | ✅ Ready | Wire into Startup.cs |
| `DreamCompanyIntelligenceService.cs` | 600+ | ✅ Ready | Wire into Startup.cs |
| `ReferralIntelligenceService.cs` | 700+ | ✅ Ready | Wire into Startup.cs |
| `InterviewDigestService.cs` | 750+ | ✅ Ready | Wire into Startup.cs |

**Total:** 2,950+ lines of service logic ready to implement

---

## 🎨 Blazor Components to Build

```
CareerDashboard.razor (Main Page)
├─ OpportunityRadarPanel.razor
│  ├─ OpportunityCard.razor (each job)
│  └─ [View All] Modal
│
├─ DreamCompanySection.razor
│  ├─ CompanyMatchCard.razor (each company)
│  └─ [Detailed Analysis] Modal
│
├─ SkillGapsPanel.razor
│  ├─ SkillGapRow.razor
│  └─ [Learning Path] Modal
│
├─ ReferralPanel.razor
│  ├─ ReferralCard.razor
│  └─ [Message] Composer
│
├─ InterviewPrepPanel.razor
│  └─ [Full Study Guide] Link
│
└─ ProgressTrackingComponent.razor
   ├─ TrendChart.razor
   └─ StatBox.razor (4 key metrics)
```

---

## 📱 UI Flow (High Level)

```
DESKTOP VIEW:
┌─────────────────────────────────────────────────┐
│ Header (HireKarlo Logo + User Menu)             │
├─────────────────────────────────────────────────┤
│                                                 │
│ 📡 Opportunity Radar (60% width)                │
│ ┌───────────────────────────────────────────┐  │
│ │ Job 1: 91%                                │  │
│ │ Job 2: 87%                                │  │
│ │ Job 3: 82%                                │  │
│ └───────────────────────────────────────────┘  │
│                                                 │
│                          📊 Intelligence (40%)  │
│                          ┌───────────────────┐  │
│                          │ Match Scores      │  │
│                          │ Skill Gaps        │  │
│                          │ Referrals         │  │
│                          │ Interview Prep    │  │
│                          └───────────────────┘  │
│                                                 │
│ 📈 Progress Tracking (full width)               │
│ ┌───────────────────────────────────────────┐  │
│ │ Last Month: 72% → Today: 84% (📈 +12%)  │  │
│ └───────────────────────────────────────────┘  │
│                                                 │
└─────────────────────────────────────────────────┘

MOBILE VIEW:
Same cards, vertical stacking
Hamburger menu for intelligence panels
```

---

## 🔗 Data Flow

```
User Resume
	↓
Parse Skills + Experience
	↓
Store in Vectors (pgvector)
	↓
Scrape Job Boards Daily
	↓
For Each Job:
├─ Extract requirements
├─ Compare with user skills
├─ Calculate match % (0-100)
├─ Identify matched skills
├─ Identify gap skills
└─ Store in DB
	↓
Dashboard Retrieves Top 10
	↓
Add dream company highlighting
	↓
Add referral opportunities (if any)
	↓
Add interview prep link
	↓
DISPLAY TO USER
```

---

## 🎯 Key Insights to Keep

1. **Opportunity Radar is the CENTER**
   - Everything else supports it
   - Users open for daily opportunities
   - This is the habit loop

2. **Match Scores Build Confidence**
   - 91% = "Apply immediately"
   - 75% = "Good fit, consider"
   - 50% = "Learn the gaps first"
   - Gives users agency

3. **Dream Companies Drive Motivation**
   - Personalization ← Dream company selection
   - Progress tracking ← Match improvement over time
   - Referrals ← Targeted company focus

4. **Referral Intelligence is the Secret Weapon**
   - 80%+ callback rate from referrals
   - Auto-discovers best contacts
   - Generates personalized messages
   - This differentiates HireKarlo

5. **Interview Prep is the Trust Builder**
   - Reduces anxiety
   - Shows specific what to study
   - RAG pipeline = advanced AI use case
   - Portfolio-worthy feature

---

## 📈 Metrics That Matter

Track these to know if you're winning:

```
ENGAGEMENT:
- Daily Active Users (target: 50+)
- Session duration (target: 15-30 min)
- Dashboard visits per user per week (target: 3-4)

QUALITY:
- Match accuracy vs actual interview conversion (90%+ = good)
- Referral score calibration (higher score = higher callback)
- Interview prep study time (target: 10+ hours/user)

CONVERSION:
- Application rate (% of viewed opportunities)
- Dream company opportunity rate (% at target companies)
- Referral outreach rate (% of discovered referrals)

RETENTION:
- 7-day retention (target: 40%+)
- 30-day retention (target: 20%+)
- Churn rate (target: <5% per month)

BUSINESS:
- Feature adoption rate
- Premium conversion rate (if you add premium)
- Net Promoter Score (target: 50+)
```

---

## 🚢 Deployment Checklist

Before going live:

- [ ] All services injected into DI container
- [ ] Database migrations tested
- [ ] API endpoints secured (JWT auth)
- [ ] Job scraper tested (no rate limit issues)
- [ ] LinkedIn integration tested (privacy compliant)
- [ ] Blazor components responsive
- [ ] Error handling comprehensive
- [ ] Logging in place
- [ ] Performance tested (dashboard loads <2s)
- [ ] Security reviewed
- [ ] Analytics tracking added
- [ ] Help/FAQ updated
- [ ] Deploy to Render
- [ ] Point custom domain
- [ ] Monitor error logs
- [ ] Get early user feedback

---

## 💬 Talking Points

**When explaining to recruiters:**
> "I built Opportunity Radar, which replaces passive job boards with active opportunity discovery. Each day, my AI finds 1000s of jobs, scores them by match %, and surfaces the top 10 with detailed analysis: why you match, what you're missing, referrals to contact, and company-specific interview prep."

**When explaining to users:**
> "Instead of browsing endlessly and wondering if you're qualified, Opportunity Radar tells you exactly how well you fit each job. It shows your matched skills, identifies gaps you can learn, finds people to refer you, and preps you for interviews at companies you care about."

**When explaining to engineers:**
> "It's built with .NET 9, PostgreSQL with pgvector, and a RAG pipeline using Groq LLM. The unusual architecture: everything converges to one intelligent dashboard that learns from your behavior."

---

## 🎁 Quick Win Ideas (Build After Phase 2)

1. **Saved Opportunities** — Bookmark jobs to apply to later
2. **Application Tracker** — Track applications → interviews → offers
3. **Salary Data** — Show market rates for roles
4. **Company Comparisons** — "Adobe vs Atlassian deep dive"
5. **Skill Recommendations** — "These 2 people did your job 2 years ago, now CTO. Here's their path."
6. **Peer Benchmarking** — "Your match % vs other applicants"
7. **Mock Interview Scheduling** — Connect with peers
8. **Learning Groups** — Find people learning same skills
9. **Mentor Matching** — Connect with someone at dream company
10. **Weekly Digest Email** — Summarize your week in 2-min read

---

## 🏁 Success = Users Come Back Daily

This is the real metric. If users open the dashboard each morning to:
- See new opportunities ✅
- Check match improvements ✅
- Message a referral ✅
- Study interview prep ✅

Then you've created **habit**.

That's when you know you've built something valuable.

---

**Ready to ship Phase 2. Let's go.** 🚀
