---
marp: true
theme: default
paginate: true
size: 16:9
footer: 'CAP · Week 5'
style: |
  section { background: #F4F0E7; color: #152536; font-family: "Avenir Next", Avenir, Arial, sans-serif; font-size: 32px; line-height: 1.3; padding: 48px 68px 65px; justify-content: flex-start; }
  h1 { font-size: 52px; line-height: 1.08; margin: 0 0 28px; color: #152536; }
  h2, strong { color: #188D91; }
  p, li { margin-bottom: 18px; }
  footer { left: 68px; bottom: 24px; font-size: 16px; color: #647276; }
  section::after { color: #647276; bottom: 22px; right: 45px; font-size: 16px; }
  section.cover { padding-top: 100px; }
---
<!-- _class: cover -->

# From a demo<br>to a product

**Who uses it? What must it do?**

CAP · Week 5 · Product discovery, MVP, PRD, and UX

<!-- 0–2m. Weeks 1–4 taught working with agents. Today define the business work those agents should perform. This deck supports 75 minutes; the outline adds a separate 45-minute demonstration and 60-minute workshop. -->

---

# Start with the real problem

Arabic books need reliable English translations.

A model produces a draft quickly.<br>A qualified human must review its meaning.

**The product makes that review practical.**

<!-- 2–5m. The recording describes religious and theological material. Do not guess unclear authors or book titles. Other language pairs are a future possibility. -->

---

# Walk through one paragraph

Read the source beside the translated draft.

Correct the target text in place.<br>Save the result. Move to the next passage.

**Keep the reviewer oriented.**

<!-- 5–8m. Use the real desktop tool if available; otherwise narrate the workflow and label sketches as illustrations. The source-language text, original machine target, and edited target are three distinct things. -->

---

# Fluent can still be wrong

- “Old man” becomes “old self.”
- Chapter and verse numbers are swapped.
- A quotation uses a different Bible version.

**Domain knowledge changes the review.**

<!-- 8–10m. These examples come from the session. Exact New King James wording was required for the demonstrated material. Do not claim that automated reference lookup proves the whole translation accurate. -->

---

# Improve the human workflow

Verse lookup. Reusable terminology.<br>Shortcuts. Change highlights. Source narration.

The presenter reported roughly **4,000 words/hour**<br>against a **200 words/hour** manual baseline.

**Experience claim, not a validated benchmark.**

<!-- 10–12m. Listening to the source while editing the target was described as a major efficiency improvement. The inconsistent later 40,000 figure is excluded. Demo features are not automatically MVP requirements. -->

---

# Who uses the application?

Reviewer · Administrator · Project manager<br>Publisher · Auditor / adjudicator

**Each role has different jobs.**

A book's author or reader is not automatically an app user.

<!-- 12–18m. Ask participants to describe each role's job. The adjudication role name was not finalized. Product personas are different from the agent responsibilities discussed in Week 4. -->

---

# The vision can be large

Multiple books and reviewers.<br>Resolve disagreements. Publish completed work.

Build trusted, aligned source and translated text<br>for possible future model improvement.

<!-- 18–23m. Explain inter-judge agreement and a gold-standard parallel corpus. No agreement formula, automatic quality guarantee, or fine-tuning pipeline was specified. -->

---

# What will the first experiment prove?

**One reviewer. One small document.**

Can this person correct the draft<br>and finish a useful review?

<!-- 23–27m. Reduce five roles to one user with sufficient authority. Separate publishing an ebook from producing a reviewed translation. The future vision remains useful context. -->

---

# Give the MVP a boundary

Include: aligned reading, editing, saved progress,<br>and translation failure handling.

Defer: separate role workflows, multiple books,<br>adjudication, and publication formatting.

**State why each choice serves the experiment.**

<!-- 27–31m. The session favored this boundary. Detailed change tracking and completion granularity remained debated. No exact page limit was agreed. -->

---

# Measure, then improve

Choose a document limit and a success threshold.

Measure total time, correction effort,<br>and where the workflow breaks down.

**A proposed target is not an observed result.**

<!-- 31–35m. The 30-page/30-minute example illustrated experimentation; it was not adopted. Students must label their own targets and proposals. -->

---

# Break down “review and edit”

Upload → Request translation → Read paragraphs<br>→ Edit → Save and resume → Complete review

**Paragraphs preserve useful context.**

<!-- 35–39m. This is the working flow, not a final architecture. Ask where the translation comes from and whether the request begins automatically or through a button. The API integration still has to be specified. -->

---

# Specify what happens when it fails

Translation fails.<br>The reviewer closes the page.<br>Some passages remain unfinished.

**What does the user see, keep, and do next?**

<!-- 39–43m. Failure handling and persistence were treated as essential. Speed optimization could wait. Define actual recovery behavior rather than assume a particular job queue or storage technology. -->

---

# Turn needs into observable behavior

After a saved edit, leave and return.<br>The corrected paragraph is still present.<br>It remains aligned with its source.

**A check should expose a meaningful failure.**

<!-- 43–47m. This is a proposed acceptance example derived from the persistence need, not a previously executed test. Walk through a paper prototype; record runtime verification as future work. -->

---

# Separate decisions from questions

Working direction: paragraph review in a web app.

Still open: exact input limit, completion rules,<br>change-history behavior, and success thresholds.

**Do not turn a discussion into invented certainty.**

<!-- 47–50m. Have students label historical direction, their own proposals, and unresolved issues. Retaining machine output does not require a full diff interface in the first release. -->

---

# Why a web application?

People can work from different locations<br>and use different machines.

**Delivery choice follows the user workflow.**

<!-- 50–53m. Web delivery was favored in the session. It does not automatically settle hosting, identity, network behavior, or offline support. -->

---

# Compare choices before choosing

Editing experience · Existing C# logic · Team skills<br>Budget · Hosting · Maintenance · Future scale

Ask why one option fits—and why another does not.

**No class technology stack was selected.**

<!-- 53–58m. React, ASP.NET Core, Blazor, TypeScript and Node.js came up. Avoid repeating questionable framework characterizations from the raw transcript. Existing-code reuse is a factor, not an automatic constraint on a fresh design. -->

---

# A small MVP still has a future

Reduce the feature set deliberately.

Changing technology after users depend on it<br>can create migration and feature-parity work.

**Keep the eventual product in view.**

<!-- 58–63m. The trade-off was discussed as case-dependent. Requirements should guide architecture; do not require a production stack decision before the PRD. -->

---

# Ask AI to interview you first

“Find gaps in my objective, users, scope,<br>failure behavior, and success criteria.

Separate evidence from assumptions.<br>Ask questions before drafting or coding.”

<!-- 63–67m. Reusable prompts are in reference.md. An assistant lacks informal stakeholder context unless supplied. Review its questions and answers; do not let it silently resolve business decisions. -->

---

# Write the PRD. Sketch the experience.

Problem → User → MVP goal → Requirements<br>→ Acceptance checks → UX → Open decisions

**A PRD defines the product.<br>A task plan organizes implementation.**

<!-- 67–71m. Include happy, failure, save/resume and completion paths. Annotate screen behavior. Screenshots of mockups are design evidence, not application test results. -->

---

# Your work before next session

Build your own **PRD and annotated wireframes**.<br>Use AI to discover gaps and challenge the scope.

Record one meaningful revision and remaining questions.<br>Bring the artifacts to the architecture discussion.

<!-- 71–75m. See homework.md for submission and the four equally weighted rubric dimensions. No code, chosen stack, deployment, or migration is required this week. Individual artifacts, shared brainstorming. -->
