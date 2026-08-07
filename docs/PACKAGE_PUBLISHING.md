# Package Publishing & Distribution Guide

## 📦 NuGet Package (.NET SDK)

### Planned for v3.1 (June 2025)

#### Package Structure
```
HireKarlo.Sdk (Main package)
├── HireKarlo.Sdk.Core
│   ├── Models (Opportunity, JobMatch, SkillGap)
│   ├── Services (OpportunityService, MatchService)
│   └── Repositories (ICacheRepository)
│
├── HireKarlo.Sdk.Http
│   ├── HttpClientFactory
│   ├── AuthenticationHandler
│   └── RequestRetryPolicy
│
└── HireKarlo.Sdk.AI
	├── Groq Integration
	├── Embedding Service
	└── Match Score Calculator
```

#### Publishing to NuGet.org

```bash
# 1. Create HireKarlo.Sdk.csproj
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
	<TargetFrameworks>net6.0;net7.0;net8.0;net9.0</TargetFrameworks>
	<PackageId>HireKarlo.Sdk</PackageId>
	<Version>3.1.0</Version>
	<Authors>K-riti</Authors>
	<PackageProjectUrl>https://github.com/K-riti/HireKarlo</PackageProjectUrl>
	<PackageLicense>MIT</PackageLicense>
	<IsPackable>true</IsPackable>
	<GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  </PropertyGroup>
</Project>

# 2. Build package
dotnet pack -c Release -o nupkg

# 3. Push to NuGet.org
dotnet nuget push nupkg/HireKarlo.Sdk.3.1.0.nupkg \
  -k $NUGET_API_KEY \
  -s https://api.nuget.org/v3/index.json

# 4. Users install:
dotnet add package HireKarlo.Sdk --version 3.1.0
```

#### Usage Example
```csharp
using HireKarlo.Sdk;

var client = new HireKarloClient(
	configureUri: () => "https://hirekarlo-api.onrender.com",
	configureApiKey: () => "your-api-key-here"
);

// Find opportunities
var opportunities = await client.Opportunities
	.FindAsync(userId, limit: 10);

foreach (var opp in opportunities) {
	Console.WriteLine($"{opp.Title} @ {opp.Company} ({opp.MatchScore}%)");
}

// Calculate match for custom job
var customJob = new JobDescription {
	Title = "Senior Engineer",
	Description = "...",
};
var matchScore = await client.Matches.CalculateAsync(userId, customJob);
```

---

## 📦 NPM Package (JavaScript/TypeScript SDK)

### Planned for v3.1 (June 2025)

#### Package Structure
```
hirekarlo-sdk (Main package on npm)
├── dist/
│   ├── esm/         (ES modules)
│   ├── cjs/         (CommonJS)
│   └── types/       (TypeScript definitions)
├── src/
│   ├── Client.ts
│   ├── models/
│   │   ├── Opportunity.ts
│   │   ├── JobMatch.ts
│   │   └── SkillGap.ts
│   └── services/
│       ├── OpportunityService.ts
│       ├── MatchService.ts
│       └── SkillService.ts
└── package.json
```

#### Publishing to npm

```bash
# 1. Build TypeScript
npm run build

# 2. Publish
npm publish

# 3. Users install:
npm install hirekarlo-sdk
```

#### Usage Example (TypeScript)
```typescript
import { HireKarloClient } from 'hirekarlo-sdk';

const client = new HireKarloClient({
	baseUrl: 'https://hirekarlo-api.onrender.com',
	apiKey: 'your-api-key'
});

// Find opportunities
const opportunities = await client.opportunities.find({
	userId,
	limit: 10
});

opportunities.forEach(opp => {
	console.log(`${opp.title} @ ${opp.company} (${opp.matchScore}%)`);
});

// Calculate match
const matchScore = await client.matches.calculate({
	userId,
	jobDescription: {
		title: 'Senior Engineer',
		description: '...'
	}
});
```

---

