---
name: Nocturne Executive
colors:
  surface: '#031427'
  surface-dim: '#031427'
  surface-bright: '#2b3a4f'
  surface-container-lowest: '#000f21'
  surface-container-low: '#0b1c2f'
  surface-container: '#102034'
  surface-container-high: '#1b2b3f'
  surface-container-highest: '#26364a'
  on-surface: '#d3e4fe'
  on-surface-variant: '#c4c7ca'
  inverse-surface: '#d3e4fe'
  inverse-on-surface: '#223145'
  outline: '#8e9195'
  outline-variant: '#44474a'
  surface-tint: '#c0c7cf'
  primary: '#ffffff'
  on-primary: '#2a3137'
  primary-container: '#dce3eb'
  on-primary-container: '#5e656c'
  inverse-primary: '#585f66'
  secondary: '#bac8d7'
  on-secondary: '#25323d'
  secondary-container: '#3d4b57'
  on-secondary-container: '#acbac9'
  tertiary: '#ffffff'
  on-tertiary: '#103251'
  tertiary-container: '#d1e4ff'
  on-tertiary-container: '#496788'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#dce3eb'
  primary-fixed-dim: '#c0c7cf'
  on-primary-fixed: '#151c22'
  on-primary-fixed-variant: '#40484e'
  secondary-fixed: '#d6e4f3'
  secondary-fixed-dim: '#bac8d7'
  on-secondary-fixed: '#0f1d28'
  on-secondary-fixed-variant: '#3b4854'
  tertiary-fixed: '#d1e4ff'
  tertiary-fixed-dim: '#abc9ef'
  on-tertiary-fixed: '#001d35'
  on-tertiary-fixed-variant: '#2a4968'
  background: '#031427'
  on-background: '#d3e4fe'
  surface-variant: '#26364a'
typography:
  display-lg:
    fontFamily: Hanken Grotesk
    fontSize: 56px
    fontWeight: '700'
    lineHeight: 64px
    letterSpacing: -0.03em
  display-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 36px
    fontWeight: '700'
    lineHeight: 44px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Hanken Grotesk
    fontSize: 26px
    fontWeight: '600'
    lineHeight: 34px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Hanken Grotesk
    fontSize: 20px
    fontWeight: '600'
    lineHeight: 28px
    letterSpacing: -0.01em
  body-lg:
    fontFamily: Hanken Grotesk
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 24px
    letterSpacing: 0em
  body-md:
    fontFamily: Hanken Grotesk
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 20px
    letterSpacing: 0.005em
  body-sm:
    fontFamily: Hanken Grotesk
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 16px
    letterSpacing: 0.01em
  label-md:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '500'
    lineHeight: 18px
    letterSpacing: 0.04em
  label-sm:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 14px
    letterSpacing: 0.06em
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  gutter: 1.5rem
  gutter-sm: 1rem
  gutter-lg: 2rem
  margin: 2rem
  margin-mobile: 1rem
  margin-desktop: 3.5rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
---

## Brand & Style

This design system embodies the precision, composure, and quiet authority of an executive boardroom after hours. It pairs deep architectural midnight tones with luminous, vaporous cool accents to create an atmosphere of decisive clarity and understated prestige.

The style blends Corporate Modernity with High-Contrast Precision Minimalism:
- **Atmosphere:** Deep navy voids structured by ultra-fine luminous boundaries, evocative of high-frequency trading terminals and elite executive dashboards.
- **Tone:** Dispassionate, authoritative, razor-sharp, and unencumbered by ornamental distraction.
- **Visual Weight:** Surfaces stay grounded in deep obsidian and midnight navy, allowing high-contrast typography and subtle ice-mist indicators to command immediate cognitive focus.

## Colors

The palette is engineered around dark light-absorption and controlled luminescence. High-contrast legibility is achieved by leveraging stark values between the deepest foundational grounds and crisp white accents.

- **Base Ground (`neutral` / `#0A1B2E`):** The foundational canvas. An impenetrable, deep navy black that minimizes visual fatigue in low-light executive environments.
- **Mid-Tone Container Tier (`tertiary` / `#1B3B5A`):** Used sparingly for structural grounding, active card surfaces, segmented rails, and elevated modules.
- **Intermediate Midnight (`#112A46`):** Sits between canvas and container tiers, serving as the default panel and card surface layer.
- **Accents & Secondary Hierarchy (`secondary` / `#C4D2E1`):** A cold, atmospheric ice mist used for secondary metadata, subtle borders, inactive toggles, and contextual labels.
- **Key Foreground & Focus (`primary` / `#E1E8F0`):** Crisp cloud blue and pure white tints reserved for dominant text hierarchies, primary key interactive triggers, and critical status callouts.

## Typography

The type system pairs the clean geometric authority of **Hanken Grotesk** for primary and narrative communication with the analytical exactitude of **JetBrains Mono** for numerical values, financial data, status flags, and table headings.

- **Primary Headings:** Set in bold weights with tight tracking (`-0.02em` to `-0.03em`) to maintain structural density. Rendered in full crisp white (`#FFFFFF`) or high-luminance cloud blue (`#E1E8F0`).
- **Body Copy:** Set with neutral spacing and relaxed line height to promote rapid executive scanning. Subdued with `#C4D2E1` at 85% opacity to establish effortless optical hierarchy against dominant headings.
- **Labels & Data Readouts:** Monospaced and slightly tracked out (`0.04em` to `0.06em`) in all caps or tabular figures for instant parsing of metrics, KPI deltas, timestamps, and system states.

