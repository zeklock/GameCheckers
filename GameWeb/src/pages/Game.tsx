import { useNavigate } from "react-router-dom";
import { useEffect, useState } from "react";
import {
  Color,
  type GameDto,
  type MoveDto,
  type PositionDto,
} from "../types/game";
import { gameApi } from "../api/gameApi";
import Board from "../components/Board";

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

  useEffect(() => {
    const saved = localStorage.getItem("currentGame");
    if (!saved) {
      navigate("/");
      return;
    }

    try {
      const parsed = JSON.parse(saved);
      // Guard lebih ketat: pastikan board & size ada
      if (!parsed || !parsed.board || typeof parsed.board.size !== "number") {
        throw new Error("Invalid or corrupted game state");
      }
      setGame(parsed as GameDto);
      setSelectablePieces(
        (parsed.availablePieces ?? []).map((p: any) => keyFor(p.position)),
      );
    } catch (e) {
      console.error("Failed to load game:", e);
      localStorage.removeItem("currentGame");
      navigate("/");
    } finally {
      setLoading(false);
    }
  }, [navigate]);

  useEffect(() => {
    if (game?.winner) {
      navigate("/game-over");
    }
  }, [game, navigate]);

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
        const newGame = await gameApi.move(move);
        setGame(newGame);
        localStorage.setItem("currentGame", JSON.stringify(newGame));
        setSelected(null);
        setPaths({});
        setSelectablePieces(
          (newGame.availablePieces ?? []).map((p) => keyFor(p.position)),
        );
      } catch (err: any) {
        setError(err.message || "Invalid move");
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
      return;
    }

    if (
      !game.availablePieces?.some(
        (p) => p.position.x === pos.x && p.position.y === pos.y,
      )
    ) {
      setError("This piece cannot move");
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
    }
  };

  if (loading) {
    return (
      <div style={{ textAlign: "center", padding: "40px" }}>
        Loading game...
      </div>
    );
  }

  // Guard utama
  if (!game) {
    return (
      <div style={{ textAlign: "center", padding: "40px", color: "salmon" }}>
        No game data
      </div>
    );
  }

  const currentBoard = game.board;

  if (!currentBoard || typeof currentBoard.size !== "number") {
    console.error("Invalid board in game state:", game);
    return (
      <div style={{ textAlign: "center", padding: "40px", color: "salmon" }}>
        Board data invalid
        <br />
        <button
          onClick={() => {
            localStorage.removeItem("currentGame");
            navigate("/");
          }}
        >
          Back to Home
        </button>
      </div>
    );
  }

  return (
    <div style={{ textAlign: "center", padding: "20px" }}>
      <h2>Checkers Game</h2>
      <div style={{ marginBottom: "16px", fontSize: "1.2rem" }}>
        <strong>Current turn:</strong> {game.currentPlayer.name} (
        {game.currentPlayer.color === Color.Black ? "Black" : "White"})
      </div>

      <Board
        board={currentBoard} // pakai variabel lokal
        onCellClick={onCellClick}
        selectableTargets={selected ? Object.keys(paths) : selectablePieces}
        selected={selected}
      />

      {error && (
        <div style={{ color: "salmon", marginTop: "16px" }}>{error}</div>
      )}

      {game.notifications?.length > 0 && (
        <div
          style={{
            marginTop: "24px",
            textAlign: "left",
            maxWidth: "500px",
            marginLeft: "auto",
            marginRight: "auto",
          }}
        >
          <h4>Notifications</h4>
          <ul style={{ paddingLeft: "20px" }}>
            {game.notifications.map((n, i) => (
              <li key={i}>{n}</li>
            ))}
          </ul>
        </div>
      )}

      <div style={{ marginTop: "24px" }}>
        <button
          onClick={() => {
            localStorage.removeItem("currentGame");
            navigate("/");
          }}
          style={{ padding: "10px 20px", fontSize: "1rem" }}
        >
          Back to Home
        </button>
      </div>
    </div>
  );
}