## 📦 Docker Distribution

### Planned for v2.5 (Dec 2024)

#### Docker Images

```bash
# Multiarch images (amd64, arm64, armv7)
docker pull ghcr.io/k-riti/hirekarlo:2.5.0
docker pull ghcr.io/k-riti/hirekarlo:2.5.0-api
docker pull ghcr.io/k-riti/hirekarlo:2.5.0-web

# Also on Docker Hub (mirror)
docker pull hirekarlo:2.5.0
```

#### Dockerfile Examples

**Dockerfile.api**
```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet build HireKarlo.slnx -c Release

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /build .
EXPOSE 5000
ENTRYPOINT ["dotnet", "HireKarlo.Api.dll"]
```

**Dockerfile.web**
```dockerfile
FROM node:18-alpine AS build-npm
WORKDIR /app
COPY src/Presentation/HireKarlo.Web ./
RUN npm ci && npm run build

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build-npm /app/dist ./wwwroot
EXPOSE 5000
ENTRYPOINT ["dotnet", "HireKarlo.Web.dll"]
```

#### Publishing to Docker Hub / GitHub Container Registry

```bash
# Login
docker login
# or
echo $GITHUB_TOKEN | docker login ghcr.io -u k-riti --password-stdin

# Build multiarch images
docker buildx create --name builder
docker buildx use builder
docker buildx build \
  --platform linux/amd64,linux/arm64,linux/armv7 \
  -t hirekarlo:2.5.0 \
  -t ghcr.io/k-riti/hirekarlo:2.5.0 \
  --push .

# Also publish to Docker Hub
docker tag hirekarlo:2.5.0 hirekarlo:latest
docker push hirekarlo:2.5.0
docker push hirekarlo:latest
```

#### docker-compose.yml Usage
```yaml
version: '3.8'

services:
  api:
	image: hirekarlo:2.5.0-api
	ports:
	  - "5000:5000"
	environment:
	  - DATABASE_URL=postgresql://user:pass@db:5432/hirekarlo
	  - Groq__ApiKey=${GROQ_API_KEY}
	  - HuggingFace__ApiKey=${HF_API_KEY}
	depends_on:
	  - db
	  - redis

  web:
	image: hirekarlo:2.5.0-web
	ports:
	  - "3000:5000"
	depends_on:
	  - api

  db:
	image: postgres:16-alpine
	environment:
	  - POSTGRES_PASSWORD=postgres
	volumes:
	  - pgdata:/var/lib/postgresql/data

  redis:
	image: redis:7-alpine

volumes:
  pgdata:
```

---

## 📦 VS Code Extension

### Planned for v3.0 (May 2025)

#### Publishing to Visual Studio Code Marketplace

```bash
# 1. Create extension structure
.
├── src/
│   ├── extension.ts
│   ├── webview/
│   └── api/
├── package.json
└── vscode.proposed.d.ts

# 2. Build
npm run compile

# 3. Create VSIX package
npm install -g vsce
vsce package

# 4. Publish to Marketplace
vsce publish -p $VSCODE_PAT
# (PAT = Personal Access Token from dev.azure.com)

# Users install:
# VS Code → Extensions → Search "HireKarlo"
# or: code --install-extension k-riti.hirekarlo
```

#### Extension Features
```typescript
// Main command: Show daily opportunities
export function activate(context: vscode.ExtensionContext) {
	let disposable = vscode.commands.registerCommand(
		'hirekarlo.showOpportunities',
		async () => {
			const panel = vscode.window.createWebviewPanel(
				'hirekarlo',
				'HireKarlo Opportunities',
				vscode.ViewColumn.Two
			);

			// Fetch from API and render dashboard
			const client = new HireKarloClient(apiKey);
			const opps = await client.opportunities.find({ limit: 10 });

			panel.webview.html = renderDashboard(opps);
		}
	);
}
```

---

## 📦 Browser Extensions (Chrome & Firefox)

### Planned for v3.0 (May 2025)

