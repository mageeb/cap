# Week 8 Reference: Parallel Verification and Deployment Preparation

Use this reference with the [session notes](session-notes.md), [full automated transcript](transcript.txt), and [homework](homework.md). Prompts are teaching aids derived from the recording, not evidence that every proposed check, environment, or deployment succeeded.

## 1. Recover the actual implementation state

The opening recap distinguishes agreed product/design from the partial backend generated in Week 7. Compilation is useful evidence, but does not establish behavior, test quality, or a live service.

```text
Read the product scope, architecture, implementation task record and current code.
Summarize what is implemented, proposed, fake-backed, blocked, and unverified.
Identify the minimum operation we can test and the actual artifact we could deploy.
Do not infer a complete HTTP host or real persistence from a successful build.
Separate recording claims from fresh checks and current repository state.
```

## 2. Give parallel work separate ownership

The class separates testing and deployment changes into different worktrees and branches. Git linked worktrees share repository history and most Git metadata, while keeping working directories and per-worktree state such as the index and HEAD separate. They allow simultaneous work on different branches without repeatedly switching one checkout. See the [official Git worktree documentation](https://git-scm.com/docs/git-worktree).

This is version-control separation, not a security sandbox. Workers can still conflict in shared services, ports, configuration or other paths. Agree on what each worker may change and who integrates the results.

```text
Plan two bounded tracks: behavior tests and deployment preparation.
Identify their common base revision, branch/worktree paths and owned files.
State each track's objective, checks, dependencies and stop conditions.
Keep deployment preparation separate from publication of unverified behavior.
Report conflicts or shared-resource assumptions before changing them.
Do not merge or claim an integrated release merely because both workers finished.
```

Before removing or moving a worktree, inspect its status and preserve wanted work. Follow the repository's chosen worktree lifecycle rather than treating a checkout as disposable scratch.

## 3. Target useful tests before chasing percentages

The demonstration requested roughly 80% unit-test coverage and immediately debated the cost and value of additional coverage. Treat this as the demonstrated target and discussion, not a proof of correctness or a reason to test trivial accessors at the expense of important behavior.

```text
Inspect the backend contract and existing tests before adding new ones.
List meaningful behavior and failure cases that are missing.
Prioritize validation, access rules, saved-review behavior, dependency failure,
ordering/alignment and cancellation where they are part of the agreed contract.
Explain what isolated tests can establish and what requires real integration.
Report the coverage measure, tool/configuration, actual result and uncovered risks.
Do not weaken assertions, invent results, or equate a percentage with correctness.
```

Choose a small representative input set with reviewed expected outcomes. If tests use a fake store or provider, say so. The deployment track does not turn those fakes into verified real adapters.

## 4. Distinguish cloud organization, identity, and resources

The recording introduces an organization/tenant, an Azure subscription and resource groups. Clarify the shorthand: Microsoft Entra ID supplies an identity directory; Azure subscriptions and resource groups provide resource-management scopes. A tenant is not simply a billing account, and an application user is not automatically a cloud deployer or runtime identity. See [Microsoft Entra overview](https://learn.microsoft.com/en-us/entra/fundamentals/what-is-entra) and [Azure Resource Manager terminology](https://learn.microsoft.com/en-us/azure/azure-resource-manager/management/overview).

A resource group collects related Azure resources for management. Meaningful names and tags help identify workload and environment. The recording's dev, integration/test, pre-production and canary examples describe a possible release progression, not a mandate to provision every environment for a small exercise. See [Azure resource organization guidance](https://learn.microsoft.com/en-us/azure/cloud-adoption-framework/ready/azure-setup-guide/organize-resources).

```text
Before proposing cloud changes, restate the intended environment and deployment goal.
Identify the tenant/directory, subscription, resource group and intended region.
Separate the deploying human/tool, application runtime identity and app users.
List existing versus proposed resources and unresolved permissions/configuration.
Keep examples and current provider terms distinct; do not invent resource IDs.
Explain the smallest suitable development setup and defer unnecessary environments.
```

This reference does not authorize changes to anyone's cloud account. Use only an approved account and scope, and avoid presenting a resource list or an agent's plan as a completed deployment.

## 5. Preserve a truthful result record

| Claim | Useful evidence | What it does not establish |
| --- | --- | --- |
| Build succeeds | Actual command, version and result | Correct behavior or integration |
| Unit checks pass | Cases, assertions and observed output | Real network/database behavior when faked |
| Coverage target reached | Tool, configuration and measured result | Full quality or all failure cases |
| Deployment prepared | Reviewed configuration and explicit prerequisites | Resources created or app reachable |
| Deployment succeeded | Actual resource/artifact/version and bounded smoke checks | Production readiness or untested scale |

Record what happened, what failed, what was corrected, and what remains open. A separate test agent and deployment agent still need one integrated decision about readiness.

## 6. Connect code-writing and deployment results explicitly

The closing recap credits connecting separate code-writing and deployment sessions so the deployer could consume the implementation artifact. Treat the handoff as a contract rather than assuming the deployer sees whichever files another agent currently has open.

```text
Identify the exact source revision and build/deployment artifact.
State how the deployment worker receives it and what checks must precede use.
Record which tests apply to this revision and what is still unverified.
After a code change, explain whether rebuilding/redeploying is needed.
Do not claim a release from one branch contains unmerged work from another.
```

The recording's recap describes a skeleton backend deployment, then explicitly says business logic still needs validation. Preserve that distinction even if the infrastructure is reachable.

## 7. Make telemetry a bounded verification step

The closing discussion describes a redeployment after adding telemetry and observing request-duration and exception-related results. A useful check connects an intentional request to collected evidence; passive dashboards alone can be ambiguous.

```text
Define one safe synthetic request with an expected backend outcome.
Record its endpoint/action, time window and relevant correlation information.
Check the response and the associated request/trace/exception evidence.
Explain whether missing data is a collection delay, configuration problem,
wrong scope/time window, or evidence that the request did not take that path.
Sanitize shared output and state exactly what this observation establishes.
```

Seeing activity arrive can verify collection for that activity. It does not establish all CRUD behavior, correct saved data, absence of faults, or production scale. A repeatable validation workflow should state prerequisites, commands/query scope, expected observations and failure handling; review it before treating it as authoritative guidance.

## 8. Close the loop on the feature, not only the deployment

The explicit closing homework returns to create/read/update/delete behavior and a small frontend connection. The intended feature must still be verified after the skeleton is deployed.

```text
Choose one book/document or paragraph journey from the reviewed API design.
Create a safe record; read its expected fields; update it; read the change;
delete the intended record and verify the resulting state.
Show a meaningful invalid/missing-resource case and preserve unrelated data.
Connect a basic frontend to the authorized development endpoint, or record
an explicit cloud-access gap while rehearsing the same flow locally.
Separate observed responses, persisted state, collected telemetry and assumptions.
```

A basic validation page is sufficient; reproducing the full wireframe design is unnecessary. The recording offers Azure account access but does not establish that every student already received it. Do not provision a new cluster merely to make a screenshot or to imitate the classroom topology.

## 9. Clarify image, container, orchestration, and storage

The live discussion sometimes uses Kubernetes, Docker image and container interchangeably. Keep these concepts separate in the design:

| Concept | Meaning for this lesson |
|---|---|
| Image | A packaged template containing the application and required runtime/files |
| Container | A running instance created from that image and runtime configuration |
| Kubernetes | A platform that manages containerized workloads and desired deployment state |
| Cloud compute | The machines/resources on which the workloads run |
| Persistent storage | State whose lifecycle must survive replacement of an application container |

An image build is not an application deployment. Kubernetes does not become the image or automatically create a complete database. Database software can run as a containerized workload, but durable data and its operational behavior still need deliberate storage configuration. See [Docker images and containers](https://docs.docker.com/get-started/docker-overview/) and [Kubernetes overview](https://kubernetes.io/docs/concepts/overview/).

```text
Explain the proposed package/deployment in these separate layers:
application and HTTP host, image build, runtime container, registry,
Kubernetes workload/service, compute, configuration and persistent storage.
State which components exist, which must be created, and what checks each layer needs.
Do not call an image a cluster or treat a healthy runtime as verified business logic.
```

## 10. Separate identity objects and authentication methods

The source's identity discussion conflates several terms. Use the following clarification when preparing an actual deployment:

- An application registration creates an application identity configuration; a service principal is its representation in a particular tenant. A human user principal and an application service principal are different actors. See [application and service principal objects](https://learn.microsoft.com/en-us/entra/identity-platform/app-objects-and-service-principals).
- A managed identity is a special workload identity for supported resources. It lets the workload obtain Entra tokens without the developer managing an application password or certificate. It still needs appropriate permissions; it is not an unhackable identity or another name for manually creating an app plus client secret. See [managed identities overview](https://learn.microsoft.com/en-us/entra/identity/managed-identities-azure-resources/overview).
- Device authorization is a supported OAuth flow in which a user signs in through a browser to authorize the requesting client. Protect the login/code context and confirm what you are authorizing. Do not describe the flow as inherently invalid or rank every authentication method solely by whether a code/certificate exists. See [Microsoft device authorization flow](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-device-code).

```text
Identify the human deployer/tool identity, application runtime identity,
and application users separately. Explain how each authenticates and what
resource scope/operation it is authorized to access.
Use the approved sign-in mechanism for the environment; do not expose secrets
in prompts, logs, browser source or committed files.
State any missing permission explicitly rather than assuming subscription admin.
Do not claim that successful sign-in grants every deployment operation.
```

These clarifications explain terminology. They do not select a final production identity architecture, authorize new grants, or instruct students to reproduce the instructor's administrative access.

## 11. Read probes as different operational questions

In the quiz, a liveness response of 200 and readiness response of 503 are not necessarily contradictory. Liveness asks whether a container needs restarting; readiness asks whether it should receive normal service traffic. With configured readiness checks, a failed result normally excludes the Pod from matching Service traffic. See [Kubernetes probe concepts](https://kubernetes.io/docs/concepts/workloads/pods/probes/).

The demonstration deliberately leaves readiness unsuccessful while required service behavior is unimplemented. Do not change the probe merely to obtain a green result. Explain what real dependency or initialization condition makes this version ready.

```text
Describe liveness, readiness and any startup check for the actual application.
State their endpoints, expected responses and what each response establishes.
Show what is intentionally unimplemented and why readiness may fail.
After implementation, verify both operational health and one real business action.
Do not treat a health response as proof that data is saved or business rules work.
```

## 12. Distinguish local prerequisites and observability layers

The Windows demonstration installs Docker and a WSL backend. That is the chosen local setup, not a rule that every Docker host requires Windows Subsystem for Linux. See [Docker Desktop's WSL 2 backend](https://docs.docker.com/desktop/features/wsl/).

OpenTelemetry is a framework/toolkit for generating, collecting and exporting signals; it is not itself the telemetry storage/dashboard backend. Application Insights is an Azure Monitor application-observability destination that supports OpenTelemetry for relevant scenarios. Keep instrumentation, export configuration, destination, and queries distinct. See [OpenTelemetry overview](https://opentelemetry.io/docs/what-is-opentelemetry/) and [Application Insights overview](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview).

```text
Trace one request from application instrumentation to the configured telemetry destination.
Identify the exact application version/environment, exporter/configuration and destination.
Generate a small safe request, then query the relevant time window and activity.
Explain whether missing results reflect configuration, ingestion delay or a wrong scope.
Keep development telemetry distinct from production when interpreting the result.
```

The transcript's AKS/ACR names refer to Azure Kubernetes Service and Azure Container Registry; similar-sounding EKS/ECR text is a recognition artifact in this Azure discussion. A registry holds images for deployment; uploading an image does not establish that a running workload uses it.

## 13. Keep the demo's scale and cost statements bounded

Examples about many regions, autoscaling, WAF/DDoS defenses, large agent fleets, and software factories motivate later investigation. They are not instructions to provision all those components for this homework.

The recording reports an instructor's nonprofit subscription credit and makes informal cost comparisons. Do not assume those credits, eligibility, or low-cost estimates apply to your account or configuration. Before creating resources, use the authorized scope and understand the selected resources' current costs, limits and cleanup. Existing test or deployment access is not permission to expand into unrelated infrastructure.

The test worker's reported 100% coverage is also a report to review. Inspect the measured scope and useful assertions; neither 80% nor 100% alone establishes that the application is correct, secure, ready for traffic, or inexpensive to operate.

## 14. Separate the cluster control plane from application access

The live proposal restricts the public Kubernetes API to an authorized administrator IP. That protects a control-plane endpoint used to manage the cluster; it does not by itself restrict the application's HTTP endpoint to that machine. Application ingress/network policy and user authorization need their own design. See [AKS API server authorized IP ranges](https://learn.microsoft.com/en-us/azure/aks/api-server-authorized-ip-ranges).

```text
List the cluster management endpoint and application HTTP endpoint separately.
For each, identify permitted callers, network exposure, identity/permission checks,
and the evidence that the intended restriction is actually enforced.
Do not infer private application access from a control-plane IP allowlist.
Do not broaden an allowlist or grant administrative rights just to bypass a failure.
```

The instructor explicitly calls the administrative shortcut unsuitable as a general practice and discusses more limited student access. Least privilege is one useful principle; it is not a complete definition of Zero Trust or a substitute for understanding each request path.

## 15. Verify where a localhost request goes

The recording shows requests to `127.0.0.1:18081` and an agent explanation that a tunnel reaches the deployed service. A loopback URL identifies the local connection endpoint; forwarding can carry that connection to a remote workload. The URL alone proves neither a local application nor a cloud deployment. For a documented example of local-to-cluster forwarding, see [Kubernetes port forwarding](https://kubernetes.io/docs/tasks/access-application-cluster/port-forward-access-application-cluster/). This example does not establish which exact forwarding command the demonstration used.

Record the forwarding process, destination environment/workload, port mapping and deployed revision before interpreting a request as remote validation. A successful response must be tied to the intended deployment. The `.http` file, editor extension and alternatives discussed in class are ways to run requests, not mandatory tools or independent evidence of the destination.

## 16. Query application activity in the appropriate telemetry scope

The session initially inspects resource-change events while looking for application traces. Azure Activity Log primarily records control-plane operations on resources; it is different from application request, trace and exception telemetry. See [Azure Activity Log](https://learn.microsoft.com/en-us/azure/azure-monitor/fundamentals/activity-log) and [Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview).

```text
Name the application, environment, deployed revision and telemetry destination.
Generate a small identifiable request and record its time and correlation identifier.
Query the destination's actual tables and fields within a short relevant time window.
Distinguish no matching activity from a query/schema/scope or ingestion problem.
Capture the result without exposing credentials or sensitive request data.
```

A KQL query copied from another workspace can target the wrong table or field. Adapt it to the configured destination and schema. A portal link or successful query execution does not by itself prove that the intended application emitted the expected event.

## 17. Make the validation workflow repeatable

The instructor asks the agent to preserve the validation process in `validation-workflow.md` and point to it from `AGENTS.md`. These are repository conventions intended to make later work consistent. A document link alone does not enforce execution or replace reviewing the actual run's results.

A useful workflow records local checks, the authorized deployment target, the exact artifact/revision, remote API behavior, relevant telemetry checks, and the final human review. Mark each step as completed, failed or blocked with evidence. Keep commands and configuration aligned with the current project rather than blindly reusing a successful earlier run.

The session ends with deployment progress and an explicit gap: business logic has not yet been reviewed deeply. Use the homework to verify a real CRUD journey and connect a basic frontend. Infrastructure and agent reports are supporting evidence; demonstrated user behavior is the next checkpoint.
