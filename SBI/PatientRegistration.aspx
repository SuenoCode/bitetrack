<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="PatientRegistration.aspx.cs" Inherits="SBI.PatientRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActivePanel" runat="server" Value="viewPatientPanel" />
    <asp:HiddenField ID="hfSelectedPatientId" runat="server" Value="" />
    <asp:HiddenField ID="hfSelectedCaseId" runat="server" Value="" />
    <asp:HiddenField ID="hfEditMode" runat="server" Value="" />

    <div class="p-6 font-heading2 text-slate-900">

        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl font-bold text-[#0b2a7a] font-hBruns">Patient Registration</h1>
                <p class="text-slate-500 text-sm mt-1">Register patients, manage bite cases, and update records.</p>
            </div>
        </div>

        <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
            <button type="button" id="btnViewPanel"
                class="panel-tab h-11 rounded-lg px-6 font-bold text-sm transition cursor-pointer border"
                onclick="showPanel('viewPatientPanel')">
                View Patient / Case Details
            </button>
            <button type="button" id="btnAddPanel"
                class="panel-tab h-11 rounded-lg px-6 font-bold text-sm transition cursor-pointer border"
                onclick="showPanel('addPatientPanel')">
                Add New Patient / Case
            </button>
        </div>

        <%-- ADD PANEL --%>
        <div id="addPatientPanel" class="panel hidden space-y-6">

            <%-- Section A --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">A. Patient Information</h3>
                </div>
                <div class="p-5 space-y-5">

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">First Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtFirstName" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. Maria" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Last Name <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtLastName" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. Santos" />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date of Birth <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtDOB" runat="server" TextMode="Date" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Gender <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlGender" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Gender" Value="" Selected="True" />
                                <asp:ListItem Text="Male" Value="M" />
                                <asp:ListItem Text="Female" Value="F" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Civil Status</label>
                            <asp:DropDownList ID="ddlCivilStatus" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Status" Value="" Selected="True" />
                                <asp:ListItem Text="Single" Value="Single" />
                                <asp:ListItem Text="Married" Value="Married" />
                                <asp:ListItem Text="Widowed" Value="Widowed" />
                                <asp:ListItem Text="Separated" Value="Separated" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact No <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtContactNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. 09123456789" />
                        </div>
                    </div>

                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Address <span class="text-red-500">*</span></label>
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <asp:TextBox ID="txtHouseNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="House No." />
                            <asp:TextBox ID="txtSubdivision" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="Subdivision / Street" />
                            <asp:TextBox ID="txtBarangay" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="Barangay *" />
                            <asp:TextBox ID="txtProvinceCity" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="City / Province *" />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Occupation <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlOccupation" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Occupation" Value="" Selected="True" />
                                <asp:ListItem Text="Student" Value="Student" />
                                <asp:ListItem Text="Employed" Value="Employed" />
                                <asp:ListItem Text="Self-Employed" Value="Self-Employed" />
                                <asp:ListItem Text="Unemployed" Value="Unemployed" />
                                <asp:ListItem Text="Retired" Value="Retired" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Emergency Contact Person <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtEmergencyContactPerson" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="Full name" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Emergency Contact Number <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtEmergencyContactNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. 09123456789" />
                        </div>
                    </div>

                    <div class="pt-2 border-t border-slate-200">
                        <button type="button" onclick="toggleVitals()" class="text-sm font-bold text-blue-600 hover:text-blue-800 transition">
                            + Optional Vitals &amp; Visit Information
                        </button>
                    </div>

                    <div id="optionalVitals" class="hidden grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 pt-3">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Blood Pressure</label>
                            <asp:TextBox ID="txtBloodPressure" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. 120/80" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Temperature (°C)</label>
                            <asp:TextBox ID="txtTemperature" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. 36.5" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Weight (kg)</label>
                            <asp:TextBox ID="txtWeight" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. 65" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Capillary Refill</label>
                            <asp:TextBox ID="txtCapillaryRefill" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. &lt; 2 seconds" />
                        </div>
                    </div>

                </div>
            </div>

            <%-- Section B --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">B. History of Biting Incident</h3>
                </div>
                <div class="p-5 space-y-5">

                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date and Time of Bite <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtBiteDateTime" runat="server" TextMode="DateTimeLocal" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                        </div>
                    </div>

                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Place of Bite <span class="text-red-500">*</span></label>
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <asp:TextBox ID="txtPlaceHouseNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="House No." />
                            <asp:TextBox ID="txtPlaceStreet" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="Street" />
                            <asp:TextBox ID="txtPlaceBarangay" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="Barangay *" />
                            <asp:TextBox ID="txtPlaceCity" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="City / Province *" />
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Biting Animal <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlBitingAnimal" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Animal" Value="" Selected="True" />
                                <asp:ListItem Text="Dog" Value="Dog" />
                                <asp:ListItem Text="Cat" Value="Cat" />
                                <asp:ListItem Text="Others" Value="Others" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Ownership</label>
                            <asp:DropDownList ID="ddlOwnership" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Ownership" Value="" Selected="True" />
                                <asp:ListItem Text="Owned" Value="Owned" />
                                <asp:ListItem Text="Stray" Value="Stray" />
                                <asp:ListItem Text="Leashed / Cage" Value="Leashed/Cage" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Circumstance</label>
                            <asp:DropDownList ID="ddlCircumstance" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Circumstance" Value="" Selected="True" />
                                <asp:ListItem Text="Provoked / Intentional" Value="Provoked" />
                                <asp:ListItem Text="Unprovoked / Unintentional" Value="Unprovoked" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Status of Biting Animal</label>
                            <asp:DropDownList ID="ddlAnimalStatus" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Status" Value="" Selected="True" />
                                <asp:ListItem Text="Alive / Healthy" Value="Alive/Healthy" />
                                <asp:ListItem Text="Sick" Value="Sick" />
                                <asp:ListItem Text="Died / Killed" Value="Died/Killed" />
                                <asp:ListItem Text="Unknown" Value="Unknown" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Type of Exposure</label>
                            <asp:DropDownList ID="ddlExposureType" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Type" Value="" Selected="True" />
                                <asp:ListItem Text="Bite" Value="Bite" />
                                <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Wound Location</label>
                            <asp:TextBox ID="txtWoundLocation" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" placeholder="e.g. Left arm" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Wound Type</label>
                            <asp:DropDownList ID="ddlWoundType" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
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

                    <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Bleeding</label>
                            <asp:DropDownList ID="ddlBleeding" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select" Value="" Selected="True" />
                                <asp:ListItem Text="No" Value="No" />
                                <asp:ListItem Text="Yes" Value="Yes" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Washing of Bite Wound</label>
                            <asp:DropDownList ID="ddlWoundWashed" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select" Value="" Selected="True" />
                                <asp:ListItem Text="Washed (15 mins)" Value="Yes" />
                                <asp:ListItem Text="Unwashed" Value="No" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Category</label>
                            <asp:DropDownList ID="ddlCategory" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                <asp:ListItem Text="Select Category" Value="" Selected="True" />
                                <asp:ListItem Text="Category I" Value="I" />
                                <asp:ListItem Text="Category II" Value="II" />
                                <asp:ListItem Text="Category III" Value="III" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Manifestation</label>
                            <asp:DropDownList ID="ddlManifestation" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
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
                <div class="px-5 py-4 bg-slate-50 border-t border-slate-200 flex flex-wrap justify-end gap-3">
                    <asp:Button ID="btnClear" runat="server" Text="Clear" CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" OnClick="btnClear_Click" />
                    <asp:Button ID="btnSave" runat="server" Text="Save Patient Record" CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer" OnClick="btnSave_Click" />
                    <asp:Button ID="btnUpdateRecord" runat="server" Text="Update Record" Visible="false" CssClass="h-11 rounded-lg bg-amber-500 px-6 font-bold text-white shadow hover:bg-amber-600 transition cursor-pointer" OnClick="btnUpdateRecord_Click" />
                    <asp:Button ID="btnCancelEditForm" runat="server" Text="Cancel Edit" Visible="false" CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" OnClick="btnCancelEditForm_Click" />
                </div>
            </div>

        </div>

        <%-- VIEW PANEL --%>
        <div id="viewPatientPanel" class="panel space-y-6">
            <div id="viewLayout" class="grid grid-cols-1 gap-6 transition-all duration-300 no-preview">

                <div id="detailsPane" class="space-y-6 min-w-0">

                    <%-- Patient Grid --%>
                    <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                            <div>
                                <h3 class="font-extrabold text-slate-800">Patient Details</h3>
                                <p class="text-slate-500 text-sm mt-1">List of registered patients</p>
                            </div>
                            <div class="flex gap-2 flex-wrap items-center">
                                <asp:TextBox ID="txtSearchPatient" runat="server" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" placeholder="Search by Patient ID, Name, Contact, Address" />
                                <asp:TextBox ID="txtPatientDateFrom" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <span class="text-sm text-slate-400">to</span>
                                <asp:TextBox ID="txtPatientDateTo" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <asp:Button ID="btnSearchPatient" runat="server" Text="Filter" OnClick="btnSearchPatient_Click" CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                                <asp:Button ID="btnResetPatientSearch" runat="server" Text="Clear" OnClick="btnResetPatientSearch_Click" CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                            </div>
                        </div>
                        <asp:GridView ID="gvPatients" runat="server" AutoGenerateColumns="False" CssClass="w-full text-sm" GridLines="None" DataKeyNames="patient_id" OnRowCommand="gvPatients_RowCommand">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                            <Columns>
                                <asp:TemplateField HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditPatient" runat="server" Text="Edit" CommandName="EditPatient" CommandArgument='<%# Eval("patient_id") %>' CssClass="text-blue-600 font-semibold text-xs hover:underline" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="patient_id" HeaderText="Patient ID" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="fname" HeaderText="First Name" ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="lname" HeaderText="Last Name" ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="gender" HeaderText="Gender" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="contact_no" HeaderText="Contact No" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="address" HeaderText="Address" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="date_added" HeaderText="Date Added" DataFormatString="{0:MMM dd, yyyy}" ItemStyle-CssClass="p-4 text-slate-500 text-xs whitespace-nowrap" HeaderStyle-CssClass="p-4" />
                            </Columns>
                            <EmptyDataTemplate><div class="p-10 text-center text-slate-400 text-sm">No patient records found.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                    <%-- Case Grid --%>
                    <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                            <div>
                                <h3 class="font-extrabold text-slate-800">Case Details</h3>
                                <p class="text-slate-500 text-sm mt-1">Recorded bite exposure cases</p>
                            </div>
                            <div class="flex gap-2 flex-wrap items-center">
                                <asp:TextBox ID="txtSearchCase" runat="server" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" placeholder="Search by Case ID, Patient ID, Case No, Place, Category" />
                                <asp:TextBox ID="txtCaseDateFrom" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <span class="text-sm text-slate-400">to</span>
                                <asp:TextBox ID="txtCaseDateTo" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <asp:Button ID="btnSearchCase" runat="server" Text="Filter" OnClick="btnSearchCase_Click" CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                                <asp:Button ID="btnResetCaseSearch" runat="server" Text="Clear" OnClick="btnResetCaseSearch_Click" CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                            </div>
                        </div>
                        <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False" CssClass="w-full text-sm" GridLines="None" DataKeyNames="case_id" OnRowCommand="gvCases_RowCommand">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                            <Columns>
                                <asp:TemplateField HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditCase" runat="server" Text="Edit" CommandName="EditCase" CommandArgument='<%# Eval("case_id") %>' CssClass="text-blue-600 font-semibold text-xs hover:underline" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="case_id" HeaderText="Case ID" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="patient_id" HeaderText="Patient ID" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="case_no" HeaderText="Case Number" ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="date_of_bite" HeaderText="Date of Bite" DataFormatString="{0:MMM dd, yyyy}" ItemStyle-CssClass="p-4 text-slate-500 text-xs whitespace-nowrap" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="place_of_bite" HeaderText="Place of Bite" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="type_of_exposure" HeaderText="Type of Exposure" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="site_of_bite" HeaderText="Site of Bite" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="category" HeaderText="Category" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                            </Columns>
                            <EmptyDataTemplate><div class="p-10 text-center text-slate-400 text-sm">No case records found.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                </div>

                <%-- Record Preview --%>
                <asp:Panel ID="pnlRecordPreviewContainer" runat="server" Visible="false" CssClass="min-w-0">
                    <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden lg:sticky lg:top-6 max-h-[85vh] flex flex-col">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex-shrink-0">
                            <h3 class="font-extrabold text-slate-800">Record Preview</h3>
                            <p class="text-slate-500 text-sm mt-1">Edit the selected record here</p>
                        </div>
                        <div class="flex-1 overflow-y-auto p-5">

                            <%-- Patient Preview --%>
                            <asp:Panel ID="pnlPatientPreview" runat="server" Visible="false" CssClass="space-y-4">
                                <div class="rounded-xl bg-blue-50 border border-blue-100 px-4 py-3">
                                    <h5 class="font-extrabold text-blue-900 text-sm">Patient Information</h5>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                    <div class="md:col-span-2">
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Patient ID</label>
                                        <asp:TextBox ID="txtPreviewPatientId" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">First Name</label>
                                        <asp:TextBox ID="txtPreviewFirstName" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Last Name</label>
                                        <asp:TextBox ID="txtPreviewLastName" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date of Birth</label>
                                        <asp:TextBox ID="txtPreviewDOB" runat="server" TextMode="Date" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Gender</label>
                                        <asp:DropDownList ID="ddlPreviewGender" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Gender" Value="" />
                                            <asp:ListItem Text="Male" Value="M" />
                                            <asp:ListItem Text="Female" Value="F" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Civil Status</label>
                                        <asp:DropDownList ID="ddlPreviewCivilStatus" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Status" Value="" />
                                            <asp:ListItem Text="Single" Value="Single" />
                                            <asp:ListItem Text="Married" Value="Married" />
                                            <asp:ListItem Text="Widowed" Value="Widowed" />
                                            <asp:ListItem Text="Separated" Value="Separated" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact No</label>
                                        <asp:TextBox ID="txtPreviewContactNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Occupation</label>
                                        <asp:DropDownList ID="ddlPreviewOccupation" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Occupation" Value="" />
                                            <asp:ListItem Text="Student" Value="Student" />
                                            <asp:ListItem Text="Employed" Value="Employed" />
                                            <asp:ListItem Text="Self-Employed" Value="Self-Employed" />
                                            <asp:ListItem Text="Unemployed" Value="Unemployed" />
                                            <asp:ListItem Text="Retired" Value="Retired" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Address</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">House No.</label>
                                            <asp:TextBox ID="txtPreviewHouseNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="House No." />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Street</label>
                                            <asp:TextBox ID="txtPreviewStreet" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Street" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Barangay</label>
                                            <asp:TextBox ID="txtPreviewBarangay" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Barangay" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">City / Province</label>
                                            <asp:TextBox ID="txtPreviewCityProvince" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="City / Province" />
                                        </div>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Emergency Contact</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact Person</label>
                                            <asp:TextBox ID="txtPreviewEmergencyPerson" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Full name" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact Number</label>
                                            <asp:TextBox ID="txtPreviewEmergencyNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 09123456789" />
                                        </div>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Vitals</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Blood Pressure</label>
                                            <asp:TextBox ID="txtPreviewBP" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 120/80" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Temperature</label>
                                            <asp:TextBox ID="txtPreviewTemp" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 36.5" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Weight</label>
                                            <asp:TextBox ID="txtPreviewWeight" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 65" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Capillary Refill</label>
                                            <asp:TextBox ID="txtPreviewCapillaryRefill" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. &lt; 2 seconds" />
                                        </div>
                                    </div>
                                </div>
                                <div>
                                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date Added</label>
                                    <asp:TextBox ID="txtPreviewDateAdded" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                </div>
                                <div class="pt-2 flex gap-3">
                                    <asp:Button ID="btnPreviewUpdatePatient" runat="server" Text="Update" CssClass="flex-1 bg-amber-500 hover:bg-amber-600 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm" OnClick="btnPreviewUpdatePatient_Click" UseSubmitBehavior="false" OnClientClick="showConfirmModal('patient'); return false;" />
                                    <asp:Button ID="btnPreviewCancelPatient" runat="server" Text="Cancel" CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm" OnClick="btnPreviewCancelPatient_Click" />
                                </div>
                            </asp:Panel>

                            <%-- Case Preview --%>
                            <asp:Panel ID="pnlCasePreview" runat="server" Visible="false" CssClass="space-y-4">
                                <div class="rounded-xl bg-emerald-50 border border-emerald-100 px-4 py-3">
                                    <h5 class="font-extrabold text-emerald-900 text-sm">Case Information</h5>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Case ID</label>
                                        <asp:TextBox ID="txtPreviewCaseId" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Patient ID</label>
                                        <asp:TextBox ID="txtPreviewCasePatientId" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Case No</label>
                                        <asp:TextBox ID="txtPreviewCaseNo" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseDateOfBite" runat="server" TextMode="Date" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Time of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseTimeOfBite" runat="server" TextMode="Time" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Exposure Type</label>
                                        <asp:DropDownList ID="ddlPreviewCaseExposureType" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Type" Value="" />
                                            <asp:ListItem Text="Bite" Value="Bite" />
                                            <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Place of Bite</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">House No.</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceHouseNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="House No." />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Street</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceStreet" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Street" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Barangay</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceBarangay" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Barangay" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">City / Province</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceCity" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="City / Province" />
                                        </div>
                                    </div>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Wound Type</label>
                                        <asp:DropDownList ID="ddlPreviewCaseWoundType" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
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
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Bleeding</label>
                                        <asp:DropDownList ID="ddlPreviewCaseBleeding" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select" Value="" />
                                            <asp:ListItem Text="No" Value="No" />
                                            <asp:ListItem Text="Yes" Value="Yes" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Site of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseSiteOfBite" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Category</label>
                                        <asp:DropDownList ID="ddlPreviewCaseCategory" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Category" Value="" />
                                            <asp:ListItem Text="I" Value="I" />
                                            <asp:ListItem Text="II" Value="II" />
                                            <asp:ListItem Text="III" Value="III" />
                                        </asp:DropDownList>
                                    </div>
                                    <div class="md:col-span-2">
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Washed</label>
                                        <asp:DropDownList ID="ddlPreviewCaseWashed" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select" Value="" />
                                            <asp:ListItem Text="Yes" Value="Yes" />
                                            <asp:ListItem Text="No" Value="No" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="pt-2 flex gap-3">
                                    <asp:Button ID="btnPreviewUpdateCase" runat="server" Text="Update" CssClass="flex-1 bg-amber-500 hover:bg-amber-600 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm" OnClick="btnPreviewUpdateCase_Click" UseSubmitBehavior="false" OnClientClick="showConfirmModal('case'); return false;" />
                                    <asp:Button ID="btnPreviewCancelCase" runat="server" Text="Cancel" CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm" OnClick="btnPreviewCancelCase_Click" />
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
            #viewLayout.with-preview { grid-template-columns: minmax(0, 1.8fr) minmax(360px, 440px); align-items: start; }
            #viewLayout.no-preview   { grid-template-columns: minmax(0, 1fr); }
        }
        @media (max-width: 1279px) {
            #viewLayout.with-preview, #viewLayout.no-preview { grid-template-columns: 1fr; }
        }
        @keyframes fadeIn { from { opacity:0; transform:translateY(8px) scale(.98); } to { opacity:1; transform:translateY(0) scale(1); } }
    </style>

    <div id="confirmModal" class="fixed inset-0 z-[100] hidden items-center justify-center bg-slate-900/50 px-4">
        <div class="w-full max-w-md rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden" style="animation:fadeIn .2s ease-out;">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                <h3 class="font-extrabold text-slate-900">Confirm Update</h3>
                <p id="confirmModalMessage" class="mt-1 text-sm text-slate-500">Are you sure you want to continue?</p>
            </div>
            <div class="px-5 py-4 flex justify-end gap-3">
                <button type="button" onclick="hideConfirmModal()" class="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition">Cancel</button>
                <button type="button" onclick="confirmModalAction()" class="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer transition">Confirm</button>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        var pendingUpdateType = '';
        function setActivePanelButton(panelId) {
            ['btnViewPanel', 'btnAddPanel'].forEach(function (id) {
                var b = document.getElementById(id); if (!b) return;
                b.classList.remove('bg-blue-600', 'text-white', 'border-blue-600', 'shadow');
                b.classList.add('bg-white', 'text-slate-700', 'border-slate-300');
            });
            var a = document.getElementById(panelId === 'viewPatientPanel' ? 'btnViewPanel' : 'btnAddPanel');
            if (a) { a.classList.remove('bg-white', 'text-slate-700', 'border-slate-300'); a.classList.add('bg-blue-600', 'text-white', 'border-blue-600', 'shadow'); }
        }
        function updateViewLayout() {
            var vl = document.getElementById('viewLayout'), pr = document.getElementById('<%=pnlRecordPreviewContainer.ClientID%>');
            if (!vl || !pr) return;
            var vis = pr.style.display !== 'none' && !pr.hasAttribute('hidden') && pr.offsetParent !== null;
            vl.classList.remove('with-preview', 'no-preview'); vl.classList.add(vis ? 'with-preview' : 'no-preview');
        }
        function showPanel(panelId) {
            document.querySelectorAll('.panel').forEach(function (p) { p.classList.add('hidden'); });
            var t = document.getElementById(panelId); if (t) t.classList.remove('hidden');
            setActivePanelButton(panelId);
            var hf = document.getElementById('<%=hfActivePanel.ClientID%>'); if (hf) hf.value = panelId;
            setTimeout(updateViewLayout, 50);
        }
        function toggleVitals() { var v = document.getElementById('optionalVitals'); if (v) v.classList.toggle('hidden'); }
        function showConfirmModal(type) {
            pendingUpdateType = type || '';
            var msg = document.getElementById('confirmModalMessage');
            if (msg) msg.textContent = type === 'patient' ? 'Are you sure you want to update this patient record?' : type === 'case' ? 'Are you sure you want to update this case record?' : 'Are you sure you want to continue?';
            var m = document.getElementById('confirmModal'); if (m) { m.classList.remove('hidden'); m.classList.add('flex'); }
            document.body.classList.add('overflow-hidden');
        }
        function hideConfirmModal() {
            var m = document.getElementById('confirmModal'); if (m) { m.classList.add('hidden'); m.classList.remove('flex'); }
            document.body.classList.remove('overflow-hidden');
        }
        function confirmModalAction() {
            var t = pendingUpdateType; hideConfirmModal();
            if (t === 'patient') __doPostBack('<%=btnPreviewUpdatePatient.UniqueID%>','');
            else if(t==='case') __doPostBack('<%=btnPreviewUpdateCase.UniqueID%>','');
        }
        document.addEventListener('DOMContentLoaded',function(){
            var hf=document.getElementById('<%=hfActivePanel.ClientID%>');
            showPanel(hf&&hf.value?hf.value:'viewPatientPanel'); setTimeout(updateViewLayout,100);
        });
        document.addEventListener('keydown',function(e){var m=document.getElementById('confirmModal');if(!m||m.classList.contains('hidden'))return;if(e.key==='Escape')hideConfirmModal();});
        document.addEventListener('click',function(e){var m=document.getElementById('confirmModal');if(!m||m.classList.contains('hidden'))return;if(e.target===m)hideConfirmModal();});
        window.addEventListener('resize',updateViewLayout);
    </script>

    <%-- ══════════════════════════════════════════════════
         NOTIFICATION MODAL (replaces alert())
         ══════════════════════════════════════════════════ --%>
    <div id="notifyModal" class="fixed inset-0 z-[200] hidden items-center justify-center bg-slate-900/50 px-4">
        <div id="notifyModalBox" class="w-full max-w-sm rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden" style="animation:fadeIn .2s ease-out;">
            <div id="notifyModalHeader" class="px-6 py-4 flex items-center gap-3">
                <div id="notifyModalIcon" class="w-10 h-10 rounded-full flex items-center justify-center text-xl flex-shrink-0"></div>
                <div>
                    <h3 id="notifyModalTitle" class="font-extrabold text-slate-900 text-base"></h3>
                    <p id="notifyModalMessage" class="text-sm text-slate-500 mt-0.5"></p>
                </div>
            </div>
            <div class="px-6 pb-4 flex justify-end">
                <button type="button" onclick="hideNotifyModal()"
                    id="notifyModalBtn"
                    class="px-6 py-2 rounded-lg text-sm font-bold cursor-pointer transition">
                    OK
                </button>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        function showNotifyModal(message, type) {
            type = type || 'info';
            var modal = document.getElementById('notifyModal');
            var icon = document.getElementById('notifyModalIcon');
            var title = document.getElementById('notifyModalTitle');
            var msg = document.getElementById('notifyModalMessage');
            var btn = document.getElementById('notifyModalBtn');
            var header = document.getElementById('notifyModalHeader');

            // Reset classes
            icon.className = 'w-10 h-10 rounded-full flex items-center justify-center text-xl flex-shrink-0';
            btn.className = 'px-6 py-2 rounded-lg text-sm font-bold cursor-pointer transition';
            header.className = 'px-6 py-4 flex items-center gap-3';

            if (type === 'success') {
                icon.className += ' bg-emerald-100 text-emerald-600';
                icon.innerHTML = '✓';
                title.textContent = 'Success';
                btn.className += ' bg-emerald-600 hover:bg-emerald-700 text-white';
                header.className += ' border-b border-emerald-100 bg-emerald-50';
            } else if (type === 'error') {
                icon.className += ' bg-red-100 text-red-600';
                icon.innerHTML = '✕';
                title.textContent = 'Error';
                btn.className += ' bg-red-600 hover:bg-red-700 text-white';
                header.className += ' border-b border-red-100 bg-red-50';
            } else if (type === 'warning') {
                icon.className += ' bg-amber-100 text-amber-600';
                icon.innerHTML = '⚠';
                title.textContent = 'Warning';
                btn.className += ' bg-amber-500 hover:bg-amber-600 text-white';
                header.className += ' border-b border-amber-100 bg-amber-50';
            } else {
                icon.className += ' bg-blue-100 text-blue-600';
                icon.innerHTML = 'ℹ';
                title.textContent = 'Notice';
                btn.className += ' bg-blue-600 hover:bg-blue-700 text-white';
                header.className += ' border-b border-blue-100 bg-blue-50';
            }

            msg.textContent = message;
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            document.body.classList.add('overflow-hidden');
        }

        function hideNotifyModal() {
            var modal = document.getElementById('notifyModal');
            modal.classList.add('hidden');
            modal.classList.remove('flex');
            document.body.classList.remove('overflow-hidden');
        }

        document.addEventListener('keydown', function (e) {
            var m = document.getElementById('notifyModal');
            if (!m || m.classList.contains('hidden')) return;
            if (e.key === 'Escape') hideNotifyModal();
        });
    </script>
</asp:Content>
