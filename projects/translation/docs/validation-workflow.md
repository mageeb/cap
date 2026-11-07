# Change validation and DEV deployment workflow

This is the standard validation process for future changes in this repository.
A code change is complete only when applicable local checks and authorized cloud
checks pass, or the handoff explicitly states which gates remain blocked.
Record evidence; a successful build or a Running pod alone is not sufficient.

## 1. Establish scope and source

1. Read applicable `AGENTS.md` instructions and the feature requirements.
2. Inspect branch/status in every repository involved. Preserve unrelated changes
   and use a feature branch. Identify the exact source being changed and built.
3. Define acceptance checks for the changed behavior, including meaningful failure
   cases. Decide which unit, component, API and telemetry checks apply.
4. Confirm the intended Azure subscription, resource group and cluster. Reuse
   existing authorization; obtain missing authorization before cloud mutations.

Current environment (verify before using; do not assume these stay unchanged):

| Item | Current value |
|---|---|
| Deployment/configuration repository | `C:\Repos\cap\projects\translation` |
| Application deployment worktree | `C:\Repos\cap\outputs\worktrees\deployment` |
| Separate tests worktree | `C:\Repos\cap\outputs\worktrees\tests` |
| Subscription / resource group / region | `SPSD.Sub1` / `Translation-Dev` / East US |
| AKS cluster / namespace / deployment | `translation-dev-aks` / `translation-dev` / `translation-backend` |
| Image repository | `translationdevku67727xgop2y.azurecr.io/translation-backend` |
| Application Insights | `translation-dev-appinsights` |

Tests passing in a separate worktree do not prove that the deployment source
passes. Integrate or otherwise reconcile the relevant tests and run them against
the release source. Record source commit and working-tree changes; do not imply
that a commit hash describes uncommitted files baked into the image.

## 2. Make the change and run local tests

Keep the change focused. Add/update tests that exercise the actual behavior,
especially error handling, invalid inputs, boundary conditions and dependencies.
Prefer behavioral coverage over tests that duplicate implementation details.

Run commands from the application source root. These are the current executable
test harnesses, so use `dotnet run`, not an assumed `dotnet test` invocation:

```powershell
dotnet run --project project/src/Translation/Backend/Tests/Translation.Backend.Tests.csproj
dotnet run --project project/src/Translation/Host.ApiTests/Translation.Host.ApiTests.csproj
dotnet run --project project/src/Translation/Host.Tests/Translation.Host.Tests.csproj
```

- Backend: unit/component behavior for application services.
- Host API: HTTP contracts, dependency/identity gates, binding, error mapping and cancellation.
- Host telemetry: logs/traces/metrics, correlation, privacy, health exclusion and exporter failures.

Inspect the current projects/README before running; commands must evolve with
the test setup. Run additional affected suites or configured coverage checks.
Do not claim coverage without its actual report. Fix applicable failures before
publishing. If a local listener/build fails due to sandbox restrictions, use the
available approval mechanism and rerun; distinguish environment failures from
application failures.

## 3. Build and test the release image

From the same application source root:

```powershell
docker --context desktop-linux build --pull -t translation-backend:local project/src/Translation
docker --context desktop-linux run --rm --read-only --cap-drop ALL --security-opt no-new-privileges --tmpfs /tmp:rw,noexec,nosuid,size=64m -p 127.0.0.1:18080:8080 translation-backend:local
```

In a second terminal, check actual HTTP status/body and inspect structured logs.
Exercise the changed API, not only `/health/live`. Verify the image runs under
the deployment's non-root/read-only restrictions. Stop the smoke container when
finished. If container configuration changes, also test those settings.

For telemetry changes, verify all affected signals and privacy behavior. Validate
collector configuration against its pinned image before deployment. Exporter
availability must not alter business API behavior.

## 4. Publish and prepare the DEV release

1. Discover the existing Azure resources and authentication state using Azure MCP
   where supported; use CLI/Kubernetes tools for capabilities MCP lacks.
2. Give the tested image a unique build tag and push it to the intended registry.
   Capture the registry digest and update
   [kustomization.yaml](../deploy/translation-dev/kustomization.yaml).
3. Deploy that exact digest. Do not rebuild between image testing and publishing
   without repeating the relevant image checks. Pin infrastructure-side images too.
4. Keep credentials/connection strings out of source, command output and evidence.
   Use the established Kubernetes Secret/identity configuration.
5. For infrastructure changes, update files under `infra/`, compile/validate them,
   and review Azure what-if before applying. Review any new cost or access changes
   within the user's authorization. Routine app releases do not recreate the cluster.

## 5. Validate and apply to the correct cluster

