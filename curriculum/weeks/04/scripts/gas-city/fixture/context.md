# Studio Board: accepted two-release product brief

Build a local drawing workspace for posters, illustrated notes and short comics.
A student should create real artwork, save it, reopen it, reuse it and export it.
The existing tiny paint page is the starting implementation, not the finished
product. These are product requirements; the native planner must generate the
implementation graph. Do not treat this document as a supplied task list.

## Delivery contract

- Deliver two coherent releases in order with stock native `build-basic`.
  Release 2 starts only from Release 1's integrated, checked code.
- Use headless interaction, `drain_policy=separate`, agent reviews,
  `max_iterations=2`, `push=false` and `open_pr=false`. Mayor remains available
  for user conversation while native sessions execute the approved work.
- Keep a plain static HTML/CSS/ES-module app. Use Canvas 2D and browser storage;
  no backend, accounts, production packages, remote assets or network services.
  Installed Playwright/Chromium are permitted for tests only.
- Use `"$GC_DEMO_PYTHON"` for stock Python artifact helpers. Setup supplies
  this absolute PyYAML-enabled interpreter; native worker PATH may prepend a
  different Python. Do not substitute bare `python3` or modify stock shell gate
  checks. Native gates use the separately prepared supervisor environment.
- Work only in this demo's app, native worktrees and city. Use local feature
  branches and focused commits. Never edit CAP, add a remote, push or open a PR.
- Preserve usable drawing throughout. Define the eraser as whole-object/whole-
  stroke deletion so its behavior is explicit and serializable.

## Release 1: a usable drawing studio

### Document and editor foundation

Define a versioned document with stable object/layer IDs, canvas dimensions,
background, layer order and serializable drawing objects. Store document
coordinates independently of CSS/display coordinates. Agree interfaces for
rendering, tool input, document commands, history and feature registration before
parallel implementation. Reject non-finite coordinates and invalid object data.

Provide a clear workspace with a toolbar, canvas, layer panel and status area.
Canvas resize/display scaling must preserve artwork. Pointer capture supports
continuous drags; canceled gestures must leave a valid document. Tools and
chosen colors remain understandable when switching modes.

### Tools and appearance

Support pencil strokes, whole-object eraser, line, rectangle, ellipse and text.
Provide color, stroke width, shape fill/outline and text size controls, with
usable defaults. Drawing previews must not create repeated final objects.
Text entry uses normal form controls and renders user text without treating it
as markup. A Clear action is undoable and requires protection against accidental
loss. The eraser preserves the chosen pencil color.

### Layers and history

Create, rename, reorder and delete layers; select an active layer; hide/lock a
layer and change its opacity. Hidden layers retain their content. Locked layers
cannot be edited. Layer ordering determines rendering/export ordering.

Undo/redo covers strokes, shapes, text, clearing and layer changes. One completed
drag is one history action. A new edit after undo clears the redo branch. Bound
history to a documented limit and handle empty history without an error.

### Save, recovery and interchange

Autosave the current document and editor preferences locally, with a visible
saved/unsaved state. Reopen the same artwork after reload. Missing, malformed,
unsupported-version or unavailable storage must keep the editor usable and
explain whether data was restored or persistence is unavailable. Do not silently
replace recoverable saved data before the user chooses what to do.

Export and import versioned JSON with validation. Invalid imports preserve the
open document. Export PNG with the selected background/transparency policy and
SVG for every supported object type, preserving geometry, text and layer order.
Downloads have useful names. Export must not mutate the document or history.

### Visible Release 1 acceptance

Create an event poster: draw a teal pencil stroke, add an outlined ellipse and
text reading "Studio Night", and put the text on a second layer. Hide/reveal and
reorder that layer; lock it and confirm edits are prevented. Undo and redo a
stroke and a layer change. Reload and confirm the actual artwork and preferences
remain. Export JSON/PNG/SVG; clear, import the JSON and confirm the restored
artwork matches. Exercise invalid import and unavailable/malformed storage.

## Release 2: a reusable creative workspace

Retain Release 1 behavior. If the document schema changes, migrate valid Release
1 saves/imports and test migration; do not discard existing student artwork.

### Selection, editing and arrangement

Select objects with visible bounds; support move, resize and rotate without
changing unrelated objects. Handle multi-selection, deselection, delete,
duplicate and copy/paste. Copies receive fresh IDs and a useful offset.
Change selected object appearance/text through appropriate controls. Provide
bring-forward/send-backward, align and distribute actions with defined behavior
for unsuitable selections. All supported mutations use the same history API.

### Projects, pages and navigation

Add a local project gallery with names, thumbnails, create/open/rename/duplicate
and confirmed deletion. Projects must not overwrite one another. Add multiple
pages with create, switch, rename, reorder, duplicate and delete, preserving
page-specific artwork. Keep unsaved changes safe when navigating.

Support fit-to-view, zoom controls and pan. Selection/drawing still use document
coordinates after navigation. Provide an overview of the active project/page
and useful empty states. Panels remain usable on narrower browser windows.

### Templates and reusable content

Offer poster, illustrated-note and three-panel-comic templates with real starter
content. Applying a template creates editable objects in a new project or an
explicitly confirmed replacement. Include a small local stamp palette such as
arrows, stars and speech bubbles, plus a way to reuse selected artwork.
All assets remain local; templates and stamps obey the shared document format.

### Keyboard access and product hardening

