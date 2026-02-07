using GameBase.Interfaces;
using GameBase.Models;

namespace GameBase.Dtos
{
    public record MovablePieceDto()
    {
        public required IPiece Piece { get; init; }
        public Position Position { get; init; }
        public int JumpLen { get; init; } = default;
    }
}
