import type { components, operations, paths } from './schema';

export type { components, operations, paths };

/**
 * Standard RFC 7807 error payload returned by the Terrun API (`LLD §3.4`, `CTZ-106`).
 */
export type ApiProblem = components['schemas']['ApiProblem'];

/**
 * Exhaustive union of domain error codes declared in the OpenAPI contract.
 */
export type ErrorCode = ApiProblem['code'];
