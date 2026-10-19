# Week 6: Architecture — Data, APIs, and Trade-offs

## Learning goals

Trace the reviewer workflow across components and durable data. Explain client/server responsibility, API resource identity, authentication versus authorization, storage alternatives, partitioning, and consistency. Defend a database choice, schema, and book/paragraph contracts using the PRD rather than an assistant's default preferences.

## Teaching through-line

Week 5 defined who needs the product and what the MVP should prove. Week 6 asks how its behavior maps to interfaces and data. Understanding distributed-system trade-offs helps choose services; it does not mean building every mechanism discussed.

Use the [session notes](session-notes.md) for the organized source, the [full transcript](transcript.txt) for raw reference, and the [reference](reference.md) for prompts and technical clarifications. The [homework](homework.md) captures the closing database/schema/API assignment.

## Delivery formats

The [90-minute presentation](presentation.md) follows the recorded progression. The three-hour workshop adds a 45-minute guided architecture exercise and 45 minutes of individual work and review. These are teaching allocations, not claims that infrastructure was deployed or an implementation completed during the recording.

## Core discussion — 90 minutes

| Time | Topic and instructor move | Evidence to capture |
| --- | --- | --- |
| 0–15 | Review PRDs/wireframes. Ask the assistant to restate its understanding. Separate student proposals from class requirements. | Corrected product summary and unresolved assumptions. |
| 15–25 | Draw client/backend boundaries. Compare compute, updates, multi-client reuse, and security responsibilities. | Component responsibilities and one operation trace. |
| 25–35 | Describe book/paragraph resources, identifiers, reads/writes, identity and resource permissions. | Draft contract and proposed access rules. |
| 35–45 | Place uploads, translations, edits, and metadata in durable storage; distinguish caches and offline synchronization. | Data ownership and MVP exclusions. |
| 45–60 | Explain vertical/horizontal scaling, partition keys, hotspots, replicas, routing, and scatter/gather. | A justified simple MVP and future scaling questions. |
| 60–68 | Find single points of failure; introduce managed services and infrastructure as code. | Configuration assumptions and unverified failure behavior. |
| 68–80 | Compare relational/document representations, identity versus order, and read-after-write requirements. | Schema proposal and consistency requirements. |
| 80–90 | Discuss migration/cost, then assign database comparison, schema, and explicit API routes. Defer service internals and detailed work items. | Defensible homework boundary and open decisions. |

### Source-sensitive teaching notes

- Student PDF input, export gating, and diff screens are examples from a student's design, not a settled class PRD.
- Review the product summary before generating long documents. The product assists translation review; it does not require building a new translation engine.
- Distinguish a logical authentication component from a separate deployed server. A token lifetime is configuration, not a universal number. Role and resource permissions both matter; a reviewer who edits needs a permitted save operation.
- GET/POST/PUT/DELETE were introductory examples, not all HTTP methods. Discuss PATCH if the proposed update is partial. Resource names and routes remain proposals.
- Do not claim relational systems cannot store long text, all SQL is vertical-only, or all NoSQL lacks transactions. Use the verified clarifications in the reference.
- A primary key does not automatically determine query order. Require explicit paragraph ordering and explain any invariant linking order to identity.
- ACID expands to atomicity, consistency, isolation, durability. Availability is separate. Read-after-write visibility and transaction consistency need precise discussion.
- Avoid universal blob-storage availability percentages and unverified claims about other companies' history. Managed services still have configuration-specific behavior and failure modes.
- Optional metadata for headings is a proposal. Full book/chapter parsing belongs to the post-MVP investigation, not silently to the initial schema assignment.

## Guided design exercise — 45 minutes

1. **Agree the product (5m).** Read a short PRD summary and ask AI to restate the reviewer job, MVP boundary, and unknowns. Correct a translation-engine assumption if it appears.
2. **Trace a save (10m).** Draw reviewer → client → backend → durable record → response. Annotate identifiers, access checks, failures, and what survives leaving the page.
3. **Compare representations (10m).** Model one book and three ordered paragraphs in relational and document forms. Explain primary keys, the book reference, position, and the access pattern each representation serves.
4. **Specify contracts (10m).** Draft create/read/update book/paragraph routes with example fields, update semantics, validation, and failures. Label these instructor proposals rather than historical class decisions.
5. **Challenge the choice (10m).** Ask what happens with a missing paragraph, unauthorized book, save followed by read, or two competing corrections. Revise a supported weakness. Record future questions without designing the whole distributed platform.

Use prose, tables, or sketches; do not deploy generated infrastructure. Static examples demonstrate reasoning, not runtime correctness. Database and HTTP details can be checked against the selected service's documentation before implementation.

## Student workshop — 45 minutes

- **15m:** Compare two storage/database options against the PRD and choose a provisional design.
- **15m:** Write the schema and routes, tracing one review/save journey across both.
- **15m:** Peer challenge: ordering, wrong-book access, interrupted save, stale read, and future book structure. Record one revision or a justified rejection of feedback.

## Exit check

Ask: Why this database? How does this paragraph belong to its book? What defines reading order? Who may change it? What should a read immediately after save return? Which future feature is deliberately deferred?

The homework is design evidence, not an implementation demo. The recording explicitly deferred detailed work items and internal service design. Later implementation, testing, and deployment require separate verified results; the next-stage prerequisite must not be described as an outcome of this week.
