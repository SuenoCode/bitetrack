using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI.WebControls;

namespace SBI
{
    public partial class PatientRegistration : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;
        private const int PageSize = 6; // Number of records per page

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            { Response.Redirect("Login.aspx"); return; }

            string role = Session["userRole"].ToString().ToUpper();
            if (role != "A" && role != "B" && role != "C")
            { Response.Redirect("Login.aspx"); return; }

            if (!IsPostBack)
            {
                FBindGrid();
                HideRecordPreview();

                hfActivePanel.Value = "viewPatientPanel";
                hfSelectedPatientId.Value = "";
                hfSelectedCaseId.Value = "";
                hfEditMode.Value = "";
                hfCasePatientId.Value = "";
            }
        }

        private void FBindGrid()
        {
            BindPatients(txtSearchPatient.Text.Trim(), ParseNullableDate(txtPatientDateFrom.Text), ParseNullableDate(txtPatientDateTo.Text));
            BindCases(txtSearchCase.Text.Trim(), ParseNullableDate(txtCaseDateFrom.Text), ParseNullableDate(txtCaseDateTo.Text));
        }

        // ── Patient / Case list binding with pagination ─────────────────────

        private void BindPatients(string searchText = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Get total count for pagination
                string countQuery = @"
                    SELECT COUNT(*)
                    FROM dbo.Patient p
                    WHERE
                        (@search = '' OR
                         CAST(p.patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         p.fname LIKE '%' + @search + '%' OR
                         p.lname LIKE '%' + @search + '%' OR
                         (p.fname + ' ' + p.lname) LIKE '%' + @search + '%' OR
                         p.contact_no LIKE '%' + @search + '%' OR
                         ISNULL(p.barangay, '') LIKE '%' + @search + '%' OR
                         ISNULL(p.city_province, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR CAST(p.date_recorded AS DATE) >= @fromDate)
                        AND (@toDate   IS NULL OR CAST(p.date_recorded AS DATE) <= @toDate)";

                SqlCommand countCmd = new SqlCommand(countQuery, conn);
                countCmd.Parameters.AddWithValue("@search", searchText);
                countCmd.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                countCmd.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);
                conn.Open();
                int totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                conn.Close();

                // Calculate page index
                int pageIndex = GetPatientPageIndex();
                int startIndex = pageIndex * PageSize;

                string query = @"
                    SELECT
                        p.patient_id,
                        p.fname,
                        p.lname,
                        p.gender,
                        p.contact_no,
                        ISNULL(p.barangay, '') + CASE WHEN p.city_province IS NOT NULL AND p.city_province != '' THEN ', ' + p.city_province ELSE '' END AS address,
                        p.date_recorded AS date_added
                    FROM dbo.Patient p
                    WHERE
                        (@search = '' OR
                         CAST(p.patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         p.fname LIKE '%' + @search + '%' OR
                         p.lname LIKE '%' + @search + '%' OR
                         (p.fname + ' ' + p.lname) LIKE '%' + @search + '%' OR
                         p.contact_no LIKE '%' + @search + '%' OR
                         ISNULL(p.barangay, '') LIKE '%' + @search + '%' OR
                         ISNULL(p.city_province, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR CAST(p.date_recorded AS DATE) >= @fromDate)
                        AND (@toDate   IS NULL OR CAST(p.date_recorded AS DATE) <= @toDate)
                    ORDER BY p.date_recorded DESC
                    OFFSET @startIndex ROWS FETCH NEXT @pageSize ROWS ONLY";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchText);
                da.SelectCommand.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@startIndex", startIndex);
                da.SelectCommand.Parameters.AddWithValue("@pageSize", PageSize);

                DataTable dt = new DataTable();
                da.Fill(dt);
                gvPatients.DataSource = dt;
                gvPatients.DataBind();

                // Update pagination controls
                UpdatePatientPagination(totalRecords);
            }
        }

        private void BindCases(string searchText = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                // Get total count for pagination
                string countQuery = @"
                    SELECT COUNT(*)
                    FROM dbo.[Case] c
                    WHERE
                        (@search = '' OR
                         CAST(c.case_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         CAST(c.patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         ISNULL(c.case_no, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.bite_barangay, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.bite_city, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.type_of_exposure, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.site_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.category, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR c.date_of_bite >= @fromDate)
                        AND (@toDate   IS NULL OR c.date_of_bite <= @toDate)";

                SqlCommand countCmd = new SqlCommand(countQuery, conn);
                countCmd.Parameters.AddWithValue("@search", searchText);
                countCmd.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                countCmd.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);
                conn.Open();
                int totalRecords = Convert.ToInt32(countCmd.ExecuteScalar());
                conn.Close();

                // Calculate page index
                int pageIndex = GetCasePageIndex();
                int startIndex = pageIndex * PageSize;

                string query = @"
                    SELECT
                        c.case_id,
                        c.patient_id,
                        c.case_no,
                        c.date_of_bite,
                        ISNULL(c.bite_barangay, '') + CASE WHEN c.bite_city IS NOT NULL AND c.bite_city != '' THEN ', ' + c.bite_city ELSE '' END AS place_of_bite,
                        c.type_of_exposure,
                        c.site_of_bite,
                        c.category
                    FROM dbo.[Case] c
                    WHERE
                        (@search = '' OR
                         CAST(c.case_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         CAST(c.patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         ISNULL(c.case_no, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.bite_barangay, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.bite_city, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.type_of_exposure, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.site_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.category, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR c.date_of_bite >= @fromDate)
                        AND (@toDate   IS NULL OR c.date_of_bite <= @toDate)
                    ORDER BY c.date_of_bite DESC, c.case_id DESC
                    OFFSET @startIndex ROWS FETCH NEXT @pageSize ROWS ONLY";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchText);
                da.SelectCommand.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@startIndex", startIndex);
                da.SelectCommand.Parameters.AddWithValue("@pageSize", PageSize);

                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCases.DataSource = dt;
                gvCases.DataBind();

                // Update pagination controls
                UpdateCasePagination(totalRecords);
            }
        }

        // ── Pagination helpers ──────────────────────────────────────────────

        private int GetPatientPageIndex()
        {
            int pageIndex;
            if (ViewState["PatientPageIndex"] == null)
            {
                pageIndex = 0;
            }
            else
            {
                pageIndex = Convert.ToInt32(ViewState["PatientPageIndex"]);
            }
            return pageIndex;
        }

        private int GetCasePageIndex()
        {
            int pageIndex;
            if (ViewState["CasePageIndex"] == null)
            {
                pageIndex = 0;
            }
            else
            {
                pageIndex = Convert.ToInt32(ViewState["CasePageIndex"]);
            }
            return pageIndex;
        }

        private void UpdatePatientPagination(int totalRecords)
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            int currentPage = GetPatientPageIndex();

            // Update page info label
            lblPatientPageInfo.Text = $"Page {currentPage + 1} of {Math.Max(1, totalPages)} (Total: {totalRecords} records)";

            // Enable/disable navigation buttons
            btnPatientPrev.Enabled = (currentPage > 0);
            btnPatientNext.Enabled = (currentPage < totalPages - 1 && totalPages > 0);

            // Store total pages for reference
            ViewState["PatientTotalPages"] = totalPages;
        }

        private void UpdateCasePagination(int totalRecords)
        {
            int totalPages = (int)Math.Ceiling((double)totalRecords / PageSize);
            int currentPage = GetCasePageIndex();

            // Update page info label
            lblCasePageInfo.Text = $"Page {currentPage + 1} of {Math.Max(1, totalPages)} (Total: {totalRecords} records)";

            // Enable/disable navigation buttons
            btnCasePrev.Enabled = (currentPage > 0);
            btnCaseNext.Enabled = (currentPage < totalPages - 1 && totalPages > 0);

            // Store total pages for reference
            ViewState["CaseTotalPages"] = totalPages;
        }

        // ── Patient Pagination Events ──────────────────────────────────────

        protected void btnPatientPrev_Click(object sender, EventArgs e)
        {
            int currentPage = GetPatientPageIndex();
            if (currentPage > 0)
            {
                ViewState["PatientPageIndex"] = currentPage - 1;
                BindPatients(txtSearchPatient.Text.Trim(),
                    ParseNullableDate(txtPatientDateFrom.Text),
                    ParseNullableDate(txtPatientDateTo.Text));
                HideRecordPreview();
            }
        }

        protected void btnPatientNext_Click(object sender, EventArgs e)
        {
            int currentPage = GetPatientPageIndex();
            int totalPages = ViewState["PatientTotalPages"] != null ? Convert.ToInt32(ViewState["PatientTotalPages"]) : 0;
            if (currentPage < totalPages - 1)
            {
                ViewState["PatientPageIndex"] = currentPage + 1;
                BindPatients(txtSearchPatient.Text.Trim(),
                    ParseNullableDate(txtPatientDateFrom.Text),
                    ParseNullableDate(txtPatientDateTo.Text));
                HideRecordPreview();
            }
        }

        // ── Case Pagination Events ────────────────────────────────────────

        protected void btnCasePrev_Click(object sender, EventArgs e)
        {
            int currentPage = GetCasePageIndex();
            if (currentPage > 0)
            {
                ViewState["CasePageIndex"] = currentPage - 1;
                BindCases(txtSearchCase.Text.Trim(),
                    ParseNullableDate(txtCaseDateFrom.Text),
                    ParseNullableDate(txtCaseDateTo.Text));
                HideRecordPreview();
            }
        }

        protected void btnCaseNext_Click(object sender, EventArgs e)
        {
            int currentPage = GetCasePageIndex();
            int totalPages = ViewState["CaseTotalPages"] != null ? Convert.ToInt32(ViewState["CaseTotalPages"]) : 0;
            if (currentPage < totalPages - 1)
            {
                ViewState["CasePageIndex"] = currentPage + 1;
                BindCases(txtSearchCase.Text.Trim(),
                    ParseNullableDate(txtCaseDateFrom.Text),
                    ParseNullableDate(txtCaseDateTo.Text));
                HideRecordPreview();
            }
        }

        // ── GvPatients_RowCommand ──────────────────────────────────────────

        protected void gvPatients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditPatient")
            {
                string role = Session["userRole"]?.ToString().ToUpper() ?? "";
                if (role == "C")
                {
                    ShowAlert("You do not have permission to edit patient records.", "error");
                    return;
                }

                string patientId = e.CommandArgument.ToString();
                hfSelectedPatientId.Value = patientId;
                hfSelectedCaseId.Value = "";
                hfEditMode.Value = "PATIENT";
                LoadPatientPreview(patientId);
                ShowRecordPreview();
                hfActivePanel.Value = "viewPatientPanel";
            }
        }

        protected void gvCases_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditCase")
            {
                string role = Session["userRole"]?.ToString().ToUpper() ?? "";
                if (role == "C")
                {
                    ShowAlert("You do not have permission to edit case records.", "error");
                    return;
                }

                int caseId = Convert.ToInt32(e.CommandArgument);
                hfSelectedCaseId.Value = caseId.ToString();
                hfSelectedPatientId.Value = "";
                hfEditMode.Value = "CASE";
                LoadCasePreview(caseId);
                ShowRecordPreview();
                hfActivePanel.Value = "viewPatientPanel";
            }
        }

        // ── Preview loaders ──────────────────────────────────────────────────

        private void LoadPatientPreview(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        p.patient_id,
                        p.fname,
                        p.lname,
                        p.date_of_birth,
                        p.gender,
                        p.civil_status,
                        p.contact_no,
                        p.occupation,
                        p.date_recorded,
                        vs.blood_pressure,
                        vs.temperature,
                        vs.wt,
                        p.house_no,
                        p.street,
                        p.barangay,
                        p.city_province,
                        p.emergency_contact_person,
                        p.emergency_contact_number
                    FROM dbo.Patient p
                    LEFT JOIN dbo.VitalSigns vs ON p.patient_id = vs.patient_id
                    WHERE p.patient_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", patientId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    pnlPatientPreview.Visible = true;
                    pnlCasePreview.Visible = false;

                    txtPreviewPatientId.Text = dr["patient_id"].ToString();
                    txtPreviewFirstName.Text = dr["fname"].ToString();
                    txtPreviewLastName.Text = dr["lname"].ToString();
                    txtPreviewDOB.Text = dr["date_of_birth"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_of_birth"]).ToString("yyyy-MM-dd");
                    ddlPreviewGender.SelectedValue = SafeDropdownValue(ddlPreviewGender, dr["gender"].ToString());
                    ddlPreviewCivilStatus.SelectedValue = SafeDropdownValue(ddlPreviewCivilStatus, dr["civil_status"].ToString());
                    txtPreviewContactNo.Text = dr["contact_no"].ToString();
                    ddlPreviewOccupation.SelectedValue = SafeDropdownValue(ddlPreviewOccupation, dr["occupation"].ToString());

                    txtPreviewHouseNo.Text = dr["house_no"] == DBNull.Value ? "" : dr["house_no"].ToString();
                    txtPreviewStreet.Text = dr["street"] == DBNull.Value ? "" : dr["street"].ToString();
                    ddlPreviewBarangay.SelectedValue = SafeDropdownValue(ddlPreviewBarangay, dr["barangay"].ToString());
                    txtPreviewCityProvince.Text = dr["city_province"] == DBNull.Value ? "" : dr["city_province"].ToString();

                    txtPreviewEmergencyPerson.Text = dr["emergency_contact_person"] == DBNull.Value ? "" : dr["emergency_contact_person"].ToString();
                    txtPreviewEmergencyNo.Text = dr["emergency_contact_number"] == DBNull.Value ? "" : dr["emergency_contact_number"].ToString();

                    txtPreviewBP.Text = dr["blood_pressure"] == DBNull.Value ? "" : dr["blood_pressure"].ToString();
                    txtPreviewTemp.Text = dr["temperature"] == DBNull.Value ? "" : dr["temperature"].ToString();
                    txtPreviewWeight.Text = dr["wt"] == DBNull.Value ? "" : dr["wt"].ToString();
                    txtPreviewCapillaryRefill.Text = "";
                    txtPreviewDateAdded.Text = dr["date_recorded"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_recorded"]).ToString("MMM dd, yyyy");

                    string role = Session["userRole"]?.ToString().ToUpper() ?? "";
                    btnPreviewUpdatePatient.Visible = (role != "C");
                }
            }
        }

        private void LoadCasePreview(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT 
                        c.case_id,
                        c.patient_id,
                        c.case_no,
                        c.date_of_bite,
                        c.time_of_bite,
                        c.type_of_exposure,
                        c.wound_type,
                        c.bleeding,
                        c.site_of_bite,
                        c.category,
                        c.washed,
                        c.bite_house_no,
                        c.bite_street,
                        c.bite_barangay,
                        c.bite_city
                    FROM dbo.[Case] c
                    WHERE c.case_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", caseId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    pnlPatientPreview.Visible = false;
                    pnlCasePreview.Visible = true;

                    txtPreviewCaseId.Text = dr["case_id"].ToString();
                    txtPreviewCasePatientId.Text = dr["patient_id"].ToString();
                    txtPreviewCaseNo.Text = dr["case_no"] == DBNull.Value ? "" : dr["case_no"].ToString();
                    txtPreviewCaseDateOfBite.Text = dr["date_of_bite"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_of_bite"]).ToString("yyyy-MM-dd");

                    if (dr["time_of_bite"] == DBNull.Value)
                    {
                        txtPreviewCaseTimeOfBite.Text = "";
                    }
                    else
                    {
                        TimeSpan biteTime;
                        txtPreviewCaseTimeOfBite.Text = TimeSpan.TryParse(dr["time_of_bite"].ToString(), out biteTime)
                            ? biteTime.ToString(@"hh\:mm") : "";
                    }

                    txtPreviewCasePlaceHouseNo.Text = dr["bite_house_no"] == DBNull.Value ? "" : dr["bite_house_no"].ToString();
                    txtPreviewCasePlaceStreet.Text = dr["bite_street"] == DBNull.Value ? "" : dr["bite_street"].ToString();
                    ddlPreviewCasePlaceBarangay.SelectedValue = SafeDropdownValue(ddlPreviewCasePlaceBarangay, dr["bite_barangay"].ToString());
                    txtPreviewCasePlaceCity.Text = dr["bite_city"] == DBNull.Value ? "" : dr["bite_city"].ToString();

                    ddlPreviewCaseExposureType.SelectedValue = SafeDropdownValue(ddlPreviewCaseExposureType, dr["type_of_exposure"].ToString());
                    ddlPreviewCaseWoundType.SelectedValue = SafeDropdownValue(ddlPreviewCaseWoundType, dr["wound_type"].ToString());
                    ddlPreviewCaseBleeding.SelectedValue = SafeDropdownValue(ddlPreviewCaseBleeding, dr["bleeding"].ToString());
                    txtPreviewCaseSiteOfBite.Text = dr["site_of_bite"] == DBNull.Value ? "" : dr["site_of_bite"].ToString();
                    ddlPreviewCaseCategory.SelectedValue = SafeDropdownValue(ddlPreviewCaseCategory, dr["category"].ToString());
                    ddlPreviewCaseWashed.SelectedValue = SafeDropdownValue(ddlPreviewCaseWashed, dr["washed"].ToString());

                    string role = Session["userRole"]?.ToString().ToUpper() ?? "";
                    btnPreviewUpdateCase.Visible = (role != "C");
                }
            }
        }

        // ── Update handlers ──────────────────────────────────────────────────

        protected void btnPreviewUpdatePatient_Click(object sender, EventArgs e)
        {
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C") { ShowAlert("You do not have permission to update patient records.", "error"); return; }
            if (string.IsNullOrWhiteSpace(txtPreviewPatientId.Text)) { ShowAlert("No patient selected."); return; }

            DateTime dob;
            if (!DateTime.TryParse(txtPreviewDOB.Text, out dob)) { ShowAlert("Invalid Date of Birth."); return; }
            if (dob > DateTime.Today) { ShowAlert("Date of Birth cannot be a future date."); return; }

            string pid = txtPreviewPatientId.Text.Trim();
            string fn = txtPreviewFirstName.Text.Trim();
            string ln = txtPreviewLastName.Text.Trim();

            if (IsDuplicatePatient(fn, ln, dob, excludePatientId: pid))
            {
                ShowAlert("Another patient with the same name and date of birth already exists.", "error");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    new SqlCommand(@"
                        UPDATE dbo.Patient
                        SET fname=@fn, lname=@ln, date_of_birth=@dob, gender=@g,
                            civil_status=@cs, contact_no=@cn, occupation=@oc,
                            house_no=@h, street=@s, barangay=@b, city_province=@c,
                            emergency_contact_person=@ep, emergency_contact_number=@en
                        WHERE patient_id=@pid", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@fn",  fn),
                            new SqlParameter("@ln",  ln),
                            new SqlParameter("@dob", dob),
                            new SqlParameter("@g",   ddlPreviewGender.SelectedValue),
                            new SqlParameter("@cs",  ddlPreviewCivilStatus.SelectedValue),
                            new SqlParameter("@cn",  txtPreviewContactNo.Text.Trim()),
                            new SqlParameter("@oc",  ddlPreviewOccupation.SelectedValue),
                            new SqlParameter("@h",   txtPreviewHouseNo.Text.Trim()),
                            new SqlParameter("@s",   txtPreviewStreet.Text.Trim()),
                            new SqlParameter("@b",   ddlPreviewBarangay.SelectedValue),
                            new SqlParameter("@c",   txtPreviewCityProvince.Text.Trim()),
                            new SqlParameter("@ep",  txtPreviewEmergencyPerson.Text.Trim()),
                            new SqlParameter("@en",  txtPreviewEmergencyNo.Text.Trim()),
                            new SqlParameter("@pid", pid)
                        }
                    }.ExecuteNonQuery();

                    UpsertVitals(conn, trans, pid,
                        txtPreviewBP.Text.Trim(),
                        txtPreviewTemp.Text.Trim(),
                        txtPreviewWeight.Text.Trim());

                    trans.Commit();
                    FBindGrid();
                    LoadPatientPreview(pid);
                    ShowAlert("Patient information updated successfully.", "success");
                }
                catch (SqlException sqlEx) when (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    trans.Rollback();
                    ShowAlert("Another patient with the same name and date of birth already exists.", "error");
                }
                catch (Exception ex) { trans.Rollback(); ShowAlert(ex.Message.Replace("'", ""), "error"); }
            }
        }

        protected void btnPreviewCancelPatient_Click(object sender, EventArgs e) { HideRecordPreview(); }

        protected void btnPreviewUpdateCase_Click(object sender, EventArgs e)
        {
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C") { ShowAlert("You do not have permission to update case records.", "error"); return; }
            if (string.IsNullOrWhiteSpace(txtPreviewCaseId.Text)) { ShowAlert("No case selected."); return; }

            DateTime biteDate;
            if (!DateTime.TryParse(txtPreviewCaseDateOfBite.Text, out biteDate)) { ShowAlert("Invalid Date of Bite.", "warning"); return; }
            if (biteDate.Date > DateTime.Today) { ShowAlert("Date of Bite cannot be a future date.", "warning"); return; }

            object timeValue = DBNull.Value;
            if (!string.IsNullOrWhiteSpace(txtPreviewCaseTimeOfBite.Text))
            {
                TimeSpan biteTime;
                if (!TimeSpan.TryParse(txtPreviewCaseTimeOfBite.Text, out biteTime)) { ShowAlert("Invalid Time of Bite.", "warning"); return; }
                if (biteDate.Date == DateTime.Today && biteTime > DateTime.Now.TimeOfDay)
                { ShowAlert("Time of Bite cannot be a future time.", "warning"); return; }
                timeValue = biteTime;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    int caseId = Convert.ToInt32(txtPreviewCaseId.Text.Trim());

                    new SqlCommand(@"
                        UPDATE dbo.[Case]
                        SET date_of_bite=@d, time_of_bite=@t, type_of_exposure=@et,
                            wound_type=@wt, bleeding=@bl, site_of_bite=@sb,
                            category=@cat, washed=@w,
                            bite_house_no=@bh, bite_street=@bs, bite_barangay=@bb, bite_city=@bc
                        WHERE case_id=@cid", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@d",   biteDate),
                            new SqlParameter("@t",   timeValue),
                            new SqlParameter("@et",  ddlPreviewCaseExposureType.SelectedValue),
                            new SqlParameter("@wt",  ddlPreviewCaseWoundType.SelectedValue),
                            new SqlParameter("@bl",  ddlPreviewCaseBleeding.SelectedValue),
                            new SqlParameter("@sb",  txtPreviewCaseSiteOfBite.Text.Trim()),
                            new SqlParameter("@cat", ddlPreviewCaseCategory.SelectedValue),
                            new SqlParameter("@w",   ddlPreviewCaseWashed.SelectedValue),
                            new SqlParameter("@bh",  txtPreviewCasePlaceHouseNo.Text.Trim()),
                            new SqlParameter("@bs",  txtPreviewCasePlaceStreet.Text.Trim()),
                            new SqlParameter("@bb",  ddlPreviewCasePlaceBarangay.SelectedValue),
                            new SqlParameter("@bc",  txtPreviewCasePlaceCity.Text.Trim()),
                            new SqlParameter("@cid", caseId)
                        }
                    }.ExecuteNonQuery();

                    trans.Commit();
                    FBindGrid();
                    LoadCasePreview(caseId);
                    ShowAlert("Case information updated successfully.", "success");
                }
                catch (Exception ex) { trans.Rollback(); ShowAlert(ex.Message.Replace("'", ""), "error"); }
            }
        }

        protected void btnPreviewCancelCase_Click(object sender, EventArgs e) { HideRecordPreview(); }

        protected void btnSearchPatient_Click(object sender, EventArgs e)
        {
            ViewState["PatientPageIndex"] = 0; // Reset to first page
            BindPatients(txtSearchPatient.Text.Trim(), ParseNullableDate(txtPatientDateFrom.Text), ParseNullableDate(txtPatientDateTo.Text));
            HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnResetPatientSearch_Click(object sender, EventArgs e)
        {
            txtSearchPatient.Text = txtPatientDateFrom.Text = txtPatientDateTo.Text = "";
            ViewState["PatientPageIndex"] = 0; // Reset to first page
            BindPatients();
            HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnSearchCase_Click(object sender, EventArgs e)
        {
            ViewState["CasePageIndex"] = 0; // Reset to first page
            BindCases(txtSearchCase.Text.Trim(), ParseNullableDate(txtCaseDateFrom.Text), ParseNullableDate(txtCaseDateTo.Text));
            HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnResetCaseSearch_Click(object sender, EventArgs e)
        {
            txtSearchCase.Text = txtCaseDateFrom.Text = txtCaseDateTo.Text = "";
            ViewState["CasePageIndex"] = 0; // Reset to first page
            BindCases();
            HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        // ── New patient + case registration ────────────────────────────────

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C") { ShowAlert("You do not have permission to register patients.", "error"); return; }

            // Get and clean the input values
            string fn = txtFirstName.Text.Trim();
            string ln = txtLastName.Text.Trim();

            // Remove extra spaces between words
            fn = System.Text.RegularExpressions.Regex.Replace(fn, @"\s+", " ");
            ln = System.Text.RegularExpressions.Regex.Replace(ln, @"\s+", " ");

            // Validate date of birth
            DateTime dob;
            if (!DateTime.TryParse(txtDOB.Text, out dob))
            {
                ShowAlert("Invalid Date of Birth", "warning");
                return;
            }

            if (dob > DateTime.Today)
            {
                ShowAlert("Date of Birth cannot be a future date.", "warning");
                return;
            }

            // Validate bite date and time
            if (!DateTime.TryParse(txtBiteDateTime.Text, out DateTime biteDateTime))
            {
                ShowAlert("Invalid Bite Date and Time", "warning");
                return;
            }

            TimeZoneInfo phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            DateTime bitePH = DateTime.SpecifyKind(biteDateTime, DateTimeKind.Unspecified);
            DateTime nowPH = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, phTimeZone);

            if (bitePH > nowPH.AddMinutes(1))
            {
                ShowAlert("Date and Time of Bite cannot be in the future.", "warning");
                return;
            }

            // DEBUG: Show what's being checked
            string debugMsg = $"Checking duplicate for: '{fn}' '{ln}' DOB: {dob:yyyy-MM-dd}";
            System.Diagnostics.Debug.WriteLine(debugMsg);

            // Check for duplicate patient
            bool isDuplicate = IsDuplicatePatient(fn, ln, dob);

            if (isDuplicate)
            {
                // Get detailed info about the duplicate
                string duplicateInfo = GetDuplicatePatientInfo(fn, ln, dob);
                ShowAlert($"A patient with the same name and date of birth is already registered.\n\n{duplicateInfo}\n\nPlease search for the existing record instead of creating a new one.", "error");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    new SqlCommand(@"
                        INSERT INTO dbo.Patient
                            (fname,lname,date_of_birth,gender,civil_status,contact_no,occupation,
                             house_no,street,barangay,city_province,
                             emergency_contact_person,emergency_contact_number,date_recorded)
                        VALUES
                            (@fn,@ln,@dob,@g,@cs,@cn,@oc,
                             @h,@s,@b,@c,
                             @ep,@en,GETDATE())", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@fn",  fn),
                            new SqlParameter("@ln",  ln),
                            new SqlParameter("@dob", dob),
                            new SqlParameter("@g",   ddlGender.SelectedValue),
                            new SqlParameter("@cs",  ddlCivilStatus.SelectedValue),
                            new SqlParameter("@cn",  txtContactNo.Text.Trim()),
                            new SqlParameter("@oc",  ddlOccupation.SelectedValue),
                            new SqlParameter("@h",   txtHouseNo.Text.Trim()),
                            new SqlParameter("@s",   txtSubdivision.Text.Trim()),
                            new SqlParameter("@b",   ddlBarangay.SelectedValue),
                            new SqlParameter("@c",   txtProvinceCity.Text.Trim()),
                            new SqlParameter("@ep",  txtEmergencyContactPerson.Text.Trim()),
                            new SqlParameter("@en",  txtEmergencyContactNo.Text.Trim())
                        }
                    }.ExecuteNonQuery();

                    string newPatientId = new SqlCommand(
                        "SELECT TOP 1 patient_id FROM dbo.Patient ORDER BY date_recorded DESC",
                        conn, trans).ExecuteScalar().ToString();

                    if (!string.IsNullOrWhiteSpace(txtBloodPressure.Text) ||
                        !string.IsNullOrWhiteSpace(txtTemperature.Text) ||
                        !string.IsNullOrWhiteSpace(txtWeight.Text))
                    {
                        UpsertVitals(conn, trans, newPatientId,
                            txtBloodPressure.Text.Trim(),
                            txtTemperature.Text.Trim(),
                            txtWeight.Text.Trim());
                    }

                    // Generate case number using the sequence
                    string newCaseNo = GenerateCaseNumber(conn, trans);

                    string symptom = GetManifestation();
                    string initialDiagnosis = BuildInitialDiagnosis(GetSelectedCategory(), GetBitingAnimal());

                    SqlCommand cmdCase = new SqlCommand(@"
                        INSERT INTO dbo.[Case]
                            (patient_id,case_no,date_of_bite,time_of_bite,type_of_exposure,wound_type,
                             bleeding,site_of_bite,category,washed,
                             bite_house_no,bite_street,bite_barangay,bite_city,
                             animal_type,ownership,animal_status,circumstances)
                        VALUES
                            (@pid,@cno,@date,@time,@et,@wt,
                             @bl,@sb,@cat,@w,
                             @bh,@bs,@bb,@bc,
                             @at,@ow,@as,@ci);
                        SELECT SCOPE_IDENTITY();", conn, trans);
                    cmdCase.Parameters.AddWithValue("@pid", newPatientId);
                    cmdCase.Parameters.AddWithValue("@cno", newCaseNo);
                    cmdCase.Parameters.AddWithValue("@date", biteDateTime.Date);
                    cmdCase.Parameters.AddWithValue("@time", biteDateTime.TimeOfDay);
                    cmdCase.Parameters.AddWithValue("@et", ddlExposureType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@wt", ddlWoundType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@bl", ddlBleeding.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@sb", txtWoundLocation.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@cat", GetSelectedCategory());
                    cmdCase.Parameters.AddWithValue("@w", GetWashed());
                    cmdCase.Parameters.AddWithValue("@bh", txtPlaceHouseNo.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@bs", txtPlaceStreet.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@bb", ddlPlaceBarangay.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@bc", txtPlaceCity.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@at", GetBitingAnimal());
                    cmdCase.Parameters.AddWithValue("@ow", string.IsNullOrEmpty(GetOwnership()) ? (object)DBNull.Value : GetOwnership());
                    cmdCase.Parameters.AddWithValue("@as", string.IsNullOrEmpty(GetAnimalStatus()) ? (object)DBNull.Value : GetAnimalStatus());
                    cmdCase.Parameters.AddWithValue("@ci", string.IsNullOrEmpty(GetCircumstance()) ? (object)DBNull.Value : GetCircumstance());
                    int newCaseId = Convert.ToInt32(cmdCase.ExecuteScalar());

                    new SqlCommand(@"
                        INSERT INTO dbo.Visit
                            (case_id, visit_type, visit_date, diagnosis, symptoms_present, manifestation_notes, status)
                        VALUES
                            (@cid, 'Initial Visit', @vdate, @diag, @symp, @notes, 'Completed')",
                        conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@cid",   newCaseId),
                            new SqlParameter("@vdate", biteDateTime.Date),
                            new SqlParameter("@diag",  (object)initialDiagnosis ?? DBNull.Value),
                            new SqlParameter("@symp",  string.IsNullOrWhiteSpace(symptom) || symptom == "None"
                                                            ? (object)DBNull.Value
                                                            : (object)symptom),
                            new SqlParameter("@notes", string.IsNullOrWhiteSpace(symptom) || symptom == "None"
                                                            ? (object)DBNull.Value
                                                            : (object)symptom)
                        }
                    }.ExecuteNonQuery();

                    trans.Commit();
                    FBindGrid();
                    ClearFormFields();
                    HideRecordPreview();
                    hfActivePanel.Value = "viewPatientPanel";
                    ShowAlert("Patient Registered Successfully", "success");
                }
                catch (SqlException sqlEx) when (sqlEx.Number == 2601 || sqlEx.Number == 2627)
                {
                    trans.Rollback();
                    ShowAlert("A patient with the same name and date of birth is already registered.", "error");
                }
                catch (Exception ex) { trans.Rollback(); ShowAlert(ex.Message.Replace("'", ""), "error"); }
            }
        }

        private string BuildInitialDiagnosis(string category, string animalType)
        {
            string cat = string.IsNullOrWhiteSpace(category) ? "" : category.Trim().ToUpper();
            string animal = string.IsNullOrWhiteSpace(animalType) ? "animal" : animalType.Trim();

            switch (cat)
            {
                case "I": return $"WHO Category I exposure — {animal} contact, no skin break. No PEP indicated.";
                case "II": return $"WHO Category II exposure — {animal} bite/scratch with minor skin break. PEP initiated.";
                case "III": return $"WHO Category III exposure — {animal} bite/scratch penetrating skin or mucous membrane contact. Urgent PEP initiated.";
                default: return $"Animal bite exposure — {animal}. Category pending assessment.";
            }
        }

        // ── Generate Case Number Helper ─────────────────────────────────────

        /// <summary>
        /// Generates a unique case number using the SeqCase sequence.
        /// Format: C{YYYY}-{0000}
        /// Example: C2026-0001, C2026-0002, etc.
        /// </summary>
        private string GenerateCaseNumber(SqlConnection conn, SqlTransaction trans)
        {
            string query = @"
                SELECT 'C' + CAST(YEAR(GETDATE()) AS VARCHAR(4)) + '-' + 
                       RIGHT('0000' + CAST(NEXT VALUE FOR dbo.SeqCase AS VARCHAR(10)), 4)";

            object result = new SqlCommand(query, conn, trans).ExecuteScalar();
            return result != null ? result.ToString() : "";
        }

        // ── Check if case already exists for patient ────────────────────────

        /// <summary>
        /// Checks if a patient already has a case with the same date of bite.
        /// </summary>
        private bool CaseExistsForPatient(string patientId, DateTime biteDate)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT COUNT(*) 
                    FROM dbo.[Case] 
                    WHERE patient_id = @pid 
                      AND CAST(date_of_bite AS DATE) = CAST(@biteDate AS DATE)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pid", patientId);
                cmd.Parameters.AddWithValue("@biteDate", biteDate.Date);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        /// <summary>
        /// Checks if a specific case number already exists in the database.
        /// </summary>
        private bool CaseNumberExists(string caseNo)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT COUNT(*) FROM dbo.[Case] WHERE case_no = @caseNo";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@caseNo", caseNo);
                conn.Open();
                return Convert.ToInt32(cmd.ExecuteScalar()) > 0;
            }
        }

        // ── Case-only registration (for existing patients) ──────────────

        protected void btnSaveCase_Click(object sender, EventArgs e)
        {
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C") { ShowAlert("You do not have permission to register cases.", "error"); return; }

            string patientId = hfCasePatientId.Value;
            if (string.IsNullOrWhiteSpace(patientId))
            {
                ShowAlert("Please select a patient first.", "warning");
                return;
            }

            // Validate required fields
            DateTime biteDateTime;
            if (!DateTime.TryParse(txtCaseBiteDateTime.Text, out biteDateTime))
            {
                ShowAlert("Invalid Bite Date and Time", "warning");
                return;
            }

            TimeZoneInfo phTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
            DateTime bitePH = DateTime.SpecifyKind(biteDateTime, DateTimeKind.Unspecified);
            DateTime nowPH = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, phTimeZone);

            if (bitePH > nowPH.AddMinutes(1))
            {
                ShowAlert("Date and Time of Bite cannot be in the future.", "warning");
                return;
            }

            if (string.IsNullOrEmpty(ddlCasePlaceBarangay.SelectedValue))
            {
                ShowAlert("Barangay (place of bite) is required.", "warning");
                return;
            }
            if (string.IsNullOrEmpty(txtCasePlaceCity.Text.Trim()))
            {
                ShowAlert("City / Province (place of bite) is required.", "warning");
                return;
            }
            if (string.IsNullOrEmpty(ddlCaseCategory.SelectedValue))
            {
                ShowAlert("Category is required.", "warning");
                return;
            }
            if (string.IsNullOrEmpty(ddlCaseBitingAnimal.SelectedValue))
            {
                ShowAlert("Animal Type is required.", "warning");
                return;
            }

            // ── DUPLICATE CHECK: Check if patient already has a case on this date ──
            if (CaseExistsForPatient(patientId, biteDateTime))
            {
                ShowAlert($"This patient already has a case recorded on {biteDateTime.ToString("MMM dd, yyyy")}. " +
                          "Please check the existing case or select a different date.", "warning");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    // Generate case number
                    string newCaseNo = GenerateCaseNumber(conn, trans);

                    // ── DUPLICATE CHECK: Verify the generated case number doesn't already exist ──
                    if (CaseNumberExists(newCaseNo))
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            newCaseNo = GenerateCaseNumber(conn, trans);
                            if (!CaseNumberExists(newCaseNo))
                                break;
                        }

                        if (CaseNumberExists(newCaseNo))
                        {
                            trans.Rollback();
                            ShowAlert("Unable to generate a unique case number. Please try again.", "error");
                            return;
                        }
                    }

                    string symptom = GetCaseManifestation();
                    string initialDiagnosis = BuildInitialDiagnosis(GetCaseCategory(), GetCaseBitingAnimal());

                    SqlCommand cmdCase = new SqlCommand(@"
                        INSERT INTO dbo.[Case]
                            (patient_id, case_no, date_of_bite, time_of_bite, type_of_exposure, wound_type,
                             bleeding, site_of_bite, category, washed,
                             bite_house_no, bite_street, bite_barangay, bite_city,
                             animal_type, ownership, animal_status, circumstances)
                        VALUES
                            (@pid, @cno, @date, @time, @et, @wt,
                             @bl, @sb, @cat, @w,
                             @bh, @bs, @bb, @bc,
                             @at, @ow, @as, @ci);
                        SELECT SCOPE_IDENTITY();", conn, trans);

                    cmdCase.Parameters.AddWithValue("@pid", patientId);
                    cmdCase.Parameters.AddWithValue("@cno", newCaseNo);
                    cmdCase.Parameters.AddWithValue("@date", biteDateTime.Date);
                    cmdCase.Parameters.AddWithValue("@time", biteDateTime.TimeOfDay);
                    cmdCase.Parameters.AddWithValue("@et", ddlCaseExposureType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@wt", ddlCaseWoundType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@bl", ddlCaseBleeding.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@sb", txtCaseWoundLocation.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@cat", GetCaseCategory());
                    cmdCase.Parameters.AddWithValue("@w", GetCaseWashed());
                    cmdCase.Parameters.AddWithValue("@bh", txtCasePlaceHouseNo.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@bs", txtCasePlaceStreet.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@bb", ddlCasePlaceBarangay.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@bc", txtCasePlaceCity.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@at", GetCaseBitingAnimal());
                    cmdCase.Parameters.AddWithValue("@ow", string.IsNullOrEmpty(GetCaseOwnership()) ? (object)DBNull.Value : GetCaseOwnership());
                    cmdCase.Parameters.AddWithValue("@as", string.IsNullOrEmpty(GetCaseAnimalStatus()) ? (object)DBNull.Value : GetCaseAnimalStatus());
                    cmdCase.Parameters.AddWithValue("@ci", string.IsNullOrEmpty(GetCaseCircumstance()) ? (object)DBNull.Value : GetCaseCircumstance());

                    int newCaseId = Convert.ToInt32(cmdCase.ExecuteScalar());

                    new SqlCommand(@"
                        INSERT INTO dbo.Visit
                            (case_id, visit_type, visit_date, diagnosis, symptoms_present, manifestation_notes, status)
                        VALUES
                            (@cid, 'Initial Visit', @vdate, @diag, @symp, @notes, 'Completed')",
                        conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@cid", newCaseId),
                            new SqlParameter("@vdate", biteDateTime.Date),
                            new SqlParameter("@diag", (object)initialDiagnosis ?? DBNull.Value),
                            new SqlParameter("@symp", string.IsNullOrWhiteSpace(symptom) || symptom == "None"
                                ? (object)DBNull.Value : (object)symptom),
                            new SqlParameter("@notes", string.IsNullOrWhiteSpace(symptom) || symptom == "None"
                                ? (object)DBNull.Value : (object)symptom)
                        }
                    }.ExecuteNonQuery();

                    trans.Commit();
                    FBindGrid();
                    ClearCaseFormFields();
                    HideRecordPreview();
                    hfActivePanel.Value = "viewPatientPanel";
                    ShowAlert($"New case {newCaseNo} registered successfully for patient: {patientId}", "success");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ShowAlert(ex.Message.Replace("'", ""), "error");
                }
            }
        }

        // ── Case patient search handlers ─────────────────────────────────

        protected void btnCasePatientSearch_Click(object sender, EventArgs e)
        {
            string searchTerm = txtCasePatientSearch.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                pnlCasePatientResults.Visible = false;
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 20
                        patient_id,
                        fname,
                        lname,
                        gender,
                        contact_no,
                        ISNULL(barangay, '') + CASE WHEN city_province IS NOT NULL AND city_province != '' THEN ', ' + city_province ELSE '' END AS address
                    FROM dbo.Patient
                    WHERE
                        CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                        fname LIKE '%' + @search + '%' OR
                        lname LIKE '%' + @search + '%' OR
                        (fname + ' ' + lname) LIKE '%' + @search + '%' OR
                        contact_no LIKE '%' + @search + '%'
                    ORDER BY date_recorded DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchTerm);
                DataTable dt = new DataTable();
                da.Fill(dt);

                gvCasePatientSearch.DataSource = dt;
                gvCasePatientSearch.DataBind();
                pnlCasePatientResults.Visible = true;
                pnlCaseSelectedPatient.Visible = false;
            }
        }

        protected void btnCasePatientClear_Click(object sender, EventArgs e)
        {
            txtCasePatientSearch.Text = "";
            pnlCasePatientResults.Visible = false;
            pnlCaseSelectedPatient.Visible = false;
            hfCasePatientId.Value = "";
            ClientScript.RegisterStartupScript(this.GetType(), "DisableNext",
                "enableCaseNextStep(false);", true);
        }

        protected void gvCasePatientSearch_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SelectPatient")
            {
                string patientId = e.CommandArgument.ToString();
                SelectPatientForCase(patientId);
            }
        }

        private void SelectPatientForCase(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT TOP 1
                        patient_id,
                        fname,
                        lname,
                        date_of_birth,
                        gender,
                        contact_no,
                        ISNULL(barangay, '') + CASE WHEN city_province IS NOT NULL AND city_province != '' THEN ', ' + city_province ELSE '' END AS address
                    FROM dbo.Patient
                    WHERE patient_id = @pid";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pid", patientId);
                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    caseSelectedPatientId.InnerText = dr["patient_id"].ToString();
                    caseSelectedPatientName.InnerText = dr["fname"].ToString() + " " + dr["lname"].ToString();
                    caseSelectedPatientDOB.InnerText = dr["date_of_birth"] == DBNull.Value ? "" : Convert.ToDateTime(dr["date_of_birth"]).ToString("MMM dd, yyyy");
                    caseSelectedPatientGender.InnerText = dr["gender"].ToString();
                    caseSelectedPatientContact.InnerText = dr["contact_no"].ToString();
                    caseSelectedPatientAddress.InnerText = dr["address"].ToString();

                    caseReviewPatientName.InnerText = dr["fname"].ToString() + " " + dr["lname"].ToString();
                    caseReviewPatientId.InnerText = dr["patient_id"].ToString();

                    hfCasePatientId.Value = patientId;
                    pnlCasePatientResults.Visible = false;
                    pnlCaseSelectedPatient.Visible = true;

                    ClientScript.RegisterStartupScript(this.GetType(), "EnableNext",
                        "enableCaseNextStep(true);", true);
                }
            }
        }

        protected void btnCaseChangePatient_Click(object sender, EventArgs e)
        {
            pnlCaseSelectedPatient.Visible = false;
            hfCasePatientId.Value = "";
            txtCasePatientSearch.Text = "";
            pnlCasePatientResults.Visible = false;
            ClientScript.RegisterStartupScript(this.GetType(), "DisableNext",
                "enableCaseNextStep(false);", true);
        }

        protected void btnUpdateRecord_Click(object sender, EventArgs e) { ShowAlert("Use the Update button inside the Record Preview."); }
        protected void btnCancelEditForm_Click(object sender, EventArgs e) { ClearFormFields(); hfEditMode.Value = ""; hfActivePanel.Value = "viewPatientPanel"; }
        protected void btnClear_Click(object sender, EventArgs e) { ClearFormFields(); }

        private void ClearFormFields()
        {
            txtFirstName.Text = txtLastName.Text = txtDOB.Text = txtContactNo.Text = "";
            ddlGender.SelectedIndex = ddlCivilStatus.SelectedIndex = ddlOccupation.SelectedIndex = 0;

            txtHouseNo.Text = txtSubdivision.Text = "";
            ddlBarangay.SelectedIndex = 0;
            ddlPlaceBarangay.SelectedIndex = 0;
            txtProvinceCity.Text = "";
            txtEmergencyContactPerson.Text = txtEmergencyContactNo.Text = "";
            txtBloodPressure.Text = txtTemperature.Text = txtWeight.Text = txtCapillaryRefill.Text = "";

            txtBiteDateTime.Text = txtWoundLocation.Text = "";
            ddlExposureType.SelectedIndex = ddlWoundType.SelectedIndex = ddlBleeding.SelectedIndex = 0;

            txtPlaceHouseNo.Text = txtPlaceStreet.Text = "";
            txtPlaceCity.Text = "";

            ddlBitingAnimal.SelectedIndex = ddlOwnership.SelectedIndex = ddlCircumstance.SelectedIndex = 0;
            ddlAnimalStatus.SelectedIndex = ddlWoundWashed.SelectedIndex = ddlCategory.SelectedIndex = 0;
            ddlManifestation.SelectedIndex = 0;
        }

        private void ClearCaseFormFields()
        {
            txtCaseBiteDateTime.Text = "";
            txtCaseWoundLocation.Text = "";
            txtCasePlaceHouseNo.Text = "";
            txtCasePlaceStreet.Text = "";
            ddlCasePlaceBarangay.SelectedIndex = 0;
            txtCasePlaceCity.Text = "";
            ddlCaseExposureType.SelectedIndex = 0;
            ddlCaseWoundType.SelectedIndex = 0;
            ddlCaseBleeding.SelectedIndex = 0;
            ddlCaseBitingAnimal.SelectedIndex = 0;
            ddlCaseOwnership.SelectedIndex = 0;
            ddlCaseCircumstance.SelectedIndex = 0;
            ddlCaseAnimalStatus.SelectedIndex = 0;
            ddlCaseWoundWashed.SelectedIndex = 0;
            ddlCaseCategory.SelectedIndex = 0;
            ddlCaseManifestation.SelectedIndex = 0;
            hfCasePatientId.Value = "";
            pnlCaseSelectedPatient.Visible = false;
            pnlCasePatientResults.Visible = false;
            txtCasePatientSearch.Text = "";
        }

        // ── FIXED Duplicate-patient check ─────────────────────────────────────────

        /// <summary>
        /// Checks if a patient with the same name and date of birth already exists.
        /// This version uses a more reliable comparison method.
        /// </summary>
        private bool IsDuplicatePatient(string fname, string lname, DateTime dob, string excludePatientId = null)
        {
            try
            {
                // Normalize the input
                fname = NormalizeName(fname);
                lname = NormalizeName(lname);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT COUNT(*)
                        FROM dbo.Patient
                        WHERE LTRIM(RTRIM(fname)) = @fn
                          AND LTRIM(RTRIM(lname)) = @ln
                          AND date_of_birth = @dob";

                    // Add exclude condition if provided
                    if (!string.IsNullOrEmpty(excludePatientId))
                    {
                        query += " AND patient_id != @excludeId";
                    }

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fn", fname);
                    cmd.Parameters.AddWithValue("@ln", lname);
                    cmd.Parameters.AddWithValue("@dob", dob.Date);

                    if (!string.IsNullOrEmpty(excludePatientId))
                    {
                        cmd.Parameters.AddWithValue("@excludeId", excludePatientId);
                    }

                    conn.Open();
                    int count = Convert.ToInt32(cmd.ExecuteScalar());

                    // Log for debugging
                    System.Diagnostics.Debug.WriteLine($"Duplicate check: Name='{fname} {lname}', DOB={dob:yyyy-MM-dd}, Count={count}");

                    return count > 0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in IsDuplicatePatient: {ex.Message}");
                return false; // Don't block on error
            }
        }

        /// <summary>
        /// Gets information about duplicate patients for display in error messages.
        /// </summary>
        private string GetDuplicatePatientInfo(string fname, string lname, DateTime dob)
        {
            try
            {
                fname = NormalizeName(fname);
                lname = NormalizeName(lname);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = @"
                        SELECT TOP 3
                            patient_id,
                            fname,
                            lname,
                            date_of_birth,
                            contact_no,
                            ISNULL(barangay, '') + CASE WHEN city_province IS NOT NULL AND city_province != '' THEN ', ' + city_province ELSE '' END AS address
                        FROM dbo.Patient
                        WHERE LTRIM(RTRIM(fname)) = @fn
                          AND LTRIM(RTRIM(lname)) = @ln
                          AND date_of_birth = @dob";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@fn", fname);
                    cmd.Parameters.AddWithValue("@ln", lname);
                    cmd.Parameters.AddWithValue("@dob", dob.Date);

                    conn.Open();
                    SqlDataReader dr = cmd.ExecuteReader();

                    if (!dr.HasRows)
                    {
                        return "No matching records found in the database.";
                    }

                    string result = "Existing record(s):\n";
                    int count = 0;

                    while (dr.Read() && count < 3)
                    {
                        result += $"• ID: {dr["patient_id"]}, Contact: {dr["contact_no"]}, Address: {dr["address"]}\n";
                        count++;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                return $"Error checking duplicates: {ex.Message}";
            }
        }

        /// <summary>
        /// Normalizes a name by trimming and removing extra spaces.
        /// </summary>
        private string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "";

            // Trim and collapse multiple spaces into single space
            name = name.Trim();
            return System.Text.RegularExpressions.Regex.Replace(name, @"\s+", " ");
        }

        // ── Upsert helpers ───────────────────────────────────────────────────

        private void UpsertVitals(SqlConnection conn, SqlTransaction trans, string pid, string bp, string temp, string wt)
        {
            bool exists = Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM dbo.VitalSigns WHERE patient_id=@pid", conn, trans)
            { Parameters = { new SqlParameter("@pid", pid) } }.ExecuteScalar()) > 0;

            string sql = exists
                ? "UPDATE dbo.VitalSigns SET blood_pressure=@bp,temperature=@t,wt=@w WHERE patient_id=@pid"
                : "INSERT INTO dbo.VitalSigns (patient_id,blood_pressure,temperature,wt) VALUES (@pid,@bp,@t,@w)";

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@bp", bp);
            cmd.Parameters.AddWithValue("@t", string.IsNullOrWhiteSpace(temp) ? (object)DBNull.Value : temp);
            cmd.Parameters.AddWithValue("@w", string.IsNullOrWhiteSpace(wt) ? (object)DBNull.Value : wt);
            cmd.ExecuteNonQuery();
        }

        // ── UI helpers ───────────────────────────────────────────────────────

        private void ShowRecordPreview() { pnlRecordPreviewContainer.Visible = true; }

        private void HideRecordPreview()
        {
            pnlRecordPreviewContainer.Visible = false;
            pnlPatientPreview.Visible = false;
            pnlCasePreview.Visible = false;
            hfSelectedPatientId.Value = "";
            hfSelectedCaseId.Value = "";
            hfEditMode.Value = "";
        }

        // ── Getters for patient wizard ──────────────────────────────────────

        private string GetSelectedCategory() { return ddlCategory.SelectedValue; }
        private string GetBitingAnimal() { return ddlBitingAnimal.SelectedValue; }
        private string GetOwnership() { return ddlOwnership.SelectedValue; }
        private string GetCircumstance() { return ddlCircumstance.SelectedValue; }
        private string GetAnimalStatus() { return ddlAnimalStatus.SelectedValue; }
        private string GetWashed() { return ddlWoundWashed.SelectedValue; }
        private string GetManifestation() { return ddlManifestation.SelectedValue; }

        // ── Getters for case wizard ─────────────────────────────────────────

        private string GetCaseCategory() { return ddlCaseCategory.SelectedValue; }
        private string GetCaseBitingAnimal() { return ddlCaseBitingAnimal.SelectedValue; }
        private string GetCaseOwnership() { return ddlCaseOwnership.SelectedValue; }
        private string GetCaseCircumstance() { return ddlCaseCircumstance.SelectedValue; }
        private string GetCaseAnimalStatus() { return ddlCaseAnimalStatus.SelectedValue; }
        private string GetCaseWashed() { return ddlCaseWoundWashed.SelectedValue; }
        private string GetCaseManifestation() { return ddlCaseManifestation.SelectedValue; }

        private string SafeDropdownValue(DropDownList ddl, string value)
        {
            return ddl.Items.FindByValue(value) != null ? value : "";
        }

        private DateTime? ParseNullableDate(string value)
        {
            DateTime d;
            return DateTime.TryParse(value, out d) ? (DateTime?)d.Date : null;
        }

        private void ShowAlert(string message, string type = "info")
        {
            // Escape the message for JavaScript
            string safe = message.Replace("\\", "\\\\")
                                .Replace("'", "\\'")
                                .Replace("\"", "\\\"")
                                .Replace(Environment.NewLine, " ")
                                .Replace("\r", "")
                                .Replace("\n", " ");

            ClientScript.RegisterStartupScript(this.GetType(), Guid.NewGuid().ToString(),
                "showNotifyModal('" + safe + "','" + type + "');", true);
        }

        protected void gvPatients_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C")
            {
                LinkButton btn = e.Row.FindControl("btnEditPatient") as LinkButton;
                if (btn != null) btn.Visible = false;
            }
        }

        protected void gvCases_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType != DataControlRowType.DataRow) return;
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C")
            {
                LinkButton btn = e.Row.FindControl("btnEditCase") as LinkButton;
                if (btn != null) btn.Visible = false;
            }
        }
    }
}