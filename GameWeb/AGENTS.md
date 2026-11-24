# GameWeb (React 19 Frontend) Guidelines

This directory contains the React 19 frontend application for the Checkers game.

## Tech Stack
- React 19, TypeScript 5.9, Vite 7, Tailwind CSS v4, React Router 7.

## Official Skills (from vercel-labs/agent-skills)
- **`react-best-practices`**: Refer to this skill for eliminating waterfalls, bundle optimization, and preventing re-renders.
- **`composition-patterns`**: Refer to this skill for clean compound component APIs and avoiding boolean prop proliferation.
- **`web-design-guidelines`**: Refer to this skill for auditing UI, accessibility (a11y), and responsive touch targets.

## Coding Standards & Rules
1. **Component Composition**:
   - Favor compound components over boolean flags.
   - Maintain pure components; do not mutate props or state directly.
2. **API Communication**:
   - All network requests to `GameApi` must go through `src/api/gameApi.ts`.
   - Avoid waterfalls; run independent requests concurrently with `Promise.all`.
3. **Styling & Accessibility**:
   - Use Tailwind CSS utility classes.
   - Ensure interactive cells have accessible roles, labels, and visible focus rings.
4. **Verification**:
   - Run `npm run build` or `npm run lint` before committing frontend changes.
