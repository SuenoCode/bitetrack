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
        private string UserId => Session["userId"]?.ToString() ?? "0";
        private string UserName => Session["fullName"]?.ToString() ?? "System";

        private bool IsAdmin => UserRole == "A";
        private bool IsRoleB => UserRole == "B";
        private bool IsRoleC => UserRole == "C";

        public bool CanOpenCase => IsAdmin || IsRoleC;
        public bool CanManageCase => IsAdmin || IsRoleB;
        public bool CanAdminister => IsAdmin || IsRoleB || IsRoleC;

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
            }
        }

        private int? GetValidUserId()
        {
            if (!string.IsNullOrEmpty(UserId) && int.TryParse(UserId, out int userId))
            {
                using (SqlConnection conn = new SqlConnection(connString))
                {
                    conn.Open();
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT COUNT(*) FROM AppUser WHERE user_id = @uid", conn))
                    {
                        cmd.Parameters.AddWithValue("@uid", userId);
                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        if (count > 0)
                            return userId;
                    }
                }
            }
            return null;
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
            panelScheduleInfo.Visible = true;
        }

        private void BindTodaySchedules()
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT s.schedule_id, r.case_id, c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           s.dose_number, s.schedule_date, v.vaccine_name,
                           c.category,
                           s.status
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
                           c.category,
                           c.date_of_bite,
                           r.regimen_type,
                           r.total_doses,
                           (SELECT COUNT(*)
                            FROM ScheduledDose sd
                            WHERE sd.regimen_id = r.regimen_id
                              AND sd.status = 'Completed') AS completed_doses,
                           CASE 
                               WHEN r.regimen_id IS NULL THEN 'No Schedule'
                               WHEN (SELECT COUNT(*) FROM ScheduledDose sd WHERE sd.regimen_id = r.regimen_id AND sd.status = 'Pending') > 0 THEN 'In Progress'
                               WHEN (SELECT COUNT(*) FROM ScheduledDose sd WHERE sd.regimen_id = r.regimen_id AND sd.status = 'Completed') = r.total_doses THEN 'Complete'
                               ELSE 'In Progress'
                           END AS case_status
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
                           s.status, v.vaccine_name, 
                           s.vaccine_id, s.batch_id,
                           s.administered_by_user,
                           ISNULL(u.fname + ' ' + u.lname, 'System') AS administered_by
                    FROM ScheduledDose s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    LEFT JOIN Vaccine v ON s.vaccine_id = v.vaccine_id
                    LEFT JOIN AppUser u ON s.administered_by_user = u.user_id
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

                    panelScheduleInfo.Visible = true;

                    int total = dt.Rows.Count;
                    int completed = 0;
                    int pending = 0;
                    foreach (DataRow row in dt.Rows)
                    {
                        string status = row["status"].ToString();
                        if (status == "Completed") completed++;
                        else if (status == "Pending") pending++;
                    }

                    litTotalDoses.Text = total.ToString();
                    litCompletedDoses.Text = completed.ToString();
                    litPendingDoses.Text = pending.ToString();

                    string protocolName = "";
                    using (SqlCommand cmd2 = new SqlCommand(
                        "SELECT TOP 1 regimen_type FROM VaccineRegimen WHERE case_id = @cid", conn))
                    {
                        cmd2.Parameters.AddWithValue("@cid", caseId);
                        object result = cmd2.ExecuteScalar();
                        if (result != null)
                            protocolName = result.ToString();
                    }
                    litProtocolDisplay.Text = string.IsNullOrEmpty(protocolName) ? "—" : protocolName;
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
                           v.diagnosis AS initial_diagnosis,
                           c.animal_type,
                           c.site_of_bite,
                           c.wound_type
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

                            litAnimalType.Text = reader["animal_type"] == DBNull.Value
                                ? "—"
                                : reader["animal_type"].ToString();

                            litSiteOfBite.Text = reader["site_of_bite"] == DBNull.Value
                                ? "—"
                                : reader["site_of_bite"].ToString();

                            litWoundType.Text = reader["wound_type"] == DBNull.Value
                                ? "—"
                                : reader["wound_type"].ToString();
                        }
                    }
                }
            }

            bool hasSchedule = CheckIfScheduleExists(caseId);
            if (!hasSchedule)
            {
                AutoGenerateSchedule(caseId);
            }
            else
            {
                AutoReserveVialsForCase(caseId);
            }
        }

        private bool CheckIfScheduleExists(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT COUNT(*) FROM VaccineRegimen WHERE case_id = @CaseId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CaseId", caseId);
                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0;
                }
            }
        }

        private int GetRecommendedVaccine(string category)
        {
            string vaccineType = category.ToUpper().Trim() == "III"
                ? "Rabies Immunoglobulin"
                : "Post-Exposure Prophylaxis";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT TOP 1 v.vaccine_id
                    FROM Vaccine v
                    WHERE v.vaccine_type = @type
                      AND v.is_active = 'Yes'
                      AND EXISTS (
                          SELECT 1 
                          FROM VaccineBatch b 
                          WHERE b.vaccine_id = v.vaccine_id 
                            AND b.current_stock > 0 
                            AND b.expiration_date >= CAST(GETDATE() AS DATE)
                      )
                    ORDER BY v.vaccine_id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@type", vaccineType);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT TOP 1 v.vaccine_id 
                    FROM Vaccine v
                    INNER JOIN VaccineBatch b ON v.vaccine_id = b.vaccine_id
                    WHERE v.is_active = 'Yes'
                      AND b.current_stock > 0
                      AND b.expiration_date >= CAST(GETDATE() AS DATE)
                    ORDER BY v.vaccine_id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                        return Convert.ToInt32(result);
                }
            }

            return 1;
        }

        private void AutoGenerateSchedule(int caseId)
        {
            string category = "";
            DateTime? biteDate = null;

            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = "SELECT category, date_of_bite FROM [Case] WHERE case_id = @CaseId";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CaseId", caseId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            category = reader["category"]?.ToString() ?? "";
                            biteDate = reader["date_of_bite"] == DBNull.Value
                                ? (DateTime?)null
                                : Convert.ToDateTime(reader["date_of_bite"]);
                        }
                    }
                }
            }

            if (string.IsNullOrEmpty(category) || !biteDate.HasValue)
                return;

            int[] doseDays;
            string protocolName;

            switch (category.ToUpper().Trim())
            {
                case "III":
                    doseDays = new[] { 0, 0, 7, 21 };
                    protocolName = "PEP Zagreb (0, 0, 7, 21)";
                    break;
                case "II":
                    doseDays = new[] { 0, 3, 7, 14, 28 };
                    protocolName = "PEP Essen (0, 3, 7, 14, 28)";
                    break;
                case "I":
                    doseDays = new[] { 0, 7, 21 };
                    protocolName = "PrEP Standard (0, 7, 21)";
                    break;
                default:
                    doseDays = new[] { 0, 3, 7, 14, 28 };
                    protocolName = "PEP Essen (0, 3, 7, 14, 28)";
                    break;
            }

            int vaccineId = GetRecommendedVaccine(category);

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int newRegimenId;
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO VaccineRegimen 
                                (case_id, vaccine_id, regimen_type, start_date, total_doses, status)
                            OUTPUT INSERTED.regimen_id
                            VALUES 
                                (@cid, @vid, @proto, @start, @total, 'Active')", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@cid", caseId);
                            cmd.Parameters.AddWithValue("@vid", vaccineId);
                            cmd.Parameters.AddWithValue("@proto", protocolName);
                            cmd.Parameters.AddWithValue("@start", biteDate.Value);
                            cmd.Parameters.AddWithValue("@total", doseDays.Length);
                            newRegimenId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        for (int i = 0; i < doseDays.Length; i++)
                        {
                            DateTime doseDate = biteDate.Value.AddDays(doseDays[i]);

                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO ScheduledDose 
                                    (regimen_id, dose_number, schedule_date, status, vaccine_id)
                                VALUES 
                                    (@rid, @dnum, @sdate, 'Pending', @vid)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@rid", newRegimenId);
                                cmd.Parameters.AddWithValue("@dnum", i + 1);
                                cmd.Parameters.AddWithValue("@sdate", doseDate);
                                cmd.Parameters.AddWithValue("@vid", vaccineId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        ReserveVialsForRegimen(conn, trans, newRegimenId, vaccineId);

                        trans.Commit();
                        ShowAlert($"Schedule automatically generated: {protocolName} for Category {category}.", "success");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ShowAlert("Schedule generation failed: " + ex.Message, "error");
                    }
                }
            }
        }

        private void ReserveVialsForRegimen(SqlConnection conn, SqlTransaction trans, int regimenId, int vaccineId)
        {
            int? userId = GetValidUserId();

            DataTable doses = new DataTable();
            using (SqlCommand cmd = new SqlCommand(
                "SELECT schedule_id FROM ScheduledDose WHERE regimen_id = @rid AND status = 'Pending'", conn, trans))
            {
                cmd.Parameters.AddWithValue("@rid", regimenId);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    da.Fill(doses);
                }
            }

            foreach (DataRow row in doses.Rows)
            {
                int scheduleId = Convert.ToInt32(row["schedule_id"]);

                int vialId = -1;
                int batchId = -1;

                using (SqlCommand cmd = new SqlCommand(@"
                    SELECT TOP 1 v.vial_id, v.batch_id
                    FROM VaccineVial v
                    INNER JOIN VaccineBatch b ON v.batch_id = b.batch_id
                    WHERE b.vaccine_id = @vid
                      AND v.vial_status = 'Sealed'
                      AND b.current_stock > 0
                      AND b.expiration_date >= CAST(GETDATE() AS DATE)
                    ORDER BY b.expiration_date ASC, v.vial_no ASC", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@vid", vaccineId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            vialId = Convert.ToInt32(reader["vial_id"]);
                            batchId = Convert.ToInt32(reader["batch_id"]);
                        }
                        reader.Close();
                    }
                }

                if (vialId == -1)
                    continue;

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE VaccineVial 
                    SET vial_status = 'Open', 
                        opened_at = GETDATE(),
                        opened_by = @userId
                    WHERE vial_id = @vid", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@vid", vialId);
                    cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE VaccineBatch 
                    SET current_stock = current_stock - 1
                    WHERE batch_id = @bid", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@bid", batchId);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO InventoryLog 
                        (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                    VALUES 
                        (@bid, 'Reserved', 1, GETDATE(), @userId, @ref)", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@bid", batchId);
                    cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@ref", scheduleId);
                    cmd.ExecuteNonQuery();
                }

                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE ScheduledDose 
                    SET batch_id = @bid
                    WHERE schedule_id = @sid", conn, trans))
                {
                    cmd.Parameters.AddWithValue("@bid", batchId);
                    cmd.Parameters.AddWithValue("@sid", scheduleId);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void AutoReserveVialsForCase(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        DataTable pendingDoses = new DataTable();
                        using (SqlCommand cmd = new SqlCommand(@"
                            SELECT s.schedule_id, s.vaccine_id
                            FROM ScheduledDose s
                            INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                            WHERE r.case_id = @cid
                              AND s.status = 'Pending'
                              AND s.batch_id IS NULL", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@cid", caseId);
                            using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                            {
                                da.Fill(pendingDoses);
                            }
                        }

                        int? userId = GetValidUserId();

                        foreach (DataRow row in pendingDoses.Rows)
                        {
                            int scheduleId = Convert.ToInt32(row["schedule_id"]);
                            int vaccineId = Convert.ToInt32(row["vaccine_id"]);

                            int vialId = -1;
                            int batchId = -1;

                            using (SqlCommand cmd = new SqlCommand(@"
                                SELECT TOP 1 v.vial_id, v.batch_id
                                FROM VaccineVial v
                                INNER JOIN VaccineBatch b ON v.batch_id = b.batch_id
                                WHERE b.vaccine_id = @vid
                                  AND v.vial_status = 'Sealed'
                                  AND b.current_stock > 0
                                  AND b.expiration_date >= CAST(GETDATE() AS DATE)
                                ORDER BY b.expiration_date ASC, v.vial_no ASC", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@vid", vaccineId);
                                using (SqlDataReader reader = cmd.ExecuteReader())
                                {
                                    if (reader.Read())
                                    {
                                        vialId = Convert.ToInt32(reader["vial_id"]);
                                        batchId = Convert.ToInt32(reader["batch_id"]);
                                    }
                                    reader.Close();
                                }
                            }

                            if (vialId == -1)
                                continue;

                            using (SqlCommand cmd = new SqlCommand(@"
                                UPDATE VaccineVial 
                                SET vial_status = 'Open', 
                                    opened_at = GETDATE(),
                                    opened_by = @userId
                                WHERE vial_id = @vid", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@vid", vialId);
                                cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(@"
                                UPDATE VaccineBatch 
                                SET current_stock = current_stock - 1
                                WHERE batch_id = @bid", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@bid", batchId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(@"
                                INSERT INTO InventoryLog 
                                    (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                                VALUES 
                                    (@bid, 'Reserved', 1, GETDATE(), @userId, @ref)", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@bid", batchId);
                                cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                                cmd.Parameters.AddWithValue("@ref", scheduleId);
                                cmd.ExecuteNonQuery();
                            }

                            using (SqlCommand cmd = new SqlCommand(@"
                                UPDATE ScheduledDose 
                                SET batch_id = @bid
                                WHERE schedule_id = @sid", conn, trans))
                            {
                                cmd.Parameters.AddWithValue("@bid", batchId);
                                cmd.Parameters.AddWithValue("@sid", scheduleId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        trans.Commit();
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        System.Diagnostics.Debug.WriteLine("Error reserving vials: " + ex.Message);
                    }
                }
            }
        }

        // ── REMOVED: BindVaccineDropdown() - no longer needed ──

        // ── REMOVED: LoadDoseForEdit() - no longer needed ──

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

                            if (reader["dose_day"] == DBNull.Value)
                            {
                                hfVisitDoseDay.Value = "";
                                litDoseDayDisplay.Text = "—";
                            }
                            else
                            {
                                int storedDay = Convert.ToInt32(reader["dose_day"]);
                                hfVisitDoseDay.Value = storedDay.ToString();
                                litDoseDayDisplay.Text = "Day " + storedDay;
                            }

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

        private int? ComputeDoseDay()
        {
            if (string.IsNullOrWhiteSpace(txtVisitDate.Text))
                return null;

            DateTime visitDate;
            if (!DateTime.TryParse(txtVisitDate.Text, out visitDate))
                return null;

            DateTime biteDate;
            string rawBite = litBiteDateDisplay.Text;
            if (string.IsNullOrWhiteSpace(rawBite) || rawBite == "—")
                return null;

            if (!DateTime.TryParse(rawBite, out biteDate))
                return null;

            int diff = (visitDate.Date - biteDate.Date).Days;
            return diff >= 0 ? (int?)diff : null;
        }

        // ── Dose Confirmation (Auto-filled) ──────────────────────────

        private void LoadDoseConfirmation(int scheduleId)
        {
            using (SqlConnection conn = new SqlConnection(connString))
            {
                string query = @"
                    SELECT 
                        s.schedule_id,
                        s.dose_number,
                        s.vaccine_id,
                        v.vaccine_name,
                        b.batch_number,
                        b.batch_id,
                        v.vaccine_type
                    FROM ScheduledDose s
                    INNER JOIN VaccineRegimen r ON s.regimen_id = r.regimen_id
                    LEFT JOIN Vaccine v ON s.vaccine_id = v.vaccine_id
                    LEFT JOIN VaccineBatch b ON s.batch_id = b.batch_id
                    WHERE s.schedule_id = @sid";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@sid", scheduleId);
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            litConfirmVaccine.Text = reader["vaccine_name"]?.ToString() ?? "—";
                            litConfirmBatch.Text = reader["batch_number"]?.ToString() ?? "—";
                            litConfirmPractitioner.Text = UserName;
                            litConfirmDoseNumber.Text = "Dose " + reader["dose_number"].ToString();

                            string vaccineType = reader["vaccine_type"]?.ToString() ?? "";
                            if (vaccineType.Contains("Immunoglobulin") || vaccineType.Contains("RIG"))
                            {
                                txtConfirmDosage.Text = "20";
                                ddlConfirmRoute.SelectedValue = "IM";
                            }
                            else
                            {
                                txtConfirmDosage.Text = "0.5";
                                ddlConfirmRoute.SelectedValue = "ID";
                            }

                            hfConfirmVaccineId.Value = reader["vaccine_id"]?.ToString() ?? "";
                            hfConfirmBatchId.Value = reader["batch_id"]?.ToString() ?? "";
                        }
                    }
                }
            }
        }

        protected void btnConfirmDose_Click(object sender, EventArgs e)
        {
            if (!CanAdminister)
            {
                ShowAlert("You do not have permission to administer doses.", "error");
                return;
            }

            if (string.IsNullOrEmpty(hfSelectedScheduleId.Value))
            {
                ShowAlert("No dose selected.", "warning");
                return;
            }

            if (string.IsNullOrEmpty(txtConfirmDosage.Text))
            {
                ShowAlert("Please enter the dosage.", "warning");
                return;
            }

            if (string.IsNullOrEmpty(ddlConfirmRoute.SelectedValue))
            {
                ShowAlert("Please select the route of administration.", "warning");
                return;
            }

            int scheduleId = Convert.ToInt32(hfSelectedScheduleId.Value);
            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            int? userId = GetValidUserId();

            if (!decimal.TryParse(txtConfirmDosage.Text, out decimal dosage))
            {
                ShowAlert("Please enter a valid dosage.", "warning");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        int batchId = -1;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT batch_id FROM ScheduledDose WHERE schedule_id = @sid", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@sid", scheduleId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                                batchId = Convert.ToInt32(result);
                        }

                        if (batchId == -1)
                        {
                            trans.Rollback();
                            ShowAlert("No reserved vial found for this dose.", "error");
                            return;
                        }

                        int visitId;
                        using (SqlCommand cmd = new SqlCommand(
                            "SELECT visit_id FROM ScheduledDose WHERE schedule_id = @sid", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@sid", scheduleId);
                            object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                visitId = Convert.ToInt32(result);
                            }
                            else
                            {
                                using (SqlCommand cmd2 = new SqlCommand(@"
                                    INSERT INTO Visit (case_id, visit_type, visit_date, status)
                                    OUTPUT INSERTED.visit_id
                                    VALUES (@cid, 'Follow-up', CAST(GETDATE() AS DATE), 'Completed')", conn, trans))
                                {
                                    cmd2.Parameters.AddWithValue("@cid", caseId);
                                    visitId = Convert.ToInt32(cmd2.ExecuteScalar());
                                }
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE ScheduledDose
                            SET status = 'Completed',
                                visit_id = @visitId,
                                dosage = @dosage,
                                route = @route,
                                administered_by_user = @userId
                            WHERE schedule_id = @sid", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@visitId", visitId);
                            cmd.Parameters.AddWithValue("@dosage", dosage);
                            cmd.Parameters.AddWithValue("@route", ddlConfirmRoute.SelectedValue);
                            cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@sid", scheduleId);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE VaccineBatch 
                            SET current_stock = current_stock - 1
                            WHERE batch_id = @bid AND current_stock > 0", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@bid", batchId);
                            int rowsAffected = cmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                trans.Rollback();
                                ShowAlert("Stock adjustment failed. Please try again.", "error");
                                return;
                            }
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            UPDATE VaccineVial 
                            SET doses_used = doses_used + 1,
                                vial_status = CASE 
                                    WHEN doses_used + 1 >= doses_per_vial THEN 'Empty'
                                    ELSE 'Open'
                                END
                            WHERE batch_id = @bid AND vial_status = 'Open'
                            ORDER BY vial_id
                            OFFSET 0 ROWS FETCH NEXT 1 ROWS ONLY", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@bid", batchId);
                            cmd.ExecuteNonQuery();
                        }

                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO InventoryLog 
                                (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                            VALUES 
                                (@bid, 'Dispensed', 1, GETDATE(), @userId, @ref)", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@bid", batchId);
                            cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                            cmd.Parameters.AddWithValue("@ref", scheduleId);
                            cmd.ExecuteNonQuery();
                        }

                        trans.Commit();

                        panelAdministration.Visible = false;
                        hfSelectedScheduleId.Value = "";
                        hfEditMode.Value = "";

                        BindOverallSchedule(caseId);
                        BindTodaySchedules();
                        BindVisitHistory(caseId);
                        ShowAlert($"Dose {litConfirmDoseNumber.Text} administered successfully.", "success");
                    }
                    catch (Exception ex)
                    {
                        trans.Rollback();
                        ShowAlert("Error: " + ex.Message, "error");
                    }
                }
            }
        }

        protected void btnCancelDose_Click(object sender, EventArgs e)
        {
            panelAdministration.Visible = false;
            hfSelectedScheduleId.Value = "";
            hfEditMode.Value = "";
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
                int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
                BindOverallSchedule(caseId);
                AutoReserveVialsForCase(caseId);
            }
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

            int? doseDayInt = ComputeDoseDay();
            litDoseDayDisplay.Text = doseDayInt.HasValue ? "Day " + doseDayInt.Value : "—";
            hfVisitDoseDay.Value = doseDayInt.HasValue ? doseDayInt.Value.ToString() : "";

            object doseDayParam = doseDayInt.HasValue ? (object)doseDayInt.Value : DBNull.Value;

            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            bool isEdit = hfVisitEditMode.Value == "true";

            using (SqlConnection conn = new SqlConnection(connString))
            {
                conn.Open();

                if (isEdit && !string.IsNullOrEmpty(hfSelectedVisitId.Value))
                {
                    string updateQuery = @"
                        UPDATE Visit
                        SET visit_type          = @visit_type,
                            visit_date          = @visit_date,
                            dose_day            = @dose_day,
                            diagnosis           = @diagnosis,
                            manifestation_notes = @manifestation_notes,
                            status              = @status
                        WHERE visit_id = @visit_id";

                    using (SqlCommand cmd = new SqlCommand(updateQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@visit_type", ddlVisitType.SelectedValue);
                        cmd.Parameters.AddWithValue("@visit_date", visitDate);
                        cmd.Parameters.AddWithValue("@dose_day", doseDayParam);
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
                        cmd.Parameters.AddWithValue("@dose_day", doseDayParam);
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

            ShowAlert("Follow-up saved successfully.", "success");
        }

        protected void gvTodaySchedules_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
            {
                if (!CanOpenCase)
                {
                    ShowAlert("You do not have permission to open cases.", "error");
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
                    ShowAlert("You do not have permission to manage cases.", "error");
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

                LoadDoseConfirmation(scheduleId);
                panelAdministration.Visible = true;
            }
            // REMOVED: EditDose functionality since we simplified
        }

        private void ShowAlert(string msg, string type = "info")
        {
            string safe = msg.Replace("\\", "\\\\").Replace("'", "\\'")
                             .Replace(Environment.NewLine, " ").Replace("\r", "").Replace("\n", " ");
            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(),
                "showNotifyModal('" + safe + "','" + type + "');", true);
        }
    }
}