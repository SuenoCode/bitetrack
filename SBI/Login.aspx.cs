using System;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace SBI
{
    public partial class Login : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text.Trim();

            string connStr = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT role, fname + ' ' + lname AS full_name FROM Users WHERE username = @username AND password = @password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar, 100).Value = password;

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        string role = reader["role"].ToString().Trim();
                        string fullName = reader["full_name"].ToString().Trim();

                        Session["userRole"] = role;
                        Session["fullName"] = fullName;

                        // A = Admin, B = Admin Assistant, C = Vaccinator
                        if (role == "A")
                            Response.Redirect("UserManagement.aspx");
                        else if (role == "B" || role == "C")
                            Response.Redirect("Dashboard.aspx");
                        else
                        {
                            errorMsg.Text = "Your account has no assigned role. Contact the administrator.";
                            errorMsg.Visible = true;
                        }
                    }
                    else
                    {
                        errorMsg.Text = "Invalid username or password.";
                        errorMsg.Visible = true;
                    }
                }
            }
        }
    }
}