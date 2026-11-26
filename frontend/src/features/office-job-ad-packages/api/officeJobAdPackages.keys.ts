export const officeJobAdPackagesKeys = {
  all: ['officeJobAdPackages'] as const,
  list: () => [...officeJobAdPackagesKeys.all, 'list'] as const,
}
