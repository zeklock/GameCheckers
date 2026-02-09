using GameBase.Enums;

namespace GameApi.Dtos;

public class GameDto
{
    public BoardDto? Board { get; set; }
    public List<PlayerDto>? Players { get; set; }
    public PlayerDto? CurrentPlayer { get; set; }
    public PlayerDto? Winner { get; set; }
    public List<AvailablePieceDto>? AvailablePieces { get; set; } = new List<AvailablePieceDto>();
    public List<string> Notifications { get; set; } = new List<string>();
    public List<Status> Status { get; set; } = new List<Status>();
}
