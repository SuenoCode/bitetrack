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
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                string query = @"
                    SELECT TOP 10 
                        patient_id AS 'ID',
                        FORMAT(date_of_bite, 'MMM dd, yyyy') AS 'Date',
                        place_of_bite AS 'Barangay'
                    FROM [Case]
                    WHERE place_of_bite IS NOT NULL 
                          AND place_of_bite <> '' 
                          AND place_of_bite <> 'NULL'
                          AND patient_id IS NOT NULL
                          AND patient_id <> ''
                          AND date_of_bite IS NOT NULL
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
            catch (Exception ex)
            {
                // Log the error and show a friendly message
                gvPreviousCases.EmptyDataText = "Error loading data: " + ex.Message;
                gvPreviousCases.DataBind();
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
                            CASE 
                                WHEN place_of_bite LIKE '%San Pedro%' THEN 'San Pedro'
                                WHEN place_of_bite LIKE '%San Juan%' THEN 'San Juan'
                                WHEN place_of_bite LIKE '%San Guillermo%' THEN 'San Guillermo'
                                WHEN place_of_bite LIKE '%CCL%' THEN 'CCL'
                                WHEN place_of_bite LIKE '%San Jose%' THEN 'San Jose'
                                WHEN place_of_bite LIKE '%Bombongan%' THEN 'Bombongan'
                                WHEN place_of_bite LIKE '%Lagundi%' THEN 'Lagundi'
                                WHEN place_of_bite LIKE '%Maybancal%' THEN 'Maybancal'
                                ELSE 'Other'
                            END AS Barangay,
                            COUNT(*) AS CaseCount
                        FROM [Case]
                        WHERE place_of_bite IS NOT NULL 
                              AND place_of_bite <> '' 
                              AND place_of_bite <> 'NULL'
                        GROUP BY 
                            CASE 
                                WHEN place_of_bite LIKE '%San Pedro%' THEN 'San Pedro'
                                WHEN place_of_bite LIKE '%San Juan%' THEN 'San Juan'
                                WHEN place_of_bite LIKE '%San Guillermo%' THEN 'San Guillermo'
                                WHEN place_of_bite LIKE '%CCL%' THEN 'CCL'
                                WHEN place_of_bite LIKE '%San Jose%' THEN 'San Jose'
                                WHEN place_of_bite LIKE '%Bombongan%' THEN 'Bombongan'
                                WHEN place_of_bite LIKE '%Lagundi%' THEN 'Lagundi'
                                WHEN place_of_bite LIKE '%Maybancal%' THEN 'Maybancal'
                                ELSE 'Other'
                            END
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
                return new { Error = ex.Message, Barangays = new string[0], CaseCounts = new int[0] };
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
                            COUNT(DISTINCT 
                                CASE 
                                    WHEN place_of_bite IS NOT NULL 
                                         AND place_of_bite <> '' 
                                         AND place_of_bite <> 'NULL' 
                                    THEN place_of_bite 
                                END
                            ) AS TotalBarangays
                        FROM [Case]";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = cmd.ExecuteReader();

                        if (reader.Read())
                        {
                            return new
                            {
                                TotalCases = reader["TotalCases"] != DBNull.Value ? Convert.ToInt32(reader["TotalCases"]) : 0,
                                TotalBarangays = reader["TotalBarangays"] != DBNull.Value ? Convert.ToInt32(reader["TotalBarangays"]) : 0
                            };
                        }
                    }
                }

                return new { TotalCases = 0, TotalBarangays = 0 };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message, TotalCases = 0, TotalBarangays = 0 };
            }
        }

        [WebMethod]
        public static object GetCaseSummary()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT 
                            COUNT(*) AS TotalCases,
                            SUM(CASE WHEN category = 'I' THEN 1 ELSE 0 END) AS CategoryI,
                            SUM(CASE WHEN category = 'II' THEN 1 ELSE 0 END) AS CategoryII,
                            SUM(CASE WHEN category = 'III' THEN 1 ELSE 0 END) AS CategoryIII,
                            SUM(CASE WHEN bleeding = 'Yes' THEN 1 ELSE 0 END) AS WithBleeding,
                            SUM(CASE WHEN washed = 'Yes' THEN 1 ELSE 0 END) AS WashedWounds
                        FROM [Case]
                        WHERE patient_id IS NOT NULL 
                              AND patient_id <> ''";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new
                                {
                                    TotalCases = reader["TotalCases"] != DBNull.Value ? Convert.ToInt32(reader["TotalCases"]) : 0,
                                    CategoryI = reader["CategoryI"] != DBNull.Value ? Convert.ToInt32(reader["CategoryI"]) : 0,
                                    CategoryII = reader["CategoryII"] != DBNull.Value ? Convert.ToInt32(reader["CategoryII"]) : 0,
                                    CategoryIII = reader["CategoryIII"] != DBNull.Value ? Convert.ToInt32(reader["CategoryIII"]) : 0,
                                    WithBleeding = reader["WithBleeding"] != DBNull.Value ? Convert.ToInt32(reader["WithBleeding"]) : 0,
                                    WashedWounds = reader["WashedWounds"] != DBNull.Value ? Convert.ToInt32(reader["WashedWounds"]) : 0
                                };
                            }
                        }
                    }
                }

                return new { TotalCases = 0, CategoryI = 0, CategoryII = 0, CategoryIII = 0, WithBleeding = 0, WashedWounds = 0 };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message };
            }
        }
    }
}