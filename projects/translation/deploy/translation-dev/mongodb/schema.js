// Physical MongoDB design derived from CAP Week 6 and the implemented C# contracts.
// UUIDs are canonical strings; version is an opaque optimistic-concurrency token.
globalThis.applyTranslationSchema = function (database) {
  const uuid = { bsonType: 'string', pattern: '^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$' };
  const text = { bsonType: 'string', minLength: 1 };
  const nullableText = { bsonType: ['string', 'null'] };
  const common = { _id: uuid, reviewerId: text, createdAt: { bsonType: 'date' }, updatedAt: { bsonType: 'date' } };
  const definitions = {
    books: {
      required: ['_id', 'reviewerId', 'title', 'createdAt', 'updatedAt'],
      properties: { ...common, title: text, author: nullableText, sourceLanguage: text, targetLanguage: text, originKey: text, lastDraftOperationId: { anyOf: [uuid, { bsonType: 'null' }] } }
    },
    chapters: {
      required: ['_id', 'reviewerId', 'bookId', 'position', 'chapterNumber', 'title', 'createdAt', 'updatedAt'],
      properties: { ...common, bookId: uuid, position: { bsonType: 'int', minimum: 0 }, chapterNumber: { bsonType: 'int', minimum: 1 }, title: text, sourceTitle: nullableText, originKey: text, sourceLineCount: { bsonType: 'int', minimum: 1 } }
    },
    paragraphs: {
      required: ['_id', 'reviewerId', 'bookId', 'chapterId', 'position', 'chapterPosition', 'sourceText', 'machineDraft', 'reviewedText', 'version', 'createdAt', 'updatedAt'],
      properties: { ...common, bookId: uuid, chapterId: uuid, position: { bsonType: 'int', minimum: 0 }, chapterPosition: { bsonType: 'int', minimum: 0 }, sourceLineNumber: { bsonType: 'int', minimum: 1 }, sourceText: text,
        machineDraft: nullableText, reviewedText: nullableText, translationModel: nullableText, importStatus: { enum: ['ImportedCurrentTarget'] }, version: text }
    },
    importFiles: {
      required: ['_id', 'reviewerId', 'bookId', 'chapterId', 'fileName', 'sha256', 'byteCount', 'data', 'createdAt', 'updatedAt'],
      properties: { ...common, bookId: uuid, chapterId: uuid, fileName: text, sha256: { bsonType: 'string', pattern: '^[0-9a-f]{64}$' }, byteCount: { bsonType: 'int', minimum: 0 }, data: { bsonType: 'binData' } }
    },
    draftOperations: {
      required: ['_id', 'reviewerId', 'bookId', 'requestId', 'state', 'failureMessage', 'createdAt', 'updatedAt'],
      properties: { ...common, bookId: uuid, requestId: text,
        state: { enum: ['Pending', 'Succeeded', 'Failed'] }, failureMessage: nullableText }
    }
  };
  for (const [name, definition] of Object.entries(definitions)) {
    const validator = { $jsonSchema: { bsonType: 'object', additionalProperties: false, ...definition } };
    if (!database.getCollectionNames().includes(name)) {
      database.createCollection(name, { validator, validationLevel: 'strict', validationAction: 'error' });
    } else {
      checkMongoCommand(database.runCommand({ collMod: name, validator, validationLevel: 'strict', validationAction: 'error' }));
    }
  }
  database.books.createIndex({ reviewerId: 1, _id: 1 }, { name: 'books_by_reviewer' });
  database.chapters.createIndex({ reviewerId: 1, bookId: 1, position: 1 }, { unique: true, name: 'unique_chapter_position' });
  database.paragraphs.createIndex({ reviewerId: 1, bookId: 1, position: 1 }, { unique: true, name: 'unique_paragraph_position' });
  database.paragraphs.createIndex({ reviewerId: 1, bookId: 1, chapterId: 1, chapterPosition: 1 }, { unique: true, name: 'unique_chapter_paragraph_position' });
  database.importFiles.createIndex({ reviewerId: 1, chapterId: 1, fileName: 1 }, { unique: true, name: 'unique_import_file' });
  database.draftOperations.createIndex({ reviewerId: 1, bookId: 1, requestId: 1 }, { unique: true, name: 'unique_draft_request' });
  database.draftOperations.createIndex({ reviewerId: 1, bookId: 1, state: 1 }, { name: 'drafts_by_book_state' });
  database.schemaVersions.updateOne({ _id: 'translation-review-v1' }, { $setOnInsert: { appliedAt: new Date(), source: 'CAP Week 6 plus C# persistence contracts' } }, { upsert: true });
};
