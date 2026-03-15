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
		/*	if (Session["userRole"] == null)
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

		protected void tabDailyActivity_Click(object sender, EventArgs e)
		{
			hfActiveReport.Value = "DailyActivitySummary";
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
				case "DailyInventorySummary":
					return GetDailyInventorySummaryData(fromDate, toDate);

				case "DailyActivitySummary":
					return GetDailyActivitySummaryData(fromDate, toDate);

				default:
					return new DataTable();
			}
		}

		private DataTable GetDailyInventorySummaryData(DateTime fromDate, DateTime toDate)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
            SELECT
                v.vaccine_name AS [Vaccine],
                ISNULL(SUM(vb.quantity_received), 0) AS [INV. BEG],
                ISNULL(SUM(CASE WHEN il.transaction_type = 'Consumed' THEN il.quantity ELSE 0 END), 0) AS [CONSUMED],
                ISNULL(SUM(CASE WHEN il.transaction_type = 'Pull-Out' THEN il.quantity ELSE 0 END), 0) AS [PULL-OUT],
                ISNULL(SUM(vb.current_stock), 0) AS [INV. END]
            FROM dbo.VaccineBatch vb
            INNER JOIN dbo.Vaccine v
                ON vb.vaccine_id = v.vaccine_id
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

		private DataTable GetDailyActivitySummaryData(DateTime fromDate, DateTime toDate)
		{
			DataTable dt = new DataTable();

			dt.Columns.Add("Name");
			dt.Columns.Add("Position");
			dt.Columns.Add("Work Hours");
			dt.Columns.Add("Description of Activity");
			dt.Columns.Add("No.");
			dt.Columns.Add("Other Remarks");

			for (int i = 1; i <= 8; i++)
			{
				DataRow dr = dt.NewRow();
				dr["Name"] = "";
				dr["Position"] = "";
				dr["Work Hours"] = "";
				dr["Description of Activity"] = "";
				dr["No."] = i.ToString();
				dr["Other Remarks"] = "";
				dt.Rows.Add(dr);
			}

			return dt;
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
				case "DailyInventorySummary":
					return "Daily Inventory Summary";
				case "DailyActivitySummary":
					return "Daily Activity Summary";
				default:
					return "Report";
			}
		}

		private void UpdateActiveTabStyles()
		{
			string activeClass = "rounded-xl border border-blue-600 bg-blue-600 px-6 py-3 text-base font-semibold text-white transition";
			string inactiveClass = "rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50";

			tabDailyInventory.CssClass = inactiveClass;
			tabDailyActivity.CssClass = inactiveClass;

			switch (hfActiveReport.Value)
			{
				case "DailyInventorySummary":
					tabDailyInventory.CssClass = activeClass;
					break;
				case "DailyActivitySummary":
					tabDailyActivity.CssClass = activeClass;
					break;
			}
		}
	}
}