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

        // ──────────────────────────────────────────────────────────
        // PAGE LOAD
        // ──────────────────────────────────────────────────────────

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
                ShowDashboard();
                LoadVaccineDropdown();
            }
        }

        // ──────────────────────────────────────────────────────────
        // NAVIGATION / TAB SWITCHING
        // ──────────────────────────────────────────────────────────

        private void ShowDashboard()
        {
            panelOverview.Visible = true;
            panelAddStock.Visible = false;
            panelBatchDetails.Visible = false;

            SetActiveTab("overview");
            LoadOverviewStats();
            BindInventoryGrid();
        }

        private void ShowAddStock()
        {
            panelOverview.Visible = false;
            panelAddStock.Visible = true;
            panelBatchDetails.Visible = false;

            LoadVaccineDropdown();
            LoadStockHistory();
            SetActiveTab("addstock");
        }

        protected void btnOverviewTab_Click(object sender, EventArgs e) => ShowDashboard();
        protected void btnAddStockTab_Click(object sender, EventArgs e) => ShowAddStock();
        protected void btnOpenAddStock_Click(object sender, EventArgs e) => ShowAddStock();
        protected void btnCancelStock_Click(object sender, EventArgs e)
        {
            ClearAddStockFields();
            ShowDashboard();
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            panelBatchDetails.Visible = false;
        }

        private void SetActiveTab(string tab)
        {
            string active = "px-4 py-2 rounded-lg font-semibold text-white bg-blue-600 text-sm";
            string inactive = "px-4 py-2 rounded-lg border border-slate-300 bg-white font-semibold text-slate-700 text-sm hover:bg-slate-50 transition";

            btnOverviewTab.CssClass = (tab == "overview") ? active : inactive;
            btnAddStockTab.CssClass = (tab == "addstock") ? active : inactive;

            btnOverviewTab.Text = "Inventory Dashboard";
            btnAddStockTab.Text = "Receive Stock";
        }

        // ──────────────────────────────────────────────────────────
        // DATA LOADING
        // ──────────────────────────────────────────────────────────

        private void LoadOverviewStats()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(current_stock),0) FROM VaccineBatch WHERE expiration_date >= CAST(GETDATE() AS DATE)", conn))
                    lblTotalDoses.Text = cmd.ExecuteScalar().ToString();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(current_stock),0) FROM VaccineBatch WHERE expiration_date >= CAST(GETDATE() AS DATE) AND expiration_date <= DATEADD(DAY,30,CAST(GETDATE() AS DATE))", conn))
                    lblExpiring30.Text = cmd.ExecuteScalar().ToString();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(current_stock),0) FROM VaccineBatch WHERE expiration_date < CAST(GETDATE() AS DATE)", conn))
                    lblExpired.Text = cmd.ExecuteScalar().ToString();

                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(quantity),0) FROM InventoryLog WHERE transaction_type='In' AND MONTH(transaction_date)=MONTH(GETDATE()) AND YEAR(transaction_date)=YEAR(GETDATE())", conn))
                    lblAdministeredMTD.Text = cmd.ExecuteScalar().ToString();
            }
        }

        private void BindInventoryGrid(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT vaccine_name, total_batches, total_stock
                    FROM   vw_VaccineInventorySummary
                    WHERE  (@search = '' OR vaccine_name LIKE @search)
                    ORDER BY vaccine_name";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search",
                    string.IsNullOrWhiteSpace(search) ? "" : "%" + search + "%");

                DataTable dt = new DataTable();
                da.Fill(dt);

                // Store full data in ViewState so pagination survives PostBack without re-querying
                ViewState["InventoryData"] = dt;

                gvInventory.DataSource = dt;
                gvInventory.DataBind();
            }
        }

        private void LoadStockHistory()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Fetch all recent In entries; the GridView pager shows 10 at a time
                string query = @"
                    SELECT TOP 50 l.transaction_date, v.vaccine_name, b.batch_number, l.quantity, l.updated_by
                    FROM   InventoryLog l
                    JOIN   VaccineBatch b ON l.batch_id  = b.batch_id
                    JOIN   Vaccine      v ON b.vaccine_id = v.vaccine_id
                    WHERE  l.transaction_type = 'In'
                    ORDER BY l.transaction_date DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["StockHistoryData"] = dt;

                gvStockHistory.DataSource = dt;
                gvStockHistory.DataBind();
            }
        }

        private void LoadBatchDetails(string vaccineName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT batch_number, manufacturing_date, expiration_date,
                           quantity_received, current_stock, stock_status
                    FROM   vw_VaccineInventoryDetails
                    WHERE  vaccine_name = @vname
                    ORDER BY expiration_date";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@vname", vaccineName);

                DataTable dt = new DataTable();
                da.Fill(dt);

                ViewState["BatchData"] = dt;
                lblSelectedVaccine.Text = vaccineName;
                gvBatchDetails.DataSource = dt;
                gvBatchDetails.DataBind();
                panelBatchDetails.Visible = true;
            }
        }

        // ──────────────────────────────────────────────────────────
        // SEARCH
        // ──────────────────────────────────────────────────────────

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            BindInventoryGrid(txtSearch.Text.Trim());
        }

        protected void btnClearSearch_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            BindInventoryGrid();
        }

        // ──────────────────────────────────────────────────────────
        // GRIDVIEW ROW COMMANDS
        // ──────────────────────────────────────────────────────────

        protected void gvInventory_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                // Use Rows[] relative to the current page
                string vaccineName = gvInventory.Rows[rowIndex].Cells[0].Text;
                LoadBatchDetails(vaccineName);
            }
        }

        // ──────────────────────────────────────────────────────────
        // PAGINATION — restore from ViewState so no extra DB hit
        // ──────────────────────────────────────────────────────────

        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInventory.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["InventoryData"] as DataTable;
            if (dt == null) BindInventoryGrid(); // fallback
            else
            {
                gvInventory.DataSource = dt;
                gvInventory.DataBind();
            }
        }

        protected void gvStockHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvStockHistory.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["StockHistoryData"] as DataTable;
            if (dt == null) LoadStockHistory();
            else
            {
                gvStockHistory.DataSource = dt;
                gvStockHistory.DataBind();
            }
        }

        protected void gvBatchDetails_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBatchDetails.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["BatchData"] as DataTable;
            if (dt == null)
            {
                if (lblSelectedVaccine != null)
                    LoadBatchDetails(lblSelectedVaccine.Text);
            }
            else
            {
                gvBatchDetails.DataSource = dt;
                gvBatchDetails.DataBind();
            }
        }

        // ──────────────────────────────────────────────────────────
        // SAVE STOCK
        // ──────────────────────────────────────────────────────────

        protected void btnSaveStock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ddlVaccineName.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtExpiryDate.Text) ||
                string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                ShowAlert("Please fill in all fields.");
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            {
                ShowAlert("Quantity must be a positive whole number.");
                return;
            }

            if (!DateTime.TryParse(txtExpiryDate.Text, out DateTime expiry))
            {
                ShowAlert("Please enter a valid expiry date.");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Insert batch and get new batch_id
                    string insertBatch = @"
                        INSERT INTO VaccineBatch 
                            (vaccine_id, manufacturing_date, expiration_date, quantity_received, current_stock, date_received)
                        VALUES (@vid, GETDATE(), @exp, @qty, @qty, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    int batchId;
                    using (SqlCommand cmd = new SqlCommand(insertBatch, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@vid", ddlVaccineName.SelectedValue);
                        cmd.Parameters.AddWithValue("@exp", expiry);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        batchId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // Log the transaction
                    string insertLog = @"
                        INSERT INTO InventoryLog (batch_id, transaction_type, quantity, transaction_date, updated_by)
                        VALUES (@bid, 'In', @qty, GETDATE(), @user)";

                    using (SqlCommand cmd = new SqlCommand(insertLog, conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@bid", batchId);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@user", Session["userName"]?.ToString() ?? "System");
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    ShowAlert("Stock added successfully.");
                    ClearAddStockFields();

                    // Stay on Add Stock tab and refresh history so the new entry appears
                    ShowAddStock();
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    System.Diagnostics.Debug.WriteLine("SaveStock error: " + ex.Message);
                    ShowAlert("Error saving stock: " + ex.Message);
                }
            }
        }

        // ──────────────────────────────────────────────────────────
        // HELPERS
        // ──────────────────────────────────────────────────────────

        private void LoadVaccineDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT vaccine_id, vaccine_name FROM Vaccine WHERE is_active = 'Yes' ORDER BY vaccine_name";
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                ddlVaccineName.DataSource = dt;
                ddlVaccineName.DataTextField = "vaccine_name";
                ddlVaccineName.DataValueField = "vaccine_id";
                ddlVaccineName.DataBind();
                ddlVaccineName.Items.Insert(0, new ListItem("-- Select Vaccine --", ""));
            }
        }

        private void ClearAddStockFields()
        {
            if (ddlVaccineName.Items.Count > 0)
                ddlVaccineName.SelectedIndex = 0;
            txtExpiryDate.Text = "";
            txtQuantity.Text = "";
        }

        /// <summary>
        /// Returns an HTML badge span for a stock status string.
        /// Called from the ASPX TemplateField via <%# FormatStockStatus(...) %>
        /// </summary>
        protected string FormatStockStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return "";

            string css;
            switch (status.ToLower())
            {
                case "expired":
                    css = "badge badge-exp";
                    break;
                case "expiring soon":
                    css = "badge badge-warn";
                    break;
                case "available":
                default:
                    css = "badge badge-ok";
                    break;
            }

            return $"<span class=\"{css}\">{System.Web.HttpUtility.HtmlEncode(status)}</span>";
        }

        private void ShowAlert(string message)
        {
            ClientScript.RegisterStartupScript(
                this.GetType(), "alert",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }
    }
}