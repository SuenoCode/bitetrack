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
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Use parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        Session["userRole"] = reader["role"].ToString();
                        Session["fullName"] = reader["full_name"].ToString();
                        // Successful login
                        Response.Redirect("~/Dashboard.aspx");
                    }
                    else
                    {
                        // Failed login
                        errorMsg.Visible = true;
                    }
                }
            }
        }
    }
}