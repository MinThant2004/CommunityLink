# System UI/UX Rule Specification: CommunityLink

**Document Standard:** Unified Design System (Dark Mode & Light Mode)  
**Foundational Specs:** `DESIGN (5).md` (Midnight Executive Dark) & `DESIGN (6).md` (Executive Platinum Light)  
**Aesthetic Profile:** Executive Architectural Tech-Minimalism (Champagne Gold, Midnight Slate, Optical Alabaster)  
**Strict Directives:**
1. Minimal, clear web layout — NO oversized, bloated, or heavy cards.
2. Complete removal and strict prohibition of cartoon emojis across all UI surfaces; replace with standardized inline vector SVGs (Lucide / Heroicons).
3. Dual-mode parity: Seamless token mapping between Midnight Dark and Platinum Light modes.
4. Disciplined typography pairing (`Plus Jakarta Sans` for titles/metrics, `Inter` for interface reading/body, `JetBrains Mono` for tabular/code/IDs).
5. 100% specification only — **Do not implement code yet**.

---

## 1. System Design Tenets & Philosophy

1. **Restrained Editorial Precision:**
   - Visual authority comes from crisp typography, deliberate negative space, and hairline 1px micro-borders—never from heavy drop shadows, decorative cartoon stickers, or chaotic gradients.
2. **Total Cartoon Emoji Deprecation:**
   - **Mandate:** Emojis such as `⭐`, `🏛️`, `💧`, `📊`, `💬`, `👥`, `🛡️`, `🪪`, `⚙️`, `🔑`, `📜`, `⏳`, `✅`, `❌`, `🔒`, `👤` are strictly prohibited in navigation, headers, buttons, status badges, tables, and dialogs.
   - **Replacement:** Use clean, hairline vector SVGs (14px–18px, `fill="none"`, `stroke="currentColor"`, `stroke-width="1.75"` or `2`).
3. **Dual-Mode Symmetry:**
   - Every component must define explicit color and border tokens for **Dark Mode** (`Midnight Executive` / `#10131a` bedrock) and **Light Mode** (`Executive Platinum Light` / `#F8F9FF` bedrock).
4. **Structural Restraint & Minimal Shapes:**
   - Small controls, buttons, inputs, tags: `rounded: 0.25rem` (4px).
   - Component cards, data panels, thread items: `rounded-lg: 0.5rem` (8px).
   - Dialogs, slide-out sheets, top modals: `rounded-xl: 0.75rem` (12px).
   - Identity badges, status dots, avatar frames: `rounded-full: 9999px`.
5. **Data & Metadata Strictness:**
   - Numerical metrics, wallet balances, timestamps, and system IDs must be rendered in uppercase tabular tracking with `JetBrains Mono` or high-clarity `Inter` numbers.

---

## 2. Dual-Mode Color Token Architecture

The system reconciles **Midnight Executive** (low-light ocular focus) with **Executive Platinum Light** (daylight boardroom clarity):

| Token Name | Midnight Executive (Dark Mode) | Executive Platinum (Light Mode) | Purpose / Scope |
| :--- | :--- | :--- | :--- |
| **Canvas Background** | `#10131A` (`#0B0E15` bedrock) | `#F8F9FF` (`#F8FAFC` alabaster) | Main application canvas |
| **Recessed Well / Ground** | `#191B23` / `#1D1F27` | `#F1F5F9` / `#EFF4FF` | Input fields, code containers, inactive tabs |
| **Surface Base (Card)** | `#111622` / `rgba(24, 32, 48, 0.75)` | `#FFFFFF` | Primary content cards, list panels |
| **Surface Hover** | `#1D263B` / `rgba(39, 42, 50, 0.85)` | `#F8FAFC` / `#EFF4FF` | Hover states on interactive cards |
| **Hairline Border (Default)** | `rgba(255, 255, 255, 0.08)` | `#E2E8F0` / `rgba(15, 23, 42, 0.08)` | Perimeter boundaries, table splitters |
| **Hairline Border (Strong)** | `rgba(255, 255, 255, 0.16)` | `#CBD5E1` | Focus indicators, selected state boundaries |
| **Text Primary (Headings)** | `#E1E2EC` (Platinum White) | `#0B1C30` / `#0F172A` (Deep Slate) | Executive titles, primary emphasis |
| **Text Secondary (Body)** | `#D0C5AF` / `#94A3B8` | `#334155` / `#45464D` | Longform body copy, standard labels |
| **Text Muted (Meta/Subtle)**| `#64748B` / `#76777D` | `#64748B` / `#94A3B8` | Timestamps, placeholders, inactive icons |
| **Executive Gold (Primary)** | `#D4AF37` / `#F2CA50` | `#B45309` / `#C69214` | Verified status, premium affordances, badges |
| **Gold Wash / Container** | `rgba(212, 175, 55, 0.15)` | `#FEF3C7` | Badge background, selected row tint |
| **Electric Indigo (Tech)** | `#6366F1` / `#C0C1FF` | `#4F46E5` / `#3131C0` | Active navigation, digital infrastructure |
| **Danger / Error** | `#FFB4AB` (`#93000A` fill) | `#BA1A1A` (`#FFDAD6` fill) | Destructive actions, validation errors |
| **Success / Approved** | `#34D399` (`rgba(16, 185, 129, 0.15)`) | `#047857` (`#D1FAE5` fill) | Verified checks, approval tags |

