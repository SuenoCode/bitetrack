<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CaseSurveillance.aspx.cs" Inherits="SBI.CaseSurveillance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

    <style>
        .badge { display:inline-flex; align-items:center; gap:5px; padding:3px 10px; border-radius:999px; font-size:11px; font-weight:700; letter-spacing:.4px; text-transform:uppercase; }
        .badge-ok   { background:#dcfce7; color:#15803d; }
        .badge-warn { background:#fef9c3; color:#a16207; }
        .badge-exp  { background:#fee2e2; color:#b91c1c; }
        .badge-in   { background:#dbeafe; color:#1d4ed8; }

        .schedule-summary {
            display: flex;
            gap: 24px;
            flex-wrap: wrap;
        }
        .schedule-summary .stat {
            display: flex;
            flex-direction: column;
        }
        .schedule-summary .stat .label {
            font-size: 10px;
            font-weight: 700;
            color: #94a3b8;
            text-transform: uppercase;
            letter-spacing: .5px;
        }
        .schedule-summary .stat .value {
            font-size: 20px;
            font-weight: 800;
        }
        .schedule-summary .stat .value.total { color: #1e293b; }
        .schedule-summary .stat .value.completed { color: #15803d; }
        .schedule-summary .stat .value.pending { color: #b45309; }
        .schedule-summary .stat .value.cancelled { color: #b91c1c; }

        @keyframes fadeIn {
            from { opacity:0; transform:translateY(8px) scale(.98); }
            to   { opacity:1; transform:translateY(0) scale(1); }
        }

        .info-box {
            background: #eff6ff;
            border: 1px solid #bfdbfe;
            border-radius: 8px;
            padding: 12px 16px;
            display: flex;
            align-items: flex-start;
            gap: 10px;
        }
        .info-box .icon {
            color: #2563eb;
            font-size: 18px;
            flex-shrink: 0;
            margin-top: 1px;
        }
        .info-box .text {
            font-size: 13px;
            color: #1e40af;
        }
        .info-box .text strong {
            font-weight: 700;
        }

        .admin-card {
            border-left: 4px solid #f59e0b;
        }

        .pagination-container {
            display: flex;
            justify-content: center;
            align-items: center;
            gap: 8px;
            padding: 12px 16px;
            border-top: 1px solid #e2e8f0;
            background: #f8fafc;
        }
        .pagination-container .page-btn {
            background: white;
            border: 1px solid #e2e8f0;
            padding: 4px 12px;
            border-radius: 6px;
            font-size: 13px;
            font-weight: 600;
            color: #475569;
            cursor: pointer;
            transition: all 0.2s;
        }
        .pagination-container .page-btn:hover:not(:disabled) {
            background: #f1f5f9;
        }
        .pagination-container .page-btn:disabled {
            opacity: 0.4;
            cursor: not-allowed;
        }
        .pagination-container .page-btn.active {
            background: #2563eb;
            color: white;
            border-color: #2563eb;
        }
        .pagination-container .page-info {
            font-size: 13px;
            color: #64748b;
            padding: 0 8px;
        }

        .summary-stats {
            display: grid;
            grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
            gap: 12px;
            margin-bottom: 16px;
        }
        .summary-stats .stat-card {
            background: white;
            border: 1px solid #e2e8f0;
            border-radius: 8px;
            padding: 10px 14px;
            text-align: center;
        }
        .summary-stats .stat-card .stat-num {
            font-size: 22px;
            font-weight: 800;
        }
        .summary-stats .stat-card .stat-label {
            font-size: 10px;
            font-weight: 700;
            color: #94a3b8;
            text-transform: uppercase;
            letter-spacing: .5px;
        }
        .summary-stats .stat-card.cancelled .stat-num { color: #b91c1c; }
        .summary-stats .stat-card.missed .stat-num { color: #b45309; }
        .summary-stats .stat-card.completed .stat-num { color: #15803d; }
        .summary-stats .stat-card.pending .stat-num { color: #1d4ed8; }

        .protocol-badge {
            display: inline-block;
            padding: 2px 10px;
            border-radius: 12px;
            font-size: 10px;
            font-weight: 700;
            text-transform: uppercase;
            letter-spacing: 0.3px;
        }
        .protocol-badge.zagreb { background: #fef3c7; color: #92400e; }
        .protocol-badge.essen { background: #dbeafe; color: #1e40af; }
        .protocol-badge.prep { background: #d1fae5; color: #065f46; }
    </style>

    <div class="p-6 font-heading2 text-slate-900">

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
        <asp:HiddenField ID="hfAnimalId" runat="server" />
        <asp:HiddenField ID="hfFollowUpId" runat="server" />
        <asp:HiddenField ID="hfConfirmVaccineId" runat="server" />
        <asp:HiddenField ID="hfConfirmBatchId" runat="server" />
        <asp:HiddenField ID="hfTodayPage" runat="server" Value="1" />
        <asp:HiddenField ID="hfCasePage" runat="server" Value="1" />
        <asp:HiddenField ID="hfTodayPageSize" runat="server" Value="10" />
        <asp:HiddenField ID="hfCasePageSize" runat="server" Value="10" />

        <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
            <asp:Button ID="btnTabToday" runat="server" Text="Today's Schedules"
                CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer"
                OnClick="btnTabToday_Click" />
            <asp:Button ID="btnTabRegistry" runat="server" Text="Case Registry"
                CssClass="h-11 rounded-lg bg-white border border-slate-300 px-6 font-bold text-slate-700 hover:bg-slate-50 transition cursor-pointer"
                OnClick="btnTabRegistry_Click" />
        </div>

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
                    <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine" NullDisplayText="—"
                        ItemStyle-CssClass="p-4 text-slate-600 text-center"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:TemplateField HeaderText="Category" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                        <ItemTemplate>
                            <span class='badge <%# Eval("category").ToString() == "III" ? "badge-exp" : Eval("category").ToString() == "II" ? "badge-warn" : "badge-ok" %>'>
                                Cat <%# Eval("category") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Protocol" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                        <ItemTemplate>
                            <span class='protocol-badge <%# GetProtocolClass(Eval("regimen_type").ToString()) %>'>
                                <%# Eval("regimen_type") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                        <ItemTemplate>
                            <span class='badge badge-warn'>Pending</span>
                        </ItemTemplate>
                    </asp:TemplateField>
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

            <div class="pagination-container">
                <asp:LinkButton ID="btnTodayPrev" runat="server" CssClass="page-btn" OnClick="btnTodayPrev_Click">Previous</asp:LinkButton>
                <span class="page-info"><asp:Literal ID="litTodayPageInfo" runat="server" Text="Page 1 of 1" /></span>
                <asp:LinkButton ID="btnTodayNext" runat="server" CssClass="page-btn" OnClick="btnTodayNext_Click">Next</asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="panelRegistrySearch" runat="server" Visible="false"
            CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden mb-6">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                <h3 class="font-extrabold text-slate-800">Case Registry</h3>
                <div class="flex gap-2 flex-wrap">
                    <asp:TextBox ID="txtSearchCase" runat="server"
                        CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        Placeholder="Search by patient name, case no..." />
                    <asp:Button ID="btnSearchCase" runat="server" Text="Filter"
                        CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition"
                        OnClick="btnSearchCase_Click" />
                    <asp:Button ID="btnClearCaseSearch" runat="server" Text="Clear"
                        CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition"
                        OnClick="btnClearCaseSearch_Click" />
                </div>
            </div>

            <div class="px-5 py-4">
                <div class="summary-stats">
                    <div class="stat-card completed">
                        <div class="stat-num"><asp:Literal ID="litStatCompleted" runat="server" Text="0" /></div>
                        <div class="stat-label">Completed</div>
                    </div>
                    <div class="stat-card pending">
                        <div class="stat-num"><asp:Literal ID="litStatPending" runat="server" Text="0" /></div>
                        <div class="stat-label">In Progress</div>
                    </div>
                    <div class="stat-card cancelled">
                        <div class="stat-num"><asp:Literal ID="litStatCancelled" runat="server" Text="0" /></div>
                        <div class="stat-label">Cancelled</div>
                    </div>
                    <div class="stat-card missed">
                        <div class="stat-num"><asp:Literal ID="litStatMissed" runat="server" Text="0" /></div>
                        <div class="stat-label">Missed (No Show)</div>
                    </div>
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
                    <asp:TemplateField HeaderText="Category" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                        <ItemTemplate>
                            <span class='badge <%# Eval("category").ToString() == "III" ? "badge-exp" : Eval("category").ToString() == "II" ? "badge-warn" : "badge-ok" %>'>
                                Cat <%# Eval("category") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="date_of_bite" HeaderText="Bite Date"
                        DataFormatString="{0:MMM dd, yyyy}"
                        ItemStyle-CssClass="p-4 text-slate-600"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="regimen_type" HeaderText="Protocol" NullDisplayText="—"
                        ItemStyle-CssClass="p-4 text-slate-600"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="total_doses" HeaderText="Total" NullDisplayText="—"
                        ItemStyle-CssClass="p-4 text-center text-slate-600"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:BoundField DataField="completed_doses" HeaderText="Completed"
                        ItemStyle-CssClass="p-4 text-center font-extrabold text-emerald-600"
                        HeaderStyle-CssClass="p-4 text-center" />
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                        <ItemTemplate>
                            <span class='badge <%# Eval("case_status").ToString() == "Complete" ? "badge-ok" : Eval("case_status").ToString() == "No Schedule" ? "badge-exp" : "badge-warn" %>'>
                                <%# Eval("case_status") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
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

            <div class="pagination-container">
                <asp:LinkButton ID="btnCasePrev" runat="server" CssClass="page-btn" OnClick="btnCasePrev_Click">Previous</asp:LinkButton>
                <span class="page-info"><asp:Literal ID="litCasePageInfo" runat="server" Text="Page 1 of 1" /></span>
                <asp:LinkButton ID="btnCaseNext" runat="server" CssClass="page-btn" OnClick="btnCaseNext_Click">Next</asp:LinkButton>
            </div>
        </asp:Panel>

        <asp:Panel ID="panelActiveCase" runat="server" Visible="false">

            <div class="mb-4">
                <asp:LinkButton ID="btnBackToCases" runat="server" OnClick="btnBackToCases_Click"
                    CssClass="text-sm text-blue-600 hover:underline flex items-center gap-1 font-semibold">
                    &larr; Back to Registry
                </asp:LinkButton>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

                <div class="lg:col-span-1 space-y-6">

                    <asp:Panel ID="panelScheduleInfo" runat="server" Visible="true"
                        CssClass="bg-white border border-emerald-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-emerald-200 bg-emerald-50">
                            <h3 class="font-extrabold text-emerald-800 flex items-center gap-2">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                Vaccination Schedule
                            </h3>
                            <p class="text-xs text-emerald-700 mt-0.5">Auto-generated based on bite category</p>
                        </div>
                        <div class="p-5">
                            <div class="schedule-summary">
                                <div class="stat">
                                    <span class="label">Total Doses</span>
                                    <span class="value total"><asp:Literal ID="litTotalDoses" runat="server" Text="0" /></span>
                                </div>
                                <div class="stat">
                                    <span class="label">Completed</span>
                                    <span class="value completed"><asp:Literal ID="litCompletedDoses" runat="server" Text="0" /></span>
                                </div>
                                <div class="stat">
                                    <span class="label">Pending</span>
                                    <span class="value pending"><asp:Literal ID="litPendingDoses" runat="server" Text="0" /></span>
                                </div>
                                <div class="stat">
                                    <span class="label">Cancelled</span>
                                    <span class="value cancelled"><asp:Literal ID="litCancelledDoses" runat="server" Text="0" /></span>
                                </div>
                            </div>
                            <div class="mt-3 text-xs text-slate-500">
                                <span class="font-semibold">Protocol:</span>
                                <asp:Literal ID="litProtocolDisplay" runat="server" Text="—" />
                                <span class="ml-2 text-emerald-600 font-semibold">
                                    <asp:Literal ID="litProtocolDoses" runat="server" Text="" />
                                </span>
                            </div>
                        </div>
                    </asp:Panel>

                    <div class="bg-white border border-slate-200 rounded-xl shadow-sm p-5">
                        <h3 class="font-extrabold text-slate-800 border-b border-slate-100 pb-3 mb-4">Case Details</h3>
                        <div class="space-y-3">
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Case Reference</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litCaseNoDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Patient Name</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litPatientNameDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Exposure Category</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litCategoryDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Bite Date</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litBiteDateDisplay" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Animal Type</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litAnimalType" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Site of Bite</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litSiteOfBite" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Wound Type</label>
                                <div class="text-slate-800 font-semibold"><asp:Literal ID="litWoundType" runat="server" /></div>
                            </div>
                            <div>
                                <label class="block text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Initial Diagnosis</label>
                                <div class="text-slate-700 text-sm"><asp:Literal ID="litInitialDiagnosis" runat="server" /></div>
                            </div>
                        </div>
                    </div>

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

                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Visit Type <span class="text-red-500">*</span></label>
                                <asp:DropDownList ID="ddlVisitType" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                    <asp:ListItem Text="-- Select Type --" Value="" />
                                    <asp:ListItem Text="Initial Visit" Value="Initial Visit" />
                                    <asp:ListItem Text="Follow-up" Value="Follow-up" />
                                    <asp:ListItem Text="Booster" Value="Booster" />
                                    <asp:ListItem Text="Pre-Exposure" Value="Pre-Exposure" />
                                </asp:DropDownList>
                            </div>

                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Dose Day</label>
                                <asp:HiddenField ID="hfVisitDoseDay" runat="server" Value="" />
                                <div class="flex items-center gap-2">
                                    <div id="divDoseDayDisplay" runat="server"
                                        class="flex-1 border border-slate-200 bg-slate-50 rounded-lg px-3 py-2.5 text-sm text-slate-700 font-semibold min-h-[42px]">
                                        <asp:Literal ID="litDoseDayDisplay" runat="server" Text="— select a visit type and date —" />
                                    </div>
                                </div>
                                <p class="text-[11px] text-slate-400 mt-1">Calculated automatically from the visit date relative to the bite date.</p>
                            </div>

                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Visit Date <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtVisitDate" runat="server" TextMode="Date"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                            </div>

                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Diagnosis</label>
                                <asp:TextBox ID="txtVisitDiagnosis" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder="e.g. Category III Bite" />
                            </div>

                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Manifestation Notes</label>
                                <asp:TextBox ID="txtManifestationNotes" runat="server" TextMode="MultiLine" Rows="3"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                    placeholder="Describe wound, location, severity..." />
                            </div>

                            <div>
                                <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Status</label>
                                <asp:DropDownList ID="ddlVisitStatus" runat="server"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                    <asp:ListItem Text="Completed" Value="Completed" />
                                    <asp:ListItem Text="Pending" Value="Pending" />
                                    <asp:ListItem Text="Cancelled" Value="Cancelled" />
                                </asp:DropDownList>
                            </div>

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

                </div>

                <div class="lg:col-span-2 flex flex-col gap-6">

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
                                    NullDisplayText="—"
                                    ItemStyle-CssClass="p-4 text-slate-600 text-center"
                                    HeaderStyle-CssClass="p-4 text-center" />
                                <asp:BoundField DataField="diagnosis" HeaderText="Diagnosis"
                                    NullDisplayText="—"
                                    ItemStyle-CssClass="p-4 text-slate-600"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                                    <ItemTemplate>
                                        <span class='badge <%# Eval("status").ToString() == "Completed" ? "badge-ok" : Eval("status").ToString() == "Cancelled" ? "badge-exp" : "badge-warn" %>'>
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

                    <asp:Panel ID="panelAdministration" runat="server" Visible="false"
                        CssClass="bg-white border border-l-4 border-l-emerald-600 border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-emerald-50">
                            <h3 class="font-extrabold text-emerald-800 flex items-center gap-2">
                                <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                Confirm Dose Administration
                            </h3>
                            <p class="text-xs text-emerald-700 mt-0.5">Vial has been automatically reserved. Confirm to administer.</p>
                        </div>
                        <div class="p-5">
                            <div class="bg-emerald-50 border border-emerald-200 rounded-lg p-4 mb-4">
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Vaccine</label>
                                        <div class="text-sm font-bold text-slate-800">
                                            <asp:Literal ID="litConfirmVaccine" runat="server" Text="—" />
                                        </div>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Batch / Vial</label>
                                        <div class="text-sm font-bold text-slate-800">
                                            <asp:Literal ID="litConfirmBatch" runat="server" Text="—" />
                                        </div>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Practitioner</label>
                                        <div class="text-sm font-bold text-slate-800">
                                            <asp:Literal ID="litConfirmPractitioner" runat="server" Text="—" />
                                        </div>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Dose Number</label>
                                        <div class="text-sm font-bold text-slate-800">
                                            <asp:Literal ID="litConfirmDoseNumber" runat="server" Text="—" />
                                        </div>
                                    </div>
                                </div>
                            </div>
                            
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div>
                                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Dosage (mL)</label>
                                    <asp:TextBox ID="txtConfirmDosage" runat="server"
                                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                                        placeholder="e.g. 0.5" />
                                </div>
                                <div>
                                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Route</label>
                                    <asp:DropDownList ID="ddlConfirmRoute" runat="server"
                                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                        <asp:ListItem Text="-- Select Route --" Value="" />
                                        <asp:ListItem Text="IM (Intramuscular)" Value="IM" />
                                        <asp:ListItem Text="ID (Intradermal)" Value="ID" />
                                        <asp:ListItem Text="SC (Subcutaneous)" Value="SC" />
                                        <asp:ListItem Text="IV (Intravenous)" Value="IV" />
                                    </asp:DropDownList>
                                </div>
                            </div>

                            <div class="mt-4 flex gap-3">
                                <asp:Button ID="btnConfirmDose" runat="server" Text="Confirm Administration"
                                    CssClass="flex-1 bg-emerald-600 hover:bg-emerald-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                    OnClick="btnConfirmDose_Click" />
                                <asp:Button ID="btnCancelConfirmDose" runat="server" Text="Cancel"
                                    CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                    OnClick="btnCancelDose_Click" />
                            </div>
                        </div>
                    </asp:Panel>

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
                                    ItemStyle-CssClass="p-4 font-bold text-slate-700 text-center"
                                    HeaderStyle-CssClass="p-4 text-center" />
                                <asp:BoundField DataField="schedule_date" HeaderText="Scheduled Date"
                                    DataFormatString="{0:MMM dd, yyyy}"
                                    ItemStyle-CssClass="p-4 text-slate-600"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine"
                                    NullDisplayText="—"
                                    ItemStyle-CssClass="p-4 text-slate-600"
                                    HeaderStyle-CssClass="p-4" />
                                <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="p-4 text-center" ItemStyle-CssClass="p-4 text-center">
                                    <ItemTemplate>
                                        <span class='badge <%# Eval("status").ToString() == "Completed" ? "badge-ok" : Eval("status").ToString() == "Cancelled" ? "badge-exp" : "badge-warn" %>'>
                                            <%# Eval("status") %>
                                        </span>
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                                    <ItemTemplate>
                                        <asp:Button ID="btnAdminister" runat="server"
                                            CommandName="AdministerDose"
                                            CommandArgument='<%# Container.DataItemIndex %>'
                                            Text="Administer"
                                            CssClass="bg-emerald-600 hover:bg-emerald-700 text-white font-bold py-1.5 px-3 rounded-lg text-xs transition cursor-pointer"
                                            Visible='<%# Eval("status").ToString() == "Pending" && CanAdminister %>' />
                                    </ItemTemplate>
                                </asp:TemplateField>
                            </Columns>
                            <EmptyDataTemplate>
                                <div class="p-10 text-center text-slate-400 text-sm">
                                    <p>No schedule generated for this case yet.</p>
                                    <p class="text-xs mt-1">The schedule is automatically generated when the case is opened.</p>
                                </div>
                            </EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                </div>
            </div>
        </asp:Panel>

    </div>

    <div id="notifyModal" class="fixed inset-0 z-[200] hidden items-center justify-center bg-slate-900/50 px-4">
        <div class="w-full max-w-sm rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden" style="animation:fadeIn .2s ease-out;">
            <div id="notifyModalHeader" class="px-6 py-4 border-b flex items-start gap-4">
                <div id="notifyModalIconWrap" class="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                    <img id="notifyModalIcon" src="<%= ResolveUrl("~/Icons/warning.svg") %>" alt="icon" class="w-5 h-5" />
                </div>
                <div class="flex-1 min-w-0">
                    <h3 id="notifyModalTitle" class="font-extrabold text-slate-900 text-base"></h3>
                    <p id="notifyModalMessage" class="text-sm text-slate-600 mt-1 leading-relaxed whitespace-pre-line"></p>
                </div>
            </div>
            <div class="px-6 py-4 flex justify-end">
                <button type="button" onclick="hideNotifyModal()" id="notifyModalBtn"
                    class="px-6 py-2 rounded-lg text-sm font-bold cursor-pointer transition text-white">
                    OK
                </button>
            </div>
        </div>
    </div>

    <script type="text/javascript">
        var _iconSrc = '<%= ResolveUrl("~/Icons/warning.svg") %>';

        function showNotifyModal(message, type) {
            type = type || 'info';
            var hdr = document.getElementById('notifyModalHeader');
            var wrap = document.getElementById('notifyModalIconWrap');
            var icon = document.getElementById('notifyModalIcon');
            var ttl = document.getElementById('notifyModalTitle');
            var msg = document.getElementById('notifyModalMessage');
            var btn = document.getElementById('notifyModalBtn');

            icon.src = _iconSrc;
            hdr.className = 'px-6 py-4 border-b flex items-start gap-4';
            wrap.className = 'w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5';
            icon.className = 'w-5 h-5';
            btn.className = 'px-6 py-2 rounded-lg text-sm font-bold cursor-pointer transition text-white';
            btn.onclick = hideNotifyModal;

            if (type === 'success') {
                hdr.className += ' bg-emerald-50 border-emerald-100';
                wrap.className += ' bg-emerald-100';
                icon.style.filter = 'invert(39%) sepia(98%) saturate(400%) hue-rotate(100deg) brightness(90%)';
                ttl.textContent = 'Success'; ttl.className = 'font-extrabold text-emerald-800 text-base';
                btn.className += ' bg-emerald-600 hover:bg-emerald-700';
            } else if (type === 'error') {
                hdr.className += ' bg-red-50 border-red-100';
                wrap.className += ' bg-red-100';
                icon.style.filter = 'invert(20%) sepia(90%) saturate(700%) hue-rotate(340deg) brightness(90%)';
                ttl.textContent = 'Error'; ttl.className = 'font-extrabold text-red-800 text-base';
                btn.className += ' bg-red-600 hover:bg-red-700';
            } else if (type === 'warning') {
                hdr.className += ' bg-amber-50 border-amber-100';
                wrap.className += ' bg-amber-100';
                icon.style.filter = 'invert(55%) sepia(90%) saturate(500%) hue-rotate(10deg) brightness(95%)';
                ttl.textContent = 'Warning'; ttl.className = 'font-extrabold text-amber-800 text-base';
                btn.className += ' bg-amber-500 hover:bg-amber-600';
            } else {
                hdr.className += ' bg-blue-50 border-blue-100';
                wrap.className += ' bg-blue-100';
                icon.style.filter = 'invert(28%) sepia(80%) saturate(600%) hue-rotate(195deg) brightness(95%)';
                ttl.textContent = 'Notice'; ttl.className = 'font-extrabold text-blue-800 text-base';
                btn.className += ' bg-blue-600 hover:bg-blue-700';
            }

            msg.textContent = message;
            var modal = document.getElementById('notifyModal');
            modal.classList.remove('hidden'); modal.classList.add('flex');
            document.body.classList.add('overflow-hidden');
        }

        function hideNotifyModal() {
            var modal = document.getElementById('notifyModal');
            modal.classList.add('hidden'); modal.classList.remove('flex');
            document.body.classList.remove('overflow-hidden');
        }

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') hideNotifyModal();
        });
        document.addEventListener('click', function (e) {
            var nm = document.getElementById('notifyModal');
            if (nm && !nm.classList.contains('hidden') && e.target === nm) hideNotifyModal();
        });
    </script>

</asp:Content>