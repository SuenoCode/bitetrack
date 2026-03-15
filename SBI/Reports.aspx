<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="SBI.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActiveReport" runat="server" Value="DailyInventorySummary" />
    <asp:HiddenField ID="hfSelectedCaseId" runat="server" Value="" />

    <div class="p-6 font-heading2 text-slate-900">

        <%-- Page Header --%>
        <div class="flex justify-between items-start mb-6">
            <div>
                 <h1 class="text-4xl text-[#0b2a7a] font-hBruns tracking-widest">Reports</h1>
                <p class="text-slate-500 text-sm mt-1">Generate and export inventory and bite case reports</p>
            </div>
        </div>

        <%-- Tab Buttons --%>
        <div class="flex gap-3 mb-6">
            <asp:Button ID="btnTabInventory" runat="server"
                Text="Daily Inventory Summary"
                CausesValidation="false" OnClick="btnTabInventory_Click"
                CssClass="rounded-xl border px-6 py-3 text-sm font-semibold transition cursor-pointer shadow-sm" />
            <asp:Button ID="btnTabBiteCase" runat="server"
                Text="Bite Case Report"
                CausesValidation="false" OnClick="btnTabBiteCase_Click"
                CssClass="rounded-xl border px-6 py-3 text-sm font-semibold transition cursor-pointer shadow-sm" />
        </div>

        <%-- Stat Cards --%>
        <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Active Report</div>
                <div class="text-xl font-extrabold text-[#0b2a7a]">
                    <asp:Label ID="lblActiveReport" runat="server" Text="Daily Inventory Summary" />
                </div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">From Date</div>
                <div class="text-2xl font-extrabold text-emerald-600">
                    <asp:Label ID="lblFromDate" runat="server" Text="—" />
                </div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-wider mb-1">To Date</div>
                <div class="text-2xl font-extrabold text-amber-500">
                    <asp:Label ID="lblToDate" runat="server" Text="—" />
                </div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Total Records</div>
                <div class="text-3xl font-extrabold text-indigo-600">
                    <asp:Label ID="lblTotalRecords" runat="server" Text="0" />
                </div>
            </div>
        </div>

        <%-- Shared message --%>
        <asp:Label ID="lblMessage" runat="server"
            CssClass="block mb-4 text-sm font-semibold text-red-600" />

        <%-- ════════════════════════════════════════════════════════
             PANEL A — Daily Inventory Summary
        ════════════════════════════════════════════════════════ --%>
        <asp:Panel ID="pnlInventory" runat="server" CssClass="space-y-6">

            <%-- Filter bar for Inventory --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                    <h3 class="font-extrabold text-slate-800">Filter Inventory Report</h3>
                    <div class="flex gap-2 flex-wrap items-end">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Period</label>
                            <asp:DropDownList ID="ddlReportPeriod" runat="server"
                                AutoPostBack="true"
                                OnSelectedIndexChanged="ddlReportPeriod_SelectedIndexChanged"
                                CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                <asp:ListItem Text="Monthly" Value="Monthly" />
                                <asp:ListItem Text="Weekly"  Value="Weekly" />
                                <asp:ListItem Text="Daily"   Value="Daily" />
                                <asp:ListItem Text="Custom"  Value="Custom" />
                            </asp:DropDownList>
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">From</label>
                            <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"
                                CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">To</label>
                            <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"
                                CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                        </div>
                        <asp:Button ID="btnGenerate" runat="server" Text="Generate"
                            OnClick="btnGenerateReport_Click"
                            CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer self-end" />
                        <asp:Button ID="btnExportExcel" runat="server" Text="Export Excel"
                            CausesValidation="false" OnClick="btnExportExcel_Click"
                            CssClass="bg-white border border-slate-300 text-emerald-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-emerald-50 transition self-end" />
                    </div>
                </div>
            </div>

            <%-- Inventory Grid --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Vaccine Inventory</h3>
                    <p class="text-slate-500 text-sm mt-1">Stock movement for the selected date range</p>
                </div>
                <div class="p-5 overflow-x-auto">
                    <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="true"
                        CssClass="w-full text-sm" GridLines="None">
                        <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                        <RowStyle CssClass="border-b border-slate-100 p-4" />
                        <AlternatingRowStyle CssClass="border-b border-slate-100 bg-slate-50/40" />
                        <EmptyDataTemplate>
                            <div class="p-10 text-center text-slate-400 text-sm">No inventory data for the selected range.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>
        </asp:Panel>

        <%-- ════════════════════════════════════════════════════════
             PANEL B — Bite Case Report
        ════════════════════════════════════════════════════════ --%>
        <asp:Panel ID="pnlBiteCase" runat="server" Visible="false" CssClass="space-y-5">

            <%-- Case Search / Selection Bar --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                    <h3 class="font-extrabold text-slate-800">Select Case</h3>
                    <div class="flex gap-2 flex-wrap items-end">
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Search</label>
                            <asp:TextBox ID="txtCaseSearch" runat="server"
                                CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                placeholder="Case No, Patient Name…" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Bite Date From</label>
                            <asp:TextBox ID="txtCaseFromDate" runat="server" TextMode="Date"
                                CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                        </div>
                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Bite Date To</label>
                            <asp:TextBox ID="txtCaseToDate" runat="server" TextMode="Date"
                                CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                        </div>
                        <asp:Button ID="btnSearchCases" runat="server" Text="Search"
                            OnClick="btnSearchCases_Click"
                            CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer self-end" />
                        <asp:Button ID="btnClearCaseSearch" runat="server" Text="Clear"
                            CausesValidation="false" OnClick="btnClearCaseSearch_Click"
                            CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition self-end" />
                    </div>
                </div>

                <%-- Case Selection Grid --%>
                <asp:GridView ID="gvCaseList" runat="server"
                    CssClass="w-full text-sm" GridLines="None"
                    AutoGenerateColumns="False"
                    DataKeyNames="case_id"
                    OnRowCommand="gvCaseList_RowCommand">
                    <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                    <RowStyle CssClass="border-b border-slate-100 hover:bg-slate-50 transition-colors" />
                    <Columns>
                        <asp:BoundField DataField="case_no" HeaderText="Case No"
                            ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                        <asp:BoundField DataField="patient_name" HeaderText="Patient Name"
                            ItemStyle-CssClass="p-4 text-slate-700" HeaderStyle-CssClass="p-4" />
                        <asp:BoundField DataField="date_of_bite" HeaderText="Date of Bite"
                            DataFormatString="{0:MMM dd, yyyy}"
                            ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                        <asp:BoundField DataField="category" HeaderText="Category"
                            ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                        <asp:BoundField DataField="regimen_type" HeaderText="Protocol" NullDisplayText="-"
                            ItemStyle-CssClass="p-4 text-slate-500" HeaderStyle-CssClass="p-4" />
                        <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                            <ItemTemplate>
                                <asp:Button ID="btnViewCase" runat="server"
                                    CommandName="ViewCase"
                                    CommandArgument='<%# Container.DataItemIndex %>'
                                    Text="View Report"
                                    CssClass="bg-blue-600 hover:bg-blue-700 text-white font-bold py-1.5 px-4 rounded-lg text-xs transition cursor-pointer" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="p-10 text-center text-slate-400 text-sm">No cases found. Use the search above to find a case.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

            <%-- Case Report Detail — shown after selecting a case --%>
            <asp:Panel ID="pnlCaseDetail" runat="server" Visible="false" CssClass="space-y-5">

                <%-- Report Action Bar --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                        <div>
                            <h3 class="font-extrabold text-slate-800">Case Report</h3>
                            <p class="text-slate-500 text-xs mt-0.5">Viewing detailed report for the selected case</p>
                        </div>
                        <div class="flex gap-2">
                            <asp:Button ID="btnBackToCaseList" runat="server" Text="← Back to Cases"
                                CausesValidation="false" OnClick="btnBackToCaseList_Click"
                                CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                            <asp:Button ID="btnExportPdf" runat="server" Text="Export PDF"
                                CausesValidation="false" OnClick="btnExportPdf_Click"
                                CssClass="bg-red-600 hover:bg-red-700 text-white px-5 py-2 rounded-lg text-sm font-bold cursor-pointer transition" />
                        </div>
                    </div>
                </div>

                <%-- I. Patient Demographics --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-slate-200 bg-[#0b2a7a]">
                        <h3 class="font-extrabold text-white text-sm tracking-wide">I. &nbsp;Patient Demographics</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Patient ID:</span>
                            <asp:Label ID="lblPatientId" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Full Name:</span>
                            <asp:Label ID="lblPatientName" runat="server" CssClass="ml-1 text-slate-800 font-bold" /></div>
                        <div><span class="font-semibold text-slate-500">Date of Birth:</span>
                            <asp:Label ID="lblDob" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Age:</span>
                            <asp:Label ID="lblAge" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Gender:</span>
                            <asp:Label ID="lblGender" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Civil Status:</span>
                            <asp:Label ID="lblCivilStatus" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Contact Number:</span>
                            <asp:Label ID="lblContact" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Occupation:</span>
                            <asp:Label ID="lblOccupation" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Address:</span>
                            <asp:Label ID="lblAddress" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Emergency Contact:</span>
                            <asp:Label ID="lblEmergencyContact" runat="server" CssClass="ml-1 text-slate-800" /></div>
                    </div>
                </div>

                <%-- II. Bite Incident Details --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-red-200 bg-red-50">
                        <h3 class="font-extrabold text-red-700 text-sm tracking-wide">II. &nbsp;Bite Incident Details</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Case Number:</span>
                            <asp:Label ID="lblCaseNo" runat="server" CssClass="ml-1 text-slate-800 font-bold" /></div>
                        <div><span class="font-semibold text-slate-500">Date of Bite:</span>
                            <asp:Label ID="lblBiteDate" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Time of Bite:</span>
                            <asp:Label ID="lblBiteTime" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Place of Bite:</span>
                            <asp:Label ID="lblBitePlace" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Type of Exposure:</span>
                            <asp:Label ID="lblExposureType" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Wound Type:</span>
                            <asp:Label ID="lblWoundType" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Site of Bite:</span>
                            <asp:Label ID="lblBiteSite" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Bleeding:</span>
                            <asp:Label ID="lblBleeding" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Category:</span>
                            <asp:Label ID="lblCategory" runat="server"
                                CssClass="ml-1 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-red-100 text-red-700" /></div>
                        <div><span class="font-semibold text-slate-500">Washed Immediately:</span>
                            <asp:Label ID="lblWashed" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Animal Type:</span>
                            <asp:Label ID="lblAnimalType" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Ownership:</span>
                            <asp:Label ID="lblAnimalOwnership" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Animal Status:</span>
                            <asp:Label ID="lblAnimalStatus" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Circumstances:</span>
                            <asp:Label ID="lblCircumstances" runat="server" CssClass="ml-1 text-slate-800" /></div>
                    </div>
                </div>

                <%-- III. Medical Assessment --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-emerald-200 bg-emerald-50">
                        <h3 class="font-extrabold text-emerald-700 text-sm tracking-wide">III. &nbsp;Medical Assessment</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Blood Pressure:</span>
                            <asp:Label ID="lblBP" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Temperature (°C):</span>
                            <asp:Label ID="lblTemp" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Weight (kg):</span>
                            <asp:Label ID="lblWeight" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Capillary Refill:</span>
                            <asp:Label ID="lblCR" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Diagnosis:</span>
                            <asp:Label ID="lblDiagnosis" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Manifestation / Symptoms:</span>
                            <asp:Label ID="lblSymptoms" runat="server" CssClass="ml-1 text-slate-800" /></div>
                    </div>
                </div>

                <%-- IV. Vaccination Regimen --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-indigo-200 bg-indigo-50">
                        <h3 class="font-extrabold text-indigo-700 text-sm tracking-wide">IV. &nbsp;Vaccination Regimen</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Regimen Type:</span>
                            <asp:Label ID="lblRegimenType" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Vaccine Name:</span>
                            <asp:Label ID="lblVaccineName" runat="server" CssClass="ml-1 text-slate-800 font-bold" /></div>
                        <div><span class="font-semibold text-slate-500">Manufacturer:</span>
                            <asp:Label ID="lblManufacturer" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Start Date:</span>
                            <asp:Label ID="lblRegimenStart" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Total Doses:</span>
                            <asp:Label ID="lblTotalDoses" runat="server" CssClass="ml-1 text-slate-800 font-bold" /></div>
                        <div><span class="font-semibold text-slate-500">Status:</span>
                            <asp:Label ID="lblRegimenStatus" runat="server" CssClass="ml-1 text-slate-800" /></div>
                    </div>
                </div>

                <%-- V. Vaccination Schedule --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-slate-200 bg-slate-700">
                        <h3 class="font-extrabold text-white text-sm tracking-wide">V. &nbsp;Vaccination Schedule &amp; Administration</h3>
                    </div>
                    <div class="p-5 overflow-x-auto">
                        <asp:GridView ID="gvSchedule" runat="server" AutoGenerateColumns="true"
                            CssClass="w-full text-sm" GridLines="None">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100" />
                            <AlternatingRowStyle CssClass="border-b border-slate-100 bg-slate-50/40" />
                            <EmptyDataTemplate>
                                <div class="p-8 text-center text-slate-400 text-sm">No scheduled doses found.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

                <%-- VI. Animal Follow-Up --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-orange-200 bg-orange-50">
                        <h3 class="font-extrabold text-orange-700 text-sm tracking-wide">VI. &nbsp;Animal Observation &amp; Follow-Up</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Animal Status:</span>
                            <asp:Label ID="lblAnimalObsStatus" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Day-14 Status:</span>
                            <asp:Label ID="lblDay14Status" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Follow-up Date:</span>
                            <asp:Label ID="lblFollowupDate" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Notes:</span>
                            <asp:Label ID="lblFollowupNotes" runat="server" CssClass="ml-1 text-slate-800" /></div>
                    </div>
                </div>

                <%-- VII. Treatment Summary --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-teal-200 bg-teal-50">
                        <h3 class="font-extrabold text-teal-700 text-sm tracking-wide">VII. &nbsp;Treatment Summary</h3>
                    </div>
                    <div class="p-5 overflow-x-auto">
                        <asp:GridView ID="gvTreatment" runat="server" AutoGenerateColumns="true"
                            CssClass="w-full text-sm" GridLines="None">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100" />
                            <AlternatingRowStyle CssClass="border-b border-slate-100 bg-slate-50/40" />
                            <EmptyDataTemplate>
                                <div class="p-8 text-center text-slate-400 text-sm">No treatment records found.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </div>
                </div>

                <%-- VIII. Previous Vaccine History --%>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-3 border-b border-purple-200 bg-purple-50">
                        <h3 class="font-extrabold text-purple-700 text-sm tracking-wide">VIII. &nbsp;Previous Vaccine History</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Had Previous Vaccine:</span>
                            <asp:Label ID="lblHadPrevVaccine" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Previous Type:</span>
                            <asp:Label ID="lblPrevVaccineType" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Brand:</span>
                            <asp:Label ID="lblPrevBrand" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Dose Status:</span>
                            <asp:Label ID="lblPrevDoseStatus" runat="server" CssClass="ml-1 text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Vaccination Date:</span>
                            <asp:Label ID="lblPrevVaccDate" runat="server" CssClass="ml-1 text-slate-800" /></div>
                    </div>
                </div>

            </asp:Panel>
        </asp:Panel>

    </div>
</asp:Content>