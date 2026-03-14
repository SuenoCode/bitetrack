<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="VaccineManagement.aspx.cs" Inherits="SBI.VaccineManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<div class="px-4 py-6 font-sans text-slate-900">

    <!-- HEADER -->
    <h1 class="text-4xl font-extrabold text-[#0b2a7a]">Vaccine Management</h1>
    <p class="text-slate-600 mt-1">
        Inventory control, stock monitoring, and administration tracking
    </p>


    <!-- NAVIGATION -->
    <div class="mt-6 bg-white border border-slate-200 rounded-xl shadow-sm p-3">
        <div class="flex gap-3">

            <asp:LinkButton ID="btnOverviewTab" runat="server"
                OnClick="btnOverviewTab_Click"
                CssClass="px-4 py-2 rounded-lg font-semibold text-white bg-blue-600">
                Inventory Overview
            </asp:LinkButton>

            <asp:LinkButton ID="btnAddStockTab" runat="server"
                OnClick="btnAddStockTab_Click"
                CssClass="px-4 py-2 rounded-lg border border-slate-300 bg-white font-semibold">
                Add Vaccine Stock
            </asp:LinkButton>

            <asp:LinkButton ID="btnInventoryTab" runat="server"
                OnClick="btnInventoryTab_Click"
                CssClass="px-4 py-2 rounded-lg border border-slate-300 bg-white font-semibold">
                Inventory Table
            </asp:LinkButton>

        </div>
    </div>


    <!-- INVENTORY OVERVIEW PANEL -->
    <asp:Panel ID="panelOverview" runat="server" CssClass="mt-6">

        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm p-5">

            <h3 class="text-lg font-extrabold text-slate-900 mb-4">
                Inventory Overview
            </h3>

            <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">

                <div class="border rounded-xl p-4">
                    <div class="text-3xl font-extrabold text-emerald-600">
                        <asp:Label ID="lblTotalDoses" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="text-sm text-slate-600 font-semibold">
                        Total Available Doses
                    </div>
                </div>

                <div class="border rounded-xl p-4">
                    <div class="text-3xl font-extrabold text-amber-600">
                        <asp:Label ID="lblExpiring30" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="text-sm text-slate-600 font-semibold">
                        Expiring within 30 days
                    </div>
                </div>

                <div class="border rounded-xl p-4">
                    <div class="text-3xl font-extrabold text-red-600">
                        <asp:Label ID="lblExpired" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="text-sm text-slate-600 font-semibold">
                        Expired Vaccines
                    </div>
                </div>

                <div class="border rounded-xl p-4">
                    <div class="text-3xl font-extrabold text-indigo-600">
                        <asp:Label ID="lblAdministeredMTD" runat="server" Text="0"></asp:Label>
                    </div>
                    <div class="text-sm text-slate-600 font-semibold">
                        Administered (MTD)
                    </div>
                </div>

            </div>

        </div>

    </asp:Panel>


    <!-- ADD STOCK PANEL -->
    <div class="px-5 pb-5 flex justify-end">
    <asp:Button ID="btnAddStock" runat="server"
        Text="Add Stock"
        OnClick="btnAddStock_Click"
        CssClass="h-11 rounded-lg bg-[#1a4ed8] px-6 font-extrabold text-white shadow hover:brightness-110 transition" />
