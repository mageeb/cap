load('/schema/assertions.js');
const bootstrapHost = process.env.MONGODB_HOST || 'mongodb-0.mongodb.translation-dev.svc.cluster.local';
const rootUser = process.env.MONGO_INITDB_ROOT_USERNAME;
const rootPassword = process.env.MONGO_INITDB_ROOT_PASSWORD;
if (!rootUser || !rootPassword || !process.env.MONGODB_APP_PASSWORD) throw new Error('MongoDB bootstrap credentials are required');
const tlsQuery = process.env.MONGODB_TLS_CA ? '&tls=true&tlsCAFile=' + encodeURIComponent(process.env.MONGODB_TLS_CA) : '';
const rootUri = 'mongodb://' + encodeURIComponent(rootUser) + ':' + encodeURIComponent(rootPassword) + '@' + bootstrapHost + ':27017/admin?directConnection=true' + tlsQuery;
let admin;
for (let attempt = 0; attempt < 90; attempt++) {
  try { admin = new Mongo(rootUri).getDB('admin'); checkMongoCommand(admin.runCommand({ ping: 1 })); break; }
  catch (error) { if (attempt === 89) throw error; sleep(2000); }
}
let replicaStatus;
try { replicaStatus = admin.runCommand({ replSetGetStatus: 1 }); }
catch (error) { if (error.code !== 94) throw error; replicaStatus = { code: 94 }; }
if (replicaStatus.code === 94) {
  checkMongoCommand(admin.runCommand({ replSetInitiate: { _id: 'rs0', members: [{ _id: 0, host: bootstrapHost + ':27017' }] } }));
} else { checkMongoCommand(replicaStatus); }
for (let attempt = 0; attempt < 90; attempt++) {
  if (admin.runCommand({ hello: 1 }).isWritablePrimary) break;
  if (attempt === 89) throw new Error('Replica set did not elect a primary');
  sleep(2000);
}
globalThis.reviewDatabase = admin.getSiblingDB('translation_review');
load('/schema/schema.js');
applyTranslationSchema(reviewDatabase);
const applicationUser = process.env.MONGODB_APP_USERNAME || 'translation_app';
if (!reviewDatabase.getUser(applicationUser)) {
  reviewDatabase.createUser({ user: applicationUser, pwd: process.env.MONGODB_APP_PASSWORD, roles: [{ role: 'readWrite', db: 'translation_review' }] });
}
print('PASS: rs0 primary; translation_review schema and application user initialized');
