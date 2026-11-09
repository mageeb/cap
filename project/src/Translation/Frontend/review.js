export const isBookId = value => /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i.test(value);
export function createParagraph(view) {
  return { ...view, text: view.reviewedText ?? view.machineDraft ?? '',
    savedText: view.reviewedText ?? view.machineDraft ?? '', saving: false, error: '', conflict: null };
}
export const isDirty = paragraph => paragraph.text !== paragraph.savedText;
// A reviewer may type again while a save is in flight. Preserve the newer text.
export function acceptSave(paragraph, returned) {
  paragraph.savedText = returned.reviewedText ?? returned.machineDraft ?? '';
  paragraph.version = returned.version;
  paragraph.reviewedText = returned.reviewedText;
  paragraph.error = '';
  paragraph.conflict = null;
}
export class ApiError extends Error {
  constructor(message, status) { super(message); this.status = status; }
}
export function createApi(fetcher = fetch) {
  async function request(path, options = {}) {
    const response = await fetcher(path, { ...options, headers: { 'Content-Type': 'application/json' } });
    if (!response.ok) {
      const messages = {
        400: 'The document or edit was rejected. Check the document ID and try again.',
        401: 'The backend requires authentication. This exercise needs a backend configured for anonymous demo access.',
        403: 'The backend denied access to this document.',
        404: 'This document or paragraph was not found.',
        409: 'This paragraph changed on the server. Your edit has been kept.',
        503: 'The backend is unavailable or its database services are not configured.'
      };
      throw new ApiError(messages[response.status] ?? 'The request failed. Your local edits have been kept.', response.status);
    }
    return response.json();
  }
  return {
    read: bookId => request('/api/books/' + encodeURIComponent(bookId)),
    save: (bookId, paragraph) => request('/api/books/' + encodeURIComponent(bookId) + '/paragraphs/' + encodeURIComponent(paragraph.id) + '/review', {
      method: 'PUT', body: JSON.stringify({ reviewedText: paragraph.text, expectedVersion: paragraph.version })
    })
  };
}
