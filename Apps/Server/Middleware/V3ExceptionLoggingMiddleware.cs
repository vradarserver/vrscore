using Microsoft.AspNetCore.Http;

namespace VirtualRadar.Server.Middleware
{
    /// <summary>
    /// Logs exceptions that are thrown during the processing of the V3 request pipeline.
    /// </summary>
    public class V3ExceptionLoggingMiddleware(
        #pragma warning disable IDE1006 // .editorconfig does not support naming rules for primary ctors
        RequestDelegate _NextMiddleware,
        ILog _Log
        #pragma warning restore IDE1006
    )
    {
        public async Task InvokeAsync(HttpContext context)
        {
            try {
                await _NextMiddleware(context);
            } catch(Exception ex) {
                _Log.Exception(ex);
            }
        }
    }
}
