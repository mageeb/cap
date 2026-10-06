import { chromium } from 'playwright';

const [url, output] = process.argv.slice(2);
const browser = await chromium.launch({ headless: true });
try {
  const context = await browser.newContext({ viewport: { width: 1280, height: 720 } });
  const page = await context.newPage();
  await page.goto(url);
  const canvas = page.locator('canvas#paint');
  const box = await canvas.isVisible() ? await canvas.boundingBox() : null;
  if (box) {
    await page.mouse.move(box.x + box.width * 0.2, box.y + box.height * 0.7);
    await page.mouse.down();
    await page.mouse.move(box.x + box.width * 0.4, box.y + box.height * 0.3, { steps: 10 });
    await page.mouse.move(box.x + box.width * 0.6, box.y + box.height * 0.65, { steps: 10 });
    await page.mouse.move(box.x + box.width * 0.8, box.y + box.height * 0.4, { steps: 10 });
    await page.mouse.up();
  }
  await page.screenshot({ path: output, fullPage: true });
} finally {
  await browser.close();
}
