export const platformSettingsKeys = {
  all: ['platformSettings'] as const,
  list: () => [...platformSettingsKeys.all, 'list'] as const,
}
