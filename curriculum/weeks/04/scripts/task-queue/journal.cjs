// Called only after the cumulative browser criterion passes.
const fs = require('node:fs');
const [directory, task] = process.argv.slice(2);
const report = JSON.parse(fs.readFileSync(`${directory}/report.json`, 'utf8'));
for (const key of ['implemented', 'envisioned', 'limitations']) {
  if (typeof report[key] !== 'string' || !report[key].trim()) {
    throw Error(`Missing ${key} in the agent report`);
  }
}
const checks = fs.readFileSync(`${directory}/checks.txt`, 'utf8').trim();
fs.appendFileSync('ledger.md', `\n## Task ${task}\n\n` +
  `Agent implemented: ${report.implemented}\n\n` +
  `Agent envisioned: ${report.envisioned}\n\n` +
  `Agent limitations: ${report.limitations}\n\n` +
  `Controller observed: ${checks}\n\n` +
  `Evidence: ${directory}/screen.png. Human acceptance pending.\n`);
