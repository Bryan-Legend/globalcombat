using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LT
{
    public class WebLogger
    {
        HttpResponseBase response;

        public WebLogger(HttpResponseBase response)
        {
            this.response = response;
            StartLog();
        }

        public void Log(string message, params object[] arguments)
        {
            Log(String.Format(message, arguments));
        }

        void StartLog()
        {
            HttpContext.Current.Items["Logging"] = true;

            var css = @"<style type=""text/css"">
    html {
        font-family: Courier;
        color: #CCCCCC;
        background: #000000;
        border: 3px double #CCCCCC;
        padding: 10px;
    }
</style>
<script>
    var scrollHeight = 0;
    window.setInterval(function() {
      if (scrollHeight != document.body.scrollHeight)
      {
        scrollHeight = document.body.scrollHeight;
        window.scrollTo(0, document.body.scrollHeight);
      }
    }, 100);
</script>
<div id=""data"">
";
            Log(css, false);
        }

        public void Log(string message, bool dumpToDebug = true)
        {
            lock (this)
            {
                if (dumpToDebug)
                    Debug.WriteLine(message);

                if (response != null && response.IsClientConnected)
                {
                    response.Write(message);
                    response.Write("<br />");
                    response.Flush();
                }
            }
        }

        public void Stop()
        {
            response = null;
        }
    }
}
