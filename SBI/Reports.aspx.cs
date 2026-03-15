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

        protected void Page_Load(object sender, EventArgs e)
        {
            /*if (Session["userRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            string role = Session["userRole"].ToString().ToLower();

            if (role != "admin assistant" && role != "vaccinators")
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                hfActiveReport.Value = "DailyInventorySummary";
                ddlReportPeriod.SelectedValue = "Daily";
                SetDefaultDates();
                UpdateActiveTabStyles();
                GenerateReport();
            } */
        }

        private void SetDefaultDates()
        {
            DateTime today = DateTime.Today;
            txtFromDate.Text = today.ToString("yyyy-MM-dd");
            txtToDate.Text = today.ToString("yyyy-MM-dd");
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

                case "Custom":
                    break;
            }
        }

        protected void tabDailyInventory_Click(object sender, EventArgs e)
        {
            hfActiveReport.Value = "DailyInventorySummary";
            UpdateActiveTabStyles();
            GenerateReport();
        }

        protected void tabBiteCaseReport_Click(object sender, EventArgs e)
        {
            hfActiveReport.Value = "BiteCaseReport";
            UpdateActiveTabStyles();
            GenerateReport();
        }

        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            GenerateReport();
        }

        // ── Panel visibility helper ──────────────────────────────────────
        private void SetPanelVisibility(string reportType)
        {
            pnlGridReport.Visible = (reportType != "BiteCaseReport");
            pnlBiteCaseReport.Visible = (reportType == "BiteCaseReport");
        }

        // ── Main report dispatcher ───────────────────────────────────────
        private void GenerateReport()
        {
            lblMessage.Text = "";

            SetPanelVisibility(hfActiveReport.Value);
            lblActiveReport.Text = GetReportTitle(hfActiveReport.Value);

            if (hfActiveReport.Value == "BiteCaseReport")
            {
                DateTime fromDate, toDate;
                if (!TryGetDates(out fromDate, out toDate)) return;

                lblFromDate.Text = fromDate.ToString("MMM dd, yyyy");
                lblToDate.Text = toDate.ToString("MMM dd, yyyy");

                LoadBiteCaseReport(fromDate, toDate);
                return;
            }

            DateTime fd, td;
            if (!TryGetDates(out fd, out td)) return;

            DataTable dt = GetReportData(hfActiveReport.Value, fd, td);

            gvReport.DataSource = dt;
            gvReport.DataBind();

            lblFromDate.Text = fd.ToString("MMM dd, yyyy");
            lblToDate.Text = td.ToString("MMM dd, yyyy");
            lblTotalRecords.Text = dt.Rows.Count.ToString();
        }

        private bool TryGetDates(out DateTime fromDate, out DateTime toDate)
        {
            fromDate = DateTime.MinValue;
            toDate = DateTime.MinValue;

            if (!DateTime.TryParse(txtFromDate.Text, out fromDate))
            {
                lblMessage.Text = "Invalid From Date.";
                return false;
            }

            if (!DateTime.TryParse(txtToDate.Text, out toDate))
            {
                lblMessage.Text = "Invalid To Date.";
                return false;
            }

            if (fromDate > toDate)
            {
                lblMessage.Text = "From Date cannot be later than To Date.";
                return false;
            }

            return true;
        }

        private DataTable GetReportData(string reportType, DateTime fromDate, DateTime toDate)
        {
            switch (reportType)
            {
                case "DailyInventorySummary":
                    return GetDailyInventorySummaryData(fromDate, toDate);
                default:
                    return new DataTable();
            }
        }

        // ── Daily Inventory Summary ──────────────────────────────────────
        private DataTable GetDailyInventorySummaryData(DateTime fromDate, DateTime toDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        v.vaccine_name AS [Vaccine],
                        ISNULL(SUM(vb.quantity_received), 0) AS [INV. BEG],
                        ISNULL(SUM(CASE WHEN il.transaction_type = 'Consumed' THEN il.quantity ELSE 0 END), 0) AS [CONSUMED],
                        ISNULL(SUM(CASE WHEN il.transaction_type = 'Pull-Out'  THEN il.quantity ELSE 0 END), 0) AS [PULL-OUT],
                        ISNULL(SUM(vb.current_stock), 0) AS [INV. END]
                    FROM dbo.VaccineBatch vb
                    INNER JOIN dbo.Vaccine v ON vb.vaccine_id = v.vaccine_id
                    LEFT JOIN dbo.InventoryLog il
                        ON vb.batch_id = il.batch_id
                        AND il.transaction_date >= @fromDate
                        AND il.transaction_date < DATEADD(DAY, 1, @toDate)
                    GROUP BY v.vaccine_name
                    ORDER BY v.vaccine_name ASC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
                da.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
        }

        // ── Bite Case Report ─────────────────────────────────────────────
        private void LoadBiteCaseReport(DateTime fromDate, DateTime toDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // ── 1. Patient + Case header (first case in range) ──────
                string caseQuery = @"
                    SELECT TOP 1
                        p.patient_id,
                        p.last_name + ', ' + p.first_name + ' ' + ISNULL(p.middle_name, '') AS full_name,
                        CONVERT(VARCHAR, p.date_of_birth, 107)          AS date_of_birth,
                        DATEDIFF(YEAR, p.date_of_birth, GETDATE())      AS age,
                        p.gender,
                        p.civil_status,
                        p.contact_number,
                        p.occupation,
                        p.address,
                        p.emergency_contact_name + ' (' + p.emergency_contact_relation + ') - ' + p.emergency_contact_number AS emergency_contact,
                        bc.case_number,
                        CONVERT(VARCHAR, bc.bite_date, 107)             AS bite_date,
                        CONVERT(VARCHAR, bc.bite_time, 100)             AS bite_time,
                        bc.bite_place,
                        bc.exposure_type,
                        bc.animal_type,
                        bc.animal_ownership,
                        bc.circumstance,
                        bc.wound_type,
                        bc.bite_site,
                        CASE WHEN bc.is_bleeding = 1 THEN 'Yes' ELSE 'No' END AS bleeding,
                        CAST(bc.category AS VARCHAR)                    AS category,
                        CASE WHEN bc.washed_immediately = 1 THEN 'Yes' ELSE 'No' END AS washed_immediately,
                        bc.blood_pressure,
                        CAST(bc.temperature AS VARCHAR) + ' °C'         AS temperature,
                        CAST(bc.weight AS VARCHAR) + ' kg'              AS weight,
                        bc.capillary_refill,
                        bc.risk_classification,
                        bc.diagnosis,
                        bc.manifestation_symptoms,
                        vr.regimen_name,
                        vc.vaccine_name,
                        vc.manufacturer,
                        CONVERT(VARCHAR, vr.start_date, 107)            AS regimen_start_date,
                        vr.total_doses,
                        ao.observation_status,
                        CONVERT(VARCHAR, ao.completion_date, 107)       AS obs_completion_date,
                        ao.follow_up_notes,
                        ao.vaccination_continuation,
                        cc.overall_compliance,
                        cc.missed_doses,
                        cc.completion_status,
                        cc.compliance_rate,
                        bc.additional_notes
                    FROM dbo.BiteCase bc
                    INNER JOIN dbo.Patient p           ON bc.patient_id     = p.patient_id
                    LEFT  JOIN dbo.VaccinationRegimen vr ON bc.case_id      = vr.case_id
                    LEFT  JOIN dbo.Vaccine vc           ON vr.vaccine_id    = vc.vaccine_id
                    LEFT  JOIN dbo.AnimalObservation ao ON bc.case_id       = ao.case_id
                    LEFT  JOIN dbo.CaseCompliance cc    ON bc.case_id       = cc.case_id
                    WHERE bc.bite_date >= @fromDate
                      AND bc.bite_date < DATEADD(DAY, 1, @toDate)
                    ORDER BY bc.bite_date DESC";

                SqlCommand cmd = new SqlCommand(caseQuery, conn);
                cmd.Parameters.AddWithValue("@fromDate", fromDate.Date);
                cmd.Parameters.AddWithValue("@toDate", toDate.Date);

                SqlDataReader rdr = cmd.ExecuteReader();

                if (rdr.Read())
                {
                    // Patient Demographics
                    lblPatientId.Text = rdr["patient_id"].ToString();
                    lblPatientName.Text = rdr["full_name"].ToString();
                    lblDob.Text = rdr["date_of_birth"].ToString();
                    lblAge.Text = rdr["age"].ToString() + " years old";
                    lblGender.Text = rdr["gender"].ToString();
                    lblCivilStatus.Text = rdr["civil_status"].ToString();
                    lblContact.Text = rdr["contact_number"].ToString();
                    lblOccupation.Text = rdr["occupation"].ToString();
                    lblAddress.Text = rdr["address"].ToString();
                    lblEmergencyContact.Text = rdr["emergency_contact"].ToString();

                    // Bite Incident
                    lblCaseNumber.Text = rdr["case_number"].ToString();
                    lblBiteDate.Text = rdr["bite_date"].ToString();
                    lblBiteTime.Text = rdr["bite_time"].ToString();
                    lblBitePlace.Text = rdr["bite_place"].ToString();
                    lblExposureType.Text = rdr["exposure_type"].ToString();
                    lblAnimalType.Text = rdr["animal_type"].ToString() + " (" + rdr["animal_ownership"].ToString() + ")";
                    lblCircumstance.Text = rdr["circumstance"].ToString();
                    lblWoundType.Text = rdr["wound_type"].ToString();
                    lblBiteSite.Text = rdr["bite_site"].ToString();
                    lblBleeding.Text = rdr["bleeding"].ToString();
                    lblBiteCategory.Text = "Category " + rdr["category"].ToString();
                    lblWashed.Text = rdr["washed_immediately"].ToString();

                    // Medical Assessment
                    lblBP.Text = rdr["blood_pressure"].ToString();
                    lblTemp.Text = rdr["temperature"].ToString();
                    lblWeight.Text = rdr["weight"].ToString();
                    lblCapRefill.Text = rdr["capillary_refill"].ToString();
                    lblRiskClass.Text = rdr["risk_classification"].ToString();
                    lblDiagnosis.Text = rdr["diagnosis"].ToString();
                    lblSymptoms.Text = rdr["manifestation_symptoms"].ToString();

                    // Vaccination Regimen
                    lblRegimenType.Text = rdr["regimen_name"].ToString();
                    lblVaccineName.Text = rdr["vaccine_name"].ToString();
                    lblManufacturer.Text = rdr["manufacturer"].ToString();
                    lblVaccineStartDate.Text = rdr["regimen_start_date"].ToString();
                    lblTotalDoses.Text = rdr["total_doses"].ToString();

                    // Animal Observation
                    lblAnimalObsStatus.Text = rdr["observation_status"].ToString();
                    lblAnimalObsDate.Text = rdr["obs_completion_date"].ToString();
                    lblAnimalObsNotes.Text = rdr["follow_up_notes"].ToString();
                    lblVacContinuation.Text = rdr["vaccination_continuation"].ToString();

                    // Compliance
                    lblCompliance.Text = rdr["overall_compliance"].ToString();
                    lblMissedDoses.Text = rdr["missed_doses"].ToString();
                    lblCompletionStatus.Text = rdr["completion_status"].ToString();
                    lblComplianceRate.Text = rdr["compliance_rate"].ToString();

                    // Notes
                    lblAdditionalNotes.Text = rdr["additional_notes"].ToString();

                    lblTotalRecords.Text = "1";
                }
                else
                {
                    lblMessage.Text = "No bite case found for the selected date range.";
                    lblTotalRecords.Text = "0";
                }

                rdr.Close();

                // ── 2. Vaccination Schedule grid ────────────────────────
                string schedQuery = @"
                    SELECT
                        vs.dose_number                                      AS [Dose],
                        CONVERT(VARCHAR, vs.scheduled_date, 107)            AS [Scheduled Date],
                        ISNULL(CONVERT(VARCHAR, vs.visit_date, 107), '-')   AS [Visit Date],
                        vs.status                                            AS [Status],
                        ISNULL(u.full_name, '-')                            AS [Administered By],
                        ISNULL(vb.batch_number, '-')                        AS [Batch No.]
                    FROM dbo.VaccinationSchedule vs
                    INNER JOIN dbo.BiteCase bc ON vs.case_id = bc.case_id
                    LEFT  JOIN dbo.AppUser u   ON vs.administered_by = u.user_id
                    LEFT  JOIN dbo.VaccineBatch vb ON vs.batch_id = vb.batch_id
                    WHERE bc.bite_date >= @fromDate
                      AND bc.bite_date < DATEADD(DAY, 1, @toDate)
                    ORDER BY vs.dose_number ASC";

                SqlDataAdapter da2 = new SqlDataAdapter(schedQuery, conn);
                da2.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
                da2.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

                DataTable dtSched = new DataTable();
                da2.Fill(dtSched);
                gvVaccinationSchedule.DataSource = dtSched;
                gvVaccinationSchedule.DataBind();

                // ── 3. Treatment Summary grid ────────────────────────────
                string treatQuery = @"
                    SELECT
                        CONVERT(VARCHAR, v.visit_date, 107)  AS [Visit Date],
                        v.visit_type                         AS [Visit Type],
                        vs.dose_number                       AS [Dose Day],
                        vc.vaccine_name                      AS [Vaccine Used],
                        ISNULL(vb.batch_number, '-')         AS [Batch No.],
                        ISNULL(u.full_name, '-')             AS [Administered By]
                    FROM dbo.Visit v
                    INNER JOIN dbo.BiteCase bc           ON v.case_id     = bc.case_id
                    LEFT  JOIN dbo.VaccinationSchedule vs ON v.schedule_id = vs.schedule_id
                    LEFT  JOIN dbo.VaccineBatch vb        ON v.batch_id    = vb.batch_id
                    LEFT  JOIN dbo.Vaccine vc             ON vb.vaccine_id = vc.vaccine_id
                    LEFT  JOIN dbo.AppUser u              ON v.administered_by = u.user_id
                    WHERE bc.bite_date >= @fromDate
                      AND bc.bite_date < DATEADD(DAY, 1, @toDate)
                    ORDER BY v.visit_date ASC";

                SqlDataAdapter da3 = new SqlDataAdapter(treatQuery, conn);
                da3.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
                da3.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

                DataTable dtTreat = new DataTable();
                da3.Fill(dtTreat);
                gvTreatmentSummary.DataSource = dtTreat;
                gvTreatmentSummary.DataBind();
            }
        }

        // ── Excel Export ─────────────────────────────────────────────────
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DateTime fromDate, toDate;
            if (!TryGetDates(out fromDate, out toDate)) return;

            if (hfActiveReport.Value == "BiteCaseReport")
            {
                lblMessage.Text = "Excel export for Bite Case Report is not yet implemented.";
                return;
            }

            DataTable dt = GetReportData(hfActiveReport.Value, fromDate, toDate);

            if (dt.Rows.Count == 0)
            {
                lblMessage.Text = "No data available to export.";
                return;
            }

            ExportToExcel(dt, GetReportTitle(hfActiveReport.Value), fromDate, toDate);
        }

        private void ExportToExcel(DataTable dt, string reportTitle, DateTime fromDate, DateTime toDate)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (ExcelPackage package = new ExcelPackage())
            {
                ExcelWorksheet ws = package.Workbook.Worksheets.Add("Report");

                ws.Cells["A1"].Value = "SBI Medical Animal Bite Center";
                ws.Cells["A2"].Value = reportTitle;
                ws.Cells["A3"].Value = "From: " + fromDate.ToString("MMM dd, yyyy") + "   To: " + toDate.ToString("MMM dd, yyyy");

                ws.Cells["A1"].Style.Font.Bold = true;
                ws.Cells["A1"].Style.Font.Size = 16;
                ws.Cells["A2"].Style.Font.Bold = true;
                ws.Cells["A2"].Style.Font.Size = 14;
                ws.Cells["A3"].Style.Font.Italic = true;

                int startRow = 5;

                for (int col = 0; col < dt.Columns.Count; col++)
                {
                    ws.Cells[startRow, col + 1].Value = dt.Columns[col].ColumnName;
                    ws.Cells[startRow, col + 1].Style.Font.Bold = true;
                    ws.Cells[startRow, col + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[startRow, col + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(11, 42, 122));
                    ws.Cells[startRow, col + 1].Style.Font.Color.SetColor(Color.White);
                    ws.Cells[startRow, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                for (int row = 0; row < dt.Rows.Count; row++)
                {
                    for (int col = 0; col < dt.Columns.Count; col++)
                    {
                        ws.Cells[row + startRow + 1, col + 1].Value = dt.Rows[row][col];
                        ws.Cells[row + startRow + 1, col + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }
                }

                ws.Cells[ws.Dimension.Address].AutoFitColumns();

                string fileName = reportTitle.Replace(" ", "_") + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";

                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                Response.BinaryWrite(package.GetAsByteArray());
                Response.End();
            }
        }

        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            lblMessage.Text = "PDF export is not yet implemented.";
        }

        private string GetReportTitle(string reportType)
        {
            switch (reportType)
            {
                case "DailyInventorySummary": return "Daily Inventory Summary";
                case "BiteCaseReport": return "Comprehensive Bite Case Report";
                default: return "Report";
            }
        }

        private void UpdateActiveTabStyles()
        {
            string activeClass = "rounded-xl border border-blue-600 bg-blue-600 px-6 py-3 text-base font-semibold text-white transition";
            string inactiveClass = "rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50";

            tabDailyInventory.Text = "Daily Inventory Summary";
            tabBiteCaseReport.Text = "Bite Case Report";

            tabDailyInventory.CssClass = inactiveClass;
            tabBiteCaseReport.CssClass = inactiveClass;

            switch (hfActiveReport.Value)
            {
                case "DailyInventorySummary": tabDailyInventory.CssClass = activeClass; break;
                case "BiteCaseReport": tabBiteCaseReport.CssClass = activeClass; break;
            }
        }
    }
}