<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="VaccineManagement.aspx.cs" Inherits="SBI.VaccineManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- PAGE HEADER -->
        <div class="flex flex-col gap-4 md:flex-row md:items-end md:justify-between">
            <div>
                <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Vaccine Management</h1>
                <p class="mt-1 text-base text-slate-600">
                    Inventory control, stock monitoring, and administration tracking
                </p>
            </div>

            <div class="flex flex-wrap gap-3">
                <asp:Button ID="btnBatchHistory" runat="server"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                    Text="Batch History" />

                <asp:Button ID="btnAddStock" runat="server"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                    Text="Add Stock" />
            </div>
        </div>

        <!-- LOW STOCK ALERT -->
        <div class="mt-5 flex flex-col gap-3 rounded-xl border border-red-200 bg-red-50 p-4 md:flex-row md:items-center md:justify-between">
            <asp:Label ID="lblLowStock" runat="server"
                CssClass="text-sm font-semibold text-red-700"
                Text="LOW STOCK ALERT: Verorab (Anti-Rabies) – 0 doses remaining · Equirab RIG – 0 vials remaining">
            </asp:Label>

            <asp:Button ID="btnRequestRestock" runat="server"
                CssClass="h-11 rounded-lg bg-red-600 px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                Text="Request Restock" />
        </div>

        <!-- INVENTORY OVERVIEW CARDS -->
        <div class="mt-5 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">

            <!-- Total Available -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-emerald-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-emerald-600">
                    <asp:Label ID="lblTotalDoses" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Total Available Doses</div>
            </div>

            <!-- Expiring -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-amber-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-amber-600">
                    <asp:Label ID="lblExpiring30" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Expiring within 30 days</div>
            </div>

            <!-- Expired -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-red-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-red-600">
                    <asp:Label ID="lblExpired" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Expired (for disposal)</div>
            </div>

            <!-- Administered -->
            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-indigo-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-indigo-600">
                    <asp:Label ID="lblAdministeredMTD" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Doses Administered (MTD)</div>
            </div>

        </div>

        <!-- ADD VACCINE STOCK FORM CARD -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

            <div class="flex items-center gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-100 text-blue-700 font-bold">
                    +
                </div>
                <h3 class="text-lg font-extrabold text-slate-900">Add Vaccine Stock</h3>
            </div>

            <div class="px-5 py-5">
                <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Vaccine Name <span class="text-red-500">*</span>
                        </label>
                        <asp:DropDownList ID="ddlVaccineName" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Batch Number <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtBatchNumber" runat="server" TextMode="SingleLine"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Expiration Date <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtExpiryDate" runat="server" TextMode="Date"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Quantity (Doses) <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtQuantity" runat="server" TextMode="Number"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                </div>
            </div>

            <div class="flex flex-col gap-3 border-t border-slate-200 bg-slate-50 px-5 py-4 sm:flex-row sm:justify-end">
                <asp:Button ID="btnReset" runat="server"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                    Text="Reset" />

                <asp:Button ID="btnAddToInventory" runat="server"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                    Text="Add to Inventory" />
            </div>

        </div>

        <!-- TABLE CARD -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

            <div class="flex flex-col gap-3 border-b border-slate-200 px-5 py-4 md:flex-row md:items-center md:justify-between">
                <h3 class="text-lg font-extrabold text-slate-900">Current Vaccine Inventory</h3>

                <div class="flex flex-col gap-3 sm:flex-row sm:items-center">
                    <asp:TextBox ID="txtSearchInventory" runat="server" TextMode="SingleLine"
                        CssClass="h-11 w-full sm:w-72 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        placeholder="Search batch / vaccine..." />

                    <asp:DropDownList ID="ddlFilterType" runat="server"
                        CssClass="h-11 w-full sm:w-44 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem Text="All Types" Value="" />
                        <asp:ListItem Text="Anti-Rabies" Value="Anti-Rabies" />
                        <asp:ListItem Text="RIG" Value="RIG" />
                    </asp:DropDownList>
                </div>
            </div>

            <!-- Grid container for horizontal scroll on small screens -->
            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-[900px] w-full text-sm"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                    RowStyle-CssClass="border-b border-slate-100"
                    AlternatingRowStyle-CssClass="bg-white"
                    GridLines="None">

                    <Columns>
                        <asp:BoundField DataField="batch_id" HeaderText="Batch ID" ReadOnly="True" />
                        <asp:BoundField DataField="vaccine_id" HeaderText="Vaccine ID" />
                        <asp:BoundField DataField="batch_number" HeaderText="Batch Number" />
                        <asp:BoundField DataField="expiration_date" HeaderText="Expiry Date" DataFormatString="{0:MMM dd, yyyy}" />
                        <asp:BoundField DataField="quantity_available" HeaderText="Quantity Available" />

                        <asp:TemplateField HeaderText="Actions">
                            <ItemTemplate>
                                <div class="flex flex-wrap gap-2 py-2">
                                    <asp:Button ID="btnAdminister" runat="server"
                                        CssClass="h-9 rounded-lg bg-emerald-600 px-3 font-bold text-white shadow-sm hover:brightness-110 transition"
                                        Text="Administer" CommandArgument='<%# Eval("batch_id") %>' />

                                    <asp:Button ID="btnEdit" runat="server"
                                        CssClass="h-9 rounded-lg bg-amber-500 px-3 font-bold text-white shadow-sm hover:brightness-110 transition"
                                        Text="Edit" CommandArgument='<%# Eval("batch_id") %>' />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div
        <div></div>

        </div>

    </div>

</asp:Content>