/** Where a role lands right after login/registration — a real dashboard for the two
 * self-service roles (so logging in visibly changes the page), the homepage for everyone else
 * (office staff keep their permission-gated nav links instead). */
export function roleLandingPath(role: string): string {
  switch (role) {
    case 'JobSeeker':
      return '/dashboard'
    case 'Employer':
      return '/employer/dashboard'
    default:
      return '/'
  }
}
