using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;

namespace SBI
{
    public partial class PatientRegistration : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["BiteTrackConnection"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                FBindGrid();
                ShowEmptyPreview();
                SetFormMode(false);

                hfActivePanel.Value = "addPatientPanel";
                hfSelectedPatientId.Value = "";
                hfSelectedCaseId.Value = "";
                hfEditMode.Value = "";
            }
        }

        private void FBindGrid()
        {
            BindPatients(txtSearchPatient.Text.Trim());
            BindCases(txtSearchCase.Text.Trim());
        }

        private void BindPatients(string searchText = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT patient_id, fname, lname, gender, contact_no, address, date_recorded
                    FROM Patient
                    WHERE
                        (@search = '' OR
                         CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         fname LIKE '%' + @search + '%' OR
                         lname LIKE '%' + @search + '%' OR
                         (fname + ' ' + lname) LIKE '%' + @search + '%' OR
                         contact_no LIKE '%' + @search + '%' OR
                         address LIKE '%' + @search + '%')
                    ORDER BY date_recorded DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchText);

                DataTable dt = new DataTable();
                da.Fill(dt);

                gvPatients.DataSource = dt;
                gvPatients.DataBind();
            }
        }

        private void BindCases(string searchText = "")
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT case_id, patient_id, case_no, date_of_bite, place_of_bite,
                           type_of_exposure, site_of_bite, category
                    FROM [Case]
                    WHERE
                        (@search = '' OR
                         CAST(case_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         CAST(patient_id AS NVARCHAR(50)) LIKE '%' + @search + '%' OR
                         ISNULL(case_no, '') LIKE '%' + @search + '%' OR
                         ISNULL(place_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(type_of_exposure, '') LIKE '%' + @search + '%' OR
                         ISNULL(site_of_bite, '') LIKE '%' + @search + '%' OR
                         ISNULL(category, '') LIKE '%' + @search + '%')
                    ORDER BY date_of_bite DESC, case_id DESC";

                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.SelectCommand.Parameters.AddWithValue("@search", searchText);

                DataTable dt = new DataTable();
                da.Fill(dt);

                gvCases.DataSource = dt;
                gvCases.DataBind();
            }
        }

        protected void gvPatients_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewPatient")
            {
                string patientId = e.CommandArgument.ToString();
                LoadPatientPreview(patientId);
                hfActivePanel.Value = "viewPatientPanel";
            }
            else if (e.CommandName == "EditPatient")
            {
                string patientId = e.CommandArgument.ToString();
                BeginEditPatient(patientId);
            }
        }

        protected void gvCases_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "ViewCase")
            {
                int caseId = Convert.ToInt32(e.CommandArgument);
                LoadCasePreview(caseId);
                hfActivePanel.Value = "viewPatientPanel";
            }
            else if (e.CommandName == "EditCase")
            {
                int caseId = Convert.ToInt32(e.CommandArgument);
                BeginEditCase(caseId);
            }
        }

        private void LoadPatientPreview(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM Patient WHERE patient_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", patientId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    hfSelectedPatientId.Value = SafeString(dr["patient_id"]);

                    pnlPreviewEmpty.Visible = false;
                    pnlPatientPreview.Visible = true;
                    pnlCasePreview.Visible = false;

                    lblPatientId.Text = SafeString(dr["patient_id"]);
                    lblPatientName.Text = SafeString(dr["fname"]) + " " + SafeString(dr["lname"]);
                    lblPatientDOB.Text = SafeDate(dr["date_of_birth"]);
                    lblPatientGender.Text = SafeString(dr["gender"]);
                    lblPatientCivilStatus.Text = SafeString(dr["civil_status"]);
                    lblPatientAddress.Text = SafeString(dr["address"]);
                    lblPatientContact.Text = SafeString(dr["contact_no"]);
                    lblPatientOccupation.Text = SafeString(dr["occupation"]);
                    lblPatientEmergencyPerson.Text = SafeString(dr["emergency_contact_person"]);
                    lblPatientEmergencyNo.Text = SafeString(dr["emergency_contact_no"]);
                    lblPatientDateAdded.Text = SafeDate(dr["date_recorded"]);

                    dr.Close();
                    LoadVitalSigns(patientId);
                }
            }
        }

        private void LoadVitalSigns(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 1 blood_pressure, temperature, capillary_refill, wt 
                                FROM VitalSigns 
                                WHERE patient_id = @pid 
                                ORDER BY vital_signs_id DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pid", patientId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    lblPatientBP.Text = SafeString(dr["blood_pressure"], true);
                    lblPatientTemp.Text = SafeString(dr["temperature"], true);
                    lblPatientCapillaryRefill.Text = SafeString(dr["capillary_refill"], true);
                    lblPatientWeight.Text = SafeString(dr["wt"], true);
                }
                else
                {
                    lblPatientBP.Text = "-";
                    lblPatientTemp.Text = "-";
                    lblPatientCapillaryRefill.Text = "-";
                    lblPatientWeight.Text = "-";
                }
            }
        }

        private void LoadCasePreview(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM [Case] WHERE case_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", caseId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    hfSelectedCaseId.Value = SafeString(dr["case_id"]);

                    pnlPreviewEmpty.Visible = false;
                    pnlPatientPreview.Visible = false;
                    pnlCasePreview.Visible = true;

                    lblCaseId.Text = SafeString(dr["case_id"]);
                    lblCasePatientId.Text = SafeString(dr["patient_id"]);
                    lblCaseNo.Text = SafeString(dr["case_no"], true);
                    lblCaseDateOfBite.Text = SafeDate(dr["date_of_bite"]);
                    lblCaseTimeOfBite.Text = SafeString(dr["time_of_bite"], true);
                    lblCasePlaceOfBite.Text = SafeString(dr["place_of_bite"]);
                    lblCaseExposureType.Text = SafeString(dr["type_of_exposure"]);
                    lblCaseWoundType.Text = SafeString(dr["wound_type"]);
                    lblCaseBleeding.Text = SafeString(dr["bleeding"]);
                    lblCaseSiteOfBite.Text = SafeString(dr["site_of_bite"]);
                    lblCaseCategory.Text = SafeString(dr["category"]);
                    lblCaseWashed.Text = SafeString(dr["washed"], true);
                }
            }
        }

        private void BeginEditPatient(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM Patient WHERE patient_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", patientId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    hfSelectedPatientId.Value = SafeString(dr["patient_id"]);
                    hfEditMode.Value = "PATIENT";

                    txtFirstName.Text = SafeString(dr["fname"]);
                    txtLastName.Text = SafeString(dr["lname"]);

                    if (dr["date_of_birth"] != DBNull.Value)
                        txtDOB.Text = Convert.ToDateTime(dr["date_of_birth"]).ToString("yyyy-MM-dd");
                    else
                        txtDOB.Text = "";

                    ddlGender.SelectedValue = SafeDropdownValue(ddlGender, SafeString(dr["gender"]));
                    ddlCivilStatus.SelectedValue = SafeDropdownValue(ddlCivilStatus, SafeString(dr["civil_status"]));
                    txtContactNo.Text = SafeString(dr["contact_no"]);
                    ddlOccupation.SelectedValue = SafeDropdownValue(ddlOccupation, SafeString(dr["occupation"]));
                    txtEmergencyContactPerson.Text = SafeString(dr["emergency_contact_person"]);
                    txtEmergencyContactNo.Text = SafeString(dr["emergency_contact_no"]);

                    SplitAddress(SafeString(dr["address"]));

                    dr.Close();
                    LoadVitalSignsForEdit(patientId);

                    SetFormMode(true);
                    hfActivePanel.Value = "addPatientPanel";
                }
            }
        }

        private void LoadVitalSignsForEdit(string patientId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT TOP 1 blood_pressure, temperature, capillary_refill, wt 
                                FROM VitalSigns 
                                WHERE patient_id = @pid 
                                ORDER BY vital_signs_id DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pid", patientId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    txtBloodPressure.Text = SafeString(dr["blood_pressure"]);
                    txtTemperature.Text = SafeString(dr["temperature"]);
                    txtCapillaryRefill.Text = SafeString(dr["capillary_refill"]);
                    txtWeight.Text = SafeString(dr["wt"]);
                }
            }
        }

        private void BeginEditCase(int caseId)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = @"SELECT * FROM [Case] WHERE case_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@id", caseId);

                conn.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    hfSelectedCaseId.Value = SafeString(dr["case_id"]);
                    hfSelectedPatientId.Value = SafeString(dr["patient_id"]);
                    hfEditMode.Value = "CASE";

                    txtPlaceExposure.Text = SafeString(dr["place_of_bite"]);
                    ddlExposureType.SelectedValue = SafeDropdownValue(ddlExposureType, SafeString(dr["type_of_exposure"]));
                    ddlWoundType.SelectedValue = SafeDropdownValue(ddlWoundType, SafeString(dr["wound_type"]));
                    ddlBleeding.SelectedValue = SafeDropdownValue(ddlBleeding, SafeString(dr["bleeding"]));
                    txtWoundLocation.Text = SafeString(dr["site_of_bite"]);

                    string category = SafeString(dr["category"]).Replace("Category ", "").Trim().ToUpper();
                    rbCategory1.Checked = category == "I";
                    rbCategory2.Checked = category == "II";
                    rbCategory3.Checked = category == "III";

                    txtBiteDateTime.Text = FormatDateTimeLocal(dr["date_of_bite"], dr["time_of_bite"]);

                    SetFormMode(true);
                    hfActivePanel.Value = "addPatientPanel";
                }
            }
        }

        protected void btnPreviewEditPatient_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(hfSelectedPatientId.Value))
                BeginEditPatient(hfSelectedPatientId.Value);
        }

        protected void btnPreviewEditCase_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(hfSelectedCaseId.Value))
                BeginEditCase(Convert.ToInt32(hfSelectedCaseId.Value));
        }

        protected void btnCancelPatientPreview_Click(object sender, EventArgs e)
        {
            ShowEmptyPreview();
            hfSelectedPatientId.Value = "";
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnCancelCasePreview_Click(object sender, EventArgs e)
        {
            ShowEmptyPreview();
            hfSelectedCaseId.Value = "";
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnSearchPatient_Click(object sender, EventArgs e)
        {
            BindPatients(txtSearchPatient.Text.Trim());
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnResetPatientSearch_Click(object sender, EventArgs e)
        {
            txtSearchPatient.Text = "";
            BindPatients();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnSearchCase_Click(object sender, EventArgs e)
        {
            BindCases(txtSearchCase.Text.Trim());
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnResetCaseSearch_Click(object sender, EventArgs e)
        {
            txtSearchCase.Text = "";
            BindCases();
            hfActivePanel.Value = "viewPatientPanel";
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            // Validate required fields
            if (string.IsNullOrEmpty(txtFirstName.Text.Trim()) ||
                string.IsNullOrEmpty(txtLastName.Text.Trim()) ||
                string.IsNullOrEmpty(txtDOB.Text.Trim()) ||
                string.IsNullOrEmpty(txtBiteDateTime.Text.Trim()) ||
                string.IsNullOrEmpty(txtPlaceExposure.Text.Trim()))
            {
                ShowAlert("Please fill in all required fields marked with *.");
                return;
            }

            DateTime dob;
            DateTime biteDateTime;

            // Parse Date of Birth
            if (!DateTime.TryParse(txtDOB.Text.Trim(), out dob))
            {
                ShowAlert("Invalid Date of Birth. Please select a valid date.");
                return;
            }

            // Parse Bite Date and Time
            if (!DateTime.TryParse(txtBiteDateTime.Text.Trim(), out biteDateTime))
            {
                ShowAlert("Invalid Bite Date and Time. Please select a valid date and time.");
                return;
            }

            // Validate category selection
            string selectedCategory = GetSelectedCategory();
            if (string.IsNullOrEmpty(selectedCategory))
            {
                ShowAlert("Please select a bite category (I, II, or III).");
                return;
            }

            string fullAddress = BuildFullAddress();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // =========================
                    // INSERT PATIENT
                    // =========================
                    // REMOVED the OUTPUT clause - we'll get the ID differently
                    string patientQuery = @"
                INSERT INTO Patient
                (fname, lname, date_of_birth, gender, civil_status, address, contact_no, occupation,
                 emergency_contact_person, emergency_contact_no)
                VALUES
                (@fname, @lname, @dob, @gender, @civil, @address, @contact, @occupation,
                 @eperson, @eno);";

                    SqlCommand cmdPatient = new SqlCommand(patientQuery, conn, trans);
                    cmdPatient.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@dob", dob);
                    cmdPatient.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@civil", ddlCivilStatus.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@address", fullAddress);
                    cmdPatient.Parameters.AddWithValue("@contact", txtContactNo.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@occupation", ddlOccupation.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@eperson", txtEmergencyContactPerson.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@eno", txtEmergencyContactNo.Text.Trim());

                    cmdPatient.ExecuteNonQuery();

                    // Get the newly inserted patient_id (assuming it's generated by the database)
                    string getPatientIdQuery = "SELECT MAX(patient_id) FROM Patient WHERE fname = @fname AND lname = @lname AND date_of_birth = @dob";
                    SqlCommand cmdGetId = new SqlCommand(getPatientIdQuery, conn, trans);
                    cmdGetId.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
                    cmdGetId.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
                    cmdGetId.Parameters.AddWithValue("@dob", dob);

                    object result = cmdGetId.ExecuteScalar();
                    if (result == null || result == DBNull.Value)
                        throw new Exception("Failed to retrieve new patient ID.");

                    string patientId = result.ToString();

                    // =========================
                    // INSERT VITAL SIGNS
                    // =========================
                    string vitalQuery = @"
                INSERT INTO VitalSigns
                (patient_id, blood_pressure, temperature, capillary_refill, wt)
                VALUES
                (@pid, @bp, @temp, @cr, @wt)";

                    SqlCommand cmdVital = new SqlCommand(vitalQuery, conn, trans);
                    cmdVital.Parameters.AddWithValue("@pid", patientId);
                    cmdVital.Parameters.AddWithValue("@bp", NullIfEmpty(txtBloodPressure.Text));
                    cmdVital.Parameters.AddWithValue("@temp", NullIfEmpty(txtTemperature.Text));
                    cmdVital.Parameters.AddWithValue("@cr", NullIfEmpty(txtCapillaryRefill.Text));
                    cmdVital.Parameters.AddWithValue("@wt", NullIfEmpty(txtWeight.Text));

                    cmdVital.ExecuteNonQuery();

                    // =========================
                    // INSERT BITE CASE
                    // =========================
                    string caseQuery = @"
                INSERT INTO [Case]
                (patient_id, date_of_bite, place_of_bite, time_of_bite, type_of_exposure,
                 wound_type, bleeding, site_of_bite, category)
                VALUES
                (@pid, @date, @place, @time, @exposure, @wound, @bleeding, @site, @category)";

                    SqlCommand cmdCase = new SqlCommand(caseQuery, conn, trans);
                    cmdCase.Parameters.AddWithValue("@pid", patientId);
                    cmdCase.Parameters.AddWithValue("@date", biteDateTime.Date);
                    cmdCase.Parameters.AddWithValue("@place", txtPlaceExposure.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@time", biteDateTime.TimeOfDay);
                    cmdCase.Parameters.AddWithValue("@exposure", ddlExposureType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@wound", ddlWoundType.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@bleeding", ddlBleeding.SelectedValue);
                    cmdCase.Parameters.AddWithValue("@site", txtWoundLocation.Text.Trim());
                    cmdCase.Parameters.AddWithValue("@category", selectedCategory);

                    cmdCase.ExecuteNonQuery();

                    trans.Commit();

                    FBindGrid();
                    ClearFormFields();
                    SetFormMode(false);
                    hfActivePanel.Value = "addPatientPanel";

                    ShowAlert("Patient Registered Successfully.");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ShowAlert("Error: " + ex.Message.Replace("'", "").Replace(Environment.NewLine, " "));
                }
            }
        }
        protected void btnUpdateRecord_Click(object sender, EventArgs e)
        {
            if (hfEditMode.Value == "PATIENT")
                UpdatePatientRecord();
            else if (hfEditMode.Value == "CASE")
                UpdateCaseRecord();
            else
                ShowAlert("No record selected for update.");
        }

        private void UpdatePatientRecord()
        {
            if (string.IsNullOrWhiteSpace(hfSelectedPatientId.Value))
            {
                ShowAlert("No patient selected.");
                return;
            }

            DateTime dob;
            if (!DateTime.TryParse(txtDOB.Text.Trim(), out dob))
            {
                ShowAlert("Invalid Date of Birth. Please select a valid date.");
                return;
            }

            string fullAddress = BuildFullAddress();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    string patientQuery = @"
                        UPDATE Patient
                        SET fname = @fname,
                            lname = @lname,
                            date_of_birth = @dob,
                            gender = @gender,
                            civil_status = @civil,
                            address = @address,
                            contact_no = @contact,
                            occupation = @occupation,
                            emergency_contact_person = @eperson,
                            emergency_contact_no = @eno
                        WHERE patient_id = @id";

                    SqlCommand cmdPatient = new SqlCommand(patientQuery, conn, trans);
                    cmdPatient.Parameters.AddWithValue("@fname", txtFirstName.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@lname", txtLastName.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@dob", dob);
                    cmdPatient.Parameters.AddWithValue("@gender", ddlGender.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@civil", ddlCivilStatus.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@address", fullAddress);
                    cmdPatient.Parameters.AddWithValue("@contact", txtContactNo.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@occupation", ddlOccupation.SelectedValue);
                    cmdPatient.Parameters.AddWithValue("@eperson", txtEmergencyContactPerson.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@eno", txtEmergencyContactNo.Text.Trim());
                    cmdPatient.Parameters.AddWithValue("@id", hfSelectedPatientId.Value);

                    cmdPatient.ExecuteNonQuery();

                    // Upsert vital signs
                    string vitalQuery = @"
                        IF EXISTS (SELECT 1 FROM VitalSigns WHERE patient_id = @pid)
                            UPDATE VitalSigns 
                            SET blood_pressure = @bp, temperature = @temp, capillary_refill = @cr, wt = @wt
                            WHERE patient_id = @pid
                        ELSE
                            INSERT INTO VitalSigns (patient_id, blood_pressure, temperature, capillary_refill, wt)
                            VALUES (@pid, @bp, @temp, @cr, @wt)";

                    SqlCommand cmdVital = new SqlCommand(vitalQuery, conn, trans);
                    cmdVital.Parameters.AddWithValue("@pid", hfSelectedPatientId.Value);
                    cmdVital.Parameters.AddWithValue("@bp", NullIfEmpty(txtBloodPressure.Text));
                    cmdVital.Parameters.AddWithValue("@temp", NullIfEmpty(txtTemperature.Text));
                    cmdVital.Parameters.AddWithValue("@cr", NullIfEmpty(txtCapillaryRefill.Text));
                    cmdVital.Parameters.AddWithValue("@wt", NullIfEmpty(txtWeight.Text));

                    cmdVital.ExecuteNonQuery();

                    trans.Commit();

                    FBindGrid();
                    LoadPatientPreview(hfSelectedPatientId.Value);

                    ClearFormFields();
                    SetFormMode(false);
                    hfEditMode.Value = "";
                    hfActivePanel.Value = "viewPatientPanel";

                    ShowAlert("Patient record updated successfully.");
                }
                catch (Exception ex)
                {
                    trans.Rollback();
                    ShowAlert("Error updating patient: " + ex.Message.Replace("'", "").Replace(Environment.NewLine, " "));
                }
            }
        }

        private void UpdateCaseRecord()
        {
            if (string.IsNullOrWhiteSpace(hfSelectedCaseId.Value))
            {
                ShowAlert("No case selected.");
                return;
            }

            DateTime biteDateTime;
            if (!DateTime.TryParse(txtBiteDateTime.Text.Trim(), out biteDateTime))
            {
                ShowAlert("Invalid Bite Date and Time. Please select a valid date and time.");
                return;
            }

            string selectedCategory = GetSelectedCategory();
            if (string.IsNullOrEmpty(selectedCategory))
            {
                ShowAlert("Please select a bite category (I, II, or III).");
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                try
                {
                    string query = @"
                        UPDATE [Case]
                        SET date_of_bite    = @date,
                            place_of_bite   = @place,
                            time_of_bite    = @time,
                            type_of_exposure = @exposure,
                            wound_type      = @wound,
                            bleeding        = @bleeding,
                            site_of_bite    = @site,
                            category        = @category
                        WHERE case_id = @id";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@date", biteDateTime.Date);
                    cmd.Parameters.AddWithValue("@place", txtPlaceExposure.Text.Trim());
                    cmd.Parameters.AddWithValue("@time", biteDateTime.TimeOfDay);
                    cmd.Parameters.AddWithValue("@exposure", ddlExposureType.SelectedValue);
                    cmd.Parameters.AddWithValue("@wound", ddlWoundType.SelectedValue);
                    cmd.Parameters.AddWithValue("@bleeding", ddlBleeding.SelectedValue);
                    cmd.Parameters.AddWithValue("@site", txtWoundLocation.Text.Trim());
                    cmd.Parameters.AddWithValue("@category", selectedCategory);
                    cmd.Parameters.AddWithValue("@id", hfSelectedCaseId.Value);

                    cmd.ExecuteNonQuery();

                    FBindGrid();
                    LoadCasePreview(Convert.ToInt32(hfSelectedCaseId.Value));

                    ClearFormFields();
                    SetFormMode(false);
                    hfEditMode.Value = "";
                    hfActivePanel.Value = "viewPatientPanel";

                    ShowAlert("Case record updated successfully.");
                }
                catch (Exception ex)
                {
                    ShowAlert("Error updating case: " + ex.Message.Replace("'", "").Replace(Environment.NewLine, " "));
                }
            }
        }

        protected void btnCancelEditForm_Click(object sender, EventArgs e)
        {
            ClearFormFields();
            SetFormMode(false);

            hfEditMode.Value = "";
            hfActivePanel.Value = "viewPatientPanel";

            ShowAlert("Edit cancelled.");
        }

        protected void btnClear_Click(object sender, EventArgs e)
        {
            ClearFormFields();
        }

        private void ClearFormFields()
        {
            txtFirstName.Text = "";
            txtLastName.Text = "";
            txtDOB.Text = "";
            txtContactNo.Text = "";

            txtHouseNo.Text = "";
            txtSubdivision.Text = "";
            txtBarangay.Text = "";
            txtProvinceCity.Text = "";

            txtEmergencyContactPerson.Text = "";
            txtEmergencyContactNo.Text = "";
            txtBloodPressure.Text = "";
            txtTemperature.Text = "";
            txtWeight.Text = "";
            txtCapillaryRefill.Text = "";

            txtBiteDateTime.Text = "";
            txtPlaceExposure.Text = "";
            txtWoundLocation.Text = "";
            txtOtherAnimal.Text = "";

            ddlGender.SelectedIndex = 0;
            ddlCivilStatus.SelectedIndex = 0;
            ddlOccupation.SelectedIndex = 0;
            ddlExposureType.SelectedIndex = 0;
            ddlWoundType.SelectedIndex = 0;
            ddlBleeding.SelectedIndex = 0;

            rbDog.Checked = true;
            rbCat.Checked = false;
            rbOtherAnimal.Checked = false;

            rbProvoked.Checked = false;
            rbUnprovoked.Checked = true;

            rbOwned.Checked = false;
            rbStray.Checked = true;
            rbLeashed.Checked = false;

            rbAlive.Checked = true;
            rbSick.Checked = false;
            rbDied.Checked = false;
            rbUnknown.Checked = false;

            rbCategory1.Checked = false;
            rbCategory2.Checked = false;
            rbCategory3.Checked = false;

            hfSelectedPatientId.Value = "";
            hfSelectedCaseId.Value = "";
        }

        private void ShowEmptyPreview()
        {
            pnlPreviewEmpty.Visible = true;
            pnlPatientPreview.Visible = false;
            pnlCasePreview.Visible = false;
        }

        private void SetFormMode(bool isEdit)
        {
            btnSave.Visible = !isEdit;
            btnUpdateRecord.Visible = isEdit;
            btnCancelEditForm.Visible = isEdit;
        }

        private string BuildFullAddress()
        {
            string[] parts = new string[]
            {
                txtHouseNo.Text.Trim(),
                txtSubdivision.Text.Trim(),
                txtBarangay.Text.Trim(),
                txtProvinceCity.Text.Trim()
            };

            return string.Join(", ", parts.Where(x => !string.IsNullOrWhiteSpace(x)));
        }

        private void SplitAddress(string fullAddress)
        {
            txtHouseNo.Text = "";
            txtSubdivision.Text = "";
            txtBarangay.Text = "";
            txtProvinceCity.Text = "";

            if (string.IsNullOrWhiteSpace(fullAddress))
                return;

            string[] parts = fullAddress
                .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            if (parts.Length >= 4)
            {
                txtHouseNo.Text = parts[0];
                txtSubdivision.Text = parts[1];
                txtBarangay.Text = parts[2];
                txtProvinceCity.Text = parts[3];
            }
            else if (parts.Length == 3)
            {
                txtHouseNo.Text = parts[0];
                txtBarangay.Text = parts[1];
                txtProvinceCity.Text = parts[2];
            }
            else if (parts.Length == 2)
            {
                txtBarangay.Text = parts[0];
                txtProvinceCity.Text = parts[1];
            }
            else if (parts.Length == 1)
            {
                txtProvinceCity.Text = parts[0];
            }
        }

        private string GetSelectedCategory()
        {
            if (rbCategory1.Checked) return "I";
            if (rbCategory2.Checked) return "II";
            if (rbCategory3.Checked) return "III";
            return "";
        }

        private string SafeDropdownValue(DropDownList ddl, string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            ListItem item = ddl.Items.FindByValue(value);
            return item != null ? value : "";
        }

        private string FormatDateTimeLocal(object dateObj, object timeObj)
        {
            if (dateObj == null || dateObj == DBNull.Value)
                return "";

            DateTime datePart = Convert.ToDateTime(dateObj).Date;
            TimeSpan timePart = TimeSpan.Zero;

            if (timeObj != null && timeObj != DBNull.Value)
            {
                if (timeObj is TimeSpan)
                    timePart = (TimeSpan)timeObj;
                else
                {
                    TimeSpan parsedTime;
                    if (TimeSpan.TryParse(timeObj.ToString(), out parsedTime))
                        timePart = parsedTime;
                }
            }

            return datePart.Add(timePart).ToString("yyyy-MM-ddTHH:mm");
        }

        private string SafeString(object value, bool useDash = false)
        {
            if (value == null || value == DBNull.Value)
                return useDash ? "-" : "";
            return value.ToString();
        }

        private string SafeDate(object value, string format = "MMM dd, yyyy")
        {
            if (value == null || value == DBNull.Value)
                return "-";

            try
            {
                return Convert.ToDateTime(value).ToString(format);
            }
            catch
            {
                return "-";
            }
        }

        private object NullIfEmpty(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? (object)DBNull.Value : value.Trim();
        }

        private void ShowAlert(string message)
        {
            string cleanMessage = message.Replace("'", "\\'").Replace("\r", " ").Replace("\n", " ");
            ClientScript.RegisterStartupScript(
                this.GetType(),
                Guid.NewGuid().ToString(),
                "alert('" + cleanMessage + "');",
                true
            );
        }
    }
}