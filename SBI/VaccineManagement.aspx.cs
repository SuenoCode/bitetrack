using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBI
{
	public partial class VaccineManagement : System.Web.UI.Page
	{
		private readonly string connectionString =
			ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

		protected void Page_Load(object sender, EventArgs e)
		{
			if (Session["userRole"] == null ||
				(Session["userRole"].ToString().ToLower() != "adminassistant" &&
				 Session["userRole"].ToString().ToLower() != "vaccinators"))
			{
				Response.Redirect("Login.aspx");
				return;
			}

			if (!IsPostBack)
			{
				panelOverview.Visible = true;
				panelAddStock.Visible = false;
				panelInventory.Visible = false;

				SetActiveTab("overview");
				LoadOverview();
				LoadVaccineDropdown();
				BindInventoryGrid();
			}
		}

		// ============================================================
		// TAB SWITCHING
		// ============================================================
		protected void btnOverviewTab_Click(object sender, EventArgs e)
		{
			panelOverview.Visible = true;
			panelAddStock.Visible = false;
			panelInventory.Visible = false;

			LoadOverview();
			SetActiveTab("overview");
		}

		protected void btnAddStockTab_Click(object sender, EventArgs e)
		{
			panelOverview.Visible = false;
			panelAddStock.Visible = true;
			panelInventory.Visible = false;

			LoadVaccineDropdown();
			SetActiveTab("addstock");
		}

		protected void btnInventoryTab_Click(object sender, EventArgs e)
		{
			panelOverview.Visible = false;
			panelAddStock.Visible = false;
			panelInventory.Visible = true;

			BindInventoryGrid();
			SetActiveTab("inventory");
		}

		protected void btnOpenAddStock_Click(object sender, EventArgs e)
		{
			panelOverview.Visible = false;
			panelAddStock.Visible = true;
			panelInventory.Visible = false;

			LoadVaccineDropdown();
			SetActiveTab("addstock");
		}

		protected void btnCancelStock_Click(object sender, EventArgs e)
		{
			ClearAddStockFields();

			panelOverview.Visible = false;
			panelAddStock.Visible = false;
			panelInventory.Visible = true;

			BindInventoryGrid();
			SetActiveTab("inventory");
		}

		private void SetActiveTab(string tab)
		{
			string active = "px-4 py-2 rounded-lg font-semibold text-white bg-blue-600";
			string inactive = "px-4 py-2 rounded-lg border border-slate-300 bg-white font-semibold";

			btnOverviewTab.CssClass = tab == "overview" ? active : inactive;
			btnAddStockTab.CssClass = tab == "addstock" ? active : inactive;
			btnInventoryTab.CssClass = tab == "inventory" ? active : inactive;
		}

		// ============================================================
		// OVERVIEW
		// ============================================================
		private void LoadOverview()
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();

				string qTotal = @"
                    SELECT ISNULL(SUM(current_stock), 0)
                    FROM VaccineBatch
                    WHERE expiration_date >= CAST(GETDATE() AS DATE)";

				using (SqlCommand cmdTotal = new SqlCommand(qTotal, conn))
				{
					lblTotalDoses.Text = Convert.ToString(cmdTotal.ExecuteScalar());
				}

				string qExp30 = @"
                    SELECT ISNULL(SUM(current_stock), 0)
                    FROM VaccineBatch
                    WHERE expiration_date >= CAST(GETDATE() AS DATE)
                      AND expiration_date <= DATEADD(DAY, 30, CAST(GETDATE() AS DATE))";

				using (SqlCommand cmdExp30 = new SqlCommand(qExp30, conn))
				{
					lblExpiring30.Text = Convert.ToString(cmdExp30.ExecuteScalar());
				}

				string qExpired = @"
                    SELECT ISNULL(SUM(current_stock), 0)
                    FROM VaccineBatch
                    WHERE expiration_date < CAST(GETDATE() AS DATE)";

				using (SqlCommand cmdExpired = new SqlCommand(qExpired, conn))
				{
					lblExpired.Text = Convert.ToString(cmdExpired.ExecuteScalar());
				}

				string qMTD = @"
                    SELECT ISNULL(SUM(quantity), 0)
                    FROM InventoryLog
                    WHERE transaction_type = 'In'
                      AND MONTH(transaction_date) = MONTH(GETDATE())
                      AND YEAR(transaction_date) = YEAR(GETDATE())";

				using (SqlCommand cmdMTD = new SqlCommand(qMTD, conn))
				{
					lblAdministeredMTD.Text = Convert.ToString(cmdMTD.ExecuteScalar());
				}
			}
		}

		// ============================================================
		// DROPDOWN
		// ============================================================
		private void LoadVaccineDropdown()
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
                    SELECT vaccine_id, vaccine_name
                    FROM Vaccine
                    WHERE is_active = 'Yes'
                    ORDER BY vaccine_name";

				using (SqlDataAdapter da = new SqlDataAdapter(query, conn))
				{
					DataTable dt = new DataTable();
					da.Fill(dt);

					ddlVaccineName.DataSource = dt;
					ddlVaccineName.DataTextField = "vaccine_name";
					ddlVaccineName.DataValueField = "vaccine_id";
					ddlVaccineName.DataBind();
					ddlVaccineName.Items.Insert(0, new ListItem("-- Select Vaccine --", ""));
				}
			}
		}

		// ============================================================
		// ADD STOCK
		// ============================================================
		protected void btnSaveStock_Click(object sender, EventArgs e)
		{
			if (string.IsNullOrWhiteSpace(ddlVaccineName.SelectedValue) ||
				string.IsNullOrWhiteSpace(txtExpiryDate.Text) ||
				string.IsNullOrWhiteSpace(txtQuantity.Text))
			{
				ShowAlert("Please fill in all fields.");
				return;
			}

			int vaccineId;
			int quantity;
			DateTime expiryDate;

			if (!int.TryParse(ddlVaccineName.SelectedValue, out vaccineId))
			{
				ShowAlert("Please select a valid vaccine.");
				return;
			}

			if (!int.TryParse(txtQuantity.Text.Trim(), out quantity) || quantity <= 0)
			{
				ShowAlert("Please enter a valid quantity.");
				return;
			}

			if (!DateTime.TryParse(txtExpiryDate.Text.Trim(), out expiryDate))
			{
				ShowAlert("Please enter a valid expiration date.");
				return;
			}

			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				conn.Open();
				SqlTransaction trans = conn.BeginTransaction();

				try
				{
					// Create a NEW batch for the selected vaccine
					string insertBatch = @"
                        INSERT INTO VaccineBatch
                            (vaccine_id, manufacturing_date, expiration_date, quantity_received, current_stock, date_received)
                        VALUES
                            (@vaccine_id, CAST(GETDATE() AS DATE), @expiration_date, @quantity, @quantity, CAST(GETDATE() AS DATE));

                        SELECT CAST(SCOPE_IDENTITY() AS INT);";

					int batchId;
					using (SqlCommand cmdBatch = new SqlCommand(insertBatch, conn, trans))
					{
						cmdBatch.Parameters.AddWithValue("@vaccine_id", vaccineId);
						cmdBatch.Parameters.AddWithValue("@expiration_date", expiryDate);
						cmdBatch.Parameters.AddWithValue("@quantity", quantity);

						batchId = Convert.ToInt32(cmdBatch.ExecuteScalar());
					}

					// Log inventory transaction
					string insertLog = @"
                        INSERT INTO InventoryLog
                            (batch_id, transaction_type, quantity, transaction_date, updated_by)
                        VALUES
                            (@batch_id, 'In', @quantity, GETDATE(), @updated_by)";

					using (SqlCommand cmdLog = new SqlCommand(insertLog, conn, trans))
					{
						cmdLog.Parameters.AddWithValue("@batch_id", batchId);
						cmdLog.Parameters.AddWithValue("@quantity", quantity);
						cmdLog.Parameters.AddWithValue("@updated_by",
							Session["userName"] != null ? Session["userName"].ToString() : "System");

						cmdLog.ExecuteNonQuery();
					}

					trans.Commit();

					ClearAddStockFields();
					LoadOverview();
					BindInventoryGrid();

					panelOverview.Visible = false;
					panelAddStock.Visible = false;
					panelInventory.Visible = true;
					SetActiveTab("inventory");

					ShowAlert("Stock added successfully. Batch number was generated automatically.");
				}
				catch (Exception ex)
				{
					trans.Rollback();
					ShowAlert("Error: " + ex.Message.Replace("'", ""));
				}
			}
		}

		private void ClearAddStockFields()
		{
			if (ddlVaccineName.Items.Count > 0)
				ddlVaccineName.SelectedIndex = 0;

			txtExpiryDate.Text = "";
			txtQuantity.Text = "";
		}

		// ============================================================
		// INVENTORY GRID (NOW USES VIEW)
		// ============================================================
		private void BindInventoryGrid(string search = "")
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
        SELECT
            vaccine_name,
            total_batches,
            total_stock
        FROM vw_VaccineInventorySummary
        WHERE (@search='' OR vaccine_name LIKE @search)
        ORDER BY vaccine_name";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);

				da.SelectCommand.Parameters.AddWithValue(
					"@search",
					string.IsNullOrWhiteSpace(search) ? "" : "%" + search + "%");

				DataTable dt = new DataTable();
				da.Fill(dt);

				gvInventory.DataSource = dt;
				gvInventory.DataBind();
			}
		}

		protected void btnSearch_Click(object sender, EventArgs e)
		{
			BindInventoryGrid(txtSearch.Text.Trim());

			panelOverview.Visible = false;
			panelAddStock.Visible = false;
			panelInventory.Visible = true;
			SetActiveTab("inventory");
		}

		protected void btnClearSearch_Click(object sender, EventArgs e)
		{
			txtSearch.Text = "";
			BindInventoryGrid();

			panelOverview.Visible = false;
			panelAddStock.Visible = false;
			panelInventory.Visible = true;
			SetActiveTab("inventory");
		}

		// ============================================================
		// HELPER
		// ============================================================
		private void ShowAlert(string message)
		{
			ClientScript.RegisterStartupScript(
				this.GetType(),
				"alert",
				$"alert('{message.Replace("'", "")}');",
				true
			);
		}

		protected void gvInventory_RowCommand(object sender, GridViewCommandEventArgs e)
		{
			if (e.CommandName == "ViewDetails")
			{
				int rowIndex = Convert.ToInt32(e.CommandArgument);

				string vaccineName =
					gvInventory.Rows[rowIndex].Cells[0].Text;

				LoadBatchDetails(vaccineName);
			}
		}
		private void LoadBatchDetails(string vaccineName)
		{
			using (SqlConnection conn = new SqlConnection(connectionString))
			{
				string query = @"
        SELECT
            batch_number,
            manufacturing_date,
            expiration_date,
            quantity_received,
            current_stock,
            stock_status
        FROM vw_VaccineInventoryDetails
        WHERE vaccine_name = @vaccine_name
        ORDER BY expiration_date";

				SqlDataAdapter da = new SqlDataAdapter(query, conn);
				da.SelectCommand.Parameters.AddWithValue("@vaccine_name", vaccineName);	

				DataTable dt = new DataTable();
				da.Fill(dt);

				gvBatchDetails.DataSource = dt;
				gvBatchDetails.DataBind();

				panelBatchDetails.Visible = true;
			}
		}
	}
}