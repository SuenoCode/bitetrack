using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Transactions;

namespace SBI
{
    public partial class VaccineManagement : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null ||
                Session["userRole"].ToString().ToLower() != "adminassistant" &&
                Session["userRole"].ToString().ToLower() != "vaccinators")
            {
                Response.Redirect("Login.aspx");
            }

            if (!IsPostBack)
            {
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

                // Total available doses
                string qTotal = "SELECT ISNULL(SUM(current_stock), 0) FROM VaccineBatch WHERE expiration_date >= GETDATE()";
                SqlCommand cmdTotal = new SqlCommand(qTotal, conn);
                lblTotalDoses.Text = cmdTotal.ExecuteScalar().ToString();

                // Expiring within 30 days
                string qExp30 = @"SELECT ISNULL(SUM(current_stock), 0) FROM VaccineBatch
                                  WHERE expiration_date >= GETDATE()
                                  AND expiration_date <= DATEADD(DAY, 30, GETDATE())";
                SqlCommand cmdExp30 = new SqlCommand(qExp30, conn);
                lblExpiring30.Text = cmdExp30.ExecuteScalar().ToString();

                // Expired
                string qExpired = @"SELECT ISNULL(SUM(current_stock), 0) FROM VaccineBatch
                                    WHERE expiration_date < GETDATE()";
                SqlCommand cmdExpired = new SqlCommand(qExpired, conn);
                lblExpired.Text = cmdExpired.ExecuteScalar().ToString();

                // Administered MTD
                string qMTD = @"SELECT ISNULL(COUNT(*), 0) FROM VaccineBatch
                                WHERE MONTH(manufacturing_date) = MONTH(GETDATE())
                                AND YEAR(manufacturing_date) = YEAR(GETDATE())";
                SqlCommand cmdMTD = new SqlCommand(qMTD, conn);
                lblAdministeredMTD.Text = cmdMTD.ExecuteScalar().ToString();
            }
        }

        // ============================================================
        // ADD STOCK
        // ============================================================
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

        protected void btnAddStock_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ddlVaccineName.SelectedValue) ||
                string.IsNullOrWhiteSpace(txtBatchNumber.Text) ||
                string.IsNullOrWhiteSpace(txtExpiryDate.Text) ||
                string.IsNullOrWhiteSpace(txtQuantity.Text))
            {
                ShowAlert("Please fill in all fields.");
                return;
            }

            int quantity;
            DateTime expiryDate;

            if (!int.TryParse(txtQuantity.Text, out quantity) || quantity <= 0)
            {
                ShowAlert("Please enter a valid quantity.");
                return;
            }

            if (!DateTime.TryParse(txtExpiryDate.Text, out expiryDate))
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
                    // Insert into VaccineBatch
                    string insertBatch = @"
                        INSERT INTO VaccineBatch 
                            (batch_number, expiration_date, quantity_received, current_stock, date_received)
                        VALUES 
                            (@batch_number, @expiration_date, @quantity, @quantity, GETDATE());
                        SELECT SCOPE_IDENTITY();";

                    SqlCommand cmdBatch = new SqlCommand(insertBatch, conn, trans);
                    cmdBatch.Parameters.AddWithValue("@batch_number", txtBatchNumber.Text.Trim());
                    cmdBatch.Parameters.AddWithValue("@expiration_date", expiryDate);
                    cmdBatch.Parameters.AddWithValue("@quantity", quantity);

                    int batchId = Convert.ToInt32(cmdBatch.ExecuteScalar());

                    // Log to InventoryLog
                    string insertLog = @"
                        INSERT INTO InventoryLog 
                            (batch_id, transaction_type, quantity, transaction_date, updated_by)
                        VALUES 
                            (@batch_id, 'In', @quantity, GETDATE(), @updated_by)";

                    SqlCommand cmdLog = new SqlCommand(insertLog, conn, trans);
                    cmdLog.Parameters.AddWithValue("@batch_id", batchId);
                    cmdLog.Parameters.AddWithValue("@quantity", quantity);
                    cmdLog.Parameters.AddWithValue("@updated_by", Session["userName"] ?? "System");

                    cmdLog.ExecuteNonQuery();

                    trans.Commit();

                    ClearAddStockFields();
                    BindInventoryGrid();
                    LoadOverview();
                    ShowAlert("Stock added successfully.");
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
            ddlVaccineName.SelectedIndex = 0;
            txtBatchNumber.Text = "";
            txtExpiryDate.Text = "";
            txtQuantity.Text = "";
        }

        // ============================================================
        // INVENTORY GRID
        // ============================================================
        private void BindInventoryGrid(string search = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        vb.batch_id,
                        v.vaccine_name,
                        vb.batch_number,
                        vb.manufacturing_date,
                        vb.expiration_date,
                        vb.quantity_received,
                        vb.current_stock,
                        vb.date_received,
                        CASE 
                            WHEN vb.expiration_date < GETDATE()                          THEN 'Expired'
                            WHEN vb.expiration_date <= DATEADD(DAY,30,GETDATE())         THEN 'Expiring Soon'
                            ELSE 'Good'
                        END AS stock_status
                    FROM VaccineBatch vb
                    LEFT JOIN Vaccine v ON vb.batch_id = v.batch_id
                    WHERE (@search = '' OR v.vaccine_name LIKE @search OR vb.batch_number LIKE @search)
                    ORDER BY vb.expiration_date ASC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search",
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
        }

        // ============================================================
        // HELPER
        // ============================================================
        private void ShowAlert(string message)
        {
            ClientScript.RegisterStartupScript(this.GetType(), "alert",
                $"alert('{message}');", true);
        }
        string query = @"
    SELECT 
        vb.batch_id,
        v.vaccine_name,
        vb.batch_number,
        vb.manufacturing_date,
        vb.expiration_date,
        vb.quantity_received,
        vb.current_stock,
        vb.date_received,
        CASE 
            WHEN vb.expiration_date < GETDATE()                      THEN 'Expired'
            WHEN vb.expiration_date <= DATEADD(DAY, 30, GETDATE())   THEN 'Expiring Soon'
            ELSE 'Good'
        END AS stock_status
    FROM VaccineBatch vb
    LEFT JOIN Vaccine v ON vb.batch_id = v.batch_id
    WHERE (@search = '' OR v.vaccine_name LIKE @search OR vb.batch_number LIKE @search)
    ORDER BY vb.expiration_date ASC";
    }
}