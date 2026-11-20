import { render, screen } from '@testing-library/react-native';
import React from 'react';

import ClubsScreen from '../../app/(tabs)/clubs';
import FeedScreen from '../../app/(tabs)/feed';
import MapScreen from '../../app/(tabs)/map';
import ProfileScreen from '../../app/(tabs)/profile';
import RecordScreen from '../../app/(tabs)/record';
import { TabScreenPlaceholder } from './TabScreenPlaceholder';

describe('TabScreenPlaceholder', () => {
  it('renders the localized title and subtitle for the map tab', () => {
    const titleKey = 'screens.map.title' as const;
    const subtitleKey = 'screens.map.subtitle' as const;

    render(<TabScreenPlaceholder titleKey={titleKey} subtitleKey={subtitleKey} />);

    expect(screen.getByText('Mapa de territorio')).toBeTruthy();
    expect(
      screen.getByText('Explora las zonas H3 de tu sector y planifica tu próxima ruta.'),
    ).toBeTruthy();
  });

  it('renders all five main tab screens with their Spanish titles', () => {
    const screens = [
      { component: <MapScreen />, expectedTitle: 'Mapa de territorio' },
      { component: <FeedScreen />, expectedTitle: 'Muro de actividad' },
      { component: <RecordScreen />, expectedTitle: 'Trackear actividad' },
      { component: <ClubsScreen />, expectedTitle: 'Clubes' },
      { component: <ProfileScreen />, expectedTitle: 'Mi perfil' },
    ];

    for (const item of screens) {
      const { unmount } = render(item.component);
      expect(screen.getByText(item.expectedTitle)).toBeTruthy();
      unmount();
    }
  });
});
