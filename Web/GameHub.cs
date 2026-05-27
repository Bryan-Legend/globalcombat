using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace WebGame
{
    public class GameHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var sessionId = Context.GetHttpContext()?.Session?.Id;
            if (!string.IsNullOrEmpty(sessionId))
                await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);

            var referer = Context.GetHttpContext()?.Request.Headers["Referer"].ToString();
            if (System.Uri.TryCreate(referer, System.UriKind.Absolute, out var referrer))
            {
                if (referrer.Segments.Length > 1 && referrer.Segments[1].StartsWith("Game-"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, referrer.Segments[1].TrimEnd('/'));

                if (referrer.Segments.Length > 1 && referrer.Segments[1].StartsWith("ProtoGame-"))
                    await Groups.AddToGroupAsync(Context.ConnectionId, referrer.Segments[1].TrimEnd('/'));
            }

            await base.OnConnectedAsync();
        }

        public Task SetGroup(int gameId)
        {
            return Groups.AddToGroupAsync(Context.ConnectionId, "Game-" + gameId);
        }

        public static void Say(string group, string message) =>
            RuntimeContext.HubContext?.Clients.Group(group).SendAsync("addMessage", message);

        public static void Refresh(string group) =>
            RuntimeContext.HubContext?.Clients.Group(group).SendAsync("reload");

        public static void SetDone(string group, int playerNumber) =>
            RuntimeContext.HubContext?.Clients.Group(group).SendAsync("setDone", playerNumber);

        public static void SendMessage(string sessionKey, int sourceId, string sourceName, string text) =>
            RuntimeContext.HubContext?.Clients.Group(sessionKey).SendAsync("receiveMessage", sourceId, sourceName, text);

        public static void SendNotification(string sessionKey, string title, string text, string targetUri = "") =>
            RuntimeContext.HubContext?.Clients.Group(sessionKey).SendAsync("sendNotification", title, text, targetUri);
    }
}
