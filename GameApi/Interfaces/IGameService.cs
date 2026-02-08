using GameApi.Dtos;
using GameApi.Services;

namespace GameApi.Interfaces;

public interface IGameService
{
    public Result<GameDto> Start(List<PlayerDto> players);
    public Result<List<List<PositionDto>>> GetAvailableMoves(PositionDto positionDto);
    public Result<GameDto> MovePiece(MoveDto move);
}
