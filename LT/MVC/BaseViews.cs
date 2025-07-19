using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;

namespace LT
{
    public abstract class BaseView<TModel> : WebViewPage<TModel>
    {
        public static int AccountID
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session["AccountID"] == null)
                    return 0;
                return (int)HttpContext.Current.Session["AccountID"];
            }
            set { HttpContext.Current.Session["AccountID"] = value; }
        }

        public static string AccountName
        {
            get { return (string)HttpContext.Current.Session["AccountName"]; }
            set { HttpContext.Current.Session["AccountName"] = value; }
        }

        public static bool IsLoggedIn
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                    return false;
                return HttpContext.Current.Session["AccountID"] != null;
            }
        }

        public virtual bool Admin
        {
            get { return IsLoggedIn && Convert.ToInt32(AccountID) == 1; }
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

        public IEnumerable<T> GetItems<T>(IList<T> items)
        {
            var size = ViewData.ContainsKey("ItemCount") ? (int)ViewData["ItemCount"] : 50;
            var offset = GetInt("Offset");
            return items.Skip(offset).Take(size);
        }
    }

    public abstract class BaseViews : WebViewPage
    {
        public static int AccountID
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session["AccountID"] == null)
                    return 0;
                return (int)HttpContext.Current.Session["AccountID"];
            }
            set { HttpContext.Current.Session["AccountID"] = value; }
        }

        public static string AccountName
        {
            get { return (string)HttpContext.Current.Session["AccountName"]; }
            set { HttpContext.Current.Session["AccountName"] = value; }
        }

        public static bool IsLoggedIn
        {
            get
            {
                if (HttpContext.Current == null || HttpContext.Current.Session == null)
                    return false;
                return HttpContext.Current.Session["AccountID"] != null;
            }
        }

        public virtual bool Admin
        {
            get { return IsLoggedIn && Convert.ToInt32(AccountID) == 1; }
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

        public IEnumerable<T> GetItems<T>(IList<T> items)
        {
            var size = ViewData.ContainsKey("ItemCount") ? (int)ViewData["ItemCount"] : 50;
            var offset = GetInt("Offset");
            return items.Skip(offset).Take(size);
        }
    }
}