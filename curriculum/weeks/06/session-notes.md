# Session 06 — From Product Requirements to Architecture and Execution

*Organized account of the October 16 recording. Incidental conversation and repetition are removed here; the [full automated transcript](transcript.txt) is preserved separately without editorial cleanup. These notes distinguish examples, proposals, and working directions. Recognition errors make some names and technical phrasing uncertain, so unclear details are not reconstructed as facts.*

## 1. The session's purpose

Week 5 defined the translation review product through users, jobs, MVP scope, a PRD, and UX. Week 6 began with student work from that assignment, then moved toward architecture, execution, and work items. The central engineering task was to connect what the reviewer needs to the components, interfaces, and data that make that experience possible.

The case study remained a web application for reviewing machine-translated text. Machine translation supplies the initial draft; the product's main value is helping a human compare, correct, and complete the review.

## 2. What the student walkthrough taught

A student demonstrated a proposed flow from upload through translation, paragraph review, and export. The design placed Arabic and English alongside one another and explored change highlighting, paragraph completion checks, translation failure states, and identifying paragraphs by their opening words.

The student's proposal disabled export until the required review checks were complete. This was a design choice in that presentation, not a final class-wide approval policy.

PDF was also the student's initial input choice. The assistant raised the complication of scanned Arabic material, showing how a seemingly simple format choice can expose extraction requirements. The session did not settle a universal PDF/OCR implementation.

Another assistant assumption treated all uploaded material as Bible text. The student corrected that assumption: the workflow needed to handle the intended source material rather than substitute a narrower category inferred by the model. This illustrated why product context and explicit scope matter.

## 3. Review AI-generated product documents

The group returned to personas, priorities, and jobs-to-be-done as a way to work backward from user success. Without that discipline, a team can keep adding technically interesting features indefinitely.

The instructor highlighted the growing burden of reviewing generated material. AI can produce a long PRD faster than a person can evaluate it. A suggested approach was to iterate on concise points until the intent is clear, then generate the more formal document. The PRD still matters as a human agreement about the product, rather than merely as input for a model.

A student described asking the assistant to restate its understanding before proceeding. This exposed a misunderstanding: the assistant was concentrating on building a translation engine rather than the review experience. Correcting the restatement helped align the design with the actual goal.

The MVP discussion also distinguished limited internal testing or a closely supported private preview from a fully self-service product. The intended release context affects the scope and assumptions that should be made explicit.

## 4. Frontend and backend responsibilities

The architecture discussion started with a web application calling a backend. The group compared a thin client, with more behavior on the server, and a thick client, with more behavior in the application running on the user's device.

The trade-offs discussed included:

- **Compute:** Server resources can be selected and managed centrally; client capabilities vary.
- **Updates:** Central backend behavior and distributed client behavior have different rollout considerations.
- **Security:** The design must consider what is exposed to or controlled by the client.
- **Multiple clients:** Placing shared business logic in each web, iOS, or Android implementation can create duplication.
- **User experience:** Platform-specific behavior and code-sharing choices require trade-offs.

The practical question was where each responsibility belongs. For example, paragraph splitting and the durable identity of a paragraph affect how review work is saved and retrieved. They should be considered deliberately rather than assigned to a component by accident.

The discussion did not produce a blanket rule that all logic belongs in one tier or that every shared-client framework produces the same user experience.

## 5. APIs as resource contracts

The group used books and paragraphs to make frontend/backend communication concrete. A paragraph needs enough context to identify the book it belongs to, rather than relying on a paragraph number that may repeat in many documents.

A resource path of the form `books/{bookId}/paragraphs/{paragraphId}` illustrated that relationship. The session discussed reading a paragraph, creating a book and obtaining its ID, adding paragraphs, and updating paragraph text or book metadata.

GET, POST, PUT, and DELETE were introduced through retrieval, creation, update, and removal examples; PATCH was also raised. Request bodies and JSON were discussed as ways to pass data. The API examples were a starting point for contract design, not a finished endpoint specification or a claim that these are the only HTTP methods.

Batching was discussed separately as grouping operations, rather than as an additional universal HTTP verb. Alternative API approaches were mentioned without being developed into a selected design.

The underlying lesson was to define the interface between components so both sides agree about resources, identifiers, inputs, and results.

## 6. Authentication and authorization boundaries

The group placed an authentication/authorization responsibility in the backend. A logical component on an architecture diagram does not automatically imply a separately deployed server.

The discussion included sign-in, delegation to an external identity provider, access tokens, refresh tokens, and checking access before serving a request. A 30-minute token lifetime was an example, not an agreed policy or a universal requirement.

Two distinct access questions emerged:

1. Is this caller allowed to perform this kind of operation?
2. Is this caller allowed to operate on this particular book or resource?

The session's hypothetical role examples were not a final permission matrix. In particular, a reviewer who is expected to edit translations must have an appropriate save/update path; the example of a read-only reviewer should not silently become the product's policy.

