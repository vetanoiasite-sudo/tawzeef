export const employerProfileKeys = {
  myProfile: () => ['employerProfile', 'me'] as const,
  myOffices: () => ['employerProfile', 'me', 'offices'] as const,
}
