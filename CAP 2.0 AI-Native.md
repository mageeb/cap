# **CAP 2.0: AI Native Software Engineering Residency**

## **Overview & Educational Vision**
The goal of this program is to take junior engineers and transform them into job-market-ready **AI Native Engineers**.

Students learn when to work **in the loop**, reviewing each step, and when to work **on the loop**, defining goals and checks while supervising execution at checkpoints. Both modes require engineering judgment. Modern engineering skill is no longer just syntax memorization; it is measured by the ability to **manage the context window**, strictly enforce API contracts, and **recognize AI hallucinations** before they reach production.

This residency does not teach students how to be passive consumers of AI. It trains them to be Sovereign Engineers who command AI to build reliable, production-grade systems.

---

## **Program Goals & Constraints**

| Parameter | Description |
| :---- | :---- |
| **Entry Prerequisites** | Students can already run a project, use Git branches, read Python/JavaScript, debug a basic error, and apply SQL fundamentals. This is not a "learn to code" bootcamp; it is a residency to master architectural AI steering. |
| **Duration** | 12 weeks total, structured progressively from Foundations to Mastery. |
| **Format** | Weekly instruction plus 5–10 hours of serious independent work. Weeks 1–4 ramp from setup to sustained and coordinated agent work; the production project begins or continues in Week 5. |
| **Day-one access** | Bring a GitHub account and your own paid AI subscription or agent plan with both VS Code integration and terminal support. Verify access in both environments; separate usage limits or billing may apply. |
| **Distribution** | Course materials are shared in full through GitHub, including instructor guidance and any teaching solutions. Students create repositories or fork existing ones and submit repository/PR links. |
| **Mentorship** | High-leverage coaching capped at 30-60 mins per mentee weekly. Mentors focus on architecture and AI steering, not syntax debugging. |
| **The "2-Hour Hatch"** | Use structured self-help: change approach after about 45 minutes, inspect evidence directly by 90 minutes, and seek targeted mentor help by two hours. Escalate access, data-loss, security, or spending blockers immediately; do not repeat failed agent runs to satisfy a timer. |

---

## **The Project: The Production-Grade Cloud Application**
To prevent students from relying on simple "one-shot" prompts, the residency develops an end-to-end **Production-Grade Cloud Application** from Week 5 through Week 12, following progressive AI engineering assignments in Weeks 1–4. This architecture is deliberately complex, forcing students to master architectural boundaries across the stack.

**The Application Architecture:**
1.  **Data/Event Ingestion:** Handling user inputs, webhooks, or external API streams.
2.  **Business Logic Layer:** A Python backend for complex rules, transactions, and transformations.
3.  **Persistence:** PostgreSQL for the shared production path; alternative persistence requires an explicit equivalent learning plan.
4.  **Interface:** A dashboard or consumer UI interacting with the backend (Strict TypeScript).
5.  **Infrastructure:** Full Dockerization and automated CI/CD.

**Approved Domains (Students select one):**
*   **Content Streaming (Netflix Clone):** Video metadata ingestion, recommendation processing, and content UI.
*   **Marketplace (Airbnb Clone):** Property ingestion, booking conflict resolution, and search interface.
*   **Social Feed (Twitter Clone):** High-throughput tweet ingestion, timeline transformation, real-time feed UI.
*   **Ad Tech (Ads Auction System):** Real-time bidding ingestion, auction logic processing, advertiser reporting.
*   **Inventory (Library Management):** Tracking availability, managing checkout race conditions, librarian dashboard.
*   **Social Reading (Goodreads Clone):** Metadata ingestion, review processing, social graph visualization.
*   **Compliance (Legal Storage):** Secure document ingestion, metadata extraction (OCR/LLM), role-based UI.

---

## **The Standardized Agentic Workspace (SAW)**
Students create or fork their own applications and configure explicit engineering checks. This repository supplies curriculum, teaching examples, and a [continuing classroom project](project/README.md), currently a scaffold to develop during teaching. Branch protection and review policy must be enabled in their own repository; copying a workflow alone does not prevent merging.
*   **Structure:** `/src`, `/tests`, `/infrastructure`, and specifically `/docs/prompt-logs`.
*   **Automated checks:** Week 6 teaches students to configure dependency installation, tests, security checks, frontend builds, and evidence-log validation for their own projects. The existing workflow file is an illustrative template requiring adaptation, not a ready-to-run application pipeline or an AI grader.

---

## **AI-Augmented Mentorship & Evaluation**
Mentors evaluate working demonstrations, engineering decisions, and verification evidence. Automated checks supply evidence; they do not establish complete correctness or assign a final grade.

The repository includes an optional **LLM-assisted review prompt**, not an implemented model-calling GitHub Action. An instructor may supply the week, assignment, diff, and selected evidence to that prompt using an approved tool, then verify the resulting provisional assessment. Student text and code are untrusted evidence, not instructions to the reviewer.

Use the shared dimensions: context management, validation/correction, structural oversight, and system integrity. Week 1 has an exploratory rubric; Weeks 2–4 use their homework rubrics; Weeks 5–12 use the four dimensions equally. Mentors spend an initial 5–10 minutes per review and use the weekly 30–60 minute coaching allocation for unresolved gaps. Require a working demonstration, an architecture explanation, and a truthful account of what the student trusted, checked, and corrected. Do not promise employment or production readiness based on course completion alone.

---

## **The 12-Week Progressive Curriculum**

