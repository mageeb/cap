# Week 6 Homework: Architecture, Data, and API Contracts

## Goal and starting point

Turn your Week 5 PRD and wireframes into a defensible architecture proposal for the translation-review case study. Research database alternatives, explain your choice, fully write the proposed schema, and define the book/paragraph API routes. The [session notes](session-notes.md) summarize the discussion; the [full transcript](transcript.txt) preserves its original wording. Use the [reference prompts and templates](reference.md) to expose assumptions and review AI output.

The recording's closing assignment concerns technology choice, schema, and routes. The submission structure and assessment below adapt that assignment to the course. It does not require service internals, implementation work items, application code, CI, deployed infrastructure, a migration script, or a working translation integration.

Bring your reviewed Week 5 product scope, user flow, and unresolved questions. Keep the reviewer MVP explicit. Use the future crowdsourcing/multiple-reviewer direction as design context without making every future feature part of this submission. A chosen technology and schema are your defended proposal, not a class-wide decision.

## Required deliverables

### 1. Database choice and tradeoffs

Compare at least two plausible technologies or concrete storage approaches, then recommend one for this product. Explain the requirements behind the recommendation: document/paragraph access, relationships, ordering, update visibility, data size assumptions, operational burden, cost factors, and eventual migration implications. Separate original-file storage from editable application records where useful.

State what you know, what you assumed, and what needs validation. Support important technology claims with primary documentation. Do not invent measurements or quote a free tier as a durable requirement. SQL versus NoSQL alone is too broad to settle guarantees; name the actual system and relevant behavior. Explain why your alternative is less suitable for the chosen scope.

### 2. Complete proposed schema

Write the model in full, using a relational table specification, document examples, or both as appropriate to your choice. Include the entities and fields your MVP needs, identifiers, relationships/references, required versus optional values, and relevant constraints. Show a book with multiple paragraphs and explain where source text, translated text, and review state belong. If content lives outside the database, identify its references and ownership.

Distinguish paragraph identity from display sequence. Explain how the model preserves paragraph order and how a query/read retrieves that order explicitly. Show enough sample data to make the model understandable; label illustrative fields and values. Identify the behavior needed when a correction is saved and then read again.

For the exercise's minimal formatting, a chapter heading can remain a paragraph. Record optional metadata and richer chapter/section structure as proposals or future questions rather than silently expanding the MVP.

### 3. Book and paragraph API routes

Fully write the routes used by the reviewed user flow. For each, specify the method and path, identifiers, request fields, response fields, important failure cases, and the identity/permission assumptions. Cover creating a book/document, adding paragraphs, retrieving the required content, and saving a correction where your PRD requires those operations. Explain full versus partial updates if relevant.

Trace one operation from a wireframe through a route to the stored records and back. An API contract can state observable behavior without designing the service's internal classes or algorithms. Role examples in class do not establish a final permission policy: your proposed permissions must support your reviewer workflow and distinguish access to one document from access to every document.

### 4. Human review and revision

Ask AI to restate its understanding before drafting. Correct material assumptions, then challenge the proposed choice, schema, and routes. Record at least one meaningful revision and its reason. If the first draft survives review, document a substantive challenge and the evidence that resolved it instead of inventing a correction.

Be ready to explain the proposal without simply reading AI prose. Mark every deliverable as complete, partial, or unresolved and identify remaining decisions. Keep advanced partitioning, offline synchronization, rich book structure, and implementation planning outside the MVP unless your reviewed requirements justify them.

## Work budget: 5–10 hours

| Activity | Suggested time |
|---|---|
| Reconcile PRD/UX and list assumptions | 30–60 minutes |
| Research and compare database choices | 1–2 hours |
| Write schema, examples, and ordering rules | 1–2 hours |
| Define routes and trace the workflow | 1–2 hours |
| Challenge, revise, and prepare explanation | 1–3 hours |

Prioritize a coherent, explainable proposal over a large speculative design. Record incomplete work truthfully.

## Submission

Submit a repository/PR link containing the technology comparison, schema, API contract, simple component/data-flow sketch, and review record. Include `docs/prompt-logs/week-06.md` using the [shared evidence-log template](../../../infrastructure/template/docs/prompt-logs/TEMPLATE.md); concise excerpts and human decisions are more useful than an unreviewed chat dump. Disclose reused examples and AI assistance. Share before the next session.

A written model, table, or JSON example is sufficient; executable database setup is optional. Clearly label anything implemented, generated, proposed, or unverified. A passing implementation does not replace the required reasoning.

## Assessment

The [shared engineering rubric](../../../tools/agentic-mentor/evaluation-rubric.md) applies, with four dimensions worth 25% each. Assess the document evidence for this week's design task:

| Dimension | Evidence for this assignment |
|---|---|
| Context management | PRD and UX drive the design; MVP, future crowdsourcing, assumptions, and open questions are explicit |
| Validation and correction | Important claims checked against documentation; AI assumptions challenged; meaningful review/revision recorded |
| Structural oversight | Database choice, schema, resource routes, and user flow agree; student explains alternatives and boundaries |
| System integrity | Durable data, identity versus permission, paragraph ordering, and required consistency are addressed without unsupported guarantees |

Missing evidence leaves the affected score provisional. There is no backend coverage threshold, CI gate, deployment requirement, or credit for unverified implementation claims in this assignment.

## Next

Bring the defended choice, schema, routes, and unresolved questions to the next architecture/design discussion. Service internals and implementation work items follow after this boundary is reviewed. If later course exercises need a runnable application, checks, migration history, or access implementation, prepare that baseline explicitly; this assignment does not claim to produce it.
