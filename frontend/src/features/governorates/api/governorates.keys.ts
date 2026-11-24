export const governoratesKeys = {
  all: ['governorates'] as const,
  list: () => [...governoratesKeys.all, 'list'] as const,
}
