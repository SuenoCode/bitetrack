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
                // Pre-Exposure doses
                var preExposureDoses = new List<Dose>
                {
                    new Dose{ Label="Day 0" },
                    new Dose{ Label="Day 7" },
                    new Dose{ Label="Day 28" },
                    new Dose{ Label="Booster 1" },
                    new Dose{ Label="Booster 2" }
                };

                rptPreExposure.DataSource = preExposureDoses;
                rptPreExposure.DataBind();

                // Post-Exposure doses
                var postExposureDoses = new List<Dose>
                {
                    new Dose{ Label="Day 0" },
                    new Dose{ Label="Day 3" },
                    new Dose{ Label="Day 7" },
                    new Dose{ Label="Day 14" },
                    new Dose{ Label="Day 28/30" }
                };

                rptPostExposure.DataSource = postExposureDoses;
                rptPostExposure.DataBind();
            }
        }

        protected void btnView_Click(object sender, EventArgs e)
        {
            // db db
        }
    }
}