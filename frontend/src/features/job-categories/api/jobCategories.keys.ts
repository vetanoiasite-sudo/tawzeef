export const jobCategoriesKeys = {
  all: ['jobCategories'] as const,
  list: () => [...jobCategoriesKeys.all, 'list'] as const,
}
