namespace Nexas.Application.Common.Models.Bunny
{
    public class BunnyWebhookPayload
    {
        public int VideoLibraryId { get; set; }
        public string VideoGuid { get; set; } = string.Empty;
        public int Status { get; set; }
    }
}
