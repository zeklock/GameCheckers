import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import {
  Color,
  Status,
  type GameDto,
  type MoveDto,
  type PositionDto,
} from "../types/game";
import { gameApi } from "../api/gameApi";
import Board from "../components/Board";
import Button from "../components/Button";

const invalidSound = new Audio("/sounds/invalid.mp3");
const sounds = [
  new Audio("/sounds/move.mp3"),
  new Audio("/sounds/capture.mp3"),
  new Audio("/sounds/promote.mp3"),
  new Audio("/sounds/gameover.mp3"),
];

function keyFor(pos: PositionDto) {
  return `${pos.x}_${pos.y}`;
}

export default function Game() {
  const navigate = useNavigate();
  const [game, setGame] = useState<GameDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<PositionDto | null>(null);
  const [paths, setPaths] = useState<Record<string, PositionDto[]>>({});
  const [selectablePieces, setSelectablePieces] = useState<string[]>([]);
  const [error, setError] = useState<string | null>(null);
  const [showNotif, setShowNotif] = useState(false);
  const [showError, setShowError] = useState(false);

  const playInvalidSound = () => {
    invalidSound.currentTime = 0;
    invalidSound.play();
  };

  const playSound = (status: Status[]) => {
    status.forEach((s) => {
      if (sounds[s]) {
        sounds[s].currentTime = 0;
        sounds[s].play();
      }
    });
  };

  const getState = async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await gameApi.state();
      const game = response as GameDto;

      if (!game.board) {
        navigate("/");
        return;
      }

      setGame(game);
      setSelectablePieces(
        (game.availablePieces ?? []).map((p: any) => keyFor(p.position)),
      );
    } catch (e) {
      console.error("Failed to load game:", e);
      navigate("/");
    } finally {
      setLoading(false);
      setShowNotif(true);
    }
  };

  useEffect(() => {
    getState();
  }, []);

  useEffect(() => {
    if (game?.winner) {
      navigate("/game-over");
    }
  }, [game, navigate]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowNotif(false);
    }, 3000);

    return () => clearTimeout(timer);
  }, [showNotif]);

  useEffect(() => {
    const timer = setTimeout(() => {
      setShowError(false);
    }, 3000);

    return () => clearTimeout(timer);
  }, [showError]);

  const onCellClick = async (pos: PositionDto) => {
    if (!game || !game.board?.cells) return;
    setError(null);
    const key = keyFor(pos);

    if (selected && paths[key]) {
      const fromCell = game.board.cells.find(
        (c) => c.position.x === selected.x && c.position.y === selected.y,
      );
      if (!fromCell?.piece) return;

      const move: MoveDto = {
        piece: fromCell.piece,
        position: selected,
        path: paths[key],
      };

      try {
        const newGameState = await gameApi.move(move);
        setGame(newGameState);
        setSelected(null);
        setPaths({});
        setSelectablePieces(
          (newGameState.availablePieces ?? []).map((p) => keyFor(p.position)),
        );
        setShowNotif(true);
        playSound(newGameState.status ?? []);
      } catch (err: any) {
        setError(err.message || "Invalid move");
        setShowError(true);
        playInvalidSound();
      }
      return;
    }

    const cell = game.board.cells.find(
      (c) => c.position.x === pos.x && c.position.y === pos.y,
    );
    if (!cell) return;

    if (!cell.piece) {
      setSelected(null);
      setPaths({});
      return;
    }

    if (cell.piece.color !== game.currentPlayer.color) {
      setError("Not your piece");
      setShowError(true);
      playInvalidSound();
      return;
    }

    if (
      !game.availablePieces?.some(
        (p) => p.position.x === pos.x && p.position.y === pos.y,
      )
    ) {
      setError("This piece cannot move");
      setShowError(true);
      playInvalidSound();
      return;
    }

    try {
      const movePaths = await gameApi.moves(pos);
      const map: Record<string, PositionDto[]> = {};
      movePaths.forEach((path) => {
        if (path.length > 0) {
          const target = path[path.length - 1];
          map[keyFor(target)] = path;
        }
      });
      setSelected(pos);
      setPaths(map);
    } catch (err: any) {
      setError(err.message || "No available moves");
      setShowError(true);
      playInvalidSound();
    }
  };

  const onBackHome = () => {
    navigate("/");
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center">
        <div className="text-white text-xl font-medium animate-pulse">
          Loading game...
        </div>
      </div>
    );
  }

  if (!game) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center px-4">
        <div className="text-center bg-gray-800/70 backdrop-blur-sm rounded-2xl shadow-2xl border border-gray-700/50 p-10 max-w-md">
          <h2 className="text-2xl font-bold text-red-400 mb-4">No game data</h2>
          <Button onClick={onBackHome}>Back to Home</Button>
        </div>
      </div>
    );
  }

  const currentBoard = game.board;

  if (!currentBoard || typeof currentBoard.size !== "number") {
    console.error("Invalid board in game state:", game);
    return (
      <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex items-center justify-center px-4">
        <div className="text-center bg-gray-800/70 backdrop-blur-sm rounded-2xl shadow-2xl border border-gray-700/50 p-10 max-w-md">
          <h2 className="text-2xl font-bold text-red-400 mb-4">
            Board data invalid
          </h2>
          <Button onClick={onBackHome}>Back to Home</Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-gray-900 via-gray-800 to-gray-950 flex flex-col items-center py-8 px-4">
      <div className="w-full max-w-5xl">
        {/* Header & Status */}
        <div className="text-center mb-8">
          <h1 className="text-4xl md:text-5xl font-bold text-white tracking-tight mb-3">
            Checkers Game
          </h1>
          <div className="inline-block bg-gray-800/80 backdrop-blur-sm px-6 py-3 rounded-full border border-gray-700/50">
            <span className="text-gray-300 mr-2">Current turn:</span>
            <span className="font-semibold text-white">
              {game.currentPlayer.name}
            </span>{" "}
            <span className="text-gray-400">
              ({game.currentPlayer.color === Color.Black ? "Black" : "White"})
            </span>
          </div>
        </div>

        {/* Notifications */}
        {game.notifications?.length > 0 && (
          <div
            className={`fixed top-8 right-8 z-10 max-w-2xl mx-auto rounded-xl border border-gray-700/50 p-6
            transition-all duration-300 ease-in-out
            ${
              showNotif
                ? "opacity-100 translate-y-0 bg-[rgb(245,222,179)] backdrop-blur-sm"
                : "opacity-0 -translate-y-2 pointer-events-none"
            }`}
          >
            <h3 className="text-lg font-semibold text-black mb-3">
              Notifications
            </h3>
            <ul className="space-y-2 text-gray-800 text-sm">
              {game.notifications.map((n, i) => (
                <li key={i} className="flex items-start">
                  <span className="text-indigo-400 mr-2">•</span>
                  {n}
                </li>
              ))}
            </ul>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div
            className={`fixed bottom-8 right-8 z-10 max-w-md mx-auto text-center text-red-400 text-sm font-medium py-3 px-6 rounded-xl border border-red-800/50
            transition-all duration-300 ease-in-out
            ${
              showError
                ? "opacity-100 translate-y-0 bg-red-900/30"
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
              board={currentBoard}
              onCellClick={onCellClick}
              selectableTargets={
                selected ? Object.keys(paths) : selectablePieces
              }
              selected={selected}
            />
          </div>
        </div>

        {/* Back Button */}
        <div className="text-center">
          <Button onClick={onBackHome}>Back to Home</Button>
        </div>
      </div>
    </div>
  );
}
