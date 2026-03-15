<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="SBI.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActiveReport" runat="server" Value="DailyInventorySummary" />

    <div class="p-6 font-heading2 text-slate-900">

        <%-- ── Page Header ───────────────────────────────────────────── --%>
        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl font-bold text-[#0b2a7a] font-hBruns">Reports</h1>
                <p class="text-slate-500 text-sm mt-1">Generate and export daily inventory, activity, and bite case reports</p>
            </div>
        </div>

        <%-- ── Tab Navigation ────────────────────────────────────────── --%>
        <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
            <asp:LinkButton ID="tabDailyInventory" runat="server"
                CausesValidation="false" OnClick="tabDailyInventory_Click" />
            <asp:LinkButton ID="tabBiteCaseReport" runat="server"
                CausesValidation="false" OnClick="tabBiteCaseReport_Click" />
        </div>

        <%-- ── Stat Cards ─────────────────────────────────────────────── --%>
        <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Active Report</div>
                <div class="text-2xl font-extrabold text-[#0b2a7a]"><asp:Label ID="lblActiveReport" runat="server" Text="Daily Inventory Summary" /></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">From Date</div>
                <div class="text-3xl font-extrabold text-emerald-600"><asp:Label ID="lblFromDate" runat="server" Text="—" /></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">To Date</div>
                <div class="text-3xl font-extrabold text-amber-500"><asp:Label ID="lblToDate" runat="server" Text="—" /></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Total Records</div>
                <div class="text-3xl font-extrabold text-indigo-600"><asp:Label ID="lblTotalRecords" runat="server" Text="0" /></div>
            </div>
        </div>

        <%-- ── Filter + Export Bar ───────────────────────────────────── --%>
        <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden mb-6">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                <h3 class="font-extrabold text-slate-800">Filter Report</h3>
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
                    <asp:Button ID="btnGenerateReport" runat="server" Text="Generate" OnClick="btnGenerateReport_Click"
                        CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer self-end" />
                    <asp:Button ID="btnExportPdf" runat="server" Text="PDF" CausesValidation="false" OnClick="btnExportPdf_Click"
                        CssClass="bg-white border border-slate-300 text-red-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-red-50 transition self-end" />
                    <asp:Button ID="btnExportExcel" runat="server" Text="Excel" CausesValidation="false" OnClick="btnExportExcel_Click"
                        CssClass="bg-white border border-slate-300 text-emerald-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-emerald-50 transition self-end" />
                </div>
            </div>
        </asp:Panel>

        <%-- ── Generated Report Table (Inventory tab) ─────────────────── --%>
        <asp:Panel ID="pnlGridReport" runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                <h3 class="font-extrabold text-slate-800">Generated Report</h3>
                <p class="text-slate-500 text-sm mt-1">Filtered report data based on selected date range</p>
            </div>
            <div class="p-5">
                <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 text-sm font-semibold text-red-600" />
                <asp:GridView ID="gvReport" runat="server" AutoGenerateColumns="true"
                    CssClass="w-full text-sm" GridLines="None">
                    <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                    <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                    <AlternatingRowStyle CssClass="border-b border-slate-100" />
                    <EmptyDataTemplate>
                        <div class="p-10 text-center text-slate-400 text-sm">No report data found for the selected date range.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>
        </asp:Panel>

        <%-- ── Bite Case Report Panel ─────────────────────────────────── --%>
        <asp:Panel ID="pnlBiteCaseReport" runat="server" Visible="false" CssClass="space-y-6">

            <%-- Patient Demographics --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex items-center gap-2">
                    <span class="w-2 h-5 rounded bg-[#0b2a7a] inline-block"></span>
                    <h3 class="font-extrabold text-slate-800">Patient Demographics</h3>
                </div>
                <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                    <div><span class="font-semibold text-slate-500">Patient ID:</span> <asp:Label ID="lblPatientId" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Full Name:</span> <asp:Label ID="lblPatientName" runat="server" CssClass="text-slate-800 font-bold" /></div>
                    <div><span class="font-semibold text-slate-500">Date of Birth:</span> <asp:Label ID="lblDob" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Age:</span> <asp:Label ID="lblAge" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Gender:</span> <asp:Label ID="lblGender" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Civil Status:</span> <asp:Label ID="lblCivilStatus" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Contact Number:</span> <asp:Label ID="lblContact" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Occupation:</span> <asp:Label ID="lblOccupation" runat="server" CssClass="text-slate-800" /></div>
                    <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Address:</span> <asp:Label ID="lblAddress" runat="server" CssClass="text-slate-800" /></div>
                    <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Emergency Contact:</span> <asp:Label ID="lblEmergencyContact" runat="server" CssClass="text-slate-800" /></div>
                </div>
            </div>

            <%-- Bite Incident Details --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex items-center gap-2">
                    <span class="w-2 h-5 rounded bg-red-500 inline-block"></span>
                    <h3 class="font-extrabold text-slate-800">Bite Incident Details</h3>
                </div>
                <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                    <div><span class="font-semibold text-slate-500">Case Number:</span> <asp:Label ID="lblCaseNumber" runat="server" CssClass="text-slate-800 font-bold" /></div>
                    <div><span class="font-semibold text-slate-500">Date of Bite:</span> <asp:Label ID="lblBiteDate" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Time of Bite:</span> <asp:Label ID="lblBiteTime" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Place of Bite:</span> <asp:Label ID="lblBitePlace" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Type of Exposure:</span> <asp:Label ID="lblExposureType" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Animal Type:</span> <asp:Label ID="lblAnimalType" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Circumstance:</span> <asp:Label ID="lblCircumstance" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Wound Type:</span> <asp:Label ID="lblWoundType" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Site of Bite:</span> <asp:Label ID="lblBiteSite" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Bleeding:</span> <asp:Label ID="lblBleeding" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Category:</span>
                        <asp:Label ID="lblBiteCategory" runat="server" CssClass="ml-1 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-red-100 text-red-700" />
                    </div>
                    <div><span class="font-semibold text-slate-500">Washed Immediately:</span> <asp:Label ID="lblWashed" runat="server" CssClass="text-slate-800" /></div>
                </div>
            </div>

            <%-- Medical Assessment --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex items-center gap-2">
                    <span class="w-2 h-5 rounded bg-emerald-500 inline-block"></span>
                    <h3 class="font-extrabold text-slate-800">Medical Assessment</h3>
                </div>
                <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                    <div><span class="font-semibold text-slate-500">Blood Pressure:</span> <asp:Label ID="lblBP" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Temperature:</span> <asp:Label ID="lblTemp" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Weight:</span> <asp:Label ID="lblWeight" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Capillary Refill:</span> <asp:Label ID="lblCapRefill" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Risk Classification:</span>
                        <asp:Label ID="lblRiskClass" runat="server" CssClass="ml-1 inline-flex items-center px-2 py-0.5 rounded-full text-xs font-bold bg-amber-100 text-amber-700" />
                    </div>
                    <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Diagnosis:</span> <asp:Label ID="lblDiagnosis" runat="server" CssClass="text-slate-800" /></div>
                    <div class="sm:col-span-2 xl:col-span-3"><span class="font-semibold text-slate-500">Manifestation Symptoms:</span> <asp:Label ID="lblSymptoms" runat="server" CssClass="text-slate-800" /></div>
                </div>
            </div>

            <%-- Vaccination Regimen --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex items-center gap-2">
                    <span class="w-2 h-5 rounded bg-indigo-500 inline-block"></span>
                    <h3 class="font-extrabold text-slate-800">Vaccination Regimen</h3>
                </div>
                <div class="p-5 grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-3 gap-x-8 gap-y-3 text-sm">
                    <div><span class="font-semibold text-slate-500">Regimen Type:</span> <asp:Label ID="lblRegimenType" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Vaccine Name:</span> <asp:Label ID="lblVaccineName" runat="server" CssClass="text-slate-800 font-bold" /></div>
                    <div><span class="font-semibold text-slate-500">Manufacturer:</span> <asp:Label ID="lblManufacturer" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Start Date:</span> <asp:Label ID="lblVaccineStartDate" runat="server" CssClass="text-slate-800" /></div>
                    <div><span class="font-semibold text-slate-500">Total Doses:</span> <asp:Label ID="lblTotalDoses" runat="server" CssClass="text-slate-800 font-bold" /></div>
                </div>
            </div>

            <%-- Vaccination Schedule --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Vaccination Schedule & Administration</h3>
                    <p class="text-slate-500 text-sm mt-1">Per-dose schedule, visit dates, and administering personnel</p>
                </div>
                <div class="p-5">
                    <asp:GridView ID="gvVaccinationSchedule" runat="server" AutoGenerateColumns="true"
                        CssClass="w-full text-sm" GridLines="None">
                        <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                        <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                        <AlternatingRowStyle CssClass="border-b border-slate-100 bg-slate-50/40" />
                        <EmptyDataTemplate>
                            <div class="p-10 text-center text-slate-400 text-sm">No vaccination schedule found.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <%-- Animal Observation & Compliance --%>
            <div class="grid grid-cols-1 xl:grid-cols-2 gap-6">
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex items-center gap-2">
                        <span class="w-2 h-5 rounded bg-orange-400 inline-block"></span>
                        <h3 class="font-extrabold text-slate-800">Animal Observation</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Observation Status:</span> <asp:Label ID="lblAnimalObsStatus" runat="server" CssClass="text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Completion Date:</span> <asp:Label ID="lblAnimalObsDate" runat="server" CssClass="text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Follow-up Notes:</span> <asp:Label ID="lblAnimalObsNotes" runat="server" CssClass="text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Vaccination Continuation:</span> <asp:Label ID="lblVacContinuation" runat="server" CssClass="text-slate-800" /></div>
                    </div>
                </div>
                <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                    <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex items-center gap-2">
                        <span class="w-2 h-5 rounded bg-teal-500 inline-block"></span>
                        <h3 class="font-extrabold text-slate-800">Case Compliance Status</h3>
                    </div>
                    <div class="p-5 grid grid-cols-1 gap-y-3 text-sm">
                        <div><span class="font-semibold text-slate-500">Overall Compliance:</span> <asp:Label ID="lblCompliance" runat="server" CssClass="text-slate-800 font-bold" /></div>
                        <div><span class="font-semibold text-slate-500">Missed Doses:</span> <asp:Label ID="lblMissedDoses" runat="server" CssClass="text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Completion Status:</span> <asp:Label ID="lblCompletionStatus" runat="server" CssClass="text-slate-800" /></div>
                        <div><span class="font-semibold text-slate-500">Compliance Rate:</span> <asp:Label ID="lblComplianceRate" runat="server" CssClass="text-slate-800 font-bold text-emerald-600" /></div>
                    </div>
                </div>
            </div>

            <%-- Treatment Summary --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Treatment Summary</h3>
                    <p class="text-slate-500 text-sm mt-1">Visit history and vaccine administration records</p>
                </div>
                <div class="p-5">
                    <asp:GridView ID="gvTreatmentSummary" runat="server" AutoGenerateColumns="true"
                        CssClass="w-full text-sm" GridLines="None">
                        <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                        <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                        <AlternatingRowStyle CssClass="border-b border-slate-100 bg-slate-50/40" />
                        <EmptyDataTemplate>
                            <div class="p-10 text-center text-slate-400 text-sm">No treatment records found.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </div>

            <%-- Additional Notes --%>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Additional Notes</h3>
                </div>
                <div class="p-5 text-sm text-slate-700">
                    <asp:Label ID="lblAdditionalNotes" runat="server" />
                </div>
            </div>

        </asp:Panel>

    </div>

</asp:Content>
