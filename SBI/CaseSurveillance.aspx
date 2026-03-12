<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CaseSurveillance.aspx.cs"
    Inherits="SBI.CaseSurveillance"
    MasterPageFile="~/Admin.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- PAGE HEADER -->
        <div class="mb-5 flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
            <div>
                <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Case Monitoring</h1>
                <p class="mt-1 text-base text-slate-600">Monitor risk levels, compliance, and vaccination schedules</p>
            </div>
            <div class="flex flex-wrap gap-3">
                <asp:Button ID="btnPrintSchedules" runat="server"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                    Text="🖨 Print Schedules" />
                <asp:Button ID="btnSendReminders" runat="server"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                    Text="📨 Send Reminders" />
            </div>
        </div>

        <!-- ============================================================ -->
        <!-- MISSED DOSE ALERT BANNER                                      -->
        <!-- ============================================================ -->
        <div class="mb-5 flex flex-col gap-3 rounded-xl border border-red-200 bg-red-50 p-4 md:flex-row md:items-center md:justify-between">
            <div class="flex items-start gap-2">
                <span class="mt-0.5 text-red-600 font-bold text-lg">⚠</span>
                <div>
                    <p class="text-sm font-extrabold text-red-700">Missed / Overdue Doses Detected</p>
                    <asp:Label ID="lblMissedSummary" runat="server"
                        CssClass="text-sm text-red-600"
                        Text="3 dose(s) across active patients are past their scheduled date.">
                    </asp:Label>
                </div>
            </div>
            <asp:Button ID="btnViewMissed" runat="server"
                CssClass="h-11 rounded-lg bg-red-600 px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                Text="View Missed Doses" />
        </div>

        <!-- DUE TODAY / UPCOMING REMINDER BANNER -->
        <div class="mb-5 flex flex-col gap-3 rounded-xl border border-blue-200 bg-blue-50 p-4 md:flex-row md:items-center md:justify-between">
            <div class="flex items-start gap-2">
                <span class="mt-0.5 text-blue-600 font-bold text-lg">🔔</span>
                <div>
                    <p class="text-sm font-extrabold text-blue-700">Doses Due Today or Within 2 Days</p>
                    <asp:Label ID="lblDueSummary" runat="server"
                        CssClass="text-sm text-blue-600"
                        Text="5 dose(s) are due today or within the next 2 days.">
                    </asp:Label>
                </div>
            </div>
        </div>

        <!-- STATS CARDS                                                   -->
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4 mb-6">

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-red-500"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">High Risk</div>
                <div class="mt-1 text-3xl font-extrabold text-red-600">
                    <asp:Label ID="lblHighRisk" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Category III Active</div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-amber-500"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">Moderate Risk</div>
                <div class="mt-1 text-3xl font-extrabold text-amber-600">
                    <asp:Label ID="lblModerateRisk" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Category II Active</div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-emerald-500"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">Low Risk</div>
                <div class="mt-1 text-3xl font-extrabold text-emerald-600">
                    <asp:Label ID="lblLowRisk" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Category I Active</div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-slate-400"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">Non-Compliant</div>
                <div class="mt-1 text-3xl font-extrabold text-slate-700">
                    <asp:Label ID="lblNonCompliant" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Defaulted cases</div>
            </div>

        </div>


        <!-- SECTION 1: ALL SURVEILLANCE CASES TABLE                      -->
        <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm" id="module-surveillance">

            <div class="flex flex-col gap-3 border-b border-slate-200 px-5 py-4 lg:flex-row lg:items-center lg:justify-between">
                <h3 class="text-lg font-extrabold text-slate-900">All Surveillance Cases</h3>

                <div class="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-center">

                    <asp:TextBox ID="txtSearch" runat="server"
                        CssClass="h-11 w-full sm:w-64 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        Placeholder="Search name, ID, barangay..." />

                    <asp:DropDownList ID="ddlRisk" runat="server"
                        CssClass="h-11 w-full sm:w-40 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem>All Risk Levels</asp:ListItem>
                        <asp:ListItem>High Risk</asp:ListItem>
                        <asp:ListItem>Moderate</asp:ListItem>
                        <asp:ListItem>Low Risk</asp:ListItem>
                    </asp:DropDownList>

                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="h-11 w-full sm:w-40 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem>All Categories</asp:ListItem>
                        <asp:ListItem>Category I</asp:ListItem>
                        <asp:ListItem>Category II</asp:ListItem>
                        <asp:ListItem>Category III</asp:ListItem>
                    </asp:DropDownList>

                    <asp:DropDownList ID="ddlBarangay" runat="server"
                        CssClass="h-11 w-full sm:w-52 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem>All Barangays</asp:ListItem>
                        <asp:ListItem>Bombongan</asp:ListItem>
                        <asp:ListItem>Caniogan-Calero-Lanang</asp:ListItem>
                        <asp:ListItem>Lagundi</asp:ListItem>
                        <asp:ListItem>Maybangcal</asp:ListItem>
                        <asp:ListItem>San Jose</asp:ListItem>
                        <asp:ListItem>San Pedro</asp:ListItem>
                        <asp:ListItem>San Guillermo</asp:ListItem>
                        <asp:ListItem>Poblacion</asp:ListItem>
                    </asp:DropDownList>

                    <asp:Button ID="btnSearch" runat="server"
                        CssClass="h-11 rounded-lg bg-slate-700 px-4 font-semibold text-white shadow-sm hover:brightness-110 transition"
                        Text="Search" />

                </div>
            </div>

            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-[1200px] w-full text-sm"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                    RowStyle-CssClass="border-b border-slate-100"
                    AlternatingRowStyle-CssClass="bg-slate-50/40"
                    GridLines="None">

                    <EmptyDataTemplate>
                        <div class="px-6 py-10 text-center text-slate-400 text-sm font-medium">No cases found.</div>
                    </EmptyDataTemplate>

                    <Columns>
                        <asp:BoundField DataField="PatientID" HeaderText="Patient ID"
                            ItemStyle-CssClass="px-4 py-3 font-bold text-slate-700 font-mono text-xs"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="Name" HeaderText="Name"
                            ItemStyle-CssClass="px-4 py-3 font-semibold"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="Barangay" HeaderText="Barangay"
                            ItemStyle-CssClass="px-4 py-3 text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="Category" HeaderText="Category"
                            ItemStyle-CssClass="px-4 py-3"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="Animal" HeaderText="Animal"
                            ItemStyle-CssClass="px-4 py-3 text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3" />

                        
                        <asp:TemplateField HeaderText="Risk" HeaderStyle-CssClass="px-4 py-3">
                            <ItemTemplate>
                                <div class="px-4 py-2">
                                    <asp:Label ID="lblRiskBadge" runat="server"
                                        Text='<%# Eval("Risk") %>'
                                        CssClass="inline-block rounded-full px-3 py-1 text-xs font-bold bg-slate-100 text-slate-600">
                                    </asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                      
                        <asp:TemplateField HeaderText="Next Dose Due" HeaderStyle-CssClass="px-4 py-3">
                            <ItemTemplate>
                                <div class="px-4 py-2">
                                    <asp:Label ID="lblNextDose" runat="server"
                                        Text='<%# Eval("NextDoseLabel") %>'
                                        CssClass="inline-block rounded-full px-3 py-1 text-xs font-bold bg-blue-100 text-blue-700">
                                    </asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        
                        <asp:TemplateField HeaderText="Compliance" HeaderStyle-CssClass="px-4 py-3">
                            <ItemTemplate>
                                <div class="px-4 py-2">
                                    <asp:Label ID="lblCompliance" runat="server"
                                        Text='<%# Eval("Compliance") %>'
                                        CssClass="inline-block rounded-full px-3 py-1 text-xs font-bold bg-slate-100 text-slate-600">
                                    </asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="Status" HeaderText="Status"
                            ItemStyle-CssClass="px-4 py-3 text-slate-600 text-xs"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="px-4 py-3">
                            <ItemTemplate>
                                <div class="flex gap-2 px-4 py-2">
                                    <asp:Button ID="btnView" runat="server"
                                        CssClass="h-9 rounded-lg bg-[#1a4ed8] px-3 font-bold text-white shadow-sm hover:brightness-110 transition text-xs"
                                        Text="View"
                                        CommandArgument='<%# Eval("PatientID") %>'
                                        OnClick="btnView_Click" />
                                    <asp:Button ID="btnSchedule" runat="server"
                                        CssClass="h-9 rounded-lg bg-emerald-600 px-3 font-bold text-white shadow-sm hover:brightness-110 transition text-xs"
                                        Text="Schedule"
                                        CommandArgument='<%# Eval("PatientID") %>' />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

      
        <div class="mt-6 overflow-hidden rounded-2xl border border-[#1a4ed8]/20 bg-white shadow-sm" id="form-vaccination">

            <div class="flex flex-col gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4 md:flex-row md:items-center md:justify-between">
                <div>
                    <h3 class="text-lg font-extrabold text-slate-900">Vaccination Schedule</h3>
                    <p class="text-xs text-slate-500 mt-0.5">
                        Select a protocol and Day 0 date to auto-generate the full dose schedule.
                    </p>
                </div>
                <asp:Button ID="btnGenerateSchedule" runat="server"
                    CssClass="h-10 rounded-lg bg-[#1a4ed8] px-4 font-bold text-white shadow-sm hover:brightness-110 transition text-sm"
                    Text="⟳ Auto-Generate Schedule" />
            </div>

            <div class="px-5 py-5">

                <!-- Protocol + Day 0 selector row -->
                <div class="mb-5 grid grid-cols-1 gap-4 md:grid-cols-3 rounded-xl border border-blue-100 bg-blue-50 p-4">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Vaccination Protocol <span class="text-red-500">*</span>
                        </label>
                        <asp:DropDownList ID="ddlProtocol" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            <asp:ListItem Text="-- Select Protocol --" Value="" />
                            <asp:ListItem Text="PEP – Essen (Day 0,3,7,14,28)"  Value="PEP_ESSEN" />
                            <asp:ListItem Text="PEP – Zagreb (Day 0,0,7,21)"    Value="PEP_ZAGREB" />
                            <asp:ListItem Text="PrEP – Standard (Day 0,7,21)"   Value="PREP_STANDARD" />
                            <asp:ListItem Text="Booster Only"                    Value="BOOSTER" />
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Date of First Dose (Day 0) <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtDose0Date" runat="server" TextMode="Date"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div class="flex items-end">
                        <asp:Button ID="btnCalculate" runat="server"
                            CssClass="h-11 w-full rounded-lg bg-emerald-600 px-4 font-bold text-white shadow hover:brightness-110 transition"
                            Text="Calculate Schedule Dates" />
                    </div>

                </div>

                <!-- Generated schedule table -->
                <h4 class="mb-3 text-xs font-bold uppercase tracking-widest text-slate-400">Generated Schedule</h4>

                <div class="w-full overflow-x-auto">
                    <asp:GridView ID="gvSchedule" runat="server" AutoGenerateColumns="False"
                        CssClass="min-w-[800px] w-full text-sm"
                        HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                        RowStyle-CssClass="border-b border-slate-100"
                        AlternatingRowStyle-CssClass="bg-slate-50/40"
                        GridLines="None">

                        <EmptyDataTemplate>
                            <div class="px-6 py-10 text-center text-slate-400 text-sm font-medium">
                                Select a protocol and click "Calculate Schedule Dates" to generate.
                            </div>
                        </EmptyDataTemplate>

                        <Columns>
                            <asp:BoundField DataField="DoseLabel"     HeaderText="Dose"
                                ItemStyle-CssClass="px-4 py-3 font-bold text-slate-800"
                                HeaderStyle-CssClass="px-4 py-3" />
                            <asp:BoundField DataField="ScheduledDay"  HeaderText="Protocol Day"
                                ItemStyle-CssClass="px-4 py-3 font-mono text-slate-500 text-xs"
                                HeaderStyle-CssClass="px-4 py-3" />
                            <asp:BoundField DataField="ScheduledDate" HeaderText="Scheduled Date"
                                DataFormatString="{0:MMM dd, yyyy}"
                                ItemStyle-CssClass="px-4 py-3 font-semibold"
                                HeaderStyle-CssClass="px-4 py-3" />
                            <asp:BoundField DataField="ActualDate"    HeaderText="Actual Date Given"
                                DataFormatString="{0:MMM dd, yyyy}"
                                ItemStyle-CssClass="px-4 py-3 text-slate-600"
                                HeaderStyle-CssClass="px-4 py-3" />
                            <asp:BoundField DataField="GivenBy"       HeaderText="Given By"
                                ItemStyle-CssClass="px-4 py-3 text-slate-600"
                                HeaderStyle-CssClass="px-4 py-3" />
                            <asp:BoundField DataField="VaccineUsed"   HeaderText="Vaccine Used"
                                ItemStyle-CssClass="px-4 py-3 text-slate-600"
                                HeaderStyle-CssClass="px-4 py-3" />

                          
                            <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="px-4 py-3">
                                <ItemTemplate>
                                    <div class="px-4 py-2">
                                        <asp:Label ID="lblDoseStatus" runat="server"
                                            Text='<%# Eval("DoseStatus") %>'
                                            CssClass="inline-block rounded-full px-3 py-1 text-xs font-bold bg-slate-100 text-slate-600">
                                        </asp:Label>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>

                          
                            <asp:TemplateField HeaderText="Record" HeaderStyle-CssClass="px-4 py-3">
                                <ItemTemplate>
                                    <div class="px-4 py-2">
                                        <asp:Button ID="btnRecordDose" runat="server"
                                            CssClass="h-9 rounded-lg bg-[#1a4ed8] px-3 font-bold text-white text-xs shadow-sm hover:brightness-110 transition"
                                            Text="Record Dose"
                                            CommandArgument='<%# Eval("ScheduleID") %>' />
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                    </asp:GridView>
                </div>

             
                <div class="mt-5 rounded-xl border border-emerald-200 bg-emerald-50 p-4">
                    <h4 class="mb-3 text-sm font-extrabold text-emerald-800">Record Dose Administration</h4>
                    <div class="grid grid-cols-1 gap-4 md:grid-cols-3">
                        <div>
                            <label class="mb-2 block text-sm font-semibold text-slate-700">Date Given <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtActualDate" runat="server" TextMode="Date"
                                CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            </asp:TextBox>
                        </div>
                        <div>
                            <label class="mb-2 block text-sm font-semibold text-slate-700">Given By <span class="text-red-500">*</span></label>
                            <asp:TextBox ID="txtGivenByRecord" runat="server" TextMode="SingleLine"
                                placeholder="Staff name"
                                CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            </asp:TextBox>
                        </div>
                        <div>
                            <label class="mb-2 block text-sm font-semibold text-slate-700">Vaccine Used <span class="text-red-500">*</span></label>
                            <asp:DropDownList ID="ddlVaccineRecord" runat="server"
                                CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                                <asp:ListItem Text="-- Select --" Value="" />
                                <asp:ListItem Text="Verorab" Value="Verorab" />
                                <asp:ListItem Text="Rabipur" Value="Rabipur" />
                                <asp:ListItem Text="PCEC"    Value="PCEC" />
                                <asp:ListItem Text="PVRV"    Value="PVRV" />
                            </asp:DropDownList>
                        </div>
                    </div>
                    <div class="mt-4">
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Notes</label>
                        <asp:TextBox ID="txtDoseNotes" runat="server" TextMode="MultiLine" Rows="2"
                            placeholder="Any remarks about this dose..."
                            CssClass="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200 resize-none">
                        </asp:TextBox>
                    </div>
                    <div class="mt-4 flex gap-3 justify-end">
                        <asp:Button ID="btnCancelDose" runat="server"
                            CssClass="h-10 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm transition text-sm"
                            Text="Cancel" />
                        <asp:Button ID="btnSaveDose" runat="server"
                            CssClass="h-10 rounded-lg bg-emerald-600 px-5 font-extrabold text-white shadow hover:brightness-110 transition text-sm"
                            Text="Save Dose Record" />
                    </div>
                </div>

                <!-- Compliance summary bar -->
                <div class="mt-5 rounded-xl border border-slate-200 bg-slate-50 p-4">
                    <div class="flex flex-wrap items-center gap-6 text-sm">
                        <span class="font-extrabold text-slate-700">Compliance Summary:</span>
                        <span class="inline-flex items-center gap-1">
                            <span class="h-3 w-3 rounded-full bg-emerald-500"></span>
                            <asp:Label ID="lblGivenCount" runat="server" Text="0" CssClass="font-bold text-emerald-700"></asp:Label>
                            <span class="text-slate-500">Given</span>
                        </span>
                        <span class="inline-flex items-center gap-1">
                            <span class="h-3 w-3 rounded-full bg-amber-400"></span>
                            <asp:Label ID="lblPendingCount" runat="server" Text="0" CssClass="font-bold text-amber-700"></asp:Label>
                            <span class="text-slate-500">Pending</span>
                        </span>
                        <span class="inline-flex items-center gap-1">
                            <span class="h-3 w-3 rounded-full bg-red-500"></span>
                            <asp:Label ID="lblMissedCount" runat="server" Text="0" CssClass="font-bold text-red-700"></asp:Label>
                            <span class="text-slate-500">Missed</span>
                        </span>
                        <span class="ml-auto font-extrabold text-[#1a4ed8]">
                            <asp:Label ID="lblCompliancePct" runat="server" Text="0% Complete"></asp:Label>
                        </span>
                    </div>
                    <div class="mt-3 h-2 w-full rounded-full bg-slate-200">
                        <div class="h-2 w-0 rounded-full bg-emerald-500 transition-all" id="progressBar"></div>
                    </div>
                </div>

            </div>
        </div>

        <!-- ============================================================ -->
        <!-- SECTION 3: MISSED DOSES TABLE                                 -->
        <!-- ============================================================ -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-red-200 bg-white shadow-sm">

            <div class="border-b border-red-100 bg-red-50 px-5 py-4">
                <h3 class="text-lg font-extrabold text-slate-900">Missed / Overdue Doses</h3>
                <p class="text-xs text-slate-500 mt-0.5">Patients with doses past their scheduled date and not yet recorded.</p>
            </div>

            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvMissedDoses" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-[900px] w-full text-sm"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                    RowStyle-CssClass="border-b border-slate-100"
                    AlternatingRowStyle-CssClass="bg-slate-50/40"
                    GridLines="None">

                    <EmptyDataTemplate>
                        <div class="px-6 py-8 text-center text-slate-400 text-sm font-medium">No missed doses found.</div>
                    </EmptyDataTemplate>

                    <Columns>
                        <asp:BoundField DataField="PatientID"     HeaderText="Patient ID"
                            ItemStyle-CssClass="px-4 py-3 font-mono text-xs text-slate-500"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="PatientName"   HeaderText="Patient Name"
                            ItemStyle-CssClass="px-4 py-3 font-semibold"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="DoseLabel"     HeaderText="Missed Dose"
                            ItemStyle-CssClass="px-4 py-3 font-bold text-red-700"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="ScheduledDate" HeaderText="Was Due"
                            DataFormatString="{0:MMM dd, yyyy}"
                            ItemStyle-CssClass="px-4 py-3 text-red-600"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="DaysOverdue"   HeaderText="Days Overdue"
                            ItemStyle-CssClass="px-4 py-3 font-extrabold text-red-700"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="ContactNumber" HeaderText="Contact No."
                            ItemStyle-CssClass="px-4 py-3 text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3" />
                        <asp:BoundField DataField="Barangay"      HeaderText="Barangay"
                            ItemStyle-CssClass="px-4 py-3 text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3" />
                    </Columns>
                </asp:GridView>
            </div>

        </div>

        
                <!-- Animal Status -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <h4 class="text-sm font-extrabold text-slate-900 mb-3">Status of Animal After Day 14</h4>
                    <div class="flex flex-wrap items-center gap-6 text-sm font-semibold text-slate-700">
                        <asp:RadioButton ID="chkAlive"      runat="server" Text="Alive"         GroupName="AnimalStatus" />
                        <asp:RadioButton ID="chkDied"       runat="server" Text="Died"          GroupName="AnimalStatus" />
                        <asp:RadioButton ID="chkLost"       runat="server" Text="Lost"          GroupName="AnimalStatus" />
                        <asp:RadioButton ID="chkRabiesTest" runat="server" Text="Rabies Test +" GroupName="AnimalStatus" />
                    </div>
                </div>

            </div>
       
</asp:Content>
