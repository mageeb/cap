# Implementation approach

First implement review and translation orchestration against approved ports. Independently
build deterministic contract fakes and regression scenarios. Then implement import and
real adapters once the user resolves input/storage/provider choices. Finish with build,
regression checks, independent adversarial review and documentation updates.

Small work units: review validation/delegation; draft ownership/success path; draft failure
and cancellation; tests per behavior group; import reader/validation; chosen persistence
adapter; selected provider adapter. Split adapter work further once choices are known.
