export const officeProfileKeys = {
  myOffice: () => ['officeProfile', 'me'] as const,
  publicOffice: (id: string) => ['officeProfile', 'public', id] as const,
}
