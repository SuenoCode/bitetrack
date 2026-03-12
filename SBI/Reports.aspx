<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="VaccineManagement.aspx.cs" Inherits="SBI.VaccineManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="px-2 py-5 font-sans text-slate-900">

        <!-- Header -->
        <div class="mb-4">
            <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Reports</h1>
            <p class="mt-1 text-base text-slate-600">
                Generate and export comprehensive health informatics reports
            </p>
        </div>

        <!-- Tabs -->
        <div class="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
            <!-- Active -->
            <asp:LinkButton ID="tabCaseSummary" runat="server"
                CssClass="flex items-center justify-center gap-2 rounded-lg border border-blue-200 bg-white px-4 py-3 font-semibold text-[#0b2a7a] shadow-sm ring-2 ring-blue-100"
                CausesValidation="false">
                <span class="text-lg"></span> Case Summary
            </asp:LinkButton>

            <asp:LinkButton ID="tabHighRisk" runat="server"
                CssClass="flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                CausesValidation="false">
                <span class="text-lg"></span> High-Risk Cases
            </asp:LinkButton>

            <asp:LinkButton ID="tabVaxUtil" runat="server"
                CssClass="flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                CausesValidation="false">
                <span class="text-lg"></span> Vaccine Utilization
            </asp:LinkButton>

            <asp:LinkButton ID="tabExpiry" runat="server"
                CssClass="flex items-center justify-center gap-2 rounded-lg border border-slate-200 bg-white px-4 py-3 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                CausesValidation="false">
                <span class="text-lg"></span> Expiry Report
            </asp:LinkButton>
        </div>

        <!-- Filter Bar -->
        <div class="mt-4 rounded-lg bg-[#0b2a7a] p-4 shadow-lg overflow-x-auto">
            <div class="grid min-w-[900px] grid-cols-6 gap-3 items-end">

                <!-- Report Period -->
                <div class="col-span-2 lg:col-span-2">
                    <label class="mb-2 block text-sm font-semibold text-white/90">Report Period</label>
                    <asp:DropDownList ID="ddlReportPeriod" runat="server"
                        CssClass="h-11 w-full rounded-md border border-white/25 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem Text="Monthly" Value="Monthly" />
                        <asp:ListItem Text="Weekly" Value="Weekly" />
                        <asp:ListItem Text="Daily" Value="Daily" />
                        <asp:ListItem Text="Custom" Value="Custom" />
                    </asp:DropDownList>
                </div>

                <!-- From -->
                <div class="col-span-2 lg:col-span-1">
                    <label class="mb-2 block text-sm font-semibold text-white/90">From Date</label>
                    <asp:TextBox ID="txtFromDate" runat="server" TextMode="Date"
                        CssClass="h-11 w-full rounded-md border border-white/25 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                </div>

                <!-- To -->
                <div class="col-span-2 lg:col-span-1">
                    <label class="mb-2 block text-sm font-semibold text-white/90">To Date</label>
                    <asp:TextBox ID="txtToDate" runat="server" TextMode="Date"
                        CssClass="h-11 w-full rounded-md border border-white/25 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                </div>

                <!-- Generate -->
                <div class="col-span-3 lg:col-span-1">
                    <asp:Button ID="btnGenerateReport" runat="server" Text="Generate Report"
                        CssClass="h-11 w-full rounded-md bg-[#1a4ed8] px-4 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition" />
                </div>

                <!-- Exports -->
                <div class="col-span-3 lg:col-span-1 flex gap-3">
                    <asp:Button ID="btnExportPdf" runat="server" Text="PDF" CausesValidation="false"
                        CssClass="h-11 flex-1 rounded-md border border-white/70 bg-white px-4 font-extrabold text-red-600 shadow hover:-translate-y-[1px] transition" />
                    <asp:Button ID="btnExportExcel" runat="server" Text="Excel" CausesValidation="false"
                        CssClass="h-11 flex-1 rounded-md border border-emerald-500/70 bg-white px-4 font-extrabold text-emerald-600 shadow hover:-translate-y-[1px] transition" />
                </div>

            </div>
        </div>

        <!-- Your existing content below -->
        <div class="mt-6">
            <!-- Paste your Vaccine Management cards/table here -->
        </div>

    </div>

</asp:Content>