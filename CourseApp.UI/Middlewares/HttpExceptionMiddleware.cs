using CourseApp.UI.Exceptions;
using System.Net;

namespace CourseApp.UI.Middlewares
{
    public class HttpExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (HttpException e)
            {
                if (e.Status == System.Net.HttpStatusCode.Unauthorized)
                {
                    context.Response.Redirect("/account/login");
                }
                else
                {
                    context.Response.Redirect("/home/error?message=" + e.Message);
                }
            }
            catch (Exception ex)
            {
                context.Response.Redirect("/home/error?message=" + ex.Message);
            }
        }
    }

}
