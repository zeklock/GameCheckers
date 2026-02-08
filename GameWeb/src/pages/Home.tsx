import { useState } from "react";
import { Color, type PlayerDto } from "../types/game";
import { useNavigate } from "react-router-dom";
import { gameApi } from "../api/gameApi";

export default function Home() {
  const navigate = useNavigate();
  const [p1, setP1] = useState("Player 1");
  const [p2, setP2] = useState("Player 2");
  const [c1, setC1] = useState<Color>(Color.Black);
  const [c2, setC2] = useState<Color>(Color.White);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const start = async () => {
    if (c1 === c2) {
      setError("Players must have different colors");
      return;
    }

    setLoading(true);
    setError(null);

    const players: PlayerDto[] = [
      { name: p1.trim() || "Player 1", color: c1 },
      { name: p2.trim() || "Player 2", color: c2 },
    ];

    try {
      const game = await gameApi.start(players);
      localStorage.setItem("currentGame", JSON.stringify(game));
      navigate("/game");
    } catch (err: any) {
      setError(err.message || "Failed to start the game");
    } finally {
      setLoading(false);
    }
  };

  return (
    <div style={{ textAlign: "center", padding: 20 }}>
      <h1>Checkers Game</h1>
      <div
        style={{ display: "grid", gap: 12, maxWidth: 400, margin: "0 auto" }}
      >
        <div>
          <label>Player 1 Name</label>
          <input value={p1} onChange={(e) => setP1(e.target.value)} />
        </div>
        <div>
          <label>Player 1 Color</label>
          <select
            value={c1}
            onChange={(e) => setC1(Number(e.target.value) as Color)}
          >
            <option value={Color.Black}>Black</option>
            <option value={Color.White}>White</option>
          </select>
        </div>

        <div>
          <label>Player 2 Name</label>
          <input value={p2} onChange={(e) => setP2(e.target.value)} />
        </div>
        <div>
          <label>Player 2 Color</label>
          <select
            value={c2}
            onChange={(e) => setC2(Number(e.target.value) as Color)}
          >
            <option value={Color.White}>White</option>
            <option value={Color.Black}>Black</option>
          </select>
        </div>

        <button onClick={start} disabled={loading}>
          {loading ? "Starting..." : "Start Game"}
        </button>

        {error && <div style={{ color: "salmon" }}>{error}</div>}
      </div>
    </div>
  );
}
