# CAP Curriculum Delivery Plan

The [curriculum overview](../CAP%202.0%20AI-Native.md) defines the sequence, prerequisites, project choices, and weekly material index. This file records delivery decisions and preparation.

## Delivery Decisions
- All material is shared in full on GitHub, including instructor guidance and teaching solutions. Students bring their own GitHub account and paid agent access in both VS Code and the terminal from day one.
- Budget 5–10 hours of independent work each week. Assignments assess demonstrated outcomes, decisions, and verification using the criteria and any weights in each homework document.
- Week 1 is a two-hour session with three nontechnical builders showing finished work and workflow evolution, plus Q&A. There is no classroom project; setup and exploration happen at home.
- Weeks 2–11 use a three-hour classroom envelope. Week 3 uses a 90-minute core lesson including a 25-minute Excalidraw follow-along; use the remaining time for guided practice, debugging, and coaching. Week 12 reserves most of the session for student demonstrations; use the weekly outline for timing.
- Week 2 introduces prompt versus context, instruction layers, project files, and skills in VS Code. Week 3 develops these through one sustained terminal agent and checkpoint supervision. Week 4 adds agent coordination through three harness designs.
- Week 3 homework applies the agentic workflow to a feature or fix in an owned or authorized repository. The Week 4 harness project may continue into Week 5 or be replaced. Week 5 establishes the production project's initial migration; Week 7 adds the data-preserving migration and PostgreSQL checks.

## Instructor Preparation
These are tasks to complete before teaching; the outlines describe planned demonstrations.

- **Week 1:** Confirm three speakers, review their product walkthroughs, and obtain permission before sharing their materials on GitHub. Add links only once those materials exist.
- **Week 2:** Rehearse the fresh task-board build using the selected VS Code agent. Prepare and publish any checkpoints used in class.
- **Week 3:** Share pre-class setup, choose one Excalidraw live-demo runbook, and rehearse its workflow. Record setup commands and baseline results for the checkout; students work in their own forks and debug feature failures together in class.
- **Week 4:** Bring one working orchestrator and its actual agent adapter. Rehearse the script-driven, instruction-driven, and hybrid demonstrations from the outline. Gas Town is optional; no provider is mandated.
- **Weeks 5–9:** Select an existing classroom application and prepare the behaviors described in each outline. Record the base commit and check commands. Before Week 8, rehearse the cloud path, access policy, persistence, budget, and cleanup. Publish prepared artifacts on GitHub when available.
- **Weeks 10–11:** Prepare the paired workflow runs and bounded interview repair described in the outlines.
- **Week 12:** Publish the presentation order. Allow ten minutes per student; arrange parallel review groups if the cohort exceeds fourteen students.

## Maintenance and Scope
Maintain one talk outline and one homework specification per week. Keep additional files only when they provide distinct, usable content. Put speaker guidance and demo plans in the outline; add slides, recordings, or external demo links only when the actual material is available. Check links after moving or removing content.

Curriculum ownership covers the full learning progression, teaching guidance, assignments, assessment, and transitions. Preserve the existing folder structure and resolve routine editorial decisions within the instructor's direction.

The [continuing classroom project](../project/README.md) now has a scaffold under `project/`; keep its specifications, tasks, code, and application-specific infrastructure there. Follow the [repository instructions](../AGENTS.md) and [project instructions](../project/AGENTS.md) for work in that area. Build application behavior, add hosted CI, or deploy infrastructure when requested as subsequent work. Use a feature branch for curriculum revisions as well as application changes, following the repository's commit-size, validation, and pre-commit review rules.
