# CommunityLink Unified UI/UX Design System Specification
**Version:** 2.0 • **Status:** Production Architecture Standard • **Target Platform:** Blazor / Web (.NET 10)

---

## 1. Executive Philosophy & System Overview

The CommunityLink interface embodies **Executive Minimalist Architecture** across two synchronized optical environments:
* **Light Mode ("Executive Minimalist"):** Pristine white and Arctic Haze canvases punctuated by Deep Navy anchors, micro-fine hairline borders, and blue-slate supporting tones. Evokes institutional authority, composure, and intellectual rigor.
* **Dark Mode ("Nocturne Executive"):** Deep Obsidian and Midnight Navy voids structured by low-contrast luminous borders (`#C4D2E1` at 12–35% opacity) and high-contrast Cloud Blue text. Evokes sovereign terminal precision, after-hours boardroom focus, and zero cognitive fatigue.

### Core Tenets for Developers
1. **Zero Fluff & Pure Typography:** No loud gradients, neon drop shadows, or decorative cartoon illustrations. Visual weight is established through typographic cadence, micro-fine outlines, and tonal layering.
2. **Dual-Mode Symmetry:** Every component, card, badge, and input must have an explicit mapping for both Light Mode (`#F8FAFC` canvas) and Dark Mode (`#0A1B2E` canvas).
3. **Calibrated Soft Geometry:** Corner radii are disciplined at `0.25rem` (4px) for controls, `0.5rem` (8px) for cards/containers, and `9999px` exclusively for circular avatars and status indicator pips.
4. **Institutional Precision:** All numerical values, timestamps, protocol IDs, and schema labels must strictly utilize **JetBrains Mono** in uppercase with expanded tracking.

---

## 2. Color Architecture & Token Catalog

### 2.1 Master Color Matrix

| Semantic Role | Light Mode Token | Hex Code | Dark Mode Token | Hex Code | Purpose & Application |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Canvas Background** | `canvas-base` | `#F8FAFC` / `#F2F6FB` | `dark-canvas-base` | `#0A1B2E` | Page root background, viewport base |
| **Card / Surface (Level 1)** | `surface-card` | `#FFFFFF` | `dark-surface-card` | `#112A46` | Primary cards, activity streams, panels |
| **Recessed / Panel (Level 2)** | `surface-recessed` | `#F1F5F9` / `#F2F6FB` | `dark-surface-elevated` | `#1B3B5A` | Nested widgets, date selectors, poll tracks |
| **Hairline Border (Default)** | `border-subtle` | `#E2E8F0` / `#E1E8F0` | `dark-border-subtle` | `rgba(196,210,225,0.12)` | 1px structural dividing lines & card frames |
| **Active / Hover Border** | `border-focus` | `#CBD5E1` / `#1B3B5A` | `dark-border-focus` | `rgba(196,210,225,0.35)` | Interactive state hover and focus rings |
| **Primary Text / Anchor** | `text-primary` | `#0A1B2E` | `dark-text-primary` | `#FFFFFF` / `#E1E8F0` | Headlines, prominent figures, dominant labels |
| **Secondary Text** | `text-secondary` | `#475569` / `#557392` | `dark-text-secondary` | `#C4D2E1` (85%) | Body paragraphs, deliverables, descriptions |
| **Muted Metadata** | `text-muted` | `#64748B` / `#7A8CA6` | `dark-text-muted` | `#7A8CA6` / `#8E9195` | Timestamp tags, SLA counters, captions |
| **Placeholder Text** | `text-placeholder` | `#94A3B8` / `#A1B2C4` | `dark-text-placeholder` | `rgba(196,210,225,0.40)` | Form input hints and inactive placeholders |
| **Primary CTA Button** | `btn-primary-bg` | `#0A1B2E` (`#FFFFFF` text) | `dark-btn-primary-bg` | `#E1E8F0` (`#0A1B2E` text) | Dominant interactive triggers, "Book", "Submit" |
| **Secondary Button** | `btn-secondary-bg`| `#FFFFFF` (`#E2E8F0` border)| `dark-btn-secondary-bg`| `#112A46` (`12%` border) | Secondary actions, "Explore", "Filter" |
| **Status Pip: Live / Online** | `status-live` | `#10B981` (Emerald) | `dark-status-live` | `#34D399` (Soft Emerald) | SignalR synced, accepting engagements |
| **Status Pip: Alert / Error**| `status-error` | `#BA1A1A` / `#EF4444` | `dark-status-error` | `#F87171` (Crimson) | SLA breached, form error states |
| **Rating Accent** | `accent-gold` | `#D97706` / `#F59E0B` | `dark-accent-gold` | `#FBBF24` (Amber Gold) | Star ratings and review stars |