---

## 3. Emoji Deprecation & Vector SVG Replacement Guide

**Zero Emoji Rule:** Under no circumstances should emojis be rendered in the application layout. Replace each semantic element with its standardized inline SVG icon:

| Legacy Emoji | Semantic Meaning | Clean SVG Replacement (16x16px or 14x14px, `fill="none"`, `stroke="currentColor"`, `stroke-width="2"`) |
| :---: | :--- | :--- |
| ⭐ | Premium / Star / Favorite | `<svg class="w-4 h-4" viewBox="0 0 24 24" fill="currentColor"><path d="M12 2l3.09 6.26L22 9.27l-5 4.87 1.18 6.88L12 17.77l-6.18 3.25L7 14.14 2 9.27l6.91-1.01L12 2z"/></svg>` |
| 🏛️ | Public Hub / VIP / Governance | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M3 21h18M5 21V9M19 21V9M9 21V9M15 21V9M2 9l10-6 10 6"/></svg>` |
| 💧 | LinkDrops / Token Balance | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M12 2.69l5.66 5.66a8 8 0 1 1-11.31 0z"/></svg>` |
| 📊 | Analytics / Telemetry / Stats | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M18 20V10M12 20V4M6 20v-6"/></svg>` |
| 💬 | Conversations / Messaging | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/></svg>` |
| 👥 | Community / Member Roster | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2M9 7a4 4 0 1 0 0-8 4 4 0 0 0 0 8zm13 14v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75"/></svg>` |
| 🛡️ | Governance / Roles / Admin | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>` |
| 🪪 | Identity Card / Verification | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><rect x="2" y="4" width="20" height="16" rx="2"/><circle cx="8" cy="10" r="2"/><path d="M14 9h4M14 13h4M6 16c0-1.5 1.5-2 3-2s3 .5 3 2"/></svg>` |
| ⚙️ | Settings / Configuration | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 1 1-2.83 2.83l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 1 1-4 0v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 1 1-2.83-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 1 1 0-4h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 1 1 2.83-2.83l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 1 1 4 0v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 1 1 2.83 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 1 1 0 4h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>` |
| 🔑 | API Keys / Permissions | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><circle cx="7.5" cy="15.5" r="4.5"/><path d="m21 3-9.5 9.5M15.5 7.5l3 3M18 5l3 3"/></svg>` |
| 📜 | Audit Log / History Records | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"/><path d="M14 2v6h6M16 13H8M16 17H8M10 9H8"/></svg>` |
| ⏳ | Pending Audit / Waiting State | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/></svg>` |
| ✅ | Approved / Success Indicator | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M20 6L9 17l-5-5"/></svg>` |
| ❌ | Rejected / Failed Action | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M18 6L6 18M6 6l12 12"/></svg>` |
| 🔒 | Encrypted / Private Vault | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>` |
| 👤 | Individual Member / Account | `<svg class="w-4 h-4" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>` |

---

## 4. Verified Identity Badges (Executive Styling)

Badges must be rendered as compact micro-pills (`height: 22px`, font-size: 11px, font-weight: 600, uppercase letter-spacing: 0.05em, radius: `9999px`):

### 1. Domain Professional Badge
- **Dark Mode:** Background `rgba(99, 102, 241, 0.15)`, Border `1px solid rgba(99, 102, 241, 0.35)`, Text `#C0C1FF`
- **Light Mode:** Background `#EEF2FF`, Border `1px solid #C7D2FE`, Text `#4338CA`
- **Icon:** Micro-Star SVG (12x12px)

### 2. Public Figure / Executive Luminary Badge
- **Dark Mode:** Background `rgba(212, 175, 55, 0.18)`, Border `1px solid rgba(212, 175, 55, 0.40)`, Text `#F2CA50`, Ambient glow `0 0 12px rgba(212, 175, 55, 0.2)`
- **Light Mode:** Background `#FEF3C7`, Border `1px solid #FDE68A`, Text `#B45309`
- **Icon:** Architectural Pillars / Columns SVG (12x12px)

### 3. Administrator / Governor Badge
- **Dark Mode:** Background `rgba(14, 165, 233, 0.14)`, Border `1px solid rgba(14, 165, 233, 0.35)`, Text `#38BDF8`
- **Light Mode:** Background `#E0F2FE`, Border `1px solid #BAE6FD`, Text `#0369A1`
- **Icon:** Shield Check SVG (12x12px)

### 4. Verified Member Seal
- **Dark Mode:** Background `rgba(16, 185, 129, 0.14)`, Border `1px solid rgba(16, 185, 129, 0.35)`, Text `#34D399`
- **Light Mode:** Background `#D1FAE5`, Border `1px solid #A7F3D0`, Text `#047857`
- **Icon:** Check Circle SVG (12x12px)

---

## 5. Typography Hierarchy

