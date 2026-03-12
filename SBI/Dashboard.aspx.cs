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
                    NameID as 'ID',
                    FORMAT(Date, 'MMM dd, yyyy') as 'Date',
                    Place as 'Barangay'
                FROM KyleMonggoloid 
                WHERE Place IS NOT NULL AND Place != 'NULL'
                ORDER BY Date DESC";

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
                            ISNULL(Place, 'Unknown') as Barangay,
                            COUNT(*) as CaseCount
                        FROM KyleMonggoloid
                        WHERE Place IS NOT NULL AND Place != 'NULL'
                        GROUP BY Place
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
                                Barangays = dt.AsEnumerable().Select(r => r["Barangay"].ToString()).ToArray(),
                                CaseCounts = dt.AsEnumerable().Select(r => Convert.ToInt32(r["CaseCount"])).ToArray()
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
                    return true;
                }
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }
    }
}