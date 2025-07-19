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
	public static class HttpResponseExtensions
	{
		public static void Write(this HttpResponse response, string format, params object[] arguments)
		{
			response.Write(String.Format(format, arguments));
		}
	}
}
