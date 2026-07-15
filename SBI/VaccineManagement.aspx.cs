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
        private string UserName => Session["fullName"]?.ToString() ?? "System";
        private string UserId => Session["userId"]?.ToString() ?? "0";

        public bool CanAddStock => UserRole == "A" || UserRole == "B"; // Admin or AdminAssistant

        private const int VialPageSize = 10;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            {
                Response.Redirect("Login.aspx");
                return;
            }

            if (!IsPostBack)
            {
                btnOpenAddStock.Visible = CanAddStock;

                ShowDashboard();
                LoadVaccineDropdown();
            }
        }

        // ── Navigation ──────────────────────────────────────────────────────

        private void ShowDashboard()
        {
            panelOverview.Visible = true;
            panelAddStock.Visible = false;
            panelAudit.Visible = false;
            panelBatchDetails.Visible = false;
            panelVials.Visible = false;
            SetActiveTab("overview");
            LoadOverviewStats();
            BindInventoryGrid();
        }

        private void ShowAddStock()
        {
            if (!CanAddStock)
            {
                ShowAlert("You do not have permission to add stock.", "warning");
                ShowDashboard();
                return;
            }

            panelOverview.Visible = false;
            panelAddStock.Visible = true;
            panelAudit.Visible = false;
            panelBatchDetails.Visible = false;
            panelVials.Visible = false;
            LoadVaccineDropdown();
            LoadStockHistory();
            SetActiveTab("addstock");
        }

        private void ShowAuditLog()
        {
            panelOverview.Visible = false;
            panelAddStock.Visible = false;
            panelAudit.Visible = true;
            panelBatchDetails.Visible = false;
            panelVials.Visible = false;
            SetActiveTab("audit");
            BindAuditLog();
        }

        protected void btnOverviewTab_Click(object sender, EventArgs e) => ShowDashboard();
        protected void btnAddStockTab_Click(object sender, EventArgs e) => ShowAddStock();
        protected void btnAuditTab_Click(object sender, EventArgs e) => ShowAuditLog();
        protected void btnOpenAddStock_Click(object sender, EventArgs e) => ShowAddStock();

        protected void btnCancelStock_Click(object sender, EventArgs e)
        {
            ClearAddStockFields();
            ShowDashboard();
        }

        protected void btnCloseDetails_Click(object sender, EventArgs e)
        {
            panelBatchDetails.Visible = false;
            panelVials.Visible = false;
        }

        protected void btnCloseVials_Click(object sender, EventArgs e)
        {
            panelVials.Visible = false;
        }

        private void SetActiveTab(string tab)
        {
            string active = "px-4 py-2 rounded-lg font-semibold text-white bg-blue-600 text-sm";
            string inactive = "px-4 py-2 rounded-lg border border-slate-300 bg-white font-semibold text-slate-700 text-sm hover:bg-slate-50 transition";

            btnOverviewTab.CssClass = (tab == "overview") ? active : inactive;
            btnOverviewTab.Text = "Inventory Dashboard";

            btnAddStockTab.Visible = CanAddStock;
            btnAddStockTab.CssClass = (tab == "addstock") ? active : inactive;
            btnAddStockTab.Text = "Receive Stock";

            btnAuditTab.CssClass = (tab == "audit") ? active : inactive;
            btnAuditTab.Text = "Audit Log";
        }

        // ── Dashboard Stats ─────────────────────────────────────────────────

        private void LoadOverviewStats()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                // Total doses (all batches, not expired)
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(current_stock),0) FROM VaccineBatch WHERE expiration_date >= CAST(GETDATE() AS DATE)", conn))
                    lblTotalDoses.Text = cmd.ExecuteScalar().ToString();

                // Total vials (all batches, not expired)
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT ISNULL(COUNT(v.vial_id),0) FROM VaccineVial v 
                      JOIN VaccineBatch b ON v.batch_id = b.batch_id 
                      WHERE b.expiration_date >= CAST(GETDATE() AS DATE) 
                        AND v.vial_status IN ('Sealed', 'Open')", conn))
                    lblTotalVials.Text = cmd.ExecuteScalar().ToString();

                // Expiring in 30 days
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT ISNULL(SUM(current_stock),0) FROM VaccineBatch 
                      WHERE expiration_date >= CAST(GETDATE() AS DATE) 
                        AND expiration_date <= DATEADD(DAY,30,CAST(GETDATE() AS DATE))", conn))
                    lblExpiring30.Text = cmd.ExecuteScalar().ToString();

                // Expired
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT ISNULL(SUM(current_stock),0) FROM VaccineBatch WHERE expiration_date < CAST(GETDATE() AS DATE)", conn))
                    lblExpired.Text = cmd.ExecuteScalar().ToString();

                // Open vials
                using (SqlCommand cmd = new SqlCommand(
                    @"SELECT ISNULL(COUNT(v.vial_id),0) FROM VaccineVial v 
                      JOIN VaccineBatch b ON v.batch_id = b.batch_id 
                      WHERE v.vial_status = 'Open' 
                        AND b.expiration_date >= CAST(GETDATE() AS DATE)", conn))
                    lblOpenVials.Text = cmd.ExecuteScalar().ToString();
            }
        }

        // ── Inventory Grid ──────────────────────────────────────────────────

        private void BindInventoryGrid(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        v.vaccine_name,
                        COUNT(DISTINCT b.batch_id) AS total_batches,
                        ISNULL(SUM(b.current_stock),0) AS total_stock,
                        ISNULL(COUNT(DISTINCT vv.vial_id),0) AS total_vials
                    FROM Vaccine v
                    LEFT JOIN VaccineBatch b ON v.vaccine_id = b.vaccine_id 
                        AND b.expiration_date >= CAST(GETDATE() AS DATE)
                    LEFT JOIN VaccineVial vv ON b.batch_id = vv.batch_id 
                        AND vv.vial_status IN ('Sealed', 'Open')
                    WHERE (@search='' OR v.vaccine_name LIKE @search)
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

        // ── Batch Details ──────────────────────────────────────────────────

        private void LoadBatchDetails(string vaccineName)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        b.batch_id,
                        b.batch_number, 
                        b.expiration_date,
                        b.current_stock,
                        (SELECT COUNT(*) FROM VaccineVial WHERE batch_id = b.batch_id) AS vial_count,
                        CASE
                            WHEN b.expiration_date < CAST(GETDATE() AS DATE) THEN 'Expired'
                            WHEN b.expiration_date <= DATEADD(DAY,30,CAST(GETDATE() AS DATE)) THEN 'Expiring Soon'
                            ELSE 'Available'
                        END AS stock_status
                    FROM VaccineBatch b
                    JOIN Vaccine v ON b.vaccine_id = v.vaccine_id
                    WHERE v.vaccine_name = @vname
                    ORDER BY b.expiration_date";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@vname", vaccineName);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ViewState["BatchData"] = dt;
                lblSelectedVaccine.Text = vaccineName;
                rptBatches.DataSource = dt;
                rptBatches.DataBind();

                // Handle empty batch state
                panelNoBatches.Visible = dt.Rows.Count == 0;

                panelBatchDetails.Visible = true;
                panelVials.Visible = false;
            }
        }

        protected void rptBatches_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "ViewVials")
            {
                int batchId = Convert.ToInt32(e.CommandArgument);
                // Reset page to 1 when viewing a new batch
                hfVialPage.Value = "1";
                LoadVials(batchId);
            }
        }

        // ── Vials (Read-Only Table with Paging) ───────────────────────────

        private void LoadVials(int batchId)
        {
            // Store the current batch ID in ViewState so paging can reuse it
            ViewState["CurrentBatchId"] = batchId;

            // Get the current page from hidden field, default to 1
            int page = 1;
            if (!string.IsNullOrEmpty(hfVialPage.Value))
                int.TryParse(hfVialPage.Value, out page);
            if (page < 1) page = 1;

            BindVialsPaged(batchId, page);
        }

        private void BindVialsPaged(int batchId, int page)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Get batch number
                string batchNumber = "";
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT batch_number FROM VaccineBatch WHERE batch_id = @bid", conn))
                {
                    cmd.Parameters.AddWithValue("@bid", batchId);
                    conn.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null)
                        batchNumber = result.ToString();
                    conn.Close();
                }
                lblSelectedBatch.Text = batchNumber;

                // Get total count for paging
                int totalCount = 0;
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM VaccineVial WHERE batch_id = @bid", conn))
                {
                    cmd.Parameters.AddWithValue("@bid", batchId);
                    conn.Open();
                    totalCount = Convert.ToInt32(cmd.ExecuteScalar());
                    conn.Close();
                }

                // Calculate total pages
                int totalPages = (int)Math.Ceiling((double)totalCount / VialPageSize);
                if (totalPages < 1) totalPages = 1;
                if (page > totalPages) page = totalPages;

                // Store page in hidden field
                hfVialPage.Value = page.ToString();

                // Get paged data
                int offset = (page - 1) * VialPageSize;
                string query = @"
                    SELECT 
                        vial_id,
                        vial_no,
                        doses_per_vial,
                        doses_used,
                        vial_status,
                        opened_at
                    FROM VaccineVial
                    WHERE batch_id = @bid
                    ORDER BY 
                        CASE vial_status 
                            WHEN 'Sealed' THEN 1
                            WHEN 'Open' THEN 2
                            WHEN 'Empty' THEN 3
                            WHEN 'Discarded' THEN 4
                        END,
                        vial_no
                    OFFSET @offset ROWS FETCH NEXT @pageSize ROWS ONLY";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@bid", batchId);
                da.SelectCommand.Parameters.AddWithValue("@offset", offset);
                da.SelectCommand.Parameters.AddWithValue("@pageSize", VialPageSize);
                DataTable dt = new DataTable();
                da.Fill(dt);

                rptVials.DataSource = dt;
                rptVials.DataBind();

                // Update pager controls
                btnVialPrev.Enabled = (page > 1);
                btnVialNext.Enabled = (page < totalPages);
                litVialPageInfo.Text = $"Page {page} of {totalPages}";

                // Show/hide empty message
                panelNoVials.Visible = (totalCount == 0);

                // Load summary stats (full count, not paged)
                LoadVialSummaryStats(batchId);

                panelVials.Visible = true;
            }
        }

        protected void btnVialPrev_Click(object sender, EventArgs e)
        {
            int page = 1;
            if (!string.IsNullOrEmpty(hfVialPage.Value))
                int.TryParse(hfVialPage.Value, out page);
            if (page > 1) page--;

            int batchId = ViewState["CurrentBatchId"] != null ? Convert.ToInt32(ViewState["CurrentBatchId"]) : 0;
            if (batchId > 0)
                BindVialsPaged(batchId, page);
        }

        protected void btnVialNext_Click(object sender, EventArgs e)
        {
            int page = 1;
            if (!string.IsNullOrEmpty(hfVialPage.Value))
                int.TryParse(hfVialPage.Value, out page);
            page++;

            int batchId = ViewState["CurrentBatchId"] != null ? Convert.ToInt32(ViewState["CurrentBatchId"]) : 0;
            if (batchId > 0)
                BindVialsPaged(batchId, page);
        }

        private void LoadVialSummaryStats(int batchId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        COUNT(CASE WHEN vial_status = 'Sealed' THEN 1 END) AS Sealed,
                        COUNT(CASE WHEN vial_status = 'Open' THEN 1 END) AS [Open],
                        COUNT(CASE WHEN vial_status = 'Empty' THEN 1 END) AS Empty,
                        COUNT(CASE WHEN vial_status = 'Discarded' THEN 1 END) AS Discarded
                    FROM VaccineVial
                    WHERE batch_id = @bid";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@bid", batchId);
                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            litSealedCount.Text = reader["Sealed"]?.ToString() ?? "0";
                            litOpenCount.Text = reader["Open"]?.ToString() ?? "0";
                            litEmptyCount.Text = reader["Empty"]?.ToString() ?? "0";
                            litDiscardedCount.Text = reader["Discarded"]?.ToString() ?? "0";
                        }
                    }
                }
            }
        }

        // ── Stock Receiving ────────────────────────────────────────────────

        private int? GetValidUserId()
        {
            if (string.IsNullOrEmpty(UserId) || !int.TryParse(UserId, out int userId))
                return null;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(
                    "SELECT COUNT(*) FROM AppUser WHERE user_id = @uid", conn))
                {
                    cmd.Parameters.AddWithValue("@uid", userId);
                    int count = Convert.ToInt32(cmd.ExecuteScalar());
                    return count > 0 ? userId : (int?)null;
                }
            }
        }

        protected void btnSaveStock_Click(object sender, EventArgs e)
        {
            if (!CanAddStock)
            {
                ShowAlert("You do not have permission to add stock.", "warning");
                return;
            }

            // Validate inputs
            if (string.IsNullOrWhiteSpace(ddlVaccineName.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtExpiryDate.Text) ||
                string.IsNullOrWhiteSpace(txtVialCount.Text))
            {
                ShowAlert("Please fill in all required fields.", "warning");
                return;
            }

            if (!int.TryParse(txtVialCount.Text, out int vialCount) || vialCount <= 0)
            {
                ShowAlert("Number of vials must be a positive whole number.", "warning");
                return;
            }

            if (!DateTime.TryParse(txtExpiryDate.Text, out DateTime expiry))
            {
                ShowAlert("Please enter a valid expiry date.", "warning");
                return;
            }

            if (expiry.Date <= DateTime.Today)
            {
                ShowAlert("Expiry date must be in the future.", "warning");
                return;
            }

            int dosesPerVial = Convert.ToInt32(ddlDosesPerVial.SelectedValue);
            int totalDoses = vialCount * dosesPerVial;
            int vaccineId = Convert.ToInt32(ddlVaccineName.SelectedValue);

            int? userId = GetValidUserId();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // 1. Insert VaccineBatch
                    int batchId;
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO VaccineBatch
                            (vaccine_id, manufacturing_date, expiration_date, quantity_received, current_stock, date_received, received_by)
                        VALUES 
                            (@vid, GETDATE(), @exp, @qty, @qty, GETDATE(), @userid);
                        SELECT SCOPE_IDENTITY();", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@vid", vaccineId);
                        cmd.Parameters.AddWithValue("@exp", expiry);
                        cmd.Parameters.AddWithValue("@qty", totalDoses);
                        cmd.Parameters.AddWithValue("@userid", userId.HasValue ? (object)userId.Value : DBNull.Value);
                        batchId = Convert.ToInt32(cmd.ExecuteScalar());
                    }

                    // 2. Insert individual vials
                    string batchNumber = "";
                    using (SqlCommand cmd = new SqlCommand(
                        "SELECT batch_number FROM VaccineBatch WHERE batch_id = @bid", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@bid", batchId);
                        batchNumber = cmd.ExecuteScalar().ToString();
                    }

                    for (int i = 1; i <= vialCount; i++)
                    {
                        string vialNo = batchNumber + "-" + i.ToString("D3");
                        using (SqlCommand cmd = new SqlCommand(@"
                            INSERT INTO VaccineVial
                                (batch_id, vial_no, doses_per_vial, doses_used, vial_status)
                            VALUES 
                                (@bid, @vialNo, @dpv, 0, 'Sealed')", conn, trans))
                        {
                            cmd.Parameters.AddWithValue("@bid", batchId);
                            cmd.Parameters.AddWithValue("@vialNo", vialNo);
                            cmd.Parameters.AddWithValue("@dpv", dosesPerVial);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // 3. Insert InventoryLog
                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO InventoryLog 
                            (batch_id, transaction_type, quantity, transaction_date, updated_by, reference_id)
                        VALUES 
                            (@bid, 'Received', @qty, GETDATE(), @userId, NULL)", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@bid", batchId);
                        cmd.Parameters.AddWithValue("@qty", totalDoses);
                        cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }

                    // 4. Insert AuditLog
                    string vaccineName = ddlVaccineName.SelectedItem.Text;
                    string auditData = $@"{{""vaccine"":""{vaccineName}"",""batch"":""{batchNumber}"",""vials"":{vialCount},""doses_per_vial"":{dosesPerVial},""total_doses"":{totalDoses},""expiry"":""{expiry:yyyy-MM-dd}""}}";

                    using (SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO AuditLog
                            (table_name, record_id, action, new_data, performed_by, ip_address, performed_at)
                        VALUES 
                            ('VaccineBatch', @recordId, 'INSERT', @newData, @userId, @ip, GETDATE())", conn, trans))
                    {
                        cmd.Parameters.AddWithValue("@recordId", batchId.ToString());
                        cmd.Parameters.AddWithValue("@newData", auditData);
                        cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                        cmd.Parameters.AddWithValue("@ip", Request.UserHostAddress ?? "Unknown");
                        cmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                    ShowAlert($"Successfully added {vialCount} vials ({totalDoses} doses) of {vaccineName}.", "success");
                    ClearAddStockFields();
                    ShowAddStock();
                    LoadOverviewStats();
                }
                catch (SqlException sqlEx) when (sqlEx.Number == 547)
                {
                    trans.Rollback();
                    ShowAlert("Unable to save: The user record was not found. Please log in again.", "error");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ShowAlert("Error saving stock: " + ex.Message, "error");
                }
            }
        }

        private void LoadStockHistory()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 50 
                        l.transaction_date, 
                        v.vaccine_name,
                        b.batch_number, 
                        l.quantity,
                        ISNULL(u.fname + ' ' + u.lname, 'System') AS updated_by,
                        (SELECT COUNT(*) FROM VaccineVial WHERE batch_id = b.batch_id) AS vials
                    FROM InventoryLog l
                    JOIN VaccineBatch b ON l.batch_id = b.batch_id
                    JOIN Vaccine v ON b.vaccine_id = v.vaccine_id
                    LEFT JOIN AppUser u ON l.updated_by = u.user_id
                    WHERE l.transaction_type = 'Received'
                    ORDER BY l.transaction_date DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                ViewState["StockHistoryData"] = dt;
                gvStockHistory.DataSource = dt;
                gvStockHistory.DataBind();
            }
        }

        protected void gvStockHistory_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvStockHistory.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["StockHistoryData"] as DataTable;
            if (dt == null) LoadStockHistory(); else { gvStockHistory.DataSource = dt; gvStockHistory.DataBind(); }
        }

        // ── Audit Log ──────────────────────────────────────────────────────

        private void LogAudit(SqlConnection conn, SqlTransaction trans, string tableName, string recordId, string action, string newData)
        {
            int? userId = GetValidUserId();

            using (SqlCommand cmd = new SqlCommand(@"
                INSERT INTO AuditLog
                    (table_name, record_id, action, new_data, performed_by, ip_address, performed_at)
                VALUES 
                    (@table, @recordId, @action, @newData, @userId, @ip, GETDATE())", conn, trans))
            {
                cmd.Parameters.AddWithValue("@table", tableName);
                cmd.Parameters.AddWithValue("@recordId", recordId);
                cmd.Parameters.AddWithValue("@action", action);
                cmd.Parameters.AddWithValue("@newData", newData);
                cmd.Parameters.AddWithValue("@userId", userId.HasValue ? (object)userId.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@ip", Request.UserHostAddress ?? "Unknown");
                cmd.ExecuteNonQuery();
            }
        }

        private void BindAuditLog(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 100
                        audit_id,
                        table_name,
                        record_id,
                        action,
                        old_data,
                        new_data,
                        ISNULL(u.fname + ' ' + u.lname, CAST(a.performed_by AS VARCHAR(50))) AS performed_by,
                        ip_address,
                        performed_at
                    FROM AuditLog a
                    LEFT JOIN AppUser u ON a.performed_by = u.user_id
                    WHERE (@search = '' OR 
                           table_name LIKE @search OR 
                           action LIKE @search OR
                           ISNULL(u.fname, '') LIKE @search OR 
                           ISNULL(u.lname, '') LIKE @search)
                    ORDER BY performed_at DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search",
                    string.IsNullOrWhiteSpace(search) ? "" : "%" + search + "%");
                DataTable dt = new DataTable();
                da.Fill(dt);
                ViewState["AuditData"] = dt;
                gvAuditLog.DataSource = dt;
                gvAuditLog.DataBind();
            }
        }

        protected void btnAuditSearch_Click(object sender, EventArgs e)
        {
            BindAuditLog(txtAuditSearch.Text.Trim());
        }

        protected void btnAuditClear_Click(object sender, EventArgs e)
        {
            txtAuditSearch.Text = "";
            BindAuditLog();
        }

        protected void gvAuditLog_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvAuditLog.PageIndex = e.NewPageIndex;
            DataTable dt = ViewState["AuditData"] as DataTable;
            if (dt == null) BindAuditLog(); else { gvAuditLog.DataSource = dt; gvAuditLog.DataBind(); }
        }

        // ── Helpers ─────────────────────────────────────────────────────────

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
            txtVialCount.Text = "";
            if (ddlDosesPerVial.Items.Count > 0) ddlDosesPerVial.SelectedIndex = 0;
        }

        // ── Formatting helpers for the UI ──────────────────────────────────

        protected string GetStockStatusClass(string status)
        {
            switch (status.ToLower())
            {
                case "expired": return "badge-exp";
                case "expiring soon": return "badge-warn";
                default: return "badge-ok";
            }
        }

        protected string GetVialBadgeClass(string status)
        {
            switch (status.ToLower())
            {
                case "sealed": return "badge-ok";
                case "open": return "badge-warn";
                case "empty": return "badge-in";
                case "discarded": return "badge-exp";
                default: return "badge-ok";
            }
        }

        protected string GetActionBadgeClass(string action)
        {
            switch (action.ToUpper())
            {
                case "INSERT": return "badge-ok";
                case "UPDATE": return "badge-warn";
                case "DELETE": return "badge-exp";
                default: return "badge-in";
            }
        }

        private void ShowAlert(string message, string type = "info")
        {
            string safe = message.Replace("\\", "\\\\").Replace("'", "\\'")
                                 .Replace(Environment.NewLine, " ").Replace("\r", "").Replace("\n", " ");
            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(),
                "showNotifyModal('" + safe + "','" + type + "');", true);
        }
    }
}