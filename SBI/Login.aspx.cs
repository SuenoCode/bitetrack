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
                // FIX: Include user_id in the SELECT query
                string query = @"SELECT user_id, role_id, fname + ' ' + lname AS full_name 
                                  FROM AppUser 
                                  WHERE username = @username AND password_hash = @password";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.Add("@username", SqlDbType.VarChar, 50).Value = username;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar, 100).Value = password;
                    conn.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.HasRows)
                        {
                            reader.Read();

                            // FIX: Get the user_id from the database
                            int userId = Convert.ToInt32(reader["user_id"]);
                            string roleId = reader["role_id"].ToString().Trim();
                            string fullName = reader["full_name"].ToString().Trim();

                            // Map numeric role_id -> letter code used across the app
                            // 1 = Admin -> A, 2 = AdminAssistant -> B, 3 = Vaccinator -> C
                            string roleCode;
                            switch (roleId)
                            {
                                case "1": roleCode = "A"; break;
                                case "2": roleCode = "B"; break;
                                case "3": roleCode = "C"; break;
                                default: roleCode = ""; break;
                            }

                            if (roleCode == "")
                            {
                                errorMsg.Text = "Your account has no assigned role. Contact the administrator.";
                                errorMsg.Visible = true;
                                return;
                            }

                            // FIX: Store userId in session
                            Session["userId"] = userId.ToString();
                            Session["userRole"] = roleCode;
                            Session["fullName"] = fullName;

                            Response.Redirect("Dashboard.aspx");
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
}