---

## 3. Typography System

The interface uses a three-tier typographic hierarchy:
* **Headlines & Display:** **Manrope** (or Hanken Grotesk) — Bold, tight tracking (`-0.025em`).
* **Body & Operational Copy:** **Inter** — Balanced, clean legibility with relaxed line-height.
* **Metadata, Counters, Protocols & Badges:** **JetBrains Mono** — Monospaced, uppercase, expanded tracking (`0.04em`–`0.08em`).

### Font Import Snippet
```html
<link rel="stylesheet" href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=JetBrains+Mono:wght@400;500;600;700&family=Manrope:wght@600;700;800&display=swap" />
```

### Typographic Scale

| Style Token | Font Family | Size | Weight | Line Height | Tracking | Usage |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| `display-hero` | Manrope | 32px / 2rem | 700 / Bold | 40px | `-0.025em` | Hero titles, profile names |
| `headline-lg` | Manrope | 24px / 1.5rem | 700 / Bold | 32px | `-0.02em` | Section headers ("Select Connection Pass") |
| `headline-md` | Manrope | 18px / 1.125rem| 600 / Semi | 26px | `-0.015em` | Card titles ("1-on-1 Strategic Advisory") |
| `headline-sm` | Manrope | 15px / 0.937rem| 600 / Semi | 22px | `-0.01em` | Hub names, widget headers |
| `body-lg` | Inter | 15px / 0.937rem| 400 / Normal| 24px | `0em` | Key briefing copy, lead paragraphs |
| `body-md` | Inter | 13px / 0.812rem| 400 / Normal| 20px | `0em` | Standard descriptions, card body text |
| `body-sm` | Inter | 12px / 0.75rem | 400 / Normal| 18px | `0em` | Secondary captions, sub-affiliations |
| `mono-label-md`| JetBrains Mono| 11px / 0.687rem| 600 / Semi | 16px | `+0.06em` | Header labels (`ACCESS PROTOCOL`, `DOMAIN EXPERTISE`)|
| `mono-label-sm`| JetBrains Mono| 10px / 0.625rem| 600 / Semi | 14px | `+0.08em` | Badges, chips (`TIER 0 VERIFIED`, `TOP 0.1%`) |
| `mono-data` | JetBrains Mono| 12px / 0.75rem | 500 / Medium| 16px | `+0.02em` | Wallets (`0x4F...89C`), timestamps, vote counts |

---

## 4. Layout Architecture & Spacing Rhythm

### 4.1 Fluid Grid Grid Rules
* **Desktop (1200px+):** 12-column layout, max-width `1400px` to `1440px`. Gutters: `2rem` (`32px`). Outer page margins: `2rem` to `3.5rem`.
* **Tablet (768px – 1199px):** 8-column layout. Gutters: `1.5rem` (`24px`). Side-by-side analytical panels collapse into stacked pairs.
* **Mobile (< 768px):** 4-column layout. Gutters: `1rem` (`16px`). Canvas padding: `1rem`. All multi-column blocks stack vertically.

