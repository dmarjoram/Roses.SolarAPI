using Roses.SolarAPI.Services;

namespace Roses.SolarAPI.Exceptions
{
    public class FoxResponseException : Exception
    {
        public FoxResponseException(FoxErrorNumber foxErrorNumber, string message) : base(message)
        {
            switch (foxErrorNumber)
            {
                case FoxErrorNumber.InvalidRequest:
                case FoxErrorNumber.OutOfBounds:
                    StatusCode = 400;
                    break;
                case FoxErrorNumber.Timeout:
                    StatusCode = 504;
                    break;
                case FoxErrorNumber.TokenInvalid:
                    StatusCode = 401;
                    break;
                case FoxErrorNumber.OK:
                    StatusCode = 200;
                    break;
                case FoxErrorNumber.NoResponse:
                case FoxErrorNumber.Unknown:
                default:
                    StatusCode = 500;
                    break;
            }
        }

        public int StatusCode { get; }
    }
}
