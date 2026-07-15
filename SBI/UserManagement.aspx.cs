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

        private void BindGrid()
        {
            using (var conn = new SqlConnection(cs))
            {
                string query = @"
                    SELECT 
                        user_id,
                        fname + ' ' + lname AS full_name,
                        username,
                        password_hash AS password,
                        email,
                        contact_no,
                        is_active,
                        created_at,
                        role_id
                    FROM AppUser
                    ORDER BY user_id DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvUsers.DataSource = dt;
                gvUsers.DataBind();
            }
        }

        // Get role label from role_id
        public string GetRoleLabel(object roleId)
        {
            if (roleId == null || roleId == DBNull.Value)
                return "Unknown";

            string id = roleId.ToString();
            switch (id)
            {
                case "1": return "Admin";
                case "2": return "Admin Assistant";
                case "3": return "Vaccinator";
                default: return "Unknown";
            }
        }

        // Get role badge class from role_id
        public string GetRoleBadgeClass(object roleId)
        {
            string base_ = "inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold uppercase tracking-wide ";

            if (roleId == null || roleId == DBNull.Value)
                return base_ + "bg-slate-100 text-slate-600";

            string id = roleId.ToString();
            switch (id)
            {
                case "1": return base_ + "bg-blue-100 text-blue-700";
                case "2": return base_ + "bg-violet-100 text-violet-700";
                case "3": return base_ + "bg-emerald-100 text-emerald-700";
                default: return base_ + "bg-slate-100 text-slate-600";
            }
        }

        // Get status label - ADD THIS METHOD
        public string GetStatusLabel(object isActive)
        {
            if (isActive == null || isActive == DBNull.Value)
                return "Inactive";

            return isActive.ToString() == "Yes" ? "Active" : "Inactive";
        }

        // Get status badge class - ADD THIS METHOD
        public string GetStatusBadgeClass(object isActive)
        {
            string base_ = "inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold uppercase tracking-wide ";

            if (isActive == null || isActive == DBNull.Value)
                return base_ + "bg-red-100 text-red-700";

            return isActive.ToString() == "Yes"
                ? base_ + "bg-green-100 text-green-700"
                : base_ + "bg-red-100 text-red-700";
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
                    "SELECT fname, lname, username, password_hash, role_id, email, contact_no, is_active FROM AppUser WHERE user_id = @id",
                    conn);
                cmd.Parameters.AddWithValue("@id", userId);
                conn.Open();
                using (var dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        txtFullName.Text = dr["fname"].ToString() + " " + dr["lname"].ToString();
                        txtUsername.Text = dr["username"].ToString();
                        txtPassword.Text = "";
                        litCurrentPassword.Text = "••••••••";
                        panelCurrentPassword.Visible = true;
                        ddlRole.SelectedValue = dr["role_id"].ToString();
                        txtEmail.Text = dr["email"].ToString();
                        txtContactNo.Text = dr["contact_no"].ToString();
                        chkIsActive.Checked = dr["is_active"].ToString() == "Yes";
                    }
                }
            }
        }

        private void ClearFields()
        {
            txtFullName.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtEmail.Text = "";
            txtContactNo.Text = "";
            ddlRole.SelectedIndex = 0;
            chkIsActive.Checked = true;
            lblFormError.Visible = false;
            panelCurrentPassword.Visible = false;
            litCurrentPassword.Text = "";
        }

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
                    {
                        sql = @"UPDATE AppUser 
                                SET fname = @fn,
                                    lname = @ln,
                                    username = @un,
                                    password_hash = @pw,
                                    role_id = @r,
                                    email = @email,
                                    contact_no = @contact,
                                    is_active = @active
                                WHERE user_id = @id";
                    }
                    else
                    {
                        sql = @"UPDATE AppUser 
                                SET fname = @fn,
                                    lname = @ln,
                                    username = @un,
                                    role_id = @r,
                                    email = @email,
                                    contact_no = @contact,
                                    is_active = @active
                                WHERE user_id = @id";
                    }

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@fn", fname);
                    cmd.Parameters.AddWithValue("@ln", lname);
                    cmd.Parameters.AddWithValue("@un", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@r", int.Parse(ddlRole.SelectedValue));
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@contact", txtContactNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@active", chkIsActive.Checked ? "Yes" : "No");
                    cmd.Parameters.AddWithValue("@id", uid);

                    if (!string.IsNullOrWhiteSpace(txtPassword.Text))
                        cmd.Parameters.AddWithValue("@pw", txtPassword.Text.Trim());

                    cmd.ExecuteNonQuery();
                    ShowAlert("User updated successfully.");
                }
                else
                {
                    string sql = @"INSERT INTO AppUser 
                                   (fname, lname, username, password_hash, role_id, email, contact_no, is_active, created_at)
                                   VALUES (@fn, @ln, @un, @pw, @r, @email, @contact, @active, GETDATE())";

                    SqlCommand cmd = new SqlCommand(sql, conn);
                    cmd.Parameters.AddWithValue("@fn", fname);
                    cmd.Parameters.AddWithValue("@ln", lname);
                    cmd.Parameters.AddWithValue("@un", txtUsername.Text.Trim());
                    cmd.Parameters.AddWithValue("@pw", txtPassword.Text.Trim());
                    cmd.Parameters.AddWithValue("@r", int.Parse(ddlRole.SelectedValue));
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@contact", txtContactNo.Text.Trim());
                    cmd.Parameters.AddWithValue("@active", chkIsActive.Checked ? "Yes" : "No");
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
                    string sql = "DELETE FROM AppUser WHERE user_id = @id";
                    SqlCommand cmd = new SqlCommand(sql, conn);
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