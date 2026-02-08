import type { GameDto, PositionDto } from "../types/game";
import Cell from "./Cell";

type Props = {
  board: GameDto["board"];
  onCellClick?: (pos: PositionDto) => void;
  selectableTargets: string[];
  selected?: PositionDto | null;
};

export default function Board({
  board,
  onCellClick,
  selectableTargets = [],
  selected,
}: Props) {
  // Guard ekstra: jangan render grid kalau board invalid
  if (
    !board ||
    typeof board.size !== "number" ||
    board.size < 1 ||
    !Array.isArray(board.cells)
  ) {
    console.warn("Board component received invalid board:", board);
    return (
      <div
        style={{
          textAlign: "center",
          padding: "40px",
          color: "salmon",
          border: "2px dashed salmon",
          borderRadius: "8px",
          maxWidth: "600px",
          margin: "20px auto",
        }}
      >
        Board data is invalid or not loaded yet.
        <br />
        <small>(size: {board?.size ?? "undefined"})</small>
      </div>
    );
  }

  const keyFor = (pos: PositionDto) => `${pos.x}_${pos.y}`;

  const sortedCells = [...board.cells].sort((a, b) => {
    if (a.position.y !== b.position.y) return a.position.y - b.position.y;
    return a.position.x - b.position.x;
  });

  return (
    <div
      style={{
        display: "grid",
        gridTemplateColumns: `repeat(${board.size}, 60px)`,
        gridGap: "2px",
        background: "#8B5A2B",
        padding: "10px",
        borderRadius: "8px",
        margin: "20px auto",
        width: "fit-content",
        boxShadow: "0 4px 12px rgba(0,0,0,0.4)",
      }}
    >
      {sortedCells.map((cell) => (
        <Cell
          key={keyFor(cell.position)}
          cell={cell}
          onClick={onCellClick}
          isSelectable={selectableTargets.includes(keyFor(cell.position))}
          isSelected={
            selected &&
            selected.x === cell.position.x &&
            selected.y === cell.position.y
          }
        />
      ))}
    </div>
  );
}
