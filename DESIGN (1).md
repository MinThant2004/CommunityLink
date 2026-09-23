---
name: Executive Minimalist
colors:
  surface: '#f7f9ff'
  surface-dim: '#bfddff'
  surface-bright: '#f7f9ff'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#edf4ff'
  surface-container: '#e3efff'
  surface-container-high: '#d9eaff'
  surface-container-highest: '#cfe5ff'
  on-surface: '#001d34'
  on-surface-variant: '#44474c'
  inverse-surface: '#10324e'
  inverse-on-surface: '#e8f1ff'
  outline: '#74777d'
  outline-variant: '#c4c6cd'
  surface-tint: '#505f75'
  primary: '#000000'
  on-primary: '#ffffff'
  primary-container: '#0b1c2f'
  on-primary-container: '#75859c'
  inverse-primary: '#b7c8e1'
  secondary: '#45617e'
  on-secondary: '#ffffff'
  secondary-container: '#bddafd'
  on-secondary-container: '#445f7d'
  tertiary: '#000000'
  on-tertiary: '#ffffff'
  tertiary-container: '#081c31'
  on-tertiary-container: '#73859f'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#d3e4fe'
  primary-fixed-dim: '#b7c8e1'
  on-primary-fixed: '#0b1c2f'
  on-primary-fixed-variant: '#38485d'
  secondary-fixed: '#d0e4ff'
  secondary-fixed-dim: '#adc9eb'
  on-secondary-fixed: '#001d34'
  on-secondary-fixed-variant: '#2d4965'
  tertiary-fixed: '#d2e4ff'
  tertiary-fixed-dim: '#b5c8e4'
  on-tertiary-fixed: '#081c31'
  on-tertiary-fixed-variant: '#36485f'
  background: '#f7f9ff'
  on-background: '#001d34'
  surface-variant: '#cfe5ff'
typography:
  display-hero:
    fontFamily: Manrope
    fontSize: 48px
    fontWeight: '700'
    lineHeight: 56px
    letterSpacing: -0.03em
  display-hero-mobile:
    fontFamily: Manrope
    fontSize: 32px
    fontWeight: '700'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg:
    fontFamily: Manrope
    fontSize: 32px
    fontWeight: '600'
    lineHeight: 40px
    letterSpacing: -0.02em
  headline-lg-mobile:
    fontFamily: Manrope
    fontSize: 26px
    fontWeight: '600'
    lineHeight: 34px
    letterSpacing: -0.015em
  headline-md:
    fontFamily: Manrope
    fontSize: 22px
    fontWeight: '600'
    lineHeight: 30px
    letterSpacing: -0.01em
  headline-sm:
    fontFamily: Manrope
    fontSize: 18px
    fontWeight: '600'
    lineHeight: 26px
    letterSpacing: -0.005em
  body-lg:
    fontFamily: Inter
    fontSize: 16px
    fontWeight: '400'
    lineHeight: 26px
    letterSpacing: -0.01em
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: 22px
    letterSpacing: 0em
  body-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '400'
    lineHeight: 18px
    letterSpacing: 0em
  label-lg:
    fontFamily: JetBrains Mono
    fontSize: 13px
    fontWeight: '500'
    lineHeight: 18px
    letterSpacing: 0.04em
  label-md:
    fontFamily: JetBrains Mono
    fontSize: 11px
    fontWeight: '500'
    lineHeight: 16px
    letterSpacing: 0.06em
  label-sm:
    fontFamily: JetBrains Mono
    fontSize: 10px
    fontWeight: '500'
    lineHeight: 14px
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
  gutter-sm: 1rem
  gutter-lg: 2rem
  margin: 2rem
  margin-sm: 1rem
  margin-lg: 3.5rem
  space-xs: 0.25rem
  space-sm: 0.5rem
  space-md: 1rem
  space-lg: 1.5rem
  space-xl: 2.5rem
---

