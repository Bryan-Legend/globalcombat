using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Collections;
using System.Text;
using System.Security.Cryptography;

namespace LT
{
    public class BaseController : Controller
    {
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

        protected override void OnException(ExceptionContext filterContext)
        {
            LT.BasePage.HandleException(filterContext.Exception, System.Web.HttpContext.Current);
            base.OnException(filterContext);
        }

        public int AccountID
        {
            get { return (int)Session["AccountID"]; }
            set { Session["AccountID"] = value; }
        }

        public string AccountName
        {
            get { return (string)Session["AccountName"]; }
            set { Session["AccountName"] = value; }
        }

        public bool IsLoggedIn
        {
            get { return Session["AccountID"] != null; }
        }

        public virtual bool Admin
        {
            get { return IsLoggedIn && Convert.ToInt32(AccountID) == 1; }
        }
    }
}