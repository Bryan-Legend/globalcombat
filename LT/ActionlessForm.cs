using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;

// http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dnaspp/html/urlrewriting.asp

// Usage:
// <%@ Register TagPrefix="lt" Namespace="LT" Assembly="LT" %>
// <lt:ActionlessForm id="Form1" method="post" runat="server">
// </lt:ActionlessForm>

namespace LT
{
	public class ActionlessForm : System.Web.UI.HtmlControls.HtmlForm
	{
		protected override void RenderAttributes(HtmlTextWriter writer)
		{
			writer.WriteAttribute("name", this.Name);
			base.Attributes.Remove("name");

			writer.WriteAttribute("method", this.Method);
			base.Attributes.Remove("method");

			this.Attributes.Render(writer);

			base.Attributes.Remove("action");

			if (base.ID != null)
				writer.WriteAttribute("id", base.ClientID);
		}
	}
}
