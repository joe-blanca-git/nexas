namespace Nexas.Api.Services
{
    public interface ICurrentUserProvider
    {
        string? ExternalId { get; }
        void SetExternalId(string externalId);
    }

    public class CurrentUserProvider : ICurrentUserProvider
    {
        public string? ExternalId { get; private set; }

        public void SetExternalId(string externalId)
        {
            ExternalId = externalId;
        }
    }
}