Make controls labeled and keyboard reachable with visible focus and sensible
focus restoration. Provide documented shortcuts for tool choice, undo/redo,
copy/paste/delete, selection movement and navigation. Do not intercept typing
shortcuts inside text fields. An accessible object/layer list lets a keyboard
user select artwork and invoke supported editing actions; announce important
save/error/selection changes without flooding announcements.

Handle empty selections/projects, duplicate names, unavailable downloads,
invalid/corrupt imports and storage failure with clear recoverable outcomes.
Review pointer and keyboard behavior, escaping of text in SVG, listener cleanup,
large-but-bounded documents and regression coverage for the first release.

### Visible Release 2 acceptance

Create a three-page illustrated project from a template. Add a stamp; select,
move, resize, rotate, duplicate and copy it to another page. Align a selection,
change text and undo/redo the edits. Zoom/pan, then draw at the expected position.
Rename/reorder/duplicate pages; reopen the project from the gallery after reload
and confirm thumbnails and artwork. Perform a supported edit through keyboard
controls, including the accessible object list. Export/reimport the assembled
JSON and export the active page as PNG/SVG. Also reopen a valid Release 1 save.

## Native planning and ownership rules

Generate requirements, the reviewed implementation plan and task decomposition
for the requested release. This scope should naturally support roughly 60–80
meaningful implementation tasks per release, approximately 120–160 in total.
Task counts are estimates, not acceptance criteria. Never create dummy tasks,
sleeps, duplicate reviews or administrative beads to inflate those counts.
GC 1.4.2 hard-caps one drain at 100 members: keep each implementation convoy
strictly below 100. Record the actual count before implementation; if the real
scope cannot fit, report the scope/limit conflict rather than launch an oversized
convoy or hide unrelated work inside an arbitrary oversized task.

Each task has one outcome, requirement trace, owned source/test files, required
input commits, true dependencies and a completion check. Aim for 2–5 minutes
including its check where practical. A passing task produces working behavior
or meaningful evidence, not merely a nonempty file or a self-reported verdict.

Establish shared contracts first. Then fan out independent tool, layer, history,
storage/interchange, workspace and QA modules into disjoint owned files. Allow
parallel tasks only when their inputs exist and their writes do not overlap.
Assign shared entry points, registries, root CSS and package scripts to one
foundation/integration owner, with ordered tasks for any shared edits. Do not
have every worker rewrite the app entry point or amend the common schema.

### Code dependencies and integration are explicit work

Native task readiness does not transfer code between item worktrees. Before
editing, a worker must read its source anchor's `work_dir`, use that Git worktree
and create/use a local feature branch if native preparation left detached HEAD.
Read prerequisite reports and import their verified commits in dependency order
into that worktree, including required ancestor changes. Check the resulting
interfaces before implementing. Skip commits already present; stop and report
an unresolved conflict or missing input rather than invent another contract.

Each worker reports its actual source/task ID, absolute worktree path, feature
branch, own focused commit SHA, prerequisite commits and commands/results.
The graph must include an integration owner dependent on the feature work and
QA inputs. That owner combines only each task's own implementation commits in
dependency order on one local release feature branch, wires shared entry points
and resolves conflicts. Prerequisite imports are reported separately as inputs;
do not import them again as if they were new product work. Run checks against
that single assembled app; independent review targets its exact path/commit.
Scattered passing worker reports and untouched launcher files do not constitute
a delivered release. Mayor does not manually integrate every individual task.

After Release 1 passes integration and review, Mayor verifies its result path,
feature branch and commit. Mayor performs one release handoff before Release 2:
verify the paint launcher's clean state, then bring the verified integrated
commit onto its local feature branch by a fast-forward or local merge. Confirm
the integrated commit is present in launcher HEAD before the next stock
worktrees are created. Preserve existing work; do not reset, overwrite unrelated
edits or launch Release 2 from the tiny baseline. If this handoff cannot be
performed safely, report the blocker.

## Required checks and evidence

Assign QA ownership for focused Node checks and a headless Playwright acceptance
runner. Integration must expose `npm run check` for all meaningful Node checks
and `npm run browser:check` for the requested release's complete browser flow.
These scripts are deliverables of the build, not scripts assumed to exist in
the tiny starter. Re-run Release 1 regression checks in Release 2.

Use the installed test-only Playwright/Chromium. Verify Playwright resolves in
the QA/integration worktree; install the existing locked test dependencies there
if needed, without adding production dependencies. Use `headless: true`,
`browser.newContext()` then `context.newPage()`, and isolated test storage.
Operate a local test server with bounded cleanup; never drive the user's browser
or tabs. Exercise real pointer/keyboard interaction and canvas output, not only
mocked DOM/storage or screenshots of an empty canvas. Check downloaded JSON,
PNG and SVG content; use a fresh context to verify import/reopen where appropriate.

Save numbered, named screenshots of actual artwork and important states under
`artifacts/release-1/` or `artifacts/release-2/`, with test logs and a concise
acceptance report identifying the tested app path/commit. Include normal editing,
layers/history, reload, restored import/export and the release-specific complete
flow. A screenshot alone is not a behavioral assertion. Missing tooling or a
failed check blocks acceptance and must be reported; never label unseen browser
behavior verified.

Review the consolidated app and evidence independently. Report requirement
coverage, actual meaningful task count, failures/repairs, started/finished times
and elapsed duration, final branch/commit, copyable `cd` command, check commands,
screenshot paths and remaining issues. Begin before class and measure the run;
no task-count or 30-minute runtime guarantee, artificial delay or sleep is allowed.
