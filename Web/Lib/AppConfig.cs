using Microsoft.Extensions.Configuration;

namespace LT
{
    public static class AppConfig
    {
        public static IConfiguration Configuration { get; set; }

        public static string Get(string key) => Configuration?[key];
    }
}
