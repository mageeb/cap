import test from 'node:test';
import assert from 'node:assert/strict';
import { once } from 'node:events';
import { createServer } from 'node:http';
import { createParagraph, isDirty, acceptSave, createApi } from '../review.js';
import { createFrontendServer } from '../server.mjs';
const id = '11111111-1111-1111-1111-111111111111';
const paragraphId = '22222222-2222-2222-2222-222222222222';
const view = { id: paragraphId, position: 0, sourceText: 'مرحبا', machineDraft: 'Hello', reviewedText: null, version: '1' };
test('intentionally empty saved text takes precedence over machine draft', () => {
  assert.equal(createParagraph({ ...view, reviewedText: '' }).text, '');
});
test('a save acknowledgement preserves edits typed while the save was in flight', () => {
  const p = createParagraph(view); p.text = 'Newer local typing';
  acceptSave(p, { ...view, reviewedText: 'Submitted text', version: '2' });
  assert.equal(p.text, 'Newer local typing'); assert.equal(p.version, '2');
  assert.equal(p.savedText, 'Submitted text'); assert.equal(isDirty(p), true);
});
test('API conflict and dependency failure do not mutate the local paragraph', async () => {
  const p = createParagraph(view); p.text = 'My edit';
  for (const status of [409, 503]) {
    const api = createApi(async () => new Response('{}', { status }));
    await assert.rejects(api.save(id, p), error => error.status === status);
    assert.equal(p.text, 'My edit'); assert.equal(p.version, '1'); assert.equal(isDirty(p), true);
  }
});
async function listen(server) {
  server.listen(0, '127.0.0.1'); await once(server, 'listening');
  return 'http://127.0.0.1:' + server.address().port;
}
test('real HTTP proxy supports read, save, reload, and stale-version rejection', async t => {
  let stored = { ...view };
  const backend = createServer(async (req, res) => {
    res.setHeader('Content-Type', 'application/json');
    if (req.method === 'GET') return res.end(JSON.stringify({ book: { id, title: 'Seeded fixture' }, paragraphs: [stored] }));
    let body = ''; for await (const chunk of req) body += chunk;
    const update = JSON.parse(body);
    if (update.expectedVersion !== stored.version) { res.writeHead(409); return res.end('{}'); }
    stored = { ...stored, reviewedText: update.reviewedText, version: String(Number(stored.version) + 1) };
    res.end(JSON.stringify(stored));
  });
  t.after(() => backend.close());
  const backendUrl = await listen(backend);
  const frontend = createFrontendServer({ backendUrl }); t.after(() => frontend.close());
  const frontendUrl = await listen(frontend);
  const connection = await (await fetch(frontendUrl + '/connection.json')).json();
  assert.equal(connection.configured, true);
  assert.equal(connection.backendOrigin, backendUrl);
  const api = createApi((path, options) => fetch(frontendUrl + path, options));
  const p = createParagraph((await api.read(id)).paragraphs[0]); p.text = 'Corrected English';
  acceptSave(p, await api.save(id, p));
  assert.equal((await api.read(id)).paragraphs[0].reviewedText, 'Corrected English');
  assert.equal(p.version, '2'); assert.equal(isDirty(p), false);
  await assert.rejects(api.save(id, { ...p, version: '1' }), error => error.status === 409);
  // A deliberately blank correction must survive the wire too.
  p.text = ''; acceptSave(p, await api.save(id, p));
  assert.equal((await api.read(id)).paragraphs[0].reviewedText, '');
  const page = await fetch(frontendUrl); assert.equal(page.status, 200);
  assert.match(await page.text(), /Parallel translation review/);
  assert.equal((await fetch(frontendUrl + '/package.json')).status, 404);
  assert.equal((await fetch(frontendUrl + '/api/unexpected')).status, 404);
});
test('unconfigured backend is explicitly unavailable rather than a fake successful save', async t => {
  const server = createFrontendServer({ backendUrl: '' }); t.after(() => server.close());
  const url = await listen(server);
  assert.equal((await fetch(url + '/api/books/' + id)).status, 503);
});


test('proxy preserves Unicode when a character is split across HTTP body chunks', async t => {
  const { request } = await import('node:http');
  let received;
  const backend = createServer(async (req, res) => {
    const chunks = []; for await (const chunk of req) chunks.push(chunk);
    received = JSON.parse(Buffer.concat(chunks).toString('utf8'));
    res.setHeader('Content-Type', 'application/json'); res.end('{}');
  });
  t.after(() => backend.close());
  const server = createFrontendServer({ backendUrl: await listen(backend) }); t.after(() => server.close());
  const url = await listen(server);
  const body = Buffer.from(JSON.stringify({ reviewedText: 'مرحبا — café 🌍', expectedVersion: '1' }));
  const split = body.indexOf(Buffer.from('م')) + 1;
  await new Promise((resolve, reject) => {
    const req = request(url + '/api/books/' + id + '/paragraphs/' + paragraphId + '/review',
      { method: 'PUT', headers: { 'Content-Type': 'application/json' } },
      res => { res.resume(); res.on('end', resolve); });
    req.on('error', reject);
    req.write(body.subarray(0, split));
    setTimeout(() => req.end(body.subarray(split)), 20);
  });
  assert.equal(received.reviewedText, 'مرحبا — café 🌍');
});
