<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="VaccineManagement.aspx.cs" Inherits="SBI.VaccineManagement" %>

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
                    Text="Batch History"
                    OnClick="btnBatchHistory_Click" />

                <asp:Button ID="btnAddStock" runat="server"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                    Text="+ Receive Vaccines"
                    OnClick="btnAddStock_Click" />
            </div>
        </div>

        <!-- SUCCESS / ERROR NOTIFICATION -->
        <asp:Panel ID="pnlMessage" runat="server" Visible="false">
            <div class="mt-4 flex items-center gap-3 rounded-xl border px-5 py-3">
                <asp:Label ID="lblMessage" runat="server" CssClass="text-sm font-semibold"></asp:Label>
            </div>
        </asp:Panel>

        <!-- LOW STOCK ALERT -->
        <div class="mt-5 flex flex-col gap-3 rounded-xl border border-red-200 bg-red-50 p-4 md:flex-row md:items-center md:justify-between">
            <div class="flex items-start gap-2">
                <span class="mt-0.5 text-red-600 font-bold text-lg">⚠</span>
                <asp:Label ID="lblLowStock" runat="server"
                    CssClass="text-sm font-semibold text-red-700"
                    Text="LOW STOCK ALERT: Verorab (Anti-Rabies) – 0 doses remaining · Equirab RIG – 0 vials remaining">
                </asp:Label>
            </div>
            <asp:Button ID="btnRequestRestock" runat="server"
                CssClass="h-11 rounded-lg bg-red-600 px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                Text="Request Restock"
                OnClick="btnRequestRestock_Click" />
        </div>

        <!-- INVENTORY OVERVIEW CARDS -->
        <div class="mt-5 grid grid-cols-1 gap-4 sm:grid-cols-2 xl:grid-cols-4">

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-emerald-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-emerald-600">
                    <asp:Label ID="lblTotalDoses" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Total Available Doses</div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-amber-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-amber-600">
                    <asp:Label ID="lblExpiring30" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Expiring within 30 days</div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-red-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-red-600">
                    <asp:Label ID="lblExpired" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Expired (for disposal)</div>
            </div>

            <div class="rounded-2xl border border-slate-200 bg-white p-5 shadow-sm">
                <div class="h-1 w-full rounded-full bg-indigo-500"></div>
                <div class="mt-4 text-3xl font-extrabold text-indigo-600">
                    <asp:Label ID="lblAdministeredMTD" runat="server" Text="0"></asp:Label>
                </div>
                <div class="mt-1 text-sm font-semibold text-slate-600">Doses Administered (MTD)</div>
            </div>

        </div>

            <!-- SECTION 1: RECEIVE / ADD VACCINE STOCK FORM                  -->
        <asp:Panel ID="pnlReceiveStock" runat="server">
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

            <div class="flex items-center gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-blue-100 text-blue-700 font-bold text-lg">
                    ↓
                </div>
                <div>
                    <h3 class="text-lg font-extrabold text-slate-900">Receive Vaccine Stock</h3>
                    <p class="text-xs text-slate-500 mt-0.5">Record incoming vaccine deliveries. All fields marked * are required.</p>
                </div>
            </div>

            <div class="px-5 py-5">

                <!-- ROW 1: Vaccine Info -->
                <p class="mb-3 text-xs font-bold uppercase tracking-widest text-slate-400">Vaccine Information</p>
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
                            placeholder="e.g. VRB-2024-001"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Manufacturer <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtManufacturer" runat="server" TextMode="SingleLine"
                            placeholder="e.g. Sanofi Pasteur"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Supplier / Source <span class="text-red-500">*</span>
                        </label>
                        <asp:DropDownList ID="ddlSupplier" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            <asp:ListItem Text="-- Select Supplier --" Value="" />
                            <asp:ListItem Text="DOH / Government Supply" Value="DOH" />
                            <asp:ListItem Text="Direct Purchase" Value="Direct" />
                            <asp:ListItem Text="Donated" Value="Donated" />
                            <asp:ListItem Text="Other" Value="Other" />
                        </asp:DropDownList>
                    </div>

                </div>

                <!-- ROW 2: Dates & Quantity -->
                <p class="mb-3 mt-5 text-xs font-bold uppercase tracking-widest text-slate-400">Stock Details</p>
                <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Delivery Date <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtDeliveryDate" runat="server" TextMode="Date"
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
                            Quantity Received (Doses) <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtQuantity" runat="server" TextMode="Number"
                            placeholder="0"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Storage Location
                        </label>
                        <asp:TextBox ID="txtStorageLocation" runat="server" TextMode="SingleLine"
                            placeholder="e.g. Refrigerator A - Shelf 2"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                </div>

                <!-- ROW 3: Notes -->
                <div class="mt-4">
                    <label class="mb-2 block text-sm font-semibold text-slate-700">Remarks / Notes</label>
                    <asp:TextBox ID="txtReceiveRemarks" runat="server" TextMode="MultiLine" Rows="2"
                        placeholder="Any additional notes about this delivery (optional)..."
                        CssClass="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200 resize-none">
                    </asp:TextBox>
                </div>

            </div>

            <div class="flex flex-col gap-3 border-t border-slate-200 bg-slate-50 px-5 py-4 sm:flex-row sm:justify-end">
                <asp:Button ID="btnReset" runat="server"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                    Text="Clear Form"
                    OnClick="btnReset_Click" />

                <asp:Button ID="btnAddToInventory" runat="server"
                    CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                    Text="Save to Inventory"
                    OnClick="btnAddToInventory_Click" />
            </div>

        </div>
        </asp:Panel>

        <!-- SECTION 2: STOCK ADJUSTMENT FORM                             -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

            <div class="flex items-center gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                <div class="flex h-9 w-9 items-center justify-center rounded-xl bg-amber-100 text-amber-700 font-bold text-lg">
                    ⇄
                </div>
                <div>
                    <h3 class="text-lg font-extrabold text-slate-900">Stock Adjustment</h3>
                    <p class="text-xs text-slate-500 mt-0.5">Correct quantities, remove expired stock, or record wastage.</p>
                </div>
            </div>

            <div class="px-5 py-5">
                <div class="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-4">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Batch Number <span class="text-red-500">*</span>
                        </label>
                        <asp:DropDownList ID="ddlAdjustBatch" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Adjustment Type <span class="text-red-500">*</span>
                        </label>
                        <asp:DropDownList ID="ddlAdjustmentType" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            <asp:ListItem Text="-- Select Type --" Value="" />
                            <asp:ListItem Text="Correction (Increase)" Value="Correction+" />
                            <asp:ListItem Text="Correction (Decrease)" Value="Correction-" />
                            <asp:ListItem Text="Expired – Mark for Disposal" Value="Expired" />
                            <asp:ListItem Text="Damaged / Wastage" Value="Damaged" />
                            <asp:ListItem Text="Transfer Out" Value="Transfer" />
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Quantity Affected <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtAdjustQty" runat="server" TextMode="Number"
                            placeholder="0"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Authorized By <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtAuthorizedBy" runat="server" TextMode="SingleLine"
                            placeholder="Staff name or ID"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        </asp:TextBox>
                    </div>

                </div>

                <div class="mt-4">
                    <label class="mb-2 block text-sm font-semibold text-slate-700">
                        Reason for Adjustment <span class="text-red-500">*</span>
                    </label>
                    <asp:TextBox ID="txtAdjustReason" runat="server" TextMode="MultiLine" Rows="2"
                        placeholder="Describe the reason for this adjustment (required for audit trail)..."
                        CssClass="w-full rounded-lg border border-slate-200 bg-white px-3 py-2 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200 resize-none">
                    </asp:TextBox>
                </div>
            </div>

            <div class="flex flex-col gap-3 border-t border-slate-200 bg-slate-50 px-5 py-4 sm:flex-row sm:justify-end">
                <asp:Button ID="btnClearAdjust" runat="server"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-4 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition"
                    Text="Clear"
                    OnClick="btnClearAdjust_Click" />

                <asp:Button ID="btnSaveAdjustment" runat="server"
                    CssClass="h-11 rounded-lg bg-amber-500 px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition"
                    Text="Save Adjustment"
                    OnClick="btnSaveAdjustment_Click" />
            </div>

        </div>

        <!-- SECTION 3: CURRENT INVENTORY TABLE                           -->
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

                    <asp:Button ID="btnSearch" runat="server"
                        CssClass="h-11 rounded-lg bg-slate-700 px-4 font-semibold text-white shadow-sm hover:brightness-110 transition"
                        Text="Search"
                        OnClick="btnSearch_Click" />
                </div>
            </div>

            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-[1000px] w-full text-sm"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                    RowStyle-CssClass="border-b border-slate-100"
                    AlternatingRowStyle-CssClass="bg-slate-50/40"
                    GridLines="None"
                    OnRowCommand="gvInventory_RowCommand">

                    <EmptyDataTemplate>
                        <div class="px-6 py-10 text-center text-slate-400 text-sm font-medium">
                            No inventory records found.
                        </div>
                    </EmptyDataTemplate>

                    <Columns>
                        <asp:BoundField DataField="batch_id"           HeaderText="Batch ID"     ReadOnly="True"
                            ItemStyle-CssClass="px-4 py-3 font-mono text-xs text-slate-500"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="vaccine_name"       HeaderText="Vaccine"
                            ItemStyle-CssClass="px-4 py-3 font-semibold text-slate-800"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="batch_number"       HeaderText="Batch No."
                            ItemStyle-CssClass="px-4 py-3 font-mono text-xs"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="manufacturer"       HeaderText="Manufacturer"
                            ItemStyle-CssClass="px-4 py-3"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="delivery_date"      HeaderText="Received"
                            DataFormatString="{0:MMM dd, yyyy}"
                            ItemStyle-CssClass="px-4 py-3 text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="expiration_date"    HeaderText="Expiry"
                            DataFormatString="{0:MMM dd, yyyy}"
                            ItemStyle-CssClass="px-4 py-3"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="quantity_received"  HeaderText="Qty Received"
                            ItemStyle-CssClass="px-4 py-3 text-center"
                            HeaderStyle-CssClass="px-4 py-3 text-center" />

                        <asp:BoundField DataField="quantity_available" HeaderText="Available"
                            ItemStyle-CssClass="px-4 py-3 text-center font-bold"
                            HeaderStyle-CssClass="px-4 py-3 text-center" />

                        <!-- Status badge via TemplateField -->
                        <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="px-4 py-3 text-center">
                            <ItemTemplate>
                                <div class="flex justify-center">
                                    <asp:Label ID="lblStatus" runat="server"
                                        Text='<%# Eval("stock_status") %>'
                                        CssClass='<%# GetStatusCssClass(Eval("stock_status").ToString()) %>'>
                                    </asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="px-4 py-3">
                            <ItemTemplate>
                                <div class="flex flex-wrap gap-2 px-4 py-2">
                                    <asp:Button ID="btnAdminister" runat="server"
                                        CssClass="h-9 rounded-lg bg-emerald-600 px-3 font-bold text-white shadow-sm hover:brightness-110 transition text-xs"
                                        Text="Administer"
                                        CommandName="Administer"
                                        CommandArgument='<%# Eval("batch_id") %>' />

                                    <asp:Button ID="btnEdit" runat="server"
                                        CssClass="h-9 rounded-lg bg-amber-500 px-3 font-bold text-white shadow-sm hover:brightness-110 transition text-xs"
                                        Text="Edit"
                                        CommandName="EditBatch"
                                        CommandArgument='<%# Eval("batch_id") %>' />

                                    <asp:Button ID="btnViewBatch" runat="server"
                                        CssClass="h-9 rounded-lg bg-slate-500 px-3 font-bold text-white shadow-sm hover:brightness-110 transition text-xs"
                                        Text="History"
                                        CommandName="ViewHistory"
                                        CommandArgument='<%# Eval("batch_id") %>' />
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

        </div>

        <!-- SECTION 4: TRANSACTION / STOCK HISTORY LOG                   -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

            <div class="flex flex-col gap-3 border-b border-slate-200 px-5 py-4 md:flex-row md:items-center md:justify-between">
                <div>
                    <h3 class="text-lg font-extrabold text-slate-900">Stock Transaction Log</h3>
                    <p class="text-xs text-slate-500 mt-0.5">Full audit trail — stock entries, administrations, adjustments, and disposals.</p>
                </div>

                <div class="flex flex-col gap-3 sm:flex-row sm:items-center">

                    <!-- Filter by transaction type -->
                    <asp:DropDownList ID="ddlLogFilter" runat="server"
                        CssClass="h-11 w-full sm:w-48 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        AutoPostBack="true" OnSelectedIndexChanged="ddlLogFilter_Changed">
                        <asp:ListItem Text="All Transactions" Value="" />
                        <asp:ListItem Text="Stock Received"   Value="Received" />
                        <asp:ListItem Text="Administered"     Value="Administered" />
                        <asp:ListItem Text="Adjustment"       Value="Adjustment" />
                        <asp:ListItem Text="Expired/Disposed" Value="Disposed" />
                    </asp:DropDownList>

                    <!-- Date range -->
                    <asp:TextBox ID="txtLogDateFrom" runat="server" TextMode="Date"
                        CssClass="h-11 w-full sm:w-40 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />

                    <asp:TextBox ID="txtLogDateTo" runat="server" TextMode="Date"
                        CssClass="h-11 w-full sm:w-40 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />

                    <asp:Button ID="btnFilterLog" runat="server"
                        CssClass="h-11 rounded-lg bg-slate-700 px-4 font-semibold text-white shadow-sm hover:brightness-110 transition"
                        Text="Filter"
                        OnClick="btnFilterLog_Click" />
                </div>
            </div>

            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvTransactionLog" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-[900px] w-full text-sm"
                    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
                    RowStyle-CssClass="border-b border-slate-100"
                    AlternatingRowStyle-CssClass="bg-slate-50/40"
                    GridLines="None"
                    AllowPaging="True" PageSize="15"
                    OnPageIndexChanging="gvTransactionLog_PageIndexChanging">

                    <PagerStyle CssClass="border-t border-slate-200 bg-slate-50 px-4 py-3 text-sm text-slate-600" />

                    <EmptyDataTemplate>
                        <div class="px-6 py-10 text-center text-slate-400 text-sm font-medium">
                            No transaction records found for the selected filters.
                        </div>
                    </EmptyDataTemplate>

                    <Columns>

                        <asp:BoundField DataField="log_id"         HeaderText="Log #"
                            ItemStyle-CssClass="px-4 py-3 font-mono text-xs text-slate-400"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="transaction_date" HeaderText="Date &amp; Time"
                            DataFormatString="{0:MMM dd, yyyy HH:mm}"
                            ItemStyle-CssClass="px-4 py-3 text-slate-700"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="vaccine_name"   HeaderText="Vaccine"
                            ItemStyle-CssClass="px-4 py-3 font-semibold text-slate-800"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="batch_number"   HeaderText="Batch No."
                            ItemStyle-CssClass="px-4 py-3 font-mono text-xs"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <!-- Transaction type badge -->
                        <asp:TemplateField HeaderText="Transaction" HeaderStyle-CssClass="px-4 py-3">
                            <ItemTemplate>
                                <div class="px-4 py-2">
                                    <asp:Label ID="lblTxnType" runat="server"
                                        Text='<%# Eval("transaction_type") %>'
                                        CssClass='<%# GetTransactionCssClass(Eval("transaction_type").ToString()) %>'>
                                    </asp:Label>
                                </div>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <asp:BoundField DataField="quantity_change" HeaderText="Qty Change"
                            ItemStyle-CssClass="px-4 py-3 text-center font-bold"
                            HeaderStyle-CssClass="px-4 py-3 text-center" />

                        <asp:BoundField DataField="quantity_after"  HeaderText="Stock After"
                            ItemStyle-CssClass="px-4 py-3 text-center text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3 text-center" />

                        <asp:BoundField DataField="performed_by"   HeaderText="Performed By"
                            ItemStyle-CssClass="px-4 py-3 text-slate-600"
                            HeaderStyle-CssClass="px-4 py-3" />

                        <asp:BoundField DataField="remarks"        HeaderText="Remarks"
                            ItemStyle-CssClass="px-4 py-3 text-slate-500 text-xs max-w-xs truncate"
                            HeaderStyle-CssClass="px-4 py-3" />

                    </Columns>
                </asp:GridView>
            </div>

        </div>

    </div>

</asp:Content>
