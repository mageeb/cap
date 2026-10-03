// Human-owned browser gate: reload preferences, draw real pixels, erase,
// and recover safely from malformed storage. Screenshots record each result.
process.env.PLAYWRIGHT_BROWSERS_PATH = '0';
const { chromium } = await import('playwright');
import assert from 'node:assert/strict';
import fs from 'node:fs';
const [url, out] = process.argv.slice(2);
fs.mkdirSync(out, {recursive:true});
const browser = await chromium.launch({headless:true});
try {
  const page = await browser.newPage({viewport:{width:900,height:500}});
  await page.goto(url);
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.getByLabel('Tool', {exact:true}).count(), 1);
  assert.equal(await page.getByLabel('Color', {exact:true}).count(), 1);
  // Choose teal, reload, and confirm the preference survived.
  await page.selectOption('#tool', 'pencil');
  await page.selectOption('#color', '#188D91');
  await page.reload();
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.locator('#tool').inputValue(), 'pencil');
  assert.equal(await page.locator('#color').inputValue(), '#188D91');
  async function dragStroke() {
    const box = await page.locator('#paint').boundingBox();
    await page.mouse.move(box.x + 41, box.y + 41);
    await page.mouse.down();
    await page.mouse.move(box.x + 111, box.y + 41, {steps:8});
    await page.mouse.up();
  }
  const readPixel = () => page.locator('#paint').evaluate(c => [...c.getContext('2d').getImageData(75,40,1,1).data]);
  const assertTeal = pixel => { assert.deepEqual(pixel.slice(0,3), [24,141,145]); assert.equal(pixel[3],255); };
  // Check a canvas pixel, not just the text shown in the toolbar.
  await dragStroke();
  assertTeal(await readPixel());
  await page.screenshot({path:`${out}/pencil-teal-reload.png`});
  await page.selectOption('#tool', 'eraser');
  await page.reload();
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.locator('#tool').inputValue(), 'eraser');
  assert.equal(await page.locator('#color').inputValue(), '#188D91');
  // Canvas contents are not persisted: draw a new stroke after this reload.
  await page.selectOption('#tool', 'pencil');
  await dragStroke(); assertTeal(await readPixel());
  await page.selectOption('#tool', 'eraser');
  await dragStroke();
  const erased = await readPixel();
  assert.ok(erased[3] === 0 || erased.every(v => v === 255), `Expected white/transparent erased pixel: ${erased}`);
  await page.screenshot({path:`${out}/eraser-after-reload.png`});
  // Malformed saved data must restore both defaults without breaking startup.
  await page.evaluate(() => localStorage.setItem('cap.paint.preferences.v1', '{broken'));
  await page.reload();
  await page.waitForFunction(() => window.appReady === true);
  assert.equal(await page.locator('#tool').inputValue(), 'pencil');
  assert.equal(await page.locator('#color').inputValue(), '#152536');
  await page.screenshot({path:`${out}/invalid-state-defaults.png`});
  console.log(JSON.stringify({reload:'passed',drawing:'passed',eraser:'passed',invalid_state:'passed'}));
} finally { await browser.close(); }
