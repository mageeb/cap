# Studio Board demo contract

Read context.md and the requested release's native requirements/plan. Use stock
native build-basic; keep Mayor available for conversation. Work only in this
demo's app, item worktrees and city, never in the parent CAP checkout.

- Run stock Python artifact helpers with `"$GC_DEMO_PYTHON"`, the absolute
  demo interpreter supplied by setup. Do not assume bare `python3` has PyYAML;
  preserve stock shell gate checks.
- Keep all edits and commits on local feature branches. A native detached item
  worktree must get a feature branch before implementation. No remote, push or PR.
- Honor the approved document/module interfaces, file ownership and dependencies.
  Use the source anchor's work_dir; import verified prerequisite commits before
  editing. Report your own commit, branch, path and actual checks.
- One explicit integration owner assembles a single release branch/app from the
  item commits. Passing isolated reports do not deliver the integrated product.
- Release 2 uses verified integrated Release 1 code at the registered launcher
  HEAD. Preserve existing work; report unresolved import/merge/base conflicts.
- No production dependencies/services/accounts. Installed Playwright is test-only.
  Deliver meaningful Node checks and headless Playwright acceptance/screenshots;
  never take over the user's browser or claim an unobserved check passed.
- Keep native convoys below 100 members. Roughly 60–80 real implementation tasks
  per release is a planning estimate, not permission to pad the graph or delay.

Follow the detailed integration and evidence requirements in context.md.
