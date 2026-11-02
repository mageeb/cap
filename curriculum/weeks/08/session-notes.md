# Session 08 — Parallel Testing and Agent-Assisted Deployment

*Organized account of the October 31 recording. Incidental conversation and repeated speech are removed here. The [full automated transcript](transcript.txt) preserves the accepted English recognition output without editorial cleanup. These notes distinguish demonstration requests, observed discussion, unresolved choices and independently verified repository facts.*

## 1. Continue the agentic software development lifecycle

The session opened by connecting the previous weeks: define a small MVP, compare architecture trade-offs, derive a reviewed work hierarchy, generate a bounded implementation and verify it. Human judgment remains responsible for context, decisions and acceptance.

The instructor revisited the difference between code that compiles and code whose behavior has been validated. A build result is useful, but does not settle whether the generated backend meets the product requirements. This session set out to develop tests while exploring deployment in parallel.

The classroom application remains a translation-review system. Earlier backend work is a starting checkpoint, not evidence that the entire service and frontend are complete. Several steps in the live demonstrations were explicitly shortened for class time.

## 2. Isolate two work streams with Git worktrees

The instructor asked for one worktree for testing and another for deployment, with separate branches and pull requests. That enables independent changes without repeatedly switching a single working directory or letting two agents overwrite the same files.

A linked worktree has its own working directory and index while sharing the repository's object/history storage. It is not a security sandbox, and separate branches do not automatically reconcile incompatible changes. Review and integration are still required.

The discussion contrasted linked worktrees with making a second clone. The important learning outcome is to know where each agent is working, which branch owns its changes, and how its result will be reviewed. It is not a claim that worktrees eliminate every disk cost or allow unrestricted concurrent edits to one checkout.

The instructor requested a background test agent so deployment exploration could continue in the foreground. That is a bounded example of agent coordination, not a complete autonomous software factory or a required number of agents for every student.

## 3. Meaningful coverage rather than trivial tests

The group distinguished unit tests from integration or end-to-end verification, while acknowledging the definitions and costs deserve more discussion. The live test request used roughly 80% coverage as a target and prioritized behavior that matters.

Participants challenged both the pursuit of 100% as an automatic goal and tests that merely exercise trivial getters or setters. The underlying concern was balancing useful checks, maintenance and execution cost. This does not mean every untested line is safe or that 80% proves correctness.

Coverage measures which code executed under the configured tests. Acceptance still depends on assertions against requirements and representative domain cases. Fake-backed service tests, real dependency tests and full user-journey tests establish different kinds of evidence.

The session's earlier domain-dataset lesson remains relevant: tests should show that source/target paragraphs, edits and saved state behave as intended, rather than only demonstrate that generated code runs.

## 4. Azure identity, subscriptions and resource organization

The instructor described an existing nonprofit Azure organization/subscription and began exploring the portal. The material does not require publishing account identifiers, grants or credentials. Discussion of available credit is not a promise that any student account or resource is free.

The session introduced tenant, subscription and resource-group terminology. For teaching clarity, a tenant is the identity/directory context, a subscription is a resource-management and billing scope, and a resource group collects related resources for management. They are distinct concepts rather than interchangeable names for one account.

Meaningful resource names were emphasized because a subscription can be shared by a team. A resource group was used to organize translation-related work. Creating or selecting a group is not equivalent to deploying the application, establishing its database, or validating its access controls.

## 5. Separate development and release environments

The instructor sketched environment/ring names such as development, integration/test, pre-production and production. Canary rollout was also mentioned. These illustrate staged release and verification rather than an instruction to create every environment for the MVP.

An environment boundary and a progressive traffic rollout are related but different ideas. A name like PPE or canary does not itself establish isolation, representative testing or safe release behavior. A team needs to define what is being checked, who uses each environment and how a promotion is accepted.

The instructor deferred a deeper general-infrastructure discussion. This session used deployment work to expose practical questions; it did not establish a complete cloud architecture merely by showing the portal or an infrastructure diagram.

## 6. Plan release geography without making it the MVP

The discussion extended the environment naming example to regional production deployments. Actual regions, replication, rollout granularity and capacity should follow users and requirements; the examples were not an approved global topology for this project.

Named locations and sovereign-cloud remarks in the conversation are not treated as verified current service-availability claims. The important concept is that environment, region and subscription are separate design choices, not interchangeable names.