## Layout & Spacing

The layout is built upon an architectural 12-column responsive fluid grid governed by strict horizontal and vertical cadences of 4px and 8px base units.

- **Desktop (1200px+):** 12 columns, 3.5rem outer canvas margins (`margin-desktop`), and 2rem gutters (`gutter-lg`). High-density layouts maintain maximum breadth for side-by-side data visualization.
- **Tablet (768px - 1199px):** 8 columns, 2rem margins (`margin`), and 1.5rem gutters (`gutter`). Dense summary cards collapse to 4-column blocks.
- **Mobile (320px - 767px):** 4 columns, 1rem margins (`margin-mobile`), and 1rem gutters (`gutter-sm`). Horizontal scrolling is forbidden for key metrics; complex multi-pane workflows transform into stacked, segmented views.
- **Spacing Rhythm:** Micro-spacers (`space-xs`, `space-sm`) bind associated typographic labels and values, while macro-spacers (`space-lg`, `space-xl`) delineate isolated logical systems.

## Elevation & Depth

This system avoids blurry drop shadows, relying instead on **Tonal Layers** and **Low-Contrast Luminous Borders** to indicate z-index and interaction states.

- **Surface Levels:**
  - *Base Layer (0dp):* `#0A1B2E` (Canvas background)
  - *Layer 1 (1dp):* `#112A46` (Cards, side navigation panels, static sheets)
  - *Layer 2 (2dp):* `#1B3B5A` (Floating toolbars, dropdown popovers, active modal dialogs)
- **Ghost Outlines:** Elevated surfaces are bounded by a crisp `1px` solid outline colored `#C4D2E1` set at `12%` opacity. On hover or active focus, this border brightness transitions to `35%` opacity with no change in border width.
- **Glow Accents:** Floating interactive triggers and selected data nodes employ an ambient, tightly focused inner glow (`0 0 12px rgba(225, 232, 240, 0.08)`) instead of casting external drop shadows, preserving an uninterrupted, clean dark field.

## Shapes

The shape hierarchy is disciplined, calibrated at **Soft (`1`)**. This creates a sharp, tailored silhouette that projects structural permanence.

- **Base Components:** Inputs, buttons, chips, table rows, and status badges use `0.25rem` (4px) corner radii.
- **Containers:** Structural cards, elevated panels, and dialogue overlays scale to `rounded-lg` (`0.5rem` / 8px).
- **Overlays:** Full modals and contextual flyouts cap out at `rounded-xl` (`0.75rem` / 12px).
- Circles and pills are reserved exclusively for avatars, dot indicators, and binary indicator pills, preventing a casual aesthetic from diluting executive tone.

## Components

### Buttons
- **Primary:** Background in luminous cloud blue (`#E1E8F0`), label in deep navy (`#0A1B2E`) bold text. High contrast, immediate visual target. Hover shifts background to pure white (`#FFFFFF`) with a subtle `0 0 16px rgba(225, 232, 240, 0.2)` bloom.
- **Secondary:** Surface `#112A46` with 1px border of `#C4D2E1` at 20% opacity. Text in `#E1E8F0`. Hover shifts border to 50% opacity and background to `#1B3B5A`.
- **Tertiary / Ghost:** Transparent background, text `#C4D2E1`, hover introduces `#112A46` background with zero border.

### Inputs & Selectors
- **Input Fields:** Grounded in `#0A1B2E` with a 1px border of `#1B3B5A`. Default placeholder text in `#C4D2E1` at 40% opacity.
- **Focus State:** 1px perimeter transition to `#E1E8F0` with no browser-native outline offset. Label moves to high-contrast white.
- **Monospace Inputs:** Numerical entries automatically invoke `JetBrains Mono` with right-alignment for financial calculations.

### Cards & Panels
- **Structure:** Solid `#112A46` core, `0.5rem` radius, bordered uniformly by 1px `#C4D2E1` at 12% opacity.
- **Header Separator:** Subtle internal 1px line divider (`#1B3B5A`) segregating summary metrics from table content.
- **Interactive State:** Hover elevates surface slightly via subtle border illumination (`#C4D2E1` at 30%) with zero physical transformation or translation.

### Data Tables & Lists
- **Rows:** Alternating transparent rows with hairline dividers (`1px solid rgba(196, 210, 225, 0.08)`).
- **Header:** Sticky, mono-styled labels in uppercase (`label-sm`), `#C4D2E1` at 70% opacity, background matched to the parent card ground.
- **Row Selection:** Marked by a solid 2px left border accent in `#E1E8F0` and an active row wash of `#1B3B5A` at 40% opacity.

### Checkboxes & Radios
- **Unchecked:** Flat `#0A1B2E` box with a 1px `#1B3B5A` frame.
- **Checked:** Solid `#E1E8F0` fill with `#0A1B2E` tick/dot mark. Zero gradient, high legibility.

### Metric KPI Badges & Chips
- Compact dimensions using `label-sm`. Background set to `#1B3B5A` at 60% opacity with a hairline border of `#C4D2E1` at 20%. Positive/negative deltas utilize muted, desaturated emerald (`#34D399`) and crimson (`#F87171`) to prevent disruption of the core dark palette.