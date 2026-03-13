using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI;

namespace SBI
{
    public partial class PatientRegistration : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FBindGrid();
            }

            /*
            if (Session["userRole"] == null || 
               (Session["userRole"].ToString().ToLower() != "adminassistant" &&
                Session["userRole"].ToString().ToLower() != "vaccinators"))
            {
                Response.Redirect("Login.aspx");
            }
            */
        }

        private void FBindGrid()
        {
            // Bind Patients GridView
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string patientQuery = @"
                    SELECT 
                        patient_id, 
                        fname, 
                        lname,
                        gender,
                        contact_no, 
                        address,
                        date_added 
                    FROM Patient 
                    ORDER BY date_added ASC";

                using (SqlCommand cmd = new SqlCommand(patientQuery, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtPatients = new DataTable();
                    da.Fill(dtPatients);

                    gvPatients.DataSource = dtPatients;
                    gvPatients.DataBind();
                }
            }

            // Bind Cases GridView
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string caseQuery = @"
                    SELECT 
                        bc.case_id, 
                        bc.patient_id, 
                        bc.case_no, 
                        bc.date_of_bite,
                        bc.place_of_bite,
                        bc.type_of_exposure,
                        bc.site_of_bite,
                        bc.category,
                        p.fname + ' ' + p.lname AS PatientName
                    FROM BiteCase bc
                    INNER JOIN Patient p ON bc.patient_id = p.patient_id
                    ORDER BY bc.date_of_bite ASC";

                using (SqlCommand cmd = new SqlCommand(caseQuery, conn))
                {
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dtCases = new DataTable();
                    da.Fill(dtCases);

                    gvCases.DataSource = dtCases;
                    gvCases.DataBind();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Insert logic here later
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {

        }
    }
}