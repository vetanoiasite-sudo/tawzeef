import type { ComponentType, LazyExoticComponent } from 'react'

export type RouteAccess = { public: true } | { public?: false; permission: string }

export type AppRouteDef = {
  path: string
  Component: LazyExoticComponent<ComponentType>
  access: RouteAccess
  nav?: { labelKey: string }
}
