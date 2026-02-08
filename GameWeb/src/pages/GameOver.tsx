import { useEffect, useState } from "react";
import { Color, type GameDto } from "../types/game";
import { useNavigate } from "react-router-dom";

export default function GameOver() {
  const [game, setGame] = useState<GameDto | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const saved = localStorage.getItem("currentGame");
    if (!saved) {
      navigate("/");
      return;
    }
    try {
      setGame(JSON.parse(saved));
    } catch {
      localStorage.removeItem("currentGame");
      navigate("/");
    }
  }, [navigate]);

  if (!game) return null;

  const winnerName = game.winner?.name;
  const winnerColor = game.winner
    ? game.winner.color === Color.Black
      ? "Black"
      : "White"
    : null;

  return (
    <div style={{ textAlign: "center", padding: 20 }}>
      <h2>Game Over</h2>
      {game.winner ? (
        <p>
          <strong>{winnerName}</strong> win! ({winnerColor})
        </p>
      ) : (
        <p>Seri / Tidak ada pemenang</p>
      )}

      {game.notifications?.length > 0 && (
        <div
          style={{
            marginTop: 20,
            textAlign: "left",
            maxWidth: 500,
            margin: "20px auto",
          }}
        >
          <h4>Last Notifications</h4>
          <ul>
            {game.notifications.map((n, i) => (
              <li key={i}>{n}</li>
            ))}
          </ul>
        </div>
      )}

      <button
        onClick={() => {
          localStorage.removeItem("currentGame");
          navigate("/");
        }}
        style={{ marginTop: 20 }}
      >
        Back to Home
      </button>
    </div>
  );
}
