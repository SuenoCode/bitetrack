<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="PatientRegistration.aspx.cs" Inherits="SBI.PatientRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActivePanel" runat="server" Value="addPatientPanel" />
    <asp:HiddenField ID="hfSelectedPatientId" runat="server" Value="" />
    <asp:HiddenField ID="hfSelectedCaseId" runat="server" Value="" />
    <asp:HiddenField ID="hfEditMode" runat="server" Value="" />

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- PANEL TOGGLE BUTTONS -->
        <div class="mb-6 rounded-2xl border border-slate-200 bg-white shadow-sm p-3">
            <div class="flex flex-wrap gap-3">
                <button type="button" id="btnViewPanel"
                    class="panel-tab rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50"
                    onclick="showPanel('viewPatientPanel')">
                    View Patient / Case Details
                </button>

                <button type="button" id="btnAddPanel"
                    class="panel-tab rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50"
                    onclick="showPanel('addPatientPanel')">
                    Add New Patient / Case
                </button>
            </div>
        </div>

        <!-- ====================== ADD PATIENT / CASE PANEL ====================== -->
        <div id="addPatientPanel" class="panel">

            <div class="mb-5">
                <h2 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Patient Registration</h2>
                <p class="mt-1 text-base text-slate-600">
                    Register patient details and document biting incident history
                </p>
            </div>

            <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

                <!-- A. Patient Information -->
                <div class="px-5 py-5 space-y-4">
                    <h3 class="text-lg font-extrabold text-slate-900 mb-2">A. Patient Information</h3>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">First Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtFirstName" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                placeholder="e.g. Maria" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Last Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtLastName" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                placeholder="e.g. Santos" />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Date of Birth <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtDOB" runat="server" TextMode="Date"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200" />
                        </div>
                       
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Gender <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlGender" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
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
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
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
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
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
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
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
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                placeholder="Full name" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Emergency Contact Number <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtEmergencyContactNo" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                placeholder="Contact number" />
                        </div>
                    </div>

                    <div class="pt-2 border-t border-slate-200">
                        <button type="button" onclick="toggleVitals()"
                            class="text-sm font-semibold text-blue-600 hover:text-blue-800">
                            + Optional Vitals & Visit Information
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
                            <asp:TextBox ID="txtCapillaryRefill" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="e.g. &lt; 2 seconds" />
                        </div>
                    </div>
                </div>

                <!-- B. History of Biting Incident -->
                <div class="px-5 py-5 border-t border-slate-200">
                    <h3 class="text-lg font-extrabold text-slate-900 mb-4">B. History of Biting Incident</h3>

                    <div class="space-y-4">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Date and Time of Bite <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtBiteDateTime" runat="server" TextMode="DateTimeLocal"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200" />
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Place of Exposure <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtPlaceExposure" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                    placeholder="e.g. Home, Street, etc." />
                            </div>
                        </div>

                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div class="rounded-xl border border-slate-200 p-4">
                                <div class="mb-2 text-sm font-bold text-slate-900">Biting Animal <span class="text-red-500">*</span></div>
                                <div class="flex flex-wrap items-center gap-4 text-sm font-semibold text-slate-700">
                                    <asp:RadioButton ID="rbDog" runat="server" GroupName="AnimalType" Text="Dog" Checked="true" />
                                    <asp:RadioButton ID="rbCat" runat="server" GroupName="AnimalType" Text="Cat" />
                                    <div class="flex items-center gap-2">
                                        <asp:RadioButton ID="rbOtherAnimal" runat="server" GroupName="AnimalType" Text="Others:" />
                                        <asp:TextBox ID="txtOtherAnimal" runat="server"
                                            CssClass="h-9 w-40 rounded-lg border border-slate-200 px-2 text-sm" placeholder="Specify" />
                                    </div>
                                </div>
                            </div>

                            <div class="rounded-xl border border-slate-200 p-4">
                                <div class="mb-2 text-sm font-bold text-slate-900">Circumstance <span class="text-red-500">*</span></div>
                                <div class="flex flex-wrap gap-4 text-sm font-semibold text-slate-700">
                                    <asp:RadioButton ID="rbProvoked" runat="server" GroupName="Circumstance" Text="Provoked" />
                                    <asp:RadioButton ID="rbUnprovoked" runat="server" GroupName="Circumstance" Text="Unprovoked" Checked="true" />
                                </div>
                            </div>
                        </div>

                        <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div class="rounded-xl border border-slate-200 p-4">
                                <div class="mb-2 text-sm font-bold text-slate-900">Ownership <span class="text-red-500">*</span></div>
                                <div class="flex flex-wrap gap-4 text-sm font-semibold text-slate-700">
                                    <asp:RadioButton ID="rbOwned" runat="server" GroupName="Ownership" Text="Owned" />
                                    <asp:RadioButton ID="rbStray" runat="server" GroupName="Ownership" Text="Stray" Checked="true" />
                                    <asp:RadioButton ID="rbLeashed" runat="server" GroupName="Ownership" Text="Leashed/Cage" />
                                </div>
                            </div>

                            <div class="rounded-xl border border-slate-200 p-4">
                                <div class="mb-2 text-sm font-bold text-slate-900">Animal Status <span class="text-red-500">*</span></div>
                                <div class="flex flex-wrap gap-4 text-sm font-semibold text-slate-700">
                                    <asp:RadioButton ID="rbAlive" runat="server" GroupName="AnimalStatus" Text="Alive" Checked="true" />
                                    <asp:RadioButton ID="rbSick" runat="server" GroupName="AnimalStatus" Text="Sick" />
                                    <asp:RadioButton ID="rbDied" runat="server" GroupName="AnimalStatus" Text="Died" />
                                    <asp:RadioButton ID="rbUnknown" runat="server" GroupName="AnimalStatus" Text="Unknown" />
                                </div>
                            </div>
                        </div>

                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Type of Exposure</label>
                                <asp:DropDownList ID="ddlExposureType" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
                                    <asp:ListItem Text="Select Type" Value="" Selected="True" />
                                    <asp:ListItem Text="Bite" Value="Bite" />
                                    <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Wound Location</label>
                                <asp:TextBox ID="txtWoundLocation" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                    placeholder="e.g. Left arm, Right leg" />
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Wound Type</label>
                                <asp:DropDownList ID="ddlWoundType" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
                                    <asp:ListItem Text="Select Type" Value="" Selected="True" />
                                    <asp:ListItem Text="Lacerated" Value="Lacerated" />
                                    <asp:ListItem Text="Avulsion" Value="Avulsion" />
                                    <asp:ListItem Text="Punctured" Value="Punctured" />
                                    <asp:ListItem Text="Abrasion" Value="Abrasion" />
                                    <asp:ListItem Text="Scratches" Value="Scratches" />
                                    <asp:ListItem Text="Hematoma" Value="Hematoma" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-sm font-semibold text-slate-700 mb-1">Bleeding</label>
                                <asp:DropDownList ID="ddlBleeding" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
                                    <asp:ListItem Text="Select" Value="" Selected="True" />
                                    <asp:ListItem Text="No" Value="No" />
                                    <asp:ListItem Text="Yes" Value="Yes" />
                                </asp:DropDownList>
                            </div>
                        </div>

                        <div class="rounded-xl border border-slate-200 p-4">
                            <div class="mb-2 text-sm font-bold text-slate-900">Category</div>
                            <div class="flex flex-wrap gap-4 text-sm font-semibold text-slate-700">
                                <asp:RadioButton ID="rbCategory1" runat="server" GroupName="Category" Text="Category I" />
                                <asp:RadioButton ID="rbCategory2" runat="server" GroupName="Category" Text="Category II" />
                                <asp:RadioButton ID="rbCategory3" runat="server" GroupName="Category" Text="Category III" />
                            </div>
                        </div>
                    </div>
                </div>

                <div class="px-5 py-4 bg-slate-50 border-t border-slate-200 flex justify-end gap-3">
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

        <!-- ====================== VIEW PATIENT DETAILS PANEL ====================== -->
        <div id="viewPatientPanel" class="panel hidden space-y-6">
            <div class="grid grid-cols-1 xl:grid-cols-3 gap-6">

                <!-- LEFT -->
                <div class="xl:col-span-2 space-y-6">

                    <!-- Patient Table -->
                    <div class="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
                        <div class="flex flex-col gap-3 px-5 py-4 border-b border-slate-200 bg-slate-50">
                            <div>
                                <h4 class="text-lg font-bold text-slate-900">Patient Details</h4>
                                <p class="text-sm text-slate-500">List of registered patients</p>
                            </div>

                            <div class="flex flex-col md:flex-row gap-3">
                                <asp:TextBox ID="txtSearchPatient" runat="server"
                                    CssClass="h-10 w-full md:w-80 rounded-lg border border-slate-200 px-3 text-sm"
                                    placeholder="Search by Patient ID, Name, Contact, Address" />
                                <asp:Button ID="btnSearchPatient" runat="server" Text="Search"
                                    CssClass="px-4 py-2 rounded-lg bg-blue-600 text-white font-semibold hover:bg-blue-700"
                                    OnClick="btnSearchPatient_Click" />
                                <asp:Button ID="btnResetPatientSearch" runat="server" Text="Reset"
                                    CssClass="px-4 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                                    OnClick="btnResetPatientSearch_Click" />
                            </div>
                        </div>

                        <div class="overflow-x-auto">
                            <asp:GridView ID="gvPatients" runat="server" AutoGenerateColumns="False"
                                CssClass="w-full min-w-[950px] text-sm text-left text-slate-700"
                                GridLines="None"
                                EmptyDataText="No patient records found."
                                DataKeyNames="patient_id"
                                OnRowCommand="gvPatients_RowCommand"
                                HeaderStyle-CssClass="bg-slate-100 text-slate-800"
                                RowStyle-CssClass="border-b border-slate-200 hover:bg-slate-50"
                                AlternatingRowStyle-CssClass="bg-slate-50/50"
                                EmptyDataRowStyle-CssClass="text-center text-slate-500 italic py-6">

                                <Columns>
                                    <asp:TemplateField HeaderText="Action">
                                        <HeaderStyle CssClass="px-4 py-3 text-xs font-bold uppercase tracking-wider" />
                                        <ItemStyle CssClass="px-4 py-3" />
                                        <ItemTemplate>
                                            <div class="flex gap-2">
                                                <asp:LinkButton ID="btnViewPatient" runat="server"
                                                    Text="View"
                                                    CommandName="ViewPatient"
                                                    CommandArgument='<%# Eval("patient_id") %>'
                                                    CssClass="inline-flex items-center rounded-lg bg-blue-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-blue-700" />

                                                <asp:LinkButton ID="btnEditPatient" runat="server"
                                                    Text="Edit"
                                                    CommandName="EditPatient"
                                                    CommandArgument='<%# Eval("patient_id") %>'
                                                    CssClass="inline-flex items-center rounded-lg bg-amber-500 px-3 py-1.5 text-xs font-semibold text-white hover:bg-amber-600" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="patient_id" HeaderText="Patient ID" />
                                    <asp:BoundField DataField="fname" HeaderText="First Name" />
                                    <asp:BoundField DataField="lname" HeaderText="Last Name" />
                                    <asp:BoundField DataField="gender" HeaderText="Gender" />
                                    <asp:BoundField DataField="contact_no" HeaderText="Contact No" />
                                    <asp:BoundField DataField="address" HeaderText="Address" />
                                    <asp:BoundField DataField="date_added" HeaderText="Date Added" DataFormatString="{0:MMM dd, yyyy}" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>

                    <!-- Case Table -->
                    <div class="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
                        <div class="flex flex-col gap-3 px-5 py-4 border-b border-slate-200 bg-slate-50">
                            <div>
                                <h4 class="text-lg font-bold text-slate-900">Case Details</h4>
                                <p class="text-sm text-slate-500">Recorded bite exposure cases</p>
                            </div>

                            <div class="flex flex-col md:flex-row gap-3">
                                <asp:TextBox ID="txtSearchCase" runat="server"
                                    CssClass="h-10 w-full md:w-80 rounded-lg border border-slate-200 px-3 text-sm"
                                    placeholder="Search by Case ID, Patient ID, Case No, Place, Category" />
                                <asp:Button ID="btnSearchCase" runat="server" Text="Search"
                                    CssClass="px-4 py-2 rounded-lg bg-emerald-600 text-white font-semibold hover:bg-emerald-700"
                                    OnClick="btnSearchCase_Click" />
                                <asp:Button ID="btnResetCaseSearch" runat="server" Text="Reset"
                                    CssClass="px-4 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                                    OnClick="btnResetCaseSearch_Click" />
                            </div>
                        </div>

                        <div class="overflow-x-auto">
                            <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False"
                                CssClass="w-full min-w-[1100px] text-sm text-left text-slate-700"
                                GridLines="None"
                                EmptyDataText="No case records found."
                                DataKeyNames="case_id"
                                OnRowCommand="gvCases_RowCommand"
                                HeaderStyle-CssClass="bg-slate-100 text-slate-800"
                                RowStyle-CssClass="border-b border-slate-200 hover:bg-slate-50"
                                AlternatingRowStyle-CssClass="bg-slate-50/50"
                                EmptyDataRowStyle-CssClass="text-center text-slate-500 italic py-6">

                                <Columns>
                                    <asp:TemplateField HeaderText="Action">
                                        <HeaderStyle CssClass="px-4 py-3 text-xs font-bold uppercase tracking-wider" />
                                        <ItemStyle CssClass="px-4 py-3" />
                                        <ItemTemplate>
                                            <div class="flex gap-2">
                                                <asp:LinkButton ID="btnViewCase" runat="server"
                                                    Text="View"
                                                    CommandName="ViewCase"
                                                    CommandArgument='<%# Eval("case_id") %>'
                                                    CssClass="inline-flex items-center rounded-lg bg-emerald-600 px-3 py-1.5 text-xs font-semibold text-white hover:bg-emerald-700" />

                                                <asp:LinkButton ID="btnEditCase" runat="server"
                                                    Text="Edit"
                                                    CommandName="EditCase"
                                                    CommandArgument='<%# Eval("case_id") %>'
                                                    CssClass="inline-flex items-center rounded-lg bg-amber-500 px-3 py-1.5 text-xs font-semibold text-white hover:bg-amber-600" />
                                            </div>
                                        </ItemTemplate>
                                    </asp:TemplateField>

                                    <asp:BoundField DataField="case_id" HeaderText="Case ID" />
                                    <asp:BoundField DataField="patient_id" HeaderText="Patient ID" />
                                    <asp:BoundField DataField="case_no" HeaderText="Case Number" />
                                    <asp:BoundField DataField="date_of_bite" HeaderText="Date of Bite" DataFormatString="{0:MMM dd, yyyy}" />
                                    <asp:BoundField DataField="place_of_bite" HeaderText="Place of Bite" />
                                    <asp:BoundField DataField="type_of_exposure" HeaderText="Type of Exposure" />
                                    <asp:BoundField DataField="site_of_bite" HeaderText="Site of Bite" />
                                    <asp:BoundField DataField="category" HeaderText="Category" />
                                </Columns>
                            </asp:GridView>
                        </div>
                    </div>
                </div>

                <!-- RIGHT PREVIEW -->
                <div class="xl:col-span-1">
                    <div class="rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden sticky top-6">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                            <h4 class="text-lg font-bold text-slate-900">Record Preview</h4>
                            <p class="text-sm text-slate-500">Click View to display full details</p>
                        </div>

                        <div class="p-5 space-y-5">
                            <asp:Panel ID="pnlPreviewEmpty" runat="server">
                                <div class="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-6 text-center text-sm text-slate-500">
                                    No record selected yet.
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlPatientPreview" runat="server" Visible="false">
                                <div class="rounded-xl bg-blue-50 border border-blue-100 p-4 mb-4">
                                    <h5 class="text-base font-bold text-blue-900">Patient Information</h5>
                                </div>

                                <div class="grid grid-cols-1 gap-3 text-sm">
                                    <div><span class="font-semibold text-slate-700">Patient ID:</span> <asp:Label ID="lblPatientId" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Name:</span> <asp:Label ID="lblPatientName" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Date of Birth:</span> <asp:Label ID="lblPatientDOB" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Gender:</span> <asp:Label ID="lblPatientGender" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Civil Status:</span> <asp:Label ID="lblPatientCivilStatus" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Address:</span> <asp:Label ID="lblPatientAddress" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Contact No:</span> <asp:Label ID="lblPatientContact" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Occupation:</span> <asp:Label ID="lblPatientOccupation" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Emergency Contact:</span> <asp:Label ID="lblPatientEmergencyPerson" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Emergency No:</span> <asp:Label ID="lblPatientEmergencyNo" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Blood Pressure:</span> <asp:Label ID="lblPatientBP" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Temperature:</span> <asp:Label ID="lblPatientTemp" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Weight:</span> <asp:Label ID="lblPatientWeight" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Capillary Refill:</span> <asp:Label ID="lblPatientCapillaryRefill" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Date Added:</span> <asp:Label ID="lblPatientDateAdded" runat="server" /></div>
                                </div>

                                <div class="pt-4 border-t border-slate-200 flex gap-3 mt-4">
                                    <asp:Button ID="btnPreviewEditPatient" runat="server" Text="Edit Patient"
                                        CssClass="px-4 py-2 rounded-lg bg-amber-500 text-white font-semibold hover:bg-amber-600"
                                        OnClick="btnPreviewEditPatient_Click" />

                                    <asp:Button ID="btnCancelPatientPreview" runat="server" Text="Cancel"
                                        CssClass="px-4 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                                        OnClick="btnCancelPatientPreview_Click" />
                                </div>
                            </asp:Panel>

                            <asp:Panel ID="pnlCasePreview" runat="server" Visible="false">
                                <div class="rounded-xl bg-emerald-50 border border-emerald-100 p-4 mb-4">
                                    <h5 class="text-base font-bold text-emerald-900">Case Information</h5>
                                </div>

                                <div class="grid grid-cols-1 gap-3 text-sm">
                                    <div><span class="font-semibold text-slate-700">Case ID:</span> <asp:Label ID="lblCaseId" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Patient ID:</span> <asp:Label ID="lblCasePatientId" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Case No:</span> <asp:Label ID="lblCaseNo" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Date of Bite:</span> <asp:Label ID="lblCaseDateOfBite" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Time of Bite:</span> <asp:Label ID="lblCaseTimeOfBite" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Place of Bite:</span> <asp:Label ID="lblCasePlaceOfBite" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Exposure Type:</span> <asp:Label ID="lblCaseExposureType" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Wound Type:</span> <asp:Label ID="lblCaseWoundType" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Bleeding:</span> <asp:Label ID="lblCaseBleeding" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Site of Bite:</span> <asp:Label ID="lblCaseSiteOfBite" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Category:</span> <asp:Label ID="lblCaseCategory" runat="server" /></div>
                                    <div><span class="font-semibold text-slate-700">Washed:</span> <asp:Label ID="lblCaseWashed" runat="server" /></div>
                                </div>

                                <div class="pt-4 border-t border-slate-200 flex gap-3 mt-4">
                                    <asp:Button ID="btnPreviewEditCase" runat="server" Text="Edit Case"
                                        CssClass="px-4 py-2 rounded-lg bg-amber-500 text-white font-semibold hover:bg-amber-600"
                                        OnClick="btnPreviewEditCase_Click" />

                                    <asp:Button ID="btnCancelCasePreview" runat="server" Text="Cancel"
                                        CssClass="px-4 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                                        OnClick="btnCancelCasePreview_Click" />
                                </div>
                            </asp:Panel>
                        </div>
                    </div>
                </div>

            </div>
        </div>

    </div>

    <script type="text/javascript">
        function setActivePanelButton(panelId) {
            var btnView = document.getElementById('btnViewPanel');
            var btnAdd = document.getElementById('btnAddPanel');

            if (btnView) {
                btnView.classList.remove('bg-blue-600', 'text-white', 'border-blue-600');
                btnView.classList.add('bg-white', 'text-slate-800', 'border-slate-300');
            }

            if (btnAdd) {
                btnAdd.classList.remove('bg-blue-600', 'text-white', 'border-blue-600');
                btnAdd.classList.add('bg-white', 'text-slate-800', 'border-slate-300');
            }

            if (panelId === 'viewPatientPanel' && btnView) {
                btnView.classList.remove('bg-white', 'text-slate-800', 'border-slate-300');
                btnView.classList.add('bg-blue-600', 'text-white', 'border-blue-600');
            }

            if (panelId === 'addPatientPanel' && btnAdd) {
                btnAdd.classList.remove('bg-white', 'text-slate-800', 'border-slate-300');
                btnAdd.classList.add('bg-blue-600', 'text-white', 'border-blue-600');
            }
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
        }

        function toggleVitals() {
            var vitals = document.getElementById('optionalVitals');
            if (vitals) {
                vitals.classList.toggle('hidden');
            }
        }

        document.addEventListener('DOMContentLoaded', function () {
            var hiddenField = document.getElementById('<%= hfActivePanel.ClientID %>');
            var activePanel = hiddenField && hiddenField.value ? hiddenField.value : 'addPatientPanel';
            showPanel(activePanel);
        });
    </script>

</asp:Content>