using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web.UI;

namespace SBI
{
    public partial class Reports : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

        // ── Page Load ────────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            string role = Session["userRole"].ToString().ToUpper();

            // All roles A, B, C can access Reports
            // Only redirect if somehow an unrecognized role slips through
            if (role != "A" && role != "B" && role != "C")
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                hfActiveReport.Value = "DailyInventorySummary";
                ddlReportPeriod.SelectedValue = "Daily";
                SetDefaultDates();
                ApplyTabStyles();
                GenerateReport();
            }
        }

        // ── Tab Button Handlers ──────────────────────────────────────────
        protected void btnTabInventory_Click(object sender, EventArgs e)
        {
            hfActiveReport.Value = "DailyInventorySummary";
            ApplyTabStyles();
            GenerateReport();
        }

        protected void btnTabBiteCase_Click(object sender, EventArgs e)
        {
            hfActiveReport.Value = "BiteCaseReport";
            ApplyTabStyles();
            GenerateReport();
        }

        // ── Filter / Generate ────────────────────────────────────────────
        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        protected void ddlReportPeriod_SelectedIndexChanged(object sender, EventArgs e)
        {
            DateTime today = DateTime.Today;
            switch (ddlReportPeriod.SelectedValue)
            {
                case "Daily":
                    txtFromDate.Text = today.ToString("yyyy-MM-dd");
                    txtToDate.Text = today.ToString("yyyy-MM-dd");
                    break;
                case "Weekly":
                    txtFromDate.Text = today.AddDays(-6).ToString("yyyy-MM-dd");
                    txtToDate.Text = today.ToString("yyyy-MM-dd");
                    break;
                case "Monthly":
                    txtFromDate.Text = new DateTime(today.Year, today.Month, 1).ToString("yyyy-MM-dd");
                    txtToDate.Text = today.ToString("yyyy-MM-dd");
                    break;
            }
        }

        // ── Core dispatcher ──────────────────────────────────────────────
        private void GenerateReport()
        {
            lblMessage.Text = "";

            bool isBite = hfActiveReport.Value == "BiteCaseReport";
            pnlInventory.Visible = !isBite;
            pnlBiteCase.Visible = isBite;
            lblActiveReport.Text = isBite ? "Bite Case Report" : "Daily Inventory Summary";

            DateTime from, to;
            if (!TryGetDates(out from, out to)) return;

            lblFromDate.Text = from.ToString("MMM dd, yyyy");
            lblToDate.Text = to.ToString("MMM dd, yyyy");

            if (isBite)
                LoadBiteCaseReport(from, to);
            else
                LoadInventoryReport(from, to);
        }

        private void SetDefaultDates()
        {
            txtFromDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
            txtToDate.Text = DateTime.Today.ToString("yyyy-MM-dd");
        }

        private bool TryGetDates(out DateTime from, out DateTime to)
        {
            from = to = DateTime.MinValue;
            if (!DateTime.TryParse(txtFromDate.Text, out from))
            { lblMessage.Text = "Invalid From Date."; return false; }
            if (!DateTime.TryParse(txtToDate.Text, out to))
            { lblMessage.Text = "Invalid To Date."; return false; }
            if (from > to)
            { lblMessage.Text = "From Date cannot be later than To Date."; return false; }
            return true;
        }

        private void ApplyTabStyles()
        {
            string active = "rounded-xl border border-blue-600 bg-blue-600 px-6 py-3 text-sm font-semibold text-white transition cursor-pointer shadow-sm";
            string inactive = "rounded-xl border border-slate-300 bg-white px-6 py-3 text-sm font-semibold text-slate-700 transition cursor-pointer shadow-sm hover:bg-slate-50";

            btnTabInventory.CssClass = hfActiveReport.Value == "DailyInventorySummary" ? active : inactive;
            btnTabBiteCase.CssClass = hfActiveReport.Value == "BiteCaseReport" ? active : inactive;
        }

        // ════════════════════════════════════════════════════════════════
        // PANEL A — Daily Inventory Summary
        // Tables: dbo.Vaccine, dbo.VaccineBatch, dbo.InventoryLog
        // ════════════════════════════════════════════════════════════════
        private void LoadInventoryReport(DateTime from, DateTime to)
        {
            string sql = @"
                SELECT
                    v.vaccine_name                                                      AS [Vaccine],
                    v.vaccine_category                                                  AS [Category],
                    ISNULL(SUM(vb.quantity_received), 0)                               AS [INV. BEG],
                    ISNULL(SUM(CASE WHEN il.transaction_type = 'Consumed'
                                    THEN il.quantity ELSE 0 END), 0)                   AS [CONSUMED],
                    ISNULL(SUM(CASE WHEN il.transaction_type = 'Pull-Out'
                                    THEN il.quantity ELSE 0 END), 0)                   AS [PULL-OUT],
                    ISNULL(SUM(vb.current_stock), 0)                                   AS [INV. END]
                FROM dbo.VaccineBatch vb
                INNER JOIN dbo.Vaccine v
                    ON vb.vaccine_id = v.vaccine_id
                LEFT JOIN dbo.InventoryLog il
                    ON vb.batch_id = il.batch_id
                   AND il.transaction_date >= @from
                   AND il.transaction_date <  DATEADD(DAY, 1, @to)
                WHERE v.is_active = 'Yes'
                GROUP BY v.vaccine_name, v.vaccine_category
                ORDER BY v.vaccine_name ASC";

            DataTable dt = ExecuteTable(sql, from, to);
            gvInventory.DataSource = dt;
            gvInventory.DataBind();
            lblTotalRecords.Text = dt.Rows.Count.ToString();
        }

        // ════════════════════════════════════════════════════════════════
        // PANEL B — Bite Case Report
        // ════════════════════════════════════════════════════════════════
        private void LoadBiteCaseReport(DateTime from, DateTime to)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // ── 1. Case header: dbo.Case + dbo.Patient + dbo.Address
                //       + dbo.EmergencyContact + dbo.VitalSigns + dbo.Visit(initial)
                //       + dbo.Manifestation + dbo.VaccineRegimen + dbo.Vaccine
                //       + dbo.Animal + dbo.AnimalFollowUp
                // ─────────────────────────────────────────────────────────
                string caseQuery = @"
                    SELECT TOP 1
                        -- Patient
                        p.patient_id,
                        p.lname + ', ' + p.fname                                        AS full_name,
                        CONVERT(VARCHAR, p.date_of_birth, 107)                          AS date_of_birth,
                        DATEDIFF(YEAR, p.date_of_birth,
                            CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.date_of_birth, GETDATE()), p.date_of_birth) > GETDATE()
                                 THEN DATEADD(YEAR, DATEDIFF(YEAR, p.date_of_birth, GETDATE()) - 1, p.date_of_birth)
                                 ELSE GETDATE() END)                                    AS age,
                        CASE p.gender WHEN 'M' THEN 'Male'
                                      WHEN 'F' THEN 'Female'
                                      ELSE p.gender END                                 AS gender,
                        p.civil_status,
                        p.contact_no,
                        p.occupation,
                        p.address                                                       AS full_address,
                        -- Emergency Contact
                        ISNULL(ec.emergency_contact_person, 'N/A') + ' — '
                            + ISNULL(ec.emergency_contact_number, '')                   AS emergency_contact,
                        -- Case
                        c.case_id,
                        c.case_no,
                        CONVERT(VARCHAR, c.date_of_bite, 107)                           AS bite_date,
                        CONVERT(VARCHAR, c.time_of_bite, 100)                           AS bite_time,
                        c.place_of_bite,
                        c.type_of_exposure,
                        c.wound_type,
                        c.site_of_bite,
                        CASE c.bleeding WHEN 'Yes' THEN 'Yes'
                                        WHEN 'No'  THEN 'No'
                                        ELSE c.bleeding END                             AS bleeding,
                        c.category,
                        CASE c.washed WHEN 'Yes' THEN 'Yes'
                                      WHEN 'No'  THEN 'No'
                                      ELSE c.washed END                                 AS washed,
                        -- Animal
                        a.animal_type,
                        a.ownership,
                        a.animal_status,
                        a.circumstances,
                        -- AnimalFollowUp
                        af.day14_status,
                        CONVERT(VARCHAR, af.followup_date, 107)                         AS followup_date,
                        af.notes                                                        AS followup_notes,
                        -- VitalSigns (most recent for this patient)
                        vs.blood_pressure,
                        CAST(vs.temperature AS VARCHAR(20))                             AS temperature,
                        CAST(vs.wt AS VARCHAR(20))                                      AS weight,
                        vs.cr                                                           AS capillary_refill,
                        -- Visit (first/initial)
                        vi.diagnosis,
                        vi.manifestation_notes,
                        -- VaccineRegimen
                        vr.regimen_type,
                        vr.start_date                                                   AS regimen_start,
                        vr.total_doses,
                        vr.status                                                       AS regimen_status,
                        -- Vaccine
                        vc.vaccine_name,
                        vc.manufacturer
                    FROM dbo.[Case] c
                    INNER JOIN dbo.Patient p
                        ON c.patient_id = p.patient_id
                    LEFT JOIN dbo.EmergencyContact ec
                        ON p.patient_id = ec.patient_id
                    LEFT JOIN dbo.Animal a
                        ON a.case_id = c.case_id
                    LEFT JOIN dbo.AnimalFollowUp af
                        ON af.animal_id = a.animal_id
                    LEFT JOIN dbo.VitalSigns vs
                        ON vs.patient_id = p.patient_id
                    LEFT JOIN dbo.Visit vi
                        ON vi.case_id = c.case_id AND vi.dose_day = 0
                    LEFT JOIN dbo.VaccineRegimen vr
                        ON vr.case_id = c.case_id
                    LEFT JOIN dbo.Vaccine vc
                        ON vc.vaccine_id = vr.vaccine_id
                    WHERE c.date_of_bite >= @from
                      AND c.date_of_bite <  DATEADD(DAY, 1, @to)
                    ORDER BY c.date_of_bite DESC";

                SqlCommand cmd = new SqlCommand(caseQuery, conn);
                cmd.Parameters.AddWithValue("@from", from.Date);
                cmd.Parameters.AddWithValue("@to", to.Date);
                SqlDataReader rdr = cmd.ExecuteReader();

                int caseId = 0;
                string patientId = "";

                if (rdr.Read())
                {
                    caseId = Convert.ToInt32(rdr["case_id"]);
                    patientId = rdr["patient_id"].ToString();

                    // Patient Demographics
                    lblPatientId.Text = rdr["patient_id"].ToString();
                    lblPatientName.Text = rdr["full_name"].ToString();
                    lblDob.Text = rdr["date_of_birth"].ToString();
                    lblAge.Text = rdr["age"].ToString() + " yrs old";
                    lblGender.Text = rdr["gender"].ToString();
                    lblCivilStatus.Text = rdr["civil_status"].ToString();
                    lblContact.Text = rdr["contact_no"].ToString();
                    lblOccupation.Text = rdr["occupation"].ToString();
                    lblAddress.Text = rdr["full_address"].ToString();
                    lblEmergencyContact.Text = rdr["emergency_contact"].ToString();

                    // Bite Incident
                    lblCaseNo.Text = rdr["case_no"].ToString();
                    lblBiteDate.Text = rdr["bite_date"].ToString();
                    lblBiteTime.Text = rdr["bite_time"].ToString();
                    lblBitePlace.Text = rdr["place_of_bite"].ToString();
                    lblExposureType.Text = rdr["type_of_exposure"].ToString();
                    lblWoundType.Text = rdr["wound_type"].ToString();
                    lblBiteSite.Text = rdr["site_of_bite"].ToString();
                    lblBleeding.Text = rdr["bleeding"].ToString();
                    lblCategory.Text = "Category " + rdr["category"].ToString();
                    lblWashed.Text = rdr["washed"].ToString();
                    lblAnimalType.Text = rdr["animal_type"].ToString();
                    lblAnimalOwnership.Text = rdr["ownership"].ToString();
                    lblAnimalStatus.Text = rdr["animal_status"].ToString();
                    lblCircumstances.Text = rdr["circumstances"].ToString();

                    // Animal Follow-Up
                    lblAnimalObsStatus.Text = rdr["animal_status"].ToString();
                    lblDay14Status.Text = rdr["day14_status"].ToString();
                    lblFollowupDate.Text = rdr["followup_date"].ToString();
                    lblFollowupNotes.Text = rdr["followup_notes"].ToString();

                    // Vital Signs & Medical Assessment
                    lblBP.Text = rdr["blood_pressure"].ToString();
                    lblTemp.Text = rdr["temperature"].ToString() + " °C";
                    lblWeight.Text = rdr["weight"].ToString() + " kg";
                    lblCR.Text = rdr["capillary_refill"].ToString();
                    lblDiagnosis.Text = rdr["diagnosis"].ToString();
                    lblSymptoms.Text = rdr["manifestation_notes"].ToString();

                    // Vaccination Regimen
                    lblRegimenType.Text = rdr["regimen_type"].ToString();
                    lblVaccineName.Text = rdr["vaccine_name"].ToString();
                    lblManufacturer.Text = rdr["manufacturer"].ToString();
                    lblRegimenStart.Text = rdr["regimen_start"] != DBNull.Value
                                           ? Convert.ToDateTime(rdr["regimen_start"]).ToString("MMM dd, yyyy")
                                           : "—";
                    lblTotalDoses.Text = rdr["total_doses"].ToString();
                    lblRegimenStatus.Text = rdr["regimen_status"].ToString();

                    lblTotalRecords.Text = "1";
                }
                else
                {
                    lblMessage.Text = "No bite case found for the selected date range.";
                    lblTotalRecords.Text = "0";
                    rdr.Close();
                    return;
                }
                rdr.Close();

                // ── Also pull Manifestation symptoms from dbo.Manifestation ──
                string manifestSql = @"
                    SELECT STRING_AGG(symptom, ', ')  AS symptoms
                    FROM   dbo.Manifestation
                    WHERE  case_id = @caseId";
                SqlCommand manifestCmd = new SqlCommand(manifestSql, conn);
                manifestCmd.Parameters.AddWithValue("@caseId", caseId);
                object manifestResult = manifestCmd.ExecuteScalar();
                if (manifestResult != DBNull.Value && manifestResult != null && manifestResult.ToString() != "")
                    lblSymptoms.Text = manifestResult.ToString();

                // ── 2. Vaccination Schedule — dbo.ScheduledDose ──────────
                // ScheduledDose: schedule_id, regimen_id, dose_number,
                //                schedule_date, visit_id(FK), status,
                //                vaccine_id(FK), batch_id(FK)
                string schedSql = @"
                    SELECT
                        sd.dose_number                                          AS [Dose],
                        CONVERT(VARCHAR, sd.schedule_date, 107)                 AS [Scheduled Date],
                        ISNULL(CONVERT(VARCHAR, vi.visit_date, 107), '—')       AS [Visit Date],
                        sd.status                                                AS [Status],
                        ISNULL(vc.vaccine_name, '—')                            AS [Vaccine],
                        ISNULL(vb.batch_number, '—')                            AS [Batch No.],
                        ISNULL(u.fname + ' ' + u.lname, '—')                   AS [Administered By]
                    FROM dbo.ScheduledDose sd
                    INNER JOIN dbo.VaccineRegimen vr
                        ON sd.regimen_id = vr.regimen_id
                    LEFT JOIN dbo.Visit vi
                        ON sd.visit_id = vi.visit_id
                    LEFT JOIN dbo.Vaccine vc
                        ON sd.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.VaccineBatch vb
                        ON sd.batch_id = vb.batch_id
                    LEFT JOIN dbo.Users u
                        ON vi.status = u.username   -- placeholder; adjust if Visit stores user_id
                    WHERE vr.case_id = @caseId
                    ORDER BY sd.dose_number ASC";

                DataTable dtSched = ExecuteTableById(schedSql, caseId, conn);
                gvSchedule.DataSource = dtSched;
                gvSchedule.DataBind();

                // ── 3. Treatment Summary — dbo.Visit + dbo.Treatment ─────
                // Visit: visit_id, case_id, visit_type, dose_day,
                //        visit_date, diagnosis, manifestation_notes, status
                // Treatment: treatment_id, visit_id(FK), vaccine_id(FK),
                //             dosage, unit, route, administered_by
                string treatSql = @"
                    SELECT
                        CONVERT(VARCHAR, vi.visit_date, 107)    AS [Visit Date],
                        vi.visit_type                           AS [Visit Type],
                        'Day ' + CAST(vi.dose_day AS VARCHAR)   AS [Dose Day],
                        ISNULL(vc.vaccine_name, '—')            AS [Vaccine Used],
                        ISNULL(vb.batch_number, '—')            AS [Batch No.],
                        CAST(t.dosage AS VARCHAR) + ' '
                            + ISNULL(t.unit,'')                 AS [Dosage],
                        ISNULL(t.route, '—')                    AS [Route],
                        ISNULL(t.administered_by, '—')          AS [Administered By]
                    FROM dbo.Visit vi
                    LEFT JOIN dbo.Treatment t
                        ON t.visit_id = vi.visit_id
                    LEFT JOIN dbo.Vaccine vc
                        ON t.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.VaccineBatch vb
                        ON t.vaccine_id = vb.vaccine_id   -- pick newest batch for display
                    WHERE vi.case_id = @caseId
                    ORDER BY vi.visit_date ASC";

                DataTable dtTreat = ExecuteTableById(treatSql, caseId, conn);
                gvTreatment.DataSource = dtTreat;
                gvTreatment.DataBind();

                // ── 4. Previous Vaccine History — dbo.PreviousVaccineHistory ──
                string prevSql = @"
                    SELECT TOP 1
                        CASE had_previous_vaccine WHEN 'Yes' THEN 'Yes' ELSE 'No' END AS [Had Previous Vaccine],
                        ISNULL(previous_vaccine_type, '—')  AS previous_vaccine_type,
                        ISNULL(previous_brand, '—')         AS previous_brand,
                        ISNULL(dose_status, '—')            AS dose_status,
                        ISNULL(CONVERT(VARCHAR, vaccination_date, 107), '—') AS vaccination_date
                    FROM dbo.PreviousVaccineHistory
                    WHERE patient_id = @patientId";

                SqlCommand prevCmd = new SqlCommand(prevSql, conn);
                prevCmd.Parameters.AddWithValue("@patientId", patientId);
                SqlDataReader prevRdr = prevCmd.ExecuteReader();
                if (prevRdr.Read())
                {
                    lblHadPrevVaccine.Text = prevRdr["Had Previous Vaccine"].ToString();
                    lblPrevVaccineType.Text = prevRdr["previous_vaccine_type"].ToString();
                    lblPrevBrand.Text = prevRdr["previous_brand"].ToString();
                    lblPrevDoseStatus.Text = prevRdr["dose_status"].ToString();
                    lblPrevVaccDate.Text = prevRdr["vaccination_date"].ToString();
                }
                else
                {
                    lblHadPrevVaccine.Text = "No record";
                    lblPrevVaccineType.Text = lblPrevBrand.Text =
                    lblPrevDoseStatus.Text = lblPrevVaccDate.Text = "—";
                }
                prevRdr.Close();
            }
        }

        // ── Excel Export ─────────────────────────────────────────────────
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            if (!TryGetDates(out from, out to)) return;

            if (hfActiveReport.Value == "BiteCaseReport")
            {
                lblMessage.Text = "Excel export for Bite Case Report is not yet implemented.";
                return;
            }

            LoadInventoryReport(from, to);   // ensure dt is fresh
            string sql = @"
                SELECT
                    v.vaccine_name      AS [Vaccine],
                    v.vaccine_category  AS [Category],
                    ISNULL(SUM(vb.quantity_received), 0) AS [INV. BEG],
                    ISNULL(SUM(CASE WHEN il.transaction_type = 'Consumed' THEN il.quantity ELSE 0 END), 0) AS [CONSUMED],
                    ISNULL(SUM(CASE WHEN il.transaction_type = 'Pull-Out'  THEN il.quantity ELSE 0 END), 0) AS [PULL-OUT],
                    ISNULL(SUM(vb.current_stock), 0) AS [INV. END]
                FROM dbo.VaccineBatch vb
                INNER JOIN dbo.Vaccine v ON vb.vaccine_id = v.vaccine_id
                LEFT JOIN dbo.InventoryLog il
                    ON vb.batch_id = il.batch_id
                   AND il.transaction_date >= @from
                   AND il.transaction_date <  DATEADD(DAY, 1, @to)
                WHERE v.is_active = 'Yes'
                GROUP BY v.vaccine_name, v.vaccine_category
                ORDER BY v.vaccine_name";

            DataTable dt = ExecuteTable(sql, from, to);
            if (dt.Rows.Count == 0) { lblMessage.Text = "No data to export."; return; }
            ExportToExcel(dt, "Daily Inventory Summary", from, to);
        }

        private void ExportToExcel(DataTable dt, string title, DateTime from, DateTime to)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (ExcelPackage pkg = new ExcelPackage())
            {
                ExcelWorksheet ws = pkg.Workbook.Worksheets.Add("Report");
                ws.Cells["A1"].Value = "SBI Medical Animal Bite Center — Morong Branch";
                ws.Cells["A2"].Value = title;
                ws.Cells["A3"].Value = "From: " + from.ToString("MMM dd, yyyy") + "   To: " + to.ToString("MMM dd, yyyy");
                ws.Cells["A1"].Style.Font.Bold = true; ws.Cells["A1"].Style.Font.Size = 14;
                ws.Cells["A2"].Style.Font.Bold = true; ws.Cells["A2"].Style.Font.Size = 12;
                ws.Cells["A3"].Style.Font.Italic = true;

                int startRow = 5;
                for (int c = 0; c < dt.Columns.Count; c++)
                {
                    ws.Cells[startRow, c + 1].Value = dt.Columns[c].ColumnName;
                    ws.Cells[startRow, c + 1].Style.Font.Bold = true;
                    ws.Cells[startRow, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[startRow, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(11, 42, 122));
                    ws.Cells[startRow, c + 1].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[startRow, c + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
                for (int r = 0; r < dt.Rows.Count; r++)
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        ws.Cells[r + startRow + 1, c + 1].Value = dt.Rows[r][c];
                        ws.Cells[r + startRow + 1, c + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();
                string fn = title.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + fn);
                Response.BinaryWrite(pkg.GetAsByteArray());
                Response.End();
            }
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "PDF export is not yet implemented.";
        }

        // ── Helpers ──────────────────────────────────────────────────────
        private DataTable ExecuteTable(string sql, DateTime from, DateTime to)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                da.SelectCommand.Parameters.AddWithValue("@from", from.Date);
                da.SelectCommand.Parameters.AddWithValue("@to", to.Date);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        private DataTable ExecuteTableById(string sql, int caseId, SqlConnection conn)
        {
            SqlDataAdapter da = new SqlDataAdapter(sql, conn);
            da.SelectCommand.Parameters.AddWithValue("@caseId", caseId);
            DataTable dt = new DataTable();
            da.Fill(dt);
            return dt;
        }
    }
}
