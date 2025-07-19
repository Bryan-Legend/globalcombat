using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LT
{
    public static class Utility
    {
        //static string invalidRegStr;

        public static string MakeValidFileName(this string name)
        {
            name = Regex.Replace(name, @"[^a-zA-Z0-9\-\ ]", "");

            //if (invalidRegStr == null)
            //{
            //    var invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()) + "+#!"); // add plus to the invalids because of url encoding problems.
            //    invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);
            //}

            name = name.Trim();

            if (name.Length > 64)
                name = name.Substring(0, 64);

            return name;

            //return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
