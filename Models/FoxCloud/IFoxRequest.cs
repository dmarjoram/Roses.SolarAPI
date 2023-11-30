namespace Roses.SolarAPI.Models.FoxCloud
{
    public interface IFoxRequest
    {
        void Validate();

        string RequestUri { get; }

        bool GetRequest { get; }
    }
}
