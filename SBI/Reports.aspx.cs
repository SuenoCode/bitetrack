using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBI
{
    public partial class Reports : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

        // ── Page Load ─────────────────────────────────────────────────
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            { Response.Redirect("Login.aspx"); return; }

            string role = Session["userRole"].ToString().ToUpper();
            if (role != "A" && role != "B" && role != "C")
            { Response.Redirect("Login.aspx"); return; }

            // ── Handle PDF export render request ──────────────────────────
            string exportCaseId = Request.QueryString["exportCaseId"];
            if (!string.IsNullOrEmpty(exportCaseId))
            {
                int caseId;
                if (int.TryParse(exportCaseId, out caseId))
                {
                    // Show only the case detail panel, hide everything else
                    hfActiveReport.Value = "BiteCaseReport";
                    hfSelectedCaseId.Value = caseId.ToString();
                    pnlInventory.Visible = false;
                    pnlBiteCase.Visible = true;
                    pnlCaseDetail.Visible = true;

                    // Hide the nav/filter UI so PDF is clean
                    btnTabInventory.Visible = false;
                    btnTabBiteCase.Visible = false;
                    btnExportPdf.Visible = false;
                    btnBackToCaseList.Visible = false;

                    LoadBiteCaseReport(caseId);
                    lblActiveReport.Text = "Bite Case Report";
                }
                return; // Don't run the rest of Page_Load
            }

            if (!IsPostBack)
            {
                hfActiveReport.Value = "DailyInventorySummary";
                hfSelectedCaseId.Value = "";
                ddlReportPeriod.SelectedValue = "Daily";
                SetDefaultDates();
                ApplyTabStyles();
                LoadInventoryReport(DateTime.Today, DateTime.Today);
                BindCaseList();
            }
        }

        // ── Tab Handlers ──────────────────────────────────────────────
        protected void btnTabInventory_Click(object sender, EventArgs e)
        {
            hfActiveReport.Value = "DailyInventorySummary";
            hfSelectedCaseId.Value = "";
            pnlInventory.Visible = true;
            pnlBiteCase.Visible = false;
            pnlCaseDetail.Visible = false;
            ApplyTabStyles();
            lblActiveReport.Text = "Daily Inventory Summary";
        }

        protected void btnTabBiteCase_Click(object sender, EventArgs e)
        {
            hfActiveReport.Value = "BiteCaseReport";
            hfSelectedCaseId.Value = "";
            pnlInventory.Visible = false;
            pnlBiteCase.Visible = true;
            pnlCaseDetail.Visible = false;
            ApplyTabStyles();
            lblActiveReport.Text = "Bite Case Report";
            BindCaseList();
        }

        // ── Inventory Filter ──────────────────────────────────────────
        protected void btnGenerateReport_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            if (!TryGetDates(out from, out to)) return;
            LoadInventoryReport(from, to);
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

        // ── Case List Search ──────────────────────────────────────────
        protected void btnSearchCases_Click(object sender, EventArgs e)
        {
            pnlCaseDetail.Visible = false;
            hfSelectedCaseId.Value = "";
            BindCaseList(txtCaseSearch.Text.Trim(),
                ParseNullableDate(txtCaseFromDate.Text),
                ParseNullableDate(txtCaseToDate.Text));
        }

        protected void btnClearCaseSearch_Click(object sender, EventArgs e)
        {
            txtCaseSearch.Text = "";
            txtCaseFromDate.Text = "";
            txtCaseToDate.Text = "";
            pnlCaseDetail.Visible = false;
            hfSelectedCaseId.Value = "";
            BindCaseList();
        }

        protected void btnBackToCaseList_Click(object sender, EventArgs e)
        {
            pnlCaseDetail.Visible = false;
            hfSelectedCaseId.Value = "";
            lblTotalRecords.Text = gvCaseList.Rows.Count.ToString();
        }

        // ── Case List Grid RowCommand — load report for selected case ─
        protected void gvCaseList_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                int caseId = Convert.ToInt32(gvCaseList.DataKeys[rowIndex].Value);
                hfSelectedCaseId.Value = caseId.ToString();
                LoadBiteCaseReport(caseId);
                pnlCaseDetail.Visible = true;
                lblTotalRecords.Text = "1";
            }
        }

        // ── ApplyTabStyles ────────────────────────────────────────────
        private void ApplyTabStyles()
        {
            string active = "rounded-xl border border-blue-600 bg-blue-600 px-6 py-3 text-sm font-semibold text-white transition cursor-pointer shadow-sm";
            string inactive = "rounded-xl border border-slate-300 bg-white px-6 py-3 text-sm font-semibold text-slate-700 transition cursor-pointer shadow-sm hover:bg-slate-50";
            btnTabInventory.CssClass = hfActiveReport.Value == "DailyInventorySummary" ? active : inactive;
            btnTabBiteCase.CssClass = hfActiveReport.Value == "BiteCaseReport" ? active : inactive;
        }

        // ── Bind Case List ────────────────────────────────────────────
        private void BindCaseList(string search = "", DateTime? from = null, DateTime? to = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT c.case_id,
                           c.case_no,
                           (p.fname + ' ' + p.lname) AS patient_name,
                           c.date_of_bite,
                           c.category,
                           r.regimen_type
                    FROM   dbo.[Case]   c
                    INNER JOIN dbo.Patient        p ON c.patient_id = p.patient_id
                    LEFT  JOIN dbo.VaccineRegimen r ON c.case_id    = r.case_id
                    WHERE  (@search = ''
                        OR  c.case_no                  LIKE '%' + @search + '%'
                        OR  p.fname                    LIKE '%' + @search + '%'
                        OR  p.lname                    LIKE '%' + @search + '%'
                        OR  (p.fname + ' ' + p.lname)  LIKE '%' + @search + '%')
                      AND  (@from IS NULL OR c.date_of_bite >= @from)
                      AND  (@to   IS NULL OR c.date_of_bite <= @to)
                    ORDER BY c.date_of_bite DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", search ?? "");
                da.SelectCommand.Parameters.AddWithValue("@from", (object)from ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@to", (object)to ?? DBNull.Value);

                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCaseList.DataSource = dt;
                gvCaseList.DataBind();
                lblTotalRecords.Text = dt.Rows.Count.ToString();
            }
        }

        // ── Inventory Report ──────────────────────────────────────────
        private void LoadInventoryReport(DateTime from, DateTime to)
        {
            lblFromDate.Text = from.ToString("MMM dd, yyyy");
            lblToDate.Text = to.ToString("MMM dd, yyyy");
            lblActiveReport.Text = "Daily Inventory Summary";

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
                INNER JOIN dbo.Vaccine v ON vb.vaccine_id = v.vaccine_id
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

        // ── Bite Case Report — load by case_id ───────────────────────
        private void LoadBiteCaseReport(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string caseQuery = @"
                    SELECT
                        p.patient_id,
                        p.lname + ', ' + p.fname                                        AS full_name,
                        CONVERT(VARCHAR, p.date_of_birth, 107)                          AS date_of_birth,
                        DATEDIFF(YEAR, p.date_of_birth,
                            CASE WHEN DATEADD(YEAR, DATEDIFF(YEAR, p.date_of_birth, GETDATE()), p.date_of_birth) > GETDATE()
                                 THEN DATEADD(YEAR, DATEDIFF(YEAR, p.date_of_birth, GETDATE()) - 1, p.date_of_birth)
                                 ELSE GETDATE() END)                                    AS age,
                        CASE p.gender WHEN 'M' THEN 'Male' WHEN 'F' THEN 'Female' ELSE p.gender END AS gender,
                        p.civil_status,
                        p.contact_no,
                        p.occupation,
                        p.address                                                       AS full_address,
                        ISNULL(ec.emergency_contact_person, 'N/A') + ' — '
                            + ISNULL(ec.emergency_contact_number, '')                   AS emergency_contact,
                        c.case_id,
                        c.case_no,
                        CONVERT(VARCHAR, c.date_of_bite, 107)                           AS bite_date,
                        CONVERT(VARCHAR, c.time_of_bite, 100)                           AS bite_time,
                        c.place_of_bite,
                        c.type_of_exposure,
                        c.wound_type,
                        c.site_of_bite,
                        c.bleeding,
                        c.category,
                        c.washed,
                        a.animal_type,
                        a.ownership,
                        a.animal_status,
                        a.circumstances,
                        af.day14_status,
                        CONVERT(VARCHAR, af.followup_date, 107)                         AS followup_date,
                        af.notes                                                        AS followup_notes,
                        vs.blood_pressure,
                        CAST(vs.temperature AS VARCHAR(20))                             AS temperature,
                        CAST(vs.wt AS VARCHAR(20))                                      AS weight,
                        vs.cr                                                           AS capillary_refill,
                        vi.diagnosis,
                        vi.manifestation_notes,
                        vr.regimen_type,
                        vr.start_date                                                   AS regimen_start,
                        vr.total_doses,
                        vr.status                                                       AS regimen_status,
                        vc.vaccine_name,
                        vc.manufacturer
                    FROM dbo.[Case] c
                    INNER JOIN dbo.Patient p        ON c.patient_id    = p.patient_id
                    LEFT JOIN dbo.EmergencyContact ec ON p.patient_id  = ec.patient_id
                    LEFT JOIN dbo.Animal a           ON a.case_id      = c.case_id
                    LEFT JOIN dbo.AnimalFollowUp af  ON af.animal_id   = a.animal_id
                    LEFT JOIN dbo.VitalSigns vs      ON vs.patient_id  = p.patient_id
                    LEFT JOIN dbo.Visit vi           ON vi.case_id     = c.case_id
                                                    AND vi.visit_type  = 'Initial Visit'
                    LEFT JOIN dbo.VaccineRegimen vr  ON vr.case_id     = c.case_id
                    LEFT JOIN dbo.Vaccine vc         ON vc.vaccine_id  = vr.vaccine_id
                    WHERE c.case_id = @caseId";

                SqlCommand cmd = new SqlCommand(caseQuery, conn);
                cmd.Parameters.AddWithValue("@caseId", caseId);
                SqlDataReader rdr = cmd.ExecuteReader();

                string patientId = "";

                if (rdr.Read())
                {
                    patientId = rdr["patient_id"].ToString();

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

                    lblAnimalObsStatus.Text = rdr["animal_status"].ToString();
                    lblDay14Status.Text = rdr["day14_status"] == DBNull.Value ? "—" : rdr["day14_status"].ToString();
                    lblFollowupDate.Text = rdr["followup_date"] == DBNull.Value ? "—" : rdr["followup_date"].ToString();
                    lblFollowupNotes.Text = rdr["followup_notes"] == DBNull.Value ? "—" : rdr["followup_notes"].ToString();

                    lblBP.Text = rdr["blood_pressure"] == DBNull.Value ? "—" : rdr["blood_pressure"].ToString();
                    lblTemp.Text = rdr["temperature"] == DBNull.Value ? "—" : rdr["temperature"].ToString() + " °C";
                    lblWeight.Text = rdr["weight"] == DBNull.Value ? "—" : rdr["weight"].ToString() + " kg";
                    lblCR.Text = rdr["capillary_refill"] == DBNull.Value ? "—" : rdr["capillary_refill"].ToString();
                    lblDiagnosis.Text = rdr["diagnosis"] == DBNull.Value ? "—" : rdr["diagnosis"].ToString();
                    lblSymptoms.Text = rdr["manifestation_notes"] == DBNull.Value ? "—" : rdr["manifestation_notes"].ToString();

                    lblRegimenType.Text = rdr["regimen_type"] == DBNull.Value ? "—" : rdr["regimen_type"].ToString();
                    lblVaccineName.Text = rdr["vaccine_name"] == DBNull.Value ? "—" : rdr["vaccine_name"].ToString();
                    lblManufacturer.Text = rdr["manufacturer"] == DBNull.Value ? "—" : rdr["manufacturer"].ToString();
                    lblRegimenStart.Text = rdr["regimen_start"] == DBNull.Value ? "—"
                        : Convert.ToDateTime(rdr["regimen_start"]).ToString("MMM dd, yyyy");
                    lblTotalDoses.Text = rdr["total_doses"] == DBNull.Value ? "—" : rdr["total_doses"].ToString();
                    lblRegimenStatus.Text = rdr["regimen_status"] == DBNull.Value ? "—" : rdr["regimen_status"].ToString();
                }
                else
                {
                    lblMessage.Text = "Case not found.";
                    rdr.Close();
                    return;
                }
                rdr.Close();

                // Pull manifestation symptoms
                string manifestSql = @"
                    SELECT STRING_AGG(symptom, ', ') AS symptoms
                    FROM   dbo.Manifestation
                    WHERE  case_id = @caseId";
                SqlCommand manifestCmd = new SqlCommand(manifestSql, conn);
                manifestCmd.Parameters.AddWithValue("@caseId", caseId);
                object manifestResult = manifestCmd.ExecuteScalar();
                if (manifestResult != DBNull.Value && manifestResult != null && manifestResult.ToString() != "")
                    lblSymptoms.Text = manifestResult.ToString();

                // Vaccination Schedule
                string schedSql = @"
                    SELECT
                        sd.dose_number                                              AS [Dose],
                        CONVERT(VARCHAR, sd.schedule_date, 107)                     AS [Scheduled Date],
                        sd.status                                                   AS [Status],
                        ISNULL(vc.vaccine_name, '—')                               AS [Vaccine],
                        ISNULL(vb.batch_number, '—')                               AS [Batch No.],
                        ISNULL(t.administered_by, '—')                             AS [Administered By],
                        ISNULL(CAST(t.dosage AS VARCHAR) + ' ' + ISNULL(t.unit,''), '—') AS [Dosage],
                        ISNULL(t.route, '—')                                       AS [Route]
                    FROM dbo.ScheduledDose sd
                    INNER JOIN dbo.VaccineRegimen vr ON sd.regimen_id = vr.regimen_id
                    LEFT JOIN dbo.Vaccine         vc ON sd.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.VaccineBatch    vb ON sd.batch_id   = vb.batch_id
                    LEFT JOIN dbo.Treatment        t ON sd.visit_id   = t.visit_id
                    WHERE vr.case_id = @caseId
                    ORDER BY sd.dose_number ASC";

                DataTable dtSched = ExecuteTableById(schedSql, caseId, conn);
                gvSchedule.DataSource = dtSched;
                gvSchedule.DataBind();

                // Treatment Summary
                string treatSql = @"
                    SELECT
                        CONVERT(VARCHAR, vi.visit_date, 107)                AS [Visit Date],
                        vi.visit_type                                       AS [Visit Type],
                        ISNULL('Day ' + CAST(vi.dose_day AS VARCHAR), '—') AS [Dose Day],
                        ISNULL(vc.vaccine_name, '—')                       AS [Vaccine Used],
                        ISNULL(vb.batch_number, '—')                       AS [Batch No.],
                        ISNULL(CAST(t.dosage AS VARCHAR)+' '+ISNULL(t.unit,''), '—') AS [Dosage],
                        ISNULL(t.route, '—')                               AS [Route],
                        ISNULL(t.administered_by, '—')                     AS [Administered By]
                    FROM dbo.Visit vi
                    LEFT JOIN dbo.Treatment    t  ON t.visit_id  = vi.visit_id
                    LEFT JOIN dbo.Vaccine      vc ON t.vaccine_id= vc.vaccine_id
                    LEFT JOIN dbo.VaccineBatch vb ON sd.batch_id = vb.batch_id
                    WHERE vi.case_id = @caseId
                    ORDER BY vi.visit_date ASC";

                // Simplified treatment — avoid joining ScheduledDose here
                string treatSqlSimple = @"
                    SELECT
                        CONVERT(VARCHAR, vi.visit_date, 107)                         AS [Visit Date],
                        vi.visit_type                                                AS [Visit Type],
                        ISNULL('Day ' + CAST(vi.dose_day AS VARCHAR), '—')           AS [Dose Day],
                        ISNULL(vc.vaccine_name, '—')                                AS [Vaccine Used],
                        ISNULL(CAST(t.dosage AS VARCHAR)+' '+ISNULL(t.unit,''), '—') AS [Dosage],
                        ISNULL(t.route, '—')                                        AS [Route],
                        ISNULL(t.administered_by, '—')                              AS [Administered By]
                    FROM dbo.Visit vi
                    LEFT JOIN dbo.Treatment t  ON t.visit_id   = vi.visit_id
                    LEFT JOIN dbo.Vaccine   vc ON t.vaccine_id = vc.vaccine_id
                    WHERE vi.case_id = @caseId
                    ORDER BY vi.visit_date ASC";

                DataTable dtTreat = ExecuteTableById(treatSqlSimple, caseId, conn);
                gvTreatment.DataSource = dtTreat;
                gvTreatment.DataBind();

                // Previous Vaccine History
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

        // ── Excel Export (Inventory only) ─────────────────────────────
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            if (!TryGetDates(out from, out to)) return;

            string sql = @"
                SELECT
                    v.vaccine_name      AS [Vaccine],
                    v.vaccine_category  AS [Category],
                    ISNULL(SUM(vb.quantity_received), 0) AS [INV. BEG],
                    ISNULL(SUM(CASE WHEN il.transaction_type='Consumed' THEN il.quantity ELSE 0 END),0) AS [CONSUMED],
                    ISNULL(SUM(CASE WHEN il.transaction_type='Pull-Out'  THEN il.quantity ELSE 0 END),0) AS [PULL-OUT],
                    ISNULL(SUM(vb.current_stock), 0) AS [INV. END]
                FROM dbo.VaccineBatch vb
                INNER JOIN dbo.Vaccine v ON vb.vaccine_id = v.vaccine_id
                LEFT JOIN dbo.InventoryLog il
                    ON vb.batch_id = il.batch_id
                   AND il.transaction_date >= @from
                   AND il.transaction_date <  DATEADD(DAY,1,@to)
                WHERE v.is_active = 'Yes'
                GROUP BY v.vaccine_name, v.vaccine_category
                ORDER BY v.vaccine_name";

            DataTable dt = ExecuteTable(sql, from, to);
            if (dt.Rows.Count == 0) { lblMessage.Text = "No data to export."; return; }
            ExportToExcel(dt, "Daily Inventory Summary", from, to);
        }

        // ── PDF Export (Bite Case only) ───────────────────────────────
        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfSelectedCaseId.Value))
            {
                lblMessage.Text = "Please select a case first.";
                return;
            }

            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);

            // Build HTML string directly — no URL fetch needed
            string html = BuildCaseReportHtml(caseId);

            SelectPdf.HtmlToPdf converter = new SelectPdf.HtmlToPdf();
            converter.Options.PdfPageSize = SelectPdf.PdfPageSize.A4;
            converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
            converter.Options.MarginTop = 20;
            converter.Options.MarginBottom = 20;
            converter.Options.MarginLeft = 20;
            converter.Options.MarginRight = 20;

            SelectPdf.PdfDocument doc = converter.ConvertHtmlString(html);

            string fileName = "CaseReport_" + hfSelectedCaseId.Value
                              + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
            doc.Save(Response.OutputStream);
            doc.Close();
            Response.End();
        }

        private string BuildCaseReportHtml(int caseId)
        {
            // Re-fetch all case data fresh for PDF
            string patientId = "";
            string caseNo = "", patientName = "", dob = "", age = "", gender = "",
                   civilStatus = "", contact = "", occupation = "", address = "",
                   emergencyContact = "",
                   biteDate = "", biteTime = "", bitePlace = "", exposureType = "",
                   woundType = "", biteSite = "", bleeding = "", category = "",
                   washed = "", animalType = "", ownership = "", animalStatus = "",
                   circumstances = "",
                   day14Status = "", followupDate = "", followupNotes = "",
                   bp = "", temp = "", weight = "", cr = "", diagnosis = "", symptoms = "",
                   regimenType = "", vaccineName = "", manufacturer = "",
                   regimenStart = "", totalDoses = "", regimenStatus = "";

            DataTable dtSched = new DataTable();
            DataTable dtTreat = new DataTable();
            string hadPrevVacc = "No record", prevType = "—", prevBrand = "—",
                   prevDoseStatus = "—", prevVaccDate = "—";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string caseQuery = @"
            SELECT
                p.patient_id,
                p.lname + ', ' + p.fname AS full_name,
                CONVERT(VARCHAR, p.date_of_birth, 107) AS date_of_birth,
                DATEDIFF(YEAR, p.date_of_birth,
                    CASE WHEN DATEADD(YEAR,DATEDIFF(YEAR,p.date_of_birth,GETDATE()),p.date_of_birth) > GETDATE()
                         THEN DATEADD(YEAR,DATEDIFF(YEAR,p.date_of_birth,GETDATE())-1,p.date_of_birth)
                         ELSE GETDATE() END) AS age,
                CASE p.gender WHEN 'M' THEN 'Male' WHEN 'F' THEN 'Female' ELSE p.gender END AS gender,
                p.civil_status, p.contact_no, p.occupation, p.address AS full_address,
                ISNULL(ec.emergency_contact_person,'N/A') + ' — ' + ISNULL(ec.emergency_contact_number,'') AS emergency_contact,
                c.case_no,
                CONVERT(VARCHAR, c.date_of_bite, 107) AS bite_date,
                CONVERT(VARCHAR, c.time_of_bite, 100) AS bite_time,
                c.place_of_bite, c.type_of_exposure, c.wound_type, c.site_of_bite,
                c.bleeding, c.category, c.washed,
                a.animal_type, a.ownership, a.animal_status, a.circumstances,
                ISNULL(af.day14_status,'—') AS day14_status,
                ISNULL(CONVERT(VARCHAR,af.followup_date,107),'—') AS followup_date,
                ISNULL(af.notes,'—') AS followup_notes,
                ISNULL(vs.blood_pressure,'—') AS blood_pressure,
                ISNULL(CAST(vs.temperature AS VARCHAR),'—') AS temperature,
                ISNULL(CAST(vs.wt AS VARCHAR),'—') AS weight,
                ISNULL(vs.cr,'—') AS capillary_refill,
                ISNULL(vi.diagnosis,'—') AS diagnosis,
                ISNULL(vi.manifestation_notes,'—') AS manifestation_notes,
                ISNULL(vr.regimen_type,'—') AS regimen_type,
                ISNULL(CONVERT(VARCHAR,vr.start_date,107),'—') AS regimen_start,
                ISNULL(CAST(vr.total_doses AS VARCHAR),'—') AS total_doses,
                ISNULL(vr.status,'—') AS regimen_status,
                ISNULL(vc.vaccine_name,'—') AS vaccine_name,
                ISNULL(vc.manufacturer,'—') AS manufacturer
            FROM dbo.[Case] c
            INNER JOIN dbo.Patient p        ON c.patient_id   = p.patient_id
            LEFT JOIN dbo.EmergencyContact ec ON p.patient_id = ec.patient_id
            LEFT JOIN dbo.Animal a           ON a.case_id     = c.case_id
            LEFT JOIN dbo.AnimalFollowUp af  ON af.animal_id  = a.animal_id
            LEFT JOIN dbo.VitalSigns vs      ON vs.patient_id = p.patient_id
            LEFT JOIN dbo.Visit vi           ON vi.case_id    = c.case_id AND vi.visit_type = 'Initial Visit'
            LEFT JOIN dbo.VaccineRegimen vr  ON vr.case_id    = c.case_id
            LEFT JOIN dbo.Vaccine vc         ON vc.vaccine_id = vr.vaccine_id
            WHERE c.case_id = @caseId";

                using (SqlCommand cmd = new SqlCommand(caseQuery, conn))
                {
                    cmd.Parameters.AddWithValue("@caseId", caseId);
                    using (SqlDataReader rdr = cmd.ExecuteReader())
                    {
                        if (rdr.Read())
                        {
                            patientId = rdr["patient_id"].ToString();
                            patientName = rdr["full_name"].ToString();
                            dob = rdr["date_of_birth"].ToString();
                            age = rdr["age"].ToString() + " yrs old";
                            gender = rdr["gender"].ToString();
                            civilStatus = rdr["civil_status"].ToString();
                            contact = rdr["contact_no"].ToString();
                            occupation = rdr["occupation"].ToString();
                            address = rdr["full_address"].ToString();
                            emergencyContact = rdr["emergency_contact"].ToString();
                            caseNo = rdr["case_no"].ToString();
                            biteDate = rdr["bite_date"].ToString();
                            biteTime = rdr["bite_time"].ToString();
                            bitePlace = rdr["place_of_bite"].ToString();
                            exposureType = rdr["type_of_exposure"].ToString();
                            woundType = rdr["wound_type"].ToString();
                            biteSite = rdr["site_of_bite"].ToString();
                            bleeding = rdr["bleeding"].ToString();
                            category = "Category " + rdr["category"].ToString();
                            washed = rdr["washed"].ToString();
                            animalType = rdr["animal_type"].ToString();
                            ownership = rdr["ownership"].ToString();
                            animalStatus = rdr["animal_status"].ToString();
                            circumstances = rdr["circumstances"].ToString();
                            day14Status = rdr["day14_status"].ToString();
                            followupDate = rdr["followup_date"].ToString();
                            followupNotes = rdr["followup_notes"].ToString();
                            bp = rdr["blood_pressure"].ToString();
                            temp = rdr["temperature"].ToString() + " °C";
                            weight = rdr["weight"].ToString() + " kg";
                            cr = rdr["capillary_refill"].ToString();
                            diagnosis = rdr["diagnosis"].ToString();
                            symptoms = rdr["manifestation_notes"].ToString();
                            regimenType = rdr["regimen_type"].ToString();
                            vaccineName = rdr["vaccine_name"].ToString();
                            manufacturer = rdr["manufacturer"].ToString();
                            regimenStart = rdr["regimen_start"].ToString();
                            totalDoses = rdr["total_doses"].ToString();
                            regimenStatus = rdr["regimen_status"].ToString();
                        }
                    }
                }

                // Override symptoms from Manifestation table if available
                object manifestResult = new SqlCommand(
                    "SELECT STRING_AGG(symptom,', ') FROM dbo.Manifestation WHERE case_id=@caseId", conn)
                { Parameters = { new SqlParameter("@caseId", caseId) } }.ExecuteScalar();
                if (manifestResult != null && manifestResult != DBNull.Value && manifestResult.ToString() != "")
                    symptoms = manifestResult.ToString();

                // Schedule
                dtSched = ExecuteTableById(@"
            SELECT sd.dose_number AS [Dose],
                   CONVERT(VARCHAR,sd.schedule_date,107) AS [Scheduled Date],
                   sd.status AS [Status],
                   ISNULL(vc.vaccine_name,'—') AS [Vaccine],
                   ISNULL(vb.batch_number,'—') AS [Batch No.],
                   ISNULL(t.administered_by,'—') AS [Administered By],
                   ISNULL(CAST(t.dosage AS VARCHAR)+' '+ISNULL(t.unit,''),'—') AS [Dosage],
                   ISNULL(t.route,'—') AS [Route]
            FROM dbo.ScheduledDose sd
            INNER JOIN dbo.VaccineRegimen vr ON sd.regimen_id = vr.regimen_id
            LEFT JOIN dbo.Vaccine      vc ON sd.vaccine_id = vc.vaccine_id
            LEFT JOIN dbo.VaccineBatch vb ON sd.batch_id   = vb.batch_id
            LEFT JOIN dbo.Treatment     t ON sd.visit_id   = t.visit_id
            WHERE vr.case_id = @caseId
            ORDER BY sd.dose_number ASC", caseId, conn);

                // Treatment
                dtTreat = ExecuteTableById(@"
            SELECT CONVERT(VARCHAR,vi.visit_date,107) AS [Visit Date],
                   vi.visit_type AS [Visit Type],
                   ISNULL('Day '+CAST(vi.dose_day AS VARCHAR),'—') AS [Dose Day],
                   ISNULL(vc.vaccine_name,'—') AS [Vaccine Used],
                   ISNULL(CAST(t.dosage AS VARCHAR)+' '+ISNULL(t.unit,''),'—') AS [Dosage],
                   ISNULL(t.route,'—') AS [Route],
                   ISNULL(t.administered_by,'—') AS [Administered By]
            FROM dbo.Visit vi
            LEFT JOIN dbo.Treatment t  ON t.visit_id   = vi.visit_id
            LEFT JOIN dbo.Vaccine   vc ON t.vaccine_id = vc.vaccine_id
            WHERE vi.case_id = @caseId
            ORDER BY vi.visit_date ASC", caseId, conn);

                // Previous vaccine history
                using (SqlCommand prevCmd = new SqlCommand(@"
            SELECT TOP 1
                CASE had_previous_vaccine WHEN 'Yes' THEN 'Yes' ELSE 'No' END AS had,
                ISNULL(previous_vaccine_type,'—') AS pvt,
                ISNULL(previous_brand,'—') AS pb,
                ISNULL(dose_status,'—') AS ds,
                ISNULL(CONVERT(VARCHAR,vaccination_date,107),'—') AS vd
            FROM dbo.PreviousVaccineHistory WHERE patient_id=@pid", conn))
                {
                    prevCmd.Parameters.AddWithValue("@pid", patientId);
                    using (SqlDataReader pr = prevCmd.ExecuteReader())
                    {
                        if (pr.Read())
                        {
                            hadPrevVacc = pr["had"].ToString();
                            prevType = pr["pvt"].ToString();
                            prevBrand = pr["pb"].ToString();
                            prevDoseStatus = pr["ds"].ToString();
                            prevVaccDate = pr["vd"].ToString();
                        }
                    }
                }
            }

            // ── Build HTML ─────────────────────────────────────────────────
            var sb = new System.Text.StringBuilder();
            sb.Append(@"<!DOCTYPE html>
<html>
<head>
<meta charset='utf-8'/>
<style>
  body { font-family: Arial, sans-serif; font-size: 11px; color: #1e293b; margin: 0; padding: 0; }
  h1   { font-size: 16px; font-weight: 800; color: #0b2a7a; margin: 0 0 2px; }
  h2   { font-size: 11px; font-weight: 700; margin: 0; color: #475569; }
  .header { padding: 12px 16px; border-bottom: 3px solid #0b2a7a; margin-bottom: 14px; }
  .section { margin-bottom: 14px; border: 1px solid #e2e8f0; border-radius: 6px; overflow: hidden; }
  .section-head { padding: 7px 12px; font-size: 11px; font-weight: 800; letter-spacing: .5px; text-transform: uppercase; }
  .section-body { padding: 10px 12px; }
  .grid { display: grid; grid-template-columns: repeat(3,1fr); gap: 6px 20px; }
  .grid .span2 { grid-column: span 2; }
  .grid .span3 { grid-column: span 3; }
  .field { margin: 0; }
  .label { font-weight: 700; color: #64748b; }
  .val   { color: #1e293b; }
  table  { width:100%; border-collapse: collapse; font-size: 10px; }
  th     { background: #f1f5f9; color: #475569; font-weight: 700; text-align: left;
           padding: 5px 8px; border-bottom: 1px solid #cbd5e1; text-transform: uppercase; font-size:9px; }
  td     { padding: 4px 8px; border-bottom: 1px solid #f1f5f9; }
  tr:nth-child(even) td { background: #f8fafc; }
  .blue  { background:#0b2a7a; color:#fff; }
  .red   { background:#fef2f2; color:#b91c1c; }
  .green { background:#f0fdf4; color:#15803d; }
  .indigo{ background:#eef2ff; color:#3730a3; }
  .slate { background:#334155; color:#fff; }
  .orange{ background:#fff7ed; color:#c2410c; }
  .teal  { background:#f0fdfa; color:#0f766e; }
  .purple{ background:#faf5ff; color:#7e22ce; }
  .badge { display:inline-block; background:#fee2e2; color:#b91c1c;
           padding:1px 7px; border-radius:999px; font-weight:700; font-size:10px; }
</style>
</head>
<body>
<div class='header'>
  <h1>SBI Medical Animal Bite Center — Morong Branch</h1>
  <h2>Bite Case Report &nbsp;|&nbsp; Case No: " + H(caseNo) + @" &nbsp;|&nbsp; Generated: " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt") + @"</h2>
</div>");

            // Section helper
            void Section(string colorClass, string title, string bodyHtml)
            {
                sb.Append("<div class='section'>");
                sb.Append("<div class='section-head " + colorClass + "'>" + title + "</div>");
                sb.Append("<div class='section-body'>" + bodyHtml + "</div>");
                sb.Append("</div>");
            }

            string F(string label, string val, string span = "")
                => "<p class='field " + span + "'><span class='label'>" + H(label) + ": </span><span class='val'>" + H(val) + "</span></p>";

            // I. Patient Demographics
            Section("blue", "I. Patient Demographics",
                "<div class='grid'>" +
                F("Patient ID", patientId) +
                F("Full Name", patientName) +
                F("Date of Birth", dob) +
                F("Age", age) +
                F("Gender", gender) +
                F("Civil Status", civilStatus) +
                F("Contact No", contact) +
                F("Occupation", occupation) +
                F("Address", address, "span3") +
                F("Emergency Contact", emergencyContact, "span3") +
                "</div>");

            // II. Bite Incident
            Section("red", "II. Bite Incident Details",
                "<div class='grid'>" +
                F("Case Number", caseNo) +
                F("Date of Bite", biteDate) +
                F("Time of Bite", biteTime) +
                F("Place of Bite", bitePlace) +
                F("Type of Exposure", exposureType) +
                F("Wound Type", woundType) +
                F("Site of Bite", biteSite) +
                F("Bleeding", bleeding) +
                "<p class='field'><span class='label'>Category: </span><span class='badge'>" + H(category) + "</span></p>" +
                F("Washed", washed) +
                F("Animal Type", animalType) +
                F("Ownership", ownership) +
                F("Animal Status", animalStatus) +
                F("Circumstances", circumstances, "span3") +
                "</div>");

            // III. Medical Assessment
            Section("green", "III. Medical Assessment",
                "<div class='grid'>" +
                F("Blood Pressure", bp) +
                F("Temperature", temp) +
                F("Weight", weight) +
                F("Capillary Refill", cr) +
                F("Diagnosis", diagnosis, "span3") +
                F("Manifestation", symptoms, "span3") +
                "</div>");

            // IV. Vaccination Regimen
            Section("indigo", "IV. Vaccination Regimen",
                "<div class='grid'>" +
                F("Regimen Type", regimenType) +
                F("Vaccine", vaccineName) +
                F("Manufacturer", manufacturer) +
                F("Start Date", regimenStart) +
                F("Total Doses", totalDoses) +
                F("Status", regimenStatus) +
                "</div>");

            // V. Vaccination Schedule
            sb.Append("<div class='section'><div class='section-head slate'>V. Vaccination Schedule &amp; Administration</div><div class='section-body'>");
            sb.Append(DataTableToHtmlTable(dtSched));
            sb.Append("</div></div>");

            // VI. Animal Follow-Up
            Section("orange", "VI. Animal Observation &amp; Follow-Up",
                "<div class='grid'>" +
                F("Animal Status", animalStatus) +
                F("Day-14 Status", day14Status) +
                F("Follow-up Date", followupDate) +
                F("Notes", followupNotes, "span3") +
                "</div>");

            // VII. Treatment Summary
            sb.Append("<div class='section'><div class='section-head teal'>VII. Treatment Summary</div><div class='section-body'>");
            sb.Append(DataTableToHtmlTable(dtTreat));
            sb.Append("</div></div>");

            // VIII. Previous Vaccine History
            Section("purple", "VIII. Previous Vaccine History",
                "<div class='grid'>" +
                F("Had Previous Vaccine", hadPrevVacc) +
                F("Previous Type", prevType) +
                F("Brand", prevBrand) +
                F("Dose Status", prevDoseStatus) +
                F("Vaccination Date", prevVaccDate) +
                "</div>");

            sb.Append("</body></html>");
            return sb.ToString();
        }

        /// <summary>HTML-encode helper.</summary>
        private string H(string s)
            => System.Web.HttpUtility.HtmlEncode(s ?? "—");

        /// <summary>Converts a DataTable to a simple HTML table string.</summary>
        private string DataTableToHtmlTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<p style='color:#94a3b8;font-style:italic;'>No records found.</p>";

            var sb = new System.Text.StringBuilder();
            sb.Append("<table><tr>");
            foreach (DataColumn col in dt.Columns)
                sb.Append("<th>" + H(col.ColumnName) + "</th>");
            sb.Append("</tr>");
            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn col in dt.Columns)
                    sb.Append("<td>" + H(row[col]?.ToString()) + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        // ── Excel Helper ──────────────────────────────────────────────
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
                string fn = "Daily_Inventory_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                Response.Clear();
                Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                Response.AddHeader("content-disposition", "attachment; filename=" + fn);
                Response.BinaryWrite(pkg.GetAsByteArray());
                Response.End();
            }
        }

        // ── Helpers ───────────────────────────────────────────────────
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

        private DateTime? ParseNullableDate(string value)
        {
            DateTime d;
            return DateTime.TryParse(value, out d) ? (DateTime?)d.Date : null;
        }

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