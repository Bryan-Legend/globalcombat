using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace WebGame
{
    public abstract class BaseView<TModel> : WebViewPage<TModel>
    {
        public bool LoggedIn
        {
            get { return Account != null; }
        }

        Account account;
        public Account Account
        {
            get
            {
                if (account == null)
                    account = Session["Account"] as Account;
                return account;
            }
            set { Session["Account"] = value; }
        }

        public List<string> OpenChatWindows
        {
            get { return BaseController.OpenChatWindows; }
        }

        public bool IsSet(string fieldName)
        {
            return BaseController.IsSet(fieldName, Request);
        }

        public int GetInt(string fieldName)
        {
            return BaseController.GetInt(fieldName, Request);
        }

        public long GetLong(string fieldName)
        {
            return BaseController.GetLong(fieldName, Request);
        }

        public string GetString(string fieldName)
        {
            return BaseController.GetString(fieldName, Request);
        }

        public static HtmlString AccountLink(int accountId, string accountName, string color = null)
        {
            var result = new StringBuilder();
            if (color != null)
                result.AppendFormat("<a href=\"/Player-Info-{0}\"><font color=\"{1}\">{2}</font></a>", accountId, color, accountName);
            else
                result.AppendFormat("<a href=\"/Player-Info-{0}\">{1}</a>", accountId, accountName);

            var playerAccountId = -1;
            var playerAccount = HttpContext.Current.Session["Account"] as Account;
            if (playerAccount != null)
                playerAccountId = playerAccount.Id;

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

    public abstract class BaseViews : WebViewPage
    {
        public bool LoggedIn
        {
            get { return Account != null; }
        }

        Account account;
        public Account Account
        {
            get
            {
                if (account == null)
                    account = Session["Account"] as Account;
                return account;
            }
            set { Session["Account"] = value; }
        }

        public List<string> OpenChatWindows
        {
            get { return BaseController.OpenChatWindows; }
        }

        public bool IsSet(string fieldName)
        {
            return BaseController.IsSet(fieldName, Request);
        }

        public int GetInt(string fieldName)
        {
            return BaseController.GetInt(fieldName, Request);
        }

        public long GetLong(string fieldName)
        {
            return BaseController.GetLong(fieldName, Request);
        }

        public string GetString(string fieldName)
        {
            return BaseController.GetString(fieldName, Request);
        }
    }
}