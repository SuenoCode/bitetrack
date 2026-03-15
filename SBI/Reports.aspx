<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="SBI.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActiveReport" runat="server" Value="DailyInventorySummary" />

    <div class="p-6 font-heading2 text-slate-900">

        <%-- ── Page Header ───────────────────────────────────────────── --%>
        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl font-bold text-[#0b2a7a] font-hBruns">Reports</h1>
                <p class="text-slate-500 text-sm mt-1">Generate and export daily inventory and activity reports</p>
            </div>
        </div>

        <%-- ── Tab Navigation ────────────────────────────────────────── --%>
        <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
            <asp:LinkButton ID="tabDailyInventory" runat="server"
                CssClass="report-tab h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer"
                CausesValidation="false"
                OnClick="tabDailyInventory_Click">
                Daily Inventory Summary
            </asp:LinkButton>
            <asp:LinkButton ID="tabDailyActivity" runat="server"
                CssClass="report-tab h-11 rounded-lg bg-white border border-slate-300 px-6 font-bold text-slate-700 hover:bg-slate-50 transition cursor-pointer"
                CausesValidation="false"
                OnClick="tabDailyActivity_Click">
                Daily Activity Summary
            </asp:LinkButton>
        </div>

        <%-- ── Stat Cards ─────────────────────────────────────────────── --%>
        <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4 mb-6">
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Active Report</div>
                <div class="text-xl font-extrabold text-[#0b2a7a]"><asp:Label ID="lblActiveReport" runat="server" Text="Daily Inventory Summary" /></div>
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

        <%-- ── Filter Bar ─────────────────────────────────────────────── --%>
        <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden mb-6">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                <h3 class="font-extrabold text-slate-800">Filter Report</h3>
            </div>
            <div class="p-5">
                <div class="flex flex-wrap gap-3 items-end">
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Report Period</label>
                        <asp:DropDownList ID="ddlReportPeriod" runat="server"
                            AutoPostBack="true"
                            OnSelectedIndexChanged="ddlReportPeriod_SelectedIndexChanged"
                            CssClass="border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                            <asp:ListItem Text="Monthly" Value="Monthly" />
                            <asp:ListItem Text="Weekly"  Value="Weekly" />
                            <asp:ListItem Text="Daily"   Value="Daily" />
                            <asp:ListItem Text="Custom"  Value="Custom" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">From Date</label>
                        <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"
                            CssClass="border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">To Date</label>
                        <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"
                            CssClass="border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    </div>
                    <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report"
                        OnClick="btnGenerateReport_Click"
                        CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer" />
                    <asp:Button ID="btnExportPdf" runat="server" Text="Export PDF" CausesValidation="false"
                        OnClick="btnExportPdf_Click"
                        CssClass="h-11 rounded-lg bg-white border border-red-300 text-red-600 px-5 font-bold hover:bg-red-50 transition cursor-pointer shadow-sm" />
                    <asp:Button ID="btnExportExcel" runat="server" Text="Export Excel" CausesValidation="false"
                        OnClick="btnExportExcel_Click"
                        CssClass="h-11 rounded-lg bg-white border border-emerald-300 text-emerald-600 px-5 font-bold hover:bg-emerald-50 transition cursor-pointer shadow-sm" />
                </div>
            </div>
        </asp:Panel>

        <%-- ── Generated Report Table ─────────────────────────────────── --%>
        <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                <h3 class="font-extrabold text-slate-800">Generated Report</h3>
                <p class="text-xs text-slate-400 mt-0.5">Filtered report data based on selected date range</p>
            </div>

            <div class="p-5">
                <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 text-sm font-semibold text-red-600" />

                <asp:GridView ID="gvReport" runat="server" AutoGenerateColumns="true"
                    CssClass="w-full text-sm" GridLines="None"
                    EmptyDataText="No report data found for the selected date range.">
                    <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                    <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                    <AlternatingRowStyle CssClass="border-b border-slate-100 hover:bg-slate-50" />
                    <EmptyDataRowStyle CssClass="text-center text-slate-400 italic py-8" />
                </asp:GridView>
            </div>
        </asp:Panel>

    </div>

</asp:Content>
