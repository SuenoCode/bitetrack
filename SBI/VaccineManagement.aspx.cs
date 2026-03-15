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

        private string UserRole => Session["userRole"]?.ToString().ToUpper() ?? "";
        private bool IsAdmin => UserRole == "A";
        // Only Admin can add/receive stock
        public bool CanAddStock => IsAdmin;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            { Response.Redirect("Login.aspx"); return; }

            if (!IsPostBack)
            {
                // Hide Add Stock button and tab for non-admins
                btnOpenAddStock.Visible = CanAddStock;

                ShowDashboard();
                LoadVaccineDropdown();
            }
        }

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
            if (!CanAddStock)
            { ShowAlert("You do not have permission to add stock."); ShowDashboard(); return; }

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
        { ClearAddStockFields(); ShowDashboard(); }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        { panelBatchDetails.Visible = false; }

        private void SetActiveTab(string tab)
        {
            string active = "px-4 py-2 rounded-lg font-semibold text-white bg-blue-600 text-sm";
            string inactive = "px-4 py-2 rounded-lg border border-slate-300 bg-white font-semibold text-slate-700 text-sm hover:bg-slate-50 transition";

            btnOverviewTab.CssClass = (tab == "overview") ? active : inactive;
            btnOverviewTab.Text = "Inventory Dashboard";

            // Only show Receive Stock tab to Admin
            btnAddStockTab.Visible = CanAddStock;
            btnAddStockTab.CssClass = (tab == "addstock") ? active : inactive;
            btnAddStockTab.Text = "Receive Stock";
        }

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
                    SELECT v.vaccine_name,
                           COUNT(b.batch_id) AS total_batches,
                           ISNULL(SUM(b.current_stock),0) AS total_stock
                    FROM   Vaccine v
                    LEFT JOIN VaccineBatch b ON v.vaccine_id = b.vaccine_id
                    WHERE  (@search='' OR v.vaccine_name LIKE @search)
                    GROUP BY v.vaccine_name
                    ORDER BY v.vaccine_name";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search",
                    string.IsNullOrWhiteSpace(search) ? "" : "%" + search + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                ViewState["InventoryData"] = dt;
                gvInventory.DataSource = dt;
                gvInventory.DataBind();
            }
        }

        private void LoadStockHistory()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 50 l.transaction_date, v.vaccine_name,
                           b.batch_number, l.quantity, l.updated_by
                    FROM   InventoryLog l
                    JOIN   VaccineBatch b ON l.batch_id   = b.batch_id
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
                    SELECT b.batch_number, b.manufacturing_date, b.expiration_date,
                           b.quantity_received, b.current_stock,
                           CASE
                               WHEN b.expiration_date < CAST(GETDATE() AS DATE) THEN 'Expired'
                               WHEN b.expiration_date <= DATEADD(DAY,30,CAST(GETDATE() AS DATE)) THEN 'Expiring Soon'
                               ELSE 'Available'
                           END AS stock_status
                    FROM   VaccineBatch b
                    JOIN   Vaccine      v ON b.vaccine_id = v.vaccine_id
                    WHERE  v.vaccine_name = @vname
                    ORDER BY b.expiration_date";

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

        protected void btnSearch_Click(object sender, EventArgs e) => BindInventoryGrid(txtSearch.Text.Trim());
        protected void btnClearSearch_Click(object sender, EventArgs e) { txtSearch.Text = ""; BindInventoryGrid(); }

        protected void gvInventory_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewDetails")
            {
                int rowIndex = Convert.ToInt32(e.CommandArgument);
                DataTable dt = ViewState["InventoryData"] as DataTable;
                if (dt != null)
                {
                    int absIndex = (gvInventory.PageIndex * gvInventory.PageSize) + rowIndex;
                    if (absIndex < dt.Rows.Count)
                        LoadBatchDetails(dt.Rows[absIndex]["vaccine_name"].ToString());
                }
            }
        }

        protected void gvInventory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvInventory.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["InventoryData"] as DataTable;
            if (dt == null) BindInventoryGrid(); else { gvInventory.DataSource = dt; gvInventory.DataBind(); }
        }

        protected void gvStockHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvStockHistory.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["StockHistoryData"] as DataTable;
            if (dt == null) LoadStockHistory(); else { gvStockHistory.DataSource = dt; gvStockHistory.DataBind(); }
        }

        protected void gvBatchDetails_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvBatchDetails.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["BatchData"] as DataTable;
            if (dt == null) LoadBatchDetails(lblSelectedVaccine.Text);
            else { gvBatchDetails.DataSource = dt; gvBatchDetails.DataBind(); }
        }

        protected void btnSaveStock_Click(object sender, EventArgs e)
        {
            if (!CanAddStock) { ShowAlert("You do not have permission to add stock."); return; }

            if (string.IsNullOrWhiteSpace(ddlVaccineName.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtExpiryDate.Text) ||
                string.IsNullOrWhiteSpace(txtQuantity.Text))
            { ShowAlert("Please fill in all fields."); return; }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty <= 0)
            { ShowAlert("Quantity must be a positive whole number."); return; }

            if (!DateTime.TryParse(txtExpiryDate.Text, out DateTime expiry))
            { ShowAlert("Please enter a valid expiry date."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int batchId;
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO VaccineBatch
                            (vaccine_id,manufacturing_date,expiration_date,quantity_received,current_stock,date_received)
                        VALUES (@vid,GETDATE(),@exp,@qty,@qty,GETDATE());
                        SELECT SCOPE_IDENTITY();", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@vid", ddlVaccineName.SelectedValue);
                        cmd.Parameters.AddWithValue("@exp", expiry);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        batchId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO InventoryLog (batch_id,transaction_type,quantity,transaction_date,updated_by)
                        VALUES (@bid,'In',@qty,GETDATE(),@user)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@bid", batchId);
                        cmd.Parameters.AddWithValue("@qty", qty);
                        cmd.Parameters.AddWithValue("@user", Session["fullName"]?.ToString() ?? "System");
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    ShowAlert("Stock added successfully.");
                    ClearAddStockFields();
                    ShowAddStock();
                }
                catch (Exception ex) { trans.Rollback(); ShowAlert("Error saving stock: " + ex.Message); }
            }
        }

        private void LoadVaccineDropdown()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(
                    "SELECT vaccine_id, vaccine_name FROM Vaccine WHERE is_active='Yes' ORDER BY vaccine_name", conn);
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
            if (ddlVaccineName.Items.Count > 0) ddlVaccineName.SelectedIndex = 0;
            txtExpiryDate.Text = "";
            txtQuantity.Text = "";
        }

        protected string FormatStockStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return "";
            string css;
            switch (status.ToLower())
            {
                case "expired": css = "badge badge-exp"; break;
                case "expiring soon": css = "badge badge-warn"; break;
                default: css = "badge badge-ok"; break;
            }
            return $"<span class=\"{css}\">{System.Web.HttpUtility.HtmlEncode(status)}</span>";
        }

        private void ShowAlert(string message)
        {
            ClientScript.RegisterStartupScript(GetType(), "alert",
                $"alert('{message.Replace("'", "\\'")}');", true);
        }
    }
}