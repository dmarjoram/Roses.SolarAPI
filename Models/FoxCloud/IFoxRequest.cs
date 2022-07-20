namespace Roses.SolarAPI.Models.FoxCloud
{
    public interface IFoxRequest
    {
        public void Validate();

        public string RequestUri { get; }
    }
}
