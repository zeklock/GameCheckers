export const PieceType = {
  Man: 0,
  King: 1,
} as const;
export type PieceType = (typeof PieceType)[keyof typeof PieceType];

export const Color = {
  Black: 0,
  White: 1,
} as const;
export type Color = (typeof Color)[keyof typeof Color];

export const Status = {
  Move: 0,
  Capture: 1,
  Promote: 2,
  GameOver: 3,
  Invalid: 99,
} as const;
export type Status = (typeof Status)[keyof typeof Status];

export interface PlayerDto {
  name: string;
  color: Color;
}

export interface PositionDto {
  x: number;
  y: number;
}

export interface PieceDto {
  color: Color;
  type: PieceType;
}

export interface MoveDto {
  piece: PieceDto;
  position: PositionDto;
  path: PositionDto[];
}

export interface GameDto {
  board: {
    size: number;
    cells: {
      position: PositionDto;
      piece?: PieceDto | null;
    }[];
  };
  players: PlayerDto[];
  currentPlayer: PlayerDto;
  winner?: PlayerDto;
  availablePieces: {
    position: PositionDto;
    piece: PieceDto;
  }[];
  notifications: string[];
  status: Status[];
}
