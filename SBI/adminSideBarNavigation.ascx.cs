using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBI
{
    public partial class adminSideBarNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);

            lnkUserManagement.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                     (currentPage.Equals("UserManagement.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkSettings.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                    (currentPage.Equals("Settings.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");
        }

        protected void btnUserManagement_Click(object sender, EventArgs e)
        {
            Response.Redirect("UserManagement.aspx");
        }
        protected void btnSettings_Click(object sender, EventArgs e)
        {
            Response.Redirect("Settings.aspx");
        }
        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Response.Redirect("Home.aspx");
        }
    }
}