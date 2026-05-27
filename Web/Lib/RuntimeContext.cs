using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Memory;

namespace WebGame
{
    public static class RuntimeContext
    {
        public static IHttpContextAccessor HttpContextAccessor { get; set; }
        public static IMemoryCache MemoryCache { get; set; }
        public static IHubContext<GameHub> HubContext { get; set; }

        public static HttpContext HttpContext => HttpContextAccessor?.HttpContext;
    }
}
