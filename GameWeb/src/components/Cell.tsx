import { type PieceDto, type PositionDto } from "../types/game";
import Piece from "./Piece";

type Props = {
  cell?: {
    position: PositionDto;
    piece?: PieceDto | null;
  };
  onClick?: (pos: PositionDto) => void;
  isSelectable?: boolean;
  isSelected?: boolean;
};

export default function Cell({
  cell,
  onClick,
  isSelectable,
  isSelected,
}: Props) {
  const { position, piece } = cell || {};
  const isDark = position ? (position.x + position.y) % 2 === 1 : false;

  const handleClick = () => {
    if (onClick && position) onClick(position);
  };

  // Gunakan outline untuk highlight biar ga nambah ukuran cell (no layout shift)
  let outlineStyle = "none";
  let outlineColor = "#000000";
  let outlineWidth = "0";
  let outlineOffset = "0";
  let boxShadow = "inset 0 0 8px rgba(0,0,0,0.3)";
  let background = isDark ? "#8B5A2B" : "#F5DEB3";

  if (isSelected) {
    outlineStyle = "solid";
    outlineColor = "#2196f3"; // biru untuk selected
    outlineWidth = "3px";
    outlineOffset = "2px";
    boxShadow = "0 0 12px #64b5f6, inset 0 0 8px rgba(0,0,0,0.3)";
    background = "#2196f3"; // biru untuk selected
  } else if (isSelectable) {
    if (piece) {
      outlineStyle = "solid";
      outlineColor = "#4caf50"; // hijau untuk piece yang bisa digerakkan
      outlineWidth = "3px";
      outlineOffset = "2px";
      boxShadow = "0 0 10px #81c784, inset 0 0 6px rgba(0,0,0,0.3)";
      background = "#4caf50"; // hijau untuk piece yang bisa digerakkan
    } else {
      outlineStyle = "dashed";
      outlineColor = "#ff9800"; // oranye dashed untuk target kosong
      outlineWidth = "3px";
      outlineOffset = "2px";
      boxShadow = "0 0 8px #ffb74d";
      background = "#ff9800"; // oranye dashed untuk target kosong
    }
  }

  return (
    <div
      onClick={handleClick}
      style={{
        width: 60,
        height: 60,
        background,
        boxSizing: "border-box",
        outline: `${outlineWidth} ${outlineStyle} ${outlineColor}`,
        outlineOffset,
        boxShadow,
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        cursor: onClick ? "pointer" : "default",
        transition: "all 0.2s ease",
      }}
    >
      {piece && <Piece color={piece.color} type={piece.type} />}
    </div>
  );
}
