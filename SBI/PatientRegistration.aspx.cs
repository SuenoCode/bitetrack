using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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

				ShowEmptyPreview();
				SetFormMode(false);

				hfActivePanel.Value = "addPatientPanel";
				hfSelectedPatientId.Value = "";
				hfSelectedCaseId.Value = "";
				hfEditMode.Value = "";
			}
		}

		private void FBindGrid()
		{
			BindPatients(txtSearchPatient.Text.Trim());
			BindCases(txtSearchCase.Text.Trim());
		}

		private void BindPatients(string searchText = "")
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT patient_id, fname, lname, gender, contact_no, address, date_added
                    FROM Patient
                    WHERE
                        (@search = '' OR
                         CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         fname LIKE '%' + @search + '%' OR
                         lname LIKE '%' + @search + '%' OR
                         (fname + ' ' + lname) LIKE '%' + @search + '%' OR
                         contact_no LIKE '%' + @search + '%' OR
                         address LIKE '%' + @search + '%')
                    ORDER BY date_added DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@search", searchText);

				DataTable dt = new DataTable();
				da.Fill(dt);

				gvPatients.DataSource = dt;
				gvPatients.DataBind();
			}
		}

		private void BindCases(string searchText = "")
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT case_id, patient_id, case_no, date_of_bite, place_of_bite,
                           type_of_exposure, site_of_bite, category
                    FROM [Case]
                    WHERE
                        (@search = '' OR
                         CAST(case_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         ISNULL(case_no, '') LIKE '%' + @search + '%' OR
                         ISNULL(place_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(type_of_exposure, '') LIKE '%' + @search + '%' OR
                         ISNULL(site_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(category, '') LIKE '%' + @search + '%')
                    ORDER BY date_of_bite DESC, case_id DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@search", searchText);

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
				string patientId = e.CommandArgument.ToString();
				LoadPatientPreview(patientId);
				hfActivePanel.Value = "viewPatientPanel";
			}
			else if (e.CommandName == "EditPatient")
			{
				string patientId = e.CommandArgument.ToString();
				BeginEditPatient(patientId);
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
			else if (e.CommandName == "EditCase")
			{
				int caseId = Convert.ToInt32(e.CommandArgument);
				BeginEditCase(caseId);
			}
		}

		private void LoadPatientPreview(string patientId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"SELECT * FROM Patient WHERE patient_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", patientId);

				conn.Open();
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					hfSelectedPatientId.Value = dr["patient_id"].ToString();

					pnlPreviewEmpty.Visible = false;
					pnlPatientPreview.Visible = true;
					pnlCasePreview.Visible = false;

					lblPatientId.Text = dr["patient_id"].ToString();
					lblPatientName.Text = (dr["fname"].ToString() + " " + dr["lname"].ToString()).Trim();
					lblPatientDOB.Text = dr["date_of_birth"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_of_birth"]).ToString("MMM dd, yyyy");
					lblPatientGender.Text = dr["gender"].ToString();
					lblPatientCivilStatus.Text = dr["civil_status"].ToString();
					lblPatientAddress.Text = dr["address"].ToString();
					lblPatientContact.Text = dr["contact_no"].ToString();
					lblPatientOccupation.Text = dr["occupation"].ToString();
					lblPatientEmergencyPerson.Text = dr["emergency_contact_person"].ToString();
					lblPatientEmergencyNo.Text = dr["emergency_contact_no"].ToString();
					lblPatientBP.Text = dr["blood_pressure"].ToString();
					lblPatientTemp.Text = dr["temperature"].ToString();
					lblPatientWeight.Text = dr["wt"].ToString();
					lblPatientDateAdded.Text = dr["date_added"] == DBNull.Value ? "-" : Convert.ToDateTime(dr["date_added"]).ToString("MMM dd, yyyy");
				}
			}
		}

		private void LoadCasePreview(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"SELECT * FROM [Case] WHERE case_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", caseId);

				conn.Open();
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					hfSelectedCaseId.Value = dr["case_id"].ToString();

					pnlPreviewEmpty.Visible = false;
					pnlPatientPreview.Visible = false;
					pnlCasePreview.Visible = true;

					lblCaseId.Text = dr["case_id"].ToString();
					lblCasePatientId.Text = dr["patient_id"].ToString();
					lblCaseNo.Text = dr["case_no"] == DBNull.Value ? "-" : dr["case_no"].ToString();
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

		private void BeginEditPatient(string patientId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"SELECT * FROM Patient WHERE patient_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", patientId);

				conn.Open();
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					hfSelectedPatientId.Value = dr["patient_id"].ToString();
					hfEditMode.Value = "PATIENT";

					txtFirstName.Text = dr["fname"].ToString();
					txtLastName.Text = dr["lname"].ToString();
					txtMiddleName.Text = "";

					if (dr["date_of_birth"] != DBNull.Value)
					{
						DateTime dob = Convert.ToDateTime(dr["date_of_birth"]);
						txtDOB.Text = dob.ToString("yyyy-MM-dd");
						txtAge.Text = ComputeAge(dob).ToString();
					}
					else
					{
						txtDOB.Text = "";
						txtAge.Text = "";
					}

					ddlGender.SelectedValue = SafeDropdownValue(ddlGender, dr["gender"].ToString());
					ddlCivilStatus.SelectedValue = SafeDropdownValue(ddlCivilStatus, dr["civil_status"].ToString());
					txtContactNo.Text = dr["contact_no"].ToString();
					ddlOccupation.SelectedValue = SafeDropdownValue(ddlOccupation, dr["occupation"].ToString());
					txtEmergencyContactPerson.Text = dr["emergency_contact_person"].ToString();
					txtEmergencyContactNo.Text = dr["emergency_contact_no"].ToString();
					txtBloodPressure.Text = dr["blood_pressure"].ToString();
					txtTemperature.Text = dr["temperature"].ToString();
					txtWeight.Text = dr["wt"].ToString();
					txtChiefComplaints.Text = dr["cr"].ToString();

					SplitAddress(dr["address"].ToString());

					SetFormMode(true);
					hfActivePanel.Value = "addPatientPanel";
				}
			}
		}

		private void BeginEditCase(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"SELECT * FROM [Case] WHERE case_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", caseId);

				conn.Open();
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					hfSelectedCaseId.Value = dr["case_id"].ToString();
					hfSelectedPatientId.Value = dr["patient_id"].ToString();
					hfEditMode.Value = "CASE";

					txtPlaceExposure.Text = dr["place_of_bite"].ToString();
					ddlExposureType.SelectedValue = SafeDropdownValue(ddlExposureType, dr["type_of_exposure"].ToString());
					ddlWoundType.SelectedValue = SafeDropdownValue(ddlWoundType, dr["wound_type"].ToString());
					ddlBleeding.SelectedValue = SafeDropdownValue(ddlBleeding, dr["bleeding"].ToString());
					txtWoundLocation.Text = dr["site_of_bite"].ToString();

					string category = dr["category"].ToString().Replace("Category ", "").Trim().ToUpper();

					rbCategory1.Checked = category == "I";
					rbCategory2.Checked = category == "II";
					rbCategory3.Checked = category == "III";

					txtBiteDateTime.Text = FormatDateTimeLocal(dr["date_of_bite"], dr["time_of_bite"]);

					SetFormMode(true);
					hfActivePanel.Value = "addPatientPanel";
				}
			}
		}

		protected void btnPreviewEditPatient_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(hfSelectedPatientId.Value))
			{
				BeginEditPatient(hfSelectedPatientId.Value);
			}
		}

		protected void btnPreviewEditCase_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(hfSelectedCaseId.Value))
			{
				BeginEditCase(Convert.ToInt32(hfSelectedCaseId.Value));
			}
		}

		protected void btnCancelPatientPreview_Click(object sender, EventArgs e)
		{
			ShowEmptyPreview();
			hfSelectedPatientId.Value = "";
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnCancelCasePreview_Click(object sender, EventArgs e)
		{
			ShowEmptyPreview();
			hfSelectedCaseId.Value = "";
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnSearchPatient_Click(object sender, EventArgs e)
		{
			BindPatients(txtSearchPatient.Text.Trim());
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnResetPatientSearch_Click(object sender, EventArgs e)
		{
			txtSearchPatient.Text = "";
			BindPatients();
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnSearchCase_Click(object sender, EventArgs e)
		{
			BindCases(txtSearchCase.Text.Trim());
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnResetCaseSearch_Click(object sender, EventArgs e)
		{
			txtSearchCase.Text = "";
			BindCases();
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			DateTime dob;
			DateTime biteDateTime;

			if (!DateTime.TryParse(txtDOB.Text, out dob))
			{
				ShowAlert("Invalid Date of Birth");
				return;
			}

			if (!DateTime.TryParse(txtBiteDateTime.Text, out biteDateTime))
			{
				ShowAlert("Invalid Bite Date and Time");
				return;
			}

			string fullAddress = BuildFullAddress();

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					int patientId;

					string patientQuery = @"
                        INSERT INTO Patient
                        (fname, lname, date_of_birth, gender, civil_status, address, contact_no, occupation,
                         emergency_contact_person, emergency_contact_no, blood_pressure, temperature, cr, wt)
                        VALUES
                        (@fname, @lname, @dob, @gender, @civil, @address, @contact, @occupation,
                         @eperson, @eno, @bp, @temp, @cr, @wt);

                        SELECT SCOPE_IDENTITY();";

					SqlCommand cmdPatient = new SqlCommand(patientQuery, conn, trans);

					cmdPatient.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@dob", dob);
					cmdPatient.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
					cmdPatient.Parameters.AddWithValue("@civil", ddlCivilStatus.SelectedValue);
					cmdPatient.Parameters.AddWithValue("@address", fullAddress);
					cmdPatient.Parameters.AddWithValue("@contact", txtContactNo.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@occupation", ddlOccupation.SelectedValue);
					cmdPatient.Parameters.AddWithValue("@eperson", txtEmergencyContactPerson.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@eno", txtEmergencyContactNo.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@bp", txtBloodPressure.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@temp", txtTemperature.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@cr", txtChiefComplaints.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@wt", txtWeight.Text.Trim());

					patientId = Convert.ToInt32(cmdPatient.ExecuteScalar());

					string caseQuery = @"
                        INSERT INTO [Case]
                        (patient_id, date_of_bite, place_of_bite, time_of_bite, type_of_exposure,
                         wound_type, bleeding, site_of_bite, category)
                        VALUES
                        (@pid, @date, @place, @time, @exposure, @wound, @bleeding, @site, @category)";

					SqlCommand cmdCase = new SqlCommand(caseQuery, conn, trans);

					cmdCase.Parameters.AddWithValue("@pid", patientId);
					cmdCase.Parameters.AddWithValue("@date", biteDateTime.Date);
					cmdCase.Parameters.AddWithValue("@place", txtPlaceExposure.Text.Trim());
					cmdCase.Parameters.AddWithValue("@time", biteDateTime.TimeOfDay);
					cmdCase.Parameters.AddWithValue("@exposure", ddlExposureType.SelectedValue);
					cmdCase.Parameters.AddWithValue("@wound", ddlWoundType.SelectedValue);
					cmdCase.Parameters.AddWithValue("@bleeding", ddlBleeding.SelectedValue);
					cmdCase.Parameters.AddWithValue("@site", txtWoundLocation.Text.Trim());
					cmdCase.Parameters.AddWithValue("@category", GetSelectedCategory());

					cmdCase.ExecuteNonQuery();

					trans.Commit();

					FBindGrid();
					ClearFormFields();
					SetFormMode(false);

					hfActivePanel.Value = "addPatientPanel";

					ShowAlert("Patient Registered Successfully");
				}
				catch (Exception ex)
				{
					trans.Rollback();
					ShowAlert(ex.Message.Replace("'", ""));
				}
			}
		}

		protected void btnUpdateRecord_Click(object sender, EventArgs e)
		{
			if (hfEditMode.Value == "PATIENT")
			{
				UpdatePatientRecord();
			}
			else if (hfEditMode.Value == "CASE")
			{
				UpdateCaseRecord();
			}
			else
			{
				ShowAlert("No record selected for update.");
			}
		}

		private void UpdatePatientRecord()
		{
			if (string.IsNullOrWhiteSpace(hfSelectedPatientId.Value))
			{
				ShowAlert("No patient selected.");
				return;
			}

			DateTime dob;
			if (!DateTime.TryParse(txtDOB.Text, out dob))
			{
				ShowAlert("Invalid Date of Birth");
				return;
			}

			string fullAddress = BuildFullAddress();

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    UPDATE Patient
                    SET fname = @fname,
                        lname = @lname,
                        date_of_birth = @dob,
                        gender = @gender,
                        civil_status = @civil,
                        address = @address,
                        contact_no = @contact,
                        occupation = @occupation,
                        emergency_contact_person = @eperson,
                        emergency_contact_no = @eno,
                        blood_pressure = @bp,
                        temperature = @temp,
                        cr = @cr,
                        wt = @wt
                    WHERE patient_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);

				cmd.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
				cmd.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
				cmd.Parameters.AddWithValue("@dob", dob);
				cmd.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
				cmd.Parameters.AddWithValue("@civil", ddlCivilStatus.SelectedValue);
				cmd.Parameters.AddWithValue("@address", fullAddress);
				cmd.Parameters.AddWithValue("@contact", txtContactNo.Text.Trim());
				cmd.Parameters.AddWithValue("@occupation", ddlOccupation.SelectedValue);
				cmd.Parameters.AddWithValue("@eperson", txtEmergencyContactPerson.Text.Trim());
				cmd.Parameters.AddWithValue("@eno", txtEmergencyContactNo.Text.Trim());
				cmd.Parameters.AddWithValue("@bp", txtBloodPressure.Text.Trim());
				cmd.Parameters.AddWithValue("@temp", txtTemperature.Text.Trim());
				cmd.Parameters.AddWithValue("@cr", txtChiefComplaints.Text.Trim());
				cmd.Parameters.AddWithValue("@wt", txtWeight.Text.Trim());
				cmd.Parameters.AddWithValue("@id", hfSelectedPatientId.Value);

				conn.Open();
				cmd.ExecuteNonQuery();
			}

			FBindGrid();
			LoadPatientPreview(hfSelectedPatientId.Value);

			ClearFormFields();
			SetFormMode(false);
			hfEditMode.Value = "";
			hfActivePanel.Value = "viewPatientPanel";

			ShowAlert("Patient record updated successfully.");
		}

		private void UpdateCaseRecord()
		{
			if (string.IsNullOrWhiteSpace(hfSelectedCaseId.Value))
			{
				ShowAlert("No case selected.");
				return;
			}

			DateTime biteDateTime;
			if (!DateTime.TryParse(txtBiteDateTime.Text, out biteDateTime))
			{
				ShowAlert("Invalid Bite Date and Time");
				return;
			}

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    UPDATE [Case]
                    SET date_of_bite = @date,
                        place_of_bite = @place,
                        time_of_bite = @time,
                        type_of_exposure = @exposure,
                        wound_type = @wound,
                        bleeding = @bleeding,
                        site_of_bite = @site,
                        category = @category
                    WHERE case_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);

				cmd.Parameters.AddWithValue("@date", biteDateTime.Date);
				cmd.Parameters.AddWithValue("@place", txtPlaceExposure.Text.Trim());
				cmd.Parameters.AddWithValue("@time", biteDateTime.TimeOfDay);
				cmd.Parameters.AddWithValue("@exposure", ddlExposureType.SelectedValue);
				cmd.Parameters.AddWithValue("@wound", ddlWoundType.SelectedValue);
				cmd.Parameters.AddWithValue("@bleeding", ddlBleeding.SelectedValue);
				cmd.Parameters.AddWithValue("@site", txtWoundLocation.Text.Trim());
				cmd.Parameters.AddWithValue("@category", GetSelectedCategory());
				cmd.Parameters.AddWithValue("@id", hfSelectedCaseId.Value);

				conn.Open();
				cmd.ExecuteNonQuery();
			}

			FBindGrid();
			LoadCasePreview(Convert.ToInt32(hfSelectedCaseId.Value));

			ClearFormFields();
			SetFormMode(false);
			hfEditMode.Value = "";
			hfActivePanel.Value = "viewPatientPanel";

			ShowAlert("Case record updated successfully.");
		}

		protected void btnCancelEditForm_Click(object sender, EventArgs e)
		{
			ClearFormFields();
			SetFormMode(false);

			hfEditMode.Value = "";
			hfActivePanel.Value = "viewPatientPanel";

			ShowAlert("Edit cancelled.");
		}

		protected void btnClear_Click(object sender, EventArgs e)
		{
			ClearFormFields();
		}

		private void ClearFormFields()
		{
			txtFirstName.Text = "";
			txtLastName.Text = "";
			txtMiddleName.Text = "";
			txtDOB.Text = "";
			txtAge.Text = "";
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
			txtOtherAnimal.Text = "";

			ddlGender.SelectedIndex = 0;
			ddlCivilStatus.SelectedIndex = 0;
			ddlOccupation.SelectedIndex = 0;
			ddlExposureType.SelectedIndex = 0;
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

			hfSelectedPatientId.Value = "";
			hfSelectedCaseId.Value = "";
		}

		private void ShowEmptyPreview()
		{
			pnlPreviewEmpty.Visible = true;
			pnlPatientPreview.Visible = false;
			pnlCasePreview.Visible = false;
		}

		private void SetFormMode(bool isEdit)
		{
			btnSave.Visible = !isEdit;
			btnUpdateRecord.Visible = isEdit;
			btnCancelEditForm.Visible = isEdit;
		}

		private string BuildFullAddress()
		{
			string[] parts = new string[]
			{
				txtHouseNo.Text.Trim(),
				txtSubdivision.Text.Trim(),
				txtBarangay.Text.Trim(),
				txtProvinceCity.Text.Trim()
			};

			return string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
		}

		private void SplitAddress(string fullAddress)
		{
			txtHouseNo.Text = "";
			txtSubdivision.Text = "";
			txtBarangay.Text = "";
			txtProvinceCity.Text = "";

			if (string.IsNullOrWhiteSpace(fullAddress))
				return;

			string[] parts = fullAddress.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
										.Select(x => x.Trim())
										.ToArray();

			if (parts.Length >= 4)
			{
				txtHouseNo.Text = parts[0];
				txtSubdivision.Text = parts[1];
				txtBarangay.Text = parts[2];
				txtProvinceCity.Text = parts[3];
			}
			else if (parts.Length == 3)
			{
				txtHouseNo.Text = parts[0];
				txtBarangay.Text = parts[1];
				txtProvinceCity.Text = parts[2];
			}
			else if (parts.Length == 2)
			{
				txtBarangay.Text = parts[0];
				txtProvinceCity.Text = parts[1];
			}
			else if (parts.Length == 1)
			{
				txtProvinceCity.Text = parts[0];
			}
		}

		private string GetSelectedCategory()
		{
			if (rbCategory1.Checked) return "I";
			if (rbCategory2.Checked) return "II";
			if (rbCategory3.Checked) return "III";
			return "";
		}

		private string SafeDropdownValue(DropDownList ddl, string value)
		{
			ListItem item = ddl.Items.FindByValue(value);
			return item != null ? value : "";
		}

		private int ComputeAge(DateTime birthDate)
		{
			int age = DateTime.Today.Year - birthDate.Year;
			if (birthDate.Date > DateTime.Today.AddYears(-age))
				age--;
			return age;
		}

		private string FormatDateTimeLocal(object dateObj, object timeObj)
		{
			if (dateObj == DBNull.Value)
				return "";

			DateTime datePart = Convert.ToDateTime(dateObj).Date;
			TimeSpan timePart = TimeSpan.Zero;

			if (timeObj != DBNull.Value)
			{
				if (timeObj is TimeSpan)
					timePart = (TimeSpan)timeObj;
				else
				{
					TimeSpan parsedTime;
					if (TimeSpan.TryParse(timeObj.ToString(), out parsedTime))
						timePart = parsedTime;
				}
			}

			DateTime combined = datePart.Add(timePart);
			return combined.ToString("yyyy-MM-ddTHH:mm");
		}

		private void ShowAlert(string message)
		{
			ClientScript.RegisterStartupScript(
				this.GetType(),
				Guid.NewGuid().ToString(),
				"alert('" + message.Replace("'", "").Replace(Environment.NewLine, " ") + "');",
				true
			);
		}
	}
}