The token discussion was conceptual. It did not establish a complete authentication protocol, secure token-validation algorithm, or tested authorization implementation.

## 7. Durable storage and the cost of losing work

The group asked where uploaded material, machine translation results, and review changes should live. Storing valuable state only with a replaceable application instance or on a single user's machine can complicate recovery and access from other locations.

The translation operation was treated as expensive work worth preserving. Losing its result should not be an incidental consequence of a device failing or an application instance being replaced.

Storage categories discussed included databases, object/blob storage, and cloud-managed file-like arrangements. Metadata and larger text/file content may have different access patterns and storage requirements. The session explored these alternatives without fixing a complete storage design.

Some comments in the raw transcript simplify database and storage capabilities. These notes do not treat a quoted availability percentage as a service guarantee, or assume that relational databases cannot store substantial text. Product access patterns, consistency needs, cost, and operational requirements still need to guide the choice.

## 8. Offline work and caching

A participant proposed downloading work, editing while disconnected, and synchronizing later. The discussion recognized the use case while questioning whether it belonged in the initial MVP.

Offline work raises additional concerns: shared access, compatibility with older saved data, where the authoritative state lives, and how changes return to the server. The proposed feature was generally treated as outside the initial one-reviewer experiment.

Caching was introduced as a related concept at several layers: client, web-facing tier, or backend. A local cached copy and a complete offline-editing synchronization system are not equivalent features. The session did not define conflict resolution or a synchronization protocol.

## 9. Scaling: larger machines versus more machines

The group contrasted vertical scaling—adding resources to an individual machine—with horizontal scaling—distributing work or data across machines.

An alphabetic user partition was used as a teaching example. One server might own one range and another server a different range. This immediately raised engineering questions:

- How does a request find the partition that owns its data?
- What happens when one range receives much more traffic than the others?
- How is data redistributed when partitions become uneven?
- What happens when a machine or partition is unavailable?
- How are compute and storage scaled, and are they coupled?

Routing by a partition key or hash, adding replicas, splitting overloaded partitions, and moving work were discussed as responses to these problems. They illustrate distributed-system concerns; they were not instructions to hand-build a distributed database for the translation MVP.

## 10. Request balancing is not data partitioning

A participant asked why every request could not simply go to whichever server had capacity. The discussion distinguished requests that can be handled by interchangeable application servers from requests that must reach the machine holding particular data.

If a data partition exists only in one place, routing a request elsewhere does not make that data appear there. The group then explored the cost of spreading related data across machines: a query may need to contact several partitions, gather their results, or combine records that are not colocated.

Scatter/gather and distributed joins made the trade-off concrete. Easier distribution can make some queries and relationships more complex. Choosing the data layout therefore requires thinking about how the product reads and changes that data.

Replication and failover were discussed as ways to improve availability. Neither replication nor managed hosting removes the need to understand consistency, failure behavior, and recovery.

## 11. Managed infrastructure and infrastructure as code

The group asked whether an application team must implement all the routing, scaling, and coordination just described. The answer was to understand those concerns well enough to choose and configure appropriate services, rather than recreate every underlying mechanism.

Managed scaling arrangements, including VM scale sets, were mentioned as examples. The discussion also identified single points of failure: adding redundant workers does not solve availability if every request still depends on one unprotected routing component. Active/active and active/passive arrangements were introduced conceptually.

Infrastructure as code was described as a declarative way to capture infrastructure configuration, with Terraform among the examples. AI can help produce such definitions from a sufficiently clear design. Generated configuration still needs review; generating a file is not evidence of a valid deployment, tested failover, or a provider-independent implementation.

No cloud infrastructure was established or verified by the recording as transcribed, and the session did not settle a region, provider, topology, availability target, or cost estimate.

## 12. Relational and document representations

The data-model example used a book with many paragraphs and each paragraph belonging to one book. In a relational representation, the group discussed book and paragraph primary keys, book metadata, and a book reference on the paragraph. Paragraph content or a reference to separately stored content was considered.

The document alternative put related fields into JSON-like records. Paragraphs could be embedded with a book, referenced separately, or carry repeated metadata. Those choices affect how much data is retrieved, how many reads are needed, and how updates are coordinated. The desired API operations help determine which representation is useful.

The examples were exploratory. They did not settle the full schema, require every paragraph's text to live in blob storage, or choose a relational or document product.

### Identity and order are separate concerns

The group debated whether paragraph IDs could also express document order. Some IDs are generated sequentially; others are not. The safe product requirement is that paragraphs appear in the intended reading order, regardless of the identifier mechanism chosen.

For teaching clarity, a stable paragraph identity and an explicit document position should be considered separately. Database results should be explicitly ordered when order matters; a primary key does not by itself guarantee a query's presentation order. This is a clarification of the recorded discussion, not a finalized class schema.

## 13. Consistency changes the user experience

