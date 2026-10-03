// A report is the agent's claim. checks.txt is the controller's observation.
const fs = require('node:fs');
const [directory, id] = process.argv.slice(2);
const report = JSON.parse(fs.readFileSync(`${directory}/report.json`, 'utf8'));
for (const key of ['implemented', 'envisioned', 'limitations']) {
  if (typeof report[key] !== 'string' || !report[key].trim()) {
    throw Error(`Missing ${key} in the agent report`);
  }
}
const checks = fs.readFileSync(`${directory}/checks.txt`, 'utf8').trim();
fs.appendFileSync('ledger.md', `\n## ${id}\n\n` +
  `Agent implemented: ${report.implemented}\n\n` +
  `Agent envisioned: ${report.envisioned}\n\n` +
  `Agent limitations: ${report.limitations}\n\n` +
  `Controller observed: ${checks}\n\n` +
  `Screenshot: ${directory}/screen.png. Product acceptance: pending human inspection.\n`);
