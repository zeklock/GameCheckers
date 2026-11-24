import { Color, PieceType } from "../types/game";
import KingIcon from "./KingIcon";

type PieceProps = {
  color: Color;
  type: PieceType;
  className?: string;
};

export default function Piece({ color, type, className = "" }: PieceProps) {
  const isBlack = color === Color.Black;
  const isKing = type === PieceType.King;
  const label = `${isBlack ? "Black" : "White"} ${isKing ? "King" : "Man"}`;

  return (
    <div
      role="img"
      aria-label={label}
      className={`
        w-11 h-11 rounded-full flex items-center justify-center font-bold text-xl select-none
        transition-all duration-200 transform hover:scale-105 shadow-md
        ${
          isBlack
            ? "bg-stone-900 border-2 border-stone-800 text-amber-400 shadow-stone-950/70"
            : "bg-stone-100 border-2 border-stone-300 text-stone-800 shadow-stone-900/40"
        }
        ${className}
      `}
    >
      {isKing && <KingIcon color={color} />}
    </div>
  );
}
