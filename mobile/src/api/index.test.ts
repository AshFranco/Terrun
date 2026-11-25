import type { ApiProblem, components, ErrorCode, paths } from './index';

describe('OpenAPI generated types', () => {
  it('exposes typed ApiProblem, ErrorCode union, and core endpoint schemas', () => {
    const problem: ApiProblem = {
      status: 409,
      code: 'HANDLE_TAKEN',
      traceId: '00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-01',
    };

    const validCodes: readonly ErrorCode[] = [
      'VALIDATION_ERROR',
      'INVALID_CREDENTIALS',
      'INVALID_TOKEN',
      'EMAIL_NOT_VERIFIED',
      'VERIFICATION_CODE_INVALID',
      'CONSENT_REQUIRED',
      'FORBIDDEN',
      'USER_SUSPENDED',
      'NOT_FOUND',
      'EMAIL_ALREADY_REGISTERED',
      'HANDLE_TAKEN',
      'ACTIVITY_OVERLAP',
      'CONCURRENCY_CONFLICT',
      'PAYLOAD_TOO_LARGE',
      'BBOX_TOO_LARGE',
      'BUSINESS_RULE_VIOLATION',
      'RATE_LIMITED',
      'INTERNAL_ERROR',
    ];

    // @ts-expect-error An undeclared error code outside the OpenAPI catalog must fail compilation
    const invalidCode: ErrorCode = 'UNKNOWN_ERROR_CODE';

    type MeGetSuccess = paths['/api/v1/me']['get']['responses'][200]['content']['application/json'];
    type ActivityDetail = components['schemas']['ActivityResponse'];

    const sampleSummaryPeriod: components['schemas']['SummaryPeriod'] = 'week';

    expect(problem.code).toBe('HANDLE_TAKEN');
    expect(validCodes).toHaveLength(18);
    expect(invalidCode).toBeDefined();
    expect(sampleSummaryPeriod).toBe('week');
    expectTypeMarker<MeGetSuccess>();
    expectTypeMarker<ActivityDetail>();
  });
});

function expectTypeMarker<_T>(): void {
  // Compile-time type assertion helper
}
