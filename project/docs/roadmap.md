# Project roadmap

The repository scaffold is in place. Product milestones below are drafts and will be refined while teaching.

| Order | Milestone | Specification and tasks |
|---|---|---|
| 1 | Define and implement the first complete user workflow | [Spec](../specs/001-first-workflow/spec.md) · [Tasks](../specs/001-first-workflow/tasks.md) |
| 2 | Review proposed Translation backend interfaces | [Spec](../specs/002-backend-skeleton/spec.md) · [Tasks](../specs/002-backend-skeleton/tasks.md) |
| 3 | Implement backend services and selected adapters | [Spec](../specs/003-backend-implementation/spec.md) · [Tasks](../specs/003-backend-implementation/tasks.md) |

Add milestones when their purpose and scope are clear. Keep detailed completion status in the linked task files.

## Teaching checkpoints

No checkpoints recorded yet. When a lesson uses the project, record the lesson link and actual Git tag or commit here. Weekly lesson plans remain in `curriculum/weeks/` at the repository root.

Container preparation: [004 spec](../specs/004-container-deployment/spec.md) and [tasks](../specs/004-container-deployment/tasks.md). Image build and restricted container smoke pass; cluster validation and production readiness remain pending.

API-host observability: [005 spec](../specs/005-api-observability/spec.md) and [tasks](../specs/005-api-observability/tasks.md). Business contracts: [006 spec](../specs/006-business-api/spec.md) and [tasks](../specs/006-business-api/tasks.md). Production business access remains blocked until adapters and identity are configured.