## Brand & Style
The design system embodies the calculated clarity, authority, and architectural quietude of modern executive leadership spaces. Designed for high-stakes decision-makers, wealth advisors, enterprise architects, and premium productivity environments, it projects deliberate competence, composure, and intellectual rigor.

Drawing fundamentally from **Corporate Minimalism** infused with nuanced **Tonal Layering**, the aesthetic relies heavily on wide visual pauses, precise vertical cadence, and pristine white surfaces punctuated by deep navy anchors. The UI rejects ornamental gradients, loud accents, and noisy micro-interactions, favoring instead micro-fine lines, crisp structural alignment, whisper-soft navy-tinted elevations, and immaculate typographic hierarchy. The emotional response is one of total institutional trust, quiet prestige, and uncompromising order.

## Colors
The color architecture establishes an intentional monochromatic hierarchy built on crisp white, cooled neutral tints, and profound navy shades:

- **Canvas & High Surfaces:** Pure white (`#ffffff`) serves as the active structural canvas, complemented by Arctic Haze (`#F2F6FB`) for structural backdrops, recessed zones, and subtle canvas alternations.
- **Tonal Dividers & Soft Fills:** Mist Blue (`#E1E8F0`) acts as the default hairline border and interactive hover tint; Cloud Blue (`#C4D2E1`) and Foggy Blue (`#A1B2C4`) provide subdued outlines, inactive toggles, and inactive indicators.
- **Mid-Tone Supporting Values:** Dusk Blue (`#7A8CA6`) and Blue Slate (`#557392`) fulfill the roles of secondary labels, disabled text, caption typography, and balanced neutral structural lines.
- **Primary Anchors:** Storm Blue (`#34506D`) provides vibrant secondary action targets; Midnight Blue (`#112A46`) and Deep Navy (`#0A1B2E`) supply unshakeable foreground anchors for key navigation, bold metrics, dominant CTA buttons, and high-priority states.

Color roles maintain strict contrast discipline: text placed on `#ffffff` or `#F2F6FB` must never drop below `#557392` to guarantee effortless accessibility across analytical surfaces.

## Typography
Typographic execution pairs the structural elegance of **Manrope** for primary display and section headers with the utilitarian clarity of **Inter** for intensive operational body copy. For analytical metadata, numeric tickers, status stamps, and table schema, **JetBrains Mono** introduces an authentic institutional precision.

Headlines should maintain tight character tracking (`-0.03em` to `-0.01em`) to simulate physical boardroom editorial layouts. Body typography remains roomy with generous line heights to preserve cognitive ease in data-rich layouts. All monospaced label tokens default to uppercase styling with wider letter spacing (`0.04em` to `0.08em`) to demarcate contextual metadata clearly from running text.

## Layout & Spacing
The layout follows a balanced 12-column fluid grid system on desktop that gracefully collapses to an 8-column format on tablet and a single 4-column column on mobile devices.

- **Desktop (1200px+):** Employs standard 12 columns with `2rem` gutters and outer horizontal margins of `3.5rem`. Container max-widths terminate deliberately at 1440px to retain executive legibility across ultra-wide displays.
- **Tablet (768px - 1199px):** Reflows into an 8-column layout with `1.5rem` gutters and `2rem` outer page margins. Side-by-side analytical panels collapse to structured vertical pairings.
- **Mobile (< 768px):** Drops to 4 columns with `1rem` gutters and `1rem` outer canvas padding. Grid cards stack vertically while data tables introduce horizontal scroll boundaries anchored to card frames.

The spacing rhythm strictly adheres to multiples of 4px and 8px, prioritizing open negative space around headline modules (`space-xl`) while enforcing surgical density within data tables, metadata groups, and input stacks (`space-xs` and `space-sm`).

## Elevation & Depth
Elevation is conveyed through a hybrid of **Tonal Layers** and **Tinted Ambient Shadows**, entirely avoiding generic black-based dropshadows. Visual depth reflects natural overhead gallery lighting:

