using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
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

			string role = Session["userRole"].ToString().ToLower();

			if (role != "adminassistant" && role != "vaccinators")
			{
				Response.Redirect("Login.aspx");
				return;
			}

			if (!IsPostBack)
			{
				hfActiveReport.Value = "CaseSummary";
				SetDefaultDates();
				UpdateActiveTabStyles();
				GenerateReport();
			}
		}

		private void SetDefaultDates()
		{
			DateTime today = DateTime.Today;
			DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);

			txtFromDate.Text = firstDayOfMonth.ToString("yyyy-MM-dd");
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

		protected void tabCaseSummary_Click(object sender, EventArgs e)
		{
			hfActiveReport.Value = "CaseSummary";
			UpdateActiveTabStyles();
			GenerateReport();
		}

		protected void tabHighRisk_Click(object sender, EventArgs e)
		{
			hfActiveReport.Value = "HighRisk";
			UpdateActiveTabStyles();
			GenerateReport();
		}

		protected void tabVaxUtil_Click(object sender, EventArgs e)
		{
			hfActiveReport.Value = "VaccineUtilization";
			UpdateActiveTabStyles();
			GenerateReport();
		}

		protected void tabExpiry_Click(object sender, EventArgs e)
		{
			hfActiveReport.Value = "Expiry";
			UpdateActiveTabStyles();
			GenerateReport();
		}

		protected void btnGenerateReport_Click(object sender, EventArgs e)
		{
			GenerateReport();
		}

		private void GenerateReport()
		{
			lblMessage.Text = "";

			DateTime fromDate, toDate;
			if (!TryGetDates(out fromDate, out toDate))
				return;

			DataTable dt = GetReportData(hfActiveReport.Value, fromDate, toDate);

			gvReport.DataSource = dt;
			gvReport.DataBind();

			lblActiveReport.Text = GetReportTitle(hfActiveReport.Value);
			lblFromDate.Text = fromDate.ToString("MMM dd, yyyy");
			lblToDate.Text = toDate.ToString("MMM dd, yyyy");
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
				case "CaseSummary":
					return GetCaseSummaryData(fromDate, toDate);

				case "HighRisk":
					return GetHighRiskData(fromDate, toDate);

				case "VaccineUtilization":
					return GetVaccineUtilizationData(fromDate, toDate);

				case "Expiry":
					return GetExpiryData(fromDate, toDate);

				default:
					return new DataTable();
			}
		}

		private DataTable GetCaseSummaryData(DateTime fromDate, DateTime toDate)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT 
                        c.case_id AS [Case ID],
                        c.patient_id AS [Patient ID],
                        ISNULL(c.case_no, '') AS [Case No],
                        c.date_of_bite AS [Date of Bite],
                        ISNULL(c.place_of_bite, '') AS [Place of Bite],
                        ISNULL(c.type_of_exposure, '') AS [Exposure Type],
                        ISNULL(c.site_of_bite, '') AS [Site of Bite],
                        ISNULL(c.category, '') AS [Category]
                    FROM [Case] c
                    WHERE c.date_of_bite >= @fromDate
                      AND c.date_of_bite < DATEADD(DAY, 1, @toDate)
                    ORDER BY c.date_of_bite DESC, c.case_id DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
				da.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

				DataTable dt = new DataTable();
				da.Fill(dt);
				return dt;
			}
		}

		private DataTable GetHighRiskData(DateTime fromDate, DateTime toDate)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
            SELECT 
                c.case_id AS [Case ID],
                c.patient_id AS [Patient ID],
                ISNULL(c.case_no, '') AS [Case No],
                c.date_of_bite AS [Date of Bite],
                ISNULL(c.place_of_bite, '') AS [Place of Bite],
                ISNULL(c.site_of_bite, '') AS [Wound Location],
                ISNULL(c.category, '') AS [Category]
            FROM dbo.[Case] c
            WHERE c.date_of_bite >= @fromDate
              AND c.date_of_bite < DATEADD(DAY, 1, @toDate)
              AND c.category = 'III'
            ORDER BY c.date_of_bite DESC, c.case_id DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
				da.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

				DataTable dt = new DataTable();
				da.Fill(dt);
				return dt;
			}
		}

		private DataTable GetVaccineUtilizationData(DateTime fromDate, DateTime toDate)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
            SELECT
                il.inventory_id AS [Inventory Log ID],
                v.vaccine_name AS [Vaccine Name],
                vb.batch_number AS [Batch Number],
                v.manufacturer AS [Manufacturer],
                il.transaction_type AS [Transaction Type],
                il.quantity AS [Quantity],
                vb.current_stock AS [Current Stock],
                il.transaction_date AS [Transaction Date],
                il.updated_by AS [Updated By]
            FROM dbo.InventoryLog il
            INNER JOIN dbo.VaccineBatch vb
                ON il.batch_id = vb.batch_id
            INNER JOIN dbo.Vaccine v
                ON vb.batch_id = v.batch_id
            WHERE il.transaction_date >= @fromDate
              AND il.transaction_date < DATEADD(DAY, 1, @toDate)
            ORDER BY il.transaction_date DESC, il.inventory_id DESC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
				da.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

				DataTable dt = new DataTable();
				da.Fill(dt);
				return dt;
			}
		}

		private DataTable GetExpiryData(DateTime fromDate, DateTime toDate)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
            SELECT
                v.vaccine_name AS [Vaccine Name],
                vb.batch_number AS [Batch Number],
                v.manufacturer AS [Manufacturer],
                vb.quantity_received AS [Quantity Received],
                vb.current_stock AS [Current Stock],
                vb.manufacturing_date AS [Manufacturing Date],
                vb.expiration_date AS [Expiration Date],
                vb.date_received AS [Date Received],
                v.is_active AS [Is Active]
            FROM dbo.VaccineBatch vb
            INNER JOIN dbo.Vaccine v
                ON vb.batch_id = v.batch_id
            WHERE vb.expiration_date >= @fromDate
              AND vb.expiration_date < DATEADD(DAY, 1, @toDate)
            ORDER BY vb.expiration_date ASC, vb.batch_id ASC";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@fromDate", fromDate.Date);
				da.SelectCommand.Parameters.AddWithValue("@toDate", toDate.Date);

				DataTable dt = new DataTable();
				da.Fill(dt);
				return dt;
			}
		}

		protected void btnExportExcel_Click(object sender, EventArgs e)
		{
			DateTime fromDate, toDate;
			if (!TryGetDates(out fromDate, out toDate))
				return;

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
				case "CaseSummary":
					return "Case Summary Report";
				case "HighRisk":
					return "High-Risk Cases Report";
				case "VaccineUtilization":
					return "Vaccine Utilization Report";
				case "Expiry":
					return "Expiry Report";
				default:
					return "Report";
			}
		}

		private void UpdateActiveTabStyles()
		{
			string activeClass = "rounded-xl border border-blue-600 bg-blue-600 px-6 py-3 text-base font-semibold text-white transition";
			string inactiveClass = "rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50";

			tabCaseSummary.CssClass = inactiveClass;
			tabHighRisk.CssClass = inactiveClass;
			tabVaxUtil.CssClass = inactiveClass;
			tabExpiry.CssClass = inactiveClass;

			switch (hfActiveReport.Value)
			{
				case "CaseSummary":
					tabCaseSummary.CssClass = activeClass;
					break;
				case "HighRisk":
					tabHighRisk.CssClass = activeClass;
					break;
				case "VaccineUtilization":
					tabVaxUtil.CssClass = activeClass;
					break;
				case "Expiry":
					tabExpiry.CssClass = activeClass;
					break;
			}
		}
	}
}