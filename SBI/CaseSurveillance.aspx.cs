using System;
using System.Collections.Generic;

namespace SBI
{
    public partial class CaseSurveillance : System.Web.UI.Page
    {
        public class Dose
        {
            public string Label { get; set; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {

                if (Session["userRole"] == null || Session["userRole"].ToString().ToLower() != "adminAssisstant" || Session["userRole"].ToString().ToLower() != "vaccinators")
                {
                    Response.Redirect("Login.aspx");
                }

            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            // db db
        }
    }
}