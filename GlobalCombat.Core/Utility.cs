using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

namespace GlobalCombat.Core
{
    public static class Utility
    {
        public static Random Random = new Random();

        /// <summary>
        /// Returns the specified time measured in the number of seconds.
        /// </summary>
        /// <param name="dateTime">The System.DateTime to obtain the number of seconds from.</param>
        /// <returns>Returns the specified time measured in the number of seconds since January 1 1970 00:00:00 GMT.</returns>
        public static int UnixTimestamp(DateTime dateTime)
        {
            long initialTicks = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
            long dateTicks = dateTime.ToUniversalTime().Ticks;
            int elapsedSeconds = System.Convert.ToInt32((dateTicks - initialTicks) / 10000000);
            return elapsedSeconds;
        }

        /// <summary>
        /// Creates a new System.DateTime from the specified timestamp (time measured in the number of seconds).
        /// </summary>
        /// <param name="unixTimestamp">The timestamp that represents the date to be created.</param>
        /// <returns>Returns a new System.DateTime instance created from the specified timestamp.</returns>
        public static DateTime FromUnixTimestamp(int unixTimestamp)
        {
            long initialTicks = new System.DateTime(1970, 1, 1, 0, 0, 0, 0).Ticks;
            long elapsedUtcTicks = System.Convert.ToInt64(unixTimestamp) * 10000000;
            elapsedUtcTicks += initialTicks;
            System.DateTime theDate = new System.DateTime(elapsedUtcTicks);//.ToLocalTime();
            return theDate;
        }

        public static string PrintTimeSpan(this TimeSpan time, bool isInPast = true)
        {
            string suffix;
            if (isInPast)
                suffix = "ago";
            else
                suffix = "left";

            if (time.Days > 365)
                return String.Format("{0} yrs, {1} days {2}", time.Days / 365, time.Days % 365, suffix);
            else if (time.Days >= 1)
                return String.Format("{0} days, {1} hrs {2}", time.Days, time.Hours, suffix);
            else if (time.Hours >= 1)
                return String.Format("{0} hrs, {1} min {2}", time.Hours, time.Minutes, suffix);
            else if (time.Minutes >= 1)
                return String.Format("{0} min, {1} sec {2}", time.Minutes, time.Seconds, suffix);
            return String.Format("{0} sec {1}", time.Seconds, suffix);
        }
    }
}