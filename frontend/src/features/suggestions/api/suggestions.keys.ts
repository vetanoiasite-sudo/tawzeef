export const suggestionsKeys = {
  all: ['suggestions'] as const,
  mine: (pageIndex: number, pageSize: number) => [...suggestionsKeys.all, 'mine', pageIndex, pageSize] as const,
}
