// Run task 01 checks, then add 02 and 03 checks cumulatively.
// The screenshot is evidence from the real browser, even after a failed check.
process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
const { chromium } = require('playwright'), assert = require('node:assert/strict');
const http = require('node:http'), fs = require('node:fs'), path = require('node:path');
const root = path.resolve(process.argv[2]), task = Number(process.argv[3]), image = process.argv[4];
const server = http.createServer((req,res) => {
  const file = path.resolve(root,'.'+new URL(req.url,'http://localhost').pathname);
  const target = file === root ? path.join(root,'index.html') : file;
  if (!target.startsWith(root+path.sep)) { res.writeHead(403); return res.end(); }
  try { res.setHeader('Content-Type', {'.html':'text/html','.js':'text/javascript','.css':'text/css'}[path.extname(target)] || 'text/plain');
    res.end(fs.readFileSync(target)); } catch { res.writeHead(404); res.end(); }
});
(async () => {
  // Port 0 selects an unused local port for this check's short-lived server.
  await new Promise(r => server.listen(0,'127.0.0.1',r));
  let browser, page;
  try {
    browser = await chromium.launch(); page = await browser.newPage({viewport:{width:1280,height:900}});
    const errors = []; page.on('pageerror',e => errors.push(e.message));
    await page.goto(`http://127.0.0.1:${server.address().port}`,{waitUntil:'networkidle'});
    const value = id => page.locator('#'+id).inputValue();
    assert.equal(await value('tool'),'pencil'); assert.equal(await value('color'),'#152536');
    assert.deepEqual(await page.locator('#tool option').evaluateAll(a => a.map(x=>x.value)),['pencil','eraser']);
    assert.deepEqual(await page.locator('#color option').evaluateAll(a => a.map(x=>x.value)),['#152536','#188D91','#E8A64C']);
    const canvas = page.locator('#paint');
    async function draw() {
      const b = await canvas.evaluate(c => {const r=c.getBoundingClientRect(); return {x:r.x+c.clientLeft,y:r.y+c.clientTop,w:c.clientWidth,h:c.clientHeight,cw:c.width,ch:c.height};});
      await page.mouse.move(b.x+50*b.w/b.cw,b.y+60*b.h/b.ch); await page.mouse.down();
      await page.mouse.move(b.x+150*b.w/b.cw,b.y+60*b.h/b.ch,{steps:15}); await page.mouse.up();
    }
    const before = await canvas.evaluate(c => c.toDataURL()); await draw();
    assert.notEqual(await canvas.evaluate(c => c.toDataURL()),before,'Default drag drew nothing');
    // Task 02 adds reload persistence and verifies a real teal pencil stroke.
    if (task >= 2) {
      await page.selectOption('#tool','pencil'); await page.selectOption('#color','#188D91');
      await page.reload(); assert.equal(await value('tool'),'pencil'); assert.equal(await value('color'),'#188D91');
      await draw();
      assert.equal(await canvas.evaluate(c => {
        const p=c.getContext('2d').getImageData(100,60,1,1).data;
        return p[0]===24 && p[1]===141 && p[2]===145 && p[3]===255;
      }),true,'Pencil stroke is not teal');
    }
    // Task 03 preserves both earlier checks, then adds eraser and bad-data safety.
    if (task >= 3) {
      await page.selectOption('#tool','eraser'); await page.reload();
      assert.equal(await value('tool'),'eraser'); assert.equal(await value('color'),'#188D91');
      // Canvas pixels do not persist: draw a NEW stroke after reload, then erase it.
      await page.selectOption('#tool','pencil'); await draw();
      await page.selectOption('#tool','eraser'); const painted=await canvas.evaluate(c => c.toDataURL());
      await draw(); assert.notEqual(await canvas.evaluate(c => c.toDataURL()),painted,'Eraser did not change pixels');
      assert.equal(await canvas.evaluate(c => {
        const p=c.getContext('2d').getImageData(100,60,1,1).data;
        return p[3]===0 || (p[0]===255 && p[1]===255 && p[2]===255);
      }),true,'Eraser did not clear the stroke');
      for (const raw of ['{broken','null','[]','{"tool":"bad","color":"#188D91"}',
        '{"tool":"eraser","color":"purple"}']) {
        await page.evaluate(raw => localStorage.setItem('cap.paint.preferences.v1',raw),raw);
        await page.reload(); assert.equal(await value('tool'),'pencil'); assert.equal(await value('color'),'#152536');
      }
      assert.equal(await page.evaluate(async () => {
        const m = await import('./settings.js'), s = {getItem(){throw Error('denied');},setItem(){throw Error('denied');}};
        const p=m.loadPreferences(s); m.savePreferences(s,p);
        return p.tool==='pencil' && p.color==='#152536';
      }),true,'Denied storage not safe');
    }
    assert.deepEqual(errors,[]);
    // A successful task requires its screenshot; capture errors fail the gate.
    await page.screenshot({path:image,fullPage:true});
    console.log(`PASS task ${String(task).padStart(2,'0')}: cumulative browser criteria.`);
  } catch (error) {
    // Preserve best-effort failure evidence without replacing the original error.
    if (page) await page.screenshot({path:image,fullPage:true}).catch(()=>{});
    throw error;
  } finally {
    try { if (browser) await browser.close(); }
    finally { await new Promise(r => server.close(r)); }
  }
})().catch(e => {console.error(e);process.exitCode=1;});
