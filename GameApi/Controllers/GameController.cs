using GameApi.Dtos;
using GameApi.Interfaces;
using GameApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/game")]
public class GameController(IGameService service) : ControllerBase
{
    [HttpPost("start")]
    public IActionResult Start([FromBody] List<PlayerDto> playerDtos)
    {
        Result<GameDto> result = service.Start(playerDtos);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("moves")]
    public IActionResult GetAvailableMove([FromBody] PositionDto positionDto)
    {
        Result<List<List<PositionDto>>> result = service.GetAvailableMoves(positionDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpPost("move")]
    public IActionResult Move([FromBody] MoveDto moveDto)
    {
        Result<GameDto> result = service.MovePiece(moveDto);
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }

    [HttpGet("state")]
    public IActionResult GetState()
    {
        Result<GameDto> result = service.GetState();
        return result.IsSuccess ? Ok(result) : BadRequest(result);
    }
}