## 7. Instrument the deployed service

The instructor introduced instrumentation and telemetry as ways to observe software running away from the developer's machine. Startup messages, errors and request behavior can be collected in a managed monitoring system, with Application Insights mentioned as an example.

Remote behavior can differ from a local run because packaging, configuration, networking and dependencies change. Telemetry is therefore part of operational verification. Direct debugging may be restricted or impractical, especially in production; the lesson is not that remote debugging is technically impossible in all environments.

Adding logging or seeing a request record does not prove the product's domain behavior. A useful validation plan must identify the expected activity and connect observed data to the release and operation being checked.

## 8. Containers, images, virtual machines and Kubernetes

The session initially used container and Kubernetes terminology loosely, then explored the distinctions through questions. For technical clarity:

| Concept | Meaning for this lesson |
| --- | --- |
| Virtual machine | A virtualized machine environment, typically with a guest operating system. |
| Container image | A packaged filesystem/application artifact with dependencies and configuration defaults. |
| Container | An instance running from an image through a container runtime. |
| Docker tooling | Tools for building and running container images; an image is not itself an orchestrator. |
| Kubernetes | An orchestration system for deploying and managing containerized workloads, not the image itself. |
| Azure Kubernetes Service | An Azure-managed Kubernetes service discussed in the cloud deployment context. |

These distinctions clarify the discussion rather than endorse every informal phrase in the transcript. Kubernetes does not require every application to become a microservice system, and using it is not a prerequisite for all small projects.

Dependency packaging and workload isolation were motivating examples. Resource contention and noisy neighbors can still matter. The recording does not establish fixed numbers of containers per machine or that container boundaries remove every operational or security risk.

## 9. Prepare the missing application and developer environment

The deployment discussion revealed that the backend checkpoint did not yet expose HTTP APIs. The instructor authorized adding the absolute basics needed for deployment. A minimal host or health endpoint is a useful deployment target, but must not be confused with completed CRUD or translation-review behavior.

The demonstration also exposed local setup gaps on the instructor's personal Windows machine. Docker and Windows Subsystem for Linux were discussed as missing prerequisites. The exact preparation, installation and runtime outcome must be checked from the relevant steps rather than inferred from an agent's initial proposal.

This was a deliberately unrehearsed exercise. Troubleshooting and back-and-forth were part of the demonstration, not a promise that every student can repeat the cloud setup in a fixed time.

## 10. Authentication is different from authorization

The instructor explored connecting the agent's tools to an Azure account, including device-code sign-in, an interactive CLI route and a possible MCP connection. The goal was to let the assistant perform authorized deployment operations rather than require manual portal work for every step.

The raw transcript conflates several identity concepts. The following clarification is separate from the live explanation:

- A user identity and an application/service principal are different security principals.
- An app registration, a service principal and a managed identity are related concepts, not interchangeable labels.
- A managed identity is an Azure-managed identity mechanism that avoids managing application credentials in the usual way; creating an app registration and copying a client secret does not make that a managed identity.
- Device authorization is a supported authentication flow. Codes, sign-in destinations and the requesting application's identity still require care; calling the entire flow inherently insecure is too broad.
- Authenticating the deployment tool does not by itself grant every resource permission, establish the application's user-access policy, or select an appropriate runtime identity.

Static secrets can leak, and managed identities still require sound permission and resource configuration. No method is described here as impossible to compromise. See the verified primary documentation and prompts in the [reference](reference.md).

The demonstration's account privileges were its own context. Students are not required to share credentials, paste secrets into a repository, or acquire subscription-wide administrator access merely to perform the learning exercise.

## 11. CLI and connector access are alternative tool paths

The instructor wanted inspectable commands and explored both CLI-style interaction and a connector/MCP route. Tool availability, authentication and environment setup created practical friction.

The lesson is to understand which identity and permission scope the chosen tool uses, what action it will perform, and how to verify the result. A connector is not universally less reliable than a CLI, and a generated shell command is not correct merely because its text is visible.

The materials do not include live authentication codes, account secrets, or a claimed cloud connection for this documentation task. Transcribing a demonstration does not authorize provisioning or changing the user's Azure resources.

## 12. Runtime instances, pods and persistent state

