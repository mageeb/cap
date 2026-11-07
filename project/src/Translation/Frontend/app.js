import { createApi, createParagraph, isDirty, isBookId, acceptSave } from './review.js';
import { sample } from './sample.js';
const api = createApi();
const $ = id => document.getElementById(id);
let book = null, paragraphs = [], preview = false, loading = false, batchSaving = false;
const rows = new Map();
const hasChanges = () => paragraphs.some(isDirty);
const isSaving = () => paragraphs.some(p => p.saving);
function message(text) { $('message').textContent = text; }
function showError(text = '') { $('error').textContent = text; $('error').hidden = !text; }
function element(tag, className, text) {
  const node = document.createElement(tag);
  if (className) node.className = className;
  if (text !== undefined) node.textContent = text;
  return node;
}
function update() {
  const dirty = paragraphs.filter(isDirty).length;
  $('save-all').disabled = preview || !dirty || isSaving() || loading || batchSaving;
  $('save-summary').textContent = preview ? 'Sample preview · saving disabled' :
    isSaving() ? 'Saving changes…' : dirty ? dirty + ' unsaved paragraph' + (dirty === 1 ? '' : 's') :
    book ? 'All changes saved' : 'No document loaded';
  $('open-button').disabled = loading || isSaving() || batchSaving;
  $('sample-button').disabled = loading || isSaving() || batchSaving;
  for (const p of paragraphs) {
    const row = rows.get(p.id);
    if (!row) continue;
    row.badge.textContent = p.saving ? 'Saving…' : p.error ? 'Needs attention' :
      isDirty(p) ? 'Unsaved changes' : p.reviewedText !== null ? 'Saved correction' : 'Machine draft';
    row.badge.classList.toggle('dirty', isDirty(p) || !!p.error);
    row.editor.disabled = loading;
    for (const button of row.section.querySelectorAll('.conflict button')) button.disabled = loading;
    row.save.disabled = loading || preview || !isDirty(p) || p.saving || !!p.conflict || batchSaving;
    row.note.textContent = p.error || (preview ? 'Preview edits are not persisted.' : 'English · editable');
  }
}
function canReplace() {
  if (isSaving() || loading || batchSaving) return false;
  return !hasChanges() || window.confirm('Discard your unsaved edits and open another document?');
}
function display(data, isPreview) {
  book = data.book; preview = isPreview;
  paragraphs = [...data.paragraphs].sort((a,b) => a.position - b.position).map(createParagraph);
  rows.clear(); $('paragraphs').replaceChildren();
  $('document-title').textContent = book.title;
  $('document-label').textContent = preview ? 'Sample workspace' : 'Review desk';
  $('paragraph-count').textContent = paragraphs.length + ' paragraphs';
  $('document-description').textContent = 'Read the Arabic source. Refine the English. Keep the meaning.';
  for (const [index, p] of paragraphs.entries()) {
    const section = element('article', 'paragraph');
    const meta = element('div', 'paragraph-meta');
    meta.append(element('span', 'paragraph-number', 'PARAGRAPH ' + String(index + 1).padStart(2,'0')));
    const badge = element('span', 'paragraph-state'); meta.append(badge);
    const body = element('div', 'paragraph-body');
    const translation = element('div', 'translation');
    const label = element('label', '', 'English translation, paragraph ' + (index + 1));
    const editor = element('textarea'); editor.id = 'editor-' + index; label.htmlFor = editor.id;
    editor.value = p.text; editor.dir = 'ltr'; editor.lang = 'en'; editor.spellcheck = true;
    editor.addEventListener('input', () => { p.text = editor.value; update(); });
    const actions = element('div', 'paragraph-actions');
    const note = element('span'); note.id = 'note-' + index; note.setAttribute('aria-live', 'polite');
    editor.setAttribute('aria-describedby', note.id);
    const save = element('button', '', 'Save paragraph'); save.type = 'button';
    save.setAttribute('aria-label', 'Save paragraph ' + (index + 1));
    save.addEventListener('click', () => saveParagraph(p));
    actions.append(note, save); translation.append(label, editor, actions);
    const source = element('p', 'source', p.sourceText); source.dir = 'rtl'; source.lang = 'ar';
    source.setAttribute('aria-label', 'Arabic source, paragraph ' + (index + 1));
    body.append(translation, source); section.append(meta, body);
    $('paragraphs').append(section); rows.set(p.id, { section, badge, editor, save, note });
  }
  if (!paragraphs.length) $('paragraphs').append(element('div', 'empty-state', 'This document has no paragraphs yet.'));
  showError();
  message(preview ? 'Sample preview only. Edits stay on this page; connect a seeded backend document to save.' : 'Document loaded from the backend. Changes are saved only when you choose Save.');
  update();
}
async function openBook(id) {
  if (!isBookId(id)) { showError('Enter a valid document UUID.'); return; }
  if (!canReplace()) return;
  loading = true; update(); showError(); message('Loading document…');
  try {
    const data = await api.read(id);
    display(data, false);
    const url = new URL(location.href); url.search = ''; url.searchParams.set('book', id);
    history.replaceState(null, '', url); $('book-id').value = id;
  } catch (error) {
    showError(error.message.startsWith('The ') || error.status ? error.message : 'Could not load the document. Check the backend connection and try again.');
    message(book ? 'Your current document and edits are still available.' : 'You can explore the sample while the backend is being configured.');
  } finally { loading = false; update(); }
}
function showConflict(p) {
  const row = rows.get(p.id);
  row.section.querySelector('.conflict')?.remove();
  const box = element('div', 'conflict');
  box.append(element('strong', '', 'Another version was saved. Your edit is still in the editor.'));
  const latest = p.conflict;
  box.append(element('p', '', 'Latest saved text:'), element('pre', '', latest.reviewedText ?? latest.machineDraft ?? ''));
  const useServer = element('button', 'secondary', 'Use saved version');
  const keepMine = element('button', 'secondary', 'Keep my edit');
  const resolve = keep => {
    p.version = latest.version;
    p.savedText = latest.reviewedText ?? latest.machineDraft ?? '';
    p.reviewedText = latest.reviewedText;
    if (!keep) { p.text = p.savedText; row.editor.value = p.text; }
    p.error = ''; p.conflict = null; box.remove(); update();
    message(keep ? 'Your edit is ready to save against the latest version. Choose Save to apply it.' : 'The latest saved version is now in the editor.');
  };
  useServer.addEventListener('click', () => resolve(false));
  keepMine.addEventListener('click', () => resolve(true));
  box.append(useServer, keepMine); row.section.append(box);
}
async function saveParagraph(p) {
  if (loading || preview || p.saving || !isDirty(p) || p.conflict) return;
  p.saving = true; p.error = ''; update(); showError();
  try {
    const returned = await api.save(book.id, p);
    acceptSave(p, returned);
    message(isDirty(p) ? 'Saved. Your newer edits still need saving.' : 'Paragraph saved to the backend.');
  } catch (error) {
    p.error = error.status ? error.message : 'Connection failed. Your edit is still here; try saving again.';
    if (error.status === 409) {
      try {
        const fresh = await api.read(book.id);
        p.conflict = fresh.paragraphs.find(other => other.id === p.id) ?? null;
        if (p.conflict) showConflict(p);
        else p.error = 'The paragraph was removed. Copy your local edit before reopening the document.';
      } catch { p.error += ' Could not retrieve the latest version. Try saving again when connected.'; }
    }
  } finally { p.saving = false; update(); }
}
$('open-form').addEventListener('submit', event => { event.preventDefault(); openBook($('book-id').value.trim()); });
$('sample-button').addEventListener('click', () => {
  if (canReplace()) {
    display(structuredClone(sample), true);
    const url = new URL(location.href); url.search = ''; url.searchParams.set('preview', '1');
    history.replaceState(null, '', url);
  }
});
$('save-all').addEventListener('click', async () => {
  if (preview || loading || isSaving() || batchSaving) return;
  batchSaving = true; update();
  try { for (const p of paragraphs.filter(isDirty)) await saveParagraph(p); }
  finally { batchSaving = false; update(); }
});
document.addEventListener('keydown', event => {
  const index = paragraphs.findIndex(p => rows.get(p.id).editor === document.activeElement);
  if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === 's') {
    event.preventDefault();
    if (index >= 0 && !batchSaving) saveParagraph(paragraphs[index]);
    else if (!$('save-all').disabled) $('save-all').click();
  }
  if (event.altKey && ['ArrowDown','ArrowUp'].includes(event.key) && index >= 0) {
    event.preventDefault();
    const next = Math.max(0, Math.min(paragraphs.length - 1, index + (event.key === 'ArrowDown' ? 1 : -1)));
    rows.get(paragraphs[next].id).editor.focus();
  }
});
window.addEventListener('beforeunload', event => {
  if (hasChanges() || isSaving()) { event.preventDefault(); event.returnValue = ''; }
});
const params = new URLSearchParams(location.search);
if (params.has('book')) { $('book-id').value = params.get('book'); openBook(params.get('book')); }
else if (params.get('preview') === '1') display(structuredClone(sample), true);

try {
  const response = await fetch('/connection.json');
  if (!response.ok) throw new Error('Configuration unavailable');
  const connection = await response.json();
  $('backend-target').textContent = connection.configured
    ? 'API target: ' + connection.backendOrigin
    : 'No backend configured. Start the DEV workspace to use the DEV tunnel.';
} catch {
  $('backend-target').textContent = 'Could not read the backend connection configuration.';
}
