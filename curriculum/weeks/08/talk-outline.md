# Week 8: Parallel Testing, Azure Deployment, and Observability

## Learning goals

Coordinate isolated testing/deployment work, interpret test evidence, distinguish images from running workloads, explain identity/resource/network boundaries, verify the actual endpoint, and preserve a repeatable change–test–deploy–observe workflow. Connect this infrastructure work to the next concrete CRUD/frontend feature rather than declare the product complete.

## Teaching through-line

Week 7 produced a bounded backend practice and emphasized domain verification. Week 8 explores the operational path to running that code and observing it. The source repeatedly distinguishes class-time shortcuts from careful engineering. Use the [session notes](session-notes.md), [raw transcript](transcript.txt), [reference](reference.md) and [homework](homework.md).

The lesson is based on a live unrehearsed demonstration. A proposed command, an agent report, a cloud resource and a verified application result must be labeled separately.

## Delivery — 180 minutes

The [presentation](presentation.md) supports a 110-minute core discussion. Add the 35-minute guided exercise and 35-minute student workshop below. The times are curriculum preparation; they do not establish a repeatable provisioning duration.

## Core discussion — 110 minutes

| Time | Instructor move | Evidence to capture |
| --- | --- | --- |
| 0–15 | Recap the SDLC and separate test/deployment worktrees. Challenge an unqualified coverage result. | Branch/work ownership, meaningful cases and test limits. |
| 15–25 | Explain tenant/subscription/resource group, environments and telemetry purpose. | Target scope, release environment and what needs observation. |
| 25–40 | Distinguish image/container/VM/orchestrator; review missing HTTP/runtime prerequisites and sign-in choices. | Build prerequisites, intended artifact and identity assumptions. |
| 40–55 | Discuss Docker/WSL setup, authorized interactive login and cross-session image/port handoff. | What was actually prepared and what another agent needs. |
| 55–70 | Use the image/liveness/readiness quiz; inspect deployment progress and access restrictions. | Expected health responses and management versus app access. |
| 70–85 | Trace registry publication, deployment, instrumentation and redeployment. | Versioned artifact path and monitoring destination. |
| 85–100 | Verify the request target, explain loopback forwarding, distinguish application telemetry from activity logs, and inspect a scoped query. | Actual environment/request/data source and observed results. |
| 100–110 | Extract the validation workflow and closing homework. State the remaining business-validation gap. | Repeatable steps, honest limitations and CRUD/frontend scope. |

## Guided verification exercise — 35 minutes

1. **Inventory the target (5m).** Use a permitted existing development target or a clearly labeled local rehearsal. Identify application version, endpoint, forwarding path if any, and access scope. Do not create new paid resources solely for this exercise.
2. **Inspect tests and health intent (10m).** Review a meaningful test and its expected outcome. Explain a healthy process with a deliberately unready dependency. If using Kubernetes, examine the relevant probe/routing configuration rather than changing readiness to mask missing behavior.
3. **Exercise one request (10m).** Use an API client, command, HTTP file or automated check. Verify the expected response/data and the actual target. A loopback URL is not sufficient evidence of either a local or remote deployment.
4. **Correlate and record (10m).** Where authorized telemetry exists, identify its workspace/component, time window and operation. Record what the response and telemetry prove separately. Write a brief repeatable validation workflow and link it from relevant instructions.

The repository's main backend is the prior checkpoint, not proof that all live-demo artifacts are integrated. If no runnable endpoint or telemetry is available, inspect an instructor-provided example, record the gap and do not manufacture a completed cloud result.

## Student workshop — 35 minutes

- **10m:** Choose one small CRUD flow from the existing API design and define expected values, a failure case and access assumptions.
- **15m:** Implement/check a bounded step or connect a basic frontend request to an already authorized backend. Keep missing cloud access and data/provider adapters explicit.
- **10m:** Peer review the evidence, endpoint identity and next step. Record what is local, forwarded, deployed, mocked or still unimplemented.

## Technical clarifications to teach explicitly

- Linked Git worktrees isolate working directories/indexes and share history; they do not replace review/integration or create security sandboxes.
- Coverage is a configured execution metric, not proof that requirements or assertion quality are sufficient. The recorded 80% target and later 100% report have context and limits.
- Kubernetes is an orchestrator, not an image. Docker tooling, image artifacts, running containers, pods/nodes and durable storage are distinct.
- Docker Desktop's WSL2 path is a specific Windows setup. Do not generalize it to every Docker platform.
- Use Azure's names AKS and ACR; EKS/ECR in the transcript are recognition/informal-name artifacts here.
- Device sign-in, user identities, app/service principals and managed identities are distinct. Do not reproduce administrator credentials or assume authentication grants every operation.
- Restricting the Kubernetes control-plane API does not establish the application's HTTP-ingress or user-authorization policy.
- OpenTelemetry instruments/collects/exports; Application Insights/Log Analytics receive and expose the relevant data. Resource activity logs are not application traces.
- A successful port-forwarded request need not prove the normal ingress path. Verify the tunnel/forwarding destination and version.
- A Markdown workflow linked from `AGENTS.md` is guidance. Actual automation, permissions and checks must enforce the intended behavior.

Use the primary sources already linked in the reference to clarify shorthand. Do not quote the instructor's historical credit/pricing/capacity anecdotes as current student entitlements or cost estimates.

## Homework and next step

The recorded assignment is backend CRUD and trying a basic frontend connected to the Azure development backend, with familiarization and appropriate account access. The [homework](homework.md) defines submission, evidence and assessment. It does not require each student to provision an independent Kubernetes environment.

The closing statement says the backend business logic still needs verification. Continue by proving a concrete feature/data flow. This update does not enact the earlier hypothetical Weeks 9–12 plan or claim the deployed skeleton is a complete product.
