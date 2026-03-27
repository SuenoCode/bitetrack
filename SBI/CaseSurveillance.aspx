<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CaseSurveillance.aspx.cs" Inherits="SBI.CaseSurveillance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <div class="p-6 font-heading2 text-slate-900">

        <%-- ── Page Header ───────────────────────────────────────────── --%>
        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl text-[#0b2a7a] font-heading2 tracking-widest">Case Surveillance</h1>
                <p class="text-slate-500 text-sm mt-1">Monitor patient cases, manage protocols, track daily administration, and record animal follow-ups.</p>
            </div>
        </div>

        <asp:HiddenField ID="hfSelectedCaseId" runat="server" />
        <asp:HiddenField ID="hfSelectedScheduleId" runat="server" />
        <asp:HiddenField ID="hfEditMode" runat="server" />
        <asp:HiddenField ID="hfSelectedVisitId" runat="server" />
        <asp:HiddenField ID="hfVisitEditMode" runat="server" />

        <%-- ── Tab Navigation ────────────────────────────────────────── --%>
        <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
            <asp:Button ID="btnTabToday" runat="server" Text="Today's Schedules"
                CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer"
                OnClick="btnTabToday_Click" />
            <asp:Button ID="btnTabRegistry" runat="server" Text="Case Registry"
                CssClass="h-11 rounded-lg bg-white border border-slate-300 px-6 font-bold text-slate-700 hover:bg-slate-50 transition cursor-pointer"
                OnClick="btnTabRegistry_Click" />
        </div>

        <%-- ════════════════════════════════════════════════════════════
             TAB 1 — TODAY'S SCHEDULES
             ════════════════════════════════════════════════════════════ --%>
        <asp:Panel ID="panelTodaySchedules" runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden mb-6">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                    <h3 class="font-extrabold text-slate-800">Due Today</h3>
                    <p class="text-xs text-slate-400 mt-0.5">Patients scheduled for vaccination today</p>
                </div>
                <asp:Button ID="btnRefreshToday" runat="server" Text="Refresh List"
                    CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition"
                    OnClick="btnRefreshToday_Click" />
            </div>

            <asp:GridView ID="gvTodaySchedules" runat="server"
                CssClass="w-full text-sm" GridLines="None"
                AutoGenerateColumns="False"
                DataKeyNames="schedule_id,case_id"
                OnRowCommand="gvTodaySchedules_RowCommand">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                <Columns>
                    <asp:BoundField DataField="case_no" HeaderText="Case No"
                        ItemStyle-CssClass="p-4 text-slate-700"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="patient_name" HeaderText="Patient Name"
                        ItemStyle-CssClass="p-4 font-bold text-slate-700"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="dose_number" HeaderText="Dose"
                        ItemStyle-CssClass="p-4 text-slate-600 text-center"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine" NullDisplayText="-"
                        ItemStyle-CssClass="p-4 text-slate-500 italic text-center"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                        <ItemTemplate>
                            <asp:Button ID="btnGoToCase" runat="server"
    CommandName="ViewCase"
    CommandArgument='<%# Container.DataItemIndex %>'
    Text="Open Case"
    Visible='<%# CanOpenCase %>'
    CssClass="bg-blue-600 hover:bg-blue-700 text-white font-bold py-1.5 px-4 rounded-lg text-xs transition cursor-pointer" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-10 text-center text-slate-400 text-sm">No vaccinations scheduled for today.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </asp:Panel>

        <%-- ════════════════════════════════════════════════════════════
             TAB 2 — CASE REGISTRY
             ════════════════════════════════════════════════════════════ --%>
        <asp:Panel ID="panelRegistrySearch" runat="server" Visible="false"
            CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden mb-6">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                <h3 class="font-extrabold text-slate-800">Case Registry</h3>
                <div class="flex gap-2">
                    <asp:TextBox ID="txtSearchCase" runat="server"
                        CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        Placeholder="Search by patient name, case no…" />
                    <asp:Button ID="btnSearchCase" runat="server" Text="Filter"
                        CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition"
                        OnClick="btnSearchCase_Click" />
                    <asp:Button ID="btnClearCaseSearch" runat="server" Text="Clear"
                        CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition"
                        OnClick="btnClearCaseSearch_Click" />
                </div>
            </div>

            <asp:GridView ID="gvSummary" runat="server"
                CssClass="w-full text-sm" GridLines="None"
                AutoGenerateColumns="False"
                DataKeyNames="case_id"
                OnRowCommand="gvSummary_RowCommand">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                <Columns>
                    <asp:BoundField DataField="case_no" HeaderText="Case No"
                        ItemStyle-CssClass="p-4 text-slate-700"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="patient_name" HeaderText="Patient Name"
                        ItemStyle-CssClass="p-4 font-bold text-slate-700"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="category" HeaderText="Category"
                        ItemStyle-CssClass="p-4 text-slate-600 text-center"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:BoundField DataField="regimen_type" HeaderText="Protocol" NullDisplayText="-"
                        ItemStyle-CssClass="p-4 text-slate-600 text-center"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:BoundField DataField="total_doses" HeaderText="Total" NullDisplayText="-"
                        ItemStyle-CssClass="p-4 text-center text-slate-600"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:BoundField DataField="completed_doses" HeaderText="Completed"
                        ItemStyle-CssClass="p-4 text-center font-extrabold text-emerald-600"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                        <ItemTemplate>
                            <asp:Button ID="btnOpenCase" runat="server"
    CommandName="OpenCase"
    CommandArgument='<%# Container.DataItemIndex %>'
    Text="Manage Case"
    Visible='<%# CanManageCase %>'
    CssClass="inline-flex items-center gap-1 text-blue-600 font-semibold text-xs hover:text-blue-800 hover:underline transition" />
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-10 text-center text-slate-400 text-sm">No cases found matching your criteria.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </asp:Panel>

        <%-- ════════════════════════════════════════════════════════════
             ACTIVE CASE VIEW
             ════════════════════════════════════════════════════════════ --%>
        <asp:Panel ID="panelActiveCase" runat="server" Visible="false">

            <div class="mb-4">
                <asp:LinkButton ID="btnBackToCases" runat="server" OnClick="btnBackToCases_Click"
                    CssClass="text-sm text-blue-600 hover:underline flex items-center gap-1 font-semibold">
                    &larr; Back to Registry
                </asp:LinkButton>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <%-- LEFT: Treatment Details + Animal Follow-Up + Visit Form + Assign Protocol --%>
                <div class="lg:col-span-1 space-y-6">

                    <%-- Animal Follow-Up --%>
                    <asp:Panel ID="panelAnimalFollowUp" runat="server" Visible="false"
                        CssClass="bg-white border border-amber-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-amber-200 bg-amber-50">
                            <h3 class="font-extrabold text-amber-800 flex items-center gap-2">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                </svg>
                                14-Day Animal Follow-Up Due
                            </h3>
                        </div>
                        <div class="p-5 space-y-4">
                            <asp:HiddenField ID="hfAnimalId" runat="server" />
                            <asp:HiddenField ID="hfFollowUpId" runat="server" />
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Animal Status</label>
                                <asp:DropDownList ID="ddlDay14Status" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                    <asp:ListItem Text="-- Select Status --" Value="" />
                                    <asp:ListItem Text="Alive and Healthy" Value="Alive" />
                                    <asp:ListItem Text="Dead / Sick" Value="Dead" />
                                    <asp:ListItem Text="Lost / Unknown" Value="Lost" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Follow-up Date</label>
                                <asp:TextBox ID="txtFollowUpDate" runat="server" TextMode="Date"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Notes</label>
                                <asp:TextBox ID="txtFollowUpNotes" runat="server" TextMode="MultiLine" Rows="2"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                            </div>
                            <asp:Button ID="btnSaveFollowUp" runat="server" Text="Save Follow-Up"
                                CssClass="w-full bg-amber-600 hover:bg-amber-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                OnClick="btnSaveFollowUp_Click" />
                        </div>
                    </asp:Panel>

                    <%-- Case Info --%>
                    <div class="bg-white border border-slate-200 rounded-xl shadow-sm p-5">
                        <h3 class="font-extrabold text-slate-800 border-b border-slate-100 pb-3 mb-4">Treatment Details</h3>
                        <div class="space-y-4">
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Case Reference</label>
                                <div class="text-slate-800 font-semibold">
                                    <asp:Literal ID="litCaseNoDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Patient Name</label>
                                <div class="text-slate-800 font-semibold">
                                    <asp:Literal ID="litPatientNameDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Exposure Category</label>
                                <div class="text-slate-800 font-semibold">
                                    <asp:Literal ID="litCategoryDisplay" runat="server" /></div>
                            </div>
                            <%-- ADD THESE TWO --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Bite Date</label>
                                <div class="text-slate-800 font-semibold">
                                    <asp:Literal ID="litBiteDateDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Initial Diagnosis</label>
                                <div class="text-slate-700 text-sm">
                                    <asp:Literal ID="litInitialDiagnosis" runat="server" /></div>
                            </div>
                        </div>
                    </div>
                    <%-- ── Visit Form ──────────────────────────────────────────── --%>
                    <asp:Panel ID="panelVisitForm" runat="server"
                        CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                            <div>
                                <h3 class="font-extrabold text-slate-800">
                                    <asp:Literal ID="litVisitFormTitle" runat="server" Text="Record Visit" />
                                </h3>
                                <p class="text-xs text-slate-400 mt-0.5">A visit record is required before a schedule can be generated.</p>
                            </div>
                        </div>
                        <div class="p-5 space-y-4">

                            <%-- Visit Type --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Visit Type</label>
                                <asp:DropDownList ID="ddlVisitType" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                    <asp:ListItem Text="-- Select Type --" Value="" />
                                    <asp:ListItem Text="Initial Visit" Value="Initial Visit" />
                                    <asp:ListItem Text="Follow-up" Value="Follow-up" />
                                    <asp:ListItem Text="Booster" Value="Booster" />
                                    <asp:ListItem Text="Pre-Exposure" Value="Pre-Exposure" />
                                </asp:DropDownList>
                            </div>

                            <%-- Dose Day — auto-generated, read-only display --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Dose Day</label>
                                <asp:HiddenField ID="hfVisitDoseDay" runat="server" Value="" />
                                <div class="flex items-center gap-2">
                                    <div id="divDoseDayDisplay" runat="server"
                                        class="flex-1 border border-slate-200 bg-slate-50 rounded-lg px-3 py-2.5 text-sm text-slate-700 font-semibold min-h-[42px]">
                                        <asp:Literal ID="litDoseDayDisplay" runat="server" Text="— select a visit type and date —" />
                                    </div>
                                </div>
                                <p class="text-[11px] text-slate-400 mt-1">Calculated automatically from the visit type and visit date relative to Day 0.</p>
                            </div>

                            <%-- Visit Date --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Visit Date</label>
                                <asp:TextBox ID="txtVisitDate" runat="server" TextMode="Date"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                            </div>

                            <%-- Diagnosis --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Diagnosis</label>
                                <asp:TextBox ID="txtVisitDiagnosis" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder="e.g. Category III Bite" />
                            </div>

                            <%-- Manifestation Notes --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Manifestation Notes</label>
                                <asp:TextBox ID="txtManifestationNotes" runat="server" TextMode="MultiLine" Rows="3"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder="Describe wound, location, severity…" />
                            </div>

                            <%-- Visit Status --%>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Status</label>
                                <asp:DropDownList ID="ddlVisitStatus" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                    <asp:ListItem Text="Completed" Value="Completed" />
                                    <asp:ListItem Text="Pending" Value="Pending" />
                                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                                </asp:DropDownList>
                            </div>

                            <%-- Validation message --%>
                            <asp:Label ID="lblVisitError" runat="server" Visible="false"
                                CssClass="block text-xs text-red-600 font-semibold" />

                            <div class="flex gap-3 pt-1">
                                <asp:Button ID="btnSaveVisit" runat="server" Text="Save Visit"
                                    CssClass="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                    OnClick="btnSaveVisit_Click" />
                                <asp:Button ID="btnCancelVisitEdit" runat="server" Text="Cancel"
                                    CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                    OnClick="btnCancelVisitEdit_Click" Visible="false" />
                            </div>
                        </div>
                    </asp:Panel>

                    <%-- Assign Protocol — locked until a visit exists --%>
                    <asp:Panel ID="panelGenerate" runat="server"
                        CssClass="bg-white border border-slate-200 rounded-xl shadow-sm p-5">
                        <h3 class="font-extrabold text-slate-800 mb-1">Assign Protocol</h3>

                        <%-- Warning shown when no visit recorded yet --%>
                        <asp:Panel ID="panelNoVisitWarning" runat="server" Visible="false"
                            CssClass="mb-4 flex items-start gap-2 bg-amber-50 border border-amber-200 rounded-lg px-4 py-3">
                            <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4 text-amber-600 mt-0.5 shrink-0" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                            </svg>
                            <p class="text-xs text-amber-700 font-semibold">A visit must be recorded before a vaccination schedule can be generated.</p>
                        </asp:Panel>

                        <div class="space-y-4">
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Regimen Protocol</label>
                                <asp:DropDownList ID="ddlProtocol" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                    <asp:ListItem Text="-- Select Protocol --" Value="" />
                                    <asp:ListItem Text="PEP Essen (0, 3, 7, 14, 28)" Value="PEP_ESSEN" />
                                    <asp:ListItem Text="PEP Zagreb (0, 7, 21)" Value="PEP_ZAGREB" />
                                    <asp:ListItem Text="PrEP (0, 7, 21)" Value="PREP" />
                                </asp:DropDownList>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Day 0 Date</label>
                                <asp:TextBox ID="txtDay0" runat="server" TextMode="Date"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                            </div>
                            <asp:Button ID="btnGenerateSchedule" runat="server" Text="Generate Schedule"
                                CssClass="w-full bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                OnClick="btnGenerateSchedule_Click" />
                        </div>
                    </asp:Panel>

                </div>

                <%-- RIGHT: Visit History + Administration Form + Schedule --%>
                <div class="lg:col-span-2 flex flex-col gap-6">

                    <%-- ── Visit History ────────────────────────────────────────── --%>
                    <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                            <div>
                                <h3 class="font-extrabold text-slate-800">Visit History</h3>
                                <p class="text-xs text-slate-400 mt-0.5">All recorded visits for this case</p>
                            </div>
                        </div>

                        <asp:GridView ID="gvVisits" runat="server"
                            CssClass="w-full text-sm" GridLines="None"
                            AutoGenerateColumns="False"
                            DataKeyNames="visit_id"
                            OnRowCommand="gvVisits_RowCommand">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                            <Columns>
                                <asp:BoundField DataField="visit_type" HeaderText="Type"
                                    ItemStyle-CssClass="p-4 font-semibold text-slate-700"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="visit_date" HeaderText="Date"
                                    DataFormatString="{0:MMM dd, yyyy}"
                                    ItemStyle-CssClass="p-4 text-slate-600"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="dose_day" HeaderText="Day"
                                    NullDisplayText="-"
                                    ItemStyle-CssClass="p-4 text-slate-600 text-center"
                                    HeaderStyle-CssClass="p-4 text-center" />
                                <asp:BoundField DataField="diagnosis" HeaderText="Diagnosis"
                                    NullDisplayText="-"
                                    ItemStyle-CssClass="p-4 text-slate-500 text-center"
                                    HeaderStyle-CssClass="p-4 text-center" />
                                <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                    <ItemTemplate>
                                        <span class='<%# Eval("status").ToString() == "Completed"
                                            ? "inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold bg-emerald-100 text-emerald-700 uppercase tracking-wide"
                                            : Eval("status").ToString() == "Cancelled"
                                                ? "inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold bg-red-100 text-red-700 uppercase tracking-wide"
                                                : "inline-flex items-center px-2.5 py-1 rounded-full text-[11px] font-bold bg-amber-100 text-amber-700 uppercase tracking-wide" %>'>
                                            <%# Eval("status") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                                    <ItemTemplate>
                                        <asp:Button ID="btnEditVisit" runat="server"
                                            CommandName="EditVisit"
                                            CommandArgument='<%# Container.DataItemIndex %>'
                                            Text="Edit"
                                            CssClass="bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 font-bold py-1.5 px-3 rounded-lg text-xs transition cursor-pointer" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="p-10 text-center text-slate-400 text-sm">
                                    No visits recorded yet. Use the <strong>Record Visit</strong> form on the left to add one.
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                    <%-- Record Dose Administration --%>
                    <asp:Panel ID="panelAdministration" runat="server" Visible="false"
                        CssClass="bg-white border border-l-4 border-l-blue-600 border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                            <h3 class="font-extrabold text-slate-800">Record Dose Administration</h3>
                        </div>
                        <div class="p-5">
                            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                <div class="col-span-1 lg:col-span-2">
                                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Vaccine</label>
                                    <asp:DropDownList ID="ddlDoseVaccine" runat="server"
                                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white" />
                                </div>
                                <div>
                                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Practitioner</label>
                                    <asp:TextBox ID="txtVaccinatedBy" runat="server"
                                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                                </div>
                                <div class="flex gap-2">
                                    <div class="flex-1">
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Dosage</label>
                                        <asp:TextBox ID="txtDosage" runat="server"
                                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                            placeholder="e.g. 0.5" />
                                    </div>
                                    <div class="flex-1">
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Route</label>
                                        <asp:TextBox ID="txtRoute" runat="server"
                                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                            placeholder="e.g. IM" />
                                    </div>
                                </div>
                            </div>
                            <div class="mt-4 flex gap-3">
                                <asp:Button ID="btnSaveDose" runat="server" Text="Confirm Administration"
                                    CssClass="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                    OnClick="btnSaveDose_Click" />
                                <asp:Button ID="btnCancelDose" runat="server" Text="Cancel"
                                    CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                    OnClick="btnCancelDose_Click" />
                            </div>
                        </div>
                    </asp:Panel>

                    <%-- Overall Schedule --%>
                    <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden flex-1">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                            <h3 class="font-extrabold text-slate-800">Overall Schedule</h3>
                            <asp:Button ID="btnRefreshSchedule" runat="server" Text="Refresh List"
                                CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition"
                                OnClick="btnRefreshSchedule_Click" />
                        </div>

                        <asp:GridView ID="gvSchedule" runat="server"
                            CssClass="w-full text-sm" GridLines="None"
                            AutoGenerateColumns="False"
                            DataKeyNames="schedule_id"
                            OnRowCommand="gvSchedule_RowCommand">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                            <Columns>
                                <asp:BoundField DataField="dose_number" HeaderText="Dose"
                                    ItemStyle-CssClass="p-4 font-bold text-slate-700"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="schedule_date" HeaderText="Date"
                                    DataFormatString="{0:MMM dd, yyyy}"
                                    ItemStyle-CssClass="p-4 text-slate-600"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                                    <ItemTemplate>
                                        <%-- Administer — visible to A and C only, Pending doses only --%>
                                        <asp:Button ID="btnAdminister" runat="server"
    CommandName="AdministerDose"
    CommandArgument='<%# Container.DataItemIndex %>'
    Text="Administer"
    CssClass="bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-1.5 px-3 rounded-lg text-xs transition cursor-pointer"
    Visible='<%# Eval("status").ToString() == "Pending" && CanAdminister %>' />
                                        <%-- Edit — visible to A and C only, Completed doses only --%>
                                        <asp:Button ID="btnEdit" runat="server"
    CommandName="EditDose"
    CommandArgument='<%# Container.DataItemIndex %>'
    Text="Edit"
    CssClass="bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 font-bold py-1.5 px-3 rounded-lg text-xs transition cursor-pointer"
    Visible='<%# Eval("status").ToString() == "Completed" && CanAdminister %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="p-10 text-center text-slate-400 text-sm">No schedule generated for this case yet.</div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                </div>
            </div>
        </asp:Panel>

    </div>

</asp:Content>