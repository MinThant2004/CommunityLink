---
name: Midnight Executive
colors:
  surface: '#10131a'
  surface-dim: '#10131a'
  surface-bright: '#363941'
  surface-container-lowest: '#0b0e15'
  surface-container-low: '#191b23'
  surface-container: '#1d1f27'
  surface-container-high: '#272a32'
  surface-container-highest: '#32353d'
  on-surface: '#e1e2ec'
  on-surface-variant: '#d0c5af'
  inverse-surface: '#e1e2ec'
  inverse-on-surface: '#2d3038'
  outline: '#99907c'
  outline-variant: '#4d4635'
  surface-tint: '#e9c349'
  primary: '#f2ca50'
  on-primary: '#3c2f00'
  primary-container: '#d4af37'
  on-primary-container: '#554300'
  inverse-primary: '#735c00'
  secondary: '#c0c1ff'
  on-secondary: '#1000a9'
  secondary-container: '#3131c0'
  on-secondary-container: '#b0b2ff'
  tertiary: '#c0d0e6'
  on-tertiary: '#233143'
  tertiary-container: '#a5b4ca'
  on-tertiary-container: '#384658'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#ffe088'
  primary-fixed-dim: '#e9c349'
  on-primary-fixed: '#241a00'
  on-primary-fixed-variant: '#574500'
  secondary-fixed: '#e1e0ff'
  secondary-fixed-dim: '#c0c1ff'
  on-secondary-fixed: '#07006c'
  on-secondary-fixed-variant: '#2f2ebe'
  tertiary-fixed: '#d4e4fa'
  tertiary-fixed-dim: '#b9c8de'
  on-tertiary-fixed: '#0d1c2d'
  on-tertiary-fixed-variant: '#39485a'
  background: '#10131a'
  on-background: '#e1e2ec'
  surface-variant: '#32353d'
typography:
  display-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.02em
  display-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 36px
    fontWeight: '600'
    lineHeight: 44px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Plus Jakarta Sans
    fontSize: 28px
    fontWeight: '600'
    lineHeight: 36px
    letterSpacing: -0.015em
  headline-md:
    fontFamily: Plus Jakarta Sans
    fontSize: 22px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  title-lg:
    fontFamily: Inter
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 24px
    letterSpacing: -0.005em
  title-md:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '600'
    lineHeight: 22px
    letterSpacing: 0em
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0em
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
    letterSpacing: 0.005em
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
    letterSpacing: 0.01em
  label-lg:
    fontFamily: Inter
    fontSize: 13px
    fontWeight: '600'
    lineHeight: 16px
    letterSpacing: 0.04em
  label-md:
    fontFamily: Inter
    fontSize: 11px
    fontWeight: '600'
    lineHeight: 14px
    letterSpacing: 0.06em
  label-sm:
    fontFamily: Inter
    fontSize: 10px
    fontWeight: '700'
    lineHeight: 12px
    letterSpacing: 0.08em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-desktop: 2rem
  margin: 1.5rem
  margin-desktop: 3rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
---

## Brand & Style
The design system establishes a private-salon atmosphere for verified industry leaders, domain luminaries, and enterprise executives. It reconciles boardroom authority with the dynamic precision of a modern liquidity-tier network. 

The aesthetic is anchored in an editorial tech-minimalism framework, layering obsidian slate foundations with hairline metallic accents and muted luminescence. The emotional target is unquestioned trust, exclusivity, and focused clarity—eliminating consumer noise in favor of high-signal interaction patterns. Visual tension is achieved by pairing crisp architectural layouts with subtle, high-index refractive glass containers.

## Colors
The palette operates strictly within a rich, low-light environment designed to reduce ocular fatigue and heighten focus.

- **Primary (`#D4AF37` / Champagne Gold):** Reserved for high-value affordances, verified badges, active membership markers, tier pricing highlights, and key executive confirmations. It conveys prestige without gaudiness.
- **Secondary (`#6366F1` / Electric Indigo):** Applied to active navigational systems, actionable telemetry, interactive data surfaces, and digital infrastructure markers.
- **Tertiary (`#94A3B8` / Slate Steel):** Governs structural framing, subtle metadata tags, inactive states, and secondary iconography.
- **Neutral (`#0A0D14` / Obsidian Slate):** The bedrock background canvas, scaling into tonal variants: `#111622` for base cards and `#182030` for elevated glass panels.

Hairline borders use a 12% alpha layer of `#D4AF37` on premium tier components, and an 8% alpha layer of `#FFFFFF` across universal containers.

