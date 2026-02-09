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

  if (!game) {
    return null;
  }

  if (game.availablePieces.length > 0) {
    navigate("/game");
    return;
  }

  const winnerName = game.winner?.name;
  const winnerColor = game.winner
    ? game.winner.color === Color.Black
      ? "Black"
      : "White"
    : null;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center px-4 py-12">
      <div className="w-full max-w-md bg-gray-800/70 backdrop-blur-sm rounded-2xl shadow-2xl border border-gray-700/50 p-8 space-y-8">
        {/* Header */}
        <div className="text-center">
          <h1 className="text-4xl md:text-5xl font-bold text-white tracking-tight mb-2">
            Game Over
          </h1>
          <p className="text-gray-400">The match has ended</p>
        </div>

        {/* Winner Announcement */}
        <div className="text-center">
          {winnerName && winnerColor ? (
            <div className="space-y-3">
              <div className="inline-block bg-gray-900/80 px-8 py-5 rounded-2xl border border-gray-700/60 shadow-lg">
                <p className="text-2xl font-bold text-white mb-1">
                  {winnerName}
                </p>
                <p className="text-lg text-gray-300">
                  wins! <span className="font-semibold">({winnerColor})</span>
                </p>
              </div>

              {/* Trophy-like accent */}
              <div className="text-6xl md:text-7xl opacity-80">🏆</div>
            </div>
          ) : (
            <div className="bg-gray-900/60 px-8 py-6 rounded-2xl border border-gray-700/50">
              <p className="text-2xl font-semibold text-gray-300">
                Draw / No Winner
              </p>
              <p className="mt-2 text-gray-400">The game ended in a tie</p>
            </div>
          )}
        </div>

        {/* Last Notifications */}
        {game.notifications?.length > 0 && (
          <div className="bg-gray-800/60 backdrop-blur-sm rounded-xl border border-gray-700/50 p-6">
            <h3 className="text-lg font-semibold text-white mb-4">
              Last Notifications
            </h3>
            <ul className="space-y-2 text-gray-300 text-sm">
              {game.notifications.map((n, i) => (
                <li key={i} className="flex items-start">
                  <span className="text-indigo-400 mr-3 mt-1">•</span>
                  <span>{n}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Back Button */}
        <div className="pt-4">
          <button
            onClick={() => {
              localStorage.removeItem("currentGame");
              navigate("/");
            }}
            className="w-full py-4 px-6 bg-indigo-600 hover:bg-indigo-700 active:bg-indigo-800 rounded-xl text-white font-semibold text-lg transition-all duration-200 shadow-lg shadow-indigo-500/30 hover:shadow-indigo-500/50 focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:ring-offset-2 focus:ring-offset-gray-900"
          >
            Back to Home
          </button>
        </div>
      </div>
    </div>
  );
}
