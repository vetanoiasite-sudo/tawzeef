/**
 * Shared title + subtitle block that sits inside an AuthScaffold card. Keeps
 * typography identical across login and the three registration flows.
 */
export function AuthHeader({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <div className="mb-6 flex flex-col items-center gap-1 text-center">
      <h1 className="text-[28px] font-bold leading-[32px] text-content-primary">{title}</h1>
      {subtitle && <p className="text-[14px] leading-[20px] text-content-secondary">{subtitle}</p>}
    </div>
  )
}
