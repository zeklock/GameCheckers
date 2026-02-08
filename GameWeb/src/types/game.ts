export enum PieceType {
  Man = 0,
  King = 1,
}

export enum Color {
  Black = 0,
  White = 1,
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
  position: PositionDto;   // ← posisi ASAL piece (from)
  path: PositionDto[];     // ← urutan tujuan (dari /moves)
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
