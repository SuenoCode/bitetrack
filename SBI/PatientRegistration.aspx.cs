using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

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

                pnlPreviewEmpty.Visible = true;
                pnlPatientPreview.Visible = false;
                pnlCasePreview.Visible = false;

                hfActivePanel.Value = "addPatientPanel";
            }
        }

        private void FBindGrid()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string patientQuery = @"
                SELECT patient_id,fname,lname,gender,contact_no,address,date_added
                FROM Patient
                ORDER BY date_added DESC";

                SqlDataAdapter da = new SqlDataAdapter(patientQuery, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPatients.DataSource = dt;
                gvPatients.DataBind();
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string caseQuery = @"
                SELECT case_id,patient_id,case_no,date_of_bite,place_of_bite,
                       type_of_exposure,site_of_bite,category
                FROM [Case]
                ORDER BY date_of_bite DESC";

                SqlDataAdapter da = new SqlDataAdapter(caseQuery, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvCases.DataSource = dt;
                gvCases.DataBind();
            }
        }

        protected void gvPatients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewPatient")
            {
                LoadPatientPreview(e.CommandArgument.ToString());
                hfActivePanel.Value = "viewPatientPanel";
            }
        }

        protected void gvCases_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
            {
                LoadCasePreview(Convert.ToInt32(e.CommandArgument));
                hfActivePanel.Value = "viewPatientPanel";
            }
        }

        private void LoadPatientPreview(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM Patient WHERE patient_id=@id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", patientId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    pnlPreviewEmpty.Visible = false;
                    pnlPatientPreview.Visible = true;
                    pnlCasePreview.Visible = false;

                    lblPatientId.Text = dr["patient_id"].ToString();
                    lblPatientName.Text = dr["fname"] + " " + dr["lname"];
                    lblPatientDOB.Text = dr["date_of_birth"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_of_birth"]).ToString("MMM dd, yyyy");
                    lblPatientGender.Text = dr["gender"].ToString();
                    lblPatientCivilStatus.Text = dr["civil_status"].ToString();
                    lblPatientAddress.Text = dr["address"].ToString();
                    lblPatientContact.Text = dr["contact_no"].ToString();
                    lblPatientOccupation.Text = dr["occupation"].ToString();
                    lblPatientEmergencyPerson.Text = dr["emergency_contact_person"].ToString();
                    lblPatientEmergencyNo.Text = dr["emergency_contact_no"].ToString();
                    lblPatientDateAdded.Text = dr["date_added"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_added"]).ToString("MMM dd, yyyy");
                }
            }
        }

        private void LoadCasePreview(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM [Case] WHERE case_id=@id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", caseId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    pnlPreviewEmpty.Visible = false;
                    pnlPatientPreview.Visible = false;
                    pnlCasePreview.Visible = true;

                    lblCaseId.Text = dr["case_id"].ToString();
                    lblCasePatientId.Text = dr["patient_id"].ToString();
                    lblCaseNo.Text = dr["case_no"].ToString();
                    lblCaseDateOfBite.Text = dr["date_of_bite"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_of_bite"]).ToString("MMM dd, yyyy");
                    lblCaseTimeOfBite.Text = dr["time_of_bite"].ToString();
                    lblCasePlaceOfBite.Text = dr["place_of_bite"].ToString();
                    lblCaseExposureType.Text = dr["type_of_exposure"].ToString();
                    lblCaseWoundType.Text = dr["wound_type"].ToString();
                    lblCaseBleeding.Text = dr["bleeding"].ToString();
                    lblCaseSiteOfBite.Text = dr["site_of_bite"].ToString();
                    lblCaseCategory.Text = dr["category"].ToString();
                    lblCaseWashed.Text = dr["washed"].ToString();
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            DateTime dob;
            DateTime biteDate;

            if (!DateTime.TryParse(txtDOB.Text, out dob))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Invalid Date of Birth');", true);
                return;
            }

            if (!DateTime.TryParse(txtBiteDateTime.Text, out biteDate))
            {
                ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Invalid Bite Date');", true);
                return;
            }

            string fullAddress = txtHouseNo.Text + ", " + txtBarangay.Text + ", " + txtProvinceCity.Text;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    int patientId;

                    // =========================
                    // INSERT PATIENT
                    // =========================
                    string patientQuery = @"
            INSERT INTO Patient
            (fname,lname,date_of_birth,gender,civil_status,address,contact_no,occupation,
             emergency_contact_person,emergency_contact_no)
            VALUES
            (@fname,@lname,@dob,@gender,@civil,@address,@contact,@occupation,
             @eperson,@eno);

            SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdPatient = new SqlCommand(patientQuery, conn, trans);

                    cmdPatient.Parameters.AddWithValue("@fname", txtFirstName.Text);
                    cmdPatient.Parameters.AddWithValue("@lname", txtLastName.Text);
                    cmdPatient.Parameters.AddWithValue("@dob", dob);
                    cmdPatient.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@civil", ddlCivilStatus.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@address", fullAddress);
                    cmdPatient.Parameters.AddWithValue("@contact", txtContactNo.Text);
                    cmdPatient.Parameters.AddWithValue("@occupation", ddlOccupation.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@eperson", txtEmergencyContactPerson.Text);
                    cmdPatient.Parameters.AddWithValue("@eno", txtEmergencyContactNo.Text);

                    patientId = Convert.ToInt32(cmdPatient.ExecuteScalar());

                    // =========================
                    // INSERT VITAL SIGNS
                    // =========================
                    string vitalQuery = @"
            INSERT INTO VitalSigns
            (patient_id, blood_pressure, temperature, cr, wt)
            VALUES
            (@pid, @bp, @temp, @cr, @wt)";

                    SqlCommand cmdVital = new SqlCommand(vitalQuery, conn, trans);

                    cmdVital.Parameters.AddWithValue("@pid", patientId);
                    cmdVital.Parameters.AddWithValue("@bp", txtBloodPressure.Text);
                    cmdVital.Parameters.AddWithValue("@temp", txtTemperature.Text);
                    cmdVital.Parameters.AddWithValue("@cr", txtChiefComplaints.Text);
                    cmdVital.Parameters.AddWithValue("@wt", txtWeight.Text);

                    cmdVital.ExecuteNonQuery();

                    // =========================
                    // INSERT BITE CASE
                    // =========================
                    string caseQuery = @"
            INSERT INTO [Case]
            (patient_id,date_of_bite,place_of_bite,time_of_bite,type_of_exposure,
             wound_type,bleeding,site_of_bite,category)
            VALUES
            (@pid,@date,@place,@time,@exposure,@wound,@bleeding,@site,@category)";

                    SqlCommand cmdCase = new SqlCommand(caseQuery, conn, trans);

                    cmdCase.Parameters.AddWithValue("@pid", patientId);
                    cmdCase.Parameters.AddWithValue("@date", biteDate.Date);
                    cmdCase.Parameters.AddWithValue("@place", txtPlaceExposure.Text);
                    cmdCase.Parameters.AddWithValue("@time", biteDate.TimeOfDay);
                    cmdCase.Parameters.AddWithValue("@exposure", ddlExposureType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@wound", ddlWoundType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@bleeding", ddlBleeding.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@site", txtWoundLocation.Text);
                    cmdCase.Parameters.AddWithValue("@category",
                        rbCategory1.Checked ? "I" :
                        rbCategory2.Checked ? "II" : "III");

                    cmdCase.ExecuteNonQuery();

                    trans.Commit();

                    FBindGrid();
                    ClearFormFields();

                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Patient Registered Successfully');", true);
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('" + ex.Message.Replace("'", "") + "');", true);
                }
            }
        }
        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }   

        private void ClearFormFields()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtDOB.Text = "";
            txtContactNo.Text = "";
            txtHouseNo.Text = "";
            txtSubdivision.Text = "";
            txtBarangay.Text = "";
            txtProvinceCity.Text = "";
            txtEmergencyContactPerson.Text = "";
            txtEmergencyContactNo.Text = "";
            txtBloodPressure.Text = "";
            txtTemperature.Text = "";
            txtWeight.Text = "";
            txtChiefComplaints.Text = "";
            txtBiteDateTime.Text = "";
            txtPlaceExposure.Text = "";
            txtWoundLocation.Text = "";
        }
    }
}