The session connected storage behavior to features such as seeing a recent edit, reading a comment after it is written, and applying later reviews to the correct earlier version. If replicas or independently handled writes are not immediately reflected in reads, a user can see stale or unexpectedly ordered information.

The discussion introduced eventual consistency and read-after-write behavior. Its key product implication was that the storage choice can affect what the user experiences; the design must specify the behavior the review workflow requires.

Several statements in the transcript are oversimplified. **ACID means atomicity, consistency, isolation, and durability.** It does not expand to availability. Relational and non-relational labels do not, by themselves, determine every transaction, consistency, or scaling guarantee. Those guarantees must be checked for the selected service and configuration.

Similarly, anecdotal references to other companies' early designs or messaging behavior are not treated here as verified explanations of those systems. They served the classroom discussion, but they are not evidence for choosing this application's architecture. See the [reference](reference.md) for verified clarifications and design-review prompts.

## 14. AI assistance does not remove migration or cost decisions

Near the end, the group considered changing database technologies with AI assistance. That involves both changing application code and moving existing data. Once real customer data exists, preserving it is part of the problem; regenerating an application does not automatically solve that obligation.

A generated migration tool was suggested as one possible approach. The discussion also returned to token/usage limits and the cost of repeatedly asking AI to rebuild large parts of a system. Clear requirements, explicit technology choices, and small incremental requests can reduce avoidable rework.

Named database services came up as examples of what an assistant might recommend. Neither a recommendation's popularity nor a free-tier suggestion establishes that a service fits the product. Current service terms, prices, acquisitions, and popularity claims are not evaluated or endorsed by this write-up.

## 15. The closing assignment: choose, model, and defend

The group explicitly observed that it had discussed architecture choices and compromises, rather than finished the architecture. Detailed work-item decomposition was still premature.

The requested next work was to:

1. Research plausible database technologies and explain why the selected option suits the application better than alternatives.
2. Write down the database schema in detail.
3. Define the book and paragraph API routes explicitly and connect them to the schema.
4. Use AI to explore and explain the choices, while being able to defend the design personally.

The assignment did **not** require implementing the internal services, deploying infrastructure, building the distributed-system examples, or producing a finished execution backlog. The [homework](homework.md) turns this closing direction into a reviewable course submission; its file paths, budget breakdown, and assessment table are curriculum preparation rather than quotations from the recording.

## 16. Chapters, future structure, and crowdsourcing

A late question asked how chapters and more complex book structures fit a book-and-paragraph model. The MVP continued to avoid full formatting: a chapter heading could be represented as a short paragraph.

Attaching metadata to paragraphs was proposed as an extensibility option—for example, to distinguish headings or levels without abandoning a sequential passage model. It was not adopted as a finished schema or a promise to parse every ebook format.

A deeper investigation of book structure was placed in the post-MVP discussion. The group also reaffirmed the eventual crowdsourcing setting, where multiple people may work on the same material. That future helps explain the architecture questions, but does not require hundreds of servers or every role in the initial release.

## 17. Consolidated status

| Topic | Position at the end of the session |
| --- | --- |
| Product focus | Human review of a machine-translated draft. |
| Student UX walkthrough | Useful proposals, including diffs and export gating; not a finalized class policy. |
| Architecture | Frontend, backend/API, identity/access, and durable storage discussed; complete design still to be proposed. |
| Resource model | Books and paragraphs were the working examples; exact contracts and schema are the assignment. |
| Storage technology | Alternatives explored; students should select and defend an option. |
| Paragraph order | Important requirement; identifier and ordering strategy still need explicit design. |
| Offline editing | Generally deferred from the MVP. |
| Scaling and redundancy | Concepts to understand and use when selecting services; not a requirement to implement a distributed platform. |
| Infrastructure as code | Introduced conceptually; no verified deployment. |
| Book/chapter structure | Minimal treatment for the MVP; deeper exploration post-MVP, metadata an option. |
| Execution backlog | Detailed work items deferred until design is clearer. |
| Next deliverables | Database choice and rationale, explicit schema, and book/paragraph API routes. |

## Recording reference points

| Approximate time | Topic |
| --- | --- |
| 00:00–00:10 | Student PRD and wireframe walkthrough. |
| 00:10–00:16 | Personas/MVP recap, reviewing generated documents, checking AI understanding. |
| 00:16–00:25 | Thin/thick clients, backend responsibilities, identity/access boundary. |
| 00:25–00:34 | Resource APIs, identifiers, token and permission examples. |
| 00:34–00:47 | Persistence, storage alternatives, device-local work, caching/offline scope. |
| 00:47–01:00 | Scaling, partitioning, skew, replicas, routing, and distributed queries. |
| 01:00–01:08 | Managed infrastructure, single points of failure, infrastructure as code. |
| 01:08–01:20 | Relational/document models, paragraph order, and consistency. |
| 01:20–01:24 | Migration, AI usage costs, and questioning technology recommendations. |
| 01:24–01:31 | Schema/API assignment, chapters and metadata, future crowdsourcing. |
