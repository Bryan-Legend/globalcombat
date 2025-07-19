using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.IO;
using System.Threading;

namespace LT
{
    public class BasePage : System.Web.UI.Page
    {
        public static string SiteName
        {
            get { return ConfigurationManager.AppSettings["SiteName"]; }
        }

        public string SiteAddress
        {
            get { return Request.Url.Authority + (Request.ApplicationPath.Length > 1 ? Request.ApplicationPath : String.Empty); }
        }

        public static string ServerAddress
        {
            get { return ConfigurationManager.AppSettings["ServerAddress"]; }
        }

        static Random random = new Random();
        public static Random RandomGenerator
        {
            get { return BasePage.random; }
        }

        public static bool Flip()
        {
            return random.Next(2) == 0;
        }

        #region Database

        DBConnection database;
        public DBConnection Database
        {
            get
            {
                if (database == null)
                {
                    database = new DBConnection();
                }
                return database;
            }
        }

        public bool DatabaseUsed
        {
            get { return database != null; }
        }

        public int Execute(string command, params object[] arguments)
        {
            return Execute(String.Format(command, arguments));
        }

        public int Execute(string command)
        {
            return Database.Execute(command);
        }

        public object Evaluate(string command, params object[] arguments)
        {
            return Evaluate(String.Format(command, arguments));
        }

        public object Evaluate(string command)
        {
            return Database.Evaluate(command);
        }

        public IDataReader OpenQuery(string command, params object[] arguments)
        {
            return OpenQuery(String.Format(command, arguments));
        }

        public IDataReader OpenQuery(string command)
        {
            return Database.OpenQuery(command);
        }

        public static Hashtable GetSingleRow(IDataRecord command)
        {
            return DBConnection.GetSingleRow(command);
        }

        public Hashtable EvaluateRow(string command, params object[] arguments)
        {
            return EvaluateRow(String.Format(command, arguments));
        }

        public Hashtable EvaluateRow(string command)
        {
            return Database.EvaluateRow(command);
        }

        public List<Hashtable> EvaluateTable(string command, params object[] arguments)
        {
            return EvaluateTable(String.Format(command, arguments));
        }

        public List<Hashtable> EvaluateTable(string command)
        {
            return Database.EvaluateTable(command);
        }

        public long LastInsertID
        {
            get { return Database.LastInsertID; }
        }

        public static object IfNull(object nullable, object value)
        {
            return DBConnection.IfNull(nullable, value);
        }

        public override void Dispose()
        {
            try
            {
                if (database != null)
                {
                    database.Dispose();
                }
            }
            finally
            {
                base.Dispose();
            }
        }

        /// <summary>
        /// Quotes the specified string with backslashes. 
        /// The characters to be quoted are: quote('), double quote("), backslash(\) and null(0).
        /// </summary>
        /// <param name="text">The string to be quoted with backslashes.</param>
        /// <returns>Returns the quoted string.</returns>
        public static string AddSlashes(string text)
        {
            return DBConnection.AddSlashes(text);
        }

        #endregion

        #region CGI Helper Methods

        public bool IsSet(string fieldName)
        {
            return IsSet(fieldName, Context);
        }

        public static bool IsSet(string fieldName, HttpContext context)
        {
            return context.Request[fieldName] != null;
        }

        public int GetInt(string fieldName)
        {
            return GetInt(fieldName, Context);
        }

        public static int GetInt(string fieldName, HttpContext context)
        {
            if (!IsSet(fieldName, context))
                return 0;

            int result;
            if (!Int32.TryParse(context.Request[fieldName], out result))
                return 0;
            return result;
        }

        public long GetLong(string fieldName)
        {
            return GetLong(fieldName, Context);
        }

        public static long GetLong(string fieldName, HttpContext context)
        {
            if (!IsSet(fieldName, context))
                return 0;

            long result;
            if (!Int64.TryParse(context.Request[fieldName], out result))
                return 0;
            return result;
        }

        public string GetString(string fieldName)
        {
            return GetString(fieldName, Context);
        }

        public static string GetString(string fieldName, HttpContext context)
        {
            if (!IsSet(fieldName, context))
                return String.Empty;

            return context.Request[fieldName];
        }

        #endregion

        //// http://www.regexlib.com/REDetails.aspx?regexp_id=711
        //private static Regex emailRegex =
        //    new Regex(@"^((?>[a-zA-Z\d!#$%&'*+\-/=?^_`{|}~]+\x20*|""((?=[\x01-\x7f])[^""\\]|\\[\x01-\x7f])*""\x20*)*(?<angle><))?((?!\.)(?>\.?[a-zA-Z\d!#$%&'*+\-/=?^_`{|}~]+)+|""((?=[\x01-\x7f])[^""\\]|\\[\x01-\x7f])*"")@(((?!-)[a-zA-Z\d\-]+(?<!-)\.)+[a-zA-Z]{2,}|\[(((?(?<!\[)\.)(25[0-5]|2[0-4]\d|[01]?\d?\d)){4}|[a-zA-Z\d\-]*[a-zA-Z\d]:((?=[\x01-\x7f])[^\\\[\]]|\\[\x01-\x7f])+)\])(?(angle)>)$");

