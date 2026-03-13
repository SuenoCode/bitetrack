<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="PatientRegistration.aspx.cs" Inherits="SBI.PatientRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- PANEL TOGGLE BUTTONS -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
            <button type="button" class="rounded-2xl border border-slate-200 bg-white shadow p-5 hover:shadow-lg transition font-semibold text-green-700"
                onclick="showPanel('viewPatientPanel')">
                View Patient / Case Details
            </button>
            <button type="button" class="rounded-2xl border border-slate-200 bg-white shadow p-5 hover:shadow-lg transition font-semibold text-blue-700"
                onclick="showPanel('addPatientPanel')">
                Add New Patient / Case

        </div>

        <!-- ====================== ADD PATIENT / CASE PANEL ====================== -->
        <div id="addPatientPanel" class="panel">

            <!-- PAGE HEADER -->
            <div class="mb-5">
                <h2 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Patient Registration</h2>
                <p class="mt-1 text-base text-slate-600">
                    Register patient details and document biting incident history
                </p>
            </div>

            <!-- PATIENT FORM -->
            <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

                <!-- A. Patient Information -->
                <div class="px-5 py-5 space-y-4">
                    <h3 class="text-lg font-extrabold text-slate-900 mb-2">A. Patient Information</h3>

                    <!-- Row 1 -->
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
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
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Middle Name</label>
                            <asp:TextBox ID="txtMiddleName" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                placeholder="e.g. Cruz" />
                        </div>
                    </div>

                    <!-- Row 2 -->
                    <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Date of Birth <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtDOB" runat="server" TextMode="Date"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">
                                Age
                                <span class="ml-2 bg-blue-600 text-white text-[10px] px-2 py-0.5 rounded-full font-bold">AUTO</span>
                            </label>
                            <asp:TextBox ID="txtAge" runat="server" ReadOnly="true"
                                CssClass="h-10 w-full rounded-lg border border-blue-100 bg-blue-50 px-3 text-sm font-bold text-blue-900"
                                placeholder="Auto-calculated" />
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Gender <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlGender" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
                                <asp:ListItem Text="Select Gender" Value="" Selected="True" />
                                <asp:ListItem Text="Male" Value="Male" />
                                <asp:ListItem Text="Female" Value="Female" />
                                <asp:ListItem Text="Other" Value="Other" />
                            </asp:DropDownList>
                        </div>
                    </div>

                    <!-- Row 3 -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Civil Status</label>
                            <asp:DropDownList ID="ddlCivilStatus" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
                                <asp:ListItem Text="Select Status" Value="" Selected="True" />
                                <asp:ListItem Text="Single" Value="Single" />
                                <asp:ListItem Text="Married" Value="Married" />
                                <asp:ListItem Text="Divorced" Value="Divorced" />
                                <asp:ListItem Text="Widowed" Value="Widowed" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Contact No <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtContactNo" runat="server"
                                CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200"
                                placeholder="e.g. 09123456789" />
                        </div>
                    </div>

                    <!-- Address -->
                    <div>
                        <label class="block text-sm font-semibold text-slate-700 mb-1">Address <span class="text-red-500">*</span></label>
                        <div class="grid grid-cols-1 md:grid-cols-4 gap-4">
                            <asp:TextBox ID="txtHouseNo" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="House No." />
                            <asp:TextBox ID="txtSubdivision" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="Subdivision/Street" />
                            <asp:TextBox ID="txtBarangay" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="Barangay *" />
                            <asp:TextBox ID="txtProvinceCity" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="City/Province *" />
                        </div>
                    </div>

                    <!-- Row 4 -->
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

                    <!-- Optional Vitals -->
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
                            <label class="block text-sm font-semibold text-slate-700 mb-1">Chief Complaints</label>
                            <asp:TextBox ID="txtChiefComplaints" runat="server" CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm" placeholder="Chief complaints" />
                        </div>
                    </div>
                </div>

                <!-- B. History of Biting Incident -->
                <div class="px-5 py-5 border-t border-slate-200">
                    <h3 class="text-lg font-extrabold text-slate-900 mb-4">B. History of Biting Incident</h3>

                    <div class="space-y-4">
                        <!-- Date + Place -->
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

                        <!-- Animal Type -->
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

                            <!-- Circumstance -->
                            <div class="rounded-xl border border-slate-200 p-4">
                                <div class="mb-2 text-sm font-bold text-slate-900">Circumstance <span class="text-red-500">*</span></div>
                                <div class="flex flex-wrap gap-4 text-sm font-semibold text-slate-700">
                                    <asp:RadioButton ID="rbProvoked" runat="server" GroupName="Circumstance" Text="Provoked" />
                                    <asp:RadioButton ID="rbUnprovoked" runat="server" GroupName="Circumstance" Text="Unprovoked" Checked="true" />
                                </div>
                            </div>
                        </div>

                        <!-- Ownership + Animal Status -->
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

                        <!-- Additional Bite Details -->
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
                                <asp:DropDownList ID="DropDownList1" runat="server"
                                    CssClass="h-10 w-full rounded-lg border border-slate-200 px-3 text-sm focus:ring-2 focus:ring-blue-200">
                                    <asp:ListItem Text="select type" Value="" Selected="True" />
                                    <asp:ListItem Text="Lacerated" Value="Lacerated" />
                                    <asp:ListItem Text="Avulsion" Value="Avulsion" />
                                    <asp:ListItem Text="Punctured" Value="Punctured" />
                                    <asp:ListItem Text="Abrasion" Value="Abrasion" />
                                    <asp:ListItem Text="Scratchs" Value="Scratchs" />
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

                        <!-- Category -->
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

                <!-- ACTION BUTTONS -->
                <div class="px-5 py-4 bg-slate-50 border-t border-slate-200 flex justify-end gap-3">
                    <asp:Button ID="btnClear" runat="server" Text="Clear"
                        CssClass="px-6 py-2 rounded-lg border border-slate-300 bg-white text-slate-700 font-semibold hover:bg-slate-100"
                        OnClick="btnClear_Click" />
                    <asp:Button ID="btnSave" runat="server" Text="Save Patient Record"
                        CssClass="px-6 py-2 rounded-lg bg-blue-700 text-white font-semibold shadow hover:bg-blue-800"
                        OnClick="btnSave_Click" />
                </div>
            </div>
        </div>
        <!-- END ADD PATIENT PANEL -->

        <!-- ====================== VIEW PATIENT DETAILS PANEL ====================== -->
