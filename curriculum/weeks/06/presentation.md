---
marp: true
theme: default
paginate: true
size: 16:9
footer: 'CAP · Week 6'
style: |
  section { background: #F4F0E7; color: #152536; font-family: "Avenir Next", Avenir, Arial, sans-serif; font-size: 32px; line-height: 1.3; padding: 48px 68px 65px; justify-content: flex-start; }
  h1 { font-size: 52px; line-height: 1.08; margin: 0 0 28px; color: #152536; }
  h2, strong { color: #188D91; }
  p, li { margin-bottom: 18px; }
  footer { left: 68px; bottom: 24px; font-size: 16px; color: #647276; }
  section::after { color: #647276; bottom: 22px; right: 45px; font-size: 16px; }
  section.cover { padding-top: 100px; }
---
# From a PRD<br>to an architecture

**Where does the work happen?<br>What data and contracts make it possible?**

CAP · Week 6 · Architecture, data, and APIs

<!-- 0–2m. Week 5 produced product and UX proposals. Today reason about architecture; the closing assignment is database choice, schema, and explicit resource routes. -->

---

# Show the product you understood

Review a PRD and its wireframes.

Parallel text. Paragraph review.<br>Change highlights. Failure feedback.

**A student proposal is not a class decision.**

<!-- 2–8m. The recording opens with student work. PDF input and disabling export until review completes were student choices, not universal product requirements. -->

---

# Read before you accept

Ask AI to restate the user, goal, and scope.

Correct the understanding before expanding it.<br>Iterate on concise points, then formalize.

**A generated PRD still needs human agreement.**

<!-- 8–15m. A student corrected the assistant from translation-engine focus to review-tool focus. Do not equate document length with reviewed understanding. -->

---

# Trace one real operation

Open a paragraph → Edit the target → Save<br>→ Read the saved correction later

**Which component owns each step?**

<!-- 15–18m. Use the actual reviewer job. This is a design trace, not an observed running implementation. -->

---

# Client and server responsibilities

Client interaction · Shared business rules<br>Compute · Updates · Secrets and permissions

**Choose the boundary deliberately.**

<!-- 18–22m. Compare thin and thick clients. No universal rule says all behavior must be in one tier. Client capabilities vary; centralized shared logic can serve other clients later. -->

---

# Design for another client

Web now. Perhaps mobile later.

Which behavior could another client reuse?<br>Which interaction belongs to this interface?

**Future direction informs today’s boundary.**

<!-- 22–25m. The session discussed iOS/Android duplication and code-sharing tradeoffs. No mobile client is required in the MVP. -->

---

# Give resources an identity

A book has an ID.<br>A paragraph belongs to a book.

`/books/{bookId}/paragraphs/{paragraphId}`

**An illustrative route, not a finalized API.**

<!-- 25–28m. Resource identity must distinguish paragraphs in different books. Route naming and behavior are homework design choices. -->

---

# Describe the operation

GET: retrieve · POST: create/process<br>PUT: replace · PATCH: partial change

Specify fields, results, validation, and failures.

**Method names alone are not a contract.**

<!-- 28–31m. These are introductory semantics, not a complete HTTP reference. Explain chosen update behavior. The source also mentioned DELETE and batching; batch grouping is not a universal HTTP verb. -->

---

# Who may act on this book?

Authentication: identify the caller.

Authorization: permit this operation<br>on this particular resource.

**The backend must enforce the rule.**

<!-- 31–35m. Logical middleware does not automatically mean a separate server. Token lifetime and read-only reviewer were examples, not final policy. Token decoding/expiry alone is not a complete validation algorithm. -->

---

# What must survive?

Original material. Translation results.<br>Reviewer corrections. Document metadata.

**Name the durable source of truth.**

<!-- 35–39m. The generated translation was described as expensive work worth preserving. Do not rely on a replaceable process or sole device copy for durable shared state. -->

---

# Choose storage from the workload

Database records · Uploaded objects · Cache

What is read together? Updated together?<br>What happens if a process disappears?

**Long text alone does not choose a database.**

<!-- 39–42m. Relational databases can store long text. The reference links PostgreSQL documentation. No universal blob SLA or mandatory external paragraph-content storage was agreed. -->

---

# Offline editing is another feature

Download. Edit while disconnected. Synchronize.

Who wins when changes conflict?<br>Which copy is authoritative?

**A cache is not a complete sync system.**

<!-- 42–45m. Offline travel use case was raised and generally deferred from MVP. Consider shared access, compatibility and lost devices without adding the feature automatically. -->

---

# Scale up. Scale out.

Vertical: more resources on one machine.

Horizontal: distribute work across machines.

**Distribution adds decisions.**

<!-- 45–49m. Explain concepts without equating relational with vertical-only or document systems with automatic scale. No multi-server build is assigned. -->

---

# Which partition owns the data?

Choose a key and route the request.

What if one partition gets most traffic?<br>What if its machine fails?

**Average capacity can hide a hotspot.**

<!-- 49–53m. Alphabet ranges were a teaching example. Discuss skew, splitting/rebalancing and replicas. These are conceptual concerns, not an endorsed partition scheme for this product. -->

---

# Requests and data move differently

An interchangeable app server can handle a request.

Persisted data still has to be reachable.

**Load balancing is not data partitioning.**

<!-- 53–57m. Clarify the classroom question about routing every request to an empty server. Managed storage can separate concerns; do not assert every database stores data identically. -->

---

# Related data can become expensive

Read several partitions. Gather the results.<br>Join records stored in different places.

**Access patterns guide the model.**

<!-- 57–60m. Explain scatter/gather and cross-partition joins. Replicas, partitions and backups address different problems. -->

---

# Find the remaining single point

Redundant workers can still depend<br>on one unprotected routing component.

**Name the failure and the recovery behavior.**

<!-- 60–65m. Managed scaling, active/active and active/passive were introduced. Availability is configuration-specific. No failover was demonstrated or tested. -->

---

# Understand before generating infrastructure

AI can draft infrastructure as code.

Your design still supplies scope,<br>permissions, durability, and cost assumptions.

**A generated file is not a verified deployment.**

<!-- 65–68m. Terraform was an example. Infrastructure generation/deployment is not the closing assignment. Avoid cross-provider portability promises. -->

---

# One book. Many paragraphs.

Relational: records and references.

Document: embedded or referenced data.

**Compare the actual reads and writes.**

<!-- 68–71m. Label primary keys and book reference. Repeated metadata may simplify reads but creates update tradeoffs. Storage choice and exact schema remain proposals. -->

---

# Identity is not reading order

A paragraph ID identifies the passage.
A position describes its place in the book.

**Order the result explicitly.**

<!-- 71–74m. Clarification: primary keys do not guarantee result ordering. Use explicit ORDER BY for a required sequence. Consider insertion/reordering; do not mandate an unexamined identifier scheme. -->

---

# What does “saved” promise?

After a correction, what does the next read show?<br>What happens with two reviewers’ changes?

**Consistency is part of the user experience.**

<!-- 74–78m. ACID means atomicity, consistency, isolation, durability. Availability is separate. NoSQL does not uniformly lack transactions or strong consistency; consult the exact service/configuration. -->

---

# Migration is more than changing code

Move the data. Preserve its meaning.<br>Check behavior before and after.

**AI assistance does not remove the obligation.**

<!-- 78–83m. The group discussed generated migration tools and AI usage cost. Named services, free tiers and company-history anecdotes are not validated recommendations. -->

---

# Keep future structure visible

MVP: a chapter heading can be a paragraph.

Optional metadata may help future structure.<br>Deeper book-format investigation comes later.

**Future crowdsourcing informs the design.**

<!-- 83–87m. Metadata was a proposal, not final schema. Do not require a universal ebook parser or hundreds of servers. Detailed work items were explicitly premature. -->

---

# Your next design review

**Compare databases. Defend one.**
Write the schema and book/paragraph routes.

Trace each choice back to the PRD.<br>Bring uncertainties and alternatives.

<!-- 87–90m. The homework supplies paths and assessment. No service implementation, migration, CI run, infrastructure deployment or execution backlog is required. See full transcript and organized notes for source distinctions. -->
