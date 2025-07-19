using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using LT;
using GlobalCombat.Core;
using System.Text;
using System.Security.Cryptography;

namespace WebGame
{
    public class BaseController : Controller
    {
        #region CGI Helper Methods

        public bool IsSet(string fieldName)
        {
            return IsSet(fieldName, Request);
        }

        public static bool IsSet(string fieldName, HttpRequestBase request)
        {
            return request[fieldName] != null;
        }

        public int GetInt(string fieldName)
        {
            return GetInt(fieldName, Request);
        }

        public static int GetInt(string fieldName, HttpRequestBase request)
        {
            if (!IsSet(fieldName, request))
                return 0;

            int result;
            if (!Int32.TryParse(request[fieldName], out result))
                return 0;
            return result;
        }

        public long GetLong(string fieldName)
        {
            return GetLong(fieldName, Request);
        }

        public static long GetLong(string fieldName, HttpRequestBase request)
        {
            if (!IsSet(fieldName, request))
                return 0;

            long result;
            if (!Int64.TryParse(request[fieldName], out result))
                return 0;
            return result;
        }

        public string GetString(string fieldName)
        {
            return GetString(fieldName, Request);
        }

        public static string GetString(string fieldName, HttpRequestBase request)
        {
            if (!IsSet(fieldName, request))
                return String.Empty;

            return request[fieldName];
        }

        #endregion

        protected override void OnException(ExceptionContext filterContext)
        {
            LT.BasePage.HandleException(filterContext.Exception, System.Web.HttpContext.Current);
            base.OnException(filterContext);
        }

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

        public static List<string> OpenChatWindows
        {
            get
            {
                var result = System.Web.HttpContext.Current.Session["OpenChatWindows"] as List<string>;
                if (result == null)
                {
                    result = new List<string>();
                    System.Web.HttpContext.Current.Session["OpenChatWindows"] = result;
                }

                return result;
            }
        }

        internal static void AddChatWindow(int targetId, string targetName)
        {
            var windowId = String.Format("{0}|{1}", targetId, targetName);
            if (!OpenChatWindows.Contains(windowId))
                OpenChatWindows.Add(windowId);
        }

        protected Account FindAccount(string emailOrAccountName)
        {
            using (var db = new DBConnection())
            {
                var row = db.EvaluateRow("select * from account where name = '{0}' or email = '{0}'", DBConnection.AddSlashes(emailOrAccountName));

                if (row == null)
                    return null;

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

            if (String.IsNullOrEmpty(result))
            {
                GameServer.SendEmail(emailAddress, accountName, "You've been challenged to a game.", String.Format(
@"You've been challenged to a game of Global Combat by {0}.

Visit http://{1}/Game-{2}/ to view the details and join the game.

Account Email: {3}
Password: {4}

You can set your account name and password at http://{1}/Account/Settings
", Account.Name, Request.Url.Host, gameId, account.EmailAddress, password));
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

            if (loginName != System.Web.HttpUtility.HtmlEncode(loginName) || loginName != DBConnection.AddSlashes(loginName))
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
                    db.Execute("update account set name = concat(name, '-', id) where id = {0}", accountId); // append -ID
            }

            return String.Empty;
        }
    }
}