### 4.2 Spacing Increments
Always use standard multiples of **4px / 8px**:
* `space-xs`: `0.25rem` (4px) — Spacing between icon and adjacent label.
* `space-sm`: `0.5rem` (8px) — Spacing within input fields, badge padding, gap in button internals.
* `space-md`: `1rem` (16px) — Gap between small cards, form input margins.
* `space-lg`: `1.5rem` (24px) — Internal padding for primary cards and analytical panels.
* `space-xl`: `2rem` to `2.5rem` (32px–40px) — Distance between distinct page sections.

---

## 5. Elevation, Depth & Geometry

### 5.1 Corner Radii Standard
```css
--radius-control: 0.25rem;  /* 4px: buttons, form inputs, chips, badges, date cards */
--radius-card:    0.5rem;   /* 8px: cards, analytical panels, banner containers */
--radius-modal:   0.75rem;  /* 12px: dialog boxes, flyout overlays */
--radius-pill:    9999px;   /* Full circle: avatars, online status pips, balance chips */
```

### 5.2 Elevation Rules (Shadows vs. Glows)
* **Light Mode:** Micro-fine shadow with navy-tinted ambient diffusion:
  * *Default Card:* `box-shadow: 0 2px 8px -2px rgba(10, 27, 46, 0.04), 0 1px 3px 0 rgba(10, 27, 46, 0.02);`
  * *Hovered Card:* `box-shadow: 0 8px 24px -4px rgba(10, 27, 46, 0.07), 0 2px 6px -1px rgba(10, 27, 46, 0.03);`
* **Dark Mode:** No external drop shadows. Uses **Luminous Edge Boundaries**:
  * *Default Card:* `1px solid rgba(196, 210, 225, 0.12);`
  * *Hovered Card:* Border brightness shifts to `rgba(196, 210, 225, 0.35);` with ambient inner glow: `0 0 16px rgba(225, 232, 240, 0.05);`

---

## 6. Standard Component Specifications

### 6.1 Top Protocol Navigation Header
Always rendered across top-level screens:
* **Brand:** `Community Link` logo with icon mark and subtitle `VERIFIED NODE PROTOCOL`.
* **Search:** Oval input with magnifying glass icon and placeholder: *"Search experts, passes, spaces..."*.
* **Navigation Links:** *"Explore & Marketplace"*, *"Profiles & Passes"* (active state), *"My Connections"*, *"Community Workspaces"*.
* **Status Chip:** `● 1,450 CREDITS · 3 ACTIVE PASSES` (monospaced pill).

### 6.2 Registry Breadcrumb & Status Bar
Separates global navigation from page context:
```html
<div class="flex items-center justify-between pb-3 border-b border-[#E2E8F0]">
    <div class="flex items-center gap-3">
        <button class="w-8 h-8 rounded bg-white border border-[#E2E8F0] text-[#64748B] flex items-center justify-center">←</button>
        <div class="font-mono text-xs text-[#64748B] uppercase tracking-wider">
            <span>REGISTRY</span> / <span>FRONTIER INTELLIGENCE</span> / <strong class="text-[#0A1B2E]">ELENA_VANCE_ID:8042</strong>
        </div>
    </div>
    <div class="flex items-center gap-3">
        <div class="font-mono text-[11px] font-semibold text-[#0A1B2E] bg-white border border-[#E2E8F0] px-3 py-1 rounded-full flex items-center gap-2">
            <span class="w-2 h-2 rounded-full bg-emerald-500 animate-pulse"></span>
            <span>ACCEPTING DIRECT ENGAGEMENTS</span>
        </div>
        <button class="w-8 h-8 rounded bg-white border border-[#E2E8F0] text-[#64748B]">↗</button>
        <button class="w-8 h-8 rounded bg-white border border-[#E2E8F0] text-[#64748B]">🔖</button>
    </div>
</div>
```

### 6.3 Buttons
* **Primary Button:**
  * *Light:* Background `#0A1B2E`, text `#FFFFFF`, radius `0.25rem`, padding `0.625rem 1.25rem`. Hover: `#112A46` with `-1px` lift.
  * *Dark:* Background `#E1E8F0`, text `#0A1B2E`, radius `0.25rem`, padding `0.625rem 1.25rem`. Hover: `#FFFFFF` with luminous bloom.
