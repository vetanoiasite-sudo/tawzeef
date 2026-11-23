export const complaintKeys = {
  mine: (pageIndex: number, pageSize: number) =>
    ['complaints', 'mine', pageIndex, pageSize] as const,
}