The group continued from images to containers, pods and nodes. A Kubernetes pod can contain one or more closely related containers, and a worker node can run multiple pods. Resource settings and orchestration determine placement and scaling; the analogy of an image as a class and a running container as an instance was used to explain the distinction.

Workloads and instances may be replaced, so valuable state needs an explicit persistence design. This does not mean storage never fails or that a database cannot run in a container. It means durable state, recovery and lifecycle cannot be inferred from the existence of a running pod.

Event-driven autoscaling, including KEDA, and resource-based scaling were discussed conceptually. The example thresholds and multiplicative capacity claims were not measured sizing recommendations. Scaling also does not replace rate limiting, abuse protection or broader security design. WAF and denial-of-service concerns were deferred to deeper infrastructure discussion.

## 13. Unblock the local build and Azure sign-in

The instructor described installing Docker and WSL for the chosen Windows/Linux-container setup, then returning to the image build. This is a specific local development path, not a claim that every Docker installation requires WSL or that an image is itself an operating system.

The device-code attempt did not complete in the demonstration. The instructor then used an interactive organizational sign-in with additional authentication and reported a verified connection. The notes do not independently establish the precise tenant-policy cause of the first failure.

The session acknowledged that using a highly privileged administrative account was inappropriate as a general practice. Student access was discussed as narrower access, not sharing the instructor's administrator identity. No credentials should be distributed through source files or copied from a recording.

The important workflow was to identify which prerequisite was missing, choose an authorized recovery path and then verify the result. Repeatedly retrying the same authentication or build command without understanding its state is not progress.

## 14. Handoff between the image builder and deployer

The deployment session needed details from the session building the image: the artifact identity, relevant files, port and deployment requirements. An attempted session-ID handoff was initially unavailable; the instructor then asked the assistant to locate the other active session and recover the details.

This illustrated why parallel agents need an explicit communication/handoff mechanism. They do not automatically share complete context merely because both are running. A file, artifact record, reviewed message or supported coordination interface must carry the information.

A useful handoff names what is ready, where it is, what version it represents, what interface it exposes and what remains blocked. Cross-session access must respect the actual tool's authorization and ownership rules. General conversation about agent memory is not proof that a particular assistant can read every other conversation.

The instructor repeatedly noted that a real project deserves slower review of generated build/deployment definitions. Generated automation remains something to inspect, not a reason to abandon the underlying design.

## 15. The image/readiness checkpoint quiz

The class paused to test understanding:

- Building an image does not deploy the backend; at that point it may only be a local artifact.
- A process can be alive while not ready to serve normal requests.
- The proposed skeleton intentionally returned a readiness failure because dependencies or application behavior were not yet implemented.
- A standard Kubernetes Service normally sends ordinary traffic to ready endpoints, so an intentionally unready workload will not receive that normal service traffic simply because its process is running.

The example contrasted a liveness response of 200 with a readiness response of 503. Those responses were not treated as a contradiction. The readiness policy needs to reflect the actual service requirements, rather than being changed to report success merely to make a deployment look healthy.

Probe behavior depends on the workload and routing configuration; these examples are not a complete Kubernetes readiness policy for every application. The [reference](reference.md) points to the relevant primary documentation.

## 16. Deployment permissions and network surfaces

The instructor discussed a development resource group and a managed Kubernetes cluster, and favored narrow initial access. Later team access could be expanded intentionally. This supported a least-privilege discussion; broad administrator permissions were explicitly criticized.

One proposed restriction concerned access to the public Kubernetes API from an authorized IP address. For clarity, that is a control-plane access question. It does not, by itself, establish the application's HTTP ingress policy or authenticate application users. Management-plane access, application-network access and application authorization must be considered separately.

The conversation used zero-trust terminology alongside minimal access. Least privilege is an important principle, but a narrow IP rule alone is not a complete zero-trust architecture.

The instructor offered resource-group-scoped access to serious participants. That is not evidence that all accounts, endpoints or grants were already distributed, nor a requirement for every student to create a separate cluster.

## 17. Observe asynchronous deployment and publish the image

The instructor checked the Azure portal and described using an authenticated agent to inspect deployment failures and logs. Submitting a command, completing the assistant's turn and completing the cloud deployment are separate states.