| Style Role | Font Family | Size | Weight | Line Height | Tracking | Usage |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| **Display Title** | `Plus Jakarta Sans` | 32px–40px | 700 | 1.15 | `-0.025em` | Hero page headers |
| **Section Header**| `Plus Jakarta Sans` | 20px–24px | 600 | 1.25 | `-0.015em` | Module & card headers |
| **Card Subtitle** | `Plus Jakarta Sans` | 16px | 600 | 1.30 | `-0.005em` | Card group titles |
| **Body Primary**  | `Inter` | 14px | 400 | 1.50 | `-0.006em` | Standard text, feed streams |
| **Body Secondary**| `Inter` | 13px | 400 | 1.45 | `0em` | Metadata, helper descriptions |
| **Micro Caption** | `Inter` | 10px–11px | 700 | 1.20 | `0.08em` | Uppercase category tags, pills |
| **Tabular / Code**| `JetBrains Mono` | 11px–12px | 500 | 1.40 | `0.04em` | Timestamps, balances, IDs |

---

## 6. Layout & Minimalist Component Rules

### A. Minimal Layout Blueprint
- **No Big Layout Clutter:** Avoid bulky cards with heavy padding, giant cartoon hero banners, or excessive whitespace that pushes actionable data below the fold.
- **Max Container Width:** `1360px` centered on desktop (`margins: 2.5rem`, `gutters: 1.5rem`).
- **Internal Card Padding:** Compact, disciplined spacing (`1rem` to `1.25rem`).
- **Dividers:** 1px hairline dividers (`border-t border-slate-200 dark:border-slate-800`).

### B. Elevation & Depth
- **Level 0 (Bedrock Canvas):** Flat (`#10131A` Dark / `#F8F9FF` Light).
- **Level 1 (Standard Card):**
  - Dark: `#111622` background with hairline `1px solid rgba(255, 255, 255, 0.08)`.
  - Light: `#FFFFFF` background with `1px solid #E2E8F0`, ambient shadow `0 1px 3px rgba(15, 23, 42, 0.04)`.
- **Level 2 (Hover / Elevated Drawer):**
  - Dark: `#182030` background with micro-glow border (`rgba(212, 175, 55, 0.20)` or `rgba(99, 102, 241, 0.25)`).
  - Light: `#FFFFFF` with `box-shadow: 0 4px 12px -2px rgba(15, 23, 42, 0.06)`.
- **Level 3 (Modal Dialogs):**
  - High blur backdrop (`backdrop-blur-md`), hairline perimeter border, sharp `rounded-xl` corners.

### C. Standardized Buttons
- **Primary Executive Action:**
  - Dark: Fill `#D4AF37` (Champagne Gold), text `#0A0D14` (Bold), hover ambient glow `0 0 14px rgba(212, 175, 55, 0.35)`.
  - Light: Fill `#0F172A` (Deep Slate), text `#FFFFFF`, hover `#1E293B`.
- **Secondary Outlined Action:**
  - Dark: Background `transparent`, border `1px solid rgba(255, 255, 255, 0.16)`, text `#E1E2EC`, hover background `rgba(255, 255, 255, 0.05)`.
  - Light: Background `#FFFFFF`, border `1px solid #CBD5E1`, text `#0F172A`, hover background `#F1F5F9`.
- **Ghost Action:**
  - Minimalist text button with no default border; hover shows light background tint.

### D. Input Fields & Form Controls
- **Height & Sizing:** Clean `38px`–`42px` height, `rounded: 0.25rem` (4px).
- **Default State:**
  - Dark: Background `#111622`, border `1px solid rgba(255, 255, 255, 0.10)`, text `#E1E2EC`, placeholder `#475569`.
  - Light: Background `#FFFFFF`, border `1px solid #CBD5E1`, text `#0F172A`, placeholder `#94A3B8`.
- **Focus State:**
  - Focus ring transitions to Electric Indigo (`#6366F1`) or Champagne Gold (`#C69214`) with a delicate hairline halo (`0 0 0 3px rgba(198, 146, 20, 0.18)`).

### E. Data Tables & Rosters
- **Header:** Uppercase `10px` tracking `0.06em`, font-weight 700, muted color (`#64748B`).
- **Row Rhythm:** `38px` to `48px` compact height, 1px bottom hairline divider.
- **Hover State:** Instant subtle tint (`rgba(255, 255, 255, 0.02)` Dark / `#F8FAFC` Light).

---

## 7. Anti-Patterns to Strictly Prohibit

1. **NO Cartoon Emojis:** Any emoji like `⭐`, `🏛️`, `💬`, `👥`, `💧`, `📊` will immediately fail UI review. Always use clean SVGs.
2. **NO Oversized Hero Containers:** Hero sections must not exceed `220px` height; content and data streams must remain prominent.
3. **NO Garish Saturated Gradients:** Avoid multi-color rainbow gradients; use monochrome slate surfaces with hairline metallic or indigo accents.
4. **NO Inconsistent Radii:** Stick strictly to `4px` for controls, `8px` for cards, `12px` for dialogs, and `9999px` for pill badges.
