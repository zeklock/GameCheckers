---
name: react-best-practices
description: >-
  Official Vercel React and Next.js performance optimization guidelines from vercel-labs/agent-skills.
  Covers eliminating waterfalls, bundle size optimization, client-side data fetching,
  re-render minimization, rendering performance, and React 19 standards.
metadata:
  author: vercel-labs
  source: https://github.com/vercel-labs/agent-skills/tree/main/skills/react-best-practices
---

# React Best Practices (Vercel Official Agent Skills)

This skill provides performance optimization rules and architectural guidelines from Vercel's official `vercel-labs/agent-skills` repository for building high-performance React applications.

## Rule Categories by Priority

| Priority | Category | Impact Level | Prefix Tag |
| :--- | :--- | :--- | :--- |
| **1** | **Eliminating Waterfalls** | `CRITICAL` | `async-` |
| **2** | **Bundle Size Optimization** | `CRITICAL` | `bundle-` |
| **3** | **Server-Side Performance** | `HIGH` | `server-` |
| **4** | **Client-Side Data Fetching** | `MEDIUM-HIGH` | `client-` |
| **5** | **Re-render Optimization** | `MEDIUM` | `rerender-` |
| **6** | **Rendering Performance** | `MEDIUM` | `rendering-` |
| **7** | **JavaScript Performance** | `LOW-MEDIUM` | `js-` |
| **8** | **Advanced Patterns** | `LOW` | `advanced-` |

---

## 1. Eliminating Waterfalls (`async-`) — Critical

- **Parallel Data Fetching**: Never chain independent asynchronous operations sequentially. Use `Promise.all` or `Promise.allSettled`:
  ```typescript
  // Bad: sequential waterfall
  const board = await fetchBoard();
  const players = await fetchPlayers();

  // Good: parallel execution
  const [board, players] = await Promise.all([fetchBoard(), fetchPlayers()]);
  ```
- **Hoist Requests**: Trigger data fetches as high up in the component tree or route loaders as possible to prevent render-blocking cascades.

## 2. Bundle Size Optimization (`bundle-`) — Critical

- **Avoid Full Barrel Imports**: Avoid importing large libraries via their root index if not tree-shaken:
  ```typescript
  // Prefer specific imports over barrel imports for large libraries
  import { CheckersIcon } from './icons/CheckersIcon';
  ```
- **Dynamic Imports (`React.lazy`)**: Split non-critical route components or heavy dialogs (like `GameOver` modal or complex SVG assets) with `React.lazy` and `Suspense`.

## 3. Client-Side Data Fetching (`client-`) — Medium-High

- **Avoid Raw `useEffect` for Data Fetching**: Raw `useEffect` fetching lacks automatic deduplication, caching, and cleanup, leading to race conditions and double requests.
- **Deduplication & Error Handling**: Ensure state updates check if the component is mounted or use an abort controller when fetching board moves or starting games.

## 4. Re-render Optimization (`rerender-`) — Medium

- **Move State Down**: If only a small part of a component tree needs a piece of state, isolate it to that child component rather than lifting it to the entire page.
- **Pass JSX as Children**: When an expensive container updates, pass unchanged children via the `children` prop to prevent unnecessary re-rendering of nested trees.
- **Do Not Prematurely Memoize**: Use `useMemo` and `useCallback` only when passing callbacks to memoized children (`React.memo`) or when calculating expensive board transformations.

## 5. Rendering Performance (`rendering-`) — Medium

- **Stable and Unique Keys**: In grid boards (like Checkers), always use stable, deterministic keys (e.g. ``${cell.position.x}-${cell.position.y}``) rather than array indexes.
- **Pure Functional Components**: Render functions must be strictly pure. Never mutate props or existing state objects directly:
  ```typescript
  // Bad: direct mutation
  state.board[y][x] = piece;

  // Good: immutable update
  setBoard(prev => prev.map((row, r) => r === y ? ... : row));
  ```

## 6. JavaScript & React 19 Patterns (`advanced-`)

- **React 19 Actions & Transitions**: Use `useTransition` to mark board moves as non-blocking transitions, preserving UI responsiveness during API calls.
- **Optimistic Updates**: For turn-based game moves, apply immediate optimistic state to the board while the backend request is in flight.