The session described publishing the image to Azure Container Registry and then deploying the registered image to Azure Kubernetes Service. The transcript sometimes recognizes these names as ECR/EKS; the Azure services here are **ACR** and **AKS**.

The registry holds image artifacts that deployment can retrieve. Running multiple instances from an image is separate from producing that image or registering it. Version and configuration choices matter when identifying what is actually running.

Fixed instance-capacity figures were illustrative, not verified performance guarantees. Kubernetes was the instructor's choice for this demonstration; other hosting approaches were acknowledged rather than excluded.

## 18. Interpret the test agent's result

The instructor returned to the testing stream and read an agent report of 100% coverage. The report was welcomed but not independently audited in the recording. A percentage needs its scope, exclusions, assertions and test configuration before it can be interpreted as useful evidence.

The group discussed review/validation agents and deliberate adverse cases. A task should describe how to validate its result, but neither a job title nor the existence of another agent guarantees an independent or sufficient check. Proposed fault-injection ideas were not demonstrated as a completed test campaign.

The wider software-factory discussion concerned coordination and repeatable verification. The session did not establish that every stage had been automated, that all review was unnecessary, or that any generated system was production-ready.

## 19. Add instrumentation and redeploy the changed artifact

After the deployment work was underway, the instructor asked for backend/API work and telemetry, with a plan first. OpenTelemetry was recommended and .NET logging through `ILogger` was discussed. The intended next step was to implement locally, rebuild/redeploy, exercise the service and inspect the resulting telemetry.

OpenTelemetry is an instrumentation/collection/export ecosystem, not itself an Azure monitoring resource. Azure Application Insights was the monitoring destination discussed. A logger call, its collection/export path and the monitoring backend are distinct parts of the flow.

Development and production telemetry were to remain distinguishable. Environment labels and resource grouping help organization, but actual separation also depends on resource/configuration/access choices. The recording does not establish that a resource-group name alone enforces full data isolation.

The key verification questions were what version was deployed, which operation was exercised, and whether the expected activity appeared in the monitoring system. Those questions prepare operational evidence; they do not replace assertions about correct business data.

## 20. Verify the actual target of an API request

The instructor explored an HTTP request file/API-client workflow in the editor, including the possibility that an extension was needed. The point was to exercise the deployed service, not merely accept an assistant's status message.

A loopback URL created confusion: was the test hitting a local host or the cloud deployment? The assistant described a development tunnel/forwarded port. A local address can represent a forwarded connection to a remote workload, but that must be established from the forwarding setup and target, not assumed from the URL or the assistant's confidence.

A useful verification record identifies the local listener, remote destination, selected environment and deployed version. Testing a localhost process and testing a remote service through a local tunnel are different claims. Reaching a pod through a forwarding path also need not demonstrate the normal public or Service ingress path.

The editor extension and computer-use interaction were incidental tool paths, not mandatory course dependencies. An API client, a suitable command or an executable test can provide equivalent request evidence when its target and assertions are clear.

## 21. Redeploy the version with instrumentation

The instructor returned to the deployment session after local API/telemetry changes and requested redeployment. This connected the image-builder and deployer handoff to a real operational concern: local changes are not present in an already running cloud release until the changed artifact is built, published and deployed.

The presenter then described Application Insights/Log Analytics resources being available and looked for emitted data. That was evidence about the monitoring path, not a complete check of the translation-review features.

The live exercise was deliberately fast. The instructor repeatedly noted that a responsible real project should inspect generated files, decisions and changes more carefully than the class demonstration did.

## 22. Query the right telemetry, not a different Azure log

The instructor tried to retrieve recent activity through the portal and then asked the authenticated coding/deployment assistant to find the appropriate monitoring resource, write a query and provide a direct portal link.

One attempted answer concerned resource changes/activity logs, while the instructor wanted application-generated events. Azure resource-management activity and application traces/requests/exceptions are different data sources. A query should identify the intended workspace/component, available table/schema, time window and relevant operation or release.

The presenter eventually reported request-duration and exception-related results and observed telemetry flowing. These notes report that outcome from the recording; this documentation task did not query the user's live Azure resources or independently reproduce the result.

A small time-window query or a deep link is useful only if it points to the intended environment. Ingestion delays, empty result sets, filters and schema choices can affect interpretation. Avoid treating the absence of one query result as proof that the application did not emit anything, or treating any nonempty table as proof of a correct release.

## 23. Preserve a repeatable validation workflow

The instructor asked the assistant to capture the demonstrated process in a workflow Markdown file and link it from the local agent instructions. The intended loop was:

1. Make a bounded local change.
2. Run the relevant unit and component checks.
3. Build/publish/redeploy the changed artifact in the authorized development environment.
4. Exercise the intended remote API path.
5. Inspect the corresponding telemetry and expected response/data.
6. Record results, limitations and recovery steps so future changes can repeat the process.

A written workflow is a useful handoff, not automatic enforcement. Scripts, tests, permissions and review must actually implement the checks. The workflow should evolve when the team finds missing cases or inefficient steps.

The recording linked this to the prior instructions/rationale/procedure discussion. This repository uses the exact `AGENTS.md` name for scoped guidance. Other tools' filenames and memory discovery are not universally interchangeable, and historical claims about which product invented a filename are not necessary to the exercise.

## 24. What was demonstrated, and what still needed validation

In the closing recap, the instructor described starting from a backend skeleton without HTTP APIs, a container image, a Kubernetes deployment or a prepared local Docker/WSL environment. The live workflow added the deployment prerequisites, used authenticated agent access, connected parallel sessions, redeployed instrumentation and observed telemetry.

The instructor explicitly said that only the backend had been deployed and that the actual business logic still needed to be exercised. A later session could verify a concrete feature and connect the frontend. The deployment bootstrap was therefore not presented as a completed translation-review product.

The demonstrated state should be read in layers:

| Claim | Evidence discussed | What it does not establish |
| --- | --- | --- |
| Code builds | Build result reported by the assistant | Required domain behavior is correct. |
| Tests run/coverage reported | Test agent's report | Every assertion or coverage exclusion was independently reviewed. |
| Image exists | Local image build and registry publication described | A running cloud workload exists. |
| Deployment progresses/completes | Cloud deployment inspection/status | Every pod is ready or every feature works. |
| Health/API request returns | Request/response and target discussion | All ingress paths or CRUD behavior are verified. |
| Telemetry arrives | Monitoring/query results reported | All business data and workflows are correct. |

The existing [classroom backend baseline](../../../project/src/Translation/Backend/README.md) is useful prior context. This curriculum update does not merge the separate test/deployment worktree changes or claim that the current main checkout contains every artifact shown on screen. Application changes remain separate practice/integration work.

## 25. The closing homework

The instructor asked participants to:

- Build the basic backend create/read/update/delete operations, using the resource/API design already discussed.
- Familiarize themselves with Azure; the instructor offered account access to participants doing the work.
- Try a basic frontend page connected to the backend deployed in the development environment.
- Be ready to inspect actual feature/data behavior rather than rely on deployment or telemetry alone.

The recording does not establish that all students already had cloud access, endpoint details or the same environment. The [homework](homework.md) provides a bounded course submission and a way to disclose an access blockage and local rehearsal honestly. It does not require each student to provision a new Kubernetes cluster or purchase resources.

Tool suggestions and subscription-price anecdotes near the end were incidental, not a required model/tool choice or a verified current price. The proposed future infrastructure topics do not revise the remaining course schedule in this material update.

## Recording reference points

| Approximate time | Topic |
| --- | --- |
| 00:00–00:08 | SDLC recap, scope/design/work hierarchy and limits of the previous skeleton. |
| 00:08–00:15 | Test/deployment worktrees, useful coverage and background test work. |
| 00:15–00:24 | Azure scopes, deployment environments and telemetry purpose. |
| 00:24–00:40 | Containers/Kubernetes discussion, authentication alternatives and local setup gaps. |
| 00:40–00:49 | Pods/scaling, Docker/WSL setup, interactive sign-in and permissions. |
| 00:49–01:00 | Cross-session artifact handoff and image/liveness/readiness quiz. |
| 01:00–01:15 | Deployment progress, access restrictions, test report and registry workflow. |
| 01:15–01:28 | Instrumentation, redeployment, API clients and tunnel/local-target confusion. |
| 01:28–01:40 | Reusable validation workflow, portal/monitoring queries and evidence. |
| 01:40–01:47 | Telemetry recap, business-validation gap and CRUD/frontend homework. |
