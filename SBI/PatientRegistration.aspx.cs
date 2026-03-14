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
                        bc.category
                    FROM [Case] bc
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

		protected void gvPatients_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "ViewPatient")
			{
				string patientId = e.CommandArgument.ToString();
				LoadPatientPreview(patientId);
				hfActivePanel.Value = "viewPatientPanel";
			}
		}

		protected void gvCases_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "ViewCase")
			{
				int caseId = Convert.ToInt32(e.CommandArgument);
				LoadCasePreview(caseId);
				hfActivePanel.Value = "viewPatientPanel";
			}
		}

		private void LoadPatientPreview(string patientId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT
                        patient_id,
                        fname,
                        lname,
                        date_of_birth,
                        gender,
                        civil_status,
                        address,
                        contact_no,
                        occupation,
                        emergency_contact_person,
                        emergency_contact_no,
                        blood_pressure,
                        temperature,
                        wt,
                        date_added
                    FROM Patient
                    WHERE patient_id = @patient_id";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@patient_id", patientId);
					conn.Open();

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							pnlPreviewEmpty.Visible = false;
							pnlPatientPreview.Visible = true;
							pnlCasePreview.Visible = false;

							lblPatientId.Text = dr["patient_id"].ToString();
							lblPatientName.Text = dr["fname"].ToString() + " " + dr["lname"].ToString();
							lblPatientDOB.Text = dr["date_of_birth"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_of_birth"]).ToString("MMM dd, yyyy");
							lblPatientGender.Text = dr["gender"].ToString();
							lblPatientCivilStatus.Text = dr["civil_status"].ToString();
							lblPatientAddress.Text = dr["address"].ToString();
							lblPatientContact.Text = dr["contact_no"].ToString();
							lblPatientOccupation.Text = dr["occupation"].ToString();
							lblPatientEmergencyPerson.Text = dr["emergency_contact_person"].ToString();
							lblPatientEmergencyNo.Text = dr["emergency_contact_no"].ToString();
							lblPatientBP.Text = dr["blood_pressure"] == DBNull.Value ? "-" : dr["blood_pressure"].ToString();
							lblPatientTemp.Text = dr["temperature"] == DBNull.Value ? "-" : dr["temperature"].ToString();
							lblPatientWeight.Text = dr["wt"] == DBNull.Value ? "-" : dr["wt"].ToString();
							lblPatientDateAdded.Text = dr["date_added"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_added"]).ToString("MMM dd, yyyy");
						}
					}
				}
			}
		}

		private void LoadCasePreview(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT
                        case_id,
                        patient_id,
                        case_no,
                        date_of_bite,
                        time_of_bite,
                        place_of_bite,
                        type_of_exposure,
                        wound_type,
                        bleeding,
                        site_of_bite,
                        category,
                        washed
                    FROM [Case]
                    WHERE case_id = @case_id";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@case_id", caseId);
					conn.Open();

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							pnlPreviewEmpty.Visible = false;
							pnlPatientPreview.Visible = false;
							pnlCasePreview.Visible = true;

							lblCaseId.Text = dr["case_id"].ToString();
							lblCasePatientId.Text = dr["patient_id"].ToString();
							lblCaseNo.Text = dr["case_no"].ToString();
							lblCaseDateOfBite.Text = dr["date_of_bite"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_of_bite"]).ToString("MMM dd, yyyy");
							lblCaseTimeOfBite.Text = dr["time_of_bite"] == DBNull.Value ? "-" : dr["time_of_bite"].ToString();
							lblCasePlaceOfBite.Text = dr["place_of_bite"].ToString();
							lblCaseExposureType.Text = dr["type_of_exposure"].ToString();
							lblCaseWoundType.Text = dr["wound_type"].ToString();
							lblCaseBleeding.Text = dr["bleeding"].ToString();
							lblCaseSiteOfBite.Text = dr["site_of_bite"].ToString();
							lblCaseCategory.Text = dr["category"].ToString();
							lblCaseWashed.Text = dr["washed"] == DBNull.Value ? "-" : dr["washed"].ToString();
						}
					}
				}
			}
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			string patientId = GeneratePatientId();
			string caseNo = GenerateCaseNo();

			string fullAddress = txtHouseNo.Text.Trim();

			if (!string.IsNullOrWhiteSpace(txtSubdivision.Text))
				fullAddress += ", " + txtSubdivision.Text.Trim();

			if (!string.IsNullOrWhiteSpace(txtBarangay.Text))
				fullAddress += ", " + txtBarangay.Text.Trim();

			if (!string.IsNullOrWhiteSpace(txtProvinceCity.Text))
				fullAddress += ", " + txtProvinceCity.Text.Trim();

			string gender = ddlGender.SelectedValue;
			string civilStatus = ddlCivilStatus.SelectedValue;
			string occupation = ddlOccupation.SelectedValue;

			string exposureType = ddlExposureType.SelectedValue;
			string woundType = ddlWoundType.SelectedValue;
			string bleeding = ddlBleeding.SelectedValue;
			string siteOfBite = txtWoundLocation.Text.Trim();

			string category = "";
			if (rbCategory1.Checked) category = "I";
			else if (rbCategory2.Checked) category = "II";
			else if (rbCategory3.Checked) category = "III";

			DateTime dob;
			if (!DateTime.TryParse(txtDOB.Text, out dob))
			{
				ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Invalid Date of Birth.');", true);
				hfActivePanel.Value = "addPatientPanel";
				return;
			}

			DateTime biteDateTime;
			if (!DateTime.TryParse(txtBiteDateTime.Text, out biteDateTime))
			{
				ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Invalid Date and Time of Bite.');", true);
				hfActivePanel.Value = "addPatientPanel";
				return;
			}

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					string patientQuery = @"
                        INSERT INTO Patient
                        (
                            patient_id,
                            fname,
                            lname,
                            date_of_birth,
                            gender,
                            civil_status,
                            address,
                            contact_no,
                            occupation,
                            emergency_contact_person,
                            emergency_contact_no,
                            blood_pressure,
                            temperature,
                            cr,
                            wt,
                            date_added
                        )
                        VALUES
                        (
                            @patient_id,
                            @fname,
                            @lname,
                            @date_of_birth,
                            @gender,
                            @civil_status,
                            @address,
                            @contact_no,
                            @occupation,
                            @emergency_contact_person,
                            @emergency_contact_no,
                            @blood_pressure,
                            @temperature,
                            @cr,
                            @wt,
                            @date_added
                        )";

					using (SqlCommand cmdPatient = new SqlCommand(patientQuery, conn, trans))
					{
						cmdPatient.Parameters.AddWithValue("@patient_id", patientId);
						cmdPatient.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
						cmdPatient.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
						cmdPatient.Parameters.AddWithValue("@date_of_birth", dob.Date);
						cmdPatient.Parameters.AddWithValue("@gender", gender);
						cmdPatient.Parameters.AddWithValue("@civil_status", civilStatus);
						cmdPatient.Parameters.AddWithValue("@address", fullAddress);
						cmdPatient.Parameters.AddWithValue("@contact_no", txtContactNo.Text.Trim());
						cmdPatient.Parameters.AddWithValue("@occupation", occupation);
						cmdPatient.Parameters.AddWithValue("@emergency_contact_person", txtEmergencyContactPerson.Text.Trim());
						cmdPatient.Parameters.AddWithValue("@emergency_contact_no", txtEmergencyContactNo.Text.Trim());

						if (string.IsNullOrWhiteSpace(txtBloodPressure.Text))
							cmdPatient.Parameters.AddWithValue("@blood_pressure", DBNull.Value);
						else
							cmdPatient.Parameters.AddWithValue("@blood_pressure", txtBloodPressure.Text.Trim());

						decimal tempValue;
						if (decimal.TryParse(txtTemperature.Text.Trim(), out tempValue))
							cmdPatient.Parameters.AddWithValue("@temperature", tempValue);
						else
							cmdPatient.Parameters.AddWithValue("@temperature", DBNull.Value);

						if (string.IsNullOrWhiteSpace(txtChiefComplaints.Text))
							cmdPatient.Parameters.AddWithValue("@cr", DBNull.Value);
						else
							cmdPatient.Parameters.AddWithValue("@cr", txtChiefComplaints.Text.Trim());

						decimal weightValue;
						if (decimal.TryParse(txtWeight.Text.Trim(), out weightValue))
							cmdPatient.Parameters.AddWithValue("@wt", weightValue);
						else
							cmdPatient.Parameters.AddWithValue("@wt", DBNull.Value);

						cmdPatient.Parameters.AddWithValue("@date_added", DateTime.Now.Date);

						cmdPatient.ExecuteNonQuery();
					}

					string caseQuery = @"
                        INSERT INTO [Case]
                        (
                            patient_id,
                            case_no,
                            date_of_bite,
                            place_of_bite,
                            time_of_bite,
                            type_of_exposure,
                            wound_type,
                            bleeding,
                            site_of_bite,
                            category
                        )
                        VALUES
                        (
                            @patient_id,
                            @case_no,
                            @date_of_bite,
                            @place_of_bite,
                            @time_of_bite,
                            @type_of_exposure,
                            @wound_type,
                            @bleeding,
                            @site_of_bite,
                            @category
                        )";

					using (SqlCommand cmdCase = new SqlCommand(caseQuery, conn, trans))
					{
						cmdCase.Parameters.AddWithValue("@patient_id", patientId);
						cmdCase.Parameters.AddWithValue("@case_no", caseNo);
						cmdCase.Parameters.AddWithValue("@date_of_bite", biteDateTime.Date);
						cmdCase.Parameters.AddWithValue("@place_of_bite", txtPlaceExposure.Text.Trim());
						cmdCase.Parameters.AddWithValue("@time_of_bite", biteDateTime.TimeOfDay);

						if (string.IsNullOrWhiteSpace(exposureType))
							cmdCase.Parameters.AddWithValue("@type_of_exposure", DBNull.Value);
						else
							cmdCase.Parameters.AddWithValue("@type_of_exposure", exposureType);

						if (string.IsNullOrWhiteSpace(woundType))
							cmdCase.Parameters.AddWithValue("@wound_type", DBNull.Value);
						else
							cmdCase.Parameters.AddWithValue("@wound_type", woundType);

						if (string.IsNullOrWhiteSpace(bleeding))
							cmdCase.Parameters.AddWithValue("@bleeding", DBNull.Value);
						else
							cmdCase.Parameters.AddWithValue("@bleeding", bleeding);

						if (string.IsNullOrWhiteSpace(siteOfBite))
							cmdCase.Parameters.AddWithValue("@site_of_bite", DBNull.Value);
						else
							cmdCase.Parameters.AddWithValue("@site_of_bite", siteOfBite);

						if (string.IsNullOrWhiteSpace(category))
							cmdCase.Parameters.AddWithValue("@category", DBNull.Value);
						else
							cmdCase.Parameters.AddWithValue("@category", category);

						cmdCase.ExecuteNonQuery();
					}

					trans.Commit();

					FBindGrid();
					ClearFormFields();
					hfActivePanel.Value = "viewPatientPanel";

					ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Patient and case record saved successfully.');", true);
				}
				catch (Exception ex)
				{
					trans.Rollback();
					hfActivePanel.Value = "addPatientPanel";
					ClientScript.RegisterStartupScript(this.GetType(), "alert", "alert('Error: " + ex.Message.Replace("'", "") + "');", true);
				}
			}
		}

		protected void btnClear_Click(object sender, EventArgs e)
		{
			ClearFormFields();
			hfActivePanel.Value = "addPatientPanel";
		}

		private string GeneratePatientId()
		{
			return "PAT-" + DateTime.Now.ToString("yyyyMMddHHmmss");
		}

		private string GenerateCaseNo()
		{
			return "CASE-" + DateTime.Now.ToString("yyyyMMddHHmmss");
		}

		private void ClearFormFields()
		{
			txtFirstName.Text = "";
			txtLastName.Text = "";
			txtMiddleName.Text = "";
			txtDOB.Text = "";
			txtAge.Text = "";
			ddlGender.SelectedIndex = 0;
			ddlCivilStatus.SelectedIndex = 0;
			txtContactNo.Text = "";
			txtHouseNo.Text = "";
			txtSubdivision.Text = "";
			txtBarangay.Text = "";
			txtProvinceCity.Text = "";
			ddlOccupation.SelectedIndex = 0;
			txtEmergencyContactPerson.Text = "";
			txtEmergencyContactNo.Text = "";
			txtBloodPressure.Text = "";
			txtTemperature.Text = "";
			txtWeight.Text = "";
			txtChiefComplaints.Text = "";
			txtBiteDateTime.Text = "";
			txtPlaceExposure.Text = "";
			txtOtherAnimal.Text = "";
			ddlExposureType.SelectedIndex = 0;
			txtWoundLocation.Text = "";
			ddlWoundType.SelectedIndex = 0;
			ddlBleeding.SelectedIndex = 0;

			rbDog.Checked = true;
			rbCat.Checked = false;
			rbOtherAnimal.Checked = false;

			rbProvoked.Checked = false;
			rbUnprovoked.Checked = true;

			rbOwned.Checked = false;
			rbStray.Checked = true;
			rbLeashed.Checked = false;

			rbAlive.Checked = true;
			rbSick.Checked = false;
			rbDied.Checked = false;
			rbUnknown.Checked = false;

			rbCategory1.Checked = false;
			rbCategory2.Checked = false;
			rbCategory3.Checked = false;
		}
	}
}