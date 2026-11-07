// Real-database component checks. Only records with this run's unique reviewer are removed.
if (!globalThis.reviewDatabase) throw new Error('Run bootstrap.js before verify.js');
const verificationOwner = '__schema_check_' + UUID().toString();
const otherId = () => require('crypto').randomUUID();
const bookId = otherId();
const paragraphId = otherId();
const operationId = otherId();
const chapterId = otherId();
const stamp = { createdAt: new Date(), updatedAt: new Date() };
const book = { _id: bookId, reviewerId: verificationOwner, title: 'Synthetic schema check', ...stamp };
const chapter = { _id: chapterId, reviewerId: verificationOwner, bookId, position: Int32(0), chapterNumber: Int32(1), title: 'Synthetic chapter', ...stamp };
const paragraph = { _id: paragraphId, reviewerId: verificationOwner, bookId, chapterId, position: Int32(0), chapterPosition: Int32(0), sourceText: 'Synthetic source', machineDraft: null, reviewedText: null, version: 'v1', ...stamp };
const operation = { _id: operationId, reviewerId: verificationOwner, bookId, requestId: 'check-request', state: 'Pending', failureMessage: null, ...stamp };
function rejects(action, code) {
  let rejected = false;
  try { action(); } catch (error) { if (error.code !== code) throw error; rejected = true; }
  check(rejected, 'Expected MongoDB error ' + code);
}
try {
  applyTranslationSchema(reviewDatabase); // Applying twice must preserve data/indexes.
  const session = reviewDatabase.getMongo().startSession();
  try {
    session.withTransaction(() => {
      const transactionDb = session.getDatabase('translation_review');
      transactionDb.books.insertOne(book);
      transactionDb.chapters.insertOne(chapter);
      transactionDb.paragraphs.insertOne(paragraph);
      transactionDb.draftOperations.insertOne(operation);
    }, { readConcern: { level: 'snapshot' }, writeConcern: { w: 'majority' } });
    print('PASS: multi-collection transaction commits');
    session.startTransaction();
    session.getDatabase('translation_review').books.updateOne({ _id: bookId }, { $set: { title: 'Must roll back' } });
    session.abortTransaction();
    checkEqual('Synthetic schema check', reviewDatabase.books.findOne({ _id: bookId }).title);
    print('PASS: transaction rollback');
  } finally { session.endSession(); }
  rejects(() => reviewDatabase.paragraphs.insertOne({ ...paragraph, _id: otherId(), position: Int32(-1) }), 121);
  rejects(() => reviewDatabase.books.insertOne({ ...book, _id: otherId(), title: '' }), 121);
  rejects(() => reviewDatabase.draftOperations.insertOne({ ...operation, _id: otherId(), state: 'Unknown' }), 121);
  print('PASS: validators reject malformed documents');
  rejects(() => reviewDatabase.paragraphs.insertOne({ ...paragraph, _id: otherId() }), 11000);
  rejects(() => reviewDatabase.chapters.insertOne({ ...chapter, _id: otherId() }), 11000);
  rejects(() => reviewDatabase.draftOperations.insertOne({ ...operation, _id: otherId() }), 11000);
  print('PASS: paragraph ordering and draft idempotency unique indexes');
  const saved = reviewDatabase.paragraphs.updateOne({ _id: paragraphId, reviewerId: verificationOwner, version: 'v1' }, { $set: { reviewedText: 'Synthetic reviewed text', version: 'v2', updatedAt: new Date() } });
  checkEqual(1, saved.modifiedCount);
  checkEqual(0, reviewDatabase.paragraphs.updateOne({ _id: paragraphId, reviewerId: verificationOwner, version: 'v1' }, { $set: { reviewedText: 'Stale write' } }).matchedCount);
  checkEqual(0, reviewDatabase.books.countDocuments({ _id: bookId, reviewerId: 'another-reviewer' }));
  print('PASS: optimistic-concurrency and reviewer-scoped query pattern');
  const appUri = 'mongodb://' + encodeURIComponent(process.env.MONGODB_APP_USERNAME || 'translation_app') + ':' + encodeURIComponent(process.env.MONGODB_APP_PASSWORD) + '@' + bootstrapHost + ':27017/translation_review?directConnection=true' + tlsQuery;
  const appDatabase = new Mongo(appUri).getDB('translation_review');
  checkEqual(1, appDatabase.books.countDocuments({ _id: bookId }));
  rejects(() => {
    const response = appDatabase.getSiblingDB('admin').runCommand({ usersInfo: 1 });
    if (response.ok !== 1) throw Object.assign(new Error('Administrative operation denied'), { code: response.code });
  }, 13);
  print('PASS: application authentication and administrative privilege restriction');
} finally {
  for (const collection of ['draftOperations', 'paragraphs', 'chapters', 'books']) reviewDatabase.getCollection(collection).deleteMany({ reviewerId: verificationOwner });
}
print('PASS: synthetic validation records removed; business collections remain empty');
