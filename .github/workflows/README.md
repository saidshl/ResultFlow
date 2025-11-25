# GitHub Actions Workflows

This directory contains GitHub Actions workflows for the ResultFlow project.

## Workflows

### 1. CI Build (`ci.yml`)
**Trigger:** Automatically runs on every push and pull request to `master`, `main`, or `develop` branches.

**Purpose:** 
- Validates that the code builds successfully
- Runs all tests
- Validates that packages can be created
- Tests against multiple .NET versions (8.0 and 9.0)

**Matrix Strategy:** Tests on .NET 8.0 and 9.0 to ensure compatibility.

---

### 2. Publish NuGet Packages (`publish-nuget.yml`)
**Trigger:** 
- Automatically when a version tag is pushed (e.g., `v2.0.0`)
- Manually via workflow dispatch

**Purpose:**
- Builds the solution in Release mode
- Runs all tests to ensure quality
- Creates NuGet packages for:
  - `ResultFlow`
  - `ResultFlow.AspNetCore`
  - `ResultFlow.FluentValidation`
- Publishes packages to NuGet.org
- Uploads package artifacts for download

**Packages Published:**
1. **ResultFlow** - Core result pattern library
2. **ResultFlow.AspNetCore** - ASP.NET Core integration
3. **ResultFlow.FluentValidation** - FluentValidation integration

---

## Setup Instructions

### 1. Configure NuGet API Key

Before you can publish packages, you need to add your NuGet.org API key as a GitHub secret:

1. Go to [NuGet.org](https://www.nuget.org) and sign in
2. Navigate to your profile ? API Keys
3. Create a new API key with:
   - **Key Name:** GitHub Actions (or any descriptive name)
   - **Glob Pattern:** `*` (or specify `ResultFlow*` for security)
   - **Scopes:** Select "Push" and "Push new packages and package versions"
4. Copy the generated API key
5. Go to your GitHub repository ? Settings ? Secrets and variables ? Actions
6. Click "New repository secret"
7. Name: `NUGET_API_KEY`
8. Value: Paste your NuGet API key
9. Click "Add secret"

### 2. Publishing Packages

#### Option A: Using Git Tags (Recommended)
```bash
# Create and push a version tag
git tag v2.0.0
git push origin v2.0.0
```

This will automatically trigger the publish workflow.

#### Option B: Manual Trigger
1. Go to your repository on GitHub
2. Click "Actions" tab
3. Select "Publish NuGet Packages" workflow
4. Click "Run workflow"
5. Optionally specify a version
6. Click "Run workflow"

### 3. Monitoring Workflow Runs

1. Go to the "Actions" tab in your GitHub repository
2. Click on the workflow run you want to monitor
3. View logs for each step
4. Download artifacts if needed

---

## Versioning

The package versions are controlled by the `.csproj` files:
- `source/ResultFlow/ResultFlow.csproj`
- `source/ResultFlow.AspNetCore/ResultFlow.AspNetCore.csproj`
- `source/ResultFlow.FluentValidation/ResultFlow.FluentValidation.csproj`

Update the `<Version>` property in these files before creating a new release:

```xml
<Version>2.0.0</Version>
<AssemblyVersion>2.0.0.0</AssemblyVersion>
<FileVersion>2.0.0.0</FileVersion>
<InformationalVersion>2.0.0</InformationalVersion>
```

---

## Troubleshooting

### Build Failures
- Check that all project files compile locally first
- Ensure all tests pass locally
- Review the workflow logs in the Actions tab

### Publish Failures
- Verify `NUGET_API_KEY` secret is set correctly
- Ensure the API key has the correct permissions
- Check if the package version already exists (use `--skip-duplicate` flag)
- Verify package metadata is valid in `.csproj` files

### Package Not Appearing on NuGet.org
- It can take a few minutes for packages to be indexed
- Check your NuGet.org account for any validation errors
- Verify the package ID doesn't conflict with existing packages

---

## Best Practices

1. **Always test locally** before pushing tags
2. **Update version numbers** in all three `.csproj` files consistently
3. **Use semantic versioning** (Major.Minor.Patch)
4. **Create release notes** on GitHub for each version
5. **Review package contents** before publishing (check artifacts)

---

## Package Information

All packages are configured with:
- **License:** MIT
- **Repository:** https://github.com/saidshl/ResultFlow
- **Author:** Said Souhayel
- **Target Frameworks:** .NET 8.0, 9.0, 10.0
- **Source Link:** Enabled for debugging support
