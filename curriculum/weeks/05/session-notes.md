# Session 05 — Designing an AI-Assisted Translation Review Platform

*Organized account of the October 9 session, based on the recording transcript. Side conversations, repetition, and unrelated remarks have been removed. This document consolidates the discussion; it is not a finalized product requirements document or architecture specification. Unclear names and transcription artifacts have been omitted rather than guessed.*

## 1. Purpose of the session

The session introduced an existing desktop application for reviewing machine-translated books and used it as a practical exercise in product development. The proposed learning project is to rebuild that concept as a web application, progressing through requirements, user experience, architecture, implementation, and iteration.

The broader educational objective is to teach participants how to guide AI through a substantial business application. A familiar game can often be generated from a short prompt because its rules and common implementations are well established. A specialized business workflow requires more discovery: the system must reflect the needs of actual users, domain-specific quality standards, and deliberate scope decisions.

Participants were encouraged to treat themselves as product owners and engineers who direct AI, evaluate its suggestions, and retain control of the result.

## 2. The underlying problem and product vision

The original problem was the time required to translate Arabic religious and theological books into English. Machine translation could produce an initial draft quickly, but that draft still needed careful review by someone who understood both languages and the subject matter.

The intended product is therefore a **translation review platform**. An external translation model supplies the first draft; the application helps a qualified reviewer turn that draft into a reliable translation.

Review must address more than grammar. A fluent sentence can still misrepresent theological meaning, use inappropriate terminology, quote a different Bible version, or contain an incorrect scripture reference. These problems matter when the output is intended for publication.

The longer-term vision includes:

- A web portal accessible across locations and platforms.
- Multiple books or articles moving through translation and review in parallel.
- Accounts, permissions, and distinct responsibilities for different users.
- Multiple reviewers and a process for resolving disagreements.
- A path from reviewed text to publication, including ebooks and other distributable formats.
- A collection of trusted, aligned source and target texts that could support future model improvement.

Arabic-to-English religious material is the starting use case. The workflow could eventually apply to other language pairs and subject areas, provided appropriate reviewers and reference material are available.

## 3. What the desktop prototype demonstrated

### Parallel reading and editing

The prototype displays the original text alongside its translation, allowing the reviewer to navigate corresponding passages and edit the target text directly. It distinguishes the machine-generated draft from the human-edited version and highlights changes.

This arrangement reduces the effort of switching between separate documents and helps the reviewer maintain context. The application is designed around the reviewer's repeated cycle: read a passage, compare meaning, make corrections, and move forward.

### Scripture verification

The application detects scripture references and retrieves the corresponding verse for comparison. The demonstration showed why this is necessary: translation can swap chapter and verse numbers, produce an inaccurate quotation, or use wording from a Bible version other than the one required for publication.

The New King James Version was discussed as the required English wording for the demonstrated material. A quotation that conveys roughly the same meaning may still need replacement with the exact reference text.

The tool can bring the reference into the review interface and make replacement easier, reducing repeated searches and copying between windows.

### Domain-specific terminology

One example concerned the biblical expression “old man” being rendered as “old self.” The point was that a plausible translation may shift the terminology or conceptual framing expected in the source's religious context. Human review must preserve the author's intended meaning and the conventions of the domain.

### Reuse and consistency

The prototype includes a mechanism for capturing recurring words or translation pairs for reuse. The intended benefit is consistency and less repeated work. The demonstration also acknowledged that automatically captured pairs can be wrong, so this assistance requires review.

### Review efficiency

Other demonstrated capabilities included keyboard shortcuts, passage navigation, flags or completion states, translation statistics, and visual cues that help the reviewer stay oriented.

A particularly useful improvement was reading the source aloud while the reviewer reads and edits the target. This replaces some of the visual effort of repeatedly reading both languages with listening to one and reading the other. The prototype also supports scrolling and alignment to help the reviewer follow the text.