Use dedicated non-admin Entra credentials. Set `KUBECONFIG` to the intended file
and verify its context before making changes. Current client paths and connection
notes are in [deployment-status.md](../deploy/translation-dev/deployment-status.md).

From the Translation project directory (`C:\Repos\cap\projects\translation`), after the target namespace exists:

```powershell
kubectl config current-context
kubectl kustomize deploy/translation-dev
kubectl apply --dry-run=server -k deploy/translation-dev
kubectl diff -k deploy/translation-dev
kubectl apply -k deploy/translation-dev
kubectl -n translation-dev get pods,services -o wide
kubectl -n translation-dev rollout status deployment/translation-backend --timeout=300s
```

Check every exit code. `kubectl diff` returns 1 for expected differences and a
larger code for an error. Windows needs a `diff` executable on PATH. Review the
rendered image, resources, probes, secrets references and admission changes before
apply. Never use an unchecked default context or silently disable TLS checks.

Verify image digest, scheduling, image pull, startup, readiness, restarts and
events. Investigate failures using container-specific logs and events; distinguish
an intentional application gate from a new regression. Capture the prior digest
and configuration for rollback. `rollout undo` is an option when a prior revision
exists; update local manifests afterward so desired state matches the rollback.

## 6. Hit the deployed APIs

For this internal-only DEV service, run this in a terminal and keep it open:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\deploy\translation-dev\Start-PortForward.ps1
```

The execution-policy override applies only to that helper process. Open
[backend.http](../deploy/translation-dev/backend.http) in VS Code REST Client and
click **Send Request**. `localhost:18081` is the tunnel to the Azure pod; it is not
a locally running backend. Restart the tunnel after pod replacement.

For each affected API, assert the expected status, response body and relevant
state changes. Include at least one appropriate invalid/unauthorized/dependency
failure case. Use isolated test data and approved cleanup for mutating requests.
Keep business traffic internal unless public exposure is explicitly requested.

### Current fail-closed baseline

As of the API/telemetry release on 2026-10-30:

- `/health/live` returns 200.
- `/health/ready` returns 503, `business_dependencies_not_configured`.
- Mapped business routes return 503 until real adapters and identity are registered.
- The pod is Running with its collector ready, but the application is not Available.

This is an explicit limitation, not a healthy end-to-end business result. Do not
replace readiness with liveness/TCP to manufacture success. When dependencies are
implemented, update the expected baseline and require readiness 200, successful
rollout, valid business responses and correct authorization/error behavior.

## 7. Verify telemetry in Azure

Follow [telemetry.md](../deploy/translation-dev/telemetry.md) for portal links and
queries. Generate a non-health API request and capture its trace/operation ID.
Verify cloud ingestion, not only console output or a healthy collector:

1. Application Insights `requests`: correct route, status and duration.
2. `traces`: a correlated structured log with the same `operation_Id`.
3. `customMetrics`: relevant counters/durations and runtime metrics.
4. No sensitive payloads, tokens or unbounded raw URL values in telemetry.

Allow for ingestion/indexing delay and check exporter logs/counters if records
are missing. Export counters prove delivery attempts/success from the collector;
stored Azure query results establish ingestion. Do not claim unverified signals.
Health checks are intentionally excluded, so they cannot prove request telemetry.

## 8. Review, record evidence and hand off

Review the final diff for correctness, regression risks, unintended changes and
local links. Run `git diff --check`. Record:

- Source revision/working-tree state, test commands and actual pass/fail results.
- Image tag/digest, target environment and infrastructure changes, if any.
- Server-validation/apply results, pod state, probe and restart observations.
- Cloud API requests and expected/actual responses, without sensitive data.
- Telemetry queries and a correlation ID proving the relevant cloud signals.
- Remaining limitations, omitted checks and rollback information.

Update [deployment-status.md](../deploy/translation-dev/deployment-status.md) for
cloud releases. Keep the final user report concise, with Azure/resource links and
any material readiness limitations. Do not conflate deployed, running, ready and
business-validated.

## Proportionate application

- Documentation-only changes: verify accuracy, examples, links and diff; no image
  build or cloud release is required unless the instructions/config being tested
  explicitly need it.
- Application code changes: run affected unit/component/API checks, then image and
  authorized cloud verification before claiming end-to-end validation.
- Manifest/infra changes: validate rendered/schema/admission behavior, what-if for
  Azure resource changes, then verify the affected cloud behavior.
- Telemetry changes: run telemetry/privacy tests and verify each affected signal
  in Azure. Repeating only an unrelated health check is insufficient.

If cloud access or authorization is missing, finish the independent local work,
record the cloud gates as pending, and request what is needed. A pending gate is
not a pass.
