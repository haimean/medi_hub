using DashboardApi.HttpConfig;
using Newtonsoft.Json;
using System.Text;

namespace HttpConfig
{
    public class HttpStatusCodeFilterMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var responseBodyStream = new MemoryStream();

            var originalResponseBody = context.Response.Body;

            try
            {
                context.Response.Body = responseBodyStream;

                await next(context);

                responseBodyStream.Seek(0, SeekOrigin.Begin);

                var responseBody = await new StreamReader(responseBodyStream).ReadToEndAsync();

                var responseObject = JsonConvert.DeserializeObject<ServiceResponse>(responseBody);

                if (responseObject.StatusCode != 0)
                {
                    context.Response.StatusCode = responseObject.StatusCode;
                }
                else
                {
                    context.Response.StatusCode = 400;
                }

                context.Response.Body = originalResponseBody;
                await context.Response.WriteAsync(responseBody, Encoding.UTF8);
            }
            catch (Exception e)
            {
                var failResponse = ServiceResponse.Fail(500, "", e.Message);

                var serializer = new JsonSerializer();
                var errorResponseBody = new MemoryStream();

                using (
                    var writer = new StreamWriter(
                        errorResponseBody,
                        Encoding.UTF8,
                        bufferSize: 1024,
                        leaveOpen: true
                    )
                )
                {
                    serializer.Serialize(writer, failResponse);
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";

                errorResponseBody.Seek(0, SeekOrigin.Begin);
                await errorResponseBody.CopyToAsync(originalResponseBody);
            }
            finally
            {
                context.Response.Body = originalResponseBody;
            }
        }
    }
}