</div>
    <asp:Panel ID="panelAddStock" runat="server" Visible="false" CssClass="mt-6">

        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm">

            <div class="border-b border-slate-200 px-5 py-4">
                <h3 class="text-lg font-extrabold text-slate-900">
                    Add Vaccine Stock
                </h3>
            </div>

            <div class="p-5 grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 gap-4">

                <div>
                    <label class="text-sm font-semibold text-slate-700">Vaccine Name</label>
                    <asp:DropDownList ID="ddlVaccineName" runat="server"
                        CssClass="h-11 w-full border border-slate-200 rounded-lg px-3">
                        <asp:ListItem Text="Select Name" Value="" Selected="True" />
                        <asp:ListItem Text="SPEEDA" Value="" Selected="True" />
                         <asp:ListItem Text="ABHAYRAD" Value="" Selected="True" />
                         <asp:ListItem Text="VAXIRAB" Value="" Selected="True" />
                         <asp:ListItem Text="EQUIRAB" Value="" Selected="True" />
                         <asp:ListItem Text="VINRAB" Value="" Selected="True" />
                         <asp:ListItem Text="HRIG" Value="" Selected="True" />
                         <asp:ListItem Text="TT" Value="" Selected="True" />
                         <asp:ListItem Text="ATS" Value="" Selected="True" />
                         <asp:ListItem Text="PPD" Value="" Selected="True" />
                         <asp:ListItem Text="NBS" Value="" Selected="True" />
                         <asp:ListItem Text="INSULIN" Value="" Selected="True" />

                    </asp:DropDownList>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Batch Number</label>
                    <asp:TextBox ID="txtBatchNumber" runat="server"
                        CssClass="h-11 w-full border border-slate-200 rounded-lg px-3"></asp:TextBox>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Expiration Date</label>
                    <asp:TextBox ID="txtExpiryDate" runat="server" TextMode="Date"
                        CssClass="h-11 w-full border border-slate-200 rounded-lg px-3"></asp:TextBox>
                </div>

                <div>
                    <label class="text-sm font-semibold text-slate-700">Quantity</label>
                    <asp:TextBox ID="txtQuantity" runat="server" TextMode="Number"
                        CssClass="h-11 w-full border border-slate-200 rounded-lg px-3"></asp:TextBox>
                </div>

            </div>

        </div>

    </asp:Panel>

    <div class="px-5 py-4 border-b border-slate-200 flex gap-3">
    <asp:TextBox ID="txtSearch" runat="server"
        placeholder="Search by vaccine name or batch number..."
        CssClass="h-11 w-full border border-slate-200 rounded-lg px-3 text-sm" />

    <asp:Button ID="btnSearch" runat="server"
        Text="Search"
        OnClick="btnSearch_Click"
        CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 transition" />

    <asp:Button ID="btnClearSearch" runat="server"
        Text="Clear"
        OnClick="btnClearSearch_Click"
        CssClass="h-11 rounded-lg border border-slate-200 bg-white px-5 font-semibold text-slate-700 shadow-sm hover:shadow-md transition" />
</div>
    <!-- INVENTORY TABLE PANEL -->
    <asp:Panel ID="panelInventory" runat="server" Visible="false" CssClass="mt-6">

        <div class="bg-white border border-slate-200 rounded-2xl shadow-sm">

            <div class="px-5 py-4 border-b border-slate-200">
                <h3 class="text-lg font-extrabold text-slate-900">
                    Current Vaccine Inventory
                </h3>
            </div>

            <div class="overflow-x-auto">

                <asp:GridView ID="gvInventory" runat="server"
    AutoGenerateColumns="False"
    CssClass="w-full text-sm">
    <Columns>
        <asp:BoundField DataField="batch_id"           HeaderText="Batch ID"           />
        <asp:BoundField DataField="vaccine_name"       HeaderText="Vaccine"            />
        <asp:BoundField DataField="batch_number"       HeaderText="Batch Number"       />
        <asp:BoundField DataField="manufacturing_date" HeaderText="Manufacturing Date" DataFormatString="{0:MMM dd, yyyy}" />
        <asp:BoundField DataField="expiration_date"    HeaderText="Expiration Date"    DataFormatString="{0:MMM dd, yyyy}" />
        <asp:BoundField DataField="quantity_received"  HeaderText="Qty Received"       />
        <asp:BoundField DataField="current_stock"      HeaderText="Current Stock"      />
        <asp:BoundField DataField="date_received"      HeaderText="Date Received"      DataFormatString="{0:MMM dd, yyyy}" />
        <asp:BoundField DataField="stock_status"       HeaderText="Status"             />
    </Columns>
</asp:GridView>

            </div>

        </div>

    </asp:Panel>

</div>

</asp:Content>