* **Secondary Button:**
  * *Light:* Background `#FFFFFF`, border `1px solid #E2E8F0`, text `#0A1B2E`. Hover: `#F1F5F9`.
  * *Dark:* Background `#112A46`, border `1px solid rgba(196,210,225,0.20)`, text `#E1E8F0`. Hover: `#1B3B5A`.
* **Ghost Button:** Transparent background, text `#557392` (Light) / `#C4D2E1` (Dark). Hover: `#F1F5F9` (Light) / `#112A46` (Dark).

### 6.4 Badges & Chips
All chips strictly use **JetBrains Mono**, uppercase, with `0.25rem` radius:
* **Standard Metadata Chip:**
  * *Light:* Background `#F1F5F9`, border `1px solid #E2E8F0`, text `#334155`.
  * *Dark:* Background `#1B3B5A` (60%), border `1px solid rgba(196,210,225,0.20)`, text `#C4D2E1`.
* **Active / Selected Chip:**
  * *Light:* Background `#0A1B2E`, border `1px solid #0A1B2E`, text `#FFFFFF`.
  * *Dark:* Background `#E1E8F0`, border `1px solid #E1E8F0`, text `#0A1B2E`.
* **Highlight / Most Popular Chip:**
  * Background `#0A1B2E`, text `#FFFFFF`, pill shape (`9999px`), padding `3px 12px` with `⚡` icon.

### 6.5 Three-Box Analytical Metrics Strip
Used on profile and dashboard views:
```html
<div class="grid grid-cols-3 border border-[#E2E8F0] rounded-lg overflow-hidden bg-white text-center py-3 divide-x divide-[#E2E8F0]">
    <div class="px-2">
        <div class="font-sans text-xl font-bold text-[#0A1B2E]">142</div>
        <div class="font-mono text-[10px] font-semibold text-[#64748B] uppercase tracking-wider mt-0.5">CONNECTIONS</div>
    </div>
    <div class="px-2">
        <div class="font-sans text-xl font-bold text-[#0A1B2E]">4.98 ★</div>
        <div class="font-mono text-[10px] font-semibold text-[#64748B] uppercase tracking-wider mt-0.5">118 REVIEWS</div>
    </div>
    <div class="px-2">
        <div class="font-sans text-xl font-bold text-[#0A1B2E]">3.4h</div>
        <div class="font-mono text-[10px] font-semibold text-[#64748B] uppercase tracking-wider mt-0.5">AVG SLA</div>
    </div>
</div>
```

### 6.6 Connection Pass Card Architecture
Used for protocol tokens, bookings, and VIP memberships:
1. **Highlight Tag (Top Right):** e.g. `⚡ MOST POPULAR ALLOCATION`.
2. **Title & Price Header:** Title (Manrope 18px Bold) + Chip (`Live Video`) alongside Large Price (`$450 / 45-MIN`) and availability counter (`3 Slots Left This Week`).
3. **Session Deliverables List:** Checkmark items with `(✓)` symbol in recessed circle.
4. **Interactive Date Grid:** 3 button slots with Day, Date, and Time. Active slot styled with solid Deep Navy `#0A1B2E` (or Cloud Blue in dark mode).
5. **Gateway Guarantee Footer:** Left encryption note (`🔒 Protected by Encrypted Video Gateway`) + Primary CTA (`Book & Unlock Connection →`).

### 6.7 Real-Time Peer Activity & Escrow Banners
* **Escrow Guarantee:** Shield icon, title, description, and right-aligned link `ESCROW PROTOCOLS ↗`.
* **Node Synchronized Ticker:**
  * Header label: `REAL-TIME PEER ACTIVITY` with pulsating `● NODE SYNCHRONIZED` emerald dot.
  * Rows displaying abbreviated cryptographic address (`0x4F...89C`), action description, and relative timestamp (`18 mins ago`).

