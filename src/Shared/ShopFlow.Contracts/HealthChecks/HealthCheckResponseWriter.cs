using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ShopFlow.Contracts.HealthChecks;

public static class HealthCheckResponseWriter
{
    public static async Task WriteResponse(
        HttpContext context,
        HealthReport report,
        string serviceName)
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status  = report.Status.ToString(),
            service = serviceName,
            checks  = report.Entries.Select(e => new
            {
                name     = e.Key,
                status   = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
}
