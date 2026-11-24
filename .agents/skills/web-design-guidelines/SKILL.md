---
name: web-design-guidelines
description: >-
  Official Web Interface Guidelines review skill from vercel-labs/agent-skills.
  Use when reviewing UI code, checking accessibility (a11y), auditing UX,
  and ensuring responsive design against web interface standards.
metadata:
  author: vercel-labs
  source: https://github.com/vercel-labs/agent-skills/tree/main/skills/web-design-guidelines
---

# Web Interface Guidelines (Vercel Official Agent Skills)

This skill provides guidelines and auditing criteria from Vercel's official `vercel-labs/agent-skills` repository for reviewing UI code against Web Interface Guidelines.

## When to Apply

- Reviewing frontend components for accessibility (a11y) and user experience.
- Auditing interactive elements, touch targets, and visual feedback states.
- Checking web layouts for responsiveness and cross-device consistency.

---

## 1. Accessibility (a11y) Standards

- **Semantic HTML**: Use native semantic elements instead of unstyled `div`s with click listeners.
  - Interactive board squares: `<button>` or `role="button"` with `tabIndex={0}` and keyboard event listeners.
- **ARIA Labels & Roles**:
  - Provide descriptive `aria-label`s for non-textual elements (e.g., `aria-label="Cell row 2 column 3, Black King"`, `aria-label="Crown icon"`).
  - Use `aria-live="polite"` for game status and turn change notifications so screen readers announce turns.
- **Focus Indicators**: Never disable outline without providing a visible `:focus-visible` ring.

## 2. Interactive States & Touch Targets

- **Touch Target Sizing**: Interactive elements (like checker cells or buttons) must have a minimum touch target area of 44x44 CSS pixels.
- **Visual Feedback**: Every interactive component must exhibit distinct states for:
  - Default
  - Hover (`:hover`)
  - Active / Pressed (`:active`)
  - Selected / Focused (`:focus-visible` / `ring`)
  - Disabled (`aria-disabled="true"`, reduced opacity, cursor not-allowed)

## 3. Visual Polish & Layout

- **Color Contrast**: Ensure text and game piece colors maintain at least a 4.5:1 contrast ratio against cell backgrounds (WCAG AA).
- **Responsive Layouts**:
  - Pointers and touch interactions should function smoothly across mobile and desktop.
  - Board should scale fluidly using CSS Grid or Flexbox without causing horizontal page scroll on small viewports.
- **Terse Feedback**: Present errors and notifications cleanly, near the source of interaction.
