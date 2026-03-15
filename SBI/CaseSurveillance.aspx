<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CaseSurveillance.aspx.cs" Inherits="SBI.CaseSurveillance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<div class="px-4 py-6 font-sans text-slate-900">

    <asp:HiddenField ID="hfSelectedCaseId" runat="server" />
    <asp:HiddenField ID="hfSelectedScheduleId" runat="server" />
    <asp:HiddenField ID="hfEditMode" runat="server" Value="Add" />

    <h1 class="text-4xl font-extrabold text-[#0b2a7a]">Case Monitoring</h1>
    <p class="text-slate-600 mt-1">
        Monitor vaccination schedules, compliance, and dose administration
    </p>

    <!-- SUMMARY PANEL -->
    <asp:Panel ID="panelSummary" runat="server" CssClass="mt-6">
        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm">
            <div class="px-5 py-4 border-b border-slate-200">
                <h3 class="text-lg font-extrabold text-slate-900">Case Vaccination Summary</h3>
            </div>

            <div class="px-5 py-4 border-b border-slate-200 flex gap-3 flex-wrap">
                <asp:TextBox ID="txtSearchCase" runat="server"
                    placeholder="Search case no, patient name..."
                    CssClass="h-11 flex-1 min-w-[260px] rounded-lg border border-slate-200 px-3 text-sm"></asp:TextBox>

                <asp:Button ID="btnSearchCase" runat="server"
                    Text="Search"
                    OnClick="btnSearchCase_Click"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 transition" />

                <asp:Button ID="btnClearCaseSearch" runat="server"
                    Text="Clear"
                    OnClick="btnClearCaseSearch_Click"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-5 font-semibold text-slate-700 shadow-sm hover:shadow-md transition" />
            </div>

            <div class="overflow-x-auto p-4">
                <asp:GridView ID="gvSummary" runat="server"
                    AutoGenerateColumns="False"
                    ShowHeaderWhenEmpty="true"
                    EmptyDataText="No cases found."
                    DataKeyNames="case_id"
                    OnRowCommand="gvSummary_RowCommand"
                    CssClass="w-full text-sm">
                    <Columns>
                        <asp:BoundField DataField="case_no" HeaderText="Case No" />
                        <asp:BoundField DataField="patient_name" HeaderText="Patient" />
                        <asp:BoundField DataField="category" HeaderText="Category" />
                        <asp:BoundField DataField="regimen_type" HeaderText="Protocol" />
                        <asp:BoundField DataField="total_doses" HeaderText="Total Doses" />
                        <asp:BoundField DataField="completed_doses" HeaderText="Completed" />
                        <asp:BoundField DataField="pending_doses" HeaderText="Pending" />
                        <asp:BoundField DataField="missed_doses" HeaderText="Missed" />
                        <asp:ButtonField Text="Open" CommandName="OpenCase" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </asp:Panel>

    <!-- GENERATE PANEL -->
    <asp:Panel ID="panelGenerate" runat="server" CssClass="mt-6" Visible="false">
        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm">
            <div class="px-5 py-4 border-b border-slate-200">
                <h3 class="text-lg font-extrabold text-slate-900">Generate Schedule</h3>
                <p class="text-sm text-slate-500 mt-1">Create a vaccination schedule for the selected case.</p>
            </div>

            <div class="p-5 grid grid-cols-1 md:grid-cols-3 gap-4">
                <div>
                    <label class="text-sm font-semibold text-slate-700">Case No</label>
                    <asp:TextBox ID="txtCaseNoDisplay" runat="server" ReadOnly="true"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-sm"></asp:TextBox>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Protocol</label>
                    <asp:DropDownList ID="ddlProtocol" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3">
                        <asp:ListItem Text="-- Select Protocol --" Value="" />
                        <asp:ListItem Text="PEP - Essen" Value="PEP_ESSEN" />
                        <asp:ListItem Text="PEP - Zagreb" Value="PEP_ZAGREB" />
                        <asp:ListItem Text="PREP" Value="PREP" />
                    </asp:DropDownList>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Day 0 Date</label>
                    <asp:TextBox ID="txtDay0" runat="server" TextMode="Date"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3"></asp:TextBox>
                </div>
            </div>

            <div class="px-5 pb-5 flex justify-end gap-3">
                <asp:Button ID="btnGenerateSchedule" runat="server"
                    Text="Generate Schedule"
                    OnClick="btnGenerateSchedule_Click"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-6 font-extrabold text-white shadow hover:brightness-110 transition" />

                <asp:Button ID="btnCloseGenerate" runat="server"
                    Text="Close"
                    OnClick="btnCloseGenerate_Click"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-6 font-semibold text-slate-700 shadow-sm hover:shadow-md transition" />
            </div>
        </div>
    </asp:Panel>

    <!-- SCHEDULE PANEL -->
    <asp:Panel ID="panelSchedule" runat="server" CssClass="mt-6" Visible="false">
        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm">
            <div class="px-5 py-4 border-b border-slate-200 flex items-center justify-between">
                <div>
                    <h3 class="text-lg font-extrabold text-slate-900">Generated Schedule</h3>
                    <p class="text-sm text-slate-500 mt-1">Assigned vaccine and batch are automatically selected using FIFO.</p>
                </div>

                <asp:Button ID="btnRefreshSchedule" runat="server"
                    Text="Refresh"
                    OnClick="btnRefreshSchedule_Click"
                    CssClass="h-10 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm hover:shadow-md transition" />
            </div>

            <div class="overflow-x-auto p-4">
                <asp:GridView ID="gvSchedule" runat="server"
                    AutoGenerateColumns="False"
                    ShowHeaderWhenEmpty="true"
                    EmptyDataText="No generated schedule yet."
                    DataKeyNames="schedule_id"
                    OnRowCommand="gvSchedule_RowCommand"
                    CssClass="w-full text-sm">
                    <Columns>
                        <asp:BoundField DataField="dose_number" HeaderText="Dose #" />
                        <asp:BoundField DataField="schedule_date" HeaderText="Schedule Date" DataFormatString="{0:MMM dd, yyyy}" />
                        <asp:BoundField DataField="status" HeaderText="Status" />
                        <asp:BoundField DataField="vaccine_name" HeaderText="Assigned Vaccine" />
                        <asp:BoundField DataField="batch_number" HeaderText="Batch" />
                        <asp:BoundField DataField="expiration_date" HeaderText="Expiry" DataFormatString="{0:MMM dd, yyyy}" />
                        <asp:BoundField DataField="vaccinated_by" HeaderText="Vaccinated By" />
                        <asp:ButtonField Text="Administer" CommandName="AdministerDose" />
                        <asp:ButtonField Text="Edit" CommandName="EditDose" />
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </asp:Panel>

    <!-- ADMINISTER / EDIT PANEL -->
    <asp:Panel ID="panelAdministration" runat="server" CssClass="mt-6" Visible="false">
        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm">
            <div class="px-5 py-4 border-b border-slate-200">
                <h3 class="text-lg font-extrabold text-slate-900">Dose Administration</h3>
                <p class="text-sm text-slate-500 mt-1">Use this panel to administer or modify a scheduled dose.</p>
            </div>

            <div class="p-5 grid grid-cols-1 md:grid-cols-5 gap-4">
                <div>
                    <label class="text-sm font-semibold text-slate-700">Vaccine</label>
                    <asp:DropDownList ID="ddlDoseVaccine" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3"></asp:DropDownList>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Dosage</label>
                    <asp:TextBox ID="txtDosage" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3"></asp:TextBox>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Unit</label>
                    <asp:TextBox ID="txtUnit" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3"></asp:TextBox>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Route</label>
                    <asp:TextBox ID="txtRoute" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3"></asp:TextBox>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Vaccinated By</label>
                    <asp:TextBox ID="txtVaccinatedBy" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 px-3"></asp:TextBox>
                </div>
            </div>

            <div class="px-5 pb-5 flex justify-end gap-3">
                <asp:Button ID="btnSaveDose" runat="server"
                    Text="Save"
                    OnClick="btnSaveDose_Click"
                    CssClass="h-11 rounded-lg bg-emerald-600 px-6 font-extrabold text-white shadow hover:brightness-110 transition" />

                <asp:Button ID="btnCancelDose" runat="server"
                    Text="Cancel"
                    OnClick="btnCancelDose_Click"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-6 font-semibold text-slate-700 shadow-sm hover:shadow-md transition" />
            </div>
        </div>
    </asp:Panel>

</div>

</asp:Content>