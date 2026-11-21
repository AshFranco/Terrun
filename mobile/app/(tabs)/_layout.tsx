import { Ionicons } from '@expo/vector-icons';
import { Tabs } from 'expo-router';
import React from 'react';

import { t } from '@/i18n';
import { colors, layout, typography } from '@/theme';

export default function TabsLayout(): React.JSX.Element {
  return (
    <Tabs
      screenOptions={{
        headerStyle: {
          backgroundColor: colors.background.surface,
        },
        headerTitleStyle: {
          color: colors.text.primary,
          fontSize: typography.size.lg,
          fontWeight: typography.weight.semibold,
        },
        headerShadowVisible: false,
        tabBarActiveTintColor: colors.brand.primary,
        tabBarInactiveTintColor: colors.text.muted,
        tabBarStyle: {
          backgroundColor: colors.background.surface,
          borderTopColor: colors.border.subtle,
          height: layout.tabBarHeight,
        },
        tabBarItemStyle: {
          minHeight: layout.minTouchTargetSize,
        },
        tabBarLabelStyle: {
          fontSize: typography.size.xs,
          fontWeight: typography.weight.medium,
        },
      }}
    >
      <Tabs.Screen
        name="map"
        options={{
          title: t('tabs.map'),
          tabBarAccessibilityLabel: t('tabs.accessibility.map'),
          tabBarIcon: ({ color, size }) => (
            <Ionicons name="map-outline" size={size} color={color} />
          ),
        }}
      />
      <Tabs.Screen
        name="feed"
        options={{
          title: t('tabs.feed'),
          tabBarAccessibilityLabel: t('tabs.accessibility.feed'),
          tabBarIcon: ({ color, size }) => (
            <Ionicons name="newspaper-outline" size={size} color={color} />
          ),
        }}
      />
      <Tabs.Screen
        name="record"
        options={{
          title: t('tabs.record'),
          tabBarAccessibilityLabel: t('tabs.accessibility.record'),
          tabBarIcon: ({ color, size }) => (
            <Ionicons name="radio-button-on-outline" size={size} color={color} />
          ),
        }}
      />
      <Tabs.Screen
        name="clubs"
        options={{
          title: t('tabs.clubs'),
          tabBarAccessibilityLabel: t('tabs.accessibility.clubs'),
          tabBarIcon: ({ color, size }) => (
            <Ionicons name="people-outline" size={size} color={color} />
          ),
        }}
      />
      <Tabs.Screen
        name="profile"
        options={{
          title: t('tabs.profile'),
          tabBarAccessibilityLabel: t('tabs.accessibility.profile'),
          tabBarIcon: ({ color, size }) => (
            <Ionicons name="person-outline" size={size} color={color} />
          ),
        }}
      />
    </Tabs>
  );
}
