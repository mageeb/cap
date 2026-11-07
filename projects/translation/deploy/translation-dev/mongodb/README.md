# Translation DEV MongoDB

Target: existing AKS `translation-dev-aks` under `Translation-Dev`, namespace
`translation-dev`. No resources are created in Translation-INT.

## Schema sources and decisions

CAP Session Transcriptions thread `01a150f6-48ee-7b31-86ca-2e98072fa770` points to
the course transcripts. Week 6's organized notes, sections 12–13, describe a book,
ordered paragraphs, alternative document layouts and consistency requirements.
They explicitly do **not** finalize the physical schema. The persistence contracts
in `C:\Repos\cap\outputs\worktrees\deployment\project\src\Translation\Backend`
add reviewer scope, separate source/draft/reviewed text, opaque version tokens,
atomic creation and draft completion, and idempotent request IDs.

On 2026-11-06 the user explicitly requested chapters and loading the Chapter01
dataset. The physical schema below is derived from those sources and that request;
it is not claimed to be a verbatim schema agreed in the recording.

| Collection | Contents / identity |
|---|---|
| `books` | UUID string `_id`, reviewer scope, title, optional author/languages/import identity |
| `chapters` | UUID `_id`, book/reviewer references, explicit position and chapter number, titles |
| `paragraphs` | UUID `_id`, book/chapter/reviewer references, book-wide and chapter-local positions, source text, machine draft, current reviewed text, opaque version |
| `draftOperations` | UUID `_id`, book/reviewer references, logical request key, Pending/Succeeded/Failed state |
| `importFiles` | Original file bytes, filename, SHA-256 and book/chapter references |
| `schemaVersions` | Applied migration marker; no business content |

JSON-schema validators reject invalid types/required fields, negative positions,
unknown draft states and unexpected fields. Unique indexes enforce chapter order,
paragraph order and draft request idempotency. UUIDs are canonical strings to match
the C# Guid contracts without driver-specific binary UUID conventions. Documents
use BSON dates, Int32 positions and binary data for original files.

MongoDB does not enforce foreign keys, reviewer row-level authorization or version
comparisons automatically. The application adapter must use reviewer-scoped queries,
version predicates and transactions. The application user is scoped to this
database, not to one reviewer. No application adapter or API identity wiring is
implemented by provisioning the database.

## Hosting

- Official MongoDB Community 8.0 image, currently 8.0.34, pinned by digest in
  `kustomization.yaml`. One-node replica set `rs0` enables transactions but provides
  no node redundancy or high availability.
- StatefulSet `mongodb`; headless internal Service `mongodb`; no public endpoint.
- 16 GiB requested Azure managed Standard SSD; StorageClass reclaim policy Retain
  and StatefulSet claim-retention settings protect against accidental workload removal.
  Retention is not a backup; backups/restores and HA are not configured.
- Non-root UID/GID 999, read-only root filesystem, writable data/config/tmp volumes,
  bounded resources and probes. Kubernetes may provision/charge additional nodes
  or disks for the workload.
- TLS required, private CA, server certificate valid for one year. Plan certificate
  renewal before expiration. Clients must trust the CA; do not disable TLS validation.
- Replica keyfile and separate admin/app credentials are Kubernetes Secrets.
  Existing secrets are preserved on rerun; deleting/recreating credentials against
  an existing data disk is not password rotation and can break access.
- NetworkPolicy permits MongoDB traffic only from the backend, MongoDB and labelled
  schema/administration pods in this namespace. This is a DEV network boundary;
  trusted Kubernetes administrators can still access the workload.

Secrets (do not print them in terminal transcripts or commit their contents):

- `translation-mongodb-auth`: admin bootstrap credentials.
- `translation-mongodb-app`: app username/password and TLS connection URI.
- `translation-mongodb-keyfile`: replica-member shared key.
- `translation-mongodb-tls`: public `ca.crt` and private server PEM.

## Validation and deployment

Run local component and import tests using the exact pinned image:

```powershell
.\deploy\translation-dev\mongodb\Test-Local.ps1 -Image 'mongo@sha256:b9924c330a8bbc5800fe9c69f04a84e221eca7b28fd049a0354147d9908fa142' -SampleDirectory 'C:\Repos\Translation\AI-Assisted-Translation\Books\Romans - Abouna Antonios Fekry\Translation\Chapter01'
```

After verifying the dedicated kubeconfig/context, initialize secrets, review
`kubectl diff`, run server dry-run and apply this directory's Kustomization.
`Initialize-Secrets.ps1` requires Docker and the existing Entra Kubernetes tools.
`mongodb-schema-v1` initializes the replica set, collections/indexes, app user and
executes the same component checks. Do not remove validation failures to get a green
Job. Completed Jobs do not rerun on apply; future schema migrations need a new version.

## Chapter01 data import

Source folder:
`C:\Repos\Translation\AI-Assisted-Translation\Books\Romans - Abouna Antonios Fekry\Translation\Chapter01`.

- One book, **Romans**, author **Abouna Antonios Fekry**; Arabic source / English target.
- One chapter, number 1, using the source/target heading text.
- Source, target, unedited target and model files have 156 aligned lines, including
  122 nonempty lines. Each nonempty line becomes one paragraph, including headings;
  explicit original line numbers preserve its location and reading order.
- `machineDraft` comes from `UneditedTarget`; `reviewedText` stores the current
  `Target` file with `importStatus=ImportedCurrentTarget`. This is working text,
  not an assertion that a reviewer approved every paragraph.
- All 10 files, including the report, flags, model information and tracked changes,
  are preserved byte-for-byte in `importFiles`, with hashes checked after insertion.
- Import scope `sample:romans-antonios-fekry` is a synthetic data owner, not a
  fabricated authenticated user. Assigning this book to a real reviewer requires
  an explicit application identity/access decision.
- Deterministic IDs and insert-only transactions make a repeated identical import
  safe. If existing records differ, the importer refuses to overwrite them.
- Real source content is transferred to MongoDB only; payload files and credentials
  are not added to this repository or application telemetry.

`New-SamplePayload.ps1` creates a temporary payload. Transfer it to the running
MongoDB pod's `/tmp/romans-chapter01.json` over the authenticated Kubernetes API,
then execute `/schema/import-sample.js` with mongosh. The script compares every
paragraph's source/draft/current target and each original file's SHA-256 against
the payload. Remove temporary payloads after verification.

For the already-configured DEV target, `Import-Sample.ps1` performs this transfer,
import, full read-back verification and temporary payload cleanup. It refuses
changed existing sample records rather than overwriting them.

Connection inside the cluster: `mongodb-0.mongodb.translation-dev.svc.cluster.local:27017`,
database `translation_review`, replica set `rs0`, TLS and authentication required.
Use the app Secret and CA in client configuration. The backend remains unchanged;
its previous readiness limitation is not resolved merely by creating this database.

See `deployment-status.md` for actual cloud results.
