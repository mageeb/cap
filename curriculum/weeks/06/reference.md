# Week 6 Reference: From Product Requirements to Architecture

Use this companion to turn the Week 5 PRD and wireframes into an explainable design. These prompts and templates are teaching aids derived from the October 16 discussion, not a claim that the class finalized every answer. See [session notes](session-notes.md) for the organized account and the [full transcript](transcript.txt) for the unedited source.

## 1. Make the assistant expose its understanding

```text
Read this PRD and UX flow. Before proposing technology, explain the user,
core job, MVP boundary, and uncertainties in concise bullets.
Distinguish a translation-review product from an LLM translation engine.
List assumptions separately. Ask questions where the source does not decide.
Do not invent settled requirements or implement anything yet.
```

Review the bullets before requesting a long specification. A readable document that humans have checked is more useful than an impressive unreviewed PRD.

## 2. Trace one operation through the system

```text
Trace one reviewer opening a document, reading a paragraph, editing its
translation, saving, and returning later. For each step identify:
- client responsibility and server responsibility;
- request/response boundary and resource identifiers;
- authentication and permission checks;
- durable data read or written;
- failure and recovery behavior.
Keep the MVP bounded. Label missing requirements and proposed choices.
```

| Question | Design record |
|---|---|
| What must the client render or manage? | Screens, local interaction state, unsaved-change behavior |
| What belongs behind the backend boundary? | Shared rules, secrets, access checks, durable writes |
| What might another client reuse later? | Resource contracts and server-side operations |
| What happens if a process or device disappears? | What survives, what can be repeated, what the user sees |
| What is deferred? | Offline synchronization, extra personas, and scale features unless justified by the PRD |

Thin and thick clients describe a distribution of responsibilities. Explain the tradeoff for this product rather than declaring one universally best.

## 3. Sketch a resource contract

The recording uses books and paragraphs as resource examples. The routes below are illustrative proposals; adapt terminology to the reviewed PRD.

| Operation | Illustrative request | Questions to settle |
|---|---|---|
| Create document metadata | `POST /books` | Accepted metadata, generated ID, who may upload |
| Add a paragraph | `POST /books/{bookId}/paragraphs` | Source/translation fields, ordering, repeated submission |
| Read a paragraph | `GET /books/{bookId}/paragraphs/{paragraphId}` | Missing resource, permitted reader, review state |
| Save a correction | `PUT /books/{bookId}/paragraphs/{paragraphId}` | Full replacement versus partial update, conflicting edits, permitted editor |

```text
Draft a small API contract for this workflow. State request and response
fields, identifiers, permissions, validation, and expected failures.
Explain whether PUT or PATCH fits the chosen update semantics.
Do not present these example routes as the only valid design.
```

HTTP supports more than the four methods emphasized during the discussion. Batch processing groups operations; it is not a universal additional HTTP method. Choose operation semantics explicitly.

## 4. Separate identity from permission

Authentication establishes who is calling. Authorization decides whether that caller may perform this operation on this particular resource. Place enforcement behind the client boundary; hiding a button does not establish permission.

| Actor | Resource | Read | Edit | Upload | Reason and unresolved questions |
|---|---|---|---|---|---|
| Reviewer | Assigned document | Proposed | Proposed | Undecided | Define the reviewed workflow first |
| Reviewer | Someone else's document | Undecided | Undecided | Undecided | Requires a sharing/assignment rule |
| Administrator | Managed documents | Undecided | Undecided | Undecided | Do not infer unlimited rights from the role name |

The session's read-only reviewer example illustrates permission design; it does not settle the product's final role matrix. The sample access-token lifetime is also illustrative. Select identity-provider configuration and validated middleware when implementing, rather than treating token decoding or an expiry check alone as authentication.

## 5. Choose storage from data and access patterns

```text
List original uploads, extracted paragraphs, generated translations,
review changes, document metadata, users, and assignments.
For each state its size assumptions, read/write pattern, relationships,
retention, source of truth, and what must survive process/device failure.
Compare the simplest plausible relational/document/object-storage choices.
Explain tradeoffs without claiming SQL cannot store long text or cannot scale.
```

| Data kind | Candidate to evaluate | Questions |
|---|---|---|
| Original uploaded file | Durable object storage | Retrieval, ownership, retention, recovery |
| Document/paragraph/review records | Relational or document database | Relationships, queries, consistency, update shape |
| Temporary derived results | Cache or reproducible workspace | Can loss be tolerated? How is it rebuilt? |
| User-device copy | Optional cache/offline feature | Conflicts, synchronization, lost device, shared access |

