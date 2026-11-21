import React from 'react';
import { StyleSheet, Text, View } from 'react-native';

import { t, type TranslationKey } from '@/i18n';
import { colors, radius, spacing, typography } from '@/theme';

export type TabScreenPlaceholderProps = {
  titleKey: TranslationKey;
  subtitleKey: TranslationKey;
};

export function TabScreenPlaceholder({
  titleKey,
  subtitleKey,
}: TabScreenPlaceholderProps): React.JSX.Element {
  return (
    <View style={styles.container}>
      <View style={styles.card}>
        <Text style={styles.badge}>{t('app.name')}</Text>
        <Text style={styles.title}>{t(titleKey)}</Text>
        <Text style={styles.subtitle}>{t(subtitleKey)}</Text>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: colors.background.primary,
    alignItems: 'center',
    justifyContent: 'center',
    padding: spacing.lg,
  },
  card: {
    width: '100%',
    backgroundColor: colors.background.surface,
    borderRadius: radius.lg,
    borderWidth: 1,
    borderColor: colors.border.subtle,
    padding: spacing.xl,
    alignItems: 'center',
    gap: spacing.sm,
  },
  badge: {
    color: colors.brand.primary,
    fontSize: typography.size.xs,
    fontWeight: typography.weight.bold,
    letterSpacing: 1,
    textTransform: 'uppercase',
  },
  title: {
    color: colors.text.primary,
    fontSize: typography.size.xl,
    fontWeight: typography.weight.bold,
    lineHeight: typography.lineHeight.xl,
    textAlign: 'center',
  },
  subtitle: {
    color: colors.text.secondary,
    fontSize: typography.size.md,
    fontWeight: typography.weight.regular,
    lineHeight: typography.lineHeight.md,
    textAlign: 'center',
  },
});