- **Level 0 (Base Canvas):** Clean surface in `#ffffff` or muted background in `#F2F6FB`.
- **Level 1 (Dimensional Cards & Panels):** Pure white `#ffffff` surface delineated by a hairline perimeter border (`1px solid #E1E8F0`) paired with an ultra-soft blue-tinted shadow: `0 2px 8px -2px rgba(10, 27, 46, 0.04), 0 1px 3px 0 rgba(10, 27, 46, 0.02)`.
- **Level 2 (Hovered Cards & Action Sheets):** Slightly elevated with intensified soft diffusion: `0 8px 24px -4px rgba(10, 27, 46, 0.07), 0 2px 6px -1px rgba(10, 27, 46, 0.03)`. Border shifts subtly to `#C4D2E1`.
- **Level 3 (Modals, Popovers & Context Menus):** High floating surfaces sitting atop an Arctic backdrop: `0 20px 48px -8px rgba(10, 27, 46, 0.12), 0 4px 12px -2px rgba(10, 27, 46, 0.04)`, surrounded by `1px solid #C4D2E1`.

Backdrop scrims for modal dialogs employ `#0A1B2E` at 30% opacity with a subtle `backdrop-filter: blur(4px)` to preserve layout continuity while centering attention.

## Shapes
The shape language uses `roundedness: 1` (Soft), reflecting an architectural, clean-edge philosophy that avoids excessive playfulness while remaining friendlier than stark brutalist edges.

- Base inputs, buttons, chips, and table records feature corners rounded to `0.25rem` (4px).
- Larger container structures, summary dashboard cards, and slide-over drawers use `0.5rem` (8px).
- Modal windows and floating popovers cap at `0.75rem` (12px).
- Avatars, radial indicators, and active status pips use continuous full-circle (`9999px`) geometry to contrast sharply against rectilinear card boundaries.

## Components

### Buttons
- **Primary:** Deep Navy background (`#0A1B2E`), pure white text (`#ffffff`), `0.25rem` radius, padding `0.625rem 1.25rem`. On hover, color transitions to Midnight Blue (`#112A46`) with subtle lift.
- **Secondary:** Cloud Blue fill (`#F2F6FB`), hairline border (`1px solid #C4D2E1`), text `#112A46`. On hover, background shifts to `#E1E8F0`.
- **Ghost:** Transparent background, Storm Blue text (`#34506D`). On hover, background fills with Arctic Haze (`#F2F6FB`).

### Chips & Badges
- Built with uppercase `label-sm` JetBrains Mono text.
- Standard state: Surface `#F2F6FB`, border `1px solid #E1E8F0`, text `#34506D`.
- Selected/Active state: Deep Navy background (`#0A1B2E`), white text (`#ffffff`), border `1px solid #0A1B2E`.

### Cards & Analytical Panels
- Background `#ffffff`, border `1px solid #E1E8F0`, border-radius `0.5rem`, padding `1.5rem`.
- Header areas feature a subtle divider in `#F2F6FB` with a top label in `label-md` uppercase Dusk Blue (`#7A8CA6`).

### Form Inputs & Selects
- Background `#ffffff`, border `1px solid #C4D2E1`, border-radius `0.25rem`, padding `0.5rem 0.75rem`.
- Focus state: Border shifts to `#1B3B5A` accompanied by an ambient halo `0 0 0 3px rgba(27, 59, 90, 0.12)`.
- Placeholder text sits in Foggy Blue (`#A1B2C4`).

### Checkboxes & Radio Buttons
- Base size `16px x 16px`, border `1.5px solid #7A8CA6`, background `#ffffff`.
- Checked state: Fill `#0A1B2E`, border `#0A1B2E`, with a crisp white check glyph or center dot.

### Data Tables
- Table header row uses Arctic Haze (`#F2F6FB`) background with `label-md` text in Dusk Blue (`#7A8CA6`).
- Body rows alternate hairline bottom dividers in Mist Blue (`#E1E8F0`). On row hover, background transitions effortlessly to `#F2F6FB`.