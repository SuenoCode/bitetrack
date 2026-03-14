<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="Reports.aspx.cs" Inherits="SBI.Reports" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActiveReport" runat="server" Value="CaseSummary" />

    <div class="px-2 py-5 font-sans text-slate-900">

        <!-- Header -->
        <div class="mb-4">
            <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Reports</h1>
            <p class="mt-1 text-base text-slate-600">
                Generate and export comprehensive health informatics reports
            </p>
        </div>

        <!-- Tabs -->
        <div class="mb-4 rounded-2xl border border-slate-200 bg-white shadow-sm p-3">
            <div class="flex flex-wrap gap-3">
                <asp:LinkButton ID="tabCaseSummary" runat="server"
                    CssClass="report-tab rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50"
                    CausesValidation="false"
                    OnClick="tabCaseSummary_Click">
                    Case Summary
                </asp:LinkButton>

                <asp:LinkButton ID="tabHighRisk" runat="server"
                    CssClass="report-tab rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50"
                    CausesValidation="false"
                    OnClick="tabHighRisk_Click">
                    High-Risk Cases
                </asp:LinkButton>

                <asp:LinkButton ID="tabVaxUtil" runat="server"
                    CssClass="report-tab rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50"
                    CausesValidation="false"
                    OnClick="tabVaxUtil_Click">
                    Vaccine Utilization
                </asp:LinkButton>

                <asp:LinkButton ID="tabExpiry" runat="server"
                    CssClass="report-tab rounded-xl border border-slate-300 bg-white px-6 py-3 text-base font-semibold text-slate-800 transition hover:bg-slate-50"
                    CausesValidation="false"
                    OnClick="tabExpiry_Click">
                    Expiry Report
                </asp:LinkButton>
            </div>
        </div>

        <!-- Filter Bar -->
        <div class="mt-4 rounded-2xl bg-[#0b2a7a] p-4 shadow-lg overflow-x-auto">
            <div class="grid min-w-[900px] grid-cols-6 gap-3 items-end">

                <div class="col-span-2 lg:col-span-2">
                    <label class="mb-2 block text-sm font-semibold text-white/90">Report Period</label>
                    <asp:DropDownList ID="ddlReportPeriod" runat="server"
                        AutoPostBack="true"
                        OnSelectedIndexChanged="ddlReportPeriod_SelectedIndexChanged"
                        CssClass="h-11 w-full rounded-md border border-white/25 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem Text="Monthly" Value="Monthly" />
                        <asp:ListItem Text="Weekly" Value="Weekly" />
                        <asp:ListItem Text="Daily" Value="Daily" />
                        <asp:ListItem Text="Custom" Value="Custom" />
                    </asp:DropDownList>
                </div>

                <div class="col-span-2 lg:col-span-1">
                    <label class="mb-2 block text-sm font-semibold text-white/90">From Date</label>
                    <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"
                        CssClass="h-11 w-full rounded-md border border-white/25 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                </div>

                <div class="col-span-2 lg:col-span-1">
                    <label class="mb-2 block text-sm font-semibold text-white/90">To Date</label>
                    <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"
                        CssClass="h-11 w-full rounded-md border border-white/25 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                </div>

                <div class="col-span-3 lg:col-span-1">
                    <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report"
                        OnClick="btnGenerateReport_Click"
                        CssClass="h-11 w-full rounded-md bg-[#1a4ed8] px-4 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition" />
                </div>

                <div class="col-span-3 lg:col-span-1 flex gap-3">
                    <asp:Button ID="btnExportPdf" runat="server" Text="PDF" CausesValidation="false"
                        OnClick="btnExportPdf_Click"
                        CssClass="h-11 flex-1 rounded-md border border-white/70 bg-white px-4 font-extrabold text-red-600 shadow hover:-translate-y-[1px] transition" />
                    <asp:Button ID="btnExportExcel" runat="server" Text="Excel" CausesValidation="false"
                        OnClick="btnExportExcel_Click"
                        CssClass="h-11 flex-1 rounded-md border border-emerald-500/70 bg-white px-4 font-extrabold text-emerald-600 shadow hover:-translate-y-[1px] transition" />
                </div>

            </div>
        </div>

        <!-- Summary Cards -->
        <div class="mt-6 grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="text-sm font-semibold text-slate-500">Active Report</div>
                <div class="mt-2 text-2xl font-extrabold text-[#0b2a7a]">
                    <asp:Label ID="lblActiveReport" runat="server" Text="Case Summary" />
                </div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="text-sm font-semibold text-slate-500">From Date</div>
                <div class="mt-2 text-2xl font-extrabold text-emerald-600">
                    <asp:Label ID="lblFromDate" runat="server" Text="-" />
                </div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="text-sm font-semibold text-slate-500">To Date</div>
                <div class="mt-2 text-2xl font-extrabold text-amber-600">
                    <asp:Label ID="lblToDate" runat="server" Text="-" />
                </div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="text-sm font-semibold text-slate-500">Total Records</div>
                <div class="mt-2 text-2xl font-extrabold text-violet-600">
                    <asp:Label ID="lblTotalRecords" runat="server" Text="0" />
                </div>
            </div>
        </div>

        <!-- Report Panel -->
        <div class="mt-6 rounded-2xl border border-slate-200 bg-white shadow-sm overflow-hidden">
            <div class="flex items-center justify-between px-5 py-4 border-b border-slate-200 bg-slate-50">
                <div>
                    <h4 class="text-lg font-bold text-slate-900">Generated Report</h4>
                    <p class="text-sm text-slate-500">Filtered report data based on selected date range</p>
                </div>
            </div>

            <div class="p-5">
                <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 text-sm font-semibold text-red-600"></asp:Label>

                <div class="overflow-x-auto">
                    <asp:GridView ID="gvReport" runat="server" AutoGenerateColumns="true"
                        CssClass="w-full min-w-[1000px] text-sm text-left text-slate-700"
                        GridLines="None"
                        EmptyDataText="No report data found for the selected date range."
                        HeaderStyle-CssClass="bg-slate-100 text-slate-800"
                        RowStyle-CssClass="border-b border-slate-200 hover:bg-slate-50"
                        AlternatingRowStyle-CssClass="bg-slate-50/50"
                        EmptyDataRowStyle-CssClass="text-center text-slate-500 italic py-6">
                    </asp:GridView>
                </div>
            </div>
        </div>

    </div>

</asp:Content>