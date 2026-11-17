import { t } from './index';

describe('t', () => {
  it('returns the Spanish label for each main navigation tab', () => {
    const expectedLabels = ['Mapa', 'Feed', 'Trackear', 'Clubes', 'Perfil'];

    const actualLabels = [
      t('tabs.map'),
      t('tabs.feed'),
      t('tabs.record'),
      t('tabs.clubs'),
      t('tabs.profile'),
    ];

    expect(actualLabels).toEqual(expectedLabels);
  });

  it('returns the application tagline from nested keys', () => {
    const key = 'app.tagline' as const;

    const result = t(key);

    expect(result).toBe('Conquista tu zona · Cada carrera suma territorio');
  });
});
