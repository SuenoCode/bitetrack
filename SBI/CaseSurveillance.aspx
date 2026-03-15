<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="CaseSurveillance.aspx.cs" Inherits="SBI.CaseSurveillance" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

        <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager>

        <div class="p-8 w-full">
            <div class="mb-6">
                <h1 class="text-[28px] font-bold text-[#1e3a8a] mb-1 font-heading2">Case Surveillance</h1>
                <p class="text-gray-500 text-sm font-heading2">Monitor patient cases, manage protocols, track daily administration, and record animal follow-ups.</p>
            </div>

            <asp:HiddenField ID="hfSelectedCaseId" runat="server" />
            <asp:HiddenField ID="hfSelectedScheduleId" runat="server" />
            <%-- hfEditMode: "true" = editing an existing completed dose, "false" = new administration --%>
            <asp:HiddenField ID="hfEditMode" runat="server" />

            <asp:Panel ID="panelNavigation" runat="server" CssClass="flex flex-wrap gap-2 mb-6">
                <asp:Button ID="btnTabToday" runat="server" Text="Today's Schedules" 
                    CssClass="bg-[#2563eb] text-white font-medium py-2 px-5 rounded-lg text-sm transition-colors border border-[#2563eb] cursor-pointer font-heading2" 
                    OnClick="btnTabToday_Click" />
                <asp:Button ID="btnTabRegistry" runat="server" Text="Case Registry" 
                    CssClass="bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium py-2 px-5 rounded-lg text-sm transition-colors cursor-pointer font-heading2" 
                    OnClick="btnTabRegistry_Click" />
            </asp:Panel>

            <%-- ========================================================
                 TAB 1: TODAY'S SCHEDULES
                 ======================================================== --%>
            <asp:Panel ID="panelTodaySchedules" runat="server" CssClass="bg-white rounded-xl shadow-sm border border-gray-200 mb-6 overflow-hidden">
                <div class="p-6 border-b border-gray-100 flex justify-between items-center">
                    <div>
                        <h2 class="text-lg font-bold text-gray-800 font-heading2">Due Today</h2>
                        <p class="text-sm text-gray-500 mt-1 font-heading2">Patients scheduled for vaccination today</p>
                    </div>
                    <asp:Button ID="btnRefreshToday" runat="server" Text="Refresh List" 
                        CssClass="text-[#2563eb] hover:text-blue-800 text-sm font-semibold bg-blue-50 hover:bg-blue-100 py-2 px-4 rounded-lg transition-colors cursor-pointer font-heading2" 
                        OnClick="btnRefreshToday_Click" />
                </div>
                
                <div class="overflow-x-auto">
                    <asp:GridView ID="gvTodaySchedules" runat="server" 
                                  CssClass="w-full text-left text-sm text-gray-600" 
                                  AutoGenerateColumns="False" 
                                  DataKeyNames="schedule_id,case_id" 
                                  OnRowCommand="gvTodaySchedules_RowCommand" 
                                  GridLines="None">
                        <HeaderStyle CssClass="bg-gray-50 text-gray-700 font-semibold border-b border-gray-200 font-heading2" />
                        <RowStyle CssClass="border-b border-gray-100 hover:bg-gray-50 font-heading2" />
                        <Columns>
                            <asp:BoundField DataField="case_no" HeaderText="Case No" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-left" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-left" />
                            <asp:BoundField DataField="patient_name" HeaderText="Patient Name" 
                                ItemStyle-CssClass="w-1/3 py-3 px-4 font-heading2 text-left" 
                                HeaderStyle-CssClass="w-1/3 py-3 px-4 font-heading2 text-left" />
                            <asp:BoundField DataField="dose_number" HeaderText="Dose" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" />
                            <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine" NullDisplayText="-" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center text-gray-400 italic" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" />
                            <asp:TemplateField HeaderText="Action" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center">
                                <ItemTemplate>
                                    <asp:Button ID="btnGoToCase" runat="server" 
                                                CommandName="ViewCase" 
                                                CommandArgument='<%# Container.DataItemIndex %>' 
                                                Text="Open Case" 
                                                CssClass="bg-[#2563eb] hover:bg-blue-700 text-white font-medium py-1.5 px-4 rounded text-xs transition-colors cursor-pointer font-heading2" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="p-8 text-center text-gray-500 text-sm font-heading2">No vaccinations scheduled for today.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </asp:Panel>

            <%-- ========================================================
                 TAB 2: CASE REGISTRY
                 ======================================================== --%>
            <asp:Panel ID="panelRegistrySearch" runat="server" Visible="false" CssClass="bg-white rounded-xl shadow-sm border border-gray-200 mb-6 overflow-hidden">
                <div class="p-6 border-b border-gray-100">
                    <h2 class="text-lg font-bold text-gray-800 mb-4 font-heading2">Case Registry</h2>
                    
                    <div class="flex flex-wrap items-center gap-3 w-full max-w-3xl">
                        <asp:TextBox ID="txtSearchCase" runat="server" 
                            CssClass="flex-1 min-w-[200px] border border-gray-300 rounded-lg px-4 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition-all font-heading2" 
                            Placeholder="Search by patient name, case no..."></asp:TextBox>
                        <asp:Button ID="btnSearchCase" runat="server" Text="Search" 
                            CssClass="bg-[#2563eb] hover:bg-blue-700 text-white font-semibold py-2.5 px-6 rounded-lg text-sm transition-colors cursor-pointer font-heading2" 
                            OnClick="btnSearchCase_Click" />
                        <asp:Button ID="btnClearCaseSearch" runat="server" Text="Clear" 
                            CssClass="bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-semibold py-2.5 px-6 rounded-lg text-sm transition-colors cursor-pointer font-heading2" 
                            OnClick="btnClearCaseSearch_Click" />
                    </div>
                </div>

                <div class="overflow-x-auto">
                    <asp:GridView ID="gvSummary" runat="server" 
                                  CssClass="w-full text-left text-sm text-gray-600" 
                                  AutoGenerateColumns="False" 
                                  DataKeyNames="case_id" 
                                  OnRowCommand="gvSummary_RowCommand" 
                                  GridLines="None">
                        <HeaderStyle CssClass="bg-gray-50 text-gray-700 font-semibold border-b border-gray-200 font-heading2" />
                        <RowStyle CssClass="border-b border-gray-100 hover:bg-gray-50 font-heading2" />
                        <Columns>
                            <asp:BoundField DataField="case_no" HeaderText="Case No" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-left" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-left" />
                            <asp:BoundField DataField="patient_name" HeaderText="Patient Name" 
                                ItemStyle-CssClass="w-1/4 py-3 px-4 font-heading2 text-left" 
                                HeaderStyle-CssClass="w-1/4 py-3 px-4 font-heading2 text-left" />
                            <asp:BoundField DataField="category" HeaderText="Category" 
                                ItemStyle-CssClass="w-1/12 py-3 px-4 font-heading2 text-center" 
                                HeaderStyle-CssClass="w-1/12 py-3 px-4 font-heading2 text-center" />
                            <asp:BoundField DataField="regimen_type" HeaderText="Protocol" NullDisplayText="-" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" />
                            <asp:BoundField DataField="total_doses" HeaderText="Total" NullDisplayText="-" 
                                ItemStyle-CssClass="w-1/12 py-3 px-4 text-center font-heading2" 
                                HeaderStyle-CssClass="w-1/12 py-3 px-4 text-center font-heading2" />
                            <asp:BoundField DataField="completed_doses" HeaderText="Completed" 
                                ItemStyle-CssClass="w-1/12 py-3 px-4 text-center text-green-600 font-bold font-heading2" 
                                HeaderStyle-CssClass="w-1/12 py-3 px-4 text-center font-heading2" />
                            <asp:TemplateField HeaderText="Action" 
                                ItemStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center" 
                                HeaderStyle-CssClass="w-1/6 py-3 px-4 font-heading2 text-center">
                                <ItemTemplate>
                                    <asp:Button ID="btnOpenCase" runat="server" 
                                                CommandName="OpenCase" 
                                                CommandArgument='<%# Container.DataItemIndex %>' 
                                                Text="Manage Case" 
                                                CssClass="bg-blue-50 hover:bg-blue-100 text-[#2563eb] font-semibold py-1.5 px-4 rounded text-xs transition-colors cursor-pointer border border-blue-200 font-heading2" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="p-8 text-center text-gray-500 text-sm font-heading2">No cases found matching your criteria.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>
                </div>
            </asp:Panel>

            <%-- ========================================================
                 ACTIVE CASE VIEW
                 ======================================================== --%>
            <asp:Panel ID="panelActiveCase" runat="server" Visible="false">
                
                <div class="mb-4">
                    <asp:LinkButton ID="btnBackToCases" runat="server" OnClick="btnBackToCases_Click" 
                        CssClass="text-sm text-[#2563eb] hover:underline flex items-center gap-1 font-medium font-heading2">
                        &larr; Back to Registry
                    </asp:LinkButton>
                </div>

                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    
                    <%-- LEFT COLUMN: Treatment Details + Animal Follow-Up + Assign Protocol --%>
                    <div class="lg:col-span-1">
                        <div class="bg-white rounded-xl shadow-sm border border-gray-200 p-6 h-full">
                            <h2 class="text-lg font-bold text-gray-800 border-b border-gray-100 pb-3 mb-4 font-heading2">Treatment Details</h2>
                            
                            <%-- Animal Follow-Up Panel (shown conditionally from code-behind) --%>
                            <asp:Panel ID="panelAnimalFollowUp" runat="server" Visible="false" CssClass="mb-6 p-4 bg-amber-50 border border-amber-200 rounded-lg shadow-sm">
                                <h3 class="text-sm font-bold text-amber-800 mb-3 flex items-center gap-2 font-heading2">
                                    <svg xmlns="http://www.w3.org/2000/svg" class="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                                        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z" />
                                    </svg>
                                    14-Day Animal Follow-Up Due
                                </h3>
                                
                                <div class="space-y-3">
                                    <asp:HiddenField ID="hfAnimalId" runat="server" />
                                    <asp:HiddenField ID="hfFollowUpId" runat="server" />

                                    <div>
                                        <label class="block text-xs font-medium text-amber-900 mb-1 font-heading2">Animal Status</label>
                                        <asp:DropDownList ID="ddlDay14Status" runat="server" 
                                            CssClass="w-full border border-amber-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500 bg-white font-heading2">
                                            <asp:ListItem Text="-- Select Status --" Value="" />
                                            <asp:ListItem Text="Alive and Healthy" Value="Alive" />
                                            <asp:ListItem Text="Dead / Sick" Value="Dead" />
                                            <asp:ListItem Text="Lost / Unknown" Value="Lost" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-medium text-amber-900 mb-1 font-heading2">Follow-up Date</label>
                                        <asp:TextBox ID="txtFollowUpDate" runat="server" TextMode="Date" 
                                            CssClass="w-full border border-amber-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500 bg-white font-heading2"></asp:TextBox>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-medium text-amber-900 mb-1 font-heading2">Notes</label>
                                        <asp:TextBox ID="txtFollowUpNotes" runat="server" TextMode="MultiLine" Rows="2" 
                                            CssClass="w-full border border-amber-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-amber-500 bg-white font-heading2"></asp:TextBox>
                                    </div>
                                    <asp:Button ID="btnSaveFollowUp" runat="server" Text="Save Follow-Up" 
                                        CssClass="bg-amber-600 hover:bg-amber-700 text-white font-semibold py-2 px-4 rounded-md text-sm transition-colors cursor-pointer w-full mt-2 font-heading2" 
                                        OnClick="btnSaveFollowUp_Click" />
                                </div>
                            </asp:Panel>

                            <%-- Case Info --%>
                            <div class="space-y-4 mb-6">
                                <div>
                                    <label class="block text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1 font-heading2">Case Reference</label>
                                    <div class="text-gray-800 font-medium font-heading2">
                                        <asp:Literal ID="litCaseNoDisplay" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div>
                                    <label class="block text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1 font-heading2">Patient Name</label>
                                    <div class="text-gray-800 font-medium font-heading2">
                                        <asp:Literal ID="litPatientNameDisplay" runat="server"></asp:Literal>
                                    </div>
                                </div>
                                <div>
                                    <label class="block text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1 font-heading2">Exposure Category</label>
                                    <div class="text-gray-800 font-medium font-heading2">
                                        <asp:Literal ID="litCategoryDisplay" runat="server"></asp:Literal>
                                    </div>
                                </div>
                            </div>

                            <%-- Assign Protocol (hidden once a schedule exists) --%>
                            <asp:Panel ID="panelGenerate" runat="server" CssClass="bg-gray-50 rounded-lg p-4 border border-gray-100">
                                <h3 class="text-sm font-bold text-gray-700 mb-3 font-heading2">Assign Protocol</h3>
                                <div class="space-y-4">
                                    <div>
                                        <label class="block text-xs font-medium text-gray-700 mb-1 font-heading2">Regimen Protocol</label>
                                        <asp:DropDownList ID="ddlProtocol" runat="server" 
                                            CssClass="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white font-heading2">
                                            <asp:ListItem Text="-- Select Protocol --" Value="" />
                                            <asp:ListItem Text="PEP Essen (0, 3, 7, 14, 28)" Value="PEP_ESSEN" />
                                            <asp:ListItem Text="PEP Zagreb (0, 7, 21)" Value="PEP_ZAGREB" />
                                            <asp:ListItem Text="PrEP (0, 7, 21)" Value="PREP" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-medium text-gray-700 mb-1 font-heading2">Day 0 Date</label>
                                        <asp:TextBox ID="txtDay0" runat="server" TextMode="Date"
                                            CssClass="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 font-heading2"></asp:TextBox>
                                    </div>
                                    <asp:Button ID="btnGenerateSchedule" runat="server" Text="Generate Schedule" 
                                        CssClass="bg-[#2563eb] hover:bg-blue-700 text-white font-semibold py-2 px-4 rounded-md text-sm transition-colors cursor-pointer w-full font-heading2" 
                                        OnClick="btnGenerateSchedule_Click" />
                                </div>
                            </asp:Panel>
                        </div>
                    </div>

                    <%-- RIGHT COLUMNS: Administration Form + Overall Schedule --%>
                    <div class="lg:col-span-2 flex flex-col gap-6">
                        
                        <%-- Record Dose Administration (shown when Administer / Edit is clicked) --%>
                        <asp:Panel ID="panelAdministration" runat="server" Visible="false" 
                            CssClass="bg-white rounded-xl shadow-sm border border-l-4 border-l-[#2563eb] border-gray-200 p-6">
                            <h2 class="text-md font-bold text-gray-800 mb-4 font-heading2">Record Dose Administration</h2>
                            
                            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                <div class="col-span-1 lg:col-span-2">
                                    <label class="block text-xs font-medium text-gray-700 mb-1 font-heading2">Vaccine</label>
                                    <asp:DropDownList ID="ddlDoseVaccine" runat="server" 
                                        CssClass="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white font-heading2">
                                    </asp:DropDownList>
                                </div>
                                <div>
                                    <label class="block text-xs font-medium text-gray-700 mb-1 font-heading2">Practitioner</label>
                                    <asp:TextBox ID="txtVaccinatedBy" runat="server" 
                                        CssClass="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 font-heading2"></asp:TextBox>
                                </div>
                                <div class="flex gap-2">
                                    <div class="flex-1">
                                        <label class="block text-xs font-medium text-gray-700 mb-1 font-heading2">Dosage</label>
                                        <asp:TextBox ID="txtDosage" runat="server" 
                                            CssClass="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 font-heading2" 
                                            Placeholder="e.g. 0.5"></asp:TextBox>
                                    </div>
                                    <div class="flex-1">
                                        <label class="block text-xs font-medium text-gray-700 mb-1 font-heading2">Route</label>
                                        <asp:TextBox ID="txtRoute" runat="server" 
                                            CssClass="w-full border border-gray-300 rounded-md px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 font-heading2" 
                                            Placeholder="e.g. IM"></asp:TextBox>
                                    </div>
                                </div>
                            </div>

                            <div class="mt-4 flex gap-2">
                                <asp:Button ID="btnSaveDose" runat="server" Text="Confirm Administration" 
                                    CssClass="bg-[#2563eb] hover:bg-blue-700 text-white font-medium py-2 px-5 rounded-md text-sm transition-colors cursor-pointer font-heading2" 
                                    OnClick="btnSaveDose_Click" />
                                <asp:Button ID="btnCancelDose" runat="server" Text="Cancel" 
                                    CssClass="bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium py-2 px-5 rounded-md text-sm transition-colors cursor-pointer font-heading2" 
                                    OnClick="btnCancelDose_Click" />
                            </div>
                        </asp:Panel>

                        <%-- Overall Schedule GridView --%>
                        <div class="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden flex-1">
                            <div class="p-5 border-b border-gray-100 flex justify-between items-center bg-gray-50/50">
                                <h2 class="text-lg font-bold text-gray-800 font-heading2">Overall Schedule</h2>
                                <asp:Button ID="btnRefreshSchedule" runat="server" Text="Refresh List" 
                                    CssClass="text-[#2563eb] hover:text-blue-800 text-sm font-semibold py-1 px-3 rounded transition-colors cursor-pointer font-heading2" 
                                    OnClick="btnRefreshSchedule_Click" />
                            </div>
                            
                            <div class="overflow-x-auto p-4">
                                <asp:GridView ID="gvSchedule" runat="server" 
                                              CssClass="w-full text-left text-sm text-gray-600" 
                                              AutoGenerateColumns="False" 
                                              DataKeyNames="schedule_id" 
                                              OnRowCommand="gvSchedule_RowCommand" 
                                              GridLines="None">
                                    <HeaderStyle CssClass="text-gray-500 font-semibold border-b border-gray-200 font-heading2" />
                                    <RowStyle CssClass="border-b border-gray-100 hover:bg-gray-50 font-heading2" />
                                    <Columns>
                                        <asp:BoundField DataField="dose_number" HeaderText="Dose" 
                                            ItemStyle-CssClass="w-1/6 py-3 px-2 font-medium font-heading2 text-left" 
                                            HeaderStyle-CssClass="w-1/6 py-2 px-2 font-heading2 text-left" />
                                        <asp:BoundField DataField="schedule_date" HeaderText="Date" 
                                            DataFormatString="{0:MMM dd, yyyy}" 
                                            ItemStyle-CssClass="w-1/4 py-3 px-2 font-heading2 text-left" 
                                            HeaderStyle-CssClass="w-1/4 py-2 px-2 font-heading2 text-left" />
                                        <asp:TemplateField HeaderText="Status" 
                                            ItemStyle-CssClass="w-1/6 py-3 px-2 font-heading2 text-center" 
                                            HeaderStyle-CssClass="w-1/6 py-2 px-2 font-heading2 text-center">
                                            <ItemTemplate>
                                                <span class='<%# Eval("status").ToString() == "Completed" 
                                                    ? "inline-flex items-center px-2 py-1 rounded text-[11px] font-bold bg-green-100 text-green-700 uppercase tracking-wide font-heading2" 
                                                    : "inline-flex items-center px-2 py-1 rounded text-[11px] font-bold bg-amber-100 text-amber-700 uppercase tracking-wide font-heading2" %>'>
                                                    <%# Eval("status") %>
                                                </span>
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                        <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine" NullDisplayText="-" 
                                            ItemStyle-CssClass="w-1/4 py-3 px-2 font-heading2 text-center text-gray-500" 
                                            HeaderStyle-CssClass="w-1/4 py-2 px-2 font-heading2 text-center" />
                                        <asp:TemplateField HeaderText="Action" 
                                            ItemStyle-CssClass="w-1/6 py-3 px-2 text-center font-heading2" 
                                            HeaderStyle-CssClass="w-1/6 py-2 px-2 text-center font-heading2">
                                            <ItemTemplate>
                                                <%-- Administer button: only shown for Pending doses --%>
                                                <asp:Button ID="btnAdminister" runat="server" 
                                                            CommandName="AdministerDose" 
                                                            CommandArgument='<%# Container.DataItemIndex %>' 
                                                            Text="Administer" 
                                                            CssClass="bg-green-600 hover:bg-green-700 text-white font-medium py-1.5 px-3 rounded text-xs transition-colors cursor-pointer font-heading2" 
                                                            Visible='<%# Eval("status").ToString() == "Pending" %>' />
                                                
                                                <%-- Edit button: only shown for Completed doses --%>
                                                <asp:Button ID="btnEdit" runat="server" 
                                                            CommandName="EditDose" 
                                                            CommandArgument='<%# Container.DataItemIndex %>' 
                                                            Text="Edit" 
                                                            CssClass="bg-white border border-gray-300 hover:bg-gray-50 text-gray-700 font-medium py-1.5 px-3 rounded text-xs transition-colors cursor-pointer font-heading2" 
                                                            Visible='<%# Eval("status").ToString() == "Completed" %>' />
                                            </ItemTemplate>
                                        </asp:TemplateField>
                                    </Columns>
                                    <EmptyDataTemplate>
                                        <div class="p-8 text-center text-gray-500 text-sm font-heading2">No schedule generated for this case yet.</div>
                                    </EmptyDataTemplate>
                                </asp:GridView>
                            </div>
                        </div>
                    </div>
                </div>
            </asp:Panel>
        </div>
</asp:Content>
