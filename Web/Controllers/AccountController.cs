using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Security.Cryptography;
using System.Text;
using LT;
using System.Collections;
using GlobalCombat.Core;
using System.Net.Mail;

namespace WebGame
{
    public class AccountController : BaseController
    {
        public ActionResult Settings()
        {
            if (!LoggedIn)
                return Redirect("/");

            if (IsSet("NewLoginName"))
                ViewBag.ResultString = ModifyLoginName(GetString("NewLoginName"));

            if (IsSet("NewEmail"))
                ViewBag.ResultString = ModifyEmail(GetString("NewEmail"));

            if (IsSet("NewPassword"))
                ViewBag.ResultString = ModifyPassword(GetString("OldPassword"), GetString("NewPassword"), GetString("NewPasswordVerify"));

            if (IsSet("ForwardEmails"))
                ViewBag.ResultString = ModifyForwardEmails(Request["ForwardEmails"]);

            return View();
        }

        [ValidateInput(false)]
        string ModifyLoginName(string newLoginName)
        {
            if (!Account.Name.EndsWith('-' + Account.Id.ToString()))
                return "You cannot change your login name";

            using (var db = new DBConnection())
            {
                newLoginName = newLoginName.Trim(new char[] { ' ', '\t', '\n', '\r', '0' });

                if (newLoginName != System.Web.HttpUtility.HtmlEncode(newLoginName) || newLoginName != DBConnection.AddSlashes(newLoginName))
                    return "Invalid login name.";

                if (db.Evaluate("select name from account where name = '" + DBConnection.AddSlashes(newLoginName) + "'") != null)
                    return "Login name already taken";

                db.Execute("update account set name = '" + DBConnection.AddSlashes(newLoginName) + "' where id = " + Account.Id);
                Account.Name = newLoginName;

//                SendEmail(Account.EmailAddress, Account.Name, "New Login Name", String.Format(
//@"You've changed your login to {0}
//
//You can change your account name and password at http://{1}/Account/Settings
//", Account.Name,Request.Url.Host));
            }
            return "Login name modified successfully.  It will not be updated in your current games.";
        }

        string ModifyEmail(string newEmail)
        {
            newEmail = newEmail.Trim();

            using (var db = new DBConnection())
            {
                if (!BasePage.IsValidEmailAddress(newEmail))
                    return "Unable to modify email address.<br>You need to enter a valid email address.";

                if (db.Evaluate("select email from account where email = '{0}' and id <> {1}", DBConnection.AddSlashes(newEmail), Account.Id) != null)
                    return "There is already an account with that email address.";

                db.Execute("update account set email = '" + DBConnection.AddSlashes(newEmail) + "' where id = " + Account.Id);
                Account.EmailAddress = newEmail;
            }
            return "Email modified successfully.";
        }

        string ModifyPassword(string oldPassword, string newPassword, string newPasswordVerify)
        {
            using (var db = new DBConnection())
            {
                var realOldPassword = db.Evaluate("select password from account where id = {0}", Account.Id) as String;
                if (oldPassword != realOldPassword)
                {
                    return "Unable to modify your password.<br>You did not enter the correct current password.";
                }
                if (newPassword != newPasswordVerify)
                {
                    return "Unable to modify your password.<br>The new passwords you entered do not match.";
                }
                if (newPassword.Length < 5)
                {
                    return "Unable to modify your password.<br>Password must be at least five letters.";
                }
                db.Execute("update account set password = '{0}' where id = {1}", DBConnection.AddSlashes(newPassword), Account.Id);
            }
            return "Password modified successfully.";
        }

        string ModifyForwardEmails(string newforward)
        {
            using (var db = new DBConnection())
            {
                if (!new string[] { "All", "GameStarts", "AllGame", "None" }.Contains(newforward))
                    return "Invalid Forward Email Setting";

                db.Execute("update account set forward_emails = '" + DBConnection.AddSlashes(newforward) + "' where id = " + Account.Id);
                Account.ForwardEmails = newforward;
            }
            return "Email notifications modified successfully.";
        }

        public ActionResult LostPassword()
        {
            if (IsSet("EmailAddress"))
                ViewBag.ResultString = ResetPassword(GetString("EmailAddress"));
            
            return View();
        }

        string ResetPassword(string email)
        {
            if (String.IsNullOrWhiteSpace(email))
                return "Invalid login name or email address.";

            using (var db = new DBConnection())
            {
                var accountRow = db.EvaluateRow("select * from account where name = '{0}' or email = '{0}'", DBConnection.AddSlashes(email));

                if (accountRow == null)
                    return "Account not found.";

                var newPassword = UserPage<int>.GeneratePassword(8);
                db.Execute("update account set password = '{0}' where id = {1}", DBConnection.AddSlashes(newPassword), (int)accountRow["id"]);

                GameServer.SendEmail((string)accountRow["email"], GameServer.FromAddress, "Global Combat Password Reset", "Login Name: " + (string)accountRow["name"] + "\nPassword: " + newPassword + "\n\nIf you have a hard time remembering it, try tattooing it to your leg for easy access. \n\nThis request was sent from https://globalcombat.com/IpAddresses?IPAddress=" + Request.UserHostAddress);

                return "A new password was sent to your email.";
            }
        }

