import { useState } from "react";
import { Color, type PlayerDto } from "../types/game";
import { useNavigate } from "react-router-dom";
import { gameApi } from "../api/gameApi";
import Button from "../components/Button";

export default function Home() {
  const navigate = useNavigate();
  const [p1, setP1] = useState("Player 1");
  const [p2, setP2] = useState("Player 2");
  // true = Player 1 Black & Player 2 White
  // false = Player 1 White & Player 2 Black
  const [player1IsBlack, setPlayer1IsBlack] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const start = async () => {
    setLoading(true);
    setError(null);

    const players: PlayerDto[] = [
      {
        name: p1.trim() || "Player 1",
        color: player1IsBlack ? Color.Black : Color.White,
      },
      {
        name: p2.trim() || "Player 2",
        color: player1IsBlack ? Color.White : Color.Black,
      },
    ];

    try {
      await gameApi.start(players);
      navigate("/game");
    } catch (err: any) {
      setError(err.message || "Failed to start the game");
    } finally {
      setLoading(false);
    }
  };

  const toggleColor = () => {
    setPlayer1IsBlack((prev) => !prev);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-md bg-gray-800/70 backdrop-blur-sm rounded-2xl shadow-2xl border border-gray-700/50 p-8 space-y-8">
        <div className="text-center">
          <h1 className="text-4xl md:text-5xl font-bold text-white tracking-tight">
            Checkers Game
          </h1>
          <p className="mt-2 text-gray-400">Classic board game</p>
        </div>

        <div className="space-y-6">
          {/* Player 1 */}
          <div className="space-y-2">
            <label className="block text-sm font-medium text-gray-300">
              Player 1
            </label>
            <input
              type="text"
              value={p1}
              onChange={(e) => setP1(e.target.value)}
              placeholder="Enter name"
              className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
            />
          </div>

          {/* Color Switch (tengah) */}
          <div className="flex flex-col items-center space-y-2 pt-2 pb-4">
            <label className="text-sm font-medium text-gray-300">
              {player1IsBlack ? "Player 1 plays Black" : "Player 1 plays White"}
            </label>

            <div
              onClick={toggleColor}
              className={`
                relative inline-flex items-center cursor-pointer w-20 h-10 rounded-full transition-all duration-300
                ${player1IsBlack ? "bg-gray-300 border-2 border-gray-400" : "bg-gray-800 border-2 border-gray-600"}
              `}
            >
              {/* Knob */}
              <span
                className={`
                  absolute left-1 w-8 h-8 rounded-full shadow-lg transform transition-all duration-300 flex items-center justify-center text-sm font-bold
                  ${
                    player1IsBlack
                      ? "translate-x-9 bg-black text-white border border-gray-700"
                      : "bg-white text-black border border-gray-300"
                  }
                `}
              ></span>

              {/* Labels inside switch */}
              <span className="absolute inset-0 flex items-center justify-between px-4 text-xs font-semibold">
                <span
                  className={
                    player1IsBlack ? "text-white opacity-80" : "text-gray-600"
                  }
                ></span>
                <span
                  className={
                    !player1IsBlack ? "text-white opacity-80" : "text-gray-600"
                  }
                ></span>
              </span>
            </div>
          </div>

          {/* Player 2 */}
          <div className="space-y-2">
            <label className="block text-sm font-medium text-gray-300">
              Player 2
            </label>
            <input
              type="text"
              value={p2}
              onChange={(e) => setP2(e.target.value)}
              placeholder="Enter name"
              className="w-full px-4 py-3 bg-gray-700 border border-gray-600 rounded-lg text-white placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-transparent transition"
            />
          </div>

          {/* Tampilkan warna Player 2 secara informatif */}
          <div className="text-center text-sm text-gray-400">
            Player 2 will play{" "}
            <span className="font-semibold text-white">
              {player1IsBlack ? "White" : "Black"}
            </span>
          </div>

          {/* Button & Error */}
          <div className="pt-6">
            <Button onClick={start} disabled={loading}>
              {loading ? "Starting..." : "Start New Game"}
            </Button>

            {error && (
              <div className="mt-4 text-center text-red-400 text-sm font-medium bg-red-900/30 py-2 px-4 rounded-lg border border-red-800/50">
                {error}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
