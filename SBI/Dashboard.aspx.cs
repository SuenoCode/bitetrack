using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Services;
using System.Web.UI;

namespace SBI
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BindPreviousCases();
            }
        }

        private void BindPreviousCases()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

            string query = @"
                SELECT TOP 10 
                    patient_id AS 'ID',
                    FORMAT(date_of_bite, 'MMM dd, yyyy') AS 'Date',
                    place_of_bite AS 'Barangay'
                FROM BiteCase
                WHERE place_of_bite IS NOT NULL 
                      AND place_of_bite <> '' 
                      AND place_of_bite <> 'NULL'
                ORDER BY date_of_bite DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    conn.Open();

                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    gvPreviousCases.DataSource = dt;
                    gvPreviousCases.DataBind();
                }
            }
        }

        [WebMethod]
        public static object GetBarangayCases()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            ISNULL(place_of_bite, 'Unknown') AS Barangay,
                            COUNT(*) AS CaseCount
                        FROM BiteCase
                        WHERE place_of_bite IS NOT NULL 
                              AND place_of_bite <> '' 
                              AND place_of_bite <> 'NULL'
                        GROUP BY place_of_bite
                        ORDER BY CaseCount DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            DataTable dt = new DataTable();
                            dt.Load(reader);

                            var result = new
                            {
                                Barangays = dt.AsEnumerable()
                                    .Select(r => r["Barangay"].ToString())
                                    .ToArray(),

                                CaseCounts = dt.AsEnumerable()
                                    .Select(r => Convert.ToInt32(r["CaseCount"]))
                                    .ToArray()
                            };

                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }

        [WebMethod]
        public static object GetDashboardStats()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    string query = @"
                        SELECT 
                            COUNT(*) AS TotalCases,
                            COUNT(DISTINCT place_of_bite) AS TotalBarangays
                        FROM BiteCase";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            return new
                            {
                                TotalCases = reader["TotalCases"],
                                TotalBarangays = reader["TotalBarangays"]
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }
    }
}