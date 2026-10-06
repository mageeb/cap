# Remember paint preferences
A user picks a drawing tool and color, reloads, and resumes with those choices.

- Tools: pencil and eraser. Colors: #152536 (navy), #188D91 (teal), #E8A64C (amber).
- Defaults: {tool:'pencil', color:'#152536'}.
- Save JSON {tool,color} under localStorage key cap.paint.preferences.v1.
- Missing, malformed or invalid data uses both defaults. Denied storage must
  not stop drawing. Eraser retains the selected drawing color.
- Keep labelled controls and the existing canvas. No packages or extra features.

Module interfaces:
- app/toolbar.js: renderToolbar(preferences) returns labelled select#tool and
  select#color HTML with the supplied choices selected. No storage or events.
- app/settings.js: loadPreferences(storage) returns a fresh valid {tool,color};
  savePreferences(storage, preferences) writes normalized valid data.
- app/main.js: loads preferences, renders controls, saves changes and handles
  pencil/eraser drawing. A Pencil/Teal stroke stays visibly teal after reload.

UI and Storage finish first, then Integration, then independent Review.
