# 004: Backend container and Kubernetes preparation

Prepare the existing backend for container hosting without deciding database, identity,
input policy or translation vendor. No cluster deployment is requested.

- AC-1: A separate HTTP host references the backend; GET /health/live returns 200.
- AC-2: GET /health/ready returns 503 while real business dependencies are absent;
  unknown routes return 404 and no unvalidated business endpoint is exposed. Feature 006 supersedes the original business-route 404 behavior with explicit blocked API responses.
- AC-3: A multi-stage Linux image publishes the host and runs non-root on port 8080.
- AC-4: Kubernetes Deployment/ClusterIP Service include probes, resource bounds,
  restricted security settings and an explicit image replacement step.
- AC-5: Reproducible build, HTTP smoke and image/manifest validation instructions
  distinguish verified results from unavailable tooling and deployment blockers.

Follow-up: feature 006 supersedes AC-2's absent business-route behavior for the five mapped /api/books contracts. They return sanitized 503 until adapters are configured; unknown routes still return 404. Readiness remains 503.
