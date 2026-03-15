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
            else
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

        #region Data Binding

        private void BindTodaySchedules()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT s.schedule_id, r.case_id, c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
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
                    SELECT c.case_id, c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           c.category, r.regimen_type, r.total_doses,
                           (SELECT COUNT(*) FROM ScheduledDose sd
                            WHERE sd.regimen_id = r.regimen_id AND sd.status = 'Completed') AS completed_doses
                    FROM [Case] c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN VaccineRegimen r ON c.case_id = r.case_id
                    WHERE (@SearchTerm = ''
                       OR p.fname LIKE '%' + @SearchTerm + '%'
                       OR p.lname LIKE '%' + @SearchTerm + '%'
                       OR c.case_no LIKE '%' + @SearchTerm + '%')
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

                    // Only show Generate panel when no schedule exists yet
                    panelGenerate.Visible = (dt.Rows.Count == 0);
                }
            }
        }

        private void LoadCaseDetails(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT c.case_no, (p.fname + ' ' + p.lname) AS patient_name, c.category
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
                                if (item != null) ddlDoseVaccine.SelectedValue = vaccineId;
                            }

                            txtVaccinatedBy.Text = reader["vaccinated_by"] == DBNull.Value ? "" : reader["vaccinated_by"].ToString();
                            txtDosage.Text = reader["dosage"] == DBNull.Value ? "" : reader["dosage"].ToString();
                            txtRoute.Text = reader["route"] == DBNull.Value ? "" : reader["route"].ToString();
                        }
                    }
                }
            }
        }

        // ── Visit helpers ─────────────────────────────────────────────────────

        /// <summary>
        /// Binds the Visit History grid for the given case.
        /// </summary>
        private void BindVisits(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT visit_id, visit_type, dose_day, visit_date, diagnosis,
                           manifestation_notes, status
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

        /// <summary>
        /// Returns true when at least one Visit row exists for the given case.
        /// Used to gate schedule generation.
        /// </summary>
        private bool CaseHasVisit(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT COUNT(*) FROM Visit WHERE case_id = @CaseId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CaseId", caseId);
                    conn.Open();
                    return (int)cmd.ExecuteScalar() > 0;
                }
            }
        }

        /// <summary>
        /// Pre-fills the Visit form fields from an existing Visit record.
        /// </summary>
        private void LoadVisitForEdit(int visitId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT visit_type, dose_day, visit_date, diagnosis,
                           manifestation_notes, status
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
                            ListItem vtItem = ddlVisitType.Items.FindByValue(reader["visit_type"].ToString());
                            if (vtItem != null) ddlVisitType.SelectedValue = reader["visit_type"].ToString();

                            // Restore the stored dose_day into the hidden field and display literal
                            string storedDay = reader["dose_day"] == DBNull.Value ? "" : reader["dose_day"].ToString();
                            hfVisitDoseDay.Value = storedDay;
                            litDoseDayDisplay.Text = string.IsNullOrEmpty(storedDay) ? "—" : "Day " + storedDay;

                            txtVisitDate.Text = reader["visit_date"] == DBNull.Value ? "" : Convert.ToDateTime(reader["visit_date"]).ToString("yyyy-MM-dd");
                            txtVisitDiagnosis.Text = reader["diagnosis"] == DBNull.Value ? "" : reader["diagnosis"].ToString();
                            txtManifestationNotes.Text = reader["manifestation_notes"] == DBNull.Value ? "" : reader["manifestation_notes"].ToString();

                            ListItem statusItem = ddlVisitStatus.Items.FindByValue(reader["status"].ToString());
                            if (statusItem != null) ddlVisitStatus.SelectedValue = reader["status"].ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resets the Visit form back to "new entry" state.
        /// </summary>
        private void ResetVisitForm()
        {
            hfSelectedVisitId.Value = "";
            hfVisitEditMode.Value = "false";
            litVisitFormTitle.Text = "Record Visit";
            ddlVisitType.SelectedIndex = 0;
            hfVisitDoseDay.Value = "";
            litDoseDayDisplay.Text = "\u2014 select a visit type and date —";
            txtVisitDate.Text = "";
            txtVisitDiagnosis.Text = "";
            txtManifestationNotes.Text = "";
            ddlVisitStatus.SelectedValue = "Completed";
            lblVisitError.Visible = false;
            btnCancelVisitEdit.Visible = false;
        }

        /// <summary>
        /// Computes dose_day by diffing visit_date against the active regimen's Day 0.
        /// Returns null when no regimen exists yet (i.e. Initial Visit).
        /// </summary>
        private int? ComputeDoseDay(int caseId, DateTime visitDate)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT TOP 1 start_date
                    FROM VaccineRegimen
                    WHERE case_id = @CaseId AND status = 'Active'
                    ORDER BY regimen_id DESC";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CaseId", caseId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();

                    if (result == null || result == DBNull.Value)
                        return null; // No regimen yet — Initial Visit scenario

                    DateTime day0 = Convert.ToDateTime(result);
                    int dayDiff = (visitDate.Date - day0.Date).Days;
                    return dayDiff < 0 ? (int?)null : dayDiff;
                }
            }
        }

        /// <summary>
        /// Evaluates whether the Assign Protocol panel should show the
        /// "no visit" warning based on the current visit count.
        /// </summary>
        private void RefreshProtocolGate(int caseId)
        {
            bool hasVisit = CaseHasVisit(caseId);
            panelNoVisitWarning.Visible = !hasVisit;

            // Disable (grey-out) the Generate button when no visit exists
            btnGenerateSchedule.Enabled = hasVisit;
            btnGenerateSchedule.CssClass = hasVisit
                ? "w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                : "w-full bg-slate-300 text-slate-500 py-2.5 rounded-lg font-bold transition text-sm cursor-not-allowed";
        }

        #endregion

        #region Button Event Handlers

        protected void btnRefreshToday_Click(object sender, EventArgs e) => BindTodaySchedules();

        protected void btnSearchCase_Click(object sender, EventArgs e) => BindRegistrySummary(txtSearchCase.Text.Trim());

        protected void btnClearCaseSearch_Click(object sender, EventArgs e)
        {
            txtSearchCase.Text = "";
            BindRegistrySummary();
        }

        protected void btnRefreshSchedule_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
                BindOverallSchedule(Convert.ToInt32(hfSelectedCaseId.Value));
        }

        // ── Visit form Save / Cancel ──────────────────────────────────────────

        protected void btnSaveVisit_Click(object sender, EventArgs e)
        {
            // Basic validation
            if (string.IsNullOrEmpty(ddlVisitType.SelectedValue) || string.IsNullOrEmpty(txtVisitDate.Text))
            {
                lblVisitError.Text = "Visit Type and Visit Date are required.";
                lblVisitError.Visible = true;
                return;
            }

            if (string.IsNullOrEmpty(hfSelectedCaseId.Value))
                return;

            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            bool isEdit = hfVisitEditMode.Value == "true";

            DateTime visitDate;
            if (!DateTime.TryParse(txtVisitDate.Text, out visitDate))
            {
                lblVisitError.Text = "Please enter a valid visit date.";
                lblVisitError.Visible = true;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query;

                if (isEdit)
                {
                    query = @"
                        UPDATE Visit
                        SET visit_type           = @VisitType,
                            dose_day             = @DoseDay,
                            visit_date           = @VisitDate,
                            diagnosis            = @Diagnosis,
                            manifestation_notes  = @ManifestationNotes,
                            status               = @Status
                        WHERE visit_id = @VisitId";
                }
                else
                {
                    query = @"
                        INSERT INTO Visit (case_id, visit_type, dose_day, visit_date,
                                           diagnosis, manifestation_notes, status)
                        VALUES (@CaseId, @VisitType, @DoseDay, @VisitDate,
                                @Diagnosis, @ManifestationNotes, @Status)";
                }

                // Compute dose_day from the active regimen's Day 0 vs. the visit date
                int? computedDoseDay = ComputeDoseDay(caseId, visitDate);

                // Update the display literal so the user sees the computed value before the postback clears it
                hfVisitDoseDay.Value = computedDoseDay.HasValue ? computedDoseDay.Value.ToString() : "";
                litDoseDayDisplay.Text = computedDoseDay.HasValue ? "Day " + computedDoseDay.Value : "— (no active regimen)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@VisitType", ddlVisitType.SelectedValue);
                    cmd.Parameters.AddWithValue("@DoseDay",
                        computedDoseDay.HasValue ? (object)computedDoseDay.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@VisitDate", visitDate);
                    cmd.Parameters.AddWithValue("@Diagnosis",
                        string.IsNullOrEmpty(txtVisitDiagnosis.Text)
                            ? (object)DBNull.Value
                            : txtVisitDiagnosis.Text.Trim());
                    cmd.Parameters.AddWithValue("@ManifestationNotes",
                        string.IsNullOrEmpty(txtManifestationNotes.Text)
                            ? (object)DBNull.Value
                            : txtManifestationNotes.Text.Trim());
                    cmd.Parameters.AddWithValue("@Status", ddlVisitStatus.SelectedValue);

                    if (isEdit)
                        cmd.Parameters.AddWithValue("@VisitId", Convert.ToInt32(hfSelectedVisitId.Value));
                    else
                        cmd.Parameters.AddWithValue("@CaseId", caseId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            ResetVisitForm();
            BindVisits(caseId);
            RefreshProtocolGate(caseId);
        }

        protected void btnCancelVisitEdit_Click(object sender, EventArgs e)
        {
            ResetVisitForm();
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

            // ── Guard: must have at least one visit ───────────────────────────
            if (!CaseHasVisit(caseId))
            {
                panelNoVisitWarning.Visible = true;
                return;
            }

            DateTime day0;
            if (!DateTime.TryParse(txtDay0.Text, out day0))
                return;

            string protocol = ddlProtocol.SelectedValue;
            int[] doseDays;

            switch (protocol)
            {
                case "PEP_ESSEN": doseDays = new[] { 0, 3, 7, 14, 28 }; break;
                case "PEP_ZAGREB": doseDays = new[] { 0, 7, 21 }; break;
                case "PREP": doseDays = new[] { 0, 7, 21 }; break;
                default: return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
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
                        return;
                    }
                }
            }

            BindOverallSchedule(caseId);
        }

        #endregion

        #region GridView Row Commands

        protected void gvTodaySchedules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int caseId = Convert.ToInt32(gvTodaySchedules.DataKeys[rowIndex].Values["case_id"]);

                OpenCase(caseId);
            }
        }

        protected void gvSummary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenCase")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int caseId = Convert.ToInt32(gvSummary.DataKeys[rowIndex].Value);

                OpenCase(caseId);
            }
        }

        /// <summary>
        /// Central helper: loads all panels for a given case and reveals the active-case view.
        /// </summary>
        private void OpenCase(int caseId)
        {
            hfSelectedCaseId.Value = caseId.ToString();

            LoadCaseDetails(caseId);
            BindOverallSchedule(caseId);
            BindVisits(caseId);
            ResetVisitForm();
            RefreshProtocolGate(caseId);
            ShowActiveCaseView();
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
                string updateQuery = @"
                    UPDATE ScheduledDose
                    SET status        = 'Completed',
                        vaccine_id    = @VaccineId,
                        vaccinated_by = @VaccinatedBy,
                        dosage        = @Dosage,
                        route         = @Route
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

            panelAdministration.Visible = false;
            hfSelectedScheduleId.Value = "";
            hfEditMode.Value = "";

            BindOverallSchedule(caseId);
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
                string query = isUpdate
                    ? @"UPDATE AnimalFollowUp
                        SET day14_status  = @Status,
                            followup_date = @Date,
                            notes         = @Notes
                        WHERE followup_id = @FollowUpId"
                    : @"INSERT INTO AnimalFollowUp (animal_id, day14_status, followup_date, notes)
                        VALUES (@AnimalId, @Status, @Date, @Notes)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Status", ddlDay14Status.SelectedValue);
                    cmd.Parameters.AddWithValue("@Date",
                        string.IsNullOrEmpty(txtFollowUpDate.Text)
                            ? (object)DBNull.Value
                            : Convert.ToDateTime(txtFollowUpDate.Text));
                    cmd.Parameters.AddWithValue("@Notes", txtFollowUpNotes.Text.Trim());

                    if (isUpdate)
                        cmd.Parameters.AddWithValue("@FollowUpId", Convert.ToInt32(hfFollowUpId.Value));
                    else
                        cmd.Parameters.AddWithValue("@AnimalId", animalId);

                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
                LoadCaseDetails(Convert.ToInt32(hfSelectedCaseId.Value));
        }

        #endregion
    }
}
