namespace Nexas.Application.Common.Models.Bunny
{
    public class VideoResponseDto
    {
        public string Guid { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int Status { get; set; }
        public int Length { get; set; }
    }
}
