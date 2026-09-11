# Migrating a Connector/Enricher to Multi-Version Targeting

This document tracks the migration of `CluedIn.Enricher.libpostal` from a single-version build to
the multi-version targeting pattern, part of a broader effort migrating CluedIn connector/enricher
repos. Modeled on the docs already written for `CluedIn.Connector.Dataverse.V2`,
`CluedIn.Enricher.GoogleMaps`, `CluedIn.Enricher.Gleif`, `CluedIn.Enricher.OpenCorporates`,
`CluedIn.Enricher.Permid`, `CluedIn.Enricher.Brreg`, `CluedIn.Enricher.KnowledgeGraph`,
`CluedIn.Enricher.ClearBit`, `CluedIn.Enricher.CompanyHouse`, and `CluedIn.Enricher.CVR` — all
completed earlier in the same effort.

Branch: `feature/multi-version-targeting` (off `develop`).

---

## Overview

| CluedIn version | .NET TFM | Package suffix |
|---|---|---|
| 4.7.0 | net6.0 | `.470` |
| 4.8.0 | net6.0 | `.480` |
| 5.0.0-beta.* | net10.0 | `.500` |

Verified independently for this repo's own feeds: `5.0.0-*` resolves to a `5.0.0-beta.*` prerelease
(consistent with every other repo migrated so far). 4.6.0 excluded — no evidence this small
`ExternalSearchProvider` needs it (no stream-repository or other 4.6/4.7-gated API usage).

This repo calls a remote libpostal HTTP service via RestSharp — not a native/P-Invoke binding to
`libpostal` itself, despite the name.

---

## Step 1 — Pipeline template (`azure-pipelines.yml`)

Status: **Done**

Replaced the single-version `crawler.build.yml` steps-template (with explicit `UseDotNet@2`
installing 8.0, and `pool: vmImage: windows-latest`) with the multi-version `crawler.build.jobs.yml`
jobs-template on `ubuntu-22.04`. Added `probeCluedInVersion`, `multiVersionCluedInTargets`, and
`useGitVersionDotNetTool: true` explicitly — omitting the latter caused `CluedIn.Enricher.Permid`'s
first CI run to fail all three legs on a retired legacy GitVersion task. Dropped
`createIntegrationEnvironmentScriptFilePath`/`Arguments` pointing at `./build/integration-test.ps1`
— that script doesn't exist in this repo (same dead reference GoogleMaps' repo had).

---

## Step 2 — `Directory.Build.props`

Status: **Done**

Honours `CluedInMultiVersionTargetFramework` (net10.0 local fallback), derives
`CLUEDIN_V47`/`V48`/`V50` `DefineConstants`, pins `LangVersion` to 13.0 up front (several prior
repos hit `CS8936` on net6.0 without this, though this repo didn't actually need it — pinned
defensively anyway).

---

## Step 3 — `Packages.props`

Status: **Done**

Guarded `_CluedIn`. Added TFM-conditional test package overrides (`Microsoft.NET.Test.Sdk` 17.12.0
vs 18.3.0, `AutoFixture.Xunit2` 4.18.0 vs `AutoFixture.Xunit3`, `xunit` 2.9.3 vs `xunit.v3`,
`xunit.runner.visualstudio` 2.8.2 vs 3.1.5) — same versions as the MasterDataServices/GoogleMaps
precedent.

---

## Step 4 — `NuGet.config` casing

Status: **Done**

`git mv`'d `Nuget.config` → `NuGet.config` (two-step rename). The `.sln` already referenced the
correct casing. No extra feed needed — `CluedIn.Core` restored cleanly at `4.7.0` and `4.8.0`
against this repo's existing `nuget.org`/`develop`/`release`/`AzurePipelines` feeds (unlike
AzureEventHubs, which needed an extra `public` feed).

---

## Step 5 — Test projects

Status: **Done**

- Stripped `test/Directory.Build.props` down to just `IsTestProject` — removed the unconditional
  `AutoFixture.Xunit3`/`xunit.v3`/etc. `PackageReference`s (would've clashed with the conditional
  v2 packages added below on the net6.0 legs — the CS0433 trap the MasterDataServices doc
  describes).
