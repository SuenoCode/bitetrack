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

            // Get connection string from Web.config
            string connStr = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

            using (SqlConnection conn = new SqlConnection(connStr))
            {
                string query = "SELECT role, full_name FROM Users WHERE username = @username AND password = @password";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Use explicit parameter types for reliability
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar, 50).Value = password;

                    conn.Open();

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        // Store user info in session (optional)
                        Session["userRole"] = reader["role"].ToString().Trim();
                        Session["fullName"] = reader["full_name"].ToString().Trim();

                        string role = Session["userRole"] as string;

                        if (role == "adminAssistant" || role == "vaccinators")
                        {
                            Response.Redirect("Dashboard.aspx");
                        }
                        else if (role == "admin")
                        {
                            Response.Redirect("UserManagement.aspx");
                        }
                    }
                    else
                    {
                        // Invalid login
                        errorMsg.Text = "Invalid username or password.";
                        errorMsg.Visible = true;
                    }
                }
            }
        }
    }
}