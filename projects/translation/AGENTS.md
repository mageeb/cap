# Repository instructions

For every future change, follow [the validation and DEV deployment workflow](docs/validation-workflow.md).
Treat its applicable checks as completion requirements: local unit/component tests,
restricted-container smoke tests, validated deployment of the same image digest,
cloud API checks, and Azure telemetry evidence. Scale the checks to the change;
documentation-only changes do not require a cloud deployment.

Read the workflow before planning or editing. Preserve existing work, use a feature
branch, and record actual commands/results and unresolved blockers. Do not report
a change as fully validated when an applicable gate has failed or was not run.
Existing authorization and scope still govern cloud changes; this workflow does
not grant standing permission for deployment or new billable resources.

Application source currently lives in CAP application and deployment worktrees. Read and
follow the applicable `AGENTS.md` files before changing it, and verify that tests and image
builds use the same source revision as the proposed release.
