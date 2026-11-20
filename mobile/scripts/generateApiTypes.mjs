import { execFileSync } from 'node:child_process';
import { resolve } from 'node:path';
import process from 'node:process';

// Generates TypeScript types from the backend OpenAPI 3.1 contract (LLD §1.3, §3.1).
// Defaults to the local Docker Compose API; override with OPENAPI_URL for QA.
const openApiUrl = process.env.OPENAPI_URL ?? 'http://localhost:5080/openapi/v1.json';
const mobileRoot = resolve(import.meta.dirname, '..');
const outputPath = resolve(mobileRoot, 'src', 'api', 'schema.d.ts');
const cliPath = resolve(mobileRoot, 'node_modules', 'openapi-typescript', 'bin', 'cli.js');

process.stdout.write(`Generating API types from ${openApiUrl} -> ${outputPath}\n`);

execFileSync(process.execPath, [cliPath, openApiUrl, '-o', outputPath], {
  stdio: 'inherit',
});
