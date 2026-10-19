# Week 6: Architecture — Data, APIs, and Trade-offs

Turn Week 5's translation-review PRD and UX into a defensible architecture proposal. Trace client/server boundaries, resource APIs, permissions, durable storage, data representation, ordering, and consistency before planning execution.

- [Talk outline and classroom agenda](talk-outline.md)
- [Presentation and speaker notes](presentation.md)
- [Organized session write-up](session-notes.md)
- [Full unedited automated transcript](transcript.txt)
- [Architecture prompts and design templates](reference.md)
- [Homework and assessment](homework.md)

## What you will produce

A database-technology comparison and defended choice, an explicit schema, and book/paragraph API routes connected to the reviewed product requirements. This is a design assignment. Service internals, an execution backlog, application code, CI, migrations, and deployed infrastructure are not required this week.

The case study continues from [Week 5](../05/README.md): one reviewer and one small document, with future crowdsourced review as context. Each student prepares their own proposal. The recording discusses alternatives rather than selecting a universal class stack or database.

## Delivery and preparation

The slides support a 90-minute lesson reflecting the roughly 91-minute recording. The outline expands this into the course's 180-minute workshop with a 45-minute guided design and 45-minute student exercise. Timings and prompts are prepared curriculum, not evidence of a rehearsed implementation.

Bring your Week 5 PRD and wireframes, an AI assistant, and a document/sketch editor. No new service account, cloud subscription, paid database, or API key is required. Use synthetic examples to trace the design.

## Source and technical clarity

The full transcript preserves the automated recognition output, including chatter, repetition, and recognition errors. It is an unedited reference, not a certified verbatim transcript. The session write-up removes incidental material and labels unresolved proposals. The reference clarifies technical shorthand using official documentation, including SQL ordering and transaction guarantees.

Student PDF/export designs, token lifetimes, role examples, storage availability figures, and named-service anecdotes are not adopted product requirements. Scaling and failover are concepts to reason about, not a requirement to implement distributed infrastructure for the MVP.

## Next stage

Bring the schema and contracts to the implementation handoff. Later containerization and deployment work requires a real runnable baseline and checks; this week's design documents do not establish those results. The course handoff records that prerequisite explicitly.

Preparation decisions and verification are recorded in [DEC-030](../../../DECISIONS.md#dec-030-week-6-architecture-and-raw-session-references).
