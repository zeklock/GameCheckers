import { useMemo } from "react";
import type { GameDto, PositionDto } from "../types/game";
import Cell from "./Cell";
import Piece from "./Piece";

type BoardProps = {
  board: GameDto["board"];
  onCellClick?: (pos: PositionDto) => void;
  selectableTargets?: string[];
  selected?: PositionDto | null;
};

// Target marker for compound composition
export function MoveTarget({ onClick }: { onClick?: () => void }) {
  return (
    <div
      onClick={onClick}
      className="w-4 h-4 rounded-full bg-amber-400/90 shadow-md animate-ping"
      aria-hidden="true"
    />
  );
}

export default function Board({
  board,
  onCellClick,
  selectableTargets = [],
  selected,
}: BoardProps) {
  const keyFor = (pos: PositionDto) => `${pos.x}_${pos.y}`;

  // Memoize deterministic cell sorting
  const sortedCells = useMemo(() => {
    if (!board?.cells || !Array.isArray(board.cells)) return [];
    return [...board.cells].sort((a, b) => {
      if (a.position.y !== b.position.y) return a.position.y - b.position.y;
      return a.position.x - b.position.x;
    });
  }, [board?.cells]);

  if (!board || typeof board.size !== "number" || board.size < 1) {
    return (
      <div className="text-center p-8 text-rose-400 border-2 border-dashed border-rose-500 rounded-xl max-w-md mx-auto">
        Board data is invalid or not loaded yet.
      </div>
    );
  }

  return (
    <div
      role="grid"
      aria-label="Checkers Board"
      className="p-3 bg-[#5c3a1e] rounded-xl shadow-2xl mx-auto border-4 border-[#3e230e]"
      style={{
        display: "grid",
        gridTemplateColumns: `repeat(${board.size}, 60px)`,
        gap: "2px",
        width: "fit-content",
      }}
    >
      {sortedCells.map((cell) => {
        const key = keyFor(cell.position);
        const isDark = (cell.position.x + cell.position.y) % 2 === 1;
        const isSelected = selected
          ? selected.x === cell.position.x && selected.y === cell.position.y
          : false;
        const isSelectable = selectableTargets.includes(key);

        return (
          <Cell
            key={key}
            position={cell.position}
            isDark={isDark}
            isSelected={isSelected}
            isSelectable={isSelectable}
            hasPiece={!!cell.piece}
            onClick={onCellClick}
          >
            {cell.piece ? (
              <Piece color={cell.piece.color} type={cell.piece.type} />
            ) : isSelectable ? (
              <MoveTarget onClick={() => onCellClick?.(cell.position)} />
            ) : null}
          </Cell>
        );
      })}
    </div>
  );
}

// Attach Compound Components for flexible modern composition
Board.Cell = Cell;
Board.Piece = Piece;
Board.MoveTarget = MoveTarget;