#### Chrome Extension (Manifest v3)

**manifest.json**
```json
{
  "manifest_version": 3,
  "name": "HireKarlo - Smart Job Search",
  "version": "3.0.0",
  "permissions": ["activeTab", "scripting"],
  "host_permissions": [
	"https://linkedin.com/*",
	"https://indeed.com/*",
	"https://wellfound.com/*"
  ],
  "content_scripts": [{
	"matches": ["<all_urls>"],
	"js": ["content.js"]
  }],
  "background": {
	"service_worker": "background.js"
  },
  "action": {
	"default_popup": "popup.html"
  }
}
```

**Publishing (Chrome Web Store)**
```bash
# Upload CRXZ file to Chrome Web Store Developer Dashboard
# https://chrome.google.com/webstore/devcenter

# Publish to Firefox Add-ons
# https://addons.mozilla.org/developers/
```

---

## 🔄 GitHub Actions CI/CD for Package Publishing

### Automated Release Workflow

**`.github/workflows/release.yml`**
```yaml
name: Release & Publish Packages

on:
  push:
	tags:
	  - 'v*.*.*'

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: ${{ github.repository }}

jobs:
  # 1. NuGet Package
  publish-nuget:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v4
	  - uses: actions/setup-dotnet@v4
		with:
		  dotnet-version: '9.0.x'
	  - run: dotnet pack -c Release -o nupkg src/Sdks/HireKarlo.Sdk/HireKarlo.Sdk.csproj
	  - run: dotnet nuget push "nupkg/*.nupkg" -k ${{ secrets.NUGET_API_KEY }} -s https://api.nuget.org/v3/index.json

  # 2. NPM Package
  publish-npm:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v4
	  - uses: actions/setup-node@v4
		with:
		  node-version: '18'
		  registry-url: 'https://registry.npmjs.org'
	  - run: npm ci
	  - run: npm run build:npm
	  - run: npm publish
		env:
		  NODE_AUTH_TOKEN: ${{ secrets.NPM_TOKEN }}

  # 3. Docker Images
  publish-docker:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v4
	  - uses: docker/setup-buildx-action@v3
	  - uses: docker/login-action@v3
		with:
		  registry: ghcr.io
		  username: ${{ github.actor }}
		  password: ${{ secrets.GITHUB_TOKEN }}
	  - uses: docker/build-push-action@v5
		with:
		  push: true
		  platforms: linux/amd64,linux/arm64,linux/armv7
		  tags: |
			ghcr.io/${{ env.IMAGE_NAME }}:${{ github.ref_name }}
			ghcr.io/${{ env.IMAGE_NAME }}:latest

  # 4. GitHub Release + Release Notes
  create-release:
	runs-on: ubuntu-latest
	steps:
	  - uses: actions/checkout@v4
	  - uses: softprops/action-gh-release@v1
		with:
		  files: .release/RELEASE_NOTES_${{ github.ref_name }}.md
		  draft: false
		  prerelease: false
		env:
		  GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```

---

## 📋 Publishing Checklist

- [ ] Version number updated (v2.0.0 → v2.1.0)
- [ ] CHANGELOG.md updated with new features
- [ ] Build succeeds: `dotnet build -c Release`
- [ ] Tests pass: `dotnet test`
- [ ] NuGet package created: `dotnet pack -c Release`
- [ ] NPM package built: `npm run build`
- [ ] Docker image built: `docker build -t hirekarlo:VERSION .`
- [ ] Git tag created: `git tag v2.1.0 && git push --tags`
- [ ] GitHub Release created with release notes
- [ ] Packages published (NuGet, npm, Docker)
- [ ] Documentation updated (README, roadmap)
- [ ] Announcement posted (GitHub Discussions, Twitter, etc.)

---

**Questions?** See [ROADMAP.md](ROADMAP.md) or [PROJECT_OVERVIEW.md](../docs/PROJECT_OVERVIEW.md)!
