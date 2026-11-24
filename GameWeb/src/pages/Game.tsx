import { useNavigate } from "react-router-dom";
import { Color } from "../types/game";
import { useCheckersGame } from "../hooks/useCheckersGame";
import Board from "../components/Board";
import Button from "../components/Button";

export default function Game() {
  const navigate = useNavigate();
  const {
    game,
    loading,
    selected,
    paths,
    selectablePieces,
    error,
    showNotif,
    showError,
    selectPiece,
  } = useCheckersGame();

  const onBackHome = () => {
    navigate("/");
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center">
        <div className="text-white text-xl font-medium animate-pulse" role="status">
          Loading game...
        </div>
      </div>
    );
  }

  if (!game || !game.board || typeof game.board.size !== "number") {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center px-4">
        <div className="text-center bg-gray-800/70 backdrop-blur-sm rounded-2xl shadow-2xl border border-gray-700/50 p-10 max-w-md">
          <h2 className="text-2xl font-bold text-red-400 mb-4">No game data</h2>
          <Button onClick={onBackHome}>Back to Home</Button>
        </div>
      </div>
    );
  }

  const isCurrentBlack = game.currentPlayer.color === Color.Black;
  const currentTurnText = `${game.currentPlayer.name}'s Turn (${isCurrentBlack ? "Black" : "White"})`;

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex flex-col items-center py-8 px-4">
      <div className="w-full max-w-5xl">
        {/* Header & Status with aria-live */}
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-bold text-white tracking-tight mb-3">
            Checkers Game
          </h1>
          <div
            aria-live="polite"
            className="inline-flex items-center gap-3 bg-gray-800/80 backdrop-blur-sm px-6 py-3 rounded-full border border-gray-700/50 shadow-md"
          >
            <span className="text-gray-300">Current turn:</span>
            <span className="font-semibold text-white">
              {game.currentPlayer.name}
            </span>
            <span
              className={`w-3.5 h-3.5 rounded-full inline-block border ${
                isCurrentBlack ? "bg-black border-stone-600" : "bg-white border-stone-300"
              }`}
              aria-hidden="true"
            />
            <span className="text-gray-400">
              ({isCurrentBlack ? "Black" : "White"})
            </span>
          </div>
        </div>

        {/* Floating Notifications */}
        {game.notifications?.length > 0 && (
          <div
            aria-live="polite"
            className={`fixed top-8 right-8 z-30 max-w-md mx-auto rounded-xl border border-amber-900/40 p-5 shadow-2xl
            transition-all duration-300 ease-in-out
            ${
              showNotif
                ? "opacity-100 translate-y-0 bg-[#F5DEB3] text-stone-900"
                : "opacity-0 -translate-y-2 pointer-events-none"
            }`}
          >
            <h3 className="text-sm font-bold uppercase tracking-wider text-amber-950 mb-2">
              Notifications
            </h3>
            <ul className="space-y-1.5 text-stone-800 text-sm">
              {game.notifications.slice(-3).map((n, i) => (
                <li key={i} className="flex items-start">
                  <span className="text-amber-800 mr-2 font-bold">•</span>
                  <span>{n}</span>
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Floating Error Toast */}
        {error && (
          <div
            role="alert"
            className={`fixed bottom-8 right-8 z-30 max-w-md mx-auto text-center text-red-300 text-sm font-medium py-3 px-6 rounded-xl border border-red-700/50 bg-red-950/80 backdrop-blur-md shadow-2xl
            transition-all duration-300 ease-in-out
            ${
              showError
                ? "opacity-100 translate-y-0"
                : "opacity-0 -translate-y-2 pointer-events-none"
            }`}
          >
            {error}
          </div>
        )}

        {/* Board Container */}
        <div className="bg-gray-800/50 backdrop-blur-sm rounded-2xl shadow-2xl border border-gray-700/60 p-6 md:p-10 mb-8">
          <div className="flex justify-center">
            <Board
              board={game.board}
              onCellClick={selectPiece}
              selectableTargets={
                selected ? Object.keys(paths) : selectablePieces
              }
              selected={selected}
            />
          </div>
        </div>

        {/* Navigation & Actions */}
        <div className="text-center max-w-xs mx-auto">
          <Button onClick={onBackHome}>Back to Home</Button>
        </div>
      </div>
    </div>
  );
}
