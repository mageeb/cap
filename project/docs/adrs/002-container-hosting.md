# ADR 002: Separate container hosting boundary

Status: Pending user review; [DEC-037](../../../DECISIONS.md#dec-037-container-hosting-preparation).

The backend has implemented orchestration services but no real adapters or business HTTP
contracts. The user requested a container and Kubernetes deployment preparation.

Add a separate ASP.NET Core host referencing the library. Expose process liveness and
fail-closed application readiness only. Preserve the existing .NET 8 target for this
preparation; production rollout requires runtime support/patch review. Package using a
multi-stage non-root Linux image and a restricted single-replica Kubernetes template.

Alternative: add hosting directly to the library or wire temporary adapters and business
endpoints. Separate hosting preserves approved contracts and avoids inventing unresolved
identity, persistence and provider choices.

Consequence: the package is useful for build/platform preparation, but readiness remains
503 and rollout cannot complete until real dependencies and business contracts exist.
Image execution and cluster validation are explicit remaining gates, not completed checks.
See [004 evidence](../../specs/004-container-deployment/tasks.md).

Follow-up (2026-10-30): the user authorized gated business HTTP contracts tested with
doubles. The health-only endpoint scope is superseded by ADR004; missing adapters and
identity still keep production business access and readiness blocked. Image build and
restricted container execution now pass; cluster validation remains pending.
