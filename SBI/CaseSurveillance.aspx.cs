using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI.WebControls;

namespace SBI
{
	public partial class CaseSurveillance : System.Web.UI.Page
	{
		string connString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

		private string UserRole => Session["userRole"]?.ToString().ToUpper() ?? "";

		private bool IsAdmin => UserRole == "A";
		private bool IsRoleB => UserRole == "B";
		private bool IsRoleC => UserRole == "C";

		// A = full access
		public bool CanOpenCase => IsAdmin || IsRoleC;
		public bool CanManageCase => IsAdmin || IsRoleB;
		public bool CanSchedule => IsAdmin || IsRoleB;
		public bool CanAdminister => IsAdmin;

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Session["userRole"] == null)
			{
				Response.Redirect("Login.aspx");
				return;
			}

			if (!IsPostBack)
			{
				AutoCancelMissedDoses();
				SwitchTab("Today");
				BindTodaySchedules();
				BindVaccineDropdown();
			}
		}

		private void AutoCancelMissedDoses()
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    UPDATE ScheduledDose
                    SET status = 'Cancelled'
                    WHERE status = 'Pending'
                      AND CAST(schedule_date AS DATE) < CAST(DATEADD(DAY, -3, GETDATE()) AS DATE)";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					conn.Open();
					cmd.ExecuteNonQuery();
				}
			}
		}

		protected void btnTabToday_Click(object sender, EventArgs e)
		{
			SwitchTab("Today");
			BindTodaySchedules();
		}

		protected void btnTabRegistry_Click(object sender, EventArgs e)
		{
			SwitchTab("Registry");
			BindRegistrySummary();
		}

		protected void btnBackToCases_Click(object sender, EventArgs e)
		{
			string tab = ViewState["ActiveTab"] as string ?? "Registry";
			SwitchTab(tab);

			if (tab == "Today")
				BindTodaySchedules();
			else
				BindRegistrySummary();
		}

		private void SwitchTab(string tabName)
		{
			ViewState["ActiveTab"] = tabName;

			panelTodaySchedules.Visible = (tabName == "Today");
			panelRegistrySearch.Visible = (tabName == "Registry");
			panelActiveCase.Visible = false;

			string active = "h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer";
			string inactive = "h-11 rounded-lg bg-white border border-slate-300 px-6 font-bold text-slate-700 hover:bg-slate-50 transition cursor-pointer";

			btnTabToday.CssClass = (tabName == "Today") ? active : inactive;
			btnTabRegistry.CssClass = (tabName == "Registry") ? active : inactive;
		}

		private void ShowActiveCaseView()
		{
			panelTodaySchedules.Visible = false;
			panelRegistrySearch.Visible = false;
			panelActiveCase.Visible = true;
			panelAdministration.Visible = false;
			panelGenerate.Visible = CanSchedule;
		}

		private void BindTodaySchedules()
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT s.schedule_id, r.case_id, c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           s.dose_number, s.schedule_date, v.vaccine_name
                    FROM ScheduledDose s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    INNER JOIN [Case] c ON r.case_id = c.case_id
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN Vaccine v ON s.vaccine_id = v.vaccine_id
                    WHERE s.status = 'Pending'
                      AND CAST(s.schedule_date AS DATE) <= CAST(GETDATE() AS DATE)
                    ORDER BY s.schedule_date ASC";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					conn.Open();
					SqlDataAdapter da = new SqlDataAdapter(cmd);
					DataTable dt = new DataTable();
					da.Fill(dt);

					gvTodaySchedules.DataSource = dt;
					gvTodaySchedules.DataBind();
				}
			}
		}

		private void BindRegistrySummary(string searchTerm = "")
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT c.case_id, c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           c.category, r.regimen_type, r.total_doses,
                           (SELECT COUNT(*)
                            FROM ScheduledDose sd
                            WHERE sd.regimen_id = r.regimen_id
                              AND sd.status = 'Completed') AS completed_doses
                    FROM [Case] c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN VaccineRegimen r ON c.case_id = r.case_id
                    WHERE (@s = ''
                           OR p.fname LIKE '%' + @s + '%'
                           OR p.lname LIKE '%' + @s + '%'
                           OR c.case_no LIKE '%' + @s + '%')
                    ORDER BY c.case_id DESC";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@s", searchTerm ?? "");
					conn.Open();

					SqlDataAdapter da = new SqlDataAdapter(cmd);
					DataTable dt = new DataTable();
					da.Fill(dt);

					gvSummary.DataSource = dt;
					gvSummary.DataBind();
				}
			}
		}

		private void BindVisitHistory(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT visit_id, visit_type, visit_date, dose_day, diagnosis, status
                    FROM Visit
                    WHERE case_id = @CaseId
                    ORDER BY visit_date DESC, visit_id DESC";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@CaseId", caseId);
					conn.Open();

					SqlDataAdapter da = new SqlDataAdapter(cmd);
					DataTable dt = new DataTable();
					da.Fill(dt);

					gvVisits.DataSource = dt;
					gvVisits.DataBind();
				}
			}
		}

		private void BindOverallSchedule(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT s.schedule_id, s.dose_number, s.schedule_date,
                           s.status, v.vaccine_name, t.administered_by
                    FROM ScheduledDose s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    LEFT JOIN Vaccine v ON s.vaccine_id = v.vaccine_id
                    LEFT JOIN Treatment t ON s.visit_id = t.visit_id
                    WHERE r.case_id = @CaseId
                    ORDER BY s.schedule_date ASC";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@CaseId", caseId);
					conn.Open();

					SqlDataAdapter da = new SqlDataAdapter(cmd);
					DataTable dt = new DataTable();
					da.Fill(dt);

					gvSchedule.DataSource = dt;
					gvSchedule.DataBind();

					panelGenerate.Visible = CanSchedule && (dt.Rows.Count == 0);
				}
			}
		}

		private void LoadCaseDetails(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT c.case_no, c.date_of_bite, c.category,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           v.diagnosis AS initial_diagnosis
                    FROM [Case] c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN Visit v ON c.case_id = v.case_id
                                     AND v.visit_type = 'Initial Visit'
                    WHERE c.case_id = @CaseId";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@CaseId", caseId);
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							litCaseNoDisplay.Text = reader["case_no"].ToString();
							litPatientNameDisplay.Text = reader["patient_name"].ToString();
							litCategoryDisplay.Text = reader["category"].ToString();

							litBiteDateDisplay.Text = reader["date_of_bite"] == DBNull.Value
								? "—"
								: Convert.ToDateTime(reader["date_of_bite"]).ToString("MMM dd, yyyy");

							litInitialDiagnosis.Text = reader["initial_diagnosis"] == DBNull.Value
								? "—"
								: reader["initial_diagnosis"].ToString();
						}
					}
				}
			}
		}

		private void BindVaccineDropdown()
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT v.vaccine_id, v.vaccine_name
                    FROM Vaccine v
                    WHERE v.is_active = 'Yes'
                      AND EXISTS (
                          SELECT 1
                          FROM VaccineBatch b
                          WHERE b.vaccine_id = v.vaccine_id
                            AND b.current_stock > 0
                            AND b.expiration_date >= CAST(GETDATE() AS DATE)
                      )
                    ORDER BY v.vaccine_name";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					conn.Open();

					SqlDataAdapter da = new SqlDataAdapter(cmd);
					DataTable dt = new DataTable();
					da.Fill(dt);

					ddlDoseVaccine.DataSource = dt;
					ddlDoseVaccine.DataTextField = "vaccine_name";
					ddlDoseVaccine.DataValueField = "vaccine_id";
					ddlDoseVaccine.DataBind();
				}
			}

			ddlDoseVaccine.Items.Insert(0, new ListItem("-- Select Vaccine --", ""));
		}

		private void LoadDoseForEdit(int scheduleId)
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT t.vaccine_id, t.administered_by, t.dosage, t.route
                    FROM ScheduledDose s
                    INNER JOIN Treatment t ON t.visit_id = s.visit_id
                    WHERE s.schedule_id = @ScheduleId";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							string vaccineId = reader["vaccine_id"] == DBNull.Value ? "" : reader["vaccine_id"].ToString();
							if (!string.IsNullOrEmpty(vaccineId))
							{
								ListItem item = ddlDoseVaccine.Items.FindByValue(vaccineId);
								if (item != null)
									ddlDoseVaccine.SelectedValue = vaccineId;
							}

							txtVaccinatedBy.Text = reader["administered_by"] == DBNull.Value ? "" : reader["administered_by"].ToString();
							txtDosage.Text = reader["dosage"] == DBNull.Value ? "" : reader["dosage"].ToString();
							txtRoute.Text = reader["route"] == DBNull.Value ? "" : reader["route"].ToString();
						}
					}
				}
			}
		}

		private void LoadVisitForEdit(int visitId)
		{
			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = @"
                    SELECT visit_type, visit_date, dose_day, diagnosis, manifestation_notes, status
                    FROM Visit
                    WHERE visit_id = @VisitId";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@VisitId", visitId);
					conn.Open();

					using (SqlDataReader reader = cmd.ExecuteReader())
					{
						if (reader.Read())
						{
							ddlVisitType.SelectedValue = reader["visit_type"] == DBNull.Value ? "" : reader["visit_type"].ToString();
							txtVisitDate.Text = reader["visit_date"] == DBNull.Value
								? ""
								: Convert.ToDateTime(reader["visit_date"]).ToString("yyyy-MM-dd");

							hfVisitDoseDay.Value = reader["dose_day"] == DBNull.Value ? "" : reader["dose_day"].ToString();
							litDoseDayDisplay.Text = string.IsNullOrWhiteSpace(hfVisitDoseDay.Value) ? "—" : hfVisitDoseDay.Value;

							txtVisitDiagnosis.Text = reader["diagnosis"] == DBNull.Value ? "" : reader["diagnosis"].ToString();
							txtManifestationNotes.Text = reader["manifestation_notes"] == DBNull.Value ? "" : reader["manifestation_notes"].ToString();
							ddlVisitStatus.SelectedValue = reader["status"] == DBNull.Value ? "Completed" : reader["status"].ToString();
						}
					}
				}
			}
		}

		private void ResetVisitForm()
		{
			hfSelectedVisitId.Value = "";
			hfVisitEditMode.Value = "";
			ddlVisitType.SelectedIndex = 0;
			txtVisitDate.Text = "";
			txtVisitDiagnosis.Text = "";
			txtManifestationNotes.Text = "";
			ddlVisitStatus.SelectedValue = "Completed";
			hfVisitDoseDay.Value = "";
			litDoseDayDisplay.Text = "— select a visit type and date —";
			litVisitFormTitle.Text = "Record Visit";
			btnCancelVisitEdit.Visible = false;
			lblVisitError.Visible = false;
			lblVisitError.Text = "";
		}

		private string ComputeDoseDayText()
		{
			DateTime biteDate;
			DateTime visitDate;

			if (string.IsNullOrWhiteSpace(txtVisitDate.Text))
				return "";

			if (!DateTime.TryParse(txtVisitDate.Text, out visitDate))
				return "";

			if (!DateTime.TryParse(litBiteDateDisplay.Text, out biteDate))
				return "";

			int diff = (visitDate.Date - biteDate.Date).Days;

			if (diff < 0)
				return "";

			return "Day " + diff;
		}

		private int DeductStock(SqlConnection conn, SqlTransaction trans, int vaccineId, string updatedBy)
		{
			int batchId = -1;

			using (SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 batch_id
                FROM VaccineBatch
                WHERE vaccine_id = @vid
                  AND current_stock > 0
                  AND expiration_date >= CAST(GETDATE() AS DATE)
                ORDER BY expiration_date ASC", conn, trans))
			{
				cmd.Parameters.AddWithValue("@vid", vaccineId);
				object result = cmd.ExecuteScalar();

				if (result == null || result == DBNull.Value)
					return -1;

				batchId = Convert.ToInt32(result);
			}

			using (SqlCommand cmd = new SqlCommand("UPDATE VaccineBatch SET current_stock = current_stock - 1 WHERE batch_id = @bid", conn, trans))
			{
				cmd.Parameters.AddWithValue("@bid", batchId);
				cmd.ExecuteNonQuery();
			}

			using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO InventoryLog (batch_id, transaction_type, quantity, transaction_date, updated_by)
                VALUES (@bid, 'Out', 1, GETDATE(), @user)", conn, trans))
			{
				cmd.Parameters.AddWithValue("@bid", batchId);
				cmd.Parameters.AddWithValue("@user", updatedBy);
				cmd.ExecuteNonQuery();
			}

			return batchId;
		}

		private int GetOrCreateVisitForSchedule(SqlConnection conn, SqlTransaction trans, int scheduleId, int caseId)
		{
			using (SqlCommand cmd = new SqlCommand(
				"SELECT visit_id FROM ScheduledDose WHERE schedule_id = @sid AND visit_id IS NOT NULL",
				conn, trans))
			{
				cmd.Parameters.AddWithValue("@sid", scheduleId);
				object result = cmd.ExecuteScalar();

				if (result != null && result != DBNull.Value)
					return Convert.ToInt32(result);
			}

			using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO Visit (case_id, visit_type, visit_date, status)
                OUTPUT INSERTED.visit_id
                VALUES (@cid, 'Follow-up', CAST(GETDATE() AS DATE), 'Completed')", conn, trans))
			{
				cmd.Parameters.AddWithValue("@cid", caseId);
				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		protected void btnRefreshToday_Click(object sender, EventArgs e)
		{
			AutoCancelMissedDoses();
			BindTodaySchedules();
		}

		protected void btnSearchCase_Click(object sender, EventArgs e)
		{
			BindRegistrySummary(txtSearchCase.Text.Trim());
		}

		protected void btnClearCaseSearch_Click(object sender, EventArgs e)
		{
			txtSearchCase.Text = "";
			BindRegistrySummary();
		}

		protected void btnRefreshSchedule_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
			{
				AutoCancelMissedDoses();
				BindOverallSchedule(Convert.ToInt32(hfSelectedCaseId.Value));
			}
		}

		protected void btnGenerateSchedule_Click(object sender, EventArgs e)
		{
			if (!CanSchedule)
			{
				ShowAlert("You do not have permission to generate schedules.");
				return;
			}

			if (string.IsNullOrEmpty(ddlProtocol.SelectedValue) ||
				string.IsNullOrEmpty(txtDay0.Text) ||
				string.IsNullOrEmpty(hfSelectedCaseId.Value))
				return;

			int caseId = Convert.ToInt32(hfSelectedCaseId.Value);

			DateTime day0;
			if (!DateTime.TryParse(txtDay0.Text, out day0))
				return;

			int[] doseDays;

			switch (ddlProtocol.SelectedValue)
			{
				case "PEP_ESSEN":
					doseDays = new[] { 0, 3, 7, 14, 28 };
					break;
				case "PEP_ZAGREB":
					doseDays = new[] { 0, 7, 21 };
					break;
				case "PREP":
					doseDays = new[] { 0, 7, 21 };
					break;
				default:
					return;
			}

			using (SqlConnection conn = new SqlConnection(connString))
			{
				conn.Open();

				using (SqlTransaction trans = conn.BeginTransaction())
				{
					try
					{
						int newRegimenId;

						using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO VaccineRegimen (case_id, regimen_type, start_date, total_doses, status)
                            OUTPUT INSERTED.regimen_id
                            VALUES (@cid, @proto, @start, @total, 'Active')", conn, trans))
						{
							cmd.Parameters.AddWithValue("@cid", caseId);
							cmd.Parameters.AddWithValue("@proto", ddlProtocol.SelectedItem.Text);
							cmd.Parameters.AddWithValue("@start", day0);
							cmd.Parameters.AddWithValue("@total", doseDays.Length);

							newRegimenId = Convert.ToInt32(cmd.ExecuteScalar());
						}

						for (int i = 0; i < doseDays.Length; i++)
						{
							using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO ScheduledDose (regimen_id, dose_number, schedule_date, status)
                                VALUES (@rid, @dnum, @sdate, 'Pending')", conn, trans))
							{
								cmd.Parameters.AddWithValue("@rid", newRegimenId);
								cmd.Parameters.AddWithValue("@dnum", i + 1);
								cmd.Parameters.AddWithValue("@sdate", day0.AddDays(doseDays[i]));
								cmd.ExecuteNonQuery();
							}
						}

						trans.Commit();
					}
					catch (Exception ex)
					{
						trans.Rollback();
						ShowAlert("Schedule generation failed: " + ex.Message);
						return;
					}
				}
			}

			BindOverallSchedule(caseId);
		}

		protected void btnSaveVisit_Click(object sender, EventArgs e)
		{
			lblVisitError.Visible = false;
			lblVisitError.Text = "";

			if (string.IsNullOrEmpty(hfSelectedCaseId.Value))
			{
				lblVisitError.Text = "No case selected.";
				lblVisitError.Visible = true;
				return;
			}

			if (string.IsNullOrEmpty(ddlVisitType.SelectedValue))
			{
				lblVisitError.Text = "Please select a visit type.";
				lblVisitError.Visible = true;
				return;
			}

			DateTime visitDate;
			if (!DateTime.TryParse(txtVisitDate.Text, out visitDate))
			{
				lblVisitError.Text = "Please enter a valid visit date.";
				lblVisitError.Visible = true;
				return;
			}

			int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
			bool isEdit = hfVisitEditMode.Value == "true";

			string doseDayText = ComputeDoseDayText();
			hfVisitDoseDay.Value = doseDayText;
			litDoseDayDisplay.Text = string.IsNullOrWhiteSpace(doseDayText) ? "—" : doseDayText;

			using (SqlConnection conn = new SqlConnection(connString))
			{
				conn.Open();

				if (isEdit && !string.IsNullOrEmpty(hfSelectedVisitId.Value))
				{
					string updateQuery = @"
                        UPDATE Visit
                        SET visit_type = @visit_type,
                            visit_date = @visit_date,
                            dose_day = @dose_day,
                            diagnosis = @diagnosis,
                            manifestation_notes = @manifestation_notes,
                            status = @status
                        WHERE visit_id = @visit_id";

					using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
					{
						cmd.Parameters.AddWithValue("@visit_type", ddlVisitType.SelectedValue);
						cmd.Parameters.AddWithValue("@visit_date", visitDate);
						cmd.Parameters.AddWithValue("@dose_day", string.IsNullOrWhiteSpace(doseDayText) ? (object)DBNull.Value : doseDayText);
						cmd.Parameters.AddWithValue("@diagnosis", string.IsNullOrWhiteSpace(txtVisitDiagnosis.Text) ? (object)DBNull.Value : txtVisitDiagnosis.Text.Trim());
						cmd.Parameters.AddWithValue("@manifestation_notes", string.IsNullOrWhiteSpace(txtManifestationNotes.Text) ? (object)DBNull.Value : txtManifestationNotes.Text.Trim());
						cmd.Parameters.AddWithValue("@status", ddlVisitStatus.SelectedValue);
						cmd.Parameters.AddWithValue("@visit_id", Convert.ToInt32(hfSelectedVisitId.Value));
						cmd.ExecuteNonQuery();
					}
				}
				else
				{
					string insertQuery = @"
                        INSERT INTO Visit (case_id, visit_type, visit_date, dose_day, diagnosis, manifestation_notes, status)
                        VALUES (@case_id, @visit_type, @visit_date, @dose_day, @diagnosis, @manifestation_notes, @status)";

					using (SqlCommand cmd = new SqlCommand(insertQuery, conn))
					{
						cmd.Parameters.AddWithValue("@case_id", caseId);
						cmd.Parameters.AddWithValue("@visit_type", ddlVisitType.SelectedValue);
						cmd.Parameters.AddWithValue("@visit_date", visitDate);
						cmd.Parameters.AddWithValue("@dose_day", string.IsNullOrWhiteSpace(doseDayText) ? (object)DBNull.Value : doseDayText);
						cmd.Parameters.AddWithValue("@diagnosis", string.IsNullOrWhiteSpace(txtVisitDiagnosis.Text) ? (object)DBNull.Value : txtVisitDiagnosis.Text.Trim());
						cmd.Parameters.AddWithValue("@manifestation_notes", string.IsNullOrWhiteSpace(txtManifestationNotes.Text) ? (object)DBNull.Value : txtManifestationNotes.Text.Trim());
						cmd.Parameters.AddWithValue("@status", ddlVisitStatus.SelectedValue);
						cmd.ExecuteNonQuery();
					}
				}
			}

			ResetVisitForm();
			LoadCaseDetails(caseId);
			BindVisitHistory(caseId);
			BindOverallSchedule(caseId);
			ShowActiveCaseView();
		}

		protected void btnCancelVisitEdit_Click(object sender, EventArgs e)
		{
			ResetVisitForm();

			if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
			{
				int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
				BindVisitHistory(caseId);
				ShowActiveCaseView();
			}
		}

		protected void btnSaveDose_Click(object sender, EventArgs e)
		{
			if (!CanAdminister)
			{
				ShowAlert("You do not have permission to administer doses.");
				return;
			}

			if (string.IsNullOrEmpty(hfSelectedScheduleId.Value) ||
				string.IsNullOrEmpty(ddlDoseVaccine.SelectedValue))
			{
				ShowAlert("Please select a vaccine before confirming.");
				return;
			}

			int scheduleId = Convert.ToInt32(hfSelectedScheduleId.Value);
			int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
			int vaccineId = Convert.ToInt32(ddlDoseVaccine.SelectedValue);
			bool isEdit = hfEditMode.Value == "true";

			string adminBy = !string.IsNullOrWhiteSpace(txtVaccinatedBy.Text)
				? txtVaccinatedBy.Text.Trim()
				: (Session["fullName"]?.ToString() ?? "System");

			using (SqlConnection conn = new SqlConnection(connString))
			{
				conn.Open();

				using (SqlTransaction trans = conn.BeginTransaction())
				{
					try
					{
						int batchId = -1;

						if (!isEdit)
						{
							batchId = DeductStock(conn, trans, vaccineId, adminBy);
							if (batchId == -1)
							{
								trans.Rollback();
								ShowAlert("No available stock for the selected vaccine.");
								return;
							}
						}

						int visitId = GetOrCreateVisitForSchedule(conn, trans, scheduleId, caseId);

						using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE ScheduledDose
                            SET status = 'Completed',
                                vaccine_id = @vid,
                                batch_id = @bid,
                                visit_id = @visitId
                            WHERE schedule_id = @sid", conn, trans))
						{
							cmd.Parameters.AddWithValue("@vid", vaccineId);
							cmd.Parameters.AddWithValue("@bid", isEdit ? (object)DBNull.Value : batchId);
							cmd.Parameters.AddWithValue("@visitId", visitId);
							cmd.Parameters.AddWithValue("@sid", scheduleId);
							cmd.ExecuteNonQuery();
						}

						using (SqlCommand cmd = new SqlCommand(@"
                            IF EXISTS (SELECT 1 FROM Treatment WHERE visit_id = @vid)
                                UPDATE Treatment
                                SET vaccine_id = @vacId,
                                    dosage = @dos,
                                    unit = 'mL',
                                    route = @rt,
                                    administered_by = @ab
                                WHERE visit_id = @vid
                            ELSE
                                INSERT INTO Treatment (visit_id, vaccine_id, dosage, unit, route, administered_by)
                                VALUES (@vid, @vacId, @dos, 'mL', @rt, @ab)", conn, trans))
						{
							cmd.Parameters.AddWithValue("@vid", visitId);
							cmd.Parameters.AddWithValue("@vacId", vaccineId);
							cmd.Parameters.AddWithValue("@dos", string.IsNullOrWhiteSpace(txtDosage.Text) ? (object)DBNull.Value : txtDosage.Text.Trim());
							cmd.Parameters.AddWithValue("@rt", string.IsNullOrWhiteSpace(txtRoute.Text) ? (object)DBNull.Value : txtRoute.Text.Trim());
							cmd.Parameters.AddWithValue("@ab", adminBy);
							cmd.ExecuteNonQuery();
						}

						trans.Commit();
					}
					catch (Exception ex)
					{
						trans.Rollback();
						ShowAlert("Error: " + ex.Message);
						return;
					}
				}
			}

			panelAdministration.Visible = false;
			hfSelectedScheduleId.Value = "";
			hfEditMode.Value = "";

			BindOverallSchedule(caseId);
			BindTodaySchedules();
			BindVisitHistory(caseId);
		}

		protected void btnCancelDose_Click(object sender, EventArgs e)
		{
			panelAdministration.Visible = false;
			hfSelectedScheduleId.Value = "";
			hfEditMode.Value = "";
		}

		protected void btnSaveFollowUp_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrEmpty(hfAnimalId.Value))
				return;

			int animalId = Convert.ToInt32(hfAnimalId.Value);
			bool isUpdate = !string.IsNullOrEmpty(hfFollowUpId.Value);

			using (SqlConnection conn = new SqlConnection(connString))
			{
				string query = isUpdate
					? "UPDATE AnimalFollowUp SET day14_status = @s, followup_date = @d, notes = @n WHERE followup_id = @fid"
					: "INSERT INTO AnimalFollowUp (animal_id, day14_status, followup_date, notes) VALUES (@aid, @s, @d, @n)";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@s", ddlDay14Status.SelectedValue);
					cmd.Parameters.AddWithValue("@d", string.IsNullOrEmpty(txtFollowUpDate.Text)
						? (object)DBNull.Value
						: Convert.ToDateTime(txtFollowUpDate.Text));
					cmd.Parameters.AddWithValue("@n", txtFollowUpNotes.Text.Trim());

					if (isUpdate)
						cmd.Parameters.AddWithValue("@fid", Convert.ToInt32(hfFollowUpId.Value));
					else
						cmd.Parameters.AddWithValue("@aid", animalId);

					conn.Open();
					cmd.ExecuteNonQuery();
				}
			}

			if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
				LoadCaseDetails(Convert.ToInt32(hfSelectedCaseId.Value));
		}

		protected void gvTodaySchedules_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "ViewCase")
			{
				if (!CanOpenCase)
				{
					ShowAlert("You do not have permission to open cases.");
					return;
				}

				int rowIndex = Convert.ToInt32(e.CommandArgument);
				int caseId = Convert.ToInt32(gvTodaySchedules.DataKeys[rowIndex].Values["case_id"]);
				OpenCase(caseId);
			}
		}

		protected void gvSummary_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "OpenCase")
			{
				if (!CanManageCase)
				{
					ShowAlert("You do not have permission to manage cases.");
					return;
				}

				int rowIndex = Convert.ToInt32(e.CommandArgument);
				int caseId = Convert.ToInt32(gvSummary.DataKeys[rowIndex].Value);
				OpenCase(caseId);
			}
		}

		protected void gvVisits_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "EditVisit")
			{
				int rowIndex = Convert.ToInt32(e.CommandArgument);
				int visitId = Convert.ToInt32(gvVisits.DataKeys[rowIndex].Value);

				hfSelectedVisitId.Value = visitId.ToString();
				hfVisitEditMode.Value = "true";
				litVisitFormTitle.Text = "Edit Visit";
				btnCancelVisitEdit.Visible = true;

				LoadVisitForEdit(visitId);
				ShowActiveCaseView();
			}
		}

		private void OpenCase(int caseId)
		{
			hfSelectedCaseId.Value = caseId.ToString();

			LoadCaseDetails(caseId);
			BindVisitHistory(caseId);
			BindOverallSchedule(caseId);
			BindVaccineDropdown();
			ResetVisitForm();
			ShowActiveCaseView();
		}

		protected void gvSchedule_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			int rowIndex = Convert.ToInt32(e.CommandArgument);
			int scheduleId = Convert.ToInt32(gvSchedule.DataKeys[rowIndex].Value);

			if (e.CommandName == "AdministerDose" && CanAdminister)
			{
				hfSelectedScheduleId.Value = scheduleId.ToString();
				hfEditMode.Value = "false";

				BindVaccineDropdown();
				ddlDoseVaccine.SelectedIndex = 0;
				txtVaccinatedBy.Text = "";
				txtDosage.Text = "";
				txtRoute.Text = "";

				panelAdministration.Visible = true;
			}
			else if (e.CommandName == "EditDose" && CanAdminister)
			{
				hfSelectedScheduleId.Value = scheduleId.ToString();
				hfEditMode.Value = "true";

				BindVaccineDropdown();
				LoadDoseForEdit(scheduleId);

				panelAdministration.Visible = true;
			}
		}

		private void ShowAlert(string msg)
		{
			ClientScript.RegisterStartupScript(
				GetType(),
				"alert",
				$"alert('{msg.Replace("'", "\\'")}');",
				true
			);
		}
	}
}