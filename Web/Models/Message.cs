using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using ProtoBuf;
using System.Collections;
using GlobalCombat.Core;
using LT;

namespace WebGame
{
    [ProtoContract]
    public class Message
    {
        public int Id { get; set; }

        public int DestinationId { get; set; }

        public string DestinationName { get; set; }

        public int SourceId { get; set; }

        public string SourceName { get; set; }

        public DateTime Sent { get; set; }

        public string Text { get; set; }

        public string TimeStamp
        {
            get
            {
                var timespan = DateTime.UtcNow - Sent;
                return timespan.PrintTimeSpan();
            }
        }

        public string Print(bool showMeta = true, bool showDelete = false, int accountId = -1)
        {
            var result = new StringBuilder();
            result.Append("<tr>");
            result.Append("<td class=\"Label\">");
            if (SourceId > 1)
            {
                if (SourceId == accountId)
                {
                    result.Append("To ");
                    result.Append(BaseView<Game>.AccountLink(DestinationId, DestinationName));
                }
                else
                    result.Append(BaseView<Game>.AccountLink(SourceId, SourceName));
            }
            result.Append("</td>");
            result.Append("<td>");
            result.Append(Text.Replace("\n", "<br />"));
            if (showMeta)
            {
                result.Append("<br />");
                result.Append("<div class=\"Meta\">");
                result.Append(TimeStamp);
                if (showDelete)
                {
                    result.AppendFormat(" - <a href=\"Messages?DeleteMessage={0}\">Delete</a>", Id);
                }
                result.Append("</div>");
            }
            result.Append("</td>");
            result.Append("</tr>\n");
            return result.ToString();
        }
    }
}