The presenter reported a review throughput of roughly **4,000 words per hour**, compared with an approximate manual translation baseline of **200 words per hour**, describing this as about a **20-fold improvement**. These were experience-based claims made during the session, not independently validated benchmarks. A later reference to 40,000 words appears inconsistent with the earlier figures and is not treated here as a separate result.

The prototype's development illustrated iterative product improvement: begin with translation and review, observe bottlenecks, then add assistance such as verse lookup, reusable terminology, and source narration.

## 4. Human review and a trusted parallel corpus

The session emphasized that machine-generated output should not automatically be treated as a completed translation. Reviewers need both language competence and sufficient knowledge of the subject to recognize errors that ordinary proofreading would miss.

At larger scale, the proposed process would involve multiple reviewers. Their judgments could be compared through **inter-judge agreement**, with disagreements sent for further review or adjudication. Agreement was discussed as an input to accepting corrections; the session did not specify a scoring formula, reviewer threshold, or detailed approval procedure.

An additional long-term asset would be a **gold-standard parallel corpus**: aligned passages in the source language and their trusted, human-reviewed translations. Such material could eventually be used for fine-tuning future models and improving translation for the relevant domain and language pair.

This was a future direction. The session did not define a training pipeline, evaluation method, or technical implementation for model fine-tuning.

## 5. Defining users before designing features

The group began product discovery by identifying personas: categories of people who will use the software and the work each category needs to accomplish.

A persona is not automatically the author of the source book or the reader of the finished translation. It is a user of this application. Different personas can require different workflows even when they work with the same underlying material.

Five initial personas emerged:

| Persona | Responsibility discussed |
| --- | --- |
| Reviewer | Compare source and target text, correct the translation, and assess its readiness. |
| Administrator | Manage user access and accounts; the extent of additional authority was debated. |
| Project manager | Coordinate books, reviewers, deadlines, and handoffs. |
| Publisher | Prepare or take the reviewed material through publication, such as an ebook release. |
| Auditor or adjudicator | Review competing corrections and accept, reject, or resolve disagreements. The role's final name was not settled. |

The exercise demonstrated why naming roles and describing their use cases matters. Clear terminology helps both people and AI reason about the same responsibilities. It also makes prioritization possible: the team can decide which users it must support first.

## 6. Narrowing the minimum viable product

The discussion moved from an ambitious multi-role platform toward a deliberately small MVP. The working direction was **one user, acting primarily as a reviewer, with the authority needed to complete the basic workflow**.

Separate administration, project management, publishing, and adjudication workflows could follow later. The same person may perform several responsibilities during early testing without the application needing separate interfaces and permission structures for all of them.

The proposed MVP objective was to translate and review **one small book or article**, then learn whether the workflow is useful. Suggestions for input size ranged from about 10 pages to 30–50 pages; the group did not settle a final limit. The consistent intent was to constrain the workload rather than support arbitrarily large books.

The distinction between a reviewed translation and a published book was important. Publishing to Amazon or preparing a fully formatted ebook was not necessary to test the core review experience.

An MVP was presented as an experiment that produces feedback. It should make it possible to identify where users spend time, where they encounter failures, and which improvements would have the greatest value. Internal use of the product—“dogfooding”—was also discussed as a way to learn before broader release.

The group repeatedly returned to the MVP goal when evaluating features. Supporting multiple books, for example, changes the initial experiment if the original goal is to validate one small document. Features should be included because they serve the chosen objective, rather than because they sound useful in the eventual product.

## 7. The core workflow and requirements discussed

The emerging MVP workflow was:

1. **Upload source material.** Bring the original text into the application within a deliberately limited document size.
2. **Request translation.** Integrate with an external translation API to generate the initial target text. Whether the user explicitly presses a translation button or translation starts automatically remained an interaction detail to resolve.
3. **Read aligned text.** Display corresponding source and translated passages together.
4. **Edit in place.** Let the reviewer correct the translation inside the interface rather than copying it to an external editor.
5. **Save work and return.** Persist the translation and edits so a reviewer can stop and resume without starting over.
6. **Indicate completion.** Provide an appropriate way to identify reviewed work, with the exact granularity still to be clarified.

