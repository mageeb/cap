# MongoDB DEV deployment and sample import

Verified on 2026-11-06 (America/Los_Angeles).
Status: deployed, authenticated TLS database ready, Chapter01 data loaded and read back.

## Target and running resources

- Subscription `SPSD.Sub1`, resource group `Translation-Dev`, existing cluster
  `translation-dev-aks`, namespace `translation-dev`.
- MongoDB Community 8.0.34; exact image digest
  `sha256:b9924c330a8bbc5800fe9c69f04a84e221eca7b28fd049a0354147d9908fa142`.
- StatefulSet `mongodb`: 1/1 ready; pod `mongodb-0`: ready with zero container restarts
  after the controlled pod replacement. One-node replica set `rs0`.
- PVC `data-mongodb-0`: Bound, 16 GiB, ReadWriteOnce, Azure managed Standard SSD.
- PV `pvc-27bd755b-1255-4827-bcbe-5f7444976f70`: verified reclaim policy `Retain`.
  Its disk is managed through AKS's node resource group, as is normal for CSI disks.
- Job `mongodb-schema-v1`: Complete, 1/1, initialized schema and ran component checks.
- TLS-required internal Service; ingress NetworkPolicy; separate admin and app Secrets.
  No public MongoDB endpoint. No resources created in `Translation-INT`.

## Imported records

Database: `translation_review`.

| Content | Stored count |
|---|---:|
| Books: Romans, Abouna Antonios Fekry | 1 |
| Chapters: Chapter One | 1 |
| Aligned nonempty paragraphs | 122 |
| Complete original files with SHA-256 | 10 |

Book ID: `a784c44a-bb58-20a5-63fb-f8cc47a9d5dc`.
Chapter ID: `0a2ca153-1d45-8bf2-0bb0-9a45fe3c1a90`.
Data-owner scope: `sample:romans-antonios-fekry`.

Source folder explicitly supplied by the user:
`C:\Repos\Translation\AI-Assisted-Translation\Books\Romans - Abouna Antonios Fekry\Translation\Chapter01`.

Source Arabic, unedited English machine draft, current English target and model
metadata were aligned by original line. All 10 originals, including the report and
sidecar files, are preserved as BSON binary documents. Current target text is
labelled `ImportedCurrentTarget`, without asserting human approval.

## Verification evidence

1. Read CAP Session Transcriptions and Week 6 schema discussion; checked later
   C# contracts. The transcript leaves the physical schema open. Chapters are an
   explicit addition from the user's current request.
2. Local real-MongoDB checks passed under TLS, authentication, UID 999, read-only
   root filesystem and pinned image: multi-collection commit, transaction rollback,
   malformed-document rejection, unique chapter/paragraph positions, draft request
   idempotency, version-predicate conflict behavior and restricted app privileges.
3. Local sample import passed twice without duplicate records, verifying every
   paragraph source/draft/current-target/model field and every original file hash.
4. Kubernetes server dry-run and diff review passed before applying. AKS injected
   scheduling preferences and emitted probe warnings for the finite schema Job;
   these were warnings, not admission rejections. The persistent server has probes.
5. The cloud schema Job completed the same database checks successfully.
6. Cloud import wrote 1 book, 1 chapter, 122 paragraphs and 10 files in a transaction.
   Read-back verified every paragraph text field and all file SHA-256 values.
7. Replaced only `mongodb-0` with a normal graceful pod deletion. Before any
   re-import, authenticated queries verified all counts and the schema marker
   survived on the persistent disk.
8. A subsequent insert-only import/read-back again verified all paragraph fields
   and original file hashes. No records were duplicated or overwritten.
9. Final state: StatefulSet ready 1/1, completed schema Job, PVC bound, PV Retain.
   PowerShell syntax, local documentation links and whitespace were checked.

Temporary input payloads and private certificate-generation files were removed.
Credentials were generated in memory and stored as Kubernetes Secrets; their
values were not written to source or verification output.

## Access

Inside the cluster:
`mongodb-0.mongodb.translation-dev.svc.cluster.local:27017`.
Database `translation_review`, replica set `rs0`, TLS and authentication required.

Application connection URI and credentials: Secret `translation-mongodb-app`.
Trusted CA: key `ca.crt` in Secret `translation-mongodb-tls`. Trust this CA in the
client; never bypass hostname/certificate verification. The server certificate
includes localhost for authenticated local port-forward diagnostics.

[Open the AKS resource in Azure Portal](https://portal.azure.com/#resource/subscriptions/3df3fa8b-637a-4277-a7d3-2f86e4606a3e/resourceGroups/Translation-Dev/providers/Microsoft.ContainerService/managedClusters/translation-dev-aks/overview)

In the portal, select Kubernetes resources / Workloads, namespace `translation-dev`,
StatefulSet `mongodb`. This is self-managed MongoDB on AKS, so database documents
are accessed using MongoDB clients rather than a Cosmos DB Data Explorer.

## Remaining boundaries

The API application was not changed or redeployed by this database task. It still
needs a concrete MongoDB persistence adapter and validated identity configuration
before its business routes/readiness can succeed. No application unit suites or
API release checks were claimed for this database-only change; actual MongoDB
component and cloud data checks above cover the changed behavior.

This is a single-node DEV deployment. Backups/restore automation and high
availability are not configured. A retained disk is not a backup. Plan TLS
certificate renewal before its one-year expiry and rotate database users through
MongoDB before updating the corresponding Secrets.

Schema/source rationale and deployment scripts: [README.md](README.md).
