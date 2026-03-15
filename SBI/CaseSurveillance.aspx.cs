using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBI
{
	public partial class CaseSurveillance : System.Web.UI.Page
	{
		private readonly string connectionString =
			ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

		protected void Page_Load(object sender, EventArgs e)
		{
			if (!IsPostBack)
			{
				BindSummary();
				BindDoseVaccineDropdown();
				txtVaccinatedBy.Text = Session["userName"] != null ? Session["userName"].ToString() : "";
			}
		}

		// ============================================================
		// SUMMARY
		// ============================================================
		private void BindSummary(string search = "")
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT
                        c.case_id,
                        c.case_no,
                        p.fname + ' ' + p.lname AS patient_name,
                        c.category,
                        ISNULL(vr.regimen_type, '-') AS regimen_type,
                        COUNT(sd.schedule_id) AS total_doses,
                        SUM(CASE WHEN sd.status = 'Completed' THEN 1 ELSE 0 END) AS completed_doses,
                        SUM(CASE WHEN sd.status = 'Pending' THEN 1 ELSE 0 END) AS pending_doses,
                        SUM(CASE WHEN sd.status = 'Missed' THEN 1 ELSE 0 END) AS missed_doses
                    FROM [Case] c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN VaccineRegimen vr ON c.case_id = vr.case_id
                    LEFT JOIN ScheduledDose sd ON vr.regimen_id = sd.regimen_id
                    WHERE (@search = ''
                           OR c.case_no LIKE @search
                           OR p.fname LIKE @search
                           OR p.lname LIKE @search
                           OR (p.fname + ' ' + p.lname) LIKE @search)
                    GROUP BY
                        c.case_id, c.case_no, p.fname, p.lname, c.category, vr.regimen_type
                    ORDER BY c.case_id DESC";

				using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
				{
					da.SelectCommand.Parameters.AddWithValue("@search",
						string.IsNullOrWhiteSpace(search) ? "" : "%" + search + "%");

					DataTable dt = new DataTable();
					da.Fill(dt);

					gvSummary.DataSource = dt;
					gvSummary.DataBind();
				}
			}
		}

		protected void btnSearchCase_Click(object sender, EventArgs e)
		{
			BindSummary(txtSearchCase.Text.Trim());
		}

		protected void btnClearCaseSearch_Click(object sender, EventArgs e)
		{
			txtSearchCase.Text = "";
			BindSummary();
		}

		protected void gvSummary_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "OpenCase")
			{
				int rowIndex = Convert.ToInt32(e.CommandArgument);
				int caseId = Convert.ToInt32(gvSummary.DataKeys[rowIndex].Value);

				hfSelectedCaseId.Value = caseId.ToString();
				txtCaseNoDisplay.Text = gvSummary.Rows[rowIndex].Cells[0].Text;

				panelGenerate.Visible = true;
				panelSchedule.Visible = true;
				panelAdministration.Visible = false;

				BindSchedule(caseId);
			}
		}

		// ============================================================
		// GENERATE SCHEDULE
		// ============================================================
		protected void btnGenerateSchedule_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(hfSelectedCaseId.Value) ||
				string.IsNullOrWhiteSpace(ddlProtocol.SelectedValue) ||
				string.IsNullOrWhiteSpace(txtDay0.Text))
			{
				ShowAlert("Please select a case, protocol, and Day 0 date.");
				return;
			}

			int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
			DateTime day0;

			if (!DateTime.TryParse(txtDay0.Text, out day0))
			{
				ShowAlert("Please enter a valid Day 0 date.");
				return;
			}

			string protocol = ddlProtocol.SelectedValue;

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					int regimenId = EnsureRegimen(caseId, protocol, conn, trans);

					DeleteOldPendingSchedule(regimenId, conn, trans);

					List<int> doseDays = GetDoseDays(protocol);
					string recommendedVaccineName = GetRecommendedVaccineName(protocol);

					for (int i = 0; i < doseDays.Count; i++)
					{
						DateTime scheduleDate = day0.AddDays(doseDays[i]);
						var fifo = GetFirstAvailableBatch(recommendedVaccineName, conn, trans);

						object vaccineIdObj = DBNull.Value;
						object batchIdObj = DBNull.Value;

						if (fifo.HasValue)
						{
							vaccineIdObj = fifo.Value.vaccineId;
							batchIdObj = fifo.Value.batchId;
						}

						string insertSchedule = @"
                            INSERT INTO ScheduledDose
                                (regimen_id, dose_number, schedule_date, visit_id, status, vaccine_id, batch_id)
                            VALUES
                                (@regimen_id, @dose_number, @schedule_date, NULL, 'Pending', @vaccine_id, @batch_id)";

						using (SqlCommand cmd = new SqlCommand(insertSchedule, conn, trans))
						{
							cmd.Parameters.AddWithValue("@regimen_id", regimenId);
							cmd.Parameters.AddWithValue("@dose_number", i + 1);
							cmd.Parameters.AddWithValue("@schedule_date", scheduleDate);
							cmd.Parameters.AddWithValue("@vaccine_id", vaccineIdObj);
							cmd.Parameters.AddWithValue("@batch_id", batchIdObj);
							cmd.ExecuteNonQuery();
						}
					}

					trans.Commit();

					BindSummary();
					BindSchedule(caseId);

					ShowAlert("Schedule generated successfully.");
				}
				catch (Exception ex)
				{
					trans.Rollback();
					ShowAlert("Error: " + ex.Message.Replace("'", ""));
				}
			}
		}

		protected void btnCloseGenerate_Click(object sender, EventArgs e)
		{
			panelGenerate.Visible = false;
		}

		private int EnsureRegimen(int caseId, string protocol, SqlConnection conn, SqlTransaction trans)
		{
			string select = @"SELECT TOP 1 regimen_id FROM VaccineRegimen WHERE case_id = @case_id ORDER BY regimen_id DESC";

			using (SqlCommand cmd = new SqlCommand(select, conn, trans))
			{
				cmd.Parameters.AddWithValue("@case_id", caseId);
				object existing = cmd.ExecuteScalar();

				if (existing != null && existing != DBNull.Value)
				{
					string update = @"
                        UPDATE VaccineRegimen
                        SET regimen_type = @regimen_type,
                            start_date = @start_date,
                            total_doses = @total_doses,
                            status = 'Ongoing'
                        WHERE regimen_id = @regimen_id";

					using (SqlCommand updateCmd = new SqlCommand(update, conn, trans))
					{
						updateCmd.Parameters.AddWithValue("@regimen_type", protocol);
						updateCmd.Parameters.AddWithValue("@start_date", DateTime.Parse(txtDay0.Text));
						updateCmd.Parameters.AddWithValue("@total_doses", GetDoseDays(protocol).Count);
						updateCmd.Parameters.AddWithValue("@regimen_id", Convert.ToInt32(existing));
						updateCmd.ExecuteNonQuery();
					}

					return Convert.ToInt32(existing);
				}
			}

			string insert = @"
                INSERT INTO VaccineRegimen
                    (case_id, vaccine_id, regimen_type, start_date, total_doses, status)
                VALUES
                    (@case_id, @vaccine_id, @regimen_type, @start_date, @total_doses, 'Ongoing');
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

			int initialVaccineId = GetAnyActiveVaccineId(conn, trans);

			using (SqlCommand cmd = new SqlCommand(insert, conn, trans))
			{
				cmd.Parameters.AddWithValue("@case_id", caseId);
				cmd.Parameters.AddWithValue("@vaccine_id", initialVaccineId);
				cmd.Parameters.AddWithValue("@regimen_type", protocol);
				cmd.Parameters.AddWithValue("@start_date", DateTime.Parse(txtDay0.Text));
				cmd.Parameters.AddWithValue("@total_doses", GetDoseDays(protocol).Count);

				return Convert.ToInt32(cmd.ExecuteScalar());
			}
		}

		private void DeleteOldPendingSchedule(int regimenId, SqlConnection conn, SqlTransaction trans)
		{
			string delete = @"DELETE FROM ScheduledDose WHERE regimen_id = @regimen_id AND ISNULL(status,'Pending') <> 'Completed'";

			using (SqlCommand cmd = new SqlCommand(delete, conn, trans))
			{
				cmd.Parameters.AddWithValue("@regimen_id", regimenId);
				cmd.ExecuteNonQuery();
			}
		}

		private List<int> GetDoseDays(string protocol)
		{
			switch (protocol)
			{
				case "PEP_ESSEN":
					return new List<int> { 0, 3, 7, 14, 28 };
				case "PEP_ZAGREB":
					return new List<int> { 0, 7, 21 };
				case "PREP":
					return new List<int> { 0, 7, 21 };
				default:
					return new List<int> { 0, 3, 7, 14, 28 };
			}
		}

		private string GetRecommendedVaccineName(string protocol)
		{
			switch (protocol)
			{
				case "PEP_ESSEN":
				case "PEP_ZAGREB":
					return "SPEEDA";
				case "PREP":
					return "ABHAYRAD";
				default:
					return "SPEEDA";
			}
		}

		private int GetAnyActiveVaccineId(SqlConnection conn, SqlTransaction trans)
		{
			string query = @"SELECT TOP 1 vaccine_id FROM Vaccine WHERE is_active = 'Yes' ORDER BY vaccine_id";

			using (SqlCommand cmd = new SqlCommand(query, conn, trans))
			{
				object result = cmd.ExecuteScalar();
				return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 1;
			}
		}

		private (int batchId, int vaccineId)? GetFirstAvailableBatch(string vaccineName, SqlConnection conn, SqlTransaction trans)
		{
			string query = @"
                SELECT TOP 1
                    vb.batch_id,
                    vb.vaccine_id
                FROM VaccineBatch vb
                INNER JOIN Vaccine v ON vb.vaccine_id = v.vaccine_id
                WHERE v.vaccine_name = @vaccine_name
                  AND vb.current_stock > 0
                  AND vb.expiration_date >= CAST(GETDATE() AS DATE)
                ORDER BY vb.expiration_date ASC, vb.date_received ASC, vb.batch_id ASC";

			using (SqlCommand cmd = new SqlCommand(query, conn, trans))
			{
				cmd.Parameters.AddWithValue("@vaccine_name", vaccineName);

				using (SqlDataReader dr = cmd.ExecuteReader())
				{
					if (dr.Read())
					{
						return (Convert.ToInt32(dr["batch_id"]), Convert.ToInt32(dr["vaccine_id"]));
					}
				}
			}

			return null;
		}

		// ============================================================
		// SCHEDULE GRID
		// ============================================================
		private void BindSchedule(int caseId)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT
                        sd.schedule_id,
                        sd.dose_number,
                        sd.schedule_date,
                        sd.status,
                        v.vaccine_name,
                        vb.batch_number,
                        vb.expiration_date,
                        ISNULL(t.administered_by, '') AS vaccinated_by
                    FROM ScheduledDose sd
                    INNER JOIN VaccineRegimen vr ON sd.regimen_id = vr.regimen_id
                    LEFT JOIN Vaccine v ON sd.vaccine_id = v.vaccine_id
                    LEFT JOIN VaccineBatch vb ON sd.batch_id = vb.batch_id
                    LEFT JOIN Treatment t ON sd.visit_id = t.visit_id
                    WHERE vr.case_id = @case_id
                    ORDER BY sd.dose_number";

				using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
				{
					da.SelectCommand.Parameters.AddWithValue("@case_id", caseId);

					DataTable dt = new DataTable();
					da.Fill(dt);

					gvSchedule.DataSource = dt;
					gvSchedule.DataBind();
				}
			}
		}

		protected void btnRefreshSchedule_Click(object sender, EventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(hfSelectedCaseId.Value))
			{
				BindSchedule(Convert.ToInt32(hfSelectedCaseId.Value));
			}
		}

		protected void gvSchedule_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			int rowIndex = Convert.ToInt32(e.CommandArgument);
			int scheduleId = Convert.ToInt32(gvSchedule.DataKeys[rowIndex].Value);

			hfSelectedScheduleId.Value = scheduleId.ToString();

			if (e.CommandName == "AdministerDose")
			{
				hfEditMode.Value = "Add";
				LoadDoseForEdit(scheduleId);
				panelAdministration.Visible = true;
			}
			else if (e.CommandName == "EditDose")
			{
				hfEditMode.Value = "Edit";
				LoadDoseForEdit(scheduleId);
				panelAdministration.Visible = true;
			}
		}

		private void LoadDoseForEdit(int scheduleId)
		{
			BindDoseVaccineDropdown();

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT TOP 1
                        sd.status,
                        v.vaccine_name,
                        t.dosage,
                        t.unit,
                        t.route,
                        t.administered_by
                    FROM ScheduledDose sd
                    LEFT JOIN Vaccine v ON sd.vaccine_id = v.vaccine_id
                    LEFT JOIN Treatment t ON sd.visit_id = t.visit_id
                    WHERE sd.schedule_id = @schedule_id";

				using (SqlCommand cmd = new SqlCommand(query, conn))
				{
					cmd.Parameters.AddWithValue("@schedule_id", scheduleId);
					conn.Open();

					using (SqlDataReader dr = cmd.ExecuteReader())
					{
						if (dr.Read())
						{
							string vaccineName = dr["vaccine_name"] == DBNull.Value ? "" : dr["vaccine_name"].ToString();
							if (ddlDoseVaccine.Items.FindByValue(vaccineName) != null)
								ddlDoseVaccine.SelectedValue = vaccineName;
							else
								ddlDoseVaccine.SelectedIndex = 0;

							txtDosage.Text = dr["dosage"] == DBNull.Value ? "" : dr["dosage"].ToString();
							txtUnit.Text = dr["unit"] == DBNull.Value ? "" : dr["unit"].ToString();
							txtRoute.Text = dr["route"] == DBNull.Value ? "" : dr["route"].ToString();
							txtVaccinatedBy.Text = dr["administered_by"] == DBNull.Value
								? (Session["userName"] != null ? Session["userName"].ToString() : "")
								: dr["administered_by"].ToString();
						}
					}
				}
			}
		}

		private void BindDoseVaccineDropdown()
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT vaccine_name
                    FROM Vaccine
                    WHERE is_active = 'Yes'
                    ORDER BY vaccine_name";

				using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
				{
					DataTable dt = new DataTable();
					da.Fill(dt);

					ddlDoseVaccine.DataSource = dt;
					ddlDoseVaccine.DataTextField = "vaccine_name";
					ddlDoseVaccine.DataValueField = "vaccine_name";
					ddlDoseVaccine.DataBind();
					ddlDoseVaccine.Items.Insert(0, new ListItem("-- Select Vaccine --", ""));
				}
			}
		}

		// ============================================================
		// SAVE DOSE
		// ============================================================
		protected void btnSaveDose_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(hfSelectedScheduleId.Value))
			{
				ShowAlert("Please select a scheduled dose first.");
				return;
			}

			if (string.IsNullOrWhiteSpace(ddlDoseVaccine.SelectedValue) ||
				string.IsNullOrWhiteSpace(txtDosage.Text) ||
				string.IsNullOrWhiteSpace(txtUnit.Text) ||
				string.IsNullOrWhiteSpace(txtRoute.Text) ||
				string.IsNullOrWhiteSpace(txtVaccinatedBy.Text))
			{
				ShowAlert("Please complete all administration fields.");
				return;
			}

			int scheduleId = Convert.ToInt32(hfSelectedScheduleId.Value);
			string chosenVaccineName = ddlDoseVaccine.SelectedValue;

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					int caseId;
					string currentStatus;
					int? oldVisitId = null;
					int? oldBatchId = null;
					int? oldVaccineId = null;

					string getSchedule = @"
                        SELECT
                            vr.case_id,
                            sd.status,
                            sd.visit_id,
                            sd.batch_id,
                            sd.vaccine_id
                        FROM ScheduledDose sd
                        INNER JOIN VaccineRegimen vr ON sd.regimen_id = vr.regimen_id
                        WHERE sd.schedule_id = @schedule_id";

					using (SqlCommand cmd = new SqlCommand(getSchedule, conn, trans))
					{
						cmd.Parameters.AddWithValue("@schedule_id", scheduleId);

						using (SqlDataReader dr = cmd.ExecuteReader())
						{
							if (!dr.Read())
								throw new Exception("The selected schedule was not found.");

							caseId = Convert.ToInt32(dr["case_id"]);
							currentStatus = dr["status"] == DBNull.Value ? "Pending" : dr["status"].ToString();

							if (dr["visit_id"] != DBNull.Value) oldVisitId = Convert.ToInt32(dr["visit_id"]);
							if (dr["batch_id"] != DBNull.Value) oldBatchId = Convert.ToInt32(dr["batch_id"]);
							if (dr["vaccine_id"] != DBNull.Value) oldVaccineId = Convert.ToInt32(dr["vaccine_id"]);
						}
					}

					var newAssignment = GetFirstAvailableBatch(chosenVaccineName, conn, trans);
					if (!newAssignment.HasValue)
						throw new Exception("No available non-expired stock was found for the selected vaccine.");

					int newBatchId = newAssignment.Value.batchId;
					int newVaccineId = newAssignment.Value.vaccineId;

					if (currentStatus == "Completed" && oldBatchId.HasValue && oldBatchId.Value != newBatchId)
					{
						string restoreOld = @"
                            UPDATE VaccineBatch
                            SET current_stock = current_stock + 1
                            WHERE batch_id = @batch_id";

						using (SqlCommand cmd = new SqlCommand(restoreOld, conn, trans))
						{
							cmd.Parameters.AddWithValue("@batch_id", oldBatchId.Value);
							cmd.ExecuteNonQuery();
						}

						string deductNew = @"
                            UPDATE VaccineBatch
                            SET current_stock = current_stock - 1
                            WHERE batch_id = @batch_id AND current_stock > 0";

						using (SqlCommand cmd = new SqlCommand(deductNew, conn, trans))
						{
							cmd.Parameters.AddWithValue("@batch_id", newBatchId);
							int affected = cmd.ExecuteNonQuery();
							if (affected == 0)
								throw new Exception("The selected replacement batch is out of stock.");
						}

						string logRestore = @"
                            INSERT INTO InventoryLog
                                (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                            VALUES
                                (@batch_id, 'Adjust-In', 1, GETDATE(), @updated_by, @reference_id)";

						using (SqlCommand cmd = new SqlCommand(logRestore, conn, trans))
						{
							cmd.Parameters.AddWithValue("@batch_id", oldBatchId.Value);
							cmd.Parameters.AddWithValue("@updated_by", txtVaccinatedBy.Text.Trim());
							cmd.Parameters.AddWithValue("@reference_id", scheduleId);
							cmd.ExecuteNonQuery();
						}

						string logDeduct = @"
                            INSERT INTO InventoryLog
                                (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                            VALUES
                                (@batch_id, 'Adjust-Out', 1, GETDATE(), @updated_by, @reference_id)";

						using (SqlCommand cmd = new SqlCommand(logDeduct, conn, trans))
						{
							cmd.Parameters.AddWithValue("@batch_id", newBatchId);
							cmd.Parameters.AddWithValue("@updated_by", txtVaccinatedBy.Text.Trim());
							cmd.Parameters.AddWithValue("@reference_id", scheduleId);
							cmd.ExecuteNonQuery();
						}
					}

					if (currentStatus != "Completed")
					{
						string stockCheckDeduct = @"
                            UPDATE VaccineBatch
                            SET current_stock = current_stock - 1
                            WHERE batch_id = @batch_id AND current_stock > 0";

						using (SqlCommand cmd = new SqlCommand(stockCheckDeduct, conn, trans))
						{
							cmd.Parameters.AddWithValue("@batch_id", newBatchId);
							int affected = cmd.ExecuteNonQuery();

							if (affected == 0)
								throw new Exception("The assigned batch is out of stock.");
						}
					}

					int visitId;
					if (oldVisitId.HasValue)
					{
						visitId = oldVisitId.Value;
					}
					else
					{
						string insertVisit = @"
                            INSERT INTO [Visit]
                                (case_id, visit_type, dose_day, visit_date, diagnosis, manifestation_notes, status)
                            VALUES
                                (@case_id, 'Dose Administration', NULL, CAST(GETDATE() AS DATE), NULL, NULL, 'Completed');
                            SELECT CAST(SCOPE_IDENTITY() AS INT);";

						using (SqlCommand cmd = new SqlCommand(insertVisit, conn, trans))
						{
							cmd.Parameters.AddWithValue("@case_id", caseId);
							visitId = Convert.ToInt32(cmd.ExecuteScalar());
						}
					}

					bool treatmentExists = false;
					string checkTreatment = @"SELECT COUNT(*) FROM Treatment WHERE visit_id = @visit_id";

					using (SqlCommand cmd = new SqlCommand(checkTreatment, conn, trans))
					{
						cmd.Parameters.AddWithValue("@visit_id", visitId);
						treatmentExists = Convert.ToInt32(cmd.ExecuteScalar()) > 0;
					}

					if (treatmentExists)
					{
						string updateTreatment = @"
                            UPDATE Treatment
                            SET vaccine_id = @vaccine_id,
                                dosage = @dosage,
                                unit = @unit,
                                route = @route,
                                administered_by = @administered_by
                            WHERE visit_id = @visit_id";

						using (SqlCommand cmd = new SqlCommand(updateTreatment, conn, trans))
						{
							cmd.Parameters.AddWithValue("@vaccine_id", newVaccineId);
							cmd.Parameters.AddWithValue("@dosage", txtDosage.Text.Trim());
							cmd.Parameters.AddWithValue("@unit", txtUnit.Text.Trim());
							cmd.Parameters.AddWithValue("@route", txtRoute.Text.Trim());
							cmd.Parameters.AddWithValue("@administered_by", txtVaccinatedBy.Text.Trim());
							cmd.Parameters.AddWithValue("@visit_id", visitId);
							cmd.ExecuteNonQuery();
						}
					}
					else
					{
						string insertTreatment = @"
                            INSERT INTO Treatment
                                (visit_id, vaccine_id, dosage, unit, route, administered_by)
                            VALUES
                                (@visit_id, @vaccine_id, @dosage, @unit, @route, @administered_by)";

						using (SqlCommand cmd = new SqlCommand(insertTreatment, conn, trans))
						{
							cmd.Parameters.AddWithValue("@visit_id", visitId);
							cmd.Parameters.AddWithValue("@vaccine_id", newVaccineId);
							cmd.Parameters.AddWithValue("@dosage", txtDosage.Text.Trim());
							cmd.Parameters.AddWithValue("@unit", txtUnit.Text.Trim());
							cmd.Parameters.AddWithValue("@route", txtRoute.Text.Trim());
							cmd.Parameters.AddWithValue("@administered_by", txtVaccinatedBy.Text.Trim());
							cmd.ExecuteNonQuery();
						}
					}

					string updateSchedule = @"
                        UPDATE ScheduledDose
                        SET status = 'Completed',
                            visit_id = @visit_id,
                            vaccine_id = @vaccine_id,
                            batch_id = @batch_id
                        WHERE schedule_id = @schedule_id";

					using (SqlCommand cmd = new SqlCommand(updateSchedule, conn, trans))
					{
						cmd.Parameters.AddWithValue("@visit_id", visitId);
						cmd.Parameters.AddWithValue("@vaccine_id", newVaccineId);
						cmd.Parameters.AddWithValue("@batch_id", newBatchId);
						cmd.Parameters.AddWithValue("@schedule_id", scheduleId);
						cmd.ExecuteNonQuery();
					}

					if (currentStatus != "Completed")
					{
						string inventoryLog = @"
                            INSERT INTO InventoryLog
                                (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                            VALUES
                                (@batch_id, 'Out', 1, GETDATE(), @updated_by, @reference_id)";

						using (SqlCommand cmd = new SqlCommand(inventoryLog, conn, trans))
						{
							cmd.Parameters.AddWithValue("@batch_id", newBatchId);
							cmd.Parameters.AddWithValue("@updated_by", txtVaccinatedBy.Text.Trim());
							cmd.Parameters.AddWithValue("@reference_id", scheduleId);
							cmd.ExecuteNonQuery();
						}
					}

					trans.Commit();

					panelAdministration.Visible = false;
					ClearDoseFields();
					BindSummary();
					BindSchedule(Convert.ToInt32(hfSelectedCaseId.Value));

					ShowAlert("Dose record saved successfully.");
				}
				catch (Exception ex)
				{
					trans.Rollback();
					ShowAlert("Error: " + ex.Message.Replace("'", ""));
				}
			}
		}

		protected void btnCancelDose_Click(object sender, EventArgs e)
		{
			panelAdministration.Visible = false;
			ClearDoseFields();
		}

		private void ClearDoseFields()
		{
			if (ddlDoseVaccine.Items.Count > 0)
				ddlDoseVaccine.SelectedIndex = 0;

			txtDosage.Text = "";
			txtUnit.Text = "";
			txtRoute.Text = "";
			txtVaccinatedBy.Text = Session["userName"] != null ? Session["userName"].ToString() : "";
		}

		// ============================================================
		// HELPER
		// ============================================================
		private void ShowAlert(string message)
		{
			ClientScript.RegisterStartupScript(
				this.GetType(),
				"alert",
				$"alert('{message.Replace("'", "")}');",
				true
			);
		}
	}
}