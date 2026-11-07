import { createServer } from 'node:http';
import { readFile } from 'node:fs/promises';
import { fileURLToPath } from 'node:url';
import { resolve, dirname } from 'node:path';
const root = dirname(fileURLToPath(import.meta.url));
const assets = new Map([
  ['/', ['index.html', 'text/html; charset=utf-8']],
  ['/index.html', ['index.html', 'text/html; charset=utf-8']],
  ['/styles.css', ['styles.css', 'text/css; charset=utf-8']],
  ...['app.js','review.js','sample.js'].map(name => ['/' + name, [name, 'text/javascript; charset=utf-8']])
]);
export function createFrontendServer({ backendUrl = process.env.BACKEND_URL } = {}) {
  const backend = backendUrl ? new URL(backendUrl) : null;
  if (backend && !['http:', 'https:'].includes(backend.protocol)) throw new Error('BACKEND_URL must use HTTP or HTTPS.');
  return createServer(async (req, res) => {
    const url = new URL(req.url, 'http://localhost');
    res.setHeader('Cache-Control', 'no-store');
    res.setHeader('X-Content-Type-Options', 'nosniff');
    if (url.pathname === '/connection.json' && req.method === 'GET') {
      res.writeHead(200, { 'Content-Type': 'application/json' });
      res.end(JSON.stringify({ configured: !!backend, backendOrigin: backend?.origin ?? null }));
      return;
    }
    if (url.pathname.startsWith('/api/')) {
      const read = /^\/api\/books\/[0-9a-f-]{36}$/i.test(url.pathname) && req.method === 'GET';
      const save = /^\/api\/books\/[0-9a-f-]{36}\/paragraphs\/[0-9a-f-]{36}\/review$/i.test(url.pathname) && req.method === 'PUT';
      if (!read && !save) { res.writeHead(404).end(); return; }
      if (!backend) {
        res.writeHead(503, { 'Content-Type': 'application/problem+json' });
        res.end(JSON.stringify({ title: 'Set BACKEND_URL to connect the review API.', code: 'frontend_backend_not_configured' }));
        return;
      }
      try {
        req.setEncoding('utf8'); // Stream decoder preserves characters split across chunks.
        let body = '';
        for await (const chunk of req) {
          body += chunk;
          if (Buffer.byteLength(body) > 1048576) { res.writeHead(413).end(); return; }
        }
        const upstream = await fetch(new URL(url.pathname, backend), {
          method: req.method, headers: { 'Content-Type': 'application/json' },
          body: save ? body : undefined, signal: AbortSignal.timeout(15000), redirect: 'error'
        });
        res.writeHead(upstream.status, { 'Content-Type': upstream.headers.get('Content-Type') ?? 'application/json' });
        res.end(await upstream.text());
      } catch {
        if (!res.headersSent) res.writeHead(503, { 'Content-Type': 'application/problem+json' });
        res.end(JSON.stringify({ title: 'Backend connection failed.', code: 'backend_unavailable' }));
      }
      return;
    }
    const asset = assets.get(url.pathname);
    if (!asset || !['GET','HEAD'].includes(req.method)) { res.writeHead(404).end(); return; }
    try {
      const content = await readFile(resolve(root, asset[0]));
      res.writeHead(200, { 'Content-Type': asset[1] });
      res.end(req.method === 'HEAD' ? undefined : content);
    } catch { res.writeHead(500).end('Could not load frontend asset.'); }
  });
}
if (process.argv[1] && resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
  const port = Number(process.env.PORT ?? 4173);
  const server = createFrontendServer();
  server.listen(port, '127.0.0.1', () => console.log('Translation workspace: http://127.0.0.1:' + port));
}
