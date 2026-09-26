# Autonomous decisions awaiting user review

This log separates agent choices from user-confirmed requirements. It starts with the introduction of this workflow; it is not a retrospective audit of earlier sessions. Follow the [repository rules](AGENTS.md#autonomous-decisions-and-user-review).

Each entry has a stable `DEC-###` ID, task or scope, status, choice, reason, alternatives, effects or links, technical verification, and user response. Add new entries without reusing IDs. Keep reviewed entries as history.

Statuses: **Pending user review**, **Confirmed**, or **Redirected**. Passing checks does not change user-review status. A deferred decision stays pending. For a redirect, record the replacement direction and follow-up work separately from its completion evidence.

## DEC-001: Location of the decision log

- **Task / scope:** Collaboration rules; repository and classroom application.
- **Status:** Pending user review.
- **Choice:** Keep a single log in the repository root at `DECISIONS.md`.
- **Reason:** Fresh sessions can find both repository and application choices through the root instructions.
- **Alternative:** Separate logs for repository work and application work.
- **Effects:** [Root instructions](AGENTS.md), [project instructions](project/AGENTS.md), and [handoff guide](project/docs/workflow.md) link here. Feature task records link to relevant decision IDs.
- **Technical verification:** Independent sanity and adversarial review passed; all 24 local links in the staged changes, including three anchors, resolve.
- **User response:** Not reviewed.

## DEC-002: Decision entry format

- **Task / scope:** Collaboration rules; decision tracking.
- **Status:** Pending user review.
- **Choice:** Use Markdown entries with stable IDs and separate user-review status, verification evidence, and response fields.
- **Reason:** This records whether a choice works without implying the user has agreed to it, and preserves corrections across sessions.
- **Alternative:** A compact table with the same fields.
- **Effects:** New entries follow the fields above; confirmation or redirection updates the existing entry while preserving the original choice.
- **Technical verification:** Independent review and mechanical checks passed; decision IDs are unique, and both choices remain pending user review.
- **User response:** Not reviewed.

## DEC-003: Module 03 transition slide

- **Task / scope:** Revise slide 3 after the user rejected the Week 2/Week 3 conceptual split.
- **Status:** Pending user review.
- **Choice:** Summarize actual Week 2 practices, then name the additional Module 03 practices; align the instructor's transition guidance.
- **Reason:** Week 2 already teaches agents, subagents, skills, and verification, so interaction versus delegation misrepresents the progression.
- **Alternative:** Remove the transition slide entirely or keep a generalized workflow comparison.
- **Effects:** [Slide deck](curriculum/weeks/03/presentation.md) and [instructor outline](curriculum/weeks/03/talk-outline.md#teaching-through-line); no change to demo scope or timing.
- **Technical verification:** Compared claims with Week 2 slides; independent adversarial source review found no textual blockers. Python checks passed for 11 local links/anchors, unchanged 54-slide count, and preservation of all other slides including the user's existing slide 2 edit. `git diff --check` passed. Used the existing compact style for fit; rendered layout not checked.
- **User response:** User requested a revision; replacement wording not yet reviewed.

## DEC-004: Module 03 CLI session and model controls

- **Task / scope:** Add session resumption and model/effort selection under the CLI section.
- **Status:** Pending user review.
- **Choice:** Add two compact slides immediately after the Claude/Codex comparison, with command tables, selection guidance, and sources; align reference and instructor notes within the existing CLI time allocation.
- **Reason:** Students can learn the controls after meeting both tools; separate slides keep each concept readable.
- **Alternative:** Combine both into one dense slide or place resume only in the later durable-context section.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [reference](curriculum/weeks/03/reference.md), and [instructor notes](curriculum/weeks/03/talk-outline.md#cli). Use model-independent guidance and available pickers rather than a fixed ranking.
- **Technical verification:** Checked `claude --help`, `codex --help`, `codex resume --help`, and linked official Claude/OpenAI docs. Independent adversarial review found no factual blockers; fixed its whitespace finding. Python checks confirmed exactly two added CLI slides, all prior slides preserved, and 15 local links/anchors resolving. Rendered both slides with cached Marp CLI (`--html --images png`); shortened overflow and visually verified the rerender, including source footers. `git diff --check` passed.
- **User response:** Topics requested; placement and wording not yet reviewed.

## DEC-005: Module 03 planning mode and chat comparison

- **Task / scope:** Teach CLI planning mode and explain whether/how it exists in chat.
- **Status:** Pending user review.
- **Choice:** Add two compact slides after model/effort: practical Plan-mode controls, then a comparison with ordinary chat without repository tools. Explicitly acknowledge dedicated Plan modes in graphical coding-agent interfaces.
- **Reason:** A terminal-versus-chat binary would misrepresent current tools; separating controls from the comparison preserves readability.
- **Alternative:** One combined slide, or a misleading claim that chat cannot plan or has no Plan modes.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [reference](curriculum/weeks/03/reference.md#planning-mode-and-planning-in-chat), and [instructor notes](curriculum/weeks/03/talk-outline.md#cli). Retains the existing CLI time allocation with depth deferred to the later planning section.
- **Technical verification:** Official Claude/OpenAI docs and local CLI help inspected. Independent research and adversarial review found no supported blockers. Python checks confirmed 58 slides, exactly two additions after model/effort, all 56 previous slides preserved, and 19 local links/anchors resolving. Rendered both added slides with cached Marp CLI (`--html --images png`) and visually verified fit, tables, and source footers. `git diff --check` passed.
- **User response:** Concept and comparison requested; wording not yet reviewed.

## DEC-006: Module 03 YOLO mode

- **Task / scope:** Explain YOLO and why it is used under the CLI section.
- **Status:** Pending user review.
- **Choice:** Add one compact slide after permissions, with Claude/Codex flags, practical motivation, and the distinction between approval bypass and sandbox removal; expand reference and instructor notes.
- **Reason:** The term is informal and tool behavior differs; students need to understand both the reduced interruptions and increased scope for unintended actions.
- **Alternative:** Mention only the nickname, or omit commands; either would leave the practical meaning unclear.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [reference](curriculum/weeks/03/reference.md#yolo-and-permission-bypass), and [instructor notes](curriculum/weeks/03/talk-outline.md#cli). Explain flags without making bypass a classroom setup requirement.
- **Technical verification:** Official docs and local CLI help inspected; independent adversarial source review passed. Rendered with Marp, shortened the overflowing first draft, and visually confirmed the final slide including its source footer. `git diff --check` and affected local-link checks passed.
- **User response:** Topic requested; wording not yet reviewed.

## DEC-007: Module 03 coding harness diagram

- **Task / scope:** Add a harness slide and diagram at the end of the CLI section.
- **Status:** Pending user review.
- **Choice:** Show the CLI and agent runtime inside the harness, with a separate model and tool environment. Keep editable Mermaid source and a slide-ready SVG.
- **Reason:** Separates the model's reasoning from context management, tool execution, permissions, and the user interface without implying the model directly operates the filesystem.
- **Alternative:** A text-only definition or a diagram that combines the CLI and model into one component.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [diagram source](curriculum/weeks/03/assets/coding-harness.mmd), [SVG](curriculum/weeks/03/assets/coding-harness.svg), reference and instructor notes.
- **Technical verification:** Read Anthropic's harness engineering article; independent adversarial review found no blockers. Required Mermaid extension tools were unavailable; manual Mermaid syntax/graph review and SVG XML validation passed. Rendered the SVG within the Marp slide and visually checked fit, labels, arrows, and footer. Confirmed the harness slide immediately precedes Hooks; the deck contains 60 slides. `git diff --check` passed.
- **User response:** Slide, placement, and diagram requested; exact depiction not yet reviewed.

## DEC-008: Remove the custom approach-comparison skill

- **Task / scope:** Remove Module 03 references to brainstorm as requested.
- **Status:** Redirected.
- **Choice:** Remove the custom skill and its command references; retain useful approach comparison as an ordinary prompt. Label remaining supplied skills as course-provided.
- **Reason:** Preserves the teaching activity without implying a built-in CLI skill.
- **Alternative:** Remove the comparison activity entirely.
- **Effects:** Module 03 presentation, README, reference, talk outline, both demo runbooks, and skill files.
- **Technical verification:** Independent cleanup and adversarial reviews passed. Module 03 Markdown has no remaining occurrences of the removed term; 42 local links/anchors across Module 03 and this log resolve. Rendered affected skill/demo slides; shortened an overflowing prompt slide and verified its rerender. `git diff --check` passed.
- **User response:** Requested a concrete replacement using built-in CLI capabilities, then explicitly instructed implementation of the Plan mode + repository exploration plan.
- **Replacement direction:** Use built-in Plan mode and file-reading/search tools to compare approaches; leave Plan mode before writing goal/spec documents, with implementation deferred. Keep the other course-provided skills unchanged.
- **User response:** Redirected: asked to replace the removed Brainstorm reference with built-in CLI capabilities; then requested a crisp instructor-led live demo students can follow, with prepared fallback states removed and failures debugged in class.
- **Replacement direction:** Use built-in Plan mode and file-reading/search tools to compare approaches; leave Plan mode before writing goal/spec documents. Keep other course-provided skills unchanged. See [DEC-010](#dec-010-module-03-live-follow-along-demo-format) for the live-demo format.

## DEC-009: Hooks basics and appendix

- **Task / scope:** Reduce the main hooks discussion to basics and move additional material to an appendix after the bonus sections.
- **Status:** Pending user review.
- **Choice:** Use two core slides for definition, triggers/uses, and a lint-after-edit example. Move detailed JSON configuration and operational advice behind an Appendix divider at the very end, after both bonuses and closing slides.
- **Reason:** Students can understand the event/action mechanism before reading setup details; the appendix remains available without interrupting the core lesson.
- **Alternative:** Keep the configuration example in the main lesson, or insert the appendix before the closing slides.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md) and [instructor outline](curriculum/weeks/03/talk-outline.md#hooks); synchronized command-path quoting in the detailed reference.
- **Technical verification:** Independent adversarial review found no blockers; added its script-output caveat to appendix speaker notes. Confirmed two core hooks slides, appendix placement after both bonuses and closing slides, and preservation of unrelated slides (61 total). Parsed the configuration JSON and confirmed it matches the reference. Rendered and visually inspected all five affected slides with Marp; no overflow. Local-link and whitespace checks passed.
- **User response:** Simplification and appendix requested; exact organization not yet reviewed.

## DEC-010: Module 03 live follow-along demo format

- **Task / scope:** Turn the Module 03 Excalidraw demo into an instructor-led, student follow-along.
- **Status:** Pending user review.
- **Choice:** Give each phase a concise explanation and exact command or prompt. Use each student's own synced fork and create a feature branch during class. Debug failures together in class.
- **Reason:** Students practice the same live workflow on their machines and see how the instructor investigates real failures.
- **Alternative:** Keep timed instructor-only directions and a feature branch rooted at an instructor-supplied starting commit.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md#follow-the-live-demo-branch-and-explore), both demo runbooks, pre-class setup, talk outline, and reference.
- **Technical verification:** Both runbooks use each student's own fork, create the feature branch during class, and end with an in-class debugging prompt; setup describes student environment installation only. Search found no checkpoint or instructor-prepared demo references in Module 03. Local link targets in 19 Markdown files exist; `git diff --check` passed. Selected live-demo slides rendered in Marp; checked readability of beginning, review, handoff, PR, and debug slides.
- **User response:** Live follow-along and in-class debugging explicitly requested; exact sequence not yet reviewed.

## DEC-011: Add practical CLI integration guidance

- **Task / scope:** Compare our Module 03 deck with the Claude-generated presentation and add only useful gaps.
- **Status:** Pending user review.
- **Choice:** Add a concise explanation of `git` versus `gh` and authenticated GitHub access, plus one MCP overview and setup example. Retain our existing harness, hook, subagent, and spec-driven material.
- **Reason:** The GitHub authentication distinction answers a practical CLI question and clarifies what account access the agent inherits. MCP adds a missing standard way to connect external tools and context.
- **Alternative:** Copy more product-specific command inventory and workflow slides from the other deck.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), reference, and instructor outline. Read the Claude worktree presentation without modifying it.
- **Technical verification:** Official MCP, Claude Code, Codex CLI, and GitHub CLI documentation and installed CLI help checked. Rendered and visually checked Git/gh, MCP, all follow-along slides, and Boris workflow slides. MCP and Boris slide contents and source lines fit. Local link targets in 19 Markdown files exist; no checkpoint/instructor-preparation references remain in Module 03; `git diff --check` passed.
- **User response:** Selective comparison and MCP slide explicitly requested; exact additions not yet reviewed.

## DEC-012: Distinguish Boris’s personal workflows from Claude Code features

- **Task / scope:** Add the requested `/commit-push-pr`, `/simplify`, `/batch`, `/go`, `/loop`, and `/goal` examples at the end of Boris Cherny’s section.
- **Status:** Pending user review.
- **Choice:** Present a compact table that labels the bundled Claude Code skills and built-in `/goal` separately from Boris’s personal skills/workflows. Note the same-name overlap with course-provided `/goal` and `/simplify` skills and direct students to `/skills` to inspect their CLI.
- **Reason:** The request mixes personal workflows and built-in functionality; preserving that distinction teaches what students can expect in a standard install.
- **Alternative:** Present the list without provenance or call all entries “skills.”
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [reference](curriculum/weeks/03/reference.md#reusable-claude-code-workflows-from-boris-cherny), and [instructor outline](curriculum/weeks/03/talk-outline.md#boris-cherny-workflow-examples).
- **Technical verification:** Official Claude Code command reference checked for `/simplify`, `/batch`, `/loop`, and `/goal` classifications and behavior. Marp render visually confirmed the table, overlap note, and source line fit. Local link targets in 19 Markdown files exist; `git diff --check` passed. `/simplify` is identified as cleanup rather than correctness review.
- **User response:** Requested concepts and placement; exact wording and provenance labels not yet reviewed.

## DEC-013: Distinguish a skill from an installable plugin

- **Task / scope:** Explain the one-`SKILL.md` procedure versus plugin bundle distinction and show a plugin example in both CLIs.
- **Status:** Pending user review.
- **Choice:** Place the comparison immediately after the Skills slide, define marketplace distribution, qualify component support by host, and use the OpenAI Developers plugin with separate Claude Code and Codex CLI install paths.
- **Reason:** The distinction is easiest to understand after students learn what a skill is; the same named example also demonstrates that plugin contents and install flows differ by CLI.
- **Alternative:** Add a generalized plugin list to the initial CLI comparison slide or imply each plugin contains the same components everywhere.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [reference](curriculum/weeks/03/reference.md#skills-and-plugins), and [instructor outline](curriculum/weeks/03/talk-outline.md#skills-and-plugins).
- **Technical verification:** Official Claude Code and OpenAI plugin documentation checked for package contents, marketplace installation, and the OpenAI Developers example. Marp rendered both the Skills slide and new plugin slide; both install paths, contents caveat, and sources are visible. Local link targets and anchors, commands, and cross-file wording checked; `git diff --check` passed.
- **User response:** Concept and both-CLI example requested; exact wording and placement not yet reviewed.

## DEC-014: Show the full clone-to-prompt demo path

- **Task / scope:** Clarify student steps for the live Excalidraw follow-along, beginning with Git clone and continuing through the exact CLI prompts.
- **Status:** Pending user review.
- **Choice:** Put the exact fork clone command in pre-class setup and the deck; label cloning as pre-class work, then direct students to verify their fork, create a feature branch, enter Plan mode, and paste the corresponding runbook prompts in order.
- **Reason:** The instructor and students need an executable sequence that distinguishes setup completed before class from commands performed together during the demo.
- **Alternative:** Start the in-class slides at branch creation without showing how the student clone was obtained.
- **Effects:** [Presentation](curriculum/weeks/03/presentation.md), [pre-class setup](curriculum/weeks/03/pre-class-setup.md#4-fork-and-clone-excalidraw), both [demo runbooks](curriculum/weeks/03/README.md#files), and [instructor outline](curriculum/weeks/03/talk-outline.md#instructor-demonstration-plan-25m).
- **Technical verification:** `git diff --check` passed. Local links and anchors in the presentation, pre-class setup, both demo runbooks, talk outline, and decision log resolve. Confirmed the literal `git clone` command, both feature-branch commands, both CLI Plan-mode paths, and links to each exact Step 2 prompt. Rendered the three affected slides with Marp and visually checked layout; all text, commands, and links fit.
- **User response:** Clone-to-prompt sequence requested; exact steps and placement not yet reviewed.

## DEC-015: Make both demo runbooks usable one step at a time

- **Task / scope:** Match both Excalidraw demo guides to the instructor/student walkthrough requested by the user.
- **Status:** Pending user review.
- **Choice:** Use a short purpose, exact terminal command or agent prompt, and expected result for each step. Include the fork-to-running-app setup in each runbook, separate goal/spec creation, and implement one agreed task per prompt. Keep platform installation details linked, with a compact macOS Homebrew route for the setup issues encountered in the walkthrough.
- **Reason:** Students need to know where to type, what a command does, and whether it worked before moving on; either optional demo should stand alone.
- **Alternative:** Link to setup only and retain larger multi-action phases.
- **Effects:** [Focus Mode runbook](curriculum/weeks/03/demo-focus-mode.md) and [Presentation Mode runbook](curriculum/weeks/03/demo-presentation-mode.md). No application changes or publishing actions.
- **Technical verification:** Independent read-only command and adversarial reviews checked installed CLI help, official GitHub/Claude documentation, and the local Excalidraw package requirements. Applied the review's fixes for re-staging corrections and inspecting commit identity before committing. All 16 local links/anchors in the runbooks and this new entry resolve; all 42 shell blocks pass `bash -n` and `zsh -n`. Numbering, code fences, feature requirements, and own-fork destinations checked. `git diff --check` passed. File hashes confirm this task changed only the two runbooks and appended this entry. Setup/feature execution was not rerun; this was a documentation review.
- **User response:** Walkthrough style explicitly requested; exact organization not yet reviewed.

## DEC-016: Publish the remaining Module 03 work from the merged base

- **Task / scope:** Commit the remaining `module03_gpt` files and create a follow-up PR after PR #7.
- **Status:** Pending user review.
- **Choice:** Start `feature/module03-follow-up` from `origin/main` and import the remaining changes from saved stash `63a8ac6`. Keep `module03_gpt` and the stash as backups. Commit the independent diagram, templates, and skills in small groups, followed by the linked curriculum replacement. Publish through the existing `tameraw/cap` fork, using a `fork` remote, as PR #7 did.
- **Reason:** The saved presentation and Excalidraw setup already match main; a fresh base avoids replaying the original commits or including already-merged file changes. The linked lesson, setup, demos, reference, assignment, and decision history need one coherent replacement commit.
- **Alternative:** Merge main into the original branch and publish its existing commit history, or put all remaining files into one large commit.
- **Effects:** Remaining Week 03 teaching files and this decision log. The PR serves as the task record and links the pending decisions.
- **Technical verification:** The initial staged tree exactly matched `63a8ac6` (`git diff --cached --exit-code 63a8ac6` passed). No presentation or Excalidraw setup changes remain relative to main. Independent content and adversarial reviews passed after the integration repairs. Staged validation passed for 107 local links/anchors, 95 shell blocks in both bash and zsh, SVG XML, unique decision IDs, and whitespace. No application code was added; setup and feature execution were not rerun. GitHub rejected a direct upstream push (403); API checks confirmed PR #7 used `tameraw/cap`, that fork is writable, and `mageeb/cap` is read-only for the active account.
- **User response:** Committing all remaining files and opening a new PR explicitly requested; packaging not yet reviewed.

## DEC-017: Align course records with the replacement Module 03 package

- **Task / scope:** Repair integration inconsistencies found while reviewing the remaining Module 03 changes.
- **Status:** Pending user review.
- **Choice:** Replace external-package language in the Week 03 README and stale seed-exercise references in the course overview and delivery plan. Reflect the saved homework's owned-or-authorized repository scope and qualitative rubric. Keep the three-hour classroom envelope around the 90-minute core lesson and reserve the balance for practice, debugging, and coaching. Fill two empty context-template bullets to remove trailing whitespace and repair three decision-log links to slide headings that changed before merge.
- **Reason:** The imported package replaces the seed exercise, changes homework scope, and supplies a 90-minute outline; the course-level records otherwise give contradictory instructions.
- **Alternative:** Preserve the stale records or redesign the saved lesson and homework to match the old package.
- **Effects:** [Week 03 README](curriculum/weeks/03/README.md), [course overview](CAP%202.0%20AI-Native.md), [delivery plan](conductor/plan.md), and [context template](curriculum/weeks/03/templates/PROJECT_CONTEXT.md).
- **Technical verification:** Independent reviewers identified the stale references and whitespace errors, then accepted the repairs. The final staged consistency review and all 107 local links/anchors passed; `git diff --cached --check` passed. The validation also found and repaired three stale slide anchors in earlier entries; their original choices and review status remain preserved.
- **User response:** Not reviewed.
