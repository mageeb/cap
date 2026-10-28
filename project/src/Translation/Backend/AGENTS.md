# Backend instructions

Follow the [project instructions](../../../AGENTS.md). Read [MEMORY.md](MEMORY.md),
the [backend README](README.md), and the relevant feature spec/tasks before editing.

- Keep application logic organized by capability; isolate HTTP, storage and vendor types behind ports.
- Enforce validated caller scope, cancellation and explicit outcomes; never log secrets or source text by default.
- Add meaningful regression checks for changed behavior. Run the build/test commands in the README.
- After changes, append local rationale to MEMORY.md: date, choice, why, status and evidence/decision links. Preserve prior entries; mark superseded choices explicitly.
- The root decision log owns approval status; feature tasks own progress. Memory explains rationale and links these records without replacing them.
- Descendant instructions refine this guidance. Do not edit generated bin/ or obj/ content.
