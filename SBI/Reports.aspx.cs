using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBI
{
    public partial class Reports : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null || Session["userRole"].ToString().ToLower() != "adminAssisstant" || Session["userRole"].ToString().ToLower() != "vaccinators")
            {
                Response.Redirect("Login.aspx");
            }
        }
    }
}