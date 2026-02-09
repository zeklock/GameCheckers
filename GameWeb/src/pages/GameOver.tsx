import { useEffect, useState } from "react";
import { Color, PieceType, type GameDto } from "../types/game";
import { useNavigate } from "react-router-dom";
import { gameApi } from "../api/gameApi";
import Button from "../components/Button";
import Cell from "../components/Cell";

export default function GameOver() {
  const [game, setGame] = useState<GameDto | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const load = async () => {
      try {
        const game = (await gameApi.state()) as GameDto;

        if (!game.board) {
          navigate("/");
          return;
        }

        if (game.availablePieces.length > 0) {
          navigate("/game");
          return;
        }

        setGame(game);
      } catch {
        navigate("/");
      }
    };

    load();
  }, [navigate]);

  const onGameOver = () => {
    navigate("/");
    return;
  };

  const winnerName = game?.winner?.name;
  const winnerColor = game?.winner
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
              <div className="flex items-center justify-center mt-4">
                <Cell
                  cell={{
                    position: { x: 0, y: 0 },
                    piece: {
                      color:
                        winnerColor === "Black" ? Color.Black : Color.White,
                      type: PieceType.King,
                    },
                  }}
                />
              </div>
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
        {game && game.notifications?.length > 0 && (
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
          <Button onClick={onGameOver}>Back to Home</Button>
        </div>
      </div>
    </div>
  );
}
