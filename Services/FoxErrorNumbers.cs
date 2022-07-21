namespace Roses.SolarAPI.Services
{
    public enum FoxErrorNumber : int
    {
        OK = 0,
        InvalidRequest = 40257,
        Timeout = 41203,
        TokenInvalid = 41808,
        OutOfBounds = 99997,
        NoResponse = 99998,
        Unknown = 99999
    }
}
