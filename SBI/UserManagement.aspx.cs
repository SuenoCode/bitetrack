using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SBI
{
    public partial class UserManagement : Page
    {
        string cs = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null || Session["userRole"].ToString().ToUpper() != "A")
                Response.Redirect("Login.aspx");

            if (!IsPostBack)
                BindGrid();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private void BindGrid()
        {
            using (var conn = new SqlConnection(cs))
            {
                var da = new SqlDataAdapter(
                    "SELECT user_id, fname + ' ' + lname AS full_name, username, password, role FROM Users ORDER BY user_id DESC",
                    conn);
                var dt = new DataTable();
                da.Fill(dt);
                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        private void SplitFullName(string fullName, out string fname, out string lname)
        {
            var parts = fullName.Trim().Split(new char[] { ' ' }, 2);
            fname = parts[0];
            lname = parts.Length > 1 ? parts[1] : "";
        }

        private void SetAddMode()
        {
            litFormTitle.Text = "Add New User";
            btnCancelEdit.Visible = false;
            panelCurrentPassword.Visible = false;
            hfEditMode.Value = "false";
            hfEditUserId.Value = "";
            ClearFields();
        }

        private void SetEditMode(int userId)
        {
            hfEditMode.Value = "true";
            hfEditUserId.Value = userId.ToString();
            litFormTitle.Text = "Edit User";
            btnCancelEdit.Visible = true;

            using (var conn = new SqlConnection(cs))
            {
                var cmd = new SqlCommand(
                    "SELECT fname, lname, username, password, role FROM Users WHERE user_id = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        txtFullName.Text = dr["fname"].ToString() + " " + dr["lname"].ToString();
                        txtUsername.Text = dr["username"].ToString();
                        txtPassword.Text = "";
                        litCurrentPassword.Text = dr["password"].ToString();
                        panelCurrentPassword.Visible = true;
                        ddlRole.SelectedValue = dr["role"].ToString();
                    }
                }
            }
        }

        private void ClearFields()
        {
            txtFullName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            ddlRole.SelectedIndex = 0;
            lblFormError.Visible = false;
            panelCurrentPassword.Visible = false;
            litCurrentPassword.Text = "";
        }

        // ── Role badge helpers (called from markup) ───────────────────────

        public string GetRoleLabel(string code)
        {
            switch (code)
            {
                case "A": return "Admin";
                case "B": return "Admin Assistant";
                case "C": return "Vaccinator";
                default: return code;
            }
        }

        public string GetRoleBadgeClass(string code)
        {
            string base_ = "inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold uppercase tracking-wide ";
            switch (code)
            {
                case "A": return base_ + "bg-blue-100 text-blue-700";
                case "B": return base_ + "bg-violet-100 text-violet-700";
                case "C": return base_ + "bg-emerald-100 text-emerald-700";
                default: return base_ + "bg-slate-100 text-slate-600";
            }
        }

        // ── Events ────────────────────────────────────────────────────────

        protected void btnSave_Click(object sender, EventArgs e)
        {
            bool isEdit = hfEditMode.Value == "true";

            if (string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(ddlRole.SelectedValue) ||
                (!isEdit && string.IsNullOrWhiteSpace(txtPassword.Text)))
            {
                lblFormError.Text = "Please fill in all required fields.";
                lblFormError.Visible = true;
                return;
            }

            lblFormError.Visible = false;

            string fname, lname;
            SplitFullName(txtFullName.Text, out fname, out lname);

            using (var conn = new SqlConnection(cs))
            {
                conn.Open();

                if (isEdit)
                {
                    int uid = int.Parse(hfEditUserId.Value);
                    string sql;

                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        sql = @"UPDATE Users 
                                SET fname    = @fn,
                                    lname    = @ln,
                                    username = @un,
                                    password = @pw,
                                    role     = @r
                                WHERE user_id = @id";
                    else
                        sql = @"UPDATE Users 
                                SET fname    = @fn,
                                    lname    = @ln,
                                    username = @un,
                                    role     = @r
                                WHERE user_id = @id";

                    var cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@fn", fname);
                    cmd.Parameters.AddWithValue("@ln", lname);
                    cmd.Parameters.AddWithValue("@un", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@r", ddlRole.SelectedValue);
                    cmd.Parameters.AddWithValue("@id", uid);

                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        cmd.Parameters.AddWithValue("@pw", txtPassword.Text.Trim());

                    cmd.ExecuteNonQuery();
                    ShowAlert("User updated successfully.");
                }
                else
                {
                    var cmd = new SqlCommand(
                        @"INSERT INTO Users (fname, lname, username, password, role)
                          VALUES (@fn, @ln, @un, @pw, @r)", conn);
                    cmd.Parameters.AddWithValue("@fn", fname);
                    cmd.Parameters.AddWithValue("@ln", lname);
                    cmd.Parameters.AddWithValue("@un", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@pw", txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@r", ddlRole.SelectedValue);
                    cmd.ExecuteNonQuery();
                    ShowAlert("User added successfully.");
                }
            }

            SetAddMode();
            BindGrid();
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            SetAddMode();
            BindGrid();
        }

        protected void gvUsers_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int idx = int.Parse(e.CommandArgument.ToString());
            int userId = int.Parse(gvUsers.DataKeys[idx].Value.ToString());

            if (e.CommandName == "EditUser")
            {
                SetEditMode(userId);
            }
            else if (e.CommandName == "DeleteUser")
            {
                using (var conn = new SqlConnection(cs))
                {
                    var cmd = new SqlCommand(
                        "DELETE FROM Users WHERE user_id = @id", conn);
                    cmd.Parameters.AddWithValue("@id", userId);
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
                SetAddMode();
                BindGrid();
                ShowAlert("User deleted successfully.");
            }
        }

        private void ShowAlert(string msg)
        {
            ClientScript.RegisterStartupScript(GetType(), "alert",
                $"alert('{msg}');", true);
        }
    }
}