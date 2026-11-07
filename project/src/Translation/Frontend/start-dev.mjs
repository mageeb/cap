import { readFile } from 'node:fs/promises';
import { createFrontendServer } from './server.mjs';
const config = JSON.parse(await readFile(new URL('./dev.config.json', import.meta.url), 'utf8'));
const backendUrl = process.env.BACKEND_URL || config.backendUrl;
const port = Number(process.env.PORT || config.port);
const server = createFrontendServer({ backendUrl });
server.listen(port, '127.0.0.1', () => {
  console.log('DEV workspace: http://127.0.0.1:' + port);
  console.log('DEV API via tunnel: ' + new URL(backendUrl).origin);
  console.log('Keep the existing AKS port-forward on port 18081 running.');
});
