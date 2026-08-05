using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace MealPlanner.UI.Mobile.Services
{
    public class TimingHandler(ILogger<TimingHandler> logger) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var stopwatch = Stopwatch.StartNew();
            try
            {
                return await base.SendAsync(request, cancellationToken);
            }
            finally
            {
                stopwatch.Stop();
                logger.LogWarning("HTTP {Method} {Uri} took {ElapsedMs} ms", request.Method, request.RequestUri, stopwatch.ElapsedMilliseconds);
            }
        }
    }
}
