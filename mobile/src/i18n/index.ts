import es from './es.json';

type TranslationDictionary = typeof es;

type NestedKeyOf<T> = T extends string
  ? never
  : {
      [K in keyof T & string]: T[K] extends string ? K : `${K}.${NestedKeyOf<T[K]>}`;
    }[keyof T & string];

export type TranslationKey = NestedKeyOf<TranslationDictionary>;

export type TranslationParams = Readonly<Record<string, string | number>>;

function resolvePath(dictionary: Record<string, unknown>, key: string): string {
  const segments = key.split('.');
  let current: unknown = dictionary;

  for (const segment of segments) {
    if (typeof current !== 'object' || current === null || !(segment in current)) {
      return key;
    }
    current = (current as Record<string, unknown>)[segment];
  }

  return typeof current === 'string' ? current : key;
}

/**
 * Resolves a localized Spanish string by its dot-separated key (`docs/04-ESTANDARES-DESARROLLO.md` §5).
 */
export function t(key: TranslationKey, params?: TranslationParams): string {
  const template = resolvePath(es as Record<string, unknown>, key);

  if (!params) {
    return template;
  }

  return template.replace(/\{\{(\w+)\}\}/g, (_match, token: string) => {
    const value = params[token];
    return value !== undefined ? String(value) : `{{${token}}}`;
  });
}

export { es };
