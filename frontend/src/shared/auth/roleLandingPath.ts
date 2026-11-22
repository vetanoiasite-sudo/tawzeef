/** Where a role lands right after login/registration — a real dashboard for the two
 * self-service roles (so logging in visibly changes the page), the homepage for everyone else
 * (office staff keep their permission-gated nav links instead). */
export function roleLandingPath(role: string): string {
  switch (role) {
    case 'SuperAdmin':
      return '/super-admin/dashboard'
    case 'JobSeeker':
      return '/dashboard'
    case 'Employer':
      return '/employer/dashboard'
    case 'OfficeAdmin':
    case 'OfficeStaff':
      return '/office/dashboard'
    default:
      return '/'
  }
}
