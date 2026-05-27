using System;
using LT;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebGame;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("Secrets.json", optional: true, reloadOnChange: true);

builder.Services.AddControllersWithViews()
    .AddRazorRuntimeCompilation();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddMemoryCache();
builder.Services.AddSignalR();

builder.Services.AddAuthentication("Cookies").AddCookie(options =>
{
    options.LoginPath = "/Account/LogOn";
});
builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Wire up statics used by LT helpers and GameServer
AppConfig.Configuration = app.Configuration;
WebGame.RuntimeContext.HttpContextAccessor = app.Services.GetRequiredService<IHttpContextAccessor>();
WebGame.RuntimeContext.MemoryCache = app.Services.GetRequiredService<Microsoft.Extensions.Caching.Memory.IMemoryCache>();
WebGame.RuntimeContext.HubContext = app.Services.GetRequiredService<IHubContext<GameHub>>();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

// Custom slug routes that mimic legacy URLs like /Game-123/, /Player-Info-456, /Tournament-7
app.MapControllerRoute(
    name: "game-slug",
    pattern: "Game-{id:int}/{action=Index}",
    defaults: new { controller = "Game" });

app.MapControllerRoute(
    name: "player-info-slug",
    pattern: "Player-Info-{id:int}",
    defaults: new { controller = "Home", action = "PlayerInfo" });

app.MapControllerRoute(
    name: "tournament-slug",
    pattern: "Tournament-{id:int}",
    defaults: new { controller = "Tourney", action = "Index" });

app.MapControllerRoute(
    name: "create-game",
    pattern: "Create-Game",
    defaults: new { controller = "Game", action = "Create" });

app.MapControllerRoute(
    name: "create-tournament",
    pattern: "Create-Tournament",
    defaults: new { controller = "Tourney", action = "Create" });

app.MapControllerRoute(
    name: "game-manual",
    pattern: "Game-Manual",
    defaults: new { controller = "Home", action = "GameManual" });

app.MapControllerRoute(
    name: "send-message",
    pattern: "Send-Message",
    defaults: new { controller = "Home", action = "SendMessage" });

app.MapControllerRoute(
    name: "home-shortcuts",
    pattern: "{action}",
    defaults: new { controller = "Home" },
    constraints: new { action = "Messages|Stats|IpAddresses|GameManual|OptOut|PlayerInfo|Chat|LoadChatMessages|CloseChatWindow|SendMessage" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/signalr");

app.Run();
