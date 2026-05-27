using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace LT
{
    public static class BasePage
    {
        public static string SiteName => AppConfig.Get("SiteName");

        public static string ServerAddress => AppConfig.Get("ServerAddress");

        static readonly Random random = new Random();
        public static Random RandomGenerator => random;

        public static bool Flip() => random.Next(2) == 0;

        public static Hashtable GetSingleRow(IDataRecord command) => DBConnection.GetSingleRow(command);

        public static object IfNull(object nullable, object value) => DBConnection.IfNull(nullable, value);

        public static string AddSlashes(string text) => DBConnection.AddSlashes(text);

        public static bool IsValidEmailAddress(string emailAddress)
        {
            if (string.IsNullOrWhiteSpace(emailAddress))
                return false;
            try
            {
                var addr = new MailAddress(emailAddress);
                return addr.Address == emailAddress;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        static readonly Regex stripHtmlRegex = new Regex(@"<(.|\n)*?>", RegexOptions.Compiled);
        public static string StripHtml(string html)
        {
            return stripHtmlRegex.Replace(html, String.Empty).Replace("&nbsp;", String.Empty);
        }

        public static uint IpAddressToInteger(string ipAddress)
        {
            uint r = 0;
            foreach (var s in ipAddress.Split('.'))
                r = (r << 8) ^ UInt32.Parse(s);
            return r;
        }

        public static string CreateErrorMessage(Exception exception)
        {
            var errorMessage = new StringBuilder();
            errorMessage.Append(exception.ToString());

            if (exception.InnerException != null)
            {
                errorMessage.Append("\n\n ***INNER EXCEPTION*** \n");
                errorMessage.Append(exception.InnerException.ToString());
            }

            System.Diagnostics.Debug.Print(errorMessage.ToString());
            return errorMessage.ToString();
        }

        public static bool HandleException(Exception exception)
        {
            var message = CreateErrorMessage(exception);
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
                    var mailClient = new SmtpClient(AppConfig.Get("MailServer"));
                    mailClient.Send
                    (
                        $"{SiteName} Web Server <{AppConfig.Get("ErrorEmail")}>",
                        AppConfig.Get("ErrorEmail"),
                        SiteName + " Error " + Guid.NewGuid().ToString(),
                        errorMessage
                    );
                }
                catch (SmtpException)
                {
                }
            }).Start();
        }
    }
}
