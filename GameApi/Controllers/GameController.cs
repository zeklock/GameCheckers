using GameApi.Dtos;
using GameApi.Interfaces;
using GameApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameApi.Controllers;

[ApiController]
[Route("api/game")]
public class GameController : ControllerBase
{
    private readonly IGameService _service;

    public GameController(IGameService service)
    {
        _service = service;
    }

    [HttpPost]
    [Route("start")]
    public IActionResult Start([FromBody] List<PlayerDto> playerDtos)
    {
        Result<GameDto> result = _service.Start(playerDtos);

        if (!result.IsSuccess) return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    [Route("moves")]
    public IActionResult GetAvailableMove([FromBody] PositionDto positionDto)
    {
        Result<List<List<PositionDto>>> result = _service.GetAvailableMoves(positionDto);

        if (!result.IsSuccess) return BadRequest(result);

        return Ok(result);
    }

    [HttpPost]
    [Route("move")]
    public IActionResult Move([FromBody] MoveDto moveDto)
    {
        Result<GameDto> result = _service.MovePiece(moveDto);

        if (!result.IsSuccess) return BadRequest(result);

        return Ok(result);
    }
}
