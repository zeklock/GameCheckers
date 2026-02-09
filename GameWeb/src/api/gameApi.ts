import {
  Color,
  PieceType,
  type GameDto,
  type MoveDto,
  type PlayerDto,
  type PositionDto,
  type PieceDto,
} from "../types/game";

const BASE_URL = "http://localhost:5271/api/game";

function mapToGameDto(raw: any): GameDto {
  if (!raw || !raw.board || typeof raw.board.size !== "number") {
    console.error("Invalid game data:", raw);
    throw new Error("Invalid game response structure");
  }

  const toPiece = (p: any): PieceDto | undefined =>
    p
      ? {
          color: p.color === 0 ? Color.Black : Color.White,
          type: p.type === 0 ? PieceType.Man : PieceType.King,
        }
      : undefined;

  const toPlayer = (p: any): PlayerDto => ({
    name: p?.name ?? "Unknown",
    color: p?.color === 0 ? Color.Black : Color.White,
  });

  return {
    board: {
      size: raw.board.size,
      cells: raw.board.cells.map((c: any) => ({
        position: {
          x: Number(c?.position?.x ?? 0),
          y: Number(c?.position?.y ?? 0),
        },
        piece: toPiece(c?.piece),
      })),
    },
    players: Array.isArray(raw.players) ? raw.players.map(toPlayer) : [],
    currentPlayer: toPlayer(raw.currentPlayer),
    winner: raw.winner ? toPlayer(raw.winner) : undefined,
    availablePieces: Array.isArray(raw.availablePieces)
      ? raw.availablePieces.map((ap: any) => ({
          position: {
            x: Number(ap?.position?.x ?? 0),
            y: Number(ap?.position?.y ?? 0),
          },
          piece: toPiece(ap?.piece)!,
        }))
      : [],
    notifications: Array.isArray(raw.notifications) ? raw.notifications : [],
    status: Array.isArray(raw.status) ? raw.status : [],
  };
}

async function fetchApi<TRequest, TResponse>(
  method: string,
  endpoint: string,
  body: TRequest,
  isGameEndpoint: boolean = true,
): Promise<TResponse> {
  const options: RequestInit = {
    method,
    headers: { "Content-Type": "application/json" },
  };
  const hasBody = ["post", "put", "patch"].includes(method.toLowerCase());
  if (hasBody) {
    options.body = JSON.stringify(body);
  }
  const res = await fetch(`${BASE_URL}${endpoint}`, options);

  if (!res.ok) {
    const err = await res.json().catch(() => ({ error: `HTTP ${res.status}` }));
    throw new Error(err.error || "Request failed");
  }

  const json = await res.json();

  if (!json.isSuccess) {
    throw new Error(json.error || "API failed");
  }

  if (!json.data) {
    throw new Error("Missing data in response");
  }

  if (isGameEndpoint) {
    return mapToGameDto(json.data) as TResponse;
  } else {
    // Untuk /moves, return langsung array tanpa mapping
    return json.data as TResponse;
  }
}

export const gameApi = {
  start: (players: PlayerDto[]) =>
    fetchApi<PlayerDto[], GameDto>("POST", "/start", players, true),
  moves: (position: PositionDto) =>
    fetchApi<PositionDto, PositionDto[][]>("POST", "/moves", position, false),
  move: (move: MoveDto) =>
    fetchApi<MoveDto, GameDto>("POST", "/move", move, true),
  state: () => fetchApi("GET", "/state", null, true),
};
