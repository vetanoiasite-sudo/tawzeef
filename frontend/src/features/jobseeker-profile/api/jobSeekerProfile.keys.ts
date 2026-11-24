export const jobSeekerProfileKeys = {
  myProfile: () => ['jobSeekerProfile', 'me'] as const,
  myOffices: () => ['jobSeekerProfile', 'me', 'offices'] as const,
}
