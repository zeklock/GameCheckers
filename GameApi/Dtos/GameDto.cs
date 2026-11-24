using GameBase.Enums;

namespace GameApi.Dtos;

public class GameDto
{
    public BoardDto? Board { get; set; }
    public List<PlayerDto>? Players { get; set; }
    public PlayerDto? CurrentPlayer { get; set; }
    public PlayerDto? Winner { get; set; }
    public List<AvailablePieceDto>? AvailablePieces { get; set; } = [];
    public List<string> Notifications { get; set; } = [];
    public List<Status> Status { get; set; } = [];
}
