using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Services;
using System.Web.UI;
using System.Web.Script.Services;

namespace SBI
{
    public partial class Dashboard : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            { Response.Redirect("Login.aspx"); return; }

            string role = Session["userRole"].ToString().ToUpper();
            if (role != "A" && role != "B" && role != "C")
            { Response.Redirect("Login.aspx"); return; }

            if (!IsPostBack)
            {
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetBarangayCases()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Using bite_barangay from the Case table (since PlaceOfBite doesn't exist)
                    string query = @"
                        SELECT
                            CASE
                                WHEN c.bite_barangay LIKE '%San Pedro%'    THEN 'San Pedro'
                                WHEN c.bite_barangay LIKE '%San Juan%'     THEN 'San Juan'
                                WHEN c.bite_barangay LIKE '%San Guillermo%'THEN 'San Guillermo'
                                WHEN c.bite_barangay LIKE '%CCL%'          THEN 'CCL'
                                WHEN c.bite_barangay LIKE '%San Jose%'     THEN 'San Jose'
                                WHEN c.bite_barangay LIKE '%Bombongan%'    THEN 'Bombongan'
                                WHEN c.bite_barangay LIKE '%Lagundi%'      THEN 'Lagundi'
                                WHEN c.bite_barangay LIKE '%Maybancal%'    THEN 'Maybancal'
                                WHEN c.bite_barangay LIKE '%San Francisco%'THEN 'San Francisco'
                                WHEN c.bite_barangay LIKE '%San Vicente%'  THEN 'San Vicente'
                                ELSE 'Other'
                            END AS Barangay,
                            COUNT(*) AS CaseCount
                        FROM dbo.[Case] c
                        WHERE c.bite_barangay IS NOT NULL
                          AND c.bite_barangay <> ''
                        GROUP BY
                            CASE
                                WHEN c.bite_barangay LIKE '%San Pedro%'    THEN 'San Pedro'
                                WHEN c.bite_barangay LIKE '%San Juan%'     THEN 'San Juan'
                                WHEN c.bite_barangay LIKE '%San Guillermo%'THEN 'San Guillermo'
                                WHEN c.bite_barangay LIKE '%CCL%'          THEN 'CCL'
                                WHEN c.bite_barangay LIKE '%San Jose%'     THEN 'San Jose'
                                WHEN c.bite_barangay LIKE '%Bombongan%'    THEN 'Bombongan'
                                WHEN c.bite_barangay LIKE '%Lagundi%'      THEN 'Lagundi'
                                WHEN c.bite_barangay LIKE '%Maybancal%'    THEN 'Maybancal'
                                WHEN c.bite_barangay LIKE '%San Francisco%'THEN 'San Francisco'
                                WHEN c.bite_barangay LIKE '%San Vicente%'  THEN 'San Vicente'
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

                            return new
                            {
                                Barangays = dt.AsEnumerable().Select(r => r["Barangay"].ToString()).ToArray(),
                                CaseCounts = dt.AsEnumerable().Select(r => Convert.ToInt32(r["CaseCount"])).ToArray()
                            };
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
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetDashboardStats()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    conn.Open();

                    // Fixed: Using proper table names from your schema
                    string query = (@"
                        SELECT
                            (SELECT COUNT(*) FROM dbo.[Case]) AS TotalCases,
                            (SELECT COUNT(DISTINCT ISNULL(bite_barangay, '')) FROM dbo.[Case] WHERE bite_barangay IS NOT NULL AND bite_barangay <> '') AS TotalBarangays,
                            (SELECT COUNT(*) FROM dbo.[Case] WHERE category = 'III') AS HighRiskCases,
                            (SELECT COUNT(*) FROM dbo.[Case] WHERE category IS NOT NULL AND category <> '') AS TotalCategories,
                            (SELECT COUNT(*) FROM ScheduledDose WHERE status = 'Pending') AS OngoingTreatments,
                            (SELECT COUNT(*) FROM ScheduledDose WHERE status = 'Completed') AS CompletedCases,
                            (SELECT ISNULL(SUM(current_stock), 0) FROM VaccineBatch WHERE expiration_date < CAST(GETDATE() AS DATE)) AS StockAlerts,
                            (SELECT COUNT(*) FROM dbo.[Case] WHERE MONTH(date_of_bite) = MONTH(GETDATE()) AND YEAR(date_of_bite) = YEAR(GETDATE())) AS MonthlyCases
                    ");

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new
                            {
                                TotalCases = reader["TotalCases"] != DBNull.Value ? Convert.ToInt32(reader["TotalCases"]) : 0,
                                TotalBarangays = reader["TotalBarangays"] != DBNull.Value ? Convert.ToInt32(reader["TotalBarangays"]) : 0,
                                HighRiskCases = reader["HighRiskCases"] != DBNull.Value ? Convert.ToInt32(reader["HighRiskCases"]) : 0,
                                OngoingTreatments = reader["OngoingTreatments"] != DBNull.Value ? Convert.ToInt32(reader["OngoingTreatments"]) : 0,
                                CompletedCases = reader["CompletedCases"] != DBNull.Value ? Convert.ToInt32(reader["CompletedCases"]) : 0,
                                StockAlerts = reader["StockAlerts"] != DBNull.Value ? Convert.ToInt32(reader["StockAlerts"]) : 0,
                                MonthlyCases = reader["MonthlyCases"] != DBNull.Value ? Convert.ToInt32(reader["MonthlyCases"]) : 0
                            };
                        }
                    }
                }

