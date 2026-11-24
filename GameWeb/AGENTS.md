# GameWeb Guidelines

`GameWeb` is a React 19 + TypeScript 5.9 application built with Vite 7 and Tailwind CSS 4. It uses React Router 7 and Playwright for end-to-end testing.

## Structure and Behavior
- Keep HTTP calls and API response mapping in `src/api/gameApi.ts`; use `src/hooks/useCheckersGame.ts` for game interaction state and `src/components/` and `src/pages/` for the UI.
- The client currently uses `http://localhost:5271/api/game`. Keep it consistent with the API's `GameWeb` CORS origin (`http://localhost:5173`) and the Playwright web-server configuration when changing local development endpoints.
- Routes currently include `/`, `/game`, and `/game-over`.
- Preserve zero-based `x` and `y` board coordinates across the UI/API boundary. The UI may format positions for display, but must not alter coordinates sent to the API.
- Keep interactive board cells accessible with meaningful labels and keyboard-visible focus; preserve existing responsive styling and Tailwind conventions.

## Validation
- `npm run build` runs the TypeScript project build and Vite production build.
- `npm run lint` runs ESLint.
- `npm run test:e2e` runs Playwright; its configuration starts the GameApi and Vite development server. Existing end-to-end tests cover a valid opening move and rejection of duplicate player names.
- Keep this file and the repository-level `AGENTS.md` current whenever fixing code or changing UI/API behavior, architecture, dependencies, commands, or constraints.
