export enum PieceType {
  Man,
  King,
}

export enum Color {
  Black,
  White,
}

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
}
