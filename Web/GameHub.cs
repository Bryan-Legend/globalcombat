using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using System.Threading.Tasks;
using SignalR;

namespace WebGame
{
    public class GameHub : Hub, IConnected, IDisconnect
    {
        public Task Connect()
        {
            var sessionCookie = Context.RequestCookies["ASP.Net_SessionId"];
            if (sessionCookie != null)
                Groups.Add(Context.ConnectionId, sessionCookie.Value);

            Uri referrer;
            if (Uri.TryCreate(Context.Headers["Referer"], UriKind.RelativeOrAbsolute, out referrer))
            {
                if (referrer.Segments.Length > 1 && referrer.Segments[1].StartsWith("Game-"))
                    Groups.Add(Context.ConnectionId, referrer.Segments[1].TrimEnd('/'));

                if (referrer.Segments.Length > 1 && referrer.Segments[1].StartsWith("ProtoGame-"))
                    Groups.Add(Context.ConnectionId, referrer.Segments[1].TrimEnd('/'));
            }

            return null;
        }

        public Task Reconnect(IEnumerable<string> groups)
        {
            return null;
        }

        public Task Disconnect()
        {
            return null;
        }

        public Task SetGroup(int gameId)
        {
            Caller.GameId = gameId;
            Caller.GroupName = "Game-" + gameId;
            return Groups.Add(Context.ConnectionId, Caller.GroupName);
        }

        public void TestThrow()
        {
            throw new NotImplementedException();
        }

        public static void Say(string group, string message)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients[group].addMessage(message);
        }

        public static void Refresh(string group)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients[group].reload();
        }

        public static void SetDone(string group, int playerNumber)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients[group].setDone(playerNumber);
        }

        public static void SendMessage(string sessionKey, int sourceId, string sourceName, string text)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients[sessionKey].recieveMessage(sourceId, sourceName, text);
        }

        public static void SendNotification(string sessionKey, string title, string text, string targetUri = "")
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<GameHub>();
            context.Clients[sessionKey].sendNotification(title, text, targetUri);
        }
    }
}