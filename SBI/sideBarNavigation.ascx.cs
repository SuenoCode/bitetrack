using System;
using System.Web.UI;

namespace SBI
{
    public partial class sideBarNavigation : System.Web.UI.UserControl
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            string currentPage = System.IO.Path.GetFileName(Request.Url.AbsolutePath);

            lnkDashboard.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                     (currentPage.Equals("Dashboard.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkPatients.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                    (currentPage.Equals("PatientRegistration.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkCaseSurv.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                   (currentPage.Equals("CaseSurveillance.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkVaccReg.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                    (currentPage.Equals("VaccineManagement.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkReports.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                   (currentPage.Equals("Reports.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkUserManagement.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                     (currentPage.Equals("UserManagement.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");

            lnkSettings.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                    (currentPage.Equals("Settings.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");
        }

        protected void btnDashboard_Click(object sender, EventArgs e)
        {
            Response.Redirect("Dashboard.aspx");
        }

        protected void btnPatientRegistration_Click(object sender, EventArgs e)
        {
            Response.Redirect("PatientRegistration.aspx");
        }

        protected void btnCaseSurveillance_Click(object sender, EventArgs e)
        {
            Response.Redirect("CaseSurveillance.aspx");
        }

        protected void btnVaccineManagement_Click(object sender, EventArgs e)
        {
            Response.Redirect("VaccineManagement.aspx");
        }

        protected void btnReports_Click(object sender, EventArgs e)
        {
            Response.Redirect("Reports.aspx");
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