## Typography
The system couples the architectural balance of Plus Jakarta Sans for titles and metrics with the functional clarity of Inter for sustained reading, complex metadata, and dense analytical data.

Display levels demand strict tracking compression (`-0.02em`) to maintain a cohesive, editorial profile on desktop screens. Micro-labels, access-tier tags, and numerical badges are rendered in uppercase tracking (`0.06em` to `0.08em`) to enforce legibility at small optical sizes against obsidian backdrops.

## Layout & Spacing
A 12-column grid is standard on desktop displays with a fixed structural max-width of `1440px`. Gutters sit at `2rem` (`32px`) to ensure distinct breathing room between modules, marketplace listings, and data sidebars.

Spacing rhythm conforms strictly to an 8px architectural grid. Component layouts leverage structural density: internal cell buffers favor compact vertical rhythm (`space-sm` and `space-md`) counterbalanced by generous module separations (`space-xl`). On smaller desktop views down to `1024px`, the system preserves grid margins while condensing column spans from 12 to 8, folding tertiary side panels into sliding contextual drawers.

## Elevation & Depth
Depth is executed through tinted tonal layering coupled with directional glass refraction rather than standard drop shadows.

- **Base Layer (0dp):** Deep slate canvas (`#0A0D14`).
- **Surface Level 1 (Card Default):** `#111622` background with a `1px` stroke of `rgba(255, 255, 255, 0.07)` and an inset highlight of `rgba(255, 255, 255, 0.03)` along the top edge.
- **Surface Level 2 (Glass Hover / Elevated Modules):** `rgba(24, 32, 48, 0.75)` with a `16px` backdrop blur, a subtle upward ambient tint (`0 12px 32px -4px rgba(0, 0, 0, 0.65)`), and a micro-glow border of `rgba(99, 102, 241, 0.18)` or `rgba(212, 175, 55, 0.20)`.
- **Surface Level 3 (Modals & Executive Drawers):** `#151B2B` with a `24px` backdrop blur over an obsidian dim layer (`rgba(10, 13, 20, 0.85)`), stabilized by a defined border of `rgba(255, 255, 255, 0.12)`.

## Shapes
The visual identity relies on sharp, restrained geometry to signal engineering precision and business maturity. 

Elements use consistent structural geometry (`roundedness: 1`):
- Inputs, standard buttons, and small tags use a strict `4px` corner radius.
- Standard cards, panels, and marketplace modules employ `8px` (`rounded-lg`).
- High-level containers and access modals use a maximum radius of `12px` (`rounded-xl`).
- Verified badges, status indicators, and avatar cutouts retain sharp or fully round geometry based on function, avoiding softened squircle designs.

## Components

### Buttons
- **Primary (Executive):** Champagne Gold (`#D4AF37`) fill with midnight charcoal (`#0A0D14`) bold text. Hover induces a luminous expansion via a soft gold ambient blur (`0 0 16px rgba(212, 175, 55, 0.35)`).
- **Secondary (Technical):** Deep slate background (`#182030`) with white text and a `1px` border of `rgba(99, 102, 241, 0.35)`. Hover intensifies the border to full Electric Indigo (`#6366F1`).
- **Tertiary / Ghost:** Transparent background with `#94A3B8` text, transitioning to white with a subtle `rgba(255, 255, 255, 0.05)` fill on hover.

### Verified Badges
- **Domain Professional:** Precision geometric badge containing a micro-star icon; Electric Indigo hairline border with an indigo-tinted background (`rgba(99, 102, 241, 0.15)`) and white typography.
- **Public Figure / Luminary:** Radiant Champagne Gold fill badge with inverted dark icon and a micro ambient ring (`rgba(212, 175, 55, 0.25)`).

### Access-Tier Modules
- Modular column blocks featuring a structural card format. Tier names sit in uppercase tracking (`label-sm`).
- Key prices feature `display-md` numbers paired with muted period labels (`body-sm`).
- Featured tiers feature a luminous gold hairline border (`rgba(212, 175, 55, 0.40)`) and a subtle top-down gradient wash (`rgba(212, 175, 55, 0.04)` to transparent).

### Input Fields
- Monolithic slate surfaces (`#111622`) with `1px` border of `rgba(255, 255, 255, 0.1)`. 
- Focus state switches the border to secondary indigo (`#6366F1`) with an ambient internal shadow ring (`0 0 0 1px #6366F1`). Placeholder text remains subdued in `#475569`.

### Chips & Badges
- Low-profile status chips with height capped at `24px`. 
- Structural styling relies on an obsidian base, `1px` borders matching the state color (e.g., gold for active, steel for pending), and uppercase `label-sm` typographic hierarchy.