### Paragraphs as the working unit

The group compared words, sentences, paragraphs, pages, and chapters as possible units of review. The working choice was the **paragraph**, because it preserves more context than a word or isolated sentence while remaining manageable for side-by-side review.

This was a product-level direction. The session did not define the algorithm for segmenting documents or aligning source and translated paragraphs.

### Editing versus a complete change-history system

In-place editing was central. A richer interface showing differences between the original machine translation and the edited result was debated.

The group explored a compromise: retain the original machine output without immediately building every comparison panel or change-tracking feature shown in the desktop tool. Detailed presentation, change reversal, and richer history were possible areas to simplify for the MVP. The precise requirements were not finalized.

### Persistence and completion

Saving progress was treated as essential: reviewers should not have to finish a whole document in a single sitting.

Completion tracking prompted a separate discussion. Marking individual paragraphs can support progress counts and resumption; a single document-level completion state is simpler. For a short document, the user might also inspect the whole text without elaborate progress reporting. The group discussed these alternatives without fully resolving them.

### Failure handling and performance

The existing tool was described as translating the document asynchronously. For the MVP, the explicit priority was **handling translation failures**, while speed optimization could wait.

This did not establish a detailed background-job architecture or latency target. It established that errors are part of a usable core workflow and that performance tuning should not expand the first release unnecessarily.

### Formatting and publication

The input was discussed as raw text. Detailed page layout, pagination, and publication formatting were associated with the publisher's workflow, which had been removed from the initial scope.

The MVP therefore did not need to reproduce the prototype's full publication-oriented experience. The eventual product could still grow toward reviewed, consistently formatted, publishable output.

## 8. Measuring whether the MVP works

The session identified a need to write down both the MVP goal and its success criteria. Without those, feature discussions easily become subjective or expand the scope.

One illustrative experiment was to translate and review a 30-page book in 30 minutes. If it took 45 minutes, the next step would be to find which stage consumed the extra time, improve it, and test again. **These numbers were examples, not adopted acceptance criteria.**

The discussion suggested examining total completion time, the time spent in different stages, the amount of correction required, and whether users find the experience useful and practical. No formal quality threshold or measurement framework was agreed.

The underlying lesson was to make the MVP small enough to test and specific enough that the results guide the next iteration.

## 9. Delivery platform and technology choices

The group favored a **web application**, primarily for accessibility and easier use across machines and platforms. This supported the eventual aim of people working from different locations without distributing and maintaining separate desktop installations.

The technology discussion included React, ASP.NET Core/C#, Blazor, TypeScript, and Node.js. The transcript does not preserve the complete on-screen comparison reliably, and no final stack selection was recorded.

Several considerations emerged:

- The existing prototype contains C# logic that might be reusable.
- Reusing that logic is a legitimate consideration for migration, but it should not automatically determine the best design for a fresh implementation.
- Frontend choices should support the actual editing experience, keyboard interaction, and review workflow.
- Consistency of language across the application can make a system easier to understand, but that benefit must be weighed against alternatives.
- Budget, hosting, future scale, and maintainability should influence the comparison.
- Choosing a temporary stack and replacing it later can create migration and feature-parity work once users depend on the product.

The group challenged the idea of accepting a technology recommendation simply because AI proposed it. Participants suggested asking why one option fits, why another was rejected, and whether the recommendation is biased by the existing repository.

There was also a discussion about sequencing: clarify the product and its requirements before committing to detailed architecture. The session ended with architecture planned for a later meeting.

## 10. How to use AI during product development

AI was treated as a useful partner for exploring options, drafting documents, understanding code, and implementing changes. However, the discussion emphasized that business context must be supplied explicitly.

An AI assistant does not automatically know a stakeholder's publishing plans, a team's informal conversations, or which compromises are acceptable. When requirements are missing, it may supply plausible assumptions that do not match the actual product.

The recommended working habits were to:

