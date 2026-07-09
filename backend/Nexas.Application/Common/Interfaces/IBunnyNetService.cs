namespace Nexas.Application.Common.Interfaces;

public interface IBunnyNetService
{
    /// <summary>
    /// Generates a signed URL for a Bunny.net stream video using Token Authentication.
    /// </summary>
    /// <param name="libraryId">The Bunny.net Library ID.</param>
    /// <param name="videoId">The Bunny.net Video ID.</param>
    /// <param name="expirationMinutes">Expiration time in minutes (default 180).</param>
    /// <returns>A signed URL string, or null if parameters are invalid.</returns>
    string? GenerateSignedVideoUrl(string? libraryId, string? videoId, int expirationMinutes = 180);
}
