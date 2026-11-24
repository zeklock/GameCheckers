import type { ReactNode } from "react";
import type { PositionDto } from "../types/game";

type CellProps = {
  position: PositionDto;
  isDark: boolean;
  isSelected?: boolean;
  isSelectable?: boolean;
  hasPiece?: boolean;
  onClick?: (pos: PositionDto) => void;
  children?: ReactNode;
};

export default function Cell({
  position,
  isDark,
  isSelected = false,
  isSelectable = false,
  hasPiece = false,
  onClick,
  children,
}: CellProps) {
  const handleClick = () => {
    if (onClick) {
      onClick(position);
    }
  };

  const colLetter = String.fromCharCode(65 + position.x);
  const rowNum = position.y + 1;
  const cellLabel = `Cell ${colLetter}${rowNum}${isDark ? " (dark)" : " (light)"}${isSelected ? " - selected" : ""}${isSelectable ? " - available target" : ""}`;

  // Dynamic state ring / outline classes
  let stateClasses = "";
  if (isSelected) {
    stateClasses = "ring-4 ring-blue-500 ring-offset-1 ring-offset-amber-900 z-10 scale-102";
  } else if (isSelectable) {
    if (hasPiece) {
      stateClasses = "ring-4 ring-emerald-500 ring-offset-1 ring-offset-amber-900 animate-pulse z-10 cursor-pointer";
    } else {
      stateClasses = "ring-4 ring-amber-400 ring-dashed ring-offset-1 ring-offset-amber-900 z-10 cursor-pointer";
    }
  }

  return (
    <button
      type="button"
      onClick={handleClick}
      aria-label={cellLabel}
      tabIndex={0}
      className={`
        w-[60px] h-[60px] flex items-center justify-center p-0 m-0 border-0 outline-none
        transition-all duration-150 relative select-none
        focus-visible:ring-4 focus-visible:ring-indigo-400 focus-visible:z-20
        ${isDark ? "bg-[#8B5A2B] hover:brightness-110" : "bg-[#F5DEB3]"}
        ${stateClasses}
      `}
    >
      {children}
    </button>
  );
}
