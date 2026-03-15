<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="PatientRegistration.aspx.cs" Inherits="SBI.PatientRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActivePanel" runat="server" Value="viewPatientPanel" />
    <asp:HiddenField ID="hfSelectedPatientId" runat="server" Value="" />
    <asp:HiddenField ID="hfSelectedCaseId" runat="server" Value="" />
    <asp:HiddenField ID="hfEditMode" runat="server" Value="" />

    <div class="px-6 py-6 font-sans text-slate-900">

        <!-- PAGE TITLE -->
        <div class="mb-6">
            <h2 class="text-3xl font-bold text-[#0b2a7a]">Patient Registration</h2>
            <p class="mt-1 text-sm text-slate-500">Register patients, manage bite cases, and update records.</p>
        </div>

        <!-- PANEL TOGGLE BUTTONS -->
        <div class="mb-6 flex flex-wrap gap-3">
            <button type="button" id="btnViewPanel"
                class="panel-tab rounded-lg border border-slate-300 bg-white px-5 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
                onclick="showPanel('viewPatientPanel')">
                View Patient / Case Details
            </button>

            <button type="button" id="btnAddPanel"
                class="panel-tab rounded-lg border border-slate-300 bg-white px-5 py-2 text-sm font-semibold text-slate-700 transition hover:bg-slate-50"
                onclick="showPanel('addPatientPanel')">
                Add New Patient / Case
            </button>
        </div>

        <!-- ====================== ADD PATIENT / CASE PANEL ====================== -->
        <div id="addPatientPanel" class="panel hidden">

            <div class="mb-5">
                <h2 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Patient Registration</h2>
                <p class="mt-1 text-base text-slate-600">
                    Register patient details and document biting incident history
                </p>
            </div>

            <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm flex flex-col min-h-[85vh]">

                <div class="px-5 py-5 space-y-4 flex-1">
                    <h3 class="text-lg font-extrabold text-slate-900 mb-2">A. Patient Information</h3>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">First Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtFirstName" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                placeholder="e.g. Maria" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Last Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtLastName" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                placeholder="e.g. Santos" />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Date of Birth <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtDOB" runat="server" TextMode="Date"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                        </div>

                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Gender <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlGender" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                <asp:ListItem Text="Select Gender" Value="" Selected="True" />
                                <asp:ListItem Text="Male" Value="M" />
                                <asp:ListItem Text="Female" Value="F" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Civil Status</label>
                            <asp:DropDownList ID="ddlCivilStatus" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                <asp:ListItem Text="Select Status" Value="" Selected="True" />
                                <asp:ListItem Text="Single" Value="Single" />
                                <asp:ListItem Text="Married" Value="Married" />
                                <asp:ListItem Text="Widowed" Value="Widowed" />
                                <asp:ListItem Text="Separated" Value="Separated" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Contact No <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtContactNo" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                placeholder="e.g. 09123456789" />
                        </div>
                    </div>

                    <div>
                        <label class="block text-sm font-semibold text-slate-700 mb-1">Address <span class="text-red-500">*</span></label>
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <asp:TextBox ID="txtHouseNo" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="House No." />
                            <asp:TextBox ID="txtSubdivision" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="Subdivision/Street" />
                            <asp:TextBox ID="txtBarangay" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="Barangay *" />
                            <asp:TextBox ID="txtProvinceCity" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="City/Province *" />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Occupation <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlOccupation" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                <asp:ListItem Text="Select Occupation" Value="" Selected="True" />
                                <asp:ListItem Text="Student" Value="Student" />
                                <asp:ListItem Text="Employed" Value="Employed" />
                                <asp:ListItem Text="Self-Employed" Value="Self-Employed" />
                                <asp:ListItem Text="Unemployed" Value="Unemployed" />
                                <asp:ListItem Text="Retired" Value="Retired" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Emergency Contact Person <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtEmergencyContactPerson" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                placeholder="Full name" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Emergency Contact Number <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtEmergencyContactNo" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                placeholder="Contact number" />
                        </div>
                    </div>

                    <div class="pt-2 border-t border-slate-200">
                        <button type="button" onclick="toggleVitals()"
                            class="text-sm font-semibold text-blue-600 hover:text-blue-800">
                            + Optional Vitals &amp; Visit Information
                        </button>
                    </div>

                    <div id="optionalVitals" class="hidden grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 pt-3">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Blood Pressure</label>
                            <asp:TextBox ID="txtBloodPressure" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="e.g. 120/80" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Temperature (°C)</label>
                            <asp:TextBox ID="txtTemperature" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="e.g. 36.5" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Weight (kg)</label>
                            <asp:TextBox ID="txtWeight" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="e.g. 65" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Capillary Refill</label>
                            <asp:TextBox ID="txtCapillaryRefill" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="e.g. < 2 seconds" />
                        </div>
                    </div>
                </div>

                <!-- ===== SECTION B: HISTORY OF BITING INCIDENT ===== -->
                <div class="px-5 py-5 border-t border-slate-200">
                    <h3 class="text-lg font-extrabold text-slate-900 mb-4">B. History of Biting Incident</h3>

                    <div class="space-y-4">

                        <%-- Row 1: Date/Time + Place of Exposure --%>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Date and Time of Bite <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtBiteDateTime" runat="server" TextMode="DateTimeLocal"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Place of Exposure <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtPlaceExposure" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                    placeholder="e.g. Home, Street, etc." />
                            </div>
                        </div>

                        <%-- Row 2: Biting Animal + Ownership + Circumstance --%>
                        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Biting Animal <span class="text-red-500">*</span></label>
                                <asp:DropDownList ID="ddlBitingAnimal" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Animal" Value="" Selected="True" />
                                    <asp:ListItem Text="Dog" Value="Dog" />
                                    <asp:ListItem Text="Cat" Value="Cat" />
                                    <asp:ListItem Text="Others" Value="Others" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Ownership</label>
                                <asp:DropDownList ID="ddlOwnership" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Ownership" Value="" Selected="True" />
                                    <asp:ListItem Text="Owned" Value="Owned" />
                                    <asp:ListItem Text="Stray" Value="Stray" />
                                    <asp:ListItem Text="Leashed / Cage" Value="Leashed/Cage" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Circumstance</label>
                                <asp:DropDownList ID="ddlCircumstance" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Circumstance" Value="" Selected="True" />
                                    <asp:ListItem Text="Provoked / Intentional" Value="Provoked" />
                                    <asp:ListItem Text="Unprovoked / Unintentional" Value="Unprovoked" />
                                </asp:DropDownList>
                            </div>
                        </div>

                        <%-- Row 3: Animal Status + Type of Exposure + Wound Location + Wound Type + Bleeding --%>
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Status of Biting Animal</label>
                                <asp:DropDownList ID="ddlAnimalStatus" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Status" Value="" Selected="True" />
                                    <asp:ListItem Text="Alive / Healthy" Value="Alive/Healthy" />
                                    <asp:ListItem Text="Sick" Value="Sick" />
                                    <asp:ListItem Text="Died / Killed" Value="Died/Killed" />
                                    <asp:ListItem Text="Unknown" Value="Unknown" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Type of Exposure</label>
                                <asp:DropDownList ID="ddlExposureType" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Type" Value="" Selected="True" />
                                    <asp:ListItem Text="Bite" Value="Bite" />
                                    <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Wound Location</label>
                                <asp:TextBox ID="txtWoundLocation" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm"
                                    placeholder="e.g. Left arm, Right leg" />
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Wound Type</label>
                                <asp:DropDownList ID="ddlWoundType" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Type" Value="" Selected="True" />
                                    <asp:ListItem Text="Lacerated" Value="Lacerated" />
                                    <asp:ListItem Text="Avulsion" Value="Avulsion" />
                                    <asp:ListItem Text="Punctured" Value="Punctured" />
                                    <asp:ListItem Text="Abrasion" Value="Abrasion" />
                                    <asp:ListItem Text="Scratches" Value="Scratches" />
                                    <asp:ListItem Text="Hematoma" Value="Hematoma" />
                                </asp:DropDownList>
                            </div>
                        </div>

                        <%-- Row 4: Bleeding + Washing + Category + Manifestation --%>
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Bleeding</label>
                                <asp:DropDownList ID="ddlBleeding" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select" Value="" Selected="True" />
                                    <asp:ListItem Text="No" Value="No" />
                                    <asp:ListItem Text="Yes" Value="Yes" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Washing of Bite Wound</label>
                                <asp:DropDownList ID="ddlWoundWashed" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select" Value="" Selected="True" />
                                    <asp:ListItem Text="Washed (15 mins)" Value="Yes" />
                                    <asp:ListItem Text="Unwashed" Value="No" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Category</label>
                                <asp:DropDownList ID="ddlCategory" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select Category" Value="" Selected="True" />
                                    <asp:ListItem Text="Category I" Value="I" />
                                    <asp:ListItem Text="Category II" Value="II" />
                                    <asp:ListItem Text="Category III" Value="III" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Manifestation</label>
                                <asp:DropDownList ID="ddlManifestation" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                    <asp:ListItem Text="Select" Value="" Selected="True" />
                                    <asp:ListItem Text="Head Ache" Value="Head Ache" />
                                    <asp:ListItem Text="Fever" Value="Fever" />
                                    <asp:ListItem Text="Numbness on Site of Bite" Value="Numbness on Site of Bite" />
                                    <asp:ListItem Text="Tingling Sensation" Value="Tingling Sensation" />
                                    <asp:ListItem Text="None" Value="None" />
                                </asp:DropDownList>
                            </div>
                        </div>

                    </div>
                </div>
                <!-- ===== END SECTION B ===== -->

                <div class="mt-auto px-5 py-4 bg-slate-50 border-t border-slate-200 flex flex-wrap justify-end gap-3">
                    <asp:Button ID="btnClear" runat="server" Text="Clear"
                        CssClass="px-6 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                        OnClick="btnClear_Click" />

                    <asp:Button ID="btnSave" runat="server" Text="Save Patient Record"
                        CssClass="px-6 py-2 rounded-lg bg-blue-700 text-white font-semibold shadow hover:bg-blue-800"
                        OnClick="btnSave_Click" />

                    <asp:Button ID="btnUpdateRecord" runat="server" Text="Update Record"
                        Visible="false"
                        CssClass="px-6 py-2 rounded-lg bg-amber-500 text-white font-semibold shadow hover:bg-amber-600"
                        OnClick="btnUpdateRecord_Click" />

                    <asp:Button ID="btnCancelEditForm" runat="server" Text="Cancel Edit"
                        Visible="false"
                        CssClass="px-6 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                        OnClick="btnCancelEditForm_Click" />
                </div>
            </div>
        </div>

        <!-- ====================== VIEW PANEL ====================== -->
        <div id="viewPatientPanel" class="panel">
            <div id="viewLayout" class="grid grid-cols-1 gap-6 transition-all duration-300 no-preview">

                <!-- LEFT -->
                <div id="detailsPane" class="space-y-6 min-w-0">

                    <!-- Patient Table -->
                    <div class="rounded-xl bg-white border border-slate-200 overflow-hidden">
                        <div class="px-6 py-5 border-b border-slate-100">
                            <h4 class="text-lg font-semibold text-slate-800">Patient Details</h4>
                            <p class="text-sm text-slate-400 mt-0.5">List of registered patients</p>
                        </div>

                        <div class="px-6 py-4 border-b border-slate-100">
                            <div class="flex flex-wrap gap-3 items-center">
                                <asp:TextBox ID="txtSearchPatient" runat="server"
                                    CssClass="h-10 w-80 rounded-lg border border-slate-200 px-3 text-sm text-slate-700 placeholder-slate-400"
                                    placeholder="Search by Patient ID, Name, Contact, Address" />

                                <asp:TextBox ID="txtPatientDateFrom" runat="server" TextMode="Date"
                                    CssClass="h-10 rounded-lg border border-slate-200 px-3 text-sm text-slate-700" />

                                <span class="text-sm text-slate-400">to</span>

                                <asp:TextBox ID="txtPatientDateTo" runat="server" TextMode="Date"
                                    CssClass="h-10 rounded-lg border border-slate-200 px-3 text-sm text-slate-700" />

                                <asp:Button ID="btnSearchPatient" runat="server" Text="Search"
                                    CssClass="h-10 px-5 rounded-lg bg-blue-600 text-white text-sm font-semibold hover:bg-blue-700"
                                    OnClick="btnSearchPatient_Click" />

                                <asp:Button ID="btnResetPatientSearch" runat="server" Text="Clear"
                                    CssClass="h-10 px-5 rounded-lg border border-slate-300 bg-white text-slate-600 text-sm font-semibold hover:bg-slate-50"
                                    OnClick="btnResetPatientSearch_Click" />
                            </div>
                        </div>

                        <div class="overflow-x-auto">
                            <asp:GridView ID="gvPatients" runat="server" AutoGenerateColumns="False"
                                CssClass="w-full min-w-[1050px] text-sm text-left text-slate-700"
                                GridLines="None"
                                EmptyDataText="No patient records found."
                                DataKeyNames="patient_id"
                                OnRowCommand="gvPatients_RowCommand"
                                HeaderStyle-CssClass="text-xs font-semibold text-slate-500 uppercase tracking-wide border-b border-slate-200"
                                RowStyle-CssClass="border-b border-slate-100 hover:bg-slate-50"
                                AlternatingRowStyle-CssClass="border-b border-slate-100 hover:bg-slate-50"
                                EmptyDataRowStyle-CssClass="text-center text-slate-400 italic py-8">

                                <Columns>
                                    <asp:TemplateField HeaderText="Action">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnEditPatient" runat="server"
                                                Text="Edit"
                                                CommandName="EditPatient"
                                                CommandArgument='<%# Eval("patient_id") %>'
                                                CssClass="inline-flex items-center rounded-lg border border-slate-300 bg-blue-600 px-3 py-1.5 text-white text-xs font-semibold text-slate-700 hover:bg-blue-700" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="patient_id" HeaderText="Patient ID">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="fname" HeaderText="First Name">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="lname" HeaderText="Last Name">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="gender" HeaderText="Gender">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="contact_no" HeaderText="Contact No">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="address" HeaderText="Address">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="date_added" HeaderText="Date Added" DataFormatString="{0:MMM dd, yyyy}">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <!-- Case Table -->
                    <div class="rounded-xl bg-white border border-slate-200 overflow-hidden">
                        <div class="px-6 py-5 border-b border-slate-100">
                            <h4 class="text-lg font-semibold text-slate-800">Case Details</h4>
                            <p class="text-sm text-slate-400 mt-0.5">Recorded bite exposure cases</p>
                        </div>

                        <div class="px-6 py-4 border-b border-slate-100">
                            <div class="flex flex-wrap gap-3 items-center">
                                <asp:TextBox ID="txtSearchCase" runat="server"
                                    CssClass="h-10 w-80 rounded-lg border border-slate-200 px-3 text-sm text-slate-700 placeholder-slate-400"
                                    placeholder="Search by Case ID, Patient ID, Case No, Place, Category" />

                                <asp:TextBox ID="txtCaseDateFrom" runat="server" TextMode="Date"
                                    CssClass="h-10 rounded-lg border border-slate-200 px-3 text-sm text-slate-700" />

                                <span class="text-sm text-slate-400">to</span>

                                <asp:TextBox ID="txtCaseDateTo" runat="server" TextMode="Date"
                                    CssClass="h-10 rounded-lg border border-slate-200 px-3 text-sm text-slate-700" />

                                <asp:Button ID="btnSearchCase" runat="server" Text="Search"
                                    CssClass="h-10 px-5 rounded-lg bg-blue-600 text-white text-sm font-semibold hover:bg-blue-700"
                                    OnClick="btnSearchCase_Click" />

                                <asp:Button ID="btnResetCaseSearch" runat="server" Text="Clear"
                                    CssClass="h-10 px-5 rounded-lg border border-slate-300 bg-white text-slate-600 text-sm font-semibold hover:bg-slate-50"
                                    OnClick="btnResetCaseSearch_Click" />
                            </div>
                        </div>

                        <div class="overflow-x-auto">
                            <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False"
                                CssClass="w-full min-w-[1200px] text-sm text-left text-slate-700"
                                GridLines="None"
                                EmptyDataText="No case records found."
                                DataKeyNames="case_id"
                                OnRowCommand="gvCases_RowCommand"
                                HeaderStyle-CssClass="text-xs font-semibold text-slate-500 uppercase tracking-wide border-b border-slate-200"
                                RowStyle-CssClass="border-b border-slate-100 hover:bg-slate-50"
                                AlternatingRowStyle-CssClass="border-b border-slate-100 hover:bg-slate-50"
                                EmptyDataRowStyle-CssClass="text-center text-slate-400 italic py-8">

                                <Columns>
                                    <asp:TemplateField HeaderText="Action">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                        <ItemTemplate>
                                            <asp:LinkButton ID="btnEditCase" runat="server"
                                                Text="Edit"
                                                CommandName="EditCase"
                                                CommandArgument='<%# Eval("case_id") %>'
                                                CssClass="inline-flex items-center rounded-lg border border-slate-300 bg-blue-600 px-3 py-1.5 text-white text-xs font-semibold text-slate-700 hover:bg-blue-700" />
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="case_id" HeaderText="Case ID">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="patient_id" HeaderText="Patient ID">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="case_no" HeaderText="Case Number">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="date_of_bite" HeaderText="Date of Bite" DataFormatString="{0:MMM dd, yyyy}">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="place_of_bite" HeaderText="Place of Bite">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="type_of_exposure" HeaderText="Type of Exposure">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="site_of_bite" HeaderText="Site of Bite">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                    <asp:BoundField DataField="category" HeaderText="Category">
                                        <HeaderStyle CssClass="px-6 py-3" />
                                        <ItemStyle CssClass="px-6 py-3" />
                                    </asp:BoundField>
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>

                <!-- RIGHT PREVIEW -->
                <asp:Panel ID="pnlRecordPreviewContainer" runat="server" Visible="false" CssClass="min-w-0">
                    <div class="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden lg:sticky lg:top-6 max-h-[85vh] flex flex-col">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex-shrink-0">
                            <h4 class="text-lg font-bold text-slate-900">Record Preview</h4>
                            <p class="text-sm text-slate-500">Edit the selected record here</p>
                        </div>

                        <div class="flex-1 overflow-y-auto p-5">
                            <!-- PATIENT PREVIEW -->
                            <asp:Panel ID="pnlPatientPreview" runat="server" Visible="false" CssClass="space-y-5">
                                <div class="rounded-xl bg-blue-50 border border-blue-100 p-3 mb-4">
                                    <h5 class="text-base font-bold text-blue-900">Patient Information</h5>
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                                    <div class="md:col-span-2">
                                        <label class="block font-semibold text-slate-700 mb-1">Patient ID</label>
                                        <asp:TextBox ID="txtPreviewPatientId" runat="server" ReadOnly="true"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 bg-slate-100 px-3 text-sm text-slate-700" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">First Name</label>
                                        <asp:TextBox ID="txtPreviewFirstName" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Last Name</label>
                                        <asp:TextBox ID="txtPreviewLastName" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Date of Birth</label>
                                        <asp:TextBox ID="txtPreviewDOB" runat="server" TextMode="Date"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Gender</label>
                                        <asp:DropDownList ID="ddlPreviewGender" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select Gender" Value="" />
                                            <asp:ListItem Text="Male" Value="M" />
                                            <asp:ListItem Text="Female" Value="F" />
                                        </asp:DropDownList>
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Civil Status</label>
                                        <asp:DropDownList ID="ddlPreviewCivilStatus" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select Status" Value="" />
                                            <asp:ListItem Text="Single" Value="Single" />
                                            <asp:ListItem Text="Married" Value="Married" />
                                            <asp:ListItem Text="Widowed" Value="Widowed" />
                                            <asp:ListItem Text="Separated" Value="Separated" />
                                        </asp:DropDownList>
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Contact No</label>
                                        <asp:TextBox ID="txtPreviewContactNo" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Occupation</label>
                                        <asp:DropDownList ID="ddlPreviewOccupation" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select Occupation" Value="" />
                                            <asp:ListItem Text="Student" Value="Student" />
                                            <asp:ListItem Text="Employed" Value="Employed" />
                                            <asp:ListItem Text="Self-Employed" Value="Self-Employed" />
                                            <asp:ListItem Text="Unemployed" Value="Unemployed" />
                                            <asp:ListItem Text="Retired" Value="Retired" />
                                        </asp:DropDownList>
                                    </div>

                                    <div class="md:col-span-2">
                                        <label class="block font-semibold text-slate-700 mb-1">Address</label>
                                        <asp:TextBox ID="txtPreviewAddress" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Emergency Contact</label>
                                        <asp:TextBox ID="txtPreviewEmergencyPerson" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Emergency No</label>
                                        <asp:TextBox ID="txtPreviewEmergencyNo" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>
                                </div>

                                <div class="mt-5 rounded-xl border border-slate-200 bg-slate-50 p-4">
                                    <h6 class="text-sm font-bold text-slate-800 mb-3">Vitals</h6>

                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block font-semibold text-slate-700 mb-1">Blood Pressure</label>
                                            <asp:TextBox ID="txtPreviewBP" runat="server"
                                                CssClass="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm"
                                                placeholder="e.g. 120/80" />
                                        </div>

                                        <div>
                                            <label class="block font-semibold text-slate-700 mb-1">Temperature</label>
                                            <asp:TextBox ID="txtPreviewTemp" runat="server"
                                                CssClass="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm"
                                                placeholder="e.g. 36.5" />
                                        </div>

                                        <div>
                                            <label class="block font-semibold text-slate-700 mb-1">Weight</label>
                                            <asp:TextBox ID="txtPreviewWeight" runat="server"
                                                CssClass="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm"
                                                placeholder="e.g. 65" />
                                        </div>

                                        <div>
                                            <label class="block font-semibold text-slate-700 mb-1">Capillary Refill</label>
                                            <asp:TextBox ID="txtPreviewCapillaryRefill" runat="server"
                                                CssClass="h-10 w-full rounded-lg border border-slate-200 bg-white px-3 text-sm"
                                                placeholder="e.g. < 2 seconds" />
                                        </div>
                                    </div>
                                </div>

                                <div class="mt-4">
                                    <label class="block font-semibold text-slate-700 mb-1">Date Added</label>
                                    <asp:TextBox ID="txtPreviewDateAdded" runat="server" ReadOnly="true"
                                        CssClass="h-10 w-full rounded-lg border border-slate-200 bg-slate-100 px-3 text-sm text-slate-700" />
                                </div>

                                <div class="pt-4 border-t border-slate-200 flex gap-3 mt-5 bg-white pb-1">
                                    <asp:Button ID="btnPreviewUpdatePatient" runat="server" Text="Update"
                                        CssClass="px-5 py-2 rounded-lg bg-amber-500 text-white font-semibold hover:bg-amber-600"
                                        OnClick="btnPreviewUpdatePatient_Click"
                                        UseSubmitBehavior="false"
                                        OnClientClick="showConfirmModal('patient'); return false;" />

                                    <asp:Button ID="btnPreviewCancelPatient" runat="server" Text="Cancel"
                                        CssClass="px-5 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                                        OnClick="btnPreviewCancelPatient_Click" />
                                </div>
                            </asp:Panel>

                            <!-- CASE PREVIEW -->
                            <asp:Panel ID="pnlCasePreview" runat="server" Visible="false" CssClass="space-y-5">
                                <div class="rounded-xl bg-emerald-50 border border-emerald-100 p-3 mb-4">
                                    <h5 class="text-base font-bold text-emerald-900">Case Information</h5>
                                </div>

                                <div class="grid grid-cols-1 gap-4 mb-4">
                                    <!-- Biting Animal -->
                                    <div class="rounded-xl border border-slate-200 p-4">
                                        <div class="mb-3 text-sm font-bold text-slate-900">
                                            Biting Animal <span class="text-red-500">*</span>
                                        </div>

                                        <div class="space-y-3 text-sm font-semibold text-slate-700">
                                            <div class="flex flex-wrap items-center gap-4">
                                                <asp:RadioButton ID="rbDog" runat="server" GroupName="AnimalType" Text="Dog" Checked="true" />
                                                <asp:RadioButton ID="rbCat" runat="server" GroupName="AnimalType" Text="Cat" />
                                            </div>

                                            <div class="flex items-center gap-3">
                                                <asp:RadioButton ID="rbOtherAnimal" runat="server" GroupName="AnimalType" Text="Others:" />
                                                <asp:TextBox ID="txtOtherAnimal" runat="server"
                                                    CssClass="h-10 flex-1 min-w-0 rounded-lg border border-slate-200 px-3 text-sm"
                                                    placeholder="Specify" />
                                            </div>
                                        </div>
                                    </div>

                                    <!-- Circumstance -->
                                    <div class="rounded-xl border border-slate-200 p-4">
                                        <div class="mb-2 text-sm font-bold text-slate-900">
                                            Circumstance <span class="text-red-500">*</span>
                                        </div>

                                        <div class="flex flex-wrap gap-4 text-sm font-semibold text-slate-700">
                                            <asp:RadioButton ID="rbProvoked" runat="server" GroupName="Circumstance" Text="Provoked" />
                                            <asp:RadioButton ID="rbUnprovoked" runat="server" GroupName="Circumstance" Text="Unprovoked" Checked="true" />
                                        </div>
                                    </div>
                                </div>

                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3 text-sm">
                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Case ID</label>
                                        <asp:TextBox ID="txtPreviewCaseId" runat="server" ReadOnly="true"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 bg-slate-100 px-3 text-sm text-slate-700" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Patient ID</label>
                                        <asp:TextBox ID="txtPreviewCasePatientId" runat="server" ReadOnly="true"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 bg-slate-100 px-3 text-sm text-slate-700" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Case No</label>
                                        <asp:TextBox ID="txtPreviewCaseNo" runat="server" ReadOnly="true"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 bg-slate-100 px-3 text-sm text-slate-700" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Date of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseDateOfBite" runat="server" TextMode="Date"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Time of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseTimeOfBite" runat="server" TextMode="Time"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Exposure Type</label>
                                        <asp:DropDownList ID="ddlPreviewCaseExposureType" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select Type" Value="" />
                                            <asp:ListItem Text="Bite" Value="Bite" />
                                            <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                                        </asp:DropDownList>
                                    </div>

                                    <div class="md:col-span-2">
                                        <label class="block font-semibold text-slate-700 mb-1">Place of Bite</label>
                                        <asp:TextBox ID="txtPreviewCasePlaceOfBite" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Wound Type</label>
                                        <asp:DropDownList ID="ddlPreviewCaseWoundType" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select Type" Value="" />
                                            <asp:ListItem Text="Lacerated" Value="Lacerated" />
                                            <asp:ListItem Text="Avulsion" Value="Avulsion" />
                                            <asp:ListItem Text="Punctured" Value="Punctured" />
                                            <asp:ListItem Text="Abrasion" Value="Abrasion" />
                                            <asp:ListItem Text="Scratches" Value="Scratches" />
                                            <asp:ListItem Text="Hematoma" Value="Hematoma" />
                                        </asp:DropDownList>
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Bleeding</label>
                                        <asp:DropDownList ID="ddlPreviewCaseBleeding" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select" Value="" />
                                            <asp:ListItem Text="No" Value="No" />
                                            <asp:ListItem Text="Yes" Value="Yes" />
                                        </asp:DropDownList>
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Site of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseSiteOfBite" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" />
                                    </div>

                                    <div>
                                        <label class="block font-semibold text-slate-700 mb-1">Category</label>
                                        <asp:DropDownList ID="ddlPreviewCaseCategory" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select Category" Value="" />
                                            <asp:ListItem Text="I" Value="I" />
                                            <asp:ListItem Text="II" Value="II" />
                                            <asp:ListItem Text="III" Value="III" />
                                        </asp:DropDownList>
                                    </div>

                                    <div class="md:col-span-2">
                                        <label class="block font-semibold text-slate-700 mb-1">Washed</label>
                                        <asp:DropDownList ID="ddlPreviewCaseWashed" runat="server"
                                            CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm">
                                            <asp:ListItem Text="Select" Value="" />
                                            <asp:ListItem Text="Yes" Value="Yes" />
                                            <asp:ListItem Text="No" Value="No" />
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="pt-4 border-t border-slate-200 flex gap-3 mt-5 bg-white">
                                    <asp:Button ID="btnPreviewUpdateCase" runat="server" Text="Update"
                                        CssClass="px-5 py-2 rounded-lg bg-amber-500 text-white font-semibold hover:bg-amber-600"
                                        OnClick="btnPreviewUpdateCase_Click"
                                        UseSubmitBehavior="false"
                                        OnClientClick="showConfirmModal('case'); return false;" />

                                    <asp:Button ID="btnPreviewCancelCase" runat="server" Text="Cancel"
                                        CssClass="px-5 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                                        OnClick="btnPreviewCancelCase_Click" />
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </asp:Panel>

            </div>
        </div>

    </div>

    <style>
        @media (min-width: 1280px) {
            #viewLayout.with-preview {
                grid-template-columns: minmax(0, 1.8fr) minmax(360px, 440px);
                align-items: start;
            }

            #viewLayout.no-preview {
                grid-template-columns: minmax(0, 1fr);
            }
        }

        @media (max-width: 1279px) {
            #viewLayout.with-preview,
            #viewLayout.no-preview {
                grid-template-columns: 1fr;
            }
        }

        @keyframes fadeIn {
            from {
                opacity: 0;
                transform: translateY(8px) scale(.98);
            }
            to {
                opacity: 1;
                transform: translateY(0) scale(1);
            }
        }
    </style>

    <!-- CONFIRM MODAL -->
    <div id="confirmModal"
        class="fixed inset-0 z-[100] hidden items-center justify-center bg-slate-900/50 px-4">

        <div class="w-full max-w-md rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden"
            style="animation: fadeIn .2s ease-out;">
            <div class="px-6 py-5 border-b border-slate-200">
                <h3 class="text-lg font-bold text-slate-900">Confirm Update</h3>
                <p id="confirmModalMessage" class="mt-1 text-sm text-slate-600">
                    Are you sure you want to continue?
                </p>
            </div>

            <div class="px-6 py-4 bg-slate-50 flex justify-end gap-3">
                <button type="button"
                    class="rounded-lg border border-slate-300 bg-white px-5 py-2 font-semibold text-slate-700 hover:bg-slate-100"
                    onclick="hideConfirmModal()">
                    Cancel
                </button>

                <button type="button"
                    class="rounded-lg bg-amber-500 px-5 py-2 font-semibold text-white hover:bg-amber-600"
                    onclick="confirmModalAction()">
                    Confirm
                </button>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        var pendingUpdateType = '';

        function setActivePanelButton(panelId) {
            var btnView = document.getElementById('btnViewPanel');
            var btnAdd = document.getElementById('btnAddPanel');

            if (btnView) {
                btnView.classList.remove('bg-blue-600', 'text-white', 'border-blue-600');
                btnView.classList.add('bg-white', 'text-slate-700', 'border-slate-300');
            }

            if (btnAdd) {
                btnAdd.classList.remove('bg-blue-600', 'text-white', 'border-blue-600');
                btnAdd.classList.add('bg-white', 'text-slate-700', 'border-slate-300');
            }

            if (panelId === 'viewPatientPanel' && btnView) {
                btnView.classList.remove('bg-white', 'text-slate-700', 'border-slate-300');
                btnView.classList.add('bg-blue-600', 'text-white', 'border-blue-600');
            }

            if (panelId === 'addPatientPanel' && btnAdd) {
                btnAdd.classList.remove('bg-white', 'text-slate-700', 'border-slate-300');
                btnAdd.classList.add('bg-blue-600', 'text-white', 'border-blue-600');
            }
        }

        function updateViewLayout() {
            var viewLayout = document.getElementById('viewLayout');
            var preview = document.getElementById('<%= pnlRecordPreviewContainer.ClientID %>');

            if (!viewLayout || !preview) return;

            var previewVisible =
                preview.style.display !== 'none' &&
                !preview.hasAttribute('hidden') &&
                preview.offsetParent !== null;

            viewLayout.classList.remove('with-preview', 'no-preview');
            viewLayout.classList.add(previewVisible ? 'with-preview' : 'no-preview');
        }

        function showPanel(panelId) {
            document.querySelectorAll('.panel').forEach(function (p) {
                p.classList.add('hidden');
            });

            var target = document.getElementById(panelId);
            if (target) {
                target.classList.remove('hidden');
            }

            setActivePanelButton(panelId);

            var hiddenField = document.getElementById('<%= hfActivePanel.ClientID %>');
            if (hiddenField) {
                hiddenField.value = panelId;
            }

            setTimeout(updateViewLayout, 50);
        }

        function toggleVitals() {
            var vitals = document.getElementById('optionalVitals');
            if (vitals) {
                vitals.classList.toggle('hidden');
            }
        }

        function showConfirmModal(type) {
            pendingUpdateType = type || '';

            var modal = document.getElementById('confirmModal');
            var message = document.getElementById('confirmModalMessage');

            if (message) {
                if (type === 'patient') {
                    message.textContent = 'Are you sure you want to update this patient record?';
                } else if (type === 'case') {
                    message.textContent = 'Are you sure you want to update this case record?';
                } else {
                    message.textContent = 'Are you sure you want to continue?';
                }
            }

            if (modal) {
                modal.classList.remove('hidden');
                modal.classList.add('flex');
            }

            document.body.classList.add('overflow-hidden');
        }

        function hideConfirmModal() {
            var modal = document.getElementById('confirmModal');

            if (modal) {
                modal.classList.add('hidden');
                modal.classList.remove('flex');
            }

            document.body.classList.remove('overflow-hidden');
        }

        function confirmModalAction() {
            var actionType = pendingUpdateType;
            hideConfirmModal();

            if (actionType === 'patient') {
                __doPostBack('<%= btnPreviewUpdatePatient.UniqueID %>', '');
            } else if (actionType === 'case') {
                __doPostBack('<%= btnPreviewUpdateCase.UniqueID %>', '');
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            var hiddenField = document.getElementById('<%= hfActivePanel.ClientID %>');
            var activePanel = hiddenField && hiddenField.value ? hiddenField.value : 'viewPatientPanel';
            showPanel(activePanel);
            setTimeout(updateViewLayout, 100);
        });

        document.addEventListener('keydown', function (e) {
            var modal = document.getElementById('confirmModal');
            if (!modal || modal.classList.contains('hidden')) return;

            if (e.key === 'Escape') {
                hideConfirmModal();
            }
        });

        document.addEventListener('click', function (e) {
            var modal = document.getElementById('confirmModal');
            if (!modal || modal.classList.contains('hidden')) return;

            if (e.target === modal) {
                hideConfirmModal();
            }
        });

        window.addEventListener('resize', updateViewLayout);
    </script>

</asp:Content>
