# CAP: AI Native Software Engineering Residency

Course materials and a continuing classroom application for teaching AI-assisted software engineering.

- [Course overview and weekly index](CAP%202.0%20AI-Native.md)
- [Classroom project](project/README.md)
- [Instructor preparation and delivery plan](conductor/plan.md)
- [Repository agent instructions](AGENTS.md)

## Layout

| Location | Purpose |
|---|---|
| `curriculum/weeks/` | Weekly lessons, homework, reference material, and isolated exercises |
| `project/` | The application developed throughout teaching, including specs and tasks |
| `infrastructure/template/` | Starter material for student repositories |
| `conductor/` | Course preparation and delivery decisions |
| `tools/` | Teaching and assessment support |

Students continue to use their own repositories as specified by each assignment. The classroom project is a shared teaching reference.

## Agent instructions

Use the exact filename `AGENTS.md`. The root file contains repository-wide guidance; [project/AGENTS.md](project/AGENTS.md) adds application-specific guidance. The root file explicitly directs agents to read the project file before working there.

Codex builds its instruction chain at session startup, from the repository root down to the working directory. Start a new session after changing instruction files to refresh automatic discovery; in an existing session, explicitly ask the agent to reread them. See the [official AGENTS.md documentation](https://learn.chatgpt.com/docs/agent-configuration/agents-md).