- Give AI the product objective, users, constraints, and future direction.
- Ask it to interview the product owner and uncover missing requirements before drafting a PRD.
- Request comparisons and explanations of trade-offs rather than a single unexplained recommendation.
- Challenge assumptions and apparent bias toward familiar technologies or the existing codebase.
- Iterate on requirements and designs before asking for substantial implementation.
- Review the resulting system and keep its organization understandable as it grows.

Participants stressed the cost of poor early decisions: specification and design problems can spread through the implementation and become expensive to fix. Spending meaningful time reviewing the product with peers, experienced engineers, or architects was presented as worthwhile.

Rapid generation of a small internal tool or dashboard can be useful. For a substantial business application, the emphasis was on deliberate development and maintaining control. AI can generate more code than a person can comfortably understand; part of the learning exercise is managing that complexity.

The discussion of specific assistants did not establish a required tool. The broader point was that the user's understanding, instructions, and interaction with AI strongly influence the result.

## 11. The PRD and preparation for the next session

The next substantial artifact was a **product requirements document (PRD)**, followed by UX flows and wireframes. The preceding discussion—problem, personas, use cases, scope, and success criteria—was intended to provide its foundation.

Participants were encouraged to use the session transcript and AI to produce a draft, then ask follow-up questions to discover gaps and ensure a shared understanding. A PRD should explain what the product must accomplish; it should not be confused with a detailed execution plan.

The requested preparation was to:

1. Write an extended PRD for the translation review application, using the discussion as input.
2. Clarify uncertainties through an AI-assisted interview or further requirements exploration.
3. Develop UX flows and wireframes for the relevant use cases.
4. Explore a suitable interface rather than mechanically copying the desktop prototype.
5. Bring the work to the next session for comparison and an architecture discussion.

The intended learning arrangement was for each participant to build their own implementation of the same core project while sharing brainstorming and discussion. This gives each person experience with the whole product rather than only a narrow assignment within a large team.

The exercise was intended to move participants through several responsibilities: product definition, architecture, planning, and implementation, with AI supporting the work throughout.

## 12. Consolidated status and unresolved questions

| Topic | Position emerging from the session |
| --- | --- |
| Product purpose | Help humans review and correct machine-translated material. |
| Initial delivery | A web application. |
| MVP persona | One user primarily acting as reviewer, with enough authority to complete the workflow. |
| Initial workload | One small book or article; exact size limit unresolved. |
| Review unit | Paragraphs were the working choice. |
| Essential capabilities | Source upload, translation integration, aligned reading, in-place editing, persistence, and error handling. |
| Review completion | Needed further definition at paragraph and document levels. |
| Change tracking | Preserve useful distinctions between draft and edits while simplifying the initial interface; exact scope unresolved. |
| Multiple books and roles | Part of the larger vision rather than the initial experiment. |
| Publication formatting | Outside the initial reviewer-focused scope. |
| Technology stack | Discussed but not selected. |
| Success criteria | Must be made explicit; illustrative targets were not final requirements. |
| Next work | PRD, clarification of gaps, UX flows, and wireframes, followed by architecture. |

Before implementation, the PRD still needs to resolve the accepted source format, exact document-size limit, completion rules, minimum change-history behavior, failure-recovery experience, and measurable success criteria. These are open details identified by consolidating the discussion, rather than additional decisions made during the session.

## Recording reference points

| Approximate time | Topic |
| --- | --- |
| 00:00–00:12 | Product background, desktop demonstration, review efficiency, and longer-term vision. |
| 00:12–00:23 | Why business applications need discovery; domain-specific review; user personas. |
| 00:23–00:35 | Role responsibilities, MVP purpose, scope reduction, and one-user workflow. |
| 00:35–00:50 | Use cases, paragraph-based review, editing, persistence, failures, and completion. |
| 00:50–01:03 | Web delivery, technology alternatives, trade-offs, and evaluating AI recommendations. |
| 01:03–01:09 | PRD, UX preparation, and individual implementations. |
| 01:09–01:17 | Maintaining control of AI-assisted development and keeping the long-term product in view. |
