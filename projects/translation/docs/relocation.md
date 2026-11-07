# Translation file consolidation

The user requested `C:\Repos\cap\projects\translation` as the home for
Translation artifacts created in scratch-space. Deployment, infrastructure and
validation files were copied with SHA-256 comparison before source removal.
Their relative layout is preserved. Workflow working-directory instructions and
the project AGENTS pointer now use this location.

## DEC-TRANSLATION-LAYOUT-001

- Status is Pending user review.
- Scope is the placement of deployment and validation artifacts.
- Choice is `deploy/`, `infra/`, `docs/` and project-level instructions under this directory.
- Reason is to preserve existing relative links and command paths.
- Alternative is splitting these files across CAP's existing project tree.
- Existing application worktrees, external book originals and unrelated scratch drawings are preserved.
- CAP has an unfinished merge with DECISIONS.md conflicted. Git refused creation
  of feature/translation-project-layout. The merge, index and current branch are
  preserved. This task remains uncommitted on the existing feature branch.
- Verification is recorded below after the relocation checks.

Verification passed for all PowerShell parsers, project Markdown file links, and both backend and MongoDB Kustomize renders. All 27 original files matched SHA-256 before removal. This relocation makes no live cloud changes. Existing application worktrees remain outside this new directory.
