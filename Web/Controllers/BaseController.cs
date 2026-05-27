using System;
using System.Collections.Generic;
using System.Text.Json;
using LT;
using GlobalCombat.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebGame
{
    public class BaseController : Controller
    {
        const string SessionAccountIdKey = "AccountId";
        const string SessionChatWindowsKey = "OpenChatWindows";

        public static string GetField(HttpRequest request, string fieldName)
        {
            if (request == null) return null;
            if (request.HasFormContentType)
            {
                var form = request.Form[fieldName].ToString();
                if (!string.IsNullOrEmpty(form)) return form;
            }
            var query = request.Query[fieldName].ToString();
            return string.IsNullOrEmpty(query) ? null : query;
        }

        public bool IsSet(string fieldName) => IsSet(fieldName, Request);
        public static bool IsSet(string fieldName, HttpRequest request) => GetField(request, fieldName) != null;

        public int GetInt(string fieldName) => GetInt(fieldName, Request);
        public static int GetInt(string fieldName, HttpRequest request)
        {
            var v = GetField(request, fieldName);
            return int.TryParse(v, out var r) ? r : 0;
        }

        public long GetLong(string fieldName) => GetLong(fieldName, Request);
        public static long GetLong(string fieldName, HttpRequest request)
        {
            var v = GetField(request, fieldName);
            return long.TryParse(v, out var r) ? r : 0;
        }

        public string GetString(string fieldName) => GetString(fieldName, Request);
        public static string GetString(string fieldName, HttpRequest request)
            => GetField(request, fieldName) ?? string.Empty;

        public override void OnActionExecuting(ActionExecutingContext context) { base.OnActionExecuting(context); }

        public bool LoggedIn => Account != null;

        Account _account;
        public Account Account
        {
            get
            {
                if (_account != null) return _account;
                var id = HttpContext.Session.GetInt32(SessionAccountIdKey);
                if (id.HasValue && id.Value > 0)
                {
                    using (var db = new DBConnection())
                        _account = Account.Load(db.EvaluateRow("select * from account where id = {0}", id.Value));
                }
                return _account;
            }
            set
            {
                _account = value;
                if (value == null)
                    HttpContext.Session.Remove(SessionAccountIdKey);
                else
                    HttpContext.Session.SetInt32(SessionAccountIdKey, value.Id);
            }
        }

        public static List<string> GetOpenChatWindows(HttpContext httpContext)
        {
            var bytes = httpContext.Session.Get(SessionChatWindowsKey);
            if (bytes == null) return new List<string>();
            return JsonSerializer.Deserialize<List<string>>(bytes) ?? new List<string>();
        }

        public static void SetOpenChatWindows(HttpContext httpContext, List<string> windows)
        {
            httpContext.Session.Set(SessionChatWindowsKey, JsonSerializer.SerializeToUtf8Bytes(windows));
        }

        public List<string> OpenChatWindows => GetOpenChatWindows(HttpContext);

        public static void AddChatWindow(HttpContext httpContext, int targetId, string targetName)
        {
            var windowId = $"{targetId}|{targetName}";
            var windows = GetOpenChatWindows(httpContext);
            if (!windows.Contains(windowId))
            {
                windows.Add(windowId);
                SetOpenChatWindows(httpContext, windows);
            }
        }

        internal void AddChatWindow(int targetId, string targetName)
        {
            var windowId = $"{targetId}|{targetName}";
            var windows = OpenChatWindows;
            if (!windows.Contains(windowId))
            {
                windows.Add(windowId);
                SetOpenChatWindows(HttpContext, windows);
            }
        }

        protected Account FindAccount(string emailOrAccountName)
        {
            using (var db = new DBConnection())
            {
                var row = db.EvaluateRow("select * from account where name = '{0}' or email = '{0}'", DBConnection.AddSlashes(emailOrAccountName));
                if (row == null) return null;
                return Account.Load(row);
            }
        }

        protected string CreateAccount(string emailAddress, int gameId, out int accountId)
        {
            accountId = 0;
            if (!LT.BasePage.IsValidEmailAddress(emailAddress))
                return "You need to enter a valid email address.";

            var password = LT.UserPage<int>.GeneratePassword(8);
            var splitEmail = emailAddress.Split('@');
            var accountName = splitEmail[0].Substring(0, splitEmail[0].Length < 3 ? 0 : splitEmail[0].Length - 3) + "...";

            var result = CreateAccount(accountName, password, password, emailAddress, out accountId, true);

            if (string.IsNullOrEmpty(result))
            {
                GameServer.SendEmail(emailAddress, accountName, "You've been challenged to a game.", string.Format(
@"You've been challenged to a game of Global Combat by {0}.

Visit http://{1}/Game-{2}/ to view the details and join the game.

Account Email: {3}
Password: {4}

You can set your account name and password at http://{1}/Account/Settings
", Account.Name, Request.Host, gameId, emailAddress, password));
            }

            return result;
        }

        protected string CreateAccount(string loginName, string password, string passwordVerify, string email, out int accountId, bool isTempLoginName = false)
        {
            accountId = 0;
            loginName = loginName.Trim(new char[] { ' ', '\t', '\n', '\r', '0' });
            email = email.Trim();

            if (!LT.BasePage.IsValidEmailAddress(email))
                return "You need to enter a valid email address.";

            if (loginName != System.Net.WebUtility.HtmlEncode(loginName) || loginName != DBConnection.AddSlashes(loginName))
                return "Invalid login name.";

            using (var db = new DBConnection())
            {
                if (db.Evaluate("select name from account where name = '" + DBConnection.AddSlashes(loginName) + "'") != null)
                    return "Login name already taken";

                if (db.Evaluate("select email from account where email = '" + DBConnection.AddSlashes(email) + "'") != null)
                    return "There is already an account with that email address.";

                if (password != passwordVerify)
                    return "The passwords you entered do not match.";
                if (password.Length < 5)
                    return "Password must be at least five letters.";

                db.Execute
                (
                    "insert into account (name, password, signed_up, email, referred_by, OptOutKey) values('{0}', '{1}', '{2}', '{3}', '{4}', {5})",
                    DBConnection.AddSlashes(loginName),
                    DBConnection.AddSlashes(password),
                    Utility.UnixTimestamp(DateTime.Now),
                    DBConnection.AddSlashes(email),
                    GetInt("ReferredBy"),
                    Utility.Random.Next(1000000)
                );

                accountId = Convert.ToInt32(db.LastInsertID);

                if (isTempLoginName)
                    db.Execute("update account set name = concat(name, '-', id) where id = {0}", accountId);
            }

            return string.Empty;
        }
    }
}
