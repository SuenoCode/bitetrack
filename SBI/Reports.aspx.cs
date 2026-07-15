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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            string role = Session["userRole"].ToString().ToUpper();
            if (role != "A" && role != "B" && role != "C")
            {
                Response.Redirect("Login.aspx");
                return;
            }

            // Handle PDF export render request - Direct PDF generation
            string exportCaseId = Request.QueryString["exportCaseId"];
            if (!string.IsNullOrEmpty(exportCaseId))
            {
                int caseId;
                if (int.TryParse(exportCaseId, out caseId))
                {
                    GenerateAndExportPdf(caseId);
                    return;
                }
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

        // ── Generate PDF Directly ─────────────────────────────────────
        private void GenerateAndExportPdf(int caseId)
        {
            try
            {
                // Build the HTML for the PDF
                string html = BuildCaseReportHtml(caseId);

                // Create PDF using SelectPdf
                SelectPdf.HtmlToPdf converter = new SelectPdf.HtmlToPdf();
                converter.Options.PdfPageSize = SelectPdf.PdfPageSize.A4;
                converter.Options.PdfPageOrientation = SelectPdf.PdfPageOrientation.Portrait;
                converter.Options.MarginTop = 20;
                converter.Options.MarginBottom = 20;
                converter.Options.MarginLeft = 20;
                converter.Options.MarginRight = 20;

                SelectPdf.PdfDocument doc = converter.ConvertHtmlString(html);

                string fileName = "CaseReport_" + caseId.ToString() + "_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".pdf";

                Response.Clear();
                Response.ContentType = "application/pdf";
                Response.AddHeader("content-disposition", "attachment; filename=" + fileName);
                doc.Save(Response.OutputStream);
                doc.Close();
                Response.End();
            }
            catch (Exception ex)
            {
                Response.Clear();
                Response.ContentType = "text/html";
                Response.Write("Error generating PDF: " + ex.Message);
                Response.End();
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
            LoadInventoryReport(DateTime.Today, DateTime.Today);
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

        // ── Case List Grid RowCommand ─────────────────────────────────
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

        protected void gvCaseList_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvCaseList.PageIndex = e.NewPageIndex;
            BindCaseList(txtCaseSearch.Text.Trim(),
                ParseNullableDate(txtCaseFromDate.Text),
                ParseNullableDate(txtCaseToDate.Text));
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
                    v.vaccine_name AS [Vaccine],
                    v.vaccine_category AS [Category],
                    ISNULL(SUM(vb.quantity_received), 0) AS [INV. BEG],
                    ISNULL(SUM(CASE WHEN il.transaction_type = 'Dispensed' THEN il.quantity ELSE 0 END), 0) AS [CONSUMED],
                    ISNULL(SUM(CASE WHEN il.transaction_type = 'Expired' THEN il.quantity ELSE 0 END), 0) AS [PULL-OUT],
                    ISNULL(SUM(vb.current_stock), 0) AS [INV. END]
                FROM dbo.VaccineBatch vb
                INNER JOIN dbo.Vaccine v ON vb.vaccine_id = v.vaccine_id
                LEFT JOIN dbo.InventoryLog il
                    ON vb.batch_id = il.batch_id
                   AND il.transaction_date >= @from
                   AND il.transaction_date < DATEADD(DAY, 1, @to)
                WHERE v.is_active = 'Yes'
                GROUP BY v.vaccine_name, v.vaccine_category
                ORDER BY v.vaccine_name ASC";

            DataTable dt = ExecuteTable(sql, from, to);
            gvInventory.DataSource = dt;
            gvInventory.DataBind();
            lblTotalRecords.Text = dt.Rows.Count.ToString();

            // Store the current data for Excel export
            ViewState["InventoryReportData"] = dt;
        }

        // ── Excel Export ──────────────────────────────────────────────
        protected void btnExportExcel_Click(object sender, EventArgs e)
        {
            DateTime from, to;
            if (!TryGetDates(out from, out to))
            {
                lblMessage.Text = "Please select valid date range.";
                return;
            }

            // Get the data - either from ViewState or fresh from database
            DataTable dt = ViewState["InventoryReportData"] as DataTable;
            if (dt == null || dt.Rows.Count == 0)
            {
                string sql = @"
                    SELECT
                        v.vaccine_name AS [Vaccine],
                        v.vaccine_category AS [Category],
                        ISNULL(SUM(vb.quantity_received), 0) AS [INV. BEG],
                        ISNULL(SUM(CASE WHEN il.transaction_type = 'Dispensed' THEN il.quantity ELSE 0 END), 0) AS [CONSUMED],
                        ISNULL(SUM(CASE WHEN il.transaction_type = 'Expired' THEN il.quantity ELSE 0 END), 0) AS [PULL-OUT],
                        ISNULL(SUM(vb.current_stock), 0) AS [INV. END]
                    FROM dbo.VaccineBatch vb
                    INNER JOIN dbo.Vaccine v ON vb.vaccine_id = v.vaccine_id
                    LEFT JOIN dbo.InventoryLog il
                        ON vb.batch_id = il.batch_id
                       AND il.transaction_date >= @from
                       AND il.transaction_date < DATEADD(DAY, 1, @to)
                    WHERE v.is_active = 'Yes'
                    GROUP BY v.vaccine_name, v.vaccine_category
                    ORDER BY v.vaccine_name ASC";

                dt = ExecuteTable(sql, from, to);
            }

            if (dt == null || dt.Rows.Count == 0)
            {
                lblMessage.Text = "No data to export.";
                return;
            }

            ExportToExcel(dt, "Daily Inventory Summary", from, to);
        }

        // ── Bite Case Report ──────────────────────────────────────────
        private void LoadBiteCaseReport(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string caseQuery = @"
                    SELECT
                        p.patient_id,
                        p.fname + ' ' + p.lname AS full_name,
                        CONVERT(VARCHAR, p.date_of_birth, 107) AS date_of_birth,
                        DATEDIFF(YEAR, p.date_of_birth, GETDATE()) AS age,
                        CASE p.gender WHEN 'M' THEN 'Male' WHEN 'F' THEN 'Female' ELSE p.gender END AS gender,
                        p.civil_status,
                        p.contact_no,
                        p.occupation,
                        ISNULL(p.house_no, '') + ' ' + ISNULL(p.street, '') + ', ' + 
                        ISNULL(p.barangay, '') + ', ' + ISNULL(p.city_province, '') AS full_address,
                        ISNULL(p.emergency_contact_person, 'N/A') + ' - ' 
                            + ISNULL(p.emergency_contact_number, '') AS emergency_contact,
                        c.case_id,
                        c.case_no,
                        CONVERT(VARCHAR, c.date_of_bite, 107) AS bite_date,
                        CONVERT(VARCHAR, c.time_of_bite, 100) AS bite_time,
                        ISNULL(c.bite_house_no, '') + ' ' + ISNULL(c.bite_street, '') + ', ' + 
                        ISNULL(c.bite_barangay, '') + ', ' + ISNULL(c.bite_city, '') AS place_of_bite,
                        c.type_of_exposure,
                        c.wound_type,
                        c.site_of_bite,
                        c.bleeding,
                        c.category,
                        c.washed,
                        c.animal_type,
                        c.ownership,
                        c.animal_status,
                        c.circumstances,
                        c.day14_status,
                        CONVERT(VARCHAR, c.followup_date, 107) AS followup_date,
                        c.followup_notes,
                        vs.blood_pressure,
                        CAST(vs.temperature AS VARCHAR(20)) AS temperature,
                        CAST(vs.wt AS VARCHAR(20)) AS weight,
                        vs.cr AS capillary_refill,
                        vi.diagnosis,
                        vi.manifestation_notes,
                        vr.regimen_type,
                        CONVERT(VARCHAR, vr.start_date, 107) AS regimen_start,
                        vr.total_doses,
                        vr.status AS regimen_status,
                        vc.vaccine_name,
                        vc.manufacturer
                    FROM dbo.[Case] c
                    INNER JOIN dbo.Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN dbo.VitalSigns vs ON vs.patient_id = p.patient_id
                    LEFT JOIN dbo.Visit vi ON vi.case_id = c.case_id AND vi.visit_type = 'Initial Visit'
                    LEFT JOIN dbo.VaccineRegimen vr ON vr.case_id = c.case_id
                    LEFT JOIN dbo.Vaccine vc ON vc.vaccine_id = vr.vaccine_id
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
                    lblDay14Status.Text = rdr["day14_status"] == DBNull.Value ? "-" : rdr["day14_status"].ToString();
                    lblFollowupDate.Text = rdr["followup_date"] == DBNull.Value ? "-" : rdr["followup_date"].ToString();
                    lblFollowupNotes.Text = rdr["followup_notes"] == DBNull.Value ? "-" : rdr["followup_notes"].ToString();

                    lblBP.Text = rdr["blood_pressure"] == DBNull.Value ? "-" : rdr["blood_pressure"].ToString();
                    lblTemp.Text = rdr["temperature"] == DBNull.Value ? "-" : rdr["temperature"].ToString() + " C";
                    lblWeight.Text = rdr["weight"] == DBNull.Value ? "-" : rdr["weight"].ToString() + " kg";
                    lblCR.Text = rdr["capillary_refill"] == DBNull.Value ? "-" : rdr["capillary_refill"].ToString();
                    lblDiagnosis.Text = rdr["diagnosis"] == DBNull.Value ? "-" : rdr["diagnosis"].ToString();
                    lblSymptoms.Text = rdr["manifestation_notes"] == DBNull.Value ? "-" : rdr["manifestation_notes"].ToString();

                    lblRegimenType.Text = rdr["regimen_type"] == DBNull.Value ? "-" : rdr["regimen_type"].ToString();
                    lblVaccineName.Text = rdr["vaccine_name"] == DBNull.Value ? "-" : rdr["vaccine_name"].ToString();
                    lblManufacturer.Text = rdr["manufacturer"] == DBNull.Value ? "-" : rdr["manufacturer"].ToString();
                    lblRegimenStart.Text = rdr["regimen_start"] == DBNull.Value ? "-" : rdr["regimen_start"].ToString();
                    lblTotalDoses.Text = rdr["total_doses"] == DBNull.Value ? "-" : rdr["total_doses"].ToString();
                    lblRegimenStatus.Text = rdr["regimen_status"] == DBNull.Value ? "-" : rdr["regimen_status"].ToString();
                }
                else
                {
                    lblMessage.Text = "Case not found.";
                    rdr.Close();
                    return;
                }
                rdr.Close();

                // Vaccination Schedule
                string schedSql = @"
                    SELECT
                        sd.dose_number AS [Dose],
                        CONVERT(VARCHAR, sd.schedule_date, 107) AS [Scheduled Date],
                        sd.status AS [Status],
                        ISNULL(vc.vaccine_name, '-') AS [Vaccine],
                        ISNULL(vb.batch_number, '-') AS [Batch No.],
                        ISNULL(u.fname + ' ' + u.lname, '-') AS [Administered By],
                        ISNULL(CAST(sd.dosage AS VARCHAR), '-') AS [Dosage],
                        ISNULL(sd.route, '-') AS [Route]
                    FROM dbo.ScheduledDose sd
                    INNER JOIN dbo.VaccineRegimen vr ON sd.regimen_id = vr.regimen_id
                    LEFT JOIN dbo.Vaccine vc ON sd.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.VaccineBatch vb ON sd.batch_id = vb.batch_id
                    LEFT JOIN dbo.AppUser u ON sd.administered_by_user = u.user_id
                    WHERE vr.case_id = @caseId
                    ORDER BY sd.dose_number ASC";

                DataTable dtSched = ExecuteTableById(schedSql, caseId, conn);
                gvSchedule.DataSource = dtSched;
                gvSchedule.DataBind();

                // Treatment Summary
                string treatSql = @"
                    SELECT
                        CONVERT(VARCHAR, vi.visit_date, 107) AS [Visit Date],
                        vi.visit_type AS [Visit Type],
                        ISNULL('Day ' + CAST(vi.dose_day AS VARCHAR), '-') AS [Dose Day],
                        ISNULL(vc.vaccine_name, '-') AS [Vaccine Used],
                        ISNULL(u.fname + ' ' + u.lname, '-') AS [Administered By]
                    FROM dbo.Visit vi
                    LEFT JOIN dbo.ScheduledDose sd ON sd.visit_id = vi.visit_id
                    LEFT JOIN dbo.Vaccine vc ON sd.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.AppUser u ON sd.administered_by_user = u.user_id
                    WHERE vi.case_id = @caseId
                    ORDER BY vi.visit_date ASC";

                DataTable dtTreat = ExecuteTableById(treatSql, caseId, conn);
                gvTreatment.DataSource = dtTreat;
                gvTreatment.DataBind();

                // Previous Vaccine History
                string prevSql = @"
                    SELECT TOP 1
                        CASE had_previous_vaccine WHEN 'Yes' THEN 'Yes' ELSE 'No' END AS [Had Previous Vaccine],
                        ISNULL(previous_vaccine_type, '-') AS previous_vaccine_type,
                        ISNULL(previous_brand, '-') AS previous_brand,
                        ISNULL(dose_status, '-') AS dose_status,
                        ISNULL(CONVERT(VARCHAR, vaccination_date, 107), '-') AS vaccination_date
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
                    lblPrevDoseStatus.Text = lblPrevVaccDate.Text = "-";
                }
                prevRdr.Close();
            }
        }

        // ── Build Case Report HTML for PDF ───────────────────────────
        private string BuildCaseReportHtml(int caseId)
        {
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
            string hadPrevVacc = "No record", prevType = "-", prevBrand = "-",
                   prevDoseStatus = "-", prevVaccDate = "-";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                string caseQuery = @"
                    SELECT
                        p.patient_id,
                        p.fname + ' ' + p.lname AS full_name,
                        CONVERT(VARCHAR, p.date_of_birth, 107) AS date_of_birth,
                        DATEDIFF(YEAR, p.date_of_birth, GETDATE()) AS age,
                        CASE p.gender WHEN 'M' THEN 'Male' WHEN 'F' THEN 'Female' ELSE p.gender END AS gender,
                        p.civil_status,
                        p.contact_no,
                        p.occupation,
                        ISNULL(p.house_no, '') + ' ' + ISNULL(p.street, '') + ', ' + 
                        ISNULL(p.barangay, '') + ', ' + ISNULL(p.city_province, '') AS full_address,
                        ISNULL(p.emergency_contact_person, 'N/A') + ' - ' 
                            + ISNULL(p.emergency_contact_number, '') AS emergency_contact,
                        c.case_no,
                        CONVERT(VARCHAR, c.date_of_bite, 107) AS bite_date,
                        CONVERT(VARCHAR, c.time_of_bite, 100) AS bite_time,
                        ISNULL(c.bite_house_no, '') + ' ' + ISNULL(c.bite_street, '') + ', ' + 
                        ISNULL(c.bite_barangay, '') + ', ' + ISNULL(c.bite_city, '') AS place_of_bite,
                        c.type_of_exposure,
                        c.wound_type,
                        c.site_of_bite,
                        c.bleeding,
                        c.category,
                        c.washed,
                        c.animal_type,
                        c.ownership,
                        c.animal_status,
                        c.circumstances,
                        ISNULL(c.day14_status, '-') AS day14_status,
                        ISNULL(CONVERT(VARCHAR, c.followup_date, 107), '-') AS followup_date,
                        ISNULL(c.followup_notes, '-') AS followup_notes,
                        ISNULL(vs.blood_pressure, '-') AS blood_pressure,
                        ISNULL(CAST(vs.temperature AS VARCHAR), '-') AS temperature,
                        ISNULL(CAST(vs.wt AS VARCHAR), '-') AS weight,
                        ISNULL(vs.cr, '-') AS capillary_refill,
                        ISNULL(vi.diagnosis, '-') AS diagnosis,
                        ISNULL(vi.manifestation_notes, '-') AS manifestation_notes,
                        ISNULL(vr.regimen_type, '-') AS regimen_type,
                        ISNULL(CONVERT(VARCHAR, vr.start_date, 107), '-') AS regimen_start,
                        ISNULL(CAST(vr.total_doses AS VARCHAR), '-') AS total_doses,
                        ISNULL(vr.status, '-') AS regimen_status,
                        ISNULL(vc.vaccine_name, '-') AS vaccine_name,
                        ISNULL(vc.manufacturer, '-') AS manufacturer
                    FROM dbo.[Case] c
                    INNER JOIN dbo.Patient p ON c.patient_id = p.patient_id
                    LEFT JOIN dbo.VitalSigns vs ON vs.patient_id = p.patient_id
                    LEFT JOIN dbo.Visit vi ON vi.case_id = c.case_id AND vi.visit_type = 'Initial Visit'
                    LEFT JOIN dbo.VaccineRegimen vr ON vr.case_id = c.case_id
                    LEFT JOIN dbo.Vaccine vc ON vc.vaccine_id = vr.vaccine_id
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
                            temp = rdr["temperature"].ToString() + " C";
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

                // Schedule
                string schedSql = @"
                    SELECT
                        sd.dose_number AS [Dose],
                        CONVERT(VARCHAR, sd.schedule_date, 107) AS [Scheduled Date],
                        sd.status AS [Status],
                        ISNULL(vc.vaccine_name, '-') AS [Vaccine],
                        ISNULL(vb.batch_number, '-') AS [Batch No.],
                        ISNULL(u.fname + ' ' + u.lname, '-') AS [Administered By],
                        ISNULL(CAST(sd.dosage AS VARCHAR), '-') AS [Dosage],
                        ISNULL(sd.route, '-') AS [Route]
                    FROM dbo.ScheduledDose sd
                    INNER JOIN dbo.VaccineRegimen vr ON sd.regimen_id = vr.regimen_id
                    LEFT JOIN dbo.Vaccine vc ON sd.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.VaccineBatch vb ON sd.batch_id = vb.batch_id
                    LEFT JOIN dbo.AppUser u ON sd.administered_by_user = u.user_id
                    WHERE vr.case_id = @caseId
                    ORDER BY sd.dose_number ASC";

                dtSched = ExecuteTableById(schedSql, caseId, conn);

                // Treatment
                string treatSql = @"
                    SELECT
                        CONVERT(VARCHAR, vi.visit_date, 107) AS [Visit Date],
                        vi.visit_type AS [Visit Type],
                        ISNULL('Day ' + CAST(vi.dose_day AS VARCHAR), '-') AS [Dose Day],
                        ISNULL(vc.vaccine_name, '-') AS [Vaccine Used],
                        ISNULL(u.fname + ' ' + u.lname, '-') AS [Administered By]
                    FROM dbo.Visit vi
                    LEFT JOIN dbo.ScheduledDose sd ON sd.visit_id = vi.visit_id
                    LEFT JOIN dbo.Vaccine vc ON sd.vaccine_id = vc.vaccine_id
                    LEFT JOIN dbo.AppUser u ON sd.administered_by_user = u.user_id
                    WHERE vi.case_id = @caseId
                    ORDER BY vi.visit_date ASC";

                dtTreat = ExecuteTableById(treatSql, caseId, conn);

                // Previous vaccine history
                string prevSql = @"
                    SELECT TOP 1
                        CASE had_previous_vaccine WHEN 'Yes' THEN 'Yes' ELSE 'No' END AS had,
                        ISNULL(previous_vaccine_type, '-') AS pvt,
                        ISNULL(previous_brand, '-') AS pb,
                        ISNULL(dose_status, '-') AS ds,
                        ISNULL(CONVERT(VARCHAR, vaccination_date, 107), '-') AS vd
                    FROM dbo.PreviousVaccineHistory
                    WHERE patient_id = @pid";

                using (SqlCommand prevCmd = new SqlCommand(prevSql, conn))
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

            // Build HTML
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
  .footer { margin-top: 20px; text-align: center; font-size: 9px; color: #94a3b8; border-top: 1px solid #e2e8f0; padding-top: 12px; }
</style>
</head>
<body>
<div class='header'>
  <h1>SBI Medical Animal Bite Center - Morong Branch</h1>
  <h2>Bite Case Report | Case No: " + HtmlEncode(caseNo) + @" | Generated: " + DateTime.Now.ToString("MMM dd, yyyy hh:mm tt") + @"</h2>
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
                => "<p class='field " + span + "'><span class='label'>" + HtmlEncode(label) + ": </span><span class='val'>" + HtmlEncode(val) + "</span></p>";

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
                "<p class='field'><span class='label'>Category: </span><span class='badge'>" + HtmlEncode(category) + "</span></p>" +
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
            sb.Append("<div class='section'><div class='section-head slate'>V. Vaccination Schedule and Administration</div><div class='section-body'>");
            sb.Append(DataTableToHtmlTable(dtSched));
            sb.Append("</div></div>");

            // VI. Animal Follow-Up
            Section("orange", "VI. Animal Observation and Follow-Up",
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

            sb.Append("<div class='footer'>Report generated from SBI Medical Animal Bite Center System</div>");
            sb.Append("</body></html>");
            return sb.ToString();
        }

        private string HtmlEncode(string s)
        {
            return System.Web.HttpUtility.HtmlEncode(s ?? "-");
        }

        private string DataTableToHtmlTable(DataTable dt)
        {
            if (dt == null || dt.Rows.Count == 0)
                return "<p style='color:#94a3b8;font-style:italic;'>No records found.</p>";

            var sb = new System.Text.StringBuilder();
            sb.Append("<table><tr>");
            foreach (DataColumn col in dt.Columns)
                sb.Append("<th>" + HtmlEncode(col.ColumnName) + "</th>");
            sb.Append("</tr>");
            foreach (DataRow row in dt.Rows)
            {
                sb.Append("<tr>");
                foreach (DataColumn col in dt.Columns)
                    sb.Append("<td>" + HtmlEncode(row[col]?.ToString()) + "</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table>");
            return sb.ToString();
        }

        // ── Export to Excel ───────────────────────────────────────────
        private void ExportToExcel(DataTable dt, string title, DateTime from, DateTime to)
        {
            try
            {
                ExcelPackage.License.SetNonCommercialPersonal("SBI Medical");

                using (ExcelPackage pkg = new ExcelPackage())
                {
                    ExcelWorksheet ws = pkg.Workbook.Worksheets.Add("Inventory Report");

                    // Header
                    ws.Cells["A1"].Value = "SBI Medical Animal Bite Center - Morong Branch";
                    ws.Cells["A2"].Value = title;
                    ws.Cells["A3"].Value = "From: " + from.ToString("MMM dd, yyyy") + "   To: " + to.ToString("MMM dd, yyyy");
                    ws.Cells["A4"].Value = "Generated: " + DateTime.Now.ToString("MMM dd, yyyy HH:mm tt");

                    ws.Cells["A1"].Style.Font.Bold = true;
                    ws.Cells["A1"].Style.Font.Size = 14;
                    ws.Cells["A2"].Style.Font.Bold = true;
                    ws.Cells["A2"].Style.Font.Size = 12;
                    ws.Cells["A3"].Style.Font.Italic = true;
                    ws.Cells["A3"].Style.Font.Size = 10;
                    ws.Cells["A4"].Style.Font.Size = 10;

                    // Data table headers
                    int startRow = 6;
                    for (int c = 0; c < dt.Columns.Count; c++)
                    {
                        ws.Cells[startRow, c + 1].Value = dt.Columns[c].ColumnName;
                        ws.Cells[startRow, c + 1].Style.Font.Bold = true;
                        ws.Cells[startRow, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        ws.Cells[startRow, c + 1].Style.Fill.BackgroundColor.SetColor(Color.FromArgb(11, 42, 122));
                        ws.Cells[startRow, c + 1].Style.Font.Color.SetColor(Color.White);
                        ws.Cells[startRow, c + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    }

                    // Data rows
                    for (int r = 0; r < dt.Rows.Count; r++)
                    {
                        for (int c = 0; c < dt.Columns.Count; c++)
                        {
                            object value = dt.Rows[r][c];
                            ws.Cells[r + startRow + 1, c + 1].Value = value != DBNull.Value ? value : "-";
                            ws.Cells[r + startRow + 1, c + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        }
                    }

                    // Auto-fit columns
                    ws.Cells[ws.Dimension.Address].AutoFitColumns();

                    // Add total row
                    int totalRow = startRow + dt.Rows.Count + 1;
                    ws.Cells[totalRow, 1].Value = "TOTAL";
                    ws.Cells[totalRow, 1].Style.Font.Bold = true;
                    ws.Cells[totalRow, 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    ws.Cells[totalRow, 1].Style.Fill.BackgroundColor.SetColor(Color.LightGray);

                    // Export to response
                    string fn = "Daily_Inventory_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx";
                    Response.Clear();
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("content-disposition", "attachment; filename=" + fn);
                    Response.BinaryWrite(pkg.GetAsByteArray());
                    Response.End();
                }
            }
            catch (Exception ex)
            {
                lblMessage.Text = "Error exporting to Excel: " + ex.Message;
            }
        }

        // ── PDF Export Button Handler ────────────────────────────────
        protected void btnExportPdf_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(hfSelectedCaseId.Value))
            {
                lblMessage.Text = "Please select a case first.";
                return;
            }

            int caseId = Convert.ToInt32(hfSelectedCaseId.Value);
            GenerateAndExportPdf(caseId);
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
            {
                lblMessage.Text = "Invalid From Date.";
                return false;
            }
            if (!DateTime.TryParse(txtToDate.Text, out to))
            {
                lblMessage.Text = "Invalid To Date.";
                return false;
            }
            if (from > to)
            {
                lblMessage.Text = "From Date cannot be later than To Date.";
                return false;
            }
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