- **Deleted `test/unit/Directory.Build.props`.** Dead scaffold — pinned ancient `Moq 4.5.30`/
  `Should 1.1.20` (bypassing central package management), but no `test/unit` csproj exists to
  consume it (same dead-scaffold pattern GoogleMaps' repo had).
- Added conditional `ItemGroup`s directly to
  `test/integration/Integration.Tests/ExternalSearch.libpostal.Integration.Tests.csproj` (which was
  otherwise an empty SDK-only project relying entirely on the now-stripped ambient
  `test/Directory.Build.props`): xunit v3 + `AutoFixture.Xunit3` under `CLUEDIN_V50`, xunit v2 +
  `AutoFixture.Xunit2` otherwise.
- No `GlobalUsings.cs` needed — the one integration test class (`libpostalTests.cs`) is entirely
  empty, no AutoFixture/namespace usage at all.
- A stray, clearly-unfinished template file `libpostalProviderTestBase.cs` sits at the repo root
  (contains literal `_Name_` placeholder text, doesn't even compile as-is) — not referenced by any
  `.csproj`, so it's dead and out of scope for this migration; left untouched.

---

## Step 6 — API compatibility audit across 4.7.0 / 4.8.0 / 5.0.0-beta.*

Status: **Done** — both src projects and the integration test project verified via real
`dotnet build` for all three legs.

### RestSharp 106-vs-114 break

Same family every enricher migrated so far has hit. Two call sites in
`libpostalExternalSearchProvider.cs`:
- `Method.Post` (PascalCase, RestSharp 114+) vs `Method.POST` (uppercase, RestSharp 106) — 2 call
  sites (`ExecuteSearch`, `VerifyConnection`), guarded with `#if CLUEDIN_V50`.
- `ConstructVerifyConnectionResponse`'s parameter type — `RestResponse` (concrete class, 114+) vs
  `IRestResponse` (interface, 106) — guarded the method signature.

No other API breaks — no custom `ISerializer`, no transitive-dependency version surprise like
Brreg/CompanyHouse/CVR/DuckDuckGo hit with `Nager.PublicSuffix` (this repo doesn't reference it).

### Repo-specific issue found only by real CI, not local builds: `ProjectReference` casing

**First CI run (build 151983) failed all three legs** with `CS0234: The type or namespace name
'Providers' does not exist in the namespace 'CluedIn.ExternalSearch'` in
`Provider.ExternalSearch.Libpostal.csproj`. Root cause, visible in the log:

```
Skipping project ".../src/ExternalSearch.Providers.Libpostal/ExternalSearch.Providers.Libpostal.csproj" because it was not found.
```

`Provider.ExternalSearch.Libpostal.csproj`'s `<ProjectReference>` points at
`..\ExternalSearch.Providers.Libpostal\ExternalSearch.Providers.Libpostal.csproj` (capital `L`), but
the actual folder/file on disk is `ExternalSearch.Providers.libpostal` (lowercase `l`, both the
directory and the `.csproj` filename). This is a **pre-existing bug in the repo**, unrelated to
this migration — it was never caught before because the old pipeline ran on
`pool: vmImage: 'windows-latest'`, and Windows' case-insensitive filesystem silently tolerates the
mismatch. Switching the pool to `ubuntu-22.04` (matching every other migrated repo) exposed it, the
same category of bug the MasterDataServices doc's "Linux case-sensitivity" section describes for
resource files, just hitting a `ProjectReference` path instead. Local builds on this (Windows) dev
machine never caught it either, for the same reason.

Fixed by correcting the `ProjectReference` path to match the real on-disk casing. The `.sln`'s
project entry has the same casing mismatch but is unaffected — the multi-version template discovers
and builds `.csproj` files directly via `Get-ChildItem -Recurse -Filter *.csproj`, never touching
the `.sln`, so it was left alone as out of scope.

---

## Step 7 — Reset the semantic version (`GitVersion.yml`)

Status: **Done**

This repo's `GitVersion.yml` already had a pre-existing `ignore: sha: []` block — merged
`commits-before` into it rather than adding a second top-level `ignore:` key (a second key is valid
YAML but silently clobbers the first, a trap `CompanyHouse` and `CVR` both hit and documented).

```yaml
next-version: 1.0
ignore:
  sha: []
  commits-before: 2026-06-20T00:00:00
```

Highest pre-existing tag by actual commit date is `4.6.2` at `2026-06-17T17:26:08+10:00` (not any
of the `v4.x` tags, which are all earlier despite alphanumeric sort suggesting otherwise). Padded 2+
days past it per the Gleif/OpenCorporates finding that `GitVersion.Tool 5.9.0` parses this cutoff
using local machine time, not UTC, and fails silently (no error, just doesn't reset) if the margin
is too tight.

Verified directly with the pipeline's pinned `GitVersion.Tool 5.9.0`: `FullSemVer:
"1.0.0-multi-version-targeting.91"` — confirms `MajorMinorPatch` really is `1.0.0`.

---

## Checklist

- [x] `azure-pipelines.yml` — switched to `crawler.build.jobs.yml` with `multiVersionCluedInTargets` (4.7.0, 4.8.0, 5.0.0-beta.*); `useGitVersionDotNetTool: true`; dead integration-test script reference removed
- [x] `Directory.Build.props` — honours `CluedInMultiVersionTargetFramework` with net10.0 local fallback; `DefineConstants` derived; `LangVersion` pinned to 13.0
- [x] `Packages.props` — `_CluedIn` guarded; TFM-conditional test package versions added
- [x] `NuGet.config` — renamed from `Nuget.config`; feeds confirmed sufficient as-is
- [x] `test/Directory.Build.props` — stripped to properties only
- [x] `test/unit/Directory.Build.props` — deleted (dead scaffold, no consuming project)
- [x] Integration test csproj — conditional xunit v2/v3 + AutoFixture selection added
- [x] Source — `#if CLUEDIN_V50` guards for the RestSharp `Method.Post`/`POST` and
      `ConstructVerifyConnectionResponse` parameter-type breaks (3 call sites)
- [x] Fixed a pre-existing `ProjectReference` casing bug (`Libpostal` vs `libpostal`) that only
      surfaced once CI moved from `windows-latest` to `ubuntu-22.04` — caught by real CI, not local
      builds
- [x] `GitVersion.yml` — merged into existing `ignore:` block; `next-version: 1.0`;
      `commits-before: 2026-06-20T00:00:00`; verified `1.0.0` with the pinned GitVersion.Tool 5.9.0
- [x] Push branch and confirm the actual Azure DevOps pipeline run is green end-to-end — PR #38: first run (build 151983) failed all three legs on the `ProjectReference` casing bug above; fixed, re-ran (build 151988), all three legs + `Multi-version: publish` passed
