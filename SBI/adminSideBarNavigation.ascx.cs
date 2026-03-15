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
            if (!IsPostBack)
            {
                string role = Session["userRole"] as string;

                if (role == "staff")
                {

                    adminHeader.Visible = false;
                    lnkUserManagement.Visible =false;
                    lnkAudit.Visible = false;
                }
                else if(role == "admin")
                {
                    mainNavigation.Visible = false;
                    lnkDashboard.Visible = false;
                    lnkCaseSurv.Visible = false;
                    lnkPatients.Visible = false;
                    lnkVaccReg.Visible = false;
                    lnkReports.Visible = false;
                }


            }
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

            lnkAudit.CssClass = "sidebar-item block px-4 py-2 rounded-lg hover:bg-blue-700" +
                                    (currentPage.Equals("Audit.aspx", StringComparison.OrdinalIgnoreCase) ? " bg-blue-500 text-white" : "");


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

        protected void btnSignOut_Click(object sender, EventArgs e)
        {
            Session.Clear();
            Response.Redirect("Login.aspx");
        }

        protected void btnAudit_Click(object sender, EventArgs e)
        {
            Response.Redirect("Audit.aspx");
        }
    }
}