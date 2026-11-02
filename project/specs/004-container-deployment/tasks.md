# Container preparation tasks and evidence

- [x] D-1 (AC-1, AC-2): Add host referencing backend; publish and verify live=200,
  ready=503 with reason, absent business route=404 through real loopback HTTP.
- [x] D-2 (AC-3, AC-4): Prepare multi-stage Dockerfile, context exclusions and restricted
  Deployment/ClusterIP Service with distinct probes. Restricted image execution verified below.
- [x] D-3 (AC-5): Document build, image smoke, digest substitution and dry-run workflow.
- [x] D-4a (AC-3, AC-5): Build and smoke the Linux image under restricted security settings.
- [ ] D-4b (AC-4, AC-5): Validate manifests with kubectl against the intended target.
- [ ] D-5: Implement real adapters, production authentication and dependency-aware
  readiness before any production rollout. Product choices are outside packaging scope.

Choice: [DEC-037](../../../DECISIONS.md#dec-037-container-hosting-preparation), pending user review.

Evidence (2026-10-30): package-free restore using an explicit empty-source NuGet config
and workspace-local APPDATA/DOTNET_CLI_HOME succeeded after the default user config was
inaccessible. `dotnet publish project/src/Translation/Host/Translation.Host.csproj -c Release
--no-restore -o outputs/deployment-check/publish /p:UseAppHost=false` passed.
`powershell -NoProfile -ExecutionPolicy Bypass -File project/infrastructure/Test-Host.ps1
-PublishedDirectory outputs/deployment-check/publish` passed all three HTTP checks.
Further review and validation results are appended below. No image build, registry push,
cluster dry-run or deployment has been performed.

Final checks: existing backend regressions passed 17/17. The manifest renderer preserves
both Deployment and Service, replaces the sole image placeholder, and rejects mutable
image tags without creating output. Smoke check rejects an occupied port. Both PowerShell
scripts pass parser validation. All 118 local file links across the affected records and
referenced ledger passed; tracked diff whitespace passed. New source/config/document
files were separately checked for trailing whitespace. Independent review-with-refuter
returned COMMENT: no retained packaging-code blocker, with unrun image/cluster checks as
explicit gates. The reported stale backend-host wording was corrected and ADR 002 added.
Review independently reproduced the three HTTP outcomes and manifest rendering.

Scope confirmed by user on 2026-10-30: prepare container/Kubernetes files and leave business endpoints blocked until adapters are chosen. Adapter implementation is deferred.

Next: complete D-4b against the intended Kubernetes target. Preparation is reviewable, not a production-ready deployment. Source is prepared on feature/deployment; the tests worktree remains separate.

## Image execution evidence (2026-10-30)

After Docker Desktop/WSL setup, Docker client/server 29.9.2 on `desktop-linux` was reachable.
`docker build --pull -t translation-backend:local project/src/Translation` succeeded.
Local image ID: `sha256:64bae0c5b9ce512b0cafdd1e1321b9738710ee338d062e507bd25bdbc29d05c2`,
platform linux/amd64. This is a local image identifier, not a pushed registry digest.

`docker run -d --rm --read-only --cap-drop ALL --security-opt no-new-privileges
--tmpfs /tmp:rw,noexec,nosuid,size=64m -p 127.0.0.1::8080 translation-backend:local`
started successfully. Real HTTP checks passed: live 200/alive, ready 503/not_ready with
business_dependencies_not_configured, /books 404. `docker exec <id> id` confirmed UID/GID
1654; inspect confirmed read-only root, dropped ALL capabilities and no-new-privileges.
The backend assembly exists at /app/Translation.Backend.dll. The temporary container was
stopped and automatically removed. No registry push or Kubernetes deployment occurred.
This supersedes earlier unavailable-Docker/image-validation statements; cluster validation
and real adapters remain pending. No code changes were required for the successful build.
