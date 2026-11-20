/**
 * Foundational design tokens for the mobile app (`docs/04-ESTANDARES-DESARROLLO.md` §5).
 * No raw colors, font sizes, or spacing values are permitted directly inside screen files.
 * Extended by the full design system in CTZ-10.
 */
export const colors = {
  background: {
    primary: '#0D1512',
    surface: '#16231E',
    elevated: '#1F312A',
  },
  text: {
    primary: '#F3F7F5',
    secondary: '#A6B8B0',
    muted: '#6E857B',
  },
  brand: {
    primary: '#2FC2AC',
    accent: '#FF6B4E',
  },
  border: {
    subtle: '#253931',
  },
} as const;

export const spacing = {
  xs: 4,
  sm: 8,
  md: 16,
  lg: 24,
  xl: 32,
  xxl: 48,
} as const;

export const typography = {
  size: {
    xs: 12,
    sm: 14,
    md: 16,
    lg: 20,
    xl: 24,
    xxl: 32,
  },
  weight: {
    regular: '400',
    medium: '500',
    semibold: '600',
    bold: '700',
  },
  lineHeight: {
    sm: 20,
    md: 24,
    lg: 28,
    xl: 32,
  },
} as const;

export const radius = {
  sm: 8,
  md: 12,
  lg: 16,
  full: 9999,
} as const;

// Minimum touch target required by docs/04-ESTANDARES-DESARROLLO.md §5 (44x44 pt)
export const layout = {
  minTouchTargetSize: 44,
  tabBarHeight: 64,
} as const;