Technical clarification: a relational database can store long text. PostgreSQL provides `text` and manages long values internally. Text length alone does not require a wide-column database or object storage. Assess the actual queries, transactions, size, costs, and operational requirements. See [PostgreSQL character types](https://www.postgresql.org/docs/current/datatype-character.html).

Durable object storage and an individual server's local filesystem have different operational characteristics. Do not assign a universal availability percentage to object storage: provider, service, redundancy configuration, and contractual definition matter. A cache is not automatically the durable source of truth. Offline editing adds synchronization and conflict decisions beyond simply caching a page.

## 6. Explain scale without prematurely building it

The class uses vertical scaling, partitions, routing, hot partitions, replicas, and cross-partition queries to reveal why distribution adds complexity. These are concepts to understand, not instructions to build a database platform for the MVP.

```text
For this proposed data model, explain vertical versus horizontal scaling.
Use a small partition-key example. Show how skew can create a hot partition,
what a replica helps with, and why a cross-partition query or join can cost more.
Separate stateless request balancing from routing to persisted data.
State what a managed database already supplies and what remains our decision.
Do not equate SQL with vertical-only or NoSQL with effortless horizontal scale.
```

Ask whether records needed together can be read together, how an ID locates data, what happens if one partition fails, and whether the assumed load justifies any added distribution. Replication, partitioning, and backups solve different problems; name the failure being addressed.

## Architecture review record

- Requirement and user operation being supported:
- Component and data-flow sketch:
- API/resource contract:
- Data ownership and durable source of truth:
- Identity and resource-permission assumptions:
- Chosen option, rejected alternative, and reason:
- Failure/recovery questions:
- MVP work versus deferred features:
- AI assumptions challenged and resulting revision:
- What is proposed, demonstrated, or still unverified:

## 7. Model identity, sequence, and consistency explicitly

The recording contrasts a relational book-to-paragraph relationship with embedded or referenced document models. Use the API access patterns to compare these representations. Embedding related data can simplify a read but may duplicate data or enlarge updates; referencing can require additional reads. Neither representation removes the need to decide paragraph order and review-state behavior.

```text
Sketch a book/document and paragraph model in relational and document forms.
Identify primary keys, references, and one-to-many relationships.
Represent paragraph position separately from identity unless a justified
invariant makes one field serve both. Show a query that explicitly orders
paragraphs. Explain how later insertion or reordering affects the model.
For a save followed by a read, state the consistency guarantee needed.
```

Technical clarifications to the classroom shorthand:

- A primary key identifies a row; it does not guarantee the order of query results. Specify `ORDER BY` for a required paragraph sequence. See [PostgreSQL sorting rows](https://www.postgresql.org/docs/current/queries-order.html).
- ACID means **atomicity, consistency, isolation, and durability**. Availability is a separate concern. Database consistency in ACID concerns valid transaction state; visibility across replicas is a related but different design question.
- NoSQL is a family of systems, not a promise of eventual consistency or the absence of transactions. For example, MongoDB supports multi-document and distributed transactions, with guarantees affected by configured read/write concerns. See [MongoDB transactions](https://www.mongodb.com/docs/manual/core/transactions/).
- Choose guarantees for the exact workflow: after saving a correction, must that reviewer immediately read it back? What should another reviewer observe? What happens when two saves overlap? Do not infer an application's internal consistency design from a social-feed or messaging anecdote.

## 8. Keep infrastructure generation under review

The session introduces load-balancer failure, redundancy, active/active versus active/passive arrangements, managed infrastructure, and declarative infrastructure as code. A component sketch is not a guarantee that failover works. AI can draft configuration only from the requirements and context supplied.

```text
Review this proposed architecture before generating infrastructure code.
List single points of failure, persistent versus replaceable components,
load/region assumptions, and which managed services supply which behavior.
Recommend the smallest MVP topology and label future scale options.
If asked for infrastructure as code, explain its resource scope,
permissions, state, verification, and cleanup before treating it as deployable.
```

For this lesson, explanation and an architecture proposal are sufficient unless the homework explicitly asks for implementation. Generating an infrastructure file does not establish that it was validated or deployed, and changing database technologies still requires a reviewed data migration and behavior checks.

## 9. Finish the architecture exercise at the right boundary

The closing assignment asks students to compare database technologies, defend a choice, fully write the schema, and define book/paragraph API routes. It explicitly stops before service internals; implementation work items are premature at this stage. See [homework](homework.md) for the prepared submission and assessment, and [full transcript](transcript.txt) for the original discussion.

```text
Using my reviewed PRD and UX, compare two plausible database technologies.
Evaluate the MVP operations, relationships, paragraph ordering, edit visibility,
operational burden, migration implications, and cost assumptions.
Recommend one and explain why the alternative is less suitable here.
Then draft the complete proposed schema and book/paragraph API routes.
Label assumptions and open questions. Stop before service internals,
implementation work items, application code, or infrastructure deployment.
Ask me to explain and challenge the recommendation before treating it as settled.
```

For the MVP, the closing discussion treats a chapter heading as a simple paragraph and avoids rich formatting. Optional paragraph metadata is an extensibility proposal, not an agreed universal book schema. Deeper chapter/section/ebook-structure investigation belongs in the post-MVP questions. The future crowdsourcing context should inform the tradeoff explanation without requiring a large distributed deployment now.