<div id="viewPatientPanel" class="panel hidden rounded-2xl border border-slate-200 bg-white shadow p-5">
    <h4 class="text-lg font-bold text-slate-900 mb-4">Patient Details</h4>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvPatients" runat="server" AutoGenerateColumns="False"
                    CssClass="w-full border-collapse border border-slate-200"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-semibold"
                    RowStyle-CssClass="border-b border-slate-200 hover:bg-slate-50"
                    AlternatingRowStyle-CssClass="bg-slate-50">
                    <Columns>
                        <asp:BoundField DataField="patient_id" HeaderText="Patient ID" />
                        <asp:BoundField DataField="fname" HeaderText="First Name" />
                        <asp:BoundField DataField="lname" HeaderText="Last Name" />
                        <asp:BoundField DataField="Gender" HeaderText="Gender" />
                        <asp:BoundField DataField="contact_no" HeaderText="Contact No" />
                        <asp:BoundField DataField="address" HeaderText="Address" />
                        <asp:BoundField DataField="date_added" HeaderText="Date Added" DataFormatString="{0:MMM dd, yyyy}" />
                    </Columns>
                </asp:GridView>
            </div>
            <br /><br />
            <h4 class="text-lg font-bold text-slate-900 mb-4">Case Details</h4>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False"
                    CssClass="w-full border-collapse border border-slate-200"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-semibold"
                    RowStyle-CssClass="border-b border-slate-200 hover:bg-slate-50"
                    AlternatingRowStyle-CssClass="bg-slate-50">
                    <Columns>
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
    <!-- JavaScript -->
    <script type="text/javascript">
        function showPanel(panelId) {
            // Hide all panels
            document.querySelectorAll('.panel').forEach(p => p.classList.add('hidden'));
            // Show selected panel
            document.getElementById(panelId).classList.remove('hidden');
        }

        function toggleVitals() {
            document.getElementById('optionalVitals').classList.toggle('hidden');
        }

        // Show Add Patient panel by default
        document.addEventListener('DOMContentLoaded', function () {
            showPanel('addPatientPanel');
        });
    </script>

</asp:Content>
