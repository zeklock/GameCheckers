using GameApi.Dtos;
using GameBase;

namespace GameApi.Data;

public class GameStore
{
    public GameController? Game { get; set; }
    public GameDto GameDto { get; } = new();
}
