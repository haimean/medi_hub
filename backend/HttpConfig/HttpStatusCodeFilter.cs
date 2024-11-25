using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace DashboardApi.HttpConfig
{
   public class HttpStatusCodeFilter : IActionFilter
   {
      private readonly ILogger<HttpStatusCodeFilter> _logger;

      public HttpStatusCodeFilter(ILogger<HttpStatusCodeFilter> logger) => _logger = logger;

      public void OnActionExecuted(ActionExecutedContext context)
      {
         if (context.Exception != null)
         {
            _logger.LogError(context.Exception, "Unhandled service's exception.");
            var mess = context.Exception?.Message;
            context.Exception = null;
            context.ExceptionDispatchInfo = null;
            context.ExceptionHandled = true;
            context.HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Result = new ObjectResult(ServiceResponse.Fail(StatusCodes.Status500InternalServerError, "G000", "Unhandled service's exception. See server logs for more details. " + mess));

            return;
         }

         var result = context.Result as ObjectResult;
         if (result?.Value is ServiceResponse serviceResponse)
         {
            context.HttpContext.Response.StatusCode = serviceResponse.StatusCode;
         }
         else
         {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            _logger.LogWarning($"The action {descriptor?.ControllerName}/{descriptor?.ActionName} does not return {nameof(ServiceResponse)} type.");
         }
      }

      public void OnActionExecuting(ActionExecutingContext context)
      {
         //
      }
   }
}
