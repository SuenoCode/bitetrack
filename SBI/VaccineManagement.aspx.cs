using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Transactions;

namespace SBI
{
    public partial class VaccineManagement : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null || Session["userRole"].ToString().ToLower() != "adminAssisstant" && Session["userRole"].ToString().ToLower() != "vaccinators")
            {
                Response.Redirect("Login.aspx");
            }
        }

        protected void btnOverviewTab_Click(object sender, EventArgs e)
        {
            panelOverview.Visible = true;
            panelAddStock.Visible = false;
            panelInventory.Visible = false;
        }

        protected void btnAddStockTab_Click(object sender, EventArgs e)
        {
            panelOverview.Visible = false;
            panelAddStock.Visible = true;
            panelInventory.Visible = false;
        }

        protected void btnInventoryTab_Click(object sender, EventArgs e)
        {
            panelOverview.Visible = false;
            panelAddStock.Visible = false;
            panelInventory.Visible = true;
        }
    }
}