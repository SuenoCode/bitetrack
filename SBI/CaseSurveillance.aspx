<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="CaseSurveillance.aspx.cs"
    Inherits="SBI.CaseSurveillance"
    MasterPageFile="~/Site1.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- PAGE HEADER (optional but matches your modern theme) -->
        <div class="mb-5">
            <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Case Monitoring</h1>
            <p class="mt-1 text-base text-slate-600">Monitor risk levels, compliance, and vaccination schedules</p>
        </div>

        <!-- STATS -->
        <div class="grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4 mb-6">

            <!-- High Risk -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-red-500"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">High Risk</div>
                <div class="mt-1 text-3xl font-extrabold text-red-600">
                    <asp:Label ID="lblHighRisk" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Category III Active</div>
            </div>

            <!-- Moderate Risk -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-amber-500"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">Moderate Risk</div>
                <div class="mt-1 text-3xl font-extrabold text-amber-600">
                    <asp:Label ID="lblModerateRisk" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Category II Active</div>
            </div>

            <!-- Low Risk -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-emerald-500"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">Low Risk</div>
                <div class="mt-1 text-3xl font-extrabold text-emerald-600">
                    <asp:Label ID="lblLowRisk" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Category I Active</div>
            </div>

            <!-- Non-Compliant -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-slate-400"></div>
                <div class="mt-4 text-sm font-semibold text-slate-600">Non-Compliant</div>
                <div class="mt-1 text-3xl font-extrabold text-slate-700">
                    <asp:Label ID="lblNonCompliant" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-500">Defaulted cases</div>
            </div>

        </div>

        <!-- TABLE CARD -->
        <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm" id="module-surveillance">

            <!-- Header -->
            <div class="flex flex-col gap-3 border-b border-slate-200 px-5 py-4 lg:flex-row lg:items-center lg:justify-between">
                <h3 class="text-lg font-extrabold text-slate-900">All Surveillance Cases</h3>

                <div class="flex flex-col gap-3 sm:flex-row sm:flex-wrap sm:items-center">

                    <asp:TextBox ID="txtSearch" runat="server"
                        CssClass="h-11 w-full sm:w-72 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        Placeholder="🔍 Search by name, ID, barangay..." />

                    <asp:DropDownList ID="ddlRisk" runat="server"
                        CssClass="h-11 w-full sm:w-44 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem>All Risk Levels</asp:ListItem>
                        <asp:ListItem>High Risk</asp:ListItem>
                        <asp:ListItem>Moderate</asp:ListItem>
                        <asp:ListItem>Low Risk</asp:ListItem>
                    </asp:DropDownList>

                    <asp:DropDownList ID="ddlCategory" runat="server"
                        CssClass="h-11 w-full sm:w-44 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem>All Categories</asp:ListItem>
                        <asp:ListItem>Category I</asp:ListItem>
                        <asp:ListItem>Category II</asp:ListItem>
                        <asp:ListItem>Category III</asp:ListItem>
                    </asp:DropDownList>

                    <asp:DropDownList ID="ddlBarangay" runat="server"
                        CssClass="h-11 w-full sm:w-60 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
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

                </div>
            </div>

            <!-- GridView -->
            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-[1200px] w-full text-sm"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                    RowStyle-CssClass="border-b border-slate-100"
                    AlternatingRowStyle-CssClass="bg-white"
                    GridLines="None">
                    <Columns>
                        <asp:BoundField DataField="PatientID" HeaderText="Patient ID" ItemStyle-CssClass="font-bold text-slate-700" />
                        <asp:BoundField DataField="Name" HeaderText="Name" />
                        <asp:BoundField DataField="Barangay" HeaderText="Barangay" />
                        <asp:BoundField DataField="Category" HeaderText="Category" />
                        <asp:BoundField DataField="Animal" HeaderText="Animal" />
                        <asp:BoundField DataField="Risk" HeaderText="Risk" />
                        <asp:BoundField DataField="Dose1" HeaderText="Dose 1" />
                        <asp:BoundField DataField="Dose2" HeaderText="Dose 2" />
                        <asp:BoundField DataField="Dose3" HeaderText="Dose 3" />
                        <asp:BoundField DataField="Compliance" HeaderText="Compliance" />
                        <asp:BoundField DataField="Status" HeaderText="Status" />
                        <asp:TemplateField HeaderText="Action">
                            <ItemTemplate>
                                <asp:Button ID="btnView" runat="server"
                                    CssClass="h-9 rounded-lg bg-[#1a4ed8] px-4 font-extrabold text-white shadow-sm hover:brightness-110 transition"
                                    Text="View"
                                    CommandArgument='<%# Eval("PatientID") %>'
                                    OnClick="btnView_Click" />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>

        <!-- VACCINATION SCHEDULE CARD -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm" id="form-vaccination">

            <div class="border-b border-slate-200 bg-slate-50 px-5 py-4">
                <h3 class="text-lg font-extrabold text-slate-900">Vaccination Schedule</h3>
            </div>

            <div class="px-5 py-5 space-y-6">

                <!-- Pre-Exposure -->
                <div>
                    <h4 class="text-sm font-extrabold text-slate-900 mb-3">Pre-Exposure Vaccination Record</h4>

                    <asp:Repeater ID="rptPreExposure" runat="server">
                        <ItemTemplate>
                            <div class="grid grid-cols-1 gap-4 md:grid-cols-3 mb-4 rounded-xl border border-slate-200 p-4">
                                <div>
                                    <label class="mb-2 block text-sm font-semibold text-slate-700"><%# Eval("Label") %> Date</label>
                                    <asp:TextBox ID="txtDate" runat="server" TextMode="Date"
                                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                                </div>

                                <div>
                                    <label class="mb-2 block text-sm font-semibold text-slate-700">Given By</label>
                                    <asp:TextBox ID="txtGivenBy" runat="server"
                                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                                </div>

                                <div>
                                    <label class="mb-2 block text-sm font-semibold text-slate-700">Vaccine Used</label>
                                    <asp:DropDownList ID="ddlRole" runat="server"
                                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                                        <asp:ListItem Text="Select Vaccine" Value="" />
                                        <asp:ListItem Text="PCEC" Value="PCEC" />
                                        <asp:ListItem Text="PVRV" Value="PVRV" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <!-- Post-Exposure -->
                <div>
                    <h4 class="text-sm font-extrabold text-slate-900 mb-3">Post-Exposure Vaccination Record</h4>

                    <asp:Repeater ID="rptPostExposure" runat="server">
                        <ItemTemplate>
                            <div class="grid grid-cols-1 gap-4 md:grid-cols-3 mb-4 rounded-xl border border-slate-200 p-4">
                                <div>
                                    <label class="mb-2 block text-sm font-semibold text-slate-700"><%# Eval("Label") %> Date</label>
                                    <asp:TextBox ID="txtDate" runat="server" TextMode="Date"
                                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                                </div>

                                <div>
                                    <label class="mb-2 block text-sm font-semibold text-slate-700">Given By</label>
                                    <asp:TextBox ID="txtGivenBy" runat="server"
                                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                                </div>

                                <div>
                                    <label class="mb-2 block text-sm font-semibold text-slate-700">Signature</label>
                                    <asp:TextBox ID="txtVaccineUsed" runat="server"
                                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>

                <!-- Animal Status after Day 14 -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <h4 class="text-sm font-extrabold text-slate-900 mb-3">Status of Animal After Day 14</h4>
                    <div class="flex flex-wrap items-center gap-6 text-sm font-semibold text-slate-700">
                        <asp:RadioButton ID="chkAlive" runat="server" Text="Alive" GroupName="AnimalStatus" />
                        <asp:RadioButton ID="chkDied" runat="server" Text="Died" GroupName="AnimalStatus" />
                        <asp:RadioButton ID="chkLost" runat="server" Text="Lost" GroupName="AnimalStatus" />
                        <asp:RadioButton ID="chkRabiesTest" runat="server" Text="Rabies Test +" GroupName="AnimalStatus" />
                    </div>
                </div>

            </div>
        </div>

    </div>

</asp:Content>