        public static bool IsValidEmailAddress(string emailAddress)
        {
            // verify the address
            //if (!emailRegex.IsMatch(emailAddress))
            //    return false;

            if (!ActiveUp.Net.Mail.Validator.ValidateSyntax(emailAddress))
                return false;

            var host = emailAddress.Split('@')[1];
            var mxs = ActiveUp.Net.Mail.Validator.GetMxRecords(host);
            if (mxs == null || mxs.Count <= 0)
            {
                return false;
            }

            //// lookup the hostname
            //try
            //{
            //    Dns.GetHostEntry(host);
            //}
            //catch(SocketException) 
            //{
            //    return false;
            //}
            return true;
        }

        // http://weblogs.asp.net/rosherove/archive/2003/05/13/6963.aspx
        private static Regex stripHtmlRegex = new Regex(@"<(.|\n)*?>");
        public static string StripHtml(string html)
        {
            //This pattern Matches everything found inside html tags;
            //(.|\n) - > Look for any character or a new line
            // *?  -> 0 or more occurences, and make a non-greedy search meaning
            //That the match will stop at the first available '>' it sees, and not at the last one
            //(if it stopped at the last one we could have overlooked 
            //nested HTML tags inside a bigger HTML tag..)
            // Thanks to Oisin and Hugh Brown for helping on this one... 
            return stripHtmlRegex.Replace(html, String.Empty).Replace("&nbsp;", String.Empty);
        }

        public static uint IpAddressToInteger(string ipAddress)
        {
            uint r = 0;
            foreach (var s in ipAddress.Split('.'))
                r = (r << 8) ^ UInt32.Parse(s);
            return r;
        }

        public static string CreateErrorMessage(Exception exception, HttpContext context)
        {
            var httpException = exception as HttpException;
            if (httpException != null && (httpException.GetHttpCode() == 405 || httpException.GetHttpCode() == 404 || httpException.GetHttpCode() == 403))
            {
                context.Response.StatusCode = httpException.GetHttpCode();
                context.Response.SuppressContent = true;
                context.Response.End();
                return null;
            }

            var errorMessage = new StringBuilder();

            if (exception is HttpUnhandledException)
                exception = exception.InnerException;

            errorMessage.Append(exception.ToString());

            if (httpException != null)
                errorMessage.Append("\n\nHTTP EXCEPTION CODE: " + httpException.GetHttpCode());

            if (exception.InnerException != null)
            {
                errorMessage.Append("\n\n ***INNER EXCEPTION*** \n");
                errorMessage.Append(exception.InnerException.ToString());
            }

            if (context != null)
            {
                //if (context.Request.IsLocal)
                //    return;
                errorMessage.AppendFormat("\n\nRequest Path = {0}\n", context.Request.Url);

                errorMessage.Append("\n\n ***REQUEST PARAMETERS*** \n");
                foreach (string name in context.Request.Params.Keys)
                {
                    errorMessage.AppendFormat("\n{0} = {1};", name, context.Request[name]);
                }

                if (context.Request.RequestType == "POST")
                {
                    errorMessage.Append("\n\n ***POST DATA*** \n");

                    using (var reader = new StreamReader(context.Request.InputStream))
                    {
                        errorMessage.Append(reader.ReadToEnd());
                    }
                }

                if (context.Session != null)
                {
                    errorMessage.Append("\n\n ***SESSION VARIABLES*** \n");
                    foreach (string key in context.Session.Keys)
                    {
                        var value = context.Session[key];
                        var list = value as IEnumerable;
                        if (list != null && !(list is String))
                        {
                            errorMessage.AppendFormat("\n{0} = ", key);
                            foreach (var item in list)
                            {
                                errorMessage.AppendFormat("{0} ", item);
                            }
                        }
                        else
                        {
                            errorMessage.AppendFormat("\n{0} = {1};", key, value);
                        }
                    }
                }

                var basePage = context.Handler as BasePage;
                if (basePage != null && basePage.database != null && basePage.database.LastQuery != null)
                {
                    errorMessage.Append("\n\n ***LAST QUERY*** \n");
                    errorMessage.Append(basePage.database.LastQuery);
                }
            }

            System.Diagnostics.Debug.Print(errorMessage.ToString());

            return errorMessage.ToString();
        }

        public static bool HandleException(Exception exception, HttpContext context)
        {
            var message = CreateErrorMessage(exception, context);
            if (message == null)
                return false;

            HandleException(message);
            return true;
        }

        public static void HandleException(string errorMessage)
        {
            new Thread(() =>
            {
                try
                {
                    var mailClient = new SmtpClient(ConfigurationManager.AppSettings["MailServer"]);
                    mailClient.Send
                    (
                        $"{SiteName} Web Server <{ConfigurationManager.AppSettings["ErrorEmail"]}>",
                        ConfigurationManager.AppSettings["ErrorEmail"],
                        SiteName + " Error " + Guid.NewGuid().ToString(),
                        errorMessage.ToString()
                    );
                }
                catch (SmtpException)
                {
                }
            }).Start();
        }
    }
}