### 6.8 Form Inputs & Selectors
* **Background:** `#FFFFFF` (Light) / `#0A1B2E` (Dark).
* **Border:** `1px solid #CBD5E1` (Light) / `1px solid #1B3B5A` (Dark), radius `0.25rem`.
* **Focus State:** Border shifts to `#0A1B2E` (Light) / `#E1E8F0` (Dark) with ambient halo: `box-shadow: 0 0 0 3px rgba(10, 27, 46, 0.10);` (Light) or `rgba(225, 232, 240, 0.15)` (Dark).
* **Placeholder:** `#94A3B8` (Light) / `rgba(196,210,225,0.40)` (Dark).

---

## 7. Master CSS Utility Reference Class Guide

Developers can directly implement these utility classes or Tailwind mappings in any Blazor page:

```css
/* ============================================================
   COMMUNITYLINK EXECUTIVE DESIGN SYSTEM - UTILITY CLASSES
   ============================================================ */

/* Typography */
.font-display { font-family: 'Manrope', sans-serif; letter-spacing: -0.025em; }
.font-body    { font-family: 'Inter', sans-serif; }
.font-mono    { font-family: 'JetBrains Mono', monospace; }

/* Structural Canvases */
.bg-canvas-light { background-color: #F8FAFC; color: #0A1B2E; }
.bg-canvas-dark  { background-color: #0A1B2E; color: #E1E8F0; }

/* Cards & Surfaces */
.card-executive {
    background-color: #FFFFFF;
    border: 1px solid #E2E8F0;
    border-radius: 0.5rem;
    box-shadow: 0 2px 8px -2px rgba(10, 27, 46, 0.04), 0 1px 3px 0 rgba(10, 27, 46, 0.02);
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
}
.dark .card-executive {
    background-color: #112A46;
    border: 1px solid rgba(196, 210, 225, 0.12);
    box-shadow: none;
}
.card-executive:hover {
    border-color: #CBD5E1;
    box-shadow: 0 8px 24px -4px rgba(10, 27, 46, 0.07);
}
.dark .card-executive:hover {
    border-color: rgba(196, 210, 225, 0.35);
    box-shadow: 0 0 16px rgba(225, 232, 240, 0.05);
}

/* Recessed Panels */
.panel-recessed {
    background-color: #F1F5F9;
    border: 1px solid #E2E8F0;
    border-radius: 0.375rem;
}
.dark .panel-recessed {
    background-color: #1B3B5A;
    border: 1px solid rgba(196, 210, 225, 0.15);
}

/* Buttons */
.btn-exec-primary {
    background-color: #0A1B2E;
    color: #FFFFFF !important;
    border: 1px solid #0A1B2E;
    border-radius: 0.25rem;
    padding: 0.625rem 1.25rem;
    font-size: 13px;
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    transition: all 0.18s ease;
    text-decoration: none;
    cursor: pointer;
}
.btn-exec-primary:hover:not(:disabled) {
    background-color: #112A46;
    transform: translateY(-1px);
    box-shadow: 0 4px 14px -2px rgba(10, 27, 46, 0.18);
}
.dark .btn-exec-primary {
    background-color: #E1E8F0;
    color: #0A1B2E !important;
    border-color: #E1E8F0;
}
.dark .btn-exec-primary:hover:not(:disabled) {
    background-color: #FFFFFF;
    box-shadow: 0 0 16px rgba(225, 232, 240, 0.25);
}

.btn-exec-secondary {
    background-color: #FFFFFF;
    border: 1px solid #E2E8F0;
    color: #0A1B2E !important;
    border-radius: 0.25rem;
    padding: 0.5rem 1rem;
    font-size: 12px;
    font-weight: 600;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    transition: all 0.18s ease;
    cursor: pointer;
}
.btn-exec-secondary:hover {
    background-color: #F1F5F9;
    border-color: #CBD5E1;
}
.dark .btn-exec-secondary {
    background-color: #112A46;
    border-color: rgba(196, 210, 225, 0.20);
    color: #E1E8F0 !important;
}
.dark .btn-exec-secondary:hover {
    background-color: #1B3B5A;
    border-color: rgba(196, 210, 225, 0.40);
}

/* Badges & Chips */
.chip-standard {
    background-color: #F1F5F9;
    border: 1px solid #E2E8F0;
    color: #334155;
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    font-weight: 600;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    border-radius: 0.25rem;
    padding: 2px 7px;
    display: inline-flex;
    align-items: center;
    gap: 4px;
}
.dark .chip-standard {
    background-color: rgba(27, 59, 90, 0.6);
    border-color: rgba(196, 210, 225, 0.2);
    color: #C4D2E1;
}

.chip-active {
    background-color: #0A1B2E;
    border: 1px solid #0A1B2E;
    color: #FFFFFF;
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    font-weight: 600;
    letter-spacing: 0.08em;
    text-transform: uppercase;
    border-radius: 0.25rem;
    padding: 2px 7px;
    display: inline-flex;
    align-items: center;
    gap: 4px;
}
.dark .chip-active {
    background-color: #E1E8F0;
    border-color: #E1E8F0;
    color: #0A1B2E;
}

/* Input Fields */
.input-executive {
    background-color: #FFFFFF;
    border: 1px solid #CBD5E1;
    border-radius: 0.25rem;
    padding: 0.5rem 0.75rem;
    font-size: 13px;
    color: #0A1B2E;
    font-family: 'Inter', sans-serif;
    outline: none;
    transition: border-color 0.15s, box-shadow 0.15s;
}
.input-executive:focus {
    border-color: #0A1B2E;
    box-shadow: 0 0 0 3px rgba(10, 27, 46, 0.10);
}
.dark .input-executive {
    background-color: #0A1B2E;
    border-color: #1B3B5A;
    color: #E1E8F0;
}
.dark .input-executive:focus {
    border-color: #E1E8F0;
    box-shadow: 0 0 0 3px rgba(225, 232, 240, 0.15);
}
```

