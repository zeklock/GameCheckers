using GameBase.Enums;

namespace GameApi.Dtos
{
    public class PlayerDto
    {
        public required string Name { get; set; }
        public required Color Color { get; set; }
    }
}
