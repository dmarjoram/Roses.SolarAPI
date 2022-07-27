using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Roses.SolarAPI.Models;

namespace Roses.SolarAPI.Exceptions
{
    public class HttpResponseExceptionFilter : IActionFilter, IOrderedFilter
    {
        public int Order => int.MaxValue - 10;

        public void OnActionExecuting(ActionExecutingContext context) { }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (context.Exception != null)
            {
                if (context.Exception is FoxResponseException foxException)
                {
                    context.Result = new ObjectResult(new ApiResult() { ResultCode = foxException.Message })
                    {
                        StatusCode = foxException.StatusCode
                    };
                }
                else
                {
                    context.Result = new ObjectResult(new ApiResult() { ResultCode = context.Exception.Message })
                    {
                        StatusCode = 500
                    };
                }

                context.ExceptionHandled = true;
            }
        }
    }
}
