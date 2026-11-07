// Credentials remain in the pod environment, never embedded in this import file.
load('/schema/assertions.js');
const fs = require('fs');
const crypto = require('crypto');
const sample = JSON.parse(fs.readFileSync(process.env.MONGODB_IMPORT_FILE || '/tmp/romans-chapter01.json', 'utf8').replace(/^\uFEFF/, ''));
const credentials = encodeURIComponent(process.env.MONGO_INITDB_ROOT_USERNAME) + ':' + encodeURIComponent(process.env.MONGO_INITDB_ROOT_PASSWORD);
const importedDatabase = new Mongo('mongodb://' + credentials + '@localhost:27017/admin?directConnection=true&tls=true&tlsCAFile=/tls/ca.crt').getDB('translation_review');
const createdAt = new Date();
function documentFor(collection, item) {
  const result = { ...item, createdAt, updatedAt: createdAt };
  for (const key of ['position', 'chapterPosition', 'chapterNumber', 'sourceLineCount', 'sourceLineNumber', 'byteCount']) {
    if (key in result) result[key] = Int32(result[key]);
  }
  if (collection === 'importFiles') { result.data = BinData(0, result.contentBase64); delete result.contentBase64; }
  return result;
}
const rows = [['books', [sample.book]], ['chapters', [sample.chapter]], ['paragraphs', sample.paragraphs], ['importFiles', sample.files]];
// Refuse changed source or edited sample data rather than silently overwrite it.
for (const [name, items] of rows) {
  for (const item of items) {
    const existing = importedDatabase.getCollection(name).findOne({ _id: item._id });
    if (!existing) continue;
    for (const [field, expected] of Object.entries(item)) {
      if (field === 'contentBase64') continue;
      checkEqual(expected, existing[field], 'Existing sample differs: ' + name + '.' + field + '; migration/review required');
    }
  }
}
const importSession = importedDatabase.getMongo().startSession();
try {
  importSession.withTransaction(() => {
    const transactional = importSession.getDatabase('translation_review');
    for (const [name, items] of rows) {
      for (const item of items) transactional.getCollection(name).updateOne({ _id: item._id }, { $setOnInsert: documentFor(name, item) }, { upsert: true });
    }
  }, { readConcern: { level: 'snapshot' }, writeConcern: { w: 'majority' } });
} finally { importSession.endSession(); }
checkEqual(1, importedDatabase.books.countDocuments({ _id: sample.book._id, reviewerId: sample.book.reviewerId }));
checkEqual(1, importedDatabase.chapters.countDocuments({ _id: sample.chapter._id, bookId: sample.book._id }));
checkEqual(sample.paragraphs.length, importedDatabase.paragraphs.countDocuments({ chapterId: sample.chapter._id }));
for (const paragraph of sample.paragraphs) {
  const actual = importedDatabase.paragraphs.findOne({ _id: paragraph._id });
  for (const field of ['bookId', 'chapterId', 'position', 'chapterPosition', 'sourceLineNumber', 'sourceText', 'machineDraft', 'reviewedText', 'translationModel']) checkEqual(paragraph[field], actual[field], 'Imported paragraph mismatch: ' + field);
}
checkEqual(sample.files.length, importedDatabase.importFiles.countDocuments({ chapterId: sample.chapter._id }));
for (const file of sample.files) {
  const stored = importedDatabase.importFiles.findOne({ _id: file._id });
  const contentBytes = Buffer.from(stored.data.buffer).subarray(0, stored.data.position);
  checkEqual(file.byteCount, contentBytes.length);
  checkEqual(file.sha256, crypto.createHash('sha256').update(contentBytes).digest('hex'));
}
print(JSON.stringify({ status:'PASS', database:'translation_review', bookId:sample.book._id, chapterId:sample.chapter._id, books:1, chapters:1, paragraphs:sample.paragraphs.length, originalFiles:sample.files.length, verified:'all paragraph text fields and SHA-256 of every original file' }));
