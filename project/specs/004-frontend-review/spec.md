# 004: Seeded-document review frontend

User scope (2026-11-06): build the basic frontend under Translation/Frontend.
Skip upload, translation generation/status and authentication. Parallel text is seeded
in the database externally. The target workflow is open, edit, save and reload.

- AC-1: Open a known document UUID through the read API; render stable paragraph order.
- AC-2: Pair read-only Arabic RTL with English LTR editable text; prefer saved correction,
  including empty text, over the machine draft.
- AC-3: Explicit paragraph/all saves send expected version and reflect server acknowledgement.
  Typing during a save remains dirty after the earlier edit is acknowledged.
- AC-4: Failed saves and conflicts retain local edits. Conflicts show server text and
  require an explicit choice before resubmission.
- AC-5: Reopening reads saved corrections from the backend. Warn before discarding
  local edits. Support keyboard save/navigation and a narrow responsive layout.
- AC-6: A visibly labeled sample permits UI exploration without implying persistence.
  Missing backend configuration gives an error; no local-storage fallback.

Real database acceptance is pending backend adapters and demo identity setup.
