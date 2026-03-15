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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SwitchTab("Today");
                BindTodaySchedules();
                BindVaccineDropdown();
            }
        }

        #region Navigation & Tabs

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
            string activeTab = ViewState["ActiveTab"] as string ?? "Registry";
            SwitchTab(activeTab);

            // Rebind the appropriate list when navigating back
            if (activeTab == "Today")
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

            if (tabName == "Today")
            {
                btnTabToday.CssClass = "bg-[#2563eb] text-white font-medium py-2 px-5 rounded-lg text-sm transition-colors border border-[#2563eb] cursor-pointer font-heading2";
                btnTabRegistry.CssClass = "bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium py-2 px-5 rounded-lg text-sm transition-colors cursor-pointer font-heading2";
            }
            else if (tabName == "Registry")
            {
                btnTabToday.CssClass = "bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium py-2 px-5 rounded-lg text-sm transition-colors cursor-pointer font-heading2";
                btnTabRegistry.CssClass = "bg-[#2563eb] text-white font-medium py-2 px-5 rounded-lg text-sm transition-colors border border-[#2563eb] cursor-pointer font-heading2";
            }
        }

        private void ShowActiveCaseView()
        {
            panelTodaySchedules.Visible = false;
            panelRegistrySearch.Visible = false;
            panelActiveCase.Visible = true;
            panelAdministration.Visible = false;
        }

        #endregion

        #region Data Binding Methods

        private void BindTodaySchedules()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT s.schedule_id, r.case_id, c.case_no, (p.fname + ' ' + p.lname) AS patient_name, 
                           s.dose_number, v.vaccine_name
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
                    SELECT c.case_id, c.case_no, (p.fname + ' ' + p.lname) AS patient_name, c.category, 
                           r.regimen_type, r.total_doses,
                           (SELECT COUNT(*) FROM ScheduledDose sd WHERE sd.regimen_id = r.regimen_id AND sd.status = 'Completed') AS completed_doses
                    FROM [Case] c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN VaccineRegimen r ON c.case_id = r.case_id
                    WHERE (@SearchTerm = '' OR p.fname LIKE '%' + @SearchTerm + '%' OR p.lname LIKE '%' + @SearchTerm + '%' OR c.case_no LIKE '%' + @SearchTerm + '%')
                    ORDER BY c.case_id DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@SearchTerm", searchTerm ?? "");
                    conn.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvSummary.DataSource = dt;
                    gvSummary.DataBind();
                }
            }
        }

        private void BindOverallSchedule(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT s.schedule_id, s.dose_number, s.schedule_date, s.status, v.vaccine_name
                    FROM ScheduledDose s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    LEFT JOIN Vaccine v ON s.vaccine_id = v.vaccine_id
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

                    // Only show the Generate panel if no schedule has been created yet
                    panelGenerate.Visible = (dt.Rows.Count == 0);
                }
            }
        }

        private void LoadCaseDetails(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT c.case_no, (p.fname + ' ' + p.lname) AS patient_name, c.category, c.date_of_bite
                    FROM [Case] c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
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

                            /* UNCOMMENT IF animal_id exists in Case table
                            if (reader["date_of_bite"] != DBNull.Value && reader["animal_id"] != DBNull.Value)
                            {
                                // animal follow up logic here
                            }
                            */
                        }
                    }
                }
            }
        }

        private void BindVaccineDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT vaccine_id, vaccine_name FROM Vaccine WHERE is_active = 'Yes' ORDER BY vaccine_name";
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

        /// <summary>
        /// Loads an existing completed dose record into the administration form for editing.
        /// </summary>
        private void LoadDoseForEdit(int scheduleId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT vaccine_id, vaccinated_by, dosage, route 
                    FROM ScheduledDose 
                    WHERE schedule_id = @ScheduleId";

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

                            txtVaccinatedBy.Text = reader["vaccinated_by"] == DBNull.Value ? "" : reader["vaccinated_by"].ToString();
                            txtDosage.Text = reader["dosage"] == DBNull.Value ? "" : reader["dosage"].ToString();
                            txtRoute.Text = reader["route"] == DBNull.Value ? "" : reader["route"].ToString();
                        }
                    }
                }
            }
        }

        #endregion

        #region Event Handlers (Buttons & Search)

        protected void btnRefreshToday_Click(object sender, EventArgs e)
        {
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
                BindOverallSchedule(Convert.ToInt32(hfSelectedCaseId.Value));
            }
        }

        #endregion

        #region Protocol Generation

        protected void btnGenerateSchedule_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ddlProtocol.SelectedValue) || string.IsNullOrEmpty(txtDay0.Text))
                return;

            if (string.IsNullOrEmpty(hfSelectedCaseId.Value))
                return;

            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            DateTime day0;

            if (!DateTime.TryParse(txtDay0.Text, out day0))
                return;

            string protocol = ddlProtocol.SelectedValue;
            int[] doseDays = null;

            switch (protocol)
            {
                case "PEP_ESSEN": doseDays = new int[] { 0, 3, 7, 14, 28 }; break;
                case "PEP_ZAGREB": doseDays = new int[] { 0, 7, 21 }; break;
                case "PREP": doseDays = new int[] { 0, 7, 21 }; break;
                default: return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Create the Regimen
                        string insertRegimenQuery = @"
                            INSERT INTO VaccineRegimen (case_id, regimen_type, start_date, total_doses, status) 
                            OUTPUT INSERTED.regimen_id
                            VALUES (@CaseId, @Protocol, @StartDate, @TotalDoses, 'Active')";

                        int newRegimenId;

                        using (SqlCommand cmdRegimen = new SqlCommand(insertRegimenQuery, conn, transaction))
                        {
                            cmdRegimen.Parameters.AddWithValue("@CaseId", caseId);
                            cmdRegimen.Parameters.AddWithValue("@Protocol", ddlProtocol.SelectedItem.Text);
                            cmdRegimen.Parameters.AddWithValue("@StartDate", day0);
                            cmdRegimen.Parameters.AddWithValue("@TotalDoses", doseDays.Length);

                            newRegimenId = (int)cmdRegimen.ExecuteScalar();
                        }

                        // 2. Insert Scheduled Doses
                        string insertDoseQuery = @"
                            INSERT INTO ScheduledDose (regimen_id, dose_number, schedule_date, status) 
                            VALUES (@RegimenId, @DoseNumber, @ScheduleDate, 'Pending')";

                        for (int i = 0; i < doseDays.Length; i++)
                        {
                            using (SqlCommand cmdInsert = new SqlCommand(insertDoseQuery, conn, transaction))
                            {
                                cmdInsert.Parameters.AddWithValue("@RegimenId", newRegimenId);
                                cmdInsert.Parameters.AddWithValue("@DoseNumber", i + 1);
                                cmdInsert.Parameters.AddWithValue("@ScheduleDate", day0.AddDays(doseDays[i]));
                                cmdInsert.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine("Schedule generation failed: " + ex.Message);
                        // TODO: Surface this error to the user via a Label or ScriptManager alert
                        return;
                    }
                }
            }

            BindOverallSchedule(caseId);
        }

        #endregion

        #region GridView Events & Administration

        protected void gvTodaySchedules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int caseId = Convert.ToInt32(gvTodaySchedules.DataKeys[rowIndex].Values["case_id"]);

                hfSelectedCaseId.Value = caseId.ToString();

                LoadCaseDetails(caseId);
                BindOverallSchedule(caseId);
                ShowActiveCaseView();
            }
        }

        protected void gvSummary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenCase")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int caseId = Convert.ToInt32(gvSummary.DataKeys[rowIndex].Value);

                hfSelectedCaseId.Value = caseId.ToString();

                LoadCaseDetails(caseId);
                BindOverallSchedule(caseId);
                ShowActiveCaseView();
            }
        }

        protected void gvSchedule_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "AdministerDose")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int scheduleId = Convert.ToInt32(gvSchedule.DataKeys[rowIndex].Value);

                hfSelectedScheduleId.Value = scheduleId.ToString();
                hfEditMode.Value = "false";

                // Clear the form for a fresh administration entry
                ddlDoseVaccine.SelectedIndex = 0;
                txtVaccinatedBy.Text = "";
                txtDosage.Text = "";
                txtRoute.Text = "";

                panelAdministration.Visible = true;
            }
            else if (e.CommandName == "EditDose")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int scheduleId = Convert.ToInt32(gvSchedule.DataKeys[rowIndex].Value);

                hfSelectedScheduleId.Value = scheduleId.ToString();
                hfEditMode.Value = "true";

                // Pre-populate the form with existing dose data
                LoadDoseForEdit(scheduleId);

                panelAdministration.Visible = true;
            }
        }

        protected void btnSaveDose_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfSelectedScheduleId.Value) || string.IsNullOrEmpty(ddlDoseVaccine.SelectedValue))
                return;

            int scheduleId = Convert.ToInt32(hfSelectedScheduleId.Value);
            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            int vaccineId = Convert.ToInt32(ddlDoseVaccine.SelectedValue);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                // Always set status to Completed regardless of edit mode
                // This covers both new administration and corrections to existing records
                string updateQuery = @"
                    UPDATE ScheduledDose 
                    SET status         = 'Completed', 
                        vaccine_id     = @VaccineId,
                        vaccinated_by  = @VaccinatedBy,
                        dosage         = @Dosage,
                        route          = @Route
                    WHERE schedule_id = @ScheduleId";

                using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@VaccineId", vaccineId);
                    cmd.Parameters.AddWithValue("@VaccinatedBy", txtVaccinatedBy.Text.Trim());
                    cmd.Parameters.AddWithValue("@Dosage", txtDosage.Text.Trim());
                    cmd.Parameters.AddWithValue("@Route", txtRoute.Text.Trim());
                    cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Reset state and refresh
            panelAdministration.Visible = false;
            hfSelectedScheduleId.Value = "";
            hfEditMode.Value = "";

            BindOverallSchedule(caseId);

            // Refresh the registry summary so completed dose counts stay accurate
            BindRegistrySummary();
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
                string query;

                if (isUpdate)
                {
                    // UPDATE: @AnimalId is not used in the WHERE clause, so only add relevant params
                    query = @"
                        UPDATE AnimalFollowUp 
                        SET day14_status  = @Status, 
                            followup_date = @Date, 
                            notes         = @Notes
                        WHERE followup_id = @FollowUpId";
                }
                else
                {
                    // INSERT: @FollowUpId is not needed here
                    query = @"
                        INSERT INTO AnimalFollowUp (animal_id, day14_status, followup_date, notes) 
                        VALUES (@AnimalId, @Status, @Date, @Notes)";
                }

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", ddlDay14Status.SelectedValue);
                    cmd.Parameters.AddWithValue("@Date",
                        string.IsNullOrEmpty(txtFollowUpDate.Text)
                            ? (object)DBNull.Value
                            : Convert.ToDateTime(txtFollowUpDate.Text));
                    cmd.Parameters.AddWithValue("@Notes", txtFollowUpNotes.Text.Trim());

                    // Only add the parameter that the chosen query actually uses
                    if (isUpdate)
                        cmd.Parameters.AddWithValue("@FollowUpId", Convert.ToInt32(hfFollowUpId.Value));
                    else
                        cmd.Parameters.AddWithValue("@AnimalId", animalId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            // Refresh the case details panel to reflect the saved follow-up
            if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
                LoadCaseDetails(Convert.ToInt32(hfSelectedCaseId.Value));
        }

        #endregion
    }
}