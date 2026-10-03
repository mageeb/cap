# Accepted product contract: remember paint preferences
A user picks a drawing tool and color, reloads, and resumes with the same choices.

Allowed tools: pencil, eraser.
Allowed colors: #152536 (navy), #188D91 (teal), #E8A64C (amber).
Defaults: {tool:'pencil', color:'#152536'}.
Storage key: cap.paint.preferences.v1. JSON shape: {tool, color} only.
Missing, malformed, or invalid saved preferences fall back to BOTH defaults.
Storage read/write exceptions must not stop drawing.
Pencil/teal must draw a visible teal stroke after reload. Check visible color
with PENCIL: an eraser stroke cannot prove which drawing color is selected.
Eraser selection and the retained drawing color must persist independently.
The page remains usable with keyboard-labelled tool and color controls.
No packages, services, build step, or other product features.

Accepted module contract:
- app/toolbar.js exports renderToolbar(preferences), an HTML string containing
  labelled select#tool and select#color with the supplied choices selected.
  It has no storage access or event handlers.
- app/settings.js exports loadPreferences(storage) and
  savePreferences(storage, preferences). The storage object is injected.
  load returns a fresh valid {tool,color}. save writes normalized valid data.
- app/main.js owns rendering, change handlers, persistence calls, and drawing.

Task graph:
UI toolbar   ──┐
              ├── Integration ── Review ── human browser acceptance
Storage      ──┘

UI and Storage can run concurrently because their owned files do not overlap.
Integration starts only after BOTH actual reports. Review starts after integration.
