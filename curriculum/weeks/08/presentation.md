---
marp: true
theme: default
paginate: true
size: 16:9
footer: 'CAP · Week 8'
style: |
  section { background: #F4F0E7; color: #152536; font-family: "Avenir Next", Avenir, Arial, sans-serif; font-size: 32px; line-height: 1.3; padding: 48px 68px 65px; justify-content: flex-start; }
  h1 { font-size: 52px; line-height: 1.08; margin: 0 0 28px; color: #152536; }
  h2, strong { color: #188D91; }
  p, li { margin-bottom: 18px; }
  footer { left: 68px; bottom: 24px; font-size: 16px; color: #647276; }
  section::after { color: #647276; bottom: 22px; right: 45px; font-size: 16px; }
  section.cover { padding-top: 100px; }
---
# From a backend skeleton<br>to an observable deployment

**Tests. Packaging. Identity.<br>A verified request to the right target.**

CAP · Week 8 · Parallel work and Azure deployment

<!-- 0–3m. Follow the October31 live exercise. Deployment bootstrap is distinct from completing the translation-review product. -->

---

# Keep the SDLC in view

MVP → Design trade-offs → Reviewed plan<br>→ Implementation → Tests → Deployment → Verification

**Human judgment still owns the result.**

<!-- 3–6m. Recap Weeks5–7. The previous skeleton/build is not evidence that all domain behavior is correct. -->

---

# Two streams need clear ownership

Testing worktree. Deployment worktree.
Separate branches and review paths.

**Isolate file changes; integrate results deliberately.**

<!-- 6–10m. Git worktrees share history/object storage but have separate working directories/indexes. They are not security sandboxes and do not automatically merge changes. -->

---

# What does coverage tell us?

Which configured code ran under the tests.

What are the assertions? What was excluded?<br>Which important failure is still untested?

**80% or 100% needs context.**

<!-- 10–14m. The live request used80%; later an agent reported100%. Neither establishes complete correctness. Avoid trivial getter/setter coverage as a substitute for meaningful cases. -->

---

# Work in parallel without losing the handoff

A background agent develops tests.
A foreground session prepares deployment.

**Record what is ready, where, and for whom.**

<!-- 14–18m. Separate contexts need supported communication or shared reviewed artifacts. Do not assume every agent has access to every conversation. -->

---

# Know your Azure scope

Tenant → identity context
Subscription → resource management and billing scope
Resource group → related-resource organization

**Authentication and permission are separate.**

<!-- 18–22m. These are clarifications of informal classroom terminology. Nonprofit credit mentioned in class is not a student entitlement or universal free service. -->

---

# Name the environment you mean

Development. Integration/test. Pre-production.<br>Production and possible regional deployments.

**Names alone do not create isolation or release checks.**

<!-- 22–26m. Environment rings/regions/canary were illustrative. No global topology or full set of environments is required for this homework. -->

---

# Why instrument the service?

Startup. Requests. Failures. Duration.

What happened away from the developer's machine?

**Telemetry makes remote behavior observable.**

<!-- 26–30m. Remote debugging may be limited, not universally impossible. Observability helps establish what happened but does not replace business assertions. -->

---

# Image, container, orchestrator

Image → packaged application artifact
Container → running instance
Kubernetes → workload orchestration

**They are different things.**

<!-- 30–34m. Correct the source's initial shorthand. Docker tooling builds/runs images; Kubernetes is not itself an image. -->

---

# Pods and state have lifecycles

Nodes run pods; pods contain containers.
Instances may be replaced or scaled.

**Durable data needs an explicit design.**

<!-- 34–38m. Containers may run databases, but durability needs storage/lifecycle/recovery decisions. Do not claim storage never fails or invent fixed container capacity. -->

---

# Prepare the actual local prerequisites

The chosen Windows setup needed Docker and WSL.
The backend also needed basic HTTP hosting.

**A health host is not the full CRUD product.**

<!-- 38–42m. WSL2 is a specific DockerDesktop setup path, not a universal requirement. Installation and host readiness must be checked, not inferred from a plan. -->

---

# Choose an authorized sign-in path

Device sign-in was attempted.<br>Interactive organizational sign-in followed.

**A successful login does not grant every operation.**

<!-- 42–46m. Device authorization is a supported flow, not inherently insecure. Distinguish user/service principal, application registration and managed identity. Protect codes/secrets and use appropriate scope. -->

---

# Do not copy the admin shortcut

Which identity deploys?<br>Which identity runs the app?<br>Who may call the app?

**Grant the access needed for each role.**

<!-- 46–50m. The instructor criticized broad administrative access. Students are not asked to share that account or create blanket grants. -->

---

# Give the deployer a concrete artifact

Image identity and version. Location/registry.<br>Port. Configuration needs. Known gaps.

**Two sessions do not imply shared understanding.**

<!-- 50–54m. The live session-ID handoff needed adjustment. Use supported, authorized communication and verify the details before deployment. -->

---

# Quiz: is the backend deployed?

The image build succeeded.

**No cloud workload has been proven yet.**

What additional evidence would you request?

<!-- 54–57m. A local image, a registry artifact, a submitted deployment and a running workload are separate states. -->

---

# Alive can still mean not ready

Liveness returns 200.
Readiness returns 503 for missing dependencies.

**That is not automatically a contradiction.**

<!-- 57–60m. The skeleton intentionally failed readiness. Standard Services normally exclude unready endpoints; direct forwarding or special configuration is a different path. Do not fake readiness to hide incomplete behavior. -->

---

# Protect the correct surface

Kubernetes management API access<br>≠ Application HTTP ingress<br>≠ Application user authorization

**An IP allowlist does not settle all three.**

<!-- 60–64m. AKS API-server authorizedIP ranges concern control-plane access. Least privilege is important but not a complete zero-trust architecture by itself. -->

---

# Track asynchronous progress

Command sent → Cloud provisioning<br>→ Workload running → Ready → Expected behavior

**An agent finishing its turn is not the final state.**

<!-- 64–68m. Inspect actual deployment state and logs. The session proposed resource-group-scoped access; account distribution was not established. -->

---

# Publish, then deploy the image

Local build → Azure Container Registry<br>→ Azure Kubernetes Service workload

**ACR holds artifacts. AKS runs workloads.**

<!-- 68–72m. Correct rawECR/EKS recognition for this Azure demonstration. Version/digest/configuration matter; no fixed replica/RPS or cost guarantee follows. -->

---

# Add the observation path

Application instrumentation and ILogger<br>→ OpenTelemetry collection/export<br>→ Application Insights / Log Analytics

**OpenTelemetry is not the monitoring resource.**

<!-- 72–76m. Keep the environment distinguishable through actual configuration and access, not only resource-group names. No single universal instrumentation choice is required by the closing homework. -->

---

# Local changes need a new release

Build the changed artifact.<br>Publish and redeploy it.<br>Identify the version you exercise.

**Local implementation does not update the running release.**

<!-- 76–80m. The instructor requested API/telemetry changes and redeployment. Verify which artifact reached the intended target. -->

---

# Which server received the request?

A localhost URL may hit a local process<br>or forward to a remote workload.

**Inspect the forwarding path and destination.**

<!-- 80–84m. The class discussed a devtunnel/forwarded port. Do not infer the precise mechanism from loopback alone. A forwarded pod request need not prove normal public ingress. -->

---

# Use a request tool you can verify

HTTP file. API client. Command. Automated check.

Record target, request, expected data and actual result.

**The editor extension is incidental.**

<!-- 84–87m. A plugin/UI-assisted send was demonstrated. No specific editor extension or computer-control workflow is required for all students. -->

---

# Query application telemetry

Select the component/workspace and time window.
Find requests, traces or exceptions for this activity.

**Resource activity logs are a different data source.**

<!-- 87–91m. Table names/schemas depend on the monitoring setup. Match the operation/release rather than accept any nonempty result. -->

---

# What does a telemetry result prove?

The observed activity reached that collection path.

Was it the intended release and operation?<br>Did the business data come back correctly?

**Collection success is one claim among several.**

<!-- 91–95m. The presenter reported dataflow/duration/exception-related results. The documentation task did not access liveAzure or independently reproduce that outcome. -->

---

# Save the validation loop

Change → Unit/component checks → Release
→ Remote API → Telemetry → Record the outcome

**Link the workflow from relevant instructions.**

<!-- 95–100m. The source asked for a workflowMD pointer inAGENTS.md. Written guidance is not automatic enforcement; actual tests/scripts/permissions must implement it. -->

---

# State the remaining gap

Only the backend was deployed in the demonstration.
The business logic still needed examination.

**Prove a concrete feature before claiming the product works.**

<!-- 100–105m. Separate source-reported deployment from current main-checkout baseline and pending worktree artifacts. Do not merge or claim unreviewed practice changes in this curriculum task. -->

---

# Homework: CRUD and a basic frontend

Implement and verify the small backend CRUD journey.
Try a basic page connected to Azure development.
Record access blockers and local rehearsal honestly.

**No new AKS cluster or paid subscription is required.**

<!-- 105–110m. Instructor offered account access; endpoint/grant distribution was not established. Use reviewed API design, tests and expected values; a local rehearsal is not a completed Azure connection. -->