        public ActionResult LogOn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult LogOn(LogOnModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = Login(model.EmailAddress, model.Password);

                if (result == null)
                {
                    FormsAuthentication.SetAuthCookie(Account.EmailAddress, true);

                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", result);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public string Login(string loginName, string password)
        {
            using (var db = new DBConnection())
            {
                var accountRow = db.EvaluateRow("select * from account where name = '{0}' or email = '{0}'", DBConnection.AddSlashes(loginName));

                if (accountRow == null)
                    return "Unknown login name or email address.";

                if ((int)accountRow["disabled_by"] != 0)
                    return "That account has been permanently disabled.";

                if (password != (string)accountRow["password"] && LT.UserPage<int>.CalculateHash(password) != (string)accountRow["password"])
                    return "Bad Password";

                SetSession(accountRow);
                return null;
            }
        }

        public static void SetSession(Hashtable accountRow)
        {
            var account = Account.Load(accountRow);

            if (account == null)
                throw new Exception("Unable to log into null account.");

            account.SessionKey = System.Web.HttpContext.Current.Session.SessionID;
            System.Web.HttpContext.Current.Session["Account"] = account;

            var agent = System.Web.HttpContext.Current.Request.UserAgent;
            if (agent.Length > 250)
                agent = agent.Substring(0, 250);

            using (var db = new DBConnection())
            {
                db.Execute("update account set last_on = {0}, num_logins = num_logins + 1 where id = {1}", Utility.UnixTimestamp(DateTime.UtcNow), account.Id);
                //if (!account.IsAdmin)
                    db.Execute("insert ignore into account_login (account_id, datetime, ipaddress, browser, adminused) values ({0}, {1}, '{2}', '{3}', 'False')", account.Id, Utility.UnixTimestamp(DateTime.UtcNow), DBConnection.AddSlashes(System.Web.HttpContext.Current.Request.UserHostAddress), DBConnection.AddSlashes(agent));

                foreach (var message in db.EvaluateTable("select distinct(from_id), account.Name from message join account on account.Id = from_id where to_id = {0} and readflag = 'False'", account.Id))
                {
                    AddChatWindow((int)message["from_id"], (string)message["Name"]);
                }

                db.Execute("update message set readflag = 'True' where to_id = {0}", account.Id);
            }

            var existingAccount = GameServer.GetOnlineAccount(account.Id);
            if (existingAccount == null)
            {
                if (account != null)
                {
                    lock (GameServer.OnlineAccounts)
                    {
                        GameServer.OnlineAccounts.Add(account);
                    }
                }
                else
                    throw new Exception("Account record was null.  This should never happen.");
            }
            else
                existingAccount.SessionKey = System.Web.HttpContext.Current.Session.SessionID;
        }

        public ActionResult LogOff()
        {
            if (!LoggedIn)
                return RedirectToAction("Index", "Home");

            var account = GameServer.GetOnlineAccount(Account.Id);
            if (account != null)
            {
                lock (GameServer.OnlineAccounts)
                {
                    GameServer.OnlineAccounts.Remove(account);
                }
            }

            Session.Abandon();
            FormsAuthentication.SignOut();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                int accountId;
                var errorMessage = CreateAccount(model.UserName, model.Password, model.ConfirmPassword, model.Email, out accountId);
                if (!String.IsNullOrEmpty(errorMessage))
                {
                    ModelState.AddModelError("", errorMessage);
                }
                else
                {
                    using (var db = new DBConnection())
                    {
                        var row = db.EvaluateRow("select * from account where id = {0}", accountId);
                        SetSession(row);
                    }
                    FormsAuthentication.SetAuthCookie(model.Email, true);
                    return RedirectToAction("Index", "Home");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult Contact()
        {
            if (!LoggedIn)
                return Redirect("/");

            if (IsSet("Comments"))
                ViewBag.ResultString = ContactEmail(GetString("Subject"), GetString("Comments"));

            return View();
        }

        [ValidateInput(false)]
        string ContactEmail(string subject, string comments)
        {
            if (String.IsNullOrWhiteSpace(subject))
                return "Subject cannot be empty.";
            
            if (String.IsNullOrEmpty(comments))
                return "Message cannot be empty.";

            GameServer.SmtpClient.Send
            (
                Account.EmailAddress + " (" + Account.Name + ")",
                GameServer.ContactEmail,
                String.Format("[{0}] {1}", LT.BasePage.SiteName, subject),
                String.Format("{0}\n\n{1}\n{2}\nhttps://globalcombat.com/Player-Info-{3}", comments, Account.Name, Account.EmailAddress, Account.Id)
            );

            return "Thank you for your feedback.";
        }
    }
}