                return new { TotalCases = 0, TotalBarangays = 0, HighRiskCases = 0, OngoingTreatments = 0, CompletedCases = 0, StockAlerts = 0, MonthlyCases = 0 };
            }
            catch (Exception ex)
            {
                return new { Error = ex.Message, TotalCases = 0, TotalBarangays = 0, HighRiskCases = 0, OngoingTreatments = 0, CompletedCases = 0, StockAlerts = 0, MonthlyCases = 0 };
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetCaseSummary()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            COUNT(*)                                                   AS TotalCases,
                            SUM(CASE WHEN category = 'I'   THEN 1 ELSE 0 END)         AS CategoryI,
                            SUM(CASE WHEN category = 'II'  THEN 1 ELSE 0 END)         AS CategoryII,
                            SUM(CASE WHEN category = 'III' THEN 1 ELSE 0 END)         AS CategoryIII,
                            SUM(CASE WHEN bleeding = 'Yes' THEN 1 ELSE 0 END)         AS WithBleeding,
                            SUM(CASE WHEN washed   = 'Yes' THEN 1 ELSE 0 END)         AS WashedWounds
                        FROM dbo.[Case]
                        WHERE patient_id IS NOT NULL AND patient_id <> ''";

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

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetMonthlyCases()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
                string[] months = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
                int[] counts = new int[12];

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT MONTH(date_of_bite) AS Month, COUNT(*) AS Total
                        FROM dbo.[Case]
                        WHERE YEAR(date_of_bite) = YEAR(GETDATE())
                          AND date_of_bite IS NOT NULL
                        GROUP BY MONTH(date_of_bite)";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                            while (dr.Read())
                                counts[Convert.ToInt32(dr["Month"]) - 1] = Convert.ToInt32(dr["Total"]);
                    }
                }
                return new { Months = months, Counts = counts };
            }
            catch (Exception ex) { return new { Error = ex.Message }; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetWeeklyVaccineUsage()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
                string[] weeks = { "Week 1", "Week 2", "Week 3", "Week 4" };
                int[] counts = new int[4];

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Using InventoryLog with 'Dispensed' type for vaccine usage
                    string query = @"
                        SELECT 
                            CASE 
                                WHEN DAY(transaction_date) <= 7 THEN 1
                                WHEN DAY(transaction_date) <= 14 THEN 2
                                WHEN DAY(transaction_date) <= 21 THEN 3
                                ELSE 4
                            END AS WeekNum, 
                            COUNT(*) AS Total
                        FROM dbo.InventoryLog
                        WHERE transaction_type = 'Dispensed'
                          AND MONTH(transaction_date) = MONTH(GETDATE())
                          AND YEAR(transaction_date) = YEAR(GETDATE())
                        GROUP BY 
                            CASE 
                                WHEN DAY(transaction_date) <= 7 THEN 1
                                WHEN DAY(transaction_date) <= 14 THEN 2
                                WHEN DAY(transaction_date) <= 21 THEN 3
                                ELSE 4
                            END";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader dr = cmd.ExecuteReader())
                            while (dr.Read())
                            {
                                int w = Convert.ToInt32(dr["WeekNum"]);
                                if (w >= 1 && w <= 4) counts[w - 1] = Convert.ToInt32(dr["Total"]);
                            }
                    }
                }
                return new { Weeks = weeks, Counts = counts };
            }
            catch (Exception ex) { return new { Error = ex.Message }; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetCasesByCategory()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            CASE category
                                WHEN 'I'   THEN 'Category I'
                                WHEN 'II'  THEN 'Category II'
                                WHEN 'III' THEN 'Category III'
                                ELSE 'Unknown'
                            END AS Label,
                            COUNT(*) AS Total
                        FROM dbo.[Case]
                        WHERE category IS NOT NULL AND category <> ''
                        GROUP BY category
                        ORDER BY category";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        return new
                        {
                            Labels = dt.AsEnumerable().Select(r => r["Label"].ToString()).ToArray(),
                            Counts = dt.AsEnumerable().Select(r => Convert.ToInt32(r["Total"])).ToArray()
                        };
                    }
                }
            }
            catch (Exception ex) { return new { Error = ex.Message }; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetCasesByAnimalType()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    // Using animal_type from the Case table
                    string query = @"
                        SELECT
                            CASE
                                WHEN LOWER(animal_type) = 'dog' THEN 'Dog'
                                WHEN LOWER(animal_type) = 'cat' THEN 'Cat'
                                ELSE 'Others'
                            END AS Label,
                            COUNT(*) AS Total
                        FROM dbo.[Case]
                        WHERE animal_type IS NOT NULL AND animal_type <> ''
                        GROUP BY
                            CASE
                                WHEN LOWER(animal_type) = 'dog' THEN 'Dog'
                                WHEN LOWER(animal_type) = 'cat' THEN 'Cat'
                                ELSE 'Others'
                            END
                        ORDER BY Total DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        return new
                        {
                            Labels = dt.AsEnumerable().Select(r => r["Label"].ToString()).ToArray(),
                            Counts = dt.AsEnumerable().Select(r => Convert.ToInt32(r["Total"])).ToArray()
                        };
                    }
                }
            }
            catch (Exception ex) { return new { Error = ex.Message }; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetCasesByExposureType()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            ISNULL(NULLIF(type_of_exposure,''), 'Unknown') AS Label,
                            COUNT(*) AS Total
                        FROM dbo.[Case]
                        WHERE type_of_exposure IS NOT NULL AND type_of_exposure <> ''
                        GROUP BY type_of_exposure
                        ORDER BY Total DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        return new
                        {
                            Labels = dt.AsEnumerable().Select(r => r["Label"].ToString()).ToArray(),
                            Counts = dt.AsEnumerable().Select(r => Convert.ToInt32(r["Total"])).ToArray()
                        };
                    }
                }
            }
            catch (Exception ex) { return new { Error = ex.Message }; }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetCasesByWoundType()
        {
            try
            {
                string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT
                            ISNULL(NULLIF(wound_type,''), 'Unknown') AS Label,
                            COUNT(*) AS Total
                        FROM dbo.[Case]
                        WHERE wound_type IS NOT NULL AND wound_type <> ''
                        GROUP BY wound_type
                        ORDER BY Total DESC";

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        DataTable dt = new DataTable();
                        new SqlDataAdapter(cmd).Fill(dt);
                        return new
                        {
                            Labels = dt.AsEnumerable().Select(r => r["Label"].ToString()).ToArray(),
                            Counts = dt.AsEnumerable().Select(r => Convert.ToInt32(r["Total"])).ToArray()
                        };
                    }
                }
            }
            catch (Exception ex) { return new { Error = ex.Message }; }
        }
    }
}