### **Phase 1: Foundations (Weeks 1-4)**
*Focus: Establishing the baseline for AI collaboration, context management, and practical engineering skills.*
*   **W1 | Inspiration:** Three nontechnical builders present finished work, workflow evolution, and lessons learned, followed by Q&A. No classroom project; setup and exploration happen at home.
*   **W2 | Guided Practice:** Build a fresh app in VS Code. Prompt versus context, instruction layers, project instructions, reusable skills, and the human-in-the-loop validation cycle.
*   **W3 | Sustained Agent:** Terminal workflows with one agent: context discovery, plans, checkpoints, skills, acceptance checks, and on-the-loop supervision. A 90-minute lesson includes a 25-minute Excalidraw follow-along; homework applies the workflow to a feature or fix in an owned or authorized repository.
*   **W4 | Orchestration:** Coordinate agents through script-driven, instruction-driven, and hybrid harnesses. Define roles, connections, ownership, stopping rules, and integration checks; use a harness for an open-ended hard build.

### **Phase 2: Production (Weeks 5-8)**
*Focus: Building, testing, and deploying the core cloud application to ensure functional skills are gained.*
*   **W5 | Production Entry:** Start a new cloud application or carry forward a suitable Week 4 project; establish architecture, data model, migrations, API contracts, and an integrated frontend.
*   **W6 | Quality & Access:** Contract-focused tests, access policy, coverage, security/dependency checks, and evidence-based quality gates.
*   **W7 | CI/CD:** Infrastructure (Optimized multi-stage Dockerfiles, `docker-compose` networking, GitHub Actions).
*   **W8 | Deployment:** A bounded demonstration with managed persistence, explicit access controls, service identity, budget alerts, verification, recovery, and cleanup.

### **Phase 3: Mastery & Workflows (Weeks 9-12)**
*Focus: Advanced operational debugging, interview prep, and workflow optimization.*
*   **W9 | Incident Response:** Break the app (e.g., N+1 queries, serialization crashes). Students must use AI to analyze raw stack traces, isolate the failure, and write regression tests before generating the fix.
*   **W10 | Workflow Audit:** Evaluate the capstone workflow using evidence from Weeks 1–9, test one improvement, and write a personal AI Workflow SOP.
*   **W11 | Interviews:** AI-Enabled SWE Interviews (Prompt audits of vulnerable code, refactoring with strict engineering constraints).
*   **W12 | Demos:** Project Demos & Workflow Sharing. Students present their live end-to-end pipelines and a "Prompt Case Study" showing Before/After code.

**Assessment alignment:** Week 1 uses the field-report rubric; Weeks 2–4 use their published assignment rubrics, with the shared engineering rubric supplying evidence anchors. Weeks 5–12 use the shared rubric. Credit verified outcomes, architecture decisions, and justified intervention; do not reward app size or penalize iteration count by itself.

## Weekly Materials and Delivery

Each weekly folder links to its talk outline and homework. The outline contains the agenda, teaching prompts, and demonstration plans. Weeks 2–4 also include reusable reference procedures; Week 3 includes pre-class setup and two alternative Excalidraw live-demo runbooks. All materials are visible to students; disclose reuse in submissions.

| Week | Start here | Homework |
|---|---|---|
| 1 | [Inspiration materials](curriculum/weeks/01/README.md) | [Setup and first attempt](curriculum/weeks/01/homework.md) |
| 2 | [Guided practice materials](curriculum/weeks/02/README.md) | [A fresh application](curriculum/weeks/02/homework.md) |
| 3 | [Terminal and sustained-agent materials](curriculum/weeks/03/README.md) | [Agentic development workflow](curriculum/weeks/03/homework.md) |
| 4 | [Orchestration materials](curriculum/weeks/04/README.md) | [An open-ended hard build](curriculum/weeks/04/homework.md) |
| 5 | [Week 5 materials](curriculum/weeks/05/README.md) | [Assignment and assessment](curriculum/weeks/05/homework.md) |
| 6 | [Week 6 materials](curriculum/weeks/06/README.md) | [Assignment and assessment](curriculum/weeks/06/homework.md) |
| 7 | [Week 7 materials](curriculum/weeks/07/README.md) | [Assignment and assessment](curriculum/weeks/07/homework.md) |
| 8 | [Week 8 materials](curriculum/weeks/08/README.md) | [Assignment and assessment](curriculum/weeks/08/homework.md) |
| 9 | [Week 9 materials](curriculum/weeks/09/README.md) | [Assignment and assessment](curriculum/weeks/09/homework.md) |
| 10 | [Week 10 materials](curriculum/weeks/10/README.md) | [Assignment and assessment](curriculum/weeks/10/homework.md) |
| 11 | [Week 11 materials](curriculum/weeks/11/README.md) | [Assignment and assessment](curriculum/weeks/11/homework.md) |
| 12 | [Week 12 materials](curriculum/weeks/12/README.md) | [Assignment and assessment](curriculum/weeks/12/homework.md) |

Week 2 introduces project instructions and skills with a small example; Week 3 develops them into tools for maintaining context across sustained work. Week 4 extends supervision to multiple agents. Gas Town is a possible instructor demonstration, not a required student purchase or a finalized course dependency. The instructor brings one working orchestrator setup and contrasts it with three harness designs.

The Week 4 problem need not become the production capstone. At Week 5, students may begin anew or continue if their project fits the production learning objectives. Do not assume a Week 4 Python backend, SQL schema, or migration history. Week 5 establishes a narrow slice and initial migration; Week 7 teaches follow-up migration and PostgreSQL transition.
