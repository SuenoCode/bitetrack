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
				HideRecordPreview();

				hfActivePanel.Value = "viewPatientPanel";
				hfSelectedPatientId.Value = "";
				hfSelectedCaseId.Value = "";
				hfEditMode.Value = "";
			}
		}

		private void FBindGrid()
		{
			BindPatients(txtSearchPatient.Text.Trim(), ParseNullableDate(txtPatientDateFrom.Text), ParseNullableDate(txtPatientDateTo.Text));
			BindCases(txtSearchCase.Text.Trim(), ParseNullableDate(txtCaseDateFrom.Text), ParseNullableDate(txtCaseDateTo.Text));
		}

		private void BindPatients(string searchText = "", DateTime? fromDate = null, DateTime? toDate = null)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT 
                        patient_id,
                        fname,
                        lname,
                        gender,
                        contact_no,
                        address,
                        date_recorded AS date_added
                    FROM dbo.Patient
                    WHERE
                        (@search = '' OR
                         CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         fname LIKE '%' + @search + '%' OR
                         lname LIKE '%' + @search + '%' OR
                         (fname + ' ' + lname) LIKE '%' + @search + '%' OR
                         contact_no LIKE '%' + @search + '%' OR
                         address LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR CAST(date_recorded AS DATE) >= @fromDate)
                        AND (@toDate IS NULL OR CAST(date_recorded AS DATE) <= @toDate)
                    ORDER BY date_recorded DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@search", searchText);
				da.SelectCommand.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
				da.SelectCommand.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);

				DataTable dt = new DataTable();
				da.Fill(dt);

				gvPatients.DataSource = dt;
				gvPatients.DataBind();
			}
		}

		private void BindCases(string searchText = "", DateTime? fromDate = null, DateTime? toDate = null)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT 
                        case_id,
                        patient_id,
                        case_no,
                        date_of_bite,
                        place_of_bite,
                        type_of_exposure,
                        site_of_bite,
                        category
                    FROM dbo.[Case]
                    WHERE
                        (@search = '' OR
                         CAST(case_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         ISNULL(case_no, '') LIKE '%' + @search + '%' OR
                         ISNULL(place_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(type_of_exposure, '') LIKE '%' + @search + '%' OR
                         ISNULL(site_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(category, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR date_of_bite >= @fromDate)
                        AND (@toDate IS NULL OR date_of_bite <= @toDate)
                    ORDER BY date_of_bite DESC, case_id DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@search", searchText);
				da.SelectCommand.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
				da.SelectCommand.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);

				DataTable dt = new DataTable();
				da.Fill(dt);

				gvCases.DataSource = dt;
				gvCases.DataBind();
			}
		}

		protected void gvPatients_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "EditPatient")
			{
				string patientId = e.CommandArgument.ToString();

				hfSelectedPatientId.Value = patientId;
				hfSelectedCaseId.Value = "";
				hfEditMode.Value = "PATIENT";

				LoadPatientPreview(patientId);
				ShowRecordPreview();
				hfActivePanel.Value = "viewPatientPanel";
			}
		}

		protected void gvCases_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "EditCase")
			{
				int caseId = Convert.ToInt32(e.CommandArgument);

				hfSelectedCaseId.Value = caseId.ToString();
				hfSelectedPatientId.Value = "";
				hfEditMode.Value = "CASE";

				LoadCasePreview(caseId);
				ShowRecordPreview();
				hfActivePanel.Value = "viewPatientPanel";
			}
		}

		private void LoadPatientPreview(string patientId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT 
                        p.patient_id,
                        p.fname,
                        p.lname,
                        p.date_of_birth,
                        p.gender,
                        p.civil_status,
                        p.address,
                        p.contact_no,
                        p.occupation,
                        p.emergency_contact_person,
                        p.emergency_contact_no,
                        p.date_recorded,
                        vs.blood_pressure,
                        vs.temperature,
                        vs.wt
                    FROM dbo.Patient p
                    LEFT JOIN dbo.VitalSigns vs
                        ON p.patient_id = vs.patient_id
                    WHERE p.patient_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", patientId);

				conn.Open();
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					pnlPatientPreview.Visible = true;
					pnlCasePreview.Visible = false;

					txtPreviewPatientId.Text = dr["patient_id"].ToString();
					txtPreviewFirstName.Text = dr["fname"].ToString();
					txtPreviewLastName.Text = dr["lname"].ToString();
					txtPreviewDOB.Text = dr["date_of_birth"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_of_birth"]).ToString("yyyy-MM-dd");
					ddlPreviewGender.SelectedValue = SafeDropdownValue(ddlPreviewGender, dr["gender"].ToString());
					ddlPreviewCivilStatus.SelectedValue = SafeDropdownValue(ddlPreviewCivilStatus, dr["civil_status"].ToString());
					txtPreviewAddress.Text = dr["address"].ToString();
					txtPreviewContactNo.Text = dr["contact_no"].ToString();
					ddlPreviewOccupation.SelectedValue = SafeDropdownValue(ddlPreviewOccupation, dr["occupation"].ToString());
					txtPreviewEmergencyPerson.Text = dr["emergency_contact_person"].ToString();
					txtPreviewEmergencyNo.Text = dr["emergency_contact_no"].ToString();
					txtPreviewBP.Text = dr["blood_pressure"] == DBNull.Value ? "" : dr["blood_pressure"].ToString();
					txtPreviewTemp.Text = dr["temperature"] == DBNull.Value ? "" : dr["temperature"].ToString();
					txtPreviewWeight.Text = dr["wt"] == DBNull.Value ? "" : dr["wt"].ToString();
					txtPreviewCapillaryRefill.Text = "";
					txtPreviewDateAdded.Text = dr["date_recorded"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_recorded"]).ToString("MMM dd, yyyy");
				}
			}
		}

		private void LoadCasePreview(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"SELECT * FROM dbo.[Case] WHERE case_id = @id";

				SqlCommand cmd = new SqlCommand(query, conn);
				cmd.Parameters.AddWithValue("@id", caseId);

				conn.Open();
				SqlDataReader dr = cmd.ExecuteReader();

				if (dr.Read())
				{
					pnlPatientPreview.Visible = false;
					pnlCasePreview.Visible = true;

					txtPreviewCaseId.Text = dr["case_id"].ToString();
					txtPreviewCasePatientId.Text = dr["patient_id"].ToString();
					txtPreviewCaseNo.Text = dr["case_no"] == DBNull.Value ? "" : dr["case_no"].ToString();
					txtPreviewCaseDateOfBite.Text = dr["date_of_bite"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_of_bite"]).ToString("yyyy-MM-dd");

					if (dr["time_of_bite"] == DBNull.Value)
					{
						txtPreviewCaseTimeOfBite.Text = "";
					}
					else
					{
						TimeSpan biteTime;
						if (TimeSpan.TryParse(dr["time_of_bite"].ToString(), out biteTime))
							txtPreviewCaseTimeOfBite.Text = biteTime.ToString(@"hh\:mm");
						else
							txtPreviewCaseTimeOfBite.Text = "";
					}

					txtPreviewCasePlaceOfBite.Text = dr["place_of_bite"] == DBNull.Value ? "" : dr["place_of_bite"].ToString();
					ddlPreviewCaseExposureType.SelectedValue = SafeDropdownValue(ddlPreviewCaseExposureType, dr["type_of_exposure"].ToString());
					ddlPreviewCaseWoundType.SelectedValue = SafeDropdownValue(ddlPreviewCaseWoundType, dr["wound_type"].ToString());
					ddlPreviewCaseBleeding.SelectedValue = SafeDropdownValue(ddlPreviewCaseBleeding, dr["bleeding"].ToString());
					txtPreviewCaseSiteOfBite.Text = dr["site_of_bite"] == DBNull.Value ? "" : dr["site_of_bite"].ToString();
					ddlPreviewCaseCategory.SelectedValue = SafeDropdownValue(ddlPreviewCaseCategory, dr["category"].ToString());
					ddlPreviewCaseWashed.SelectedValue = SafeDropdownValue(ddlPreviewCaseWashed, dr["washed"].ToString());
				}
			}
		}

		protected void btnPreviewUpdatePatient_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtPreviewPatientId.Text))
			{
				ShowAlert("No patient selected.");
				return;
			}

			DateTime dob;
			if (!DateTime.TryParse(txtPreviewDOB.Text, out dob))
			{
				ShowAlert("Invalid Date of Birth.");
				return;
			}

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					string updatePatientQuery = @"
                        UPDATE dbo.Patient
                        SET fname = @fname,
                            lname = @lname,
                            date_of_birth = @dob,
                            gender = @gender,
                            civil_status = @civil_status,
                            address = @address,
                            contact_no = @contact_no,
                            occupation = @occupation,
                            emergency_contact_person = @emergency_contact_person,
                            emergency_contact_no = @emergency_contact_no
                        WHERE patient_id = @patient_id";

					SqlCommand cmdPatient = new SqlCommand(updatePatientQuery, conn, trans);
					cmdPatient.Parameters.AddWithValue("@fname", txtPreviewFirstName.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@lname", txtPreviewLastName.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@dob", dob);
					cmdPatient.Parameters.AddWithValue("@gender", ddlPreviewGender.SelectedValue);
					cmdPatient.Parameters.AddWithValue("@civil_status", ddlPreviewCivilStatus.SelectedValue);
					cmdPatient.Parameters.AddWithValue("@address", txtPreviewAddress.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@contact_no", txtPreviewContactNo.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@occupation", ddlPreviewOccupation.SelectedValue);
					cmdPatient.Parameters.AddWithValue("@emergency_contact_person", txtPreviewEmergencyPerson.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@emergency_contact_no", txtPreviewEmergencyNo.Text.Trim());
					cmdPatient.Parameters.AddWithValue("@patient_id", txtPreviewPatientId.Text.Trim());
					cmdPatient.ExecuteNonQuery();

					string checkVitalQuery = @"SELECT COUNT(*) FROM dbo.VitalSigns WHERE patient_id = @patient_id";
					SqlCommand cmdCheck = new SqlCommand(checkVitalQuery, conn, trans);
					cmdCheck.Parameters.AddWithValue("@patient_id", txtPreviewPatientId.Text.Trim());

					int count = Convert.ToInt32(cmdCheck.ExecuteScalar());

					if (count > 0)
					{
						string updateVitalQuery = @"
                            UPDATE dbo.VitalSigns
                            SET blood_pressure = @blood_pressure,
                                temperature = @temperature,
                                wt = @wt
                            WHERE patient_id = @patient_id";

						SqlCommand cmdVital = new SqlCommand(updateVitalQuery, conn, trans);
						cmdVital.Parameters.AddWithValue("@blood_pressure", txtPreviewBP.Text.Trim());
						cmdVital.Parameters.AddWithValue("@temperature", string.IsNullOrWhiteSpace(txtPreviewTemp.Text) ? (object)DBNull.Value : txtPreviewTemp.Text.Trim());
						cmdVital.Parameters.AddWithValue("@wt", string.IsNullOrWhiteSpace(txtPreviewWeight.Text) ? (object)DBNull.Value : txtPreviewWeight.Text.Trim());
						cmdVital.Parameters.AddWithValue("@patient_id", txtPreviewPatientId.Text.Trim());
						cmdVital.ExecuteNonQuery();
					}
					else
					{
						string insertVitalQuery = @"
                            INSERT INTO dbo.VitalSigns (patient_id, blood_pressure, temperature, wt)
                            VALUES (@patient_id, @blood_pressure, @temperature, @wt)";

						SqlCommand cmdVital = new SqlCommand(insertVitalQuery, conn, trans);
						cmdVital.Parameters.AddWithValue("@patient_id", txtPreviewPatientId.Text.Trim());
						cmdVital.Parameters.AddWithValue("@blood_pressure", txtPreviewBP.Text.Trim());
						cmdVital.Parameters.AddWithValue("@temperature", string.IsNullOrWhiteSpace(txtPreviewTemp.Text) ? (object)DBNull.Value : txtPreviewTemp.Text.Trim());
						cmdVital.Parameters.AddWithValue("@wt", string.IsNullOrWhiteSpace(txtPreviewWeight.Text) ? (object)DBNull.Value : txtPreviewWeight.Text.Trim());
						cmdVital.ExecuteNonQuery();
					}

					trans.Commit();

					FBindGrid();
					LoadPatientPreview(txtPreviewPatientId.Text.Trim());
					ShowAlert("Patient information updated successfully.");
				}
				catch (Exception ex)
				{
					trans.Rollback();
					ShowAlert(ex.Message.Replace("'", ""));
				}
			}
		}

		protected void btnPreviewCancelPatient_Click(object sender, EventArgs e)
		{
			HideRecordPreview();
		}

		protected void btnPreviewUpdateCase_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(txtPreviewCaseId.Text))
			{
				ShowAlert("No case selected.");
				return;
			}

			DateTime biteDate;
			if (!DateTime.TryParse(txtPreviewCaseDateOfBite.Text, out biteDate))
			{
				ShowAlert("Invalid Date of Bite.");
				return;
			}

			object timeValue = DBNull.Value;
			if (!string.IsNullOrWhiteSpace(txtPreviewCaseTimeOfBite.Text))
			{
				TimeSpan biteTime;
				if (!TimeSpan.TryParse(txtPreviewCaseTimeOfBite.Text, out biteTime))
				{
					ShowAlert("Invalid Time of Bite.");
					return;
				}
				timeValue = biteTime;
			}

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    UPDATE dbo.[Case]
                    SET date_of_bite = @date_of_bite,
                        time_of_bite = @time_of_bite,
                        place_of_bite = @place_of_bite,
                        type_of_exposure = @type_of_exposure,
                        wound_type = @wound_type,
                        bleeding = @bleeding,
                        site_of_bite = @site_of_bite,
                        category = @category,
                        washed = @washed
                    WHERE case_id = @case_id";

				SqlCommand cmd = new SqlCommand(query, conn);

				cmd.Parameters.AddWithValue("@date_of_bite", biteDate);
				cmd.Parameters.AddWithValue("@time_of_bite", timeValue);
				cmd.Parameters.AddWithValue("@place_of_bite", txtPreviewCasePlaceOfBite.Text.Trim());
				cmd.Parameters.AddWithValue("@type_of_exposure", ddlPreviewCaseExposureType.SelectedValue);
				cmd.Parameters.AddWithValue("@wound_type", ddlPreviewCaseWoundType.SelectedValue);
				cmd.Parameters.AddWithValue("@bleeding", ddlPreviewCaseBleeding.SelectedValue);
				cmd.Parameters.AddWithValue("@site_of_bite", txtPreviewCaseSiteOfBite.Text.Trim());
				cmd.Parameters.AddWithValue("@category", ddlPreviewCaseCategory.SelectedValue);
				cmd.Parameters.AddWithValue("@washed", ddlPreviewCaseWashed.SelectedValue);
				cmd.Parameters.AddWithValue("@case_id", txtPreviewCaseId.Text.Trim());

				conn.Open();
				cmd.ExecuteNonQuery();
			}

			FBindGrid();
			LoadCasePreview(Convert.ToInt32(txtPreviewCaseId.Text.Trim()));
			ShowAlert("Case information updated successfully.");
		}

		protected void btnPreviewCancelCase_Click(object sender, EventArgs e)
		{
			HideRecordPreview();
		}

		protected void btnSearchPatient_Click(object sender, EventArgs e)
		{
			BindPatients(
				txtSearchPatient.Text.Trim(),
				ParseNullableDate(txtPatientDateFrom.Text),
				ParseNullableDate(txtPatientDateTo.Text)
			);
			HideRecordPreview();
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnResetPatientSearch_Click(object sender, EventArgs e)
		{
			txtSearchPatient.Text = "";
			txtPatientDateFrom.Text = "";
			txtPatientDateTo.Text = "";
			BindPatients();
			HideRecordPreview();
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnSearchCase_Click(object sender, EventArgs e)
		{
			BindCases(
				txtSearchCase.Text.Trim(),
				ParseNullableDate(txtCaseDateFrom.Text),
				ParseNullableDate(txtCaseDateTo.Text)
			);
			HideRecordPreview();
			hfActivePanel.Value = "viewPatientPanel";
		}

		protected void btnResetCaseSearch_Click(object sender, EventArgs e)
		{
			txtSearchCase.Text = "";
			txtCaseDateFrom.Text = "";
			txtCaseDateTo.Text = "";
			BindCases();
			HideRecordPreview();
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
					string newPatientId = GenerateNextPatientId(conn, trans);

					string patientQuery = @"
                        INSERT INTO dbo.Patient
                        (patient_id, fname, lname, date_of_birth, gender, civil_status, address, contact_no, occupation,
                         emergency_contact_person, emergency_contact_no, date_recorded)
                        VALUES
                        (@patient_id, @fname, @lname, @dob, @gender, @civil, @address, @contact, @occupation,
                         @eperson, @eno, GETDATE())";

					SqlCommand cmdPatient = new SqlCommand(patientQuery, conn, trans);

					cmdPatient.Parameters.AddWithValue("@patient_id", newPatientId);
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

					cmdPatient.ExecuteNonQuery();

					if (!string.IsNullOrWhiteSpace(txtBloodPressure.Text) ||
						!string.IsNullOrWhiteSpace(txtTemperature.Text) ||
						!string.IsNullOrWhiteSpace(txtWeight.Text))
					{
						string vitalQuery = @"
                            INSERT INTO dbo.VitalSigns
                            (patient_id, blood_pressure, temperature, wt)
                            VALUES
                            (@patient_id, @bp, @temp, @wt)";

						SqlCommand cmdVital = new SqlCommand(vitalQuery, conn, trans);
						cmdVital.Parameters.AddWithValue("@patient_id", newPatientId);
						cmdVital.Parameters.AddWithValue("@bp", txtBloodPressure.Text.Trim());
						cmdVital.Parameters.AddWithValue("@temp", string.IsNullOrWhiteSpace(txtTemperature.Text) ? (object)DBNull.Value : txtTemperature.Text.Trim());
						cmdVital.Parameters.AddWithValue("@wt", string.IsNullOrWhiteSpace(txtWeight.Text) ? (object)DBNull.Value : txtWeight.Text.Trim());
						cmdVital.ExecuteNonQuery();
					}

					string caseQuery = @"
                        INSERT INTO dbo.[Case]
                        (patient_id, date_of_bite, place_of_bite, time_of_bite, type_of_exposure,
                         wound_type, bleeding, site_of_bite, category)
                        VALUES
                        (@pid, @date, @place, @time, @exposure, @wound, @bleeding, @site, @category)";

					SqlCommand cmdCase = new SqlCommand(caseQuery, conn, trans);

					cmdCase.Parameters.AddWithValue("@pid", newPatientId);
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
					HideRecordPreview();
					hfActivePanel.Value = "viewPatientPanel";

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
			ShowAlert("Use the Update button inside the Record Preview.");
		}

		protected void btnCancelEditForm_Click(object sender, EventArgs e)
		{
			ClearFormFields();
			hfEditMode.Value = "";
			hfActivePanel.Value = "viewPatientPanel";
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
			txtCapillaryRefill.Text = "";

			txtBiteDateTime.Text = "";
			txtPlaceExposure.Text = "";
			txtWoundLocation.Text = "";
		

			ddlGender.SelectedIndex = 0;
			ddlCivilStatus.SelectedIndex = 0;
			ddlOccupation.SelectedIndex = 0;
			ddlExposureType.SelectedIndex = 0;
			ddlWoundType.SelectedIndex = 0;
			ddlBleeding.SelectedIndex = 0;

			rbCategory1.Checked = false;
			rbCategory2.Checked = false;
			rbCategory3.Checked = false;
		}

		private void ShowRecordPreview()
		{
			pnlRecordPreviewContainer.Visible = true;
		}

		private void HideRecordPreview()
		{
			pnlRecordPreviewContainer.Visible = false;
			pnlPatientPreview.Visible = false;
			pnlCasePreview.Visible = false;

			hfSelectedPatientId.Value = "";
			hfSelectedCaseId.Value = "";
			hfEditMode.Value = "";
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

		private DateTime? ParseNullableDate(string value)
		{
			DateTime parsedDate;
			if (DateTime.TryParse(value, out parsedDate))
				return parsedDate.Date;

			return null;
		}

		private string GenerateNextPatientId(SqlConnection conn, SqlTransaction trans)
		{
			string yearPrefix = DateTime.Now.Year.ToString();

			string query = @"
                SELECT TOP 1 patient_id
                FROM dbo.Patient
                WHERE patient_id LIKE @prefix
                ORDER BY patient_id DESC";

			SqlCommand cmd = new SqlCommand(query, conn, trans);
			cmd.Parameters.AddWithValue("@prefix", yearPrefix + "-%");

			object result = cmd.ExecuteScalar();

			if (result == null || result == DBNull.Value)
				return yearPrefix + "-0001";

			string lastId = result.ToString();
			string[] parts = lastId.Split('-');

			int nextNumber = 1;
			if (parts.Length == 2)
			{
				int currentNumber;
				if (int.TryParse(parts[1], out currentNumber))
					nextNumber = currentNumber + 1;
			}

			return yearPrefix + "-" + nextNumber.ToString("D4");
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