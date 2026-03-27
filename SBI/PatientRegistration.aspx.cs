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

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["userRole"] == null)
            { Response.Redirect("Login.aspx"); return; }

            string role = Session["userRole"].ToString().ToUpper();
            if (role != "A" && role != "B" && role != "C")
            { Response.Redirect("Login.aspx"); return; }

            btnAddPanel.Text = "Add New Patient / Case";
            btnAddPanel.Visible = (role != "C");

            // Always runs on every request including postbacks
            btnAddPanel.Visible = (role != "C");

            if (!IsPostBack)
            {
                FBindGrid();
                HideRecordPreview();

                hfActivePanel.Value = "viewPatientPanel";
                hfSelectedPatientId.Value = "";
                hfSelectedCaseId.Value = "";
                hfEditMode.Value = "";
            }
        }

        private void FBindGrid()
        {
            BindPatients(txtSearchPatient.Text.Trim(), ParseNullableDate(txtPatientDateFrom.Text), ParseNullableDate(txtPatientDateTo.Text));
            BindCases(txtSearchCase.Text.Trim(), ParseNullableDate(txtCaseDateFrom.Text), ParseNullableDate(txtCaseDateTo.Text));
        }

        private void BindPatients(string searchText = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        p.patient_id,
                        p.fname,
                        p.lname,
                        p.gender,
                        p.contact_no,
                        ISNULL(a.barangay, '') + CASE WHEN a.city_province IS NOT NULL AND a.city_province != '' THEN ', ' + a.city_province ELSE '' END AS address,
                        p.date_recorded AS date_added
                    FROM dbo.Patient p
                    LEFT JOIN dbo.Address a ON p.patient_id = a.patient_id
                    WHERE
                        (@search = '' OR
                         CAST(p.patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         p.fname LIKE '%' + @search + '%' OR
                         p.lname LIKE '%' + @search + '%' OR
                         (p.fname + ' ' + p.lname) LIKE '%' + @search + '%' OR
                         p.contact_no LIKE '%' + @search + '%' OR
                         ISNULL(a.barangay, '') LIKE '%' + @search + '%' OR
                         ISNULL(a.city_province, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR CAST(p.date_recorded AS DATE) >= @fromDate)
                        AND (@toDate   IS NULL OR CAST(p.date_recorded AS DATE) <= @toDate)
                    ORDER BY p.date_recorded DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchText);
                da.SelectCommand.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);

                DataTable dt = new DataTable();
                da.Fill(dt);
                gvPatients.DataSource = dt;
                gvPatients.DataBind();
            }
        }

        private void BindCases(string searchText = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT
                        c.case_id,
                        c.patient_id,
                        c.case_no,
                        c.date_of_bite,
                        ISNULL(pb.barangay, '') + CASE WHEN pb.city_province IS NOT NULL AND pb.city_province != '' THEN ', ' + pb.city_province ELSE '' END AS place_of_bite,
                        c.type_of_exposure,
                        c.site_of_bite,
                        c.category
                    FROM dbo.[Case] c
                    LEFT JOIN dbo.PlaceOfBite pb ON c.case_id = pb.case_id
                    WHERE
                        (@search = '' OR
                         CAST(c.case_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         CAST(c.patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         ISNULL(c.case_no, '') LIKE '%' + @search + '%' OR
                         ISNULL(pb.barangay, '') LIKE '%' + @search + '%' OR
                         ISNULL(pb.city_province, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.type_of_exposure, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.site_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(c.category, '') LIKE '%' + @search + '%')
                        AND (@fromDate IS NULL OR c.date_of_bite >= @fromDate)
                        AND (@toDate   IS NULL OR c.date_of_bite <= @toDate)
                    ORDER BY c.date_of_bite DESC, c.case_id DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchText);
                da.SelectCommand.Parameters.AddWithValue("@fromDate", (object)fromDate ?? DBNull.Value);
                da.SelectCommand.Parameters.AddWithValue("@toDate", (object)toDate ?? DBNull.Value);

                DataTable dt = new DataTable();
                da.Fill(dt);
                gvCases.DataSource = dt;
                gvCases.DataBind();
            }
        }

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
                        a.house_no,
                        a.street,
                        a.barangay,
                        a.city_province,
                        ec.emergency_contact_person,
                        ec.emergency_contact_number
                    FROM dbo.Patient p
                    LEFT JOIN dbo.VitalSigns      vs ON p.patient_id = vs.patient_id
                    LEFT JOIN dbo.Address          a  ON p.patient_id = a.patient_id
                    LEFT JOIN dbo.EmergencyContact ec ON p.patient_id = ec.patient_id
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
                    txtPreviewBarangay.Text = dr["barangay"] == DBNull.Value ? "" : dr["barangay"].ToString();
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
                        pb.house_no,
                        pb.street,
                        pb.barangay,
                        pb.city_province
                    FROM dbo.[Case] c
                    LEFT JOIN dbo.PlaceOfBite pb ON c.case_id = pb.case_id
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

                    txtPreviewCasePlaceHouseNo.Text = dr["house_no"] == DBNull.Value ? "" : dr["house_no"].ToString();
                    txtPreviewCasePlaceStreet.Text = dr["street"] == DBNull.Value ? "" : dr["street"].ToString();
                    txtPreviewCasePlaceBarangay.Text = dr["barangay"] == DBNull.Value ? "" : dr["barangay"].ToString();
                    txtPreviewCasePlaceCity.Text = dr["city_province"] == DBNull.Value ? "" : dr["city_province"].ToString();

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

        protected void btnPreviewUpdatePatient_Click(object sender, EventArgs e)
        {
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C") { ShowAlert("You do not have permission to update patient records.", "error"); return; }
            if (string.IsNullOrWhiteSpace(txtPreviewPatientId.Text)) { ShowAlert("No patient selected."); return; }

            DateTime dob;
            if (!DateTime.TryParse(txtPreviewDOB.Text, out dob)) { ShowAlert("Invalid Date of Birth."); return; }
            if (dob > DateTime.Today) { ShowAlert("Date of Birth cannot be a future date."); return; }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    string pid = txtPreviewPatientId.Text.Trim();

                    new SqlCommand(@"
                        UPDATE dbo.Patient
                        SET fname=@fn, lname=@ln, date_of_birth=@dob, gender=@g,
                            civil_status=@cs, contact_no=@cn, occupation=@oc
                        WHERE patient_id=@pid", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@fn",  txtPreviewFirstName.Text.Trim()),
                            new SqlParameter("@ln",  txtPreviewLastName.Text.Trim()),
                            new SqlParameter("@dob", dob),
                            new SqlParameter("@g",   ddlPreviewGender.SelectedValue),
                            new SqlParameter("@cs",  ddlPreviewCivilStatus.SelectedValue),
                            new SqlParameter("@cn",  txtPreviewContactNo.Text.Trim()),
                            new SqlParameter("@oc",  ddlPreviewOccupation.SelectedValue),
                            new SqlParameter("@pid", pid)
                        }
                    }.ExecuteNonQuery();

                    UpsertRecord(conn, trans,
                        "SELECT COUNT(*) FROM dbo.Address WHERE patient_id=@k",
                        "UPDATE dbo.Address SET house_no=@h,street=@s,barangay=@b,city_province=@c WHERE patient_id=@k",
                        "INSERT INTO dbo.Address (patient_id,house_no,street,barangay,city_province) VALUES (@k,@h,@s,@b,@c)",
                        pid,
                        txtPreviewHouseNo.Text.Trim(), txtPreviewStreet.Text.Trim(),
                        txtPreviewBarangay.Text.Trim(), txtPreviewCityProvince.Text.Trim());

                    UpsertEC(conn, trans, pid,
                        txtPreviewEmergencyPerson.Text.Trim(),
                        txtPreviewEmergencyNo.Text.Trim());

                    UpsertVitals(conn, trans, pid,
                        txtPreviewBP.Text.Trim(),
                        txtPreviewTemp.Text.Trim(),
                        txtPreviewWeight.Text.Trim());

                    trans.Commit();
                    FBindGrid();
                    LoadPatientPreview(pid);
                    ShowAlert("Patient information updated successfully.", "success");
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
                            category=@cat, washed=@w
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
                            new SqlParameter("@cid", caseId)
                        }
                    }.ExecuteNonQuery();

                    UpsertPlaceOfBite(conn, trans, caseId,
                        txtPreviewCasePlaceHouseNo.Text.Trim(), txtPreviewCasePlaceStreet.Text.Trim(),
                        txtPreviewCasePlaceBarangay.Text.Trim(), txtPreviewCasePlaceCity.Text.Trim());

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
            BindPatients(txtSearchPatient.Text.Trim(), ParseNullableDate(txtPatientDateFrom.Text), ParseNullableDate(txtPatientDateTo.Text));
            HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnResetPatientSearch_Click(object sender, EventArgs e)
        {
            txtSearchPatient.Text = txtPatientDateFrom.Text = txtPatientDateTo.Text = "";
            BindPatients(); HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnSearchCase_Click(object sender, EventArgs e)
        {
            BindCases(txtSearchCase.Text.Trim(), ParseNullableDate(txtCaseDateFrom.Text), ParseNullableDate(txtCaseDateTo.Text));
            HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnResetCaseSearch_Click(object sender, EventArgs e)
        {
            txtSearchCase.Text = txtCaseDateFrom.Text = txtCaseDateTo.Text = "";
            BindCases(); HideRecordPreview();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            string role = Session["userRole"]?.ToString().ToUpper() ?? "";
            if (role == "C") { ShowAlert("You do not have permission to register patients.", "error"); return; }

            DateTime dob, biteDateTime;
            if (!DateTime.TryParse(txtDOB.Text, out dob)) { ShowAlert("Invalid Date of Birth", "warning"); return; }
            if (dob > DateTime.Today) { ShowAlert("Date of Birth cannot be a future date.", "warning"); return; }
            if (!DateTime.TryParse(txtBiteDateTime.Text, out biteDateTime)) { ShowAlert("Invalid Bite Date and Time", "warning"); return; }
            if (biteDateTime > DateTime.Now.AddMinutes(1))
            {
                ShowAlert("Date and Time of Bite cannot be in the future.", "warning");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();
                try
                {
                    new SqlCommand(@"
                        INSERT INTO dbo.Patient (fname,lname,date_of_birth,gender,civil_status,contact_no,occupation,date_recorded)
                        VALUES (@fn,@ln,@dob,@g,@cs,@cn,@oc,GETDATE())", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@fn",  txtFirstName.Text.Trim()),
                            new SqlParameter("@ln",  txtLastName.Text.Trim()),
                            new SqlParameter("@dob", dob),
                            new SqlParameter("@g",   ddlGender.SelectedValue),
                            new SqlParameter("@cs",  ddlCivilStatus.SelectedValue),
                            new SqlParameter("@cn",  txtContactNo.Text.Trim()),
                            new SqlParameter("@oc",  ddlOccupation.SelectedValue)
                        }
                    }.ExecuteNonQuery();

                    string newPatientId = new SqlCommand(
                        "SELECT TOP 1 patient_id FROM dbo.Patient ORDER BY date_recorded DESC",
                        conn, trans).ExecuteScalar().ToString();

                    new SqlCommand(@"
                        INSERT INTO dbo.Address (patient_id,house_no,street,barangay,city_province)
                        VALUES (@pid,@h,@s,@b,@c)", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@pid", newPatientId),
                            new SqlParameter("@h",   txtHouseNo.Text.Trim()),
                            new SqlParameter("@s",   txtSubdivision.Text.Trim()),
                            new SqlParameter("@b",   txtBarangay.Text.Trim()),
                            new SqlParameter("@c",   txtProvinceCity.Text.Trim())
                        }
                    }.ExecuteNonQuery();

                    new SqlCommand(@"
                        INSERT INTO dbo.EmergencyContact (patient_id,emergency_contact_person,emergency_contact_number)
                        VALUES (@pid,@p,@n)", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@pid", newPatientId),
                            new SqlParameter("@p",   txtEmergencyContactPerson.Text.Trim()),
                            new SqlParameter("@n",   txtEmergencyContactNo.Text.Trim())
                        }
                    }.ExecuteNonQuery();

                    if (!string.IsNullOrWhiteSpace(txtBloodPressure.Text) ||
                        !string.IsNullOrWhiteSpace(txtTemperature.Text) ||
                        !string.IsNullOrWhiteSpace(txtWeight.Text))
                    {
                        UpsertVitals(conn, trans, newPatientId,
                            txtBloodPressure.Text.Trim(),
                            txtTemperature.Text.Trim(),
                            txtWeight.Text.Trim());
                    }

                    string newCaseNo = new SqlCommand(
                        "SELECT 'C' + CAST(YEAR(GETDATE()) AS VARCHAR) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR dbo.SeqCase AS VARCHAR), 4)",
                        conn, trans).ExecuteScalar().ToString();

                    SqlCommand cmdCase = new SqlCommand(@"
                        INSERT INTO dbo.[Case]
                        (patient_id,case_no,date_of_bite,time_of_bite,type_of_exposure,wound_type,bleeding,site_of_bite,category,washed)
                        VALUES (@pid,@cno,@date,@time,@et,@wt,@bl,@sb,@cat,@w);
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
                    int newCaseId = Convert.ToInt32(cmdCase.ExecuteScalar());

                    UpsertPlaceOfBite(conn, trans, newCaseId,
                        txtPlaceHouseNo.Text.Trim(), txtPlaceStreet.Text.Trim(),
                        txtPlaceBarangay.Text.Trim(), txtPlaceCity.Text.Trim());

                    new SqlCommand(@"
                        INSERT INTO dbo.Animal (case_id,animal_type,ownership,animal_status,circumstances)
                        VALUES (@cid,@at,@ow,@as,@ci)", conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@cid", newCaseId),
                            new SqlParameter("@at",  GetBitingAnimal()),
                            new SqlParameter("@ow",  string.IsNullOrEmpty(GetOwnership())    ? (object)DBNull.Value : GetOwnership()),
                            new SqlParameter("@as",  string.IsNullOrEmpty(GetAnimalStatus()) ? (object)DBNull.Value : GetAnimalStatus()),
                            new SqlParameter("@ci",  string.IsNullOrEmpty(GetCircumstance()) ? (object)DBNull.Value : GetCircumstance())
                        }
                    }.ExecuteNonQuery();

                    string symptom = GetManifestation();
                    if (!string.IsNullOrEmpty(symptom) && symptom != "None")
                    {
                        new SqlCommand(@"
                            INSERT INTO dbo.Manifestation (case_id,symptom,present)
                            VALUES (@cid,@sym,'Yes')", conn, trans)
                        {
                            Parameters = {
                                new SqlParameter("@cid", newCaseId),
                                new SqlParameter("@sym", symptom)
                            }
                        }.ExecuteNonQuery();
                    }

                    string initialDiagnosis = BuildInitialDiagnosis(GetSelectedCategory(), GetBitingAnimal());

                    new SqlCommand(@"
                        INSERT INTO dbo.Visit
                            (case_id, visit_type, visit_date, diagnosis, manifestation_notes, status)
                        VALUES
                            (@cid, 'Initial Visit', @vdate, @diag, @notes, 'Completed')",
                        conn, trans)
                    {
                        Parameters = {
                            new SqlParameter("@cid",   newCaseId),
                            new SqlParameter("@vdate", biteDateTime.Date),
                            new SqlParameter("@diag",  (object)initialDiagnosis ?? DBNull.Value),
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

        protected void btnUpdateRecord_Click(object sender, EventArgs e) { ShowAlert("Use the Update button inside the Record Preview."); }
        protected void btnCancelEditForm_Click(object sender, EventArgs e) { ClearFormFields(); hfEditMode.Value = ""; hfActivePanel.Value = "viewPatientPanel"; }
        protected void btnClear_Click(object sender, EventArgs e) { ClearFormFields(); }

        private void ClearFormFields()
        {
            txtFirstName.Text = txtLastName.Text = txtDOB.Text = txtContactNo.Text = "";
            ddlGender.SelectedIndex = ddlCivilStatus.SelectedIndex = ddlOccupation.SelectedIndex = 0;

            txtHouseNo.Text = txtSubdivision.Text = txtBarangay.Text = txtProvinceCity.Text = "";
            txtEmergencyContactPerson.Text = txtEmergencyContactNo.Text = "";
            txtBloodPressure.Text = txtTemperature.Text = txtWeight.Text = txtCapillaryRefill.Text = "";

            txtBiteDateTime.Text = txtWoundLocation.Text = "";
            ddlExposureType.SelectedIndex = ddlWoundType.SelectedIndex = ddlBleeding.SelectedIndex = 0;

            txtPlaceHouseNo.Text = txtPlaceStreet.Text = txtPlaceBarangay.Text = txtPlaceCity.Text = "";

            ddlBitingAnimal.SelectedIndex = ddlOwnership.SelectedIndex = ddlCircumstance.SelectedIndex = 0;
            ddlAnimalStatus.SelectedIndex = ddlWoundWashed.SelectedIndex = ddlCategory.SelectedIndex = 0;
            ddlManifestation.SelectedIndex = 0;
        }

        // ── Upsert helpers ───────────────────────────────────────────────────

        private void UpsertRecord(SqlConnection conn, SqlTransaction trans,
            string checkSql, string updateSql, string insertSql,
            string key, string h, string s, string b, string c)
        {
            SqlCommand chk = new SqlCommand(checkSql, conn, trans);
            chk.Parameters.AddWithValue("@k", key);
            bool exists = Convert.ToInt32(chk.ExecuteScalar()) > 0;

            SqlCommand cmd = new SqlCommand(exists ? updateSql : insertSql, conn, trans);
            cmd.Parameters.AddWithValue("@k", key);
            cmd.Parameters.AddWithValue("@h", h);
            cmd.Parameters.AddWithValue("@s", s);
            cmd.Parameters.AddWithValue("@b", b);
            cmd.Parameters.AddWithValue("@c", c);
            cmd.ExecuteNonQuery();
        }

        private void UpsertEC(SqlConnection conn, SqlTransaction trans, string pid, string person, string number)
        {
            bool exists = Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM dbo.EmergencyContact WHERE patient_id=@pid", conn, trans)
            { Parameters = { new SqlParameter("@pid", pid) } }.ExecuteScalar()) > 0;

            string sql = exists
                ? "UPDATE dbo.EmergencyContact SET emergency_contact_person=@p, emergency_contact_number=@n WHERE patient_id=@pid"
                : "INSERT INTO dbo.EmergencyContact (patient_id,emergency_contact_person,emergency_contact_number) VALUES (@pid,@p,@n)";

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@pid", pid);
            cmd.Parameters.AddWithValue("@p", person);
            cmd.Parameters.AddWithValue("@n", number);
            cmd.ExecuteNonQuery();
        }

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

        private void UpsertPlaceOfBite(SqlConnection conn, SqlTransaction trans, int caseId, string h, string s, string b, string c)
        {
            bool exists = Convert.ToInt32(new SqlCommand(
                "SELECT COUNT(*) FROM dbo.PlaceOfBite WHERE case_id=@cid", conn, trans)
            { Parameters = { new SqlParameter("@cid", caseId) } }.ExecuteScalar()) > 0;

            string sql = exists
                ? "UPDATE dbo.PlaceOfBite SET house_no=@h,street=@s,barangay=@b,city_province=@c WHERE case_id=@cid"
                : "INSERT INTO dbo.PlaceOfBite (case_id,house_no,street,barangay,city_province) VALUES (@cid,@h,@s,@b,@c)";

            SqlCommand cmd = new SqlCommand(sql, conn, trans);
            cmd.Parameters.AddWithValue("@cid", caseId);
            cmd.Parameters.AddWithValue("@h", h);
            cmd.Parameters.AddWithValue("@s", s);
            cmd.Parameters.AddWithValue("@b", b);
            cmd.Parameters.AddWithValue("@c", c);
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

        private string GetSelectedCategory() { return ddlCategory.SelectedValue; }
        private string GetBitingAnimal() { return ddlBitingAnimal.SelectedValue; }
        private string GetOwnership() { return ddlOwnership.SelectedValue; }
        private string GetCircumstance() { return ddlCircumstance.SelectedValue; }
        private string GetAnimalStatus() { return ddlAnimalStatus.SelectedValue; }
        private string GetWashed() { return ddlWoundWashed.SelectedValue; }
        private string GetManifestation() { return ddlManifestation.SelectedValue; }

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
            string safe = message.Replace("\\", "\\\\").Replace("'", "\\'")
                                 .Replace(Environment.NewLine, " ").Replace("\r", "").Replace("\n", " ");
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