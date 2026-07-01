using System.Text.Json.Serialization;

namespace Nexas.Application.Common.Interfaces;

public class PixQrCodeResponseDto
{
    [JsonPropertyName("encodedImage")]
    public string EncodedImage { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public string Payload { get; set; } = string.Empty;
}
