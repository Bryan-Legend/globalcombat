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

namespace LT
{
	public static class DateTimeExtensions
	{
        public static string ToTimeSince(this DateTime timeStamp)
        {
            var now = DateTime.Now;
            if (now < timeStamp)
                return "future";

            var timeSpan = DateTime.Now - timeStamp;

            if (timeSpan.Days >= 365)
                return String.Format("{0}y {1}d", timeSpan.Days / 365, timeSpan.Days % 365);

            if (timeSpan.Days >= 1)
                return String.Format("{0}d {1}h", timeSpan.Days, timeSpan.Hours);

            if (timeSpan.Hours >= 1)
                return String.Format("{0}h {1}m", timeSpan.Hours, timeSpan.Minutes);

            if (timeSpan.Minutes >= 1)
                return String.Format("{0}m {1}s", timeSpan.Minutes, timeSpan.Seconds);

            return String.Format("{0}s", timeSpan.Seconds);
        }
	}
}
