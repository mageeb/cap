// Serve the local app on an available port, draw once, and capture the result.
process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
const { chromium } = require('playwright');
const http = require('node:http');
const fs = require('node:fs');
const path = require('node:path');
const root = path.resolve(process.argv[2]);
const output = process.argv[3];
const mime = { '.html': 'text/html', '.js': 'text/javascript', '.css': 'text/css' };

const server = http.createServer((request, response) => {
  const file = path.resolve(root, '.' + new URL(request.url, 'http://localhost').pathname);
  const target = file === root ? path.join(root, 'index.html') : file;
  if (!target.startsWith(root + path.sep)) {
    response.writeHead(403); return response.end();
  }
  try {
    response.setHeader('Content-Type', mime[path.extname(target)] || 'text/plain');
    response.end(fs.readFileSync(target));
  } catch {
    response.writeHead(404); response.end();
  }
});

(async () => {
  // Port 0 asks the OS for an unused port, so separate fixtures can coexist.
  await new Promise(resolve => server.listen(0, '127.0.0.1', resolve));
  let browser;
  try {
    browser = await chromium.launch();
    const page = await browser.newPage({ viewport: { width: 1280, height: 900 } });
    const errors = [];
    page.on('pageerror', error => errors.push(error.message));
    await page.goto(`http://127.0.0.1:${server.address().port}`, { waitUntil: 'networkidle' });
    const canvas = page.locator('#paint');
    await canvas.waitFor({ state: 'visible' });
    const size = await canvas.evaluate(canvas => [canvas.width, canvas.height]);
    if (size[0] < 400 || size[1] < 240) throw Error('Canvas must be at least 400x240');

    // Compare actual canvas pixels before and after a pointer drag.
    const before = await canvas.evaluate(canvas => canvas.toDataURL());
    const box = await canvas.boundingBox();
    await page.mouse.move(box.x + 30, box.y + 30);
    await page.mouse.down();
    await page.mouse.move(box.x + 150, box.y + 80, { steps: 12 });
    await page.mouse.up();
    if (before === await canvas.evaluate(canvas => canvas.toDataURL())) {
      throw Error('Drag drew no pixels');
    }
    if (errors.length) throw Error(errors.join('\n'));
    await page.screenshot({ path: output, fullPage: true });
    console.log('PASS: visible canvas, default drag changes pixels, no page errors.');
  } finally {
    if (browser) await browser.close();
    await new Promise(resolve => server.close(resolve));
  }
})().catch(error => { console.error(error); process.exitCode = 1; });
