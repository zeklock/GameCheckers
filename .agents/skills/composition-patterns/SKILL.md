---
name: composition-patterns
description: >-
  Official React component composition guidelines from vercel-labs/agent-skills.
  Avoid boolean prop proliferation using compound components, lifting state,
  custom hooks, and modern React 19 composition patterns.
metadata:
  author: vercel-labs
  source: https://github.com/vercel-labs/agent-skills/tree/main/skills/composition-patterns
---

# React Composition Patterns (Vercel Official Agent Skills)

This skill provides component composition guidelines from Vercel's official `vercel-labs/agent-skills` repository. It focuses on building flexible, maintainable React components by avoiding boolean prop proliferation and leveraging compound components.

## When to Apply

- Refactoring components with numerous boolean flags (`isSelected`, `isAvailable`, `isDark`, `isKing`, `hasIcon`).
- Designing clean, scalable component APIs for game boards, tiles, and pieces.
- Structuring layout and state boundaries for human and AI maintainability.

## Rule Categories by Priority

| Priority | Category | Impact | Prefix |
| :--- | :--- | :--- | :--- |
| **1** | Component Architecture | `HIGH` | `architecture-` |
| **2** | State Management | `MEDIUM` | `state-` |
| **3** | Implementation Patterns | `MEDIUM` | `patterns-` |
| **4** | React 19 APIs | `MEDIUM` | `react19-` |

---

## 1. Component Architecture (`architecture-`)

### Avoid Boolean Prop Proliferation
Instead of adding multiple boolean flags to a component, compose specialized child components or use compound component patterns:

```typescript
// Anti-pattern: Boolean prop soup
<Cell isDark isSelected isHighlight isAvailable hasPiece pieceColor="black" isKing onMove={...} />

// Recommended: Compound Component Composition
<Board.Cell position={pos} isDark={true}>
  {isAvailable && <Board.MoveTarget onClick={() => handleMove(pos)} />}
  {piece && (
    <Board.Piece color={piece.color} isKing={piece.type === PieceType.King} />
  )}
</Board.Cell>
```

## 2. State Management (`state-`)

- **Lift State to Nearest Common Ancestor**: Keep state local until shared between components.
- **Split Contexts**: Separate high-frequency state (e.g. cursor hover, selected piece) from low-frequency state (game status, player names) to avoid re-rendering the entire board unnecessarily.
- **Derive State During Render**: Avoid syncing state in `useEffect`. If available moves can be computed from `selectedPiece` and `game.availablePieces`, compute it directly.

## 3. Implementation Patterns (`patterns-`)

- **Slots Pattern**: Allow passing custom UI into designated component slots (e.g. `headerSlot`, `footerSlot`) instead of configuring nested props.
- **Custom Hook Encapsulation**: Extract complex game logic, API polling, or keyboard listeners into dedicated hooks (`useCheckersGame()`, `useBoardInteractions()`).

## 4. Modern React 19 APIs (`react19-`)

- **Direct Ref Passing**: In React 19, `ref` can be passed as a standard prop to functional components without needing `forwardRef`.
- **`use()` Hook for Context**: Use the new `use(Context)` API for conditional reading of context within loops or branches where applicable.
