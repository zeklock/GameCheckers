import { useState, useEffect, useCallback, useMemo } from "react";
import { useNavigate } from "react-router-dom";
import {
  Color,
  type GameDto,
  type MoveDto,
  type PositionDto,
} from "../types/game";
import { gameApi } from "../api/gameApi";
import { useGameAudio } from "./useGameAudio";

export function keyFor(pos: PositionDto) {
  return `${pos.x}_${pos.y}`;
}

export function useCheckersGame() {
  const navigate = useNavigate();
  const { playSoundByStatus, playInvalidSound } = useGameAudio();

  const [game, setGame] = useState<GameDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [selected, setSelected] = useState<PositionDto | null>(null);
  const [paths, setPaths] = useState<Record<string, PositionDto[]>>({});
  const [error, setError] = useState<string | null>(null);
  const [showNotif, setShowNotif] = useState(false);
  const [showError, setShowError] = useState(false);

  // Derive selectable pieces directly from game state without redundant syncing
  const selectablePieces = useMemo(() => {
    return (game?.availablePieces ?? []).map((p) => keyFor(p.position));
  }, [game?.availablePieces]);

  // Load initial game state
  const loadState = useCallback(async () => {
    setLoading(true);
    setError(null);

    try {
      const response = await gameApi.state();
      const loadedGame = response as GameDto;

      if (!loadedGame.board) {
        navigate("/");
        return;
      }

      setGame(loadedGame);
      setShowNotif(true);
    } catch (e) {
      console.error("Failed to load game:", e);
      navigate("/");
    } finally {
      setLoading(false);
    }
  }, [navigate]);

  useEffect(() => {
    loadState();
  }, [loadState]);

  // Auto redirect on game over
  useEffect(() => {
    if (game?.winner) {
      navigate("/game-over");
    }
  }, [game?.winner, navigate]);

  // Notification auto-dismiss
  useEffect(() => {
    if (!showNotif) return;
    const timer = setTimeout(() => setShowNotif(false), 3000);
    return () => clearTimeout(timer);
  }, [showNotif]);

  // Error auto-dismiss
  useEffect(() => {
    if (!showError) return;
    const timer = setTimeout(() => setShowError(false), 3000);
    return () => clearTimeout(timer);
  }, [showError]);

  const selectPiece = useCallback(
    async (pos: PositionDto) => {
      if (!game || !game.board?.cells) return;
      setError(null);
      const key = keyFor(pos);

      // Deselect if clicked on currently selected
      if (selected?.x === pos.x && selected?.y === pos.y) {
        setSelected(null);
        setPaths({});
        return;
      }

      // If already selected and clicking on a destination path
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
          setShowNotif(true);
          playSoundByStatus(newGameState.status ?? []);
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
      if (!cell || !cell.piece) {
        setSelected(null);
        setPaths({});
        return;
      }

      if (cell.piece.color !== game.currentPlayer.color) {
        setError("Not your piece");
        setSelected(null);
        setPaths({});
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
    },
    [game, selected, paths, playSoundByStatus, playInvalidSound],
  );

  return {
    game,
    loading,
    selected,
    paths,
    selectablePieces,
    error,
    showNotif,
    showError,
    selectPiece,
    loadState,
  };
}
