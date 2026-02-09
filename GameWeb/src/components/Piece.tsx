import { Color, PieceType } from "../types/game";
import KingIcon from "./KingIcon";

type PropsType = {
  color: Color;
  type: PieceType;
};

export default function Piece({ color, type }: PropsType) {
  return (
    <div
      style={{
        width: 44,
        height: 44,
        borderRadius: "50%",
        background: color === Color.Black ? "#222" : "#f0f0f0",
        border: color === Color.Black ? "2px solid #111" : "2px solid #ccc",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        boxShadow: "0 4px 8px rgba(0,0,0,0.5)",
        fontSize: "1.6rem",
        fontWeight: "bold",
        color: color === Color.Black ? "#ffcc00" : "#333",
      }}
    >
      {type === PieceType.King && <KingIcon color={color} />}
    </div>
  );
}