---

## 8. Screen Implementation Checklist for Developers

When building or updating ANY screen across CommunityLink, verify against this checklist:

* [ ] **Mode Support:** Does the page respect both Light (`#F8FAFC`) and Dark (`#0A1B2E`) background environments?
* [ ] **Typography Rule:** Are headings strictly **Manrope**, body text **Inter**, and counters/dates/IDs **JetBrains Mono**?
* [ ] **Font Casing:** Are section metadata labels (`ACCESS PROTOCOL`, `DOMAIN EXPERTISE`, `CREDENTIAL STATUS`) in all-caps with expanded tracking (`0.06em` to `0.08em`)?
* [ ] **Corner Radius Discipline:** Are all buttons, inputs, and chips set to `0.25rem` (4px)? Are cards set to `0.5rem` (8px)? (No excessive rounded-2xl on buttons or cards).
* [ ] **Hairline Dividers:** Are dividers razor-thin (`1px solid #E2E8F0` for light, `1px solid rgba(196,210,225,0.12)` for dark)?
* [ ] **Status Indicators:** Do online/active indicators use a pure circle (`w-2 h-2 rounded-full`) with the standard emerald `#10B981` / `#34D399` tone?
* [ ] **Metrics Structure:** Are KPI metrics styled cleanly with high-contrast numerals and secondary uppercase monospaced labels underneath?
* [ ] **Interactive Hover:** Do cards provide subtle border brightness transitions without erratic resizing or loud colored glows?
* [ ] **No Raw Primary Blues:** Avoid generic browser blues (e.g. `#0000FF` or `#3B82F6`); strictly use **Deep Navy (`#0A1B2E`)**, **Midnight Blue (`#112A46`)**, and **Cloud Blue (`#E1E8F0`)**.

---
*Maintained by the CommunityLink Core Architecture Group. Refer to this document as the sole source of truth for all UI engineering.*
