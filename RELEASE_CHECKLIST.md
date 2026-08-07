# Release Checklist for HireKarlo v{VERSION}

## Pre-Release

- [ ] Update version in:
  - [ ] `package.json` (NPM)
  - [ ] `src/Sdks/HireKarlo.Sdk/HireKarlo.Sdk.csproj` (NuGet)
  - [ ] `src/Extensions/*/manifest.json` or `package.json` (Extensions)

- [ ] Update `README.md` with new features

- [ ] Update `CHANGELOG.md` with:
  - Features
  - Bug Fixes
  - Breaking Changes
  - Migration Guide (if needed)

- [ ] Run full test suite:
  ```bash
  dotnet test
  npm test
  ```

- [ ] Code review completed

- [ ] All CI/CD checks passing

## Release

### 1. Create Release on GitHub
```bash
git tag -a v{VERSION} -m "Release v{VERSION}"
git push origin v{VERSION}
```

### 2. Publish NPM Package
```bash
npm version {VERSION}
npm publish
```

### 3. Publish NuGet Package
```bash
dotnet pack -c Release
dotnet nuget push bin/Release/HireKarlo.Sdk.{VERSION}.nupkg \
  -k {NUGET_API_KEY} \
  -s https://api.nuget.org/v3/index.json
```

### 4. Publish Docker Image
```bash
docker build -t hirekarlo/api:latest -t hirekarlo/api:{VERSION} .
docker push hirekarlo/api:{VERSION}
docker push hirekarlo/api:latest
```

### 5. Publish VS Code Extension
```bash
vsce publish {VERSION}
```

### 6. Publish Chrome Extension
```bash
chrome-webstore-upload upload \
  --source dist/extension-chrome.zip \
  --extension-id {CHROME_EXTENSION_ID} \
  --client-id {CHROME_CLIENT_ID} \
  --client-secret {CHROME_CLIENT_SECRET} \
  --refresh-token {CHROME_REFRESH_TOKEN}
```

### 7. Publish Firefox Extension
```bash
web-ext submit \
  --api-key={FIREFOX_API_KEY} \
  --api-secret={FIREFOX_API_SECRET}
```

## Post-Release

- [ ] Create GitHub Release with:
  - Release notes
  - Links to packages (NPM, NuGet, Docker)
  - Download links for extension builds

- [ ] Update website/documentation

- [ ] Announce in:
  - [ ] GitHub Discussions
  - [ ] Email newsletter (if applicable)
  - [ ] Social media

- [ ] Monitor for issues

- [ ] Update CDN/distribution caches

## Version Format

Follows Semantic Versioning: `{MAJOR}.{MINOR}.{PATCH}`

- **MAJOR**: Breaking changes
- **MINOR**: New features (backward compatible)
- **PATCH**: Bug fixes and minor updates

Example: v2.0.0 → v2.1.0 (feature) → v2.1.1 (patch)
