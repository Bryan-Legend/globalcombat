using System.Collections.Generic;
using System.Text;
using LT;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;

namespace WebGame
{
    public static class ViewHelpers
    {
        public static HtmlString AccountLink(int accountId, string accountName, string color = null)
        {
            var result = new StringBuilder();
            if (color != null)
                result.AppendFormat("<a href=\"/Player-Info-{0}\"><font color=\"{1}\">{2}</font></a>", accountId, color, accountName);
            else
                result.AppendFormat("<a href=\"/Player-Info-{0}\">{1}</a>", accountId, accountName);

            var session = RuntimeContext.HttpContext?.Session;
            var playerAccountId = session?.GetInt32("AccountId") ?? -1;

            if (accountId == playerAccountId)
            {
                result.AppendFormat(" <img src=\"/images/chat_online.png\" width=\"12\" height=\"12\" />");
            }
            else
            {
                var account = GameServer.GetOnlineAccount(accountId);
                if (account != null)
                    result.AppendFormat(" <img src=\"/images/chat_online.png\" width=\"12\" height=\"12\" alt=\"{0}|{1}\" class=\"chat_user\" title=\"Click to chat with {1}.\" />", accountId, accountName);
                else
                    result.AppendFormat(" <img src=\"/images/chat_offline.png\" width=\"12\" height=\"12\" alt=\"{0}|{1}\" class=\"chat_user\" title=\"{1} is offline.  Click to message.\" offline=\"1\" />", accountId, accountName);
            }

            return new HtmlString(result.ToString());
        }
    }

    public abstract class BaseView<TModel> : RazorPage<TModel>
    {
        public HttpRequest Request => Context.Request;

        public HtmlString AccountLink(int accountId, string accountName, string color = null) =>
            ViewHelpers.AccountLink(accountId, accountName, color);

        public bool LoggedIn => Account != null;

        Account _account;
        public Account Account
        {
            get
            {
                if (_account != null) return _account;
                var id = Context.Session.GetInt32("AccountId");
                if (id.HasValue && id.Value > 0)
                {
                    using (var db = new DBConnection())
                        _account = Account.Load(db.EvaluateRow("select * from account where id = {0}", id.Value));
                }
                return _account;
            }
        }

        public List<string> OpenChatWindows => BaseController.GetOpenChatWindows(Context);

        public bool IsSet(string fieldName) => BaseController.IsSet(fieldName, Context.Request);
        public int GetInt(string fieldName) => BaseController.GetInt(fieldName, Context.Request);
        public long GetLong(string fieldName) => BaseController.GetLong(fieldName, Context.Request);
        public string GetString(string fieldName) => BaseController.GetString(fieldName, Context.Request);
    }

    public abstract class BaseViews : RazorPage
    {
        public HttpRequest Request => Context.Request;

        public HtmlString AccountLink(int accountId, string accountName, string color = null) =>
            ViewHelpers.AccountLink(accountId, accountName, color);

        public bool LoggedIn => Account != null;

        Account _account;
        public Account Account
        {
            get
            {
                if (_account != null) return _account;
                var id = Context.Session.GetInt32("AccountId");
                if (id.HasValue && id.Value > 0)
                {
                    using (var db = new DBConnection())
                        _account = Account.Load(db.EvaluateRow("select * from account where id = {0}", id.Value));
                }
                return _account;
            }
        }

        public List<string> OpenChatWindows => BaseController.GetOpenChatWindows(Context);

        public bool IsSet(string fieldName) => BaseController.IsSet(fieldName, Context.Request);
        public int GetInt(string fieldName) => BaseController.GetInt(fieldName, Context.Request);
        public long GetLong(string fieldName) => BaseController.GetLong(fieldName, Context.Request);
        public string GetString(string fieldName) => BaseController.GetString(fieldName, Context.Request);
    }
}
