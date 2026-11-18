# Tawzeef brand assets

Exported from the logo design canvas on 2026-11-18 — the approved direction: a cross for the
letter **T**, a tilted briefcase sitting on top of it, and a small raised flag for the letter
**F**, bookending the wordmark (foundation → outcome).

## Files

| File | Use |
|---|---|
| `favicon.svg` | Icon-only mark (cross + briefcase, no text). Pure vector shapes — no font dependency. Use for the browser favicon, app icon, and anywhere the mark needs to work at a very small size. |
| `logo-wordmark.svg` | Full "TAWZEEF" lockup with the cross-T and flag-F. Built with an embedded `<foreignObject>` + CSS flex layout, matching the approved design pixel-for-pixel. |

## Colors

| Role | Hex | Used for |
|---|---|---|
| `cross` | `#072F5F` | The cross shape (T) only |
| `primary` | `#0A66C2` | The plain letters (A, W, Z, E, E) and the flag's pole |
| `accent` | `#70B5F9` | The briefcase on the cross, the flag's main pennant, and the underline bar |

These are also the three `Tweaks` color props on the live design canvas artboards, if further color
exploration is wanted before this is locked in.

## Typography

- **Space Grotesk**, weight 700, for the plain letters — loaded from Google Fonts
  (`family=Space+Grotesk:wght@700`).
- The cross (T) and the flag (F) are hand-drawn vector shapes, not font glyphs.

## Important: `<foreignObject>` + web font caveat

`logo-wordmark.svg` uses live HTML `<text>`-equivalent (styled `<span>`s inside a
`<foreignObject>`), not outlined vector paths. This renders correctly:

- Inlined directly in a web page (React component, `<object>`, or pasted inline `<svg>`).
- **Not guaranteed** when loaded as a plain `<img src="logo-wordmark.svg">` — some browsers block
  the external Google Fonts `@import` for security reasons in that context, and the text can fall
  back to a generic sans-serif.

For the React app: import it as an inline/component SVG (e.g. via `vite-plugin-svgr` or by pasting
the markup into a component) rather than referencing it with a plain `<img>` tag, and make sure
`Space Grotesk` is already loaded globally (the same font-loading convention
PROJECT_ARCHITECTURE_FRONTEND.md §15 uses for the rest of the app).

If this logo is ever needed outside the web app — print, a partner's slide deck, an app-store
listing — open `logo-wordmark.svg` in a vector editor (Figma, Illustrator, Inkscape) and use
"Create Outlines" / "Convert to Outlines" **once**, then save that as a separate
`logo-wordmark-outlined.svg`. That removes the font dependency entirely. `favicon.svg` already has
no font dependency and needs no such step.
