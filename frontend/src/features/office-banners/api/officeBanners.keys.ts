export const officeBannersKeys = {
  all: ['officeBanners'] as const,
  list: () => [...officeBannersKeys.all, 'list'] as const,
}
