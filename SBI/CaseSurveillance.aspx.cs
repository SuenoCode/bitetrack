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
        private bool IsAdminAssist => UserRole == "B";
        private bool IsVaccinator => UserRole == "C";

        // B can assign protocol / generate schedule — A and B
        public bool CanSchedule => IsAdmin || IsAdminAssist;
        // A and C can administer doses
        public bool CanAdminister => IsAdmin || IsVaccinator;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            { Response.Redirect("Login.aspx"); return; }

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
                    SET    status = 'Cancelled'
                    WHERE  status = 'Pending'
                      AND  CAST(schedule_date AS DATE) < CAST(DATEADD(DAY,-3,GETDATE()) AS DATE)";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                { conn.Open(); cmd.ExecuteNonQuery(); }
            }
        }

        // ── TAB / NAVIGATION ──────────────────────────────────────

        protected void btnTabToday_Click(object sender, EventArgs e) { SwitchTab("Today"); BindTodaySchedules(); }
        protected void btnTabRegistry_Click(object sender, EventArgs e) { SwitchTab("Registry"); BindRegistrySummary(); }

        protected void btnBackToCases_Click(object sender, EventArgs e)
        {
            string tab = ViewState["ActiveTab"] as string ?? "Registry";
            SwitchTab(tab);
            if (tab == "Today") BindTodaySchedules(); else BindRegistrySummary();
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
            // Assign Protocol panel visibility controlled per role
            panelGenerate.Visible = CanSchedule;
        }

        // ── DATA BINDING ──────────────────────────────────────────

        private void BindTodaySchedules()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT s.schedule_id, r.case_id, c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           s.dose_number, s.schedule_date, v.vaccine_name
                    FROM   ScheduledDose  s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    INNER JOIN [Case]         c ON r.case_id    = c.case_id
                    INNER JOIN Patient        p ON c.patient_id = p.patient_id
                    LEFT  JOIN Vaccine        v ON s.vaccine_id = v.vaccine_id
                    WHERE  s.status = 'Pending'
                      AND  CAST(s.schedule_date AS DATE) <= CAST(GETDATE() AS DATE)
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
                            WHERE sd.regimen_id = r.regimen_id
                              AND sd.status = 'Completed') AS completed_doses
                    FROM   [Case]   c
                    INNER JOIN Patient        p ON c.patient_id = p.patient_id
                    LEFT  JOIN VaccineRegimen r ON c.case_id    = r.case_id
                    WHERE  (@s='' OR p.fname   LIKE '%'+@s+'%'
                                  OR p.lname   LIKE '%'+@s+'%'
                                  OR c.case_no LIKE '%'+@s+'%')
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

        private void BindOverallSchedule(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT s.schedule_id, s.dose_number, s.schedule_date,
                           s.status, v.vaccine_name, t.administered_by
                    FROM   ScheduledDose  s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    LEFT  JOIN Vaccine        v ON s.vaccine_id = v.vaccine_id
                    LEFT  JOIN Treatment      t ON s.visit_id   = t.visit_id
                    WHERE  r.case_id = @CaseId
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
                    // Show Assign Protocol only for A and B, only when no schedule yet
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
                    FROM   [Case]   c
                    INNER JOIN Patient p ON c.patient_id = p.patient_id
                    LEFT  JOIN Visit   v ON c.case_id    = v.case_id
                                       AND v.visit_type  = 'Initial Visit'
                    WHERE  c.case_id = @CaseId";

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
                            litBiteDateDisplay.Text = reader["date_of_bite"] == DBNull.Value ? "—"
                                : Convert.ToDateTime(reader["date_of_bite"]).ToString("MMM dd, yyyy");
                            litInitialDiagnosis.Text = reader["initial_diagnosis"] == DBNull.Value ? "—"
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
                    FROM   Vaccine v
                    WHERE  v.is_active = 'Yes'
                      AND  EXISTS (
                               SELECT 1 FROM VaccineBatch b
                               WHERE  b.vaccine_id      = v.vaccine_id
                                 AND  b.current_stock   > 0
                                 AND  b.expiration_date >= CAST(GETDATE() AS DATE))
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

        // ── DOSE ADMINISTRATION ───────────────────────────────────

        private void LoadDoseForEdit(int scheduleId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT t.vaccine_id, t.administered_by, t.dosage, t.route
                    FROM   ScheduledDose s
                    INNER JOIN Treatment t ON t.visit_id = s.visit_id
                    WHERE  s.schedule_id = @ScheduleId";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@ScheduleId", scheduleId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string vid = reader["vaccine_id"] == DBNull.Value ? "" : reader["vaccine_id"].ToString();
                            if (!string.IsNullOrEmpty(vid))
                            { ListItem item = ddlDoseVaccine.Items.FindByValue(vid); if (item != null) ddlDoseVaccine.SelectedValue = vid; }
                            txtVaccinatedBy.Text = reader["administered_by"] == DBNull.Value ? "" : reader["administered_by"].ToString();
                            txtDosage.Text = reader["dosage"] == DBNull.Value ? "" : reader["dosage"].ToString();
                            txtRoute.Text = reader["route"] == DBNull.Value ? "" : reader["route"].ToString();
                        }
                    }
                }
            }
        }

        private int DeductStock(SqlConnection conn, SqlTransaction trans, int vaccineId, string updatedBy)
        {
            int batchId = -1;
            using (SqlCommand cmd = new SqlCommand(@"
                SELECT TOP 1 batch_id FROM VaccineBatch
                WHERE vaccine_id=@vid AND current_stock>0
                  AND expiration_date >= CAST(GETDATE() AS DATE)
                ORDER BY expiration_date ASC", conn, trans))
            {
                cmd.Parameters.AddWithValue("@vid", vaccineId);
                object r = cmd.ExecuteScalar();
                if (r == null || r == DBNull.Value) return -1;
                batchId = Convert.ToInt32(r);
            }

            new SqlCommand("UPDATE VaccineBatch SET current_stock=current_stock-1 WHERE batch_id=@bid", conn, trans)
            { Parameters = { new SqlParameter("@bid", batchId) } }.ExecuteNonQuery();

            new SqlCommand(@"
                INSERT INTO InventoryLog (batch_id,transaction_type,quantity,transaction_date,updated_by)
                VALUES (@bid,'Out',1,GETDATE(),@user)", conn, trans)
            {
                Parameters = {
                    new SqlParameter("@bid",  batchId),
                    new SqlParameter("@user", updatedBy)
                }
            }.ExecuteNonQuery();

            return batchId;
        }

        private int GetOrCreateVisitForSchedule(SqlConnection conn, SqlTransaction trans, int scheduleId, int caseId)
        {
            object r = new SqlCommand(
                "SELECT visit_id FROM ScheduledDose WHERE schedule_id=@sid AND visit_id IS NOT NULL",
                conn, trans)
            { Parameters = { new SqlParameter("@sid", scheduleId) } }.ExecuteScalar();

            if (r != null && r != DBNull.Value) return Convert.ToInt32(r);

            return Convert.ToInt32(new SqlCommand(@"
                INSERT INTO Visit (case_id,visit_type,visit_date,status)
                OUTPUT INSERTED.visit_id
                VALUES (@cid,'Follow-up',CAST(GETDATE() AS DATE),'Completed')",
                conn, trans)
            { Parameters = { new SqlParameter("@cid", caseId) } }.ExecuteScalar());
        }

        // ── BUTTON EVENTS ─────────────────────────────────────────

        protected void btnRefreshToday_Click(object sender, EventArgs e)
        { AutoCancelMissedDoses(); BindTodaySchedules(); }

        protected void btnSearchCase_Click(object sender, EventArgs e) => BindRegistrySummary(txtSearchCase.Text.Trim());
        protected void btnClearCaseSearch_Click(object sender, EventArgs e) { txtSearchCase.Text = ""; BindRegistrySummary(); }

        protected void btnRefreshSchedule_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
            { AutoCancelMissedDoses(); BindOverallSchedule(Convert.ToInt32(hfSelectedCaseId.Value)); }
        }

        protected void btnGenerateSchedule_Click(object sender, EventArgs e)
        {
            // A and B can schedule
            if (!CanSchedule) { ShowAlert("You do not have permission to generate schedules."); return; }

            if (string.IsNullOrEmpty(ddlProtocol.SelectedValue) ||
                string.IsNullOrEmpty(txtDay0.Text) ||
                string.IsNullOrEmpty(hfSelectedCaseId.Value)) return;

            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            DateTime day0;
            if (!DateTime.TryParse(txtDay0.Text, out day0)) return;

            int[] doseDays;
            switch (ddlProtocol.SelectedValue)
            {
                case "PEP_ESSEN": doseDays = new[] { 0, 3, 7, 14, 28 }; break;
                case "PEP_ZAGREB": doseDays = new[] { 0, 7, 21 }; break;
                case "PREP": doseDays = new[] { 0, 7, 21 }; break;
                default: return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int newRegimenId = Convert.ToInt32(new SqlCommand(@"
                            INSERT INTO VaccineRegimen (case_id,regimen_type,start_date,total_doses,status)
                            OUTPUT INSERTED.regimen_id
                            VALUES (@cid,@proto,@start,@total,'Active')", conn, trans)
                        {
                            Parameters = {
                                new SqlParameter("@cid",   caseId),
                                new SqlParameter("@proto", ddlProtocol.SelectedItem.Text),
                                new SqlParameter("@start", day0),
                                new SqlParameter("@total", doseDays.Length)
                            }
                        }.ExecuteScalar());

                        for (int i = 0; i < doseDays.Length; i++)
                        {
                            new SqlCommand(@"
                                INSERT INTO ScheduledDose (regimen_id,dose_number,schedule_date,status)
                                VALUES (@rid,@dnum,@sdate,'Pending')", conn, trans)
                            {
                                Parameters = {
                                    new SqlParameter("@rid",   newRegimenId),
                                    new SqlParameter("@dnum",  i + 1),
                                    new SqlParameter("@sdate", day0.AddDays(doseDays[i]))
                                }
                            }.ExecuteNonQuery();
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    { trans.Rollback(); ShowAlert("Schedule generation failed: " + ex.Message); return; }
                }
            }

            BindOverallSchedule(caseId);
        }

        protected void btnSaveDose_Click(object sender, EventArgs e)
        {
            // A and C can administer
            if (!CanAdminister) { ShowAlert("You do not have permission to administer doses."); return; }

            if (string.IsNullOrEmpty(hfSelectedScheduleId.Value) ||
                string.IsNullOrEmpty(ddlDoseVaccine.SelectedValue))
            { ShowAlert("Please select a vaccine before confirming."); return; }

            int scheduleId = Convert.ToInt32(hfSelectedScheduleId.Value);
            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            int vaccineId = Convert.ToInt32(ddlDoseVaccine.SelectedValue);
            bool isEdit = hfEditMode.Value == "true";
            string adminBy = !string.IsNullOrWhiteSpace(txtVaccinatedBy.Text)
                ? txtVaccinatedBy.Text.Trim()
                : Session["fullName"]?.ToString() ?? "System";

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
                            { trans.Rollback(); ShowAlert("No available stock for the selected vaccine."); return; }
                        }

                        int visitId = GetOrCreateVisitForSchedule(conn, trans, scheduleId, caseId);

                        new SqlCommand(@"
                            UPDATE ScheduledDose
                            SET status='Completed',vaccine_id=@vid,batch_id=@bid,visit_id=@visitId
                            WHERE schedule_id=@sid", conn, trans)
                        {
                            Parameters = {
                                new SqlParameter("@vid",     vaccineId),
                                new SqlParameter("@bid",     isEdit ? (object)DBNull.Value : batchId),
                                new SqlParameter("@visitId", visitId),
                                new SqlParameter("@sid",     scheduleId)
                            }
                        }.ExecuteNonQuery();

                        new SqlCommand(@"
                            IF EXISTS (SELECT 1 FROM Treatment WHERE visit_id=@vid)
                                UPDATE Treatment SET vaccine_id=@vacId,dosage=@dos,unit='mL',route=@rt,administered_by=@ab WHERE visit_id=@vid
                            ELSE
                                INSERT INTO Treatment (visit_id,vaccine_id,dosage,unit,route,administered_by)
                                VALUES (@vid,@vacId,@dos,'mL',@rt,@ab)", conn, trans)
                        {
                            Parameters = {
                                new SqlParameter("@vid",   visitId),
                                new SqlParameter("@vacId", vaccineId),
                                new SqlParameter("@dos",   string.IsNullOrWhiteSpace(txtDosage.Text) ? (object)DBNull.Value : txtDosage.Text.Trim()),
                                new SqlParameter("@rt",    string.IsNullOrWhiteSpace(txtRoute.Text)  ? (object)DBNull.Value : txtRoute.Text.Trim()),
                                new SqlParameter("@ab",    adminBy)
                            }
                        }.ExecuteNonQuery();

                        trans.Commit();
                    }
                    catch (Exception ex) { trans.Rollback(); ShowAlert("Error: " + ex.Message); return; }
                }
            }

            panelAdministration.Visible = false;
            hfSelectedScheduleId.Value = "";
            hfEditMode.Value = "";
            BindOverallSchedule(caseId);
            BindTodaySchedules();
        }

        protected void btnCancelDose_Click(object sender, EventArgs e)
        {
            panelAdministration.Visible = false;
            hfSelectedScheduleId.Value = "";
            hfEditMode.Value = "";
        }

        protected void btnSaveFollowUp_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfAnimalId.Value)) return;
            int animalId = Convert.ToInt32(hfAnimalId.Value);
            bool isUpdate = !string.IsNullOrEmpty(hfFollowUpId.Value);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = isUpdate
                    ? "UPDATE AnimalFollowUp SET day14_status=@s,followup_date=@d,notes=@n WHERE followup_id=@fid"
                    : "INSERT INTO AnimalFollowUp (animal_id,day14_status,followup_date,notes) VALUES (@aid,@s,@d,@n)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@s", ddlDay14Status.SelectedValue);
                    cmd.Parameters.AddWithValue("@d", string.IsNullOrEmpty(txtFollowUpDate.Text)
                        ? (object)DBNull.Value : Convert.ToDateTime(txtFollowUpDate.Text));
                    cmd.Parameters.AddWithValue("@n", txtFollowUpNotes.Text.Trim());
                    if (isUpdate) cmd.Parameters.AddWithValue("@fid", Convert.ToInt32(hfFollowUpId.Value));
                    else cmd.Parameters.AddWithValue("@aid", animalId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }

            if (!string.IsNullOrEmpty(hfSelectedCaseId.Value))
                LoadCaseDetails(Convert.ToInt32(hfSelectedCaseId.Value));
        }

        // ── GRIDVIEW ROW COMMANDS ─────────────────────────────────

        protected void gvTodaySchedules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
                OpenCase(Convert.ToInt32(gvTodaySchedules.DataKeys[Convert.ToInt32(e.CommandArgument)].Values["case_id"]));
        }

        protected void gvSummary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "OpenCase")
                OpenCase(Convert.ToInt32(gvSummary.DataKeys[Convert.ToInt32(e.CommandArgument)].Value));
        }

        private void OpenCase(int caseId)
        {
            hfSelectedCaseId.Value = caseId.ToString();
            LoadCaseDetails(caseId);
            BindOverallSchedule(caseId);
            BindVaccineDropdown();
            ShowActiveCaseView();
        }

        protected void gvSchedule_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idx = Convert.ToInt32(e.CommandArgument);
            int scheduleId = Convert.ToInt32(gvSchedule.DataKeys[idx].Value);

            if (e.CommandName == "AdministerDose" && CanAdminister)
            {
                hfSelectedScheduleId.Value = scheduleId.ToString();
                hfEditMode.Value = "false";
                BindVaccineDropdown();
                ddlDoseVaccine.SelectedIndex = 0;
                txtVaccinatedBy.Text = txtDosage.Text = txtRoute.Text = "";
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
            ClientScript.RegisterStartupScript(GetType(), "alert",
                $"alert('{msg.Replace("'", "\\'")}');", true);
        }
    }
}