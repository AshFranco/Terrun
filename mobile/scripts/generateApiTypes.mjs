import { execFileSync } from 'node:child_process';
import { resolve } from 'node:path';
import process from 'node:process';

// Generates TypeScript types from the backend OpenAPI 3.1 contract (LLD §1.3, §3.1).
// Defaults to the local Docker Compose API; override with OPENAPI_URL for QA.
const openApiUrl = process.env.OPENAPI_URL ?? 'http://localhost:5080/openapi/v1.json';
const outputPath = resolve(import.meta.dirname, '..', 'src', 'api', 'schema.d.ts');
const npxBinary = process.platform === 'win32' ? 'npx.cmd' : 'npx';

process.stdout.write(`Generating API types from ${openApiUrl} -> ${outputPath}\n`);

execFileSync(npxBinary, ['openapi-typescript', openApiUrl, '-o', outputPath], {
  stdio: 'inherit',
});
