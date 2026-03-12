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
                string query = "SELECT COUNT(*) FROM Users WHERE username = @username AND password = @password";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    // Use parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password", password);

                    conn.Open();
                    int count = (int)cmd.ExecuteScalar();
                    conn.Close();

                    if (count > 0)
                    {
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