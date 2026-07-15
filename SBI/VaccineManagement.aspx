<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="VaccineManagement.aspx.cs" Inherits="SBI.VaccineManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<style>
    /* ── Pagination ─────────────────────────────────────────────── */
    .pager-wrap { display:flex; align-items:center; justify-content:space-between; padding:12px 20px; border-top:1px solid #e2e8f0; background:#f8fafc; }
    .pager-wrap .pager-info { font-size:13px; color:#64748b; }
    .pager-wrap table { margin:0; }
    .pager-wrap td { padding:0 3px; }
    .pager-wrap a, .pager-wrap span {
        display:inline-flex; align-items:center; justify-content:center;
        min-width:32px; height:32px; padding:0 10px;
        border-radius:6px; font-size:13px; font-weight:600;
        border:1px solid #e2e8f0; background:#fff; color:#475569;
        text-decoration:none; transition:all .15s;
    }
    .pager-wrap a:hover { background:#2563eb; color:#fff; border-color:#2563eb; }
    .pager-wrap span { background:#2563eb; color:#fff; border-color:#2563eb; cursor:default; }

    /* ── Table rows ─────────────────────────────────────────────── */
    .gv-table tr:hover td { background:#f1f5f9; }
    .gv-table td, .gv-table th { vertical-align:middle !important; }

    /* ── Status badges ──────────────────────────────────────────── */
    .badge { display:inline-flex; align-items:center; gap:5px; padding:3px 10px; border-radius:999px; font-size:11px; font-weight:700; letter-spacing:.4px; text-transform:uppercase; }
    .badge-ok   { background:#dcfce7; color:#15803d; }
    .badge-warn { background:#fef9c3; color:#a16207; }
    .badge-exp  { background:#fee2e2; color:#b91c1c; }
    .badge-in   { background:#dbeafe; color:#1d4ed8; }

    /* ── Stat Cards ──────────────────────────────────────────────── */
    .stat-grid {
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(150px, 1fr));
        gap: 12px;
        margin-bottom: 20px;
    }
    .stat-card {
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 10px;
        padding: 14px 16px;
        text-align: center;
        transition: all .2s;
    }
    .stat-card:hover { border-color: #94a3b8; }
    .stat-card .stat-number {
        font-size: 24px;
        font-weight: 800;
        line-height: 1.2;
    }
    .stat-card .stat-label {
        font-size: 10px;
        font-weight: 700;
        color: #94a3b8;
        text-transform: uppercase;
        letter-spacing: .5px;
        margin-top: 2px;
    }
    .stat-card .stat-number.green { color: #15803d; }
    .stat-card .stat-number.blue { color: #2563eb; }
    .stat-card .stat-number.amber { color: #d97706; }
    .stat-card .stat-number.red { color: #dc2626; }
    .stat-card .stat-number.purple { color: #7c3aed; }

    /* ── Vial Display - Simplified ──────────────────────────────── */
    .vial-container {
        max-height: 350px;
        overflow-y: auto;
        padding: 4px 0;
    }
    .vial-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
        gap: 8px;
    }
    .vial-item {
        background: #f8fafc;
        border: 1px solid #e2e8f0;
        border-radius: 8px;
        padding: 8px 10px;
        text-align: center;
        transition: all .15s;
        cursor: default;
        position: relative;
    }
    .vial-item:hover { border-color: #94a3b8; background: #f1f5f9; }
    .vial-item .vial-number {
        font-size: 13px;
        font-weight: 700;
        color: #0f172a;
        font-family: monospace;
    }
    .vial-item .vial-status-text {
        font-size: 9px;
        font-weight: 700;
        text-transform: uppercase;
        letter-spacing: .3px;
        padding: 1px 8px;
        border-radius: 10px;
        display: inline-block;
        margin-top: 2px;
    }
    .vial-item .vial-status-text.sealed { background: #dcfce7; color: #15803d; }
    .vial-item .vial-status-text.open { background: #fef3c7; color: #b45309; }
    .vial-item .vial-status-text.empty { background: #dbeafe; color: #1d4ed8; }
    .vial-item .vial-status-text.discarded { background: #fee2e2; color: #b91c1c; }
    
    .vial-item .vial-doses {
        font-size: 11px;
        color: #64748b;
        margin-top: 2px;
    }
    .vial-item .vial-doses span { font-weight: 700; color: #0f172a; }
    
    .vial-item .vial-actions {
        display: flex;
        gap: 4px;
        justify-content: center;
        margin-top: 6px;
        flex-wrap: wrap;
    }
    .vial-item .vial-actions .btn-sm {
        font-size: 9px;
        padding: 2px 8px;
        border: none;
        border-radius: 4px;
        font-weight: 600;
        cursor: pointer;
        transition: all .15s;
        text-decoration: none;
        display: inline-block;
    }
    .vial-item .vial-actions .btn-sm.open-btn { background: #f59e0b; color: #fff; }
    .vial-item .vial-actions .btn-sm.open-btn:hover { background: #d97706; }
    .vial-item .vial-actions .btn-sm.use-btn { background: #3b82f6; color: #fff; }
    .vial-item .vial-actions .btn-sm.use-btn:hover { background: #2563eb; }
    .vial-item .vial-actions .btn-sm.discard-btn { background: #ef4444; color: #fff; }
    .vial-item .vial-actions .btn-sm.discard-btn:hover { background: #dc2626; }
    .vial-item .vial-actions .btn-sm:disabled { opacity:0.4; cursor:not-allowed; }

    /* Vial border colors */
    .vial-item.sealed { border-left: 3px solid #22c55e; }
    .vial-item.open { border-left: 3px solid #f59e0b; background: #fffbeb; }
    .vial-item.empty { border-left: 3px solid #3b82f6; background: #eff6ff; }
    .vial-item.discarded { border-left: 3px solid #ef4444; background: #fef2f2; opacity:0.6; }

    /* ── Vial Summary ────────────────────────────────────────────── */
    .vial-summary {
        display: flex;
        gap: 16px;
        flex-wrap: wrap;
        padding: 6px 0 10px 0;
        border-bottom: 1px solid #e2e8f0;
        margin-bottom: 10px;
    }
    .vial-summary .vs-item {
        display: flex;
        align-items: center;
        gap: 6px;
        font-size: 12px;
        color: #475569;
    }
    .vial-summary .vs-item .vs-dot {
        width: 10px;
        height: 10px;
        border-radius: 50%;
        flex-shrink: 0;
    }
    .vial-summary .vs-item .vs-dot.green { background: #22c55e; }
    .vial-summary .vs-item .vs-dot.amber { background: #f59e0b; }
    .vial-summary .vs-item .vs-dot.blue { background: #3b82f6; }
    .vial-summary .vs-item .vs-dot.red { background: #ef4444; }
    .vial-summary .vs-item .vs-count { font-weight: 700; color: #0f172a; }

    /* ── Section headers ──────────────────────────────────────────── */
    .section-title {
        font-size: 13px;
        font-weight: 700;
        color: #1e293b;
        display: flex;
        align-items: center;
        gap: 8px;
    }
    .section-title .badge-count {
        font-size: 10px;
        font-weight: 600;
        color: #94a3b8;
        background: #f1f5f9;
        padding: 0 8px;
        border-radius: 10px;
    }

    /* ── Batch details card ───────────────────────────────────────── */
    .batch-card {
        background: white;
        border: 1px solid #e2e8f0;
        border-radius: 10px;
        padding: 12px 16px;
        margin-bottom: 8px;
        transition: all .15s;
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: wrap;
        gap: 8px;
    }
    .batch-card:hover { border-color: #94a3b8; background: #fafbfc; }
    .batch-card .batch-info {
        display: flex;
        align-items: center;
        gap: 16px;
        flex-wrap: wrap;
    }
    .batch-card .batch-number {
        font-weight: 700;
        font-family: monospace;
        font-size: 13px;
        color: #0f172a;
    }
    .batch-card .batch-expiry {
        font-size: 12px;
        color: #64748b;
    }
    .batch-card .batch-stock {
        font-weight: 700;
        font-size: 14px;
    }
    .batch-card .batch-stock.green { color: #15803d; }
    .batch-card .batch-stock.amber { color: #d97706; }
    .batch-card .batch-stock.red { color: #dc2626; }
</style>

<div class="p-6 font-heading2 text-slate-900">

    <%-- Page Header --%>
    <div class="flex justify-between items-center mb-4">
        <div>
            <h1 class="text-3xl text-[#0b2a7a] font-heading2 tracking-widest">Vaccine Management</h1>
            <p class="text-slate-500 text-sm mt-1">Inventory control and vial tracking</p>
        </div>
        <asp:Button ID="btnOpenAddStock" runat="server" Text="+ Receive New Batch" OnClick="btnOpenAddStock_Click"
            CssClass="h-10 rounded-lg bg-blue-600 px-5 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer text-sm" />
    </div>

    <%-- Tab Navigation --%>
    <div class="flex gap-2 border-b border-slate-200 pb-px mb-5">
        <asp:LinkButton ID="btnOverviewTab" runat="server" OnClick="btnOverviewTab_Click" />
        <asp:LinkButton ID="btnAddStockTab" runat="server" OnClick="btnAddStockTab_Click" />
        <asp:LinkButton ID="btnAuditTab" runat="server" OnClick="btnAuditTab_Click" />
    </div>

    <%-- ════════════════════════════════════════════════════════════
         TAB 1 — INVENTORY DASHBOARD
         ════════════════════════════════════════════════════════════ --%>
    <asp:Panel ID="panelOverview" runat="server" Visible="true" CssClass="space-y-4">

        <%-- Stats --%>
        <div class="stat-grid">
            <div class="stat-card">
                <div class="stat-number green"><asp:Label ID="lblTotalDoses" runat="server" Text="0" /></div>
                <div class="stat-label">Total Doses</div>
            </div>
            <div class="stat-card">
                <div class="stat-number blue"><asp:Label ID="lblTotalVials" runat="server" Text="0" /></div>
                <div class="stat-label">Available Vials</div>
            </div>
            <div class="stat-card">
                <div class="stat-number amber"><asp:Label ID="lblExpiring30" runat="server" Text="0" /></div>
                <div class="stat-label">Expiring (30d)</div>
            </div>
            <div class="stat-card">
                <div class="stat-number red"><asp:Label ID="lblExpired" runat="server" Text="0" /></div>
                <div class="stat-label">Expired</div>
            </div>
            <div class="stat-card">
                <div class="stat-number purple"><asp:Label ID="lblOpenVials" runat="server" Text="0" /></div>
                <div class="stat-label">Open Vials</div>
            </div>
        </div>

        <%-- Inventory grid --%>
        <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-4 py-3 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-2">
                <span class="section-title">Vaccine Stock</span>
                <div class="flex gap-2 flex-wrap">
                    <asp:TextBox ID="txtSearch" runat="server" placeholder="Search vaccine…"
                        CssClass="border border-slate-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:Button ID="btnSearch" runat="server" Text="Filter" OnClick="btnSearch_Click"
                        CssClass="bg-slate-800 text-white px-3 py-1.5 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                    <asp:Button ID="btnClearSearch" runat="server" Text="Clear" OnClick="btnClearSearch_Click"
                        CssClass="bg-white border border-slate-300 text-slate-600 px-3 py-1.5 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                </div>
            </div>

            <asp:GridView ID="gvInventory" runat="server" AutoGenerateColumns="False"
                          CssClass="gv-table w-full text-sm" GridLines="None"
                          OnRowCommand="gvInventory_RowCommand"
                          AllowPaging="True" PageSize="10"
                          OnPageIndexChanging="gvInventory_PageIndexChanging"
                          PagerStyle-CssClass="pager-wrap">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                <PagerStyle CssClass="pager-wrap" />
                <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" PageButtonCount="5" />
                <Columns>
                    <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine"
                        ItemStyle-CssClass="p-3 font-bold text-slate-700"
                        HeaderStyle-CssClass="p-3" />
                    <asp:BoundField DataField="total_batches" HeaderText="Batches"
                        ItemStyle-CssClass="p-3 text-slate-600 text-center"
                        HeaderStyle-CssClass="p-3 text-center" />
                    <asp:BoundField DataField="total_vials" HeaderText="Vials"
                        ItemStyle-CssClass="p-3 text-slate-600 text-center"
                        HeaderStyle-CssClass="p-3 text-center" />
                    <asp:BoundField DataField="total_stock" HeaderText="Doses"
                        ItemStyle-CssClass="p-3 font-extrabold text-slate-800 text-center"
                        HeaderStyle-CssClass="p-3 text-center" />
                    <asp:TemplateField HeaderStyle-CssClass="p-3 text-right" ItemStyle-CssClass="p-3 text-right">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnView" runat="server" CommandName="ViewDetails"
                                CommandArgument='<%# Container.DataItemIndex %>'
                                CssClass="text-blue-600 font-semibold text-xs hover:text-blue-800 hover:underline transition">
                                View Details
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-8 text-center text-slate-400 text-sm">No vaccine records found.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>

        <%-- Batch & Vial Details --%>
        <asp:Panel ID="panelBatchDetails" runat="server" Visible="false"
            CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-4 py-3 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                <span class="section-title">
                    <asp:Label ID="lblSelectedVaccine" runat="server" CssClass="text-blue-600" />
                    <span class="badge-count">Batches</span>
                </span>
                <asp:LinkButton ID="btnCloseDetails" runat="server" OnClick="btnCloseDetails_Click"
                    CssClass="text-slate-400 hover:text-red-500 font-bold text-lg leading-none transition">×</asp:LinkButton>
            </div>

            <div class="p-4">
                <asp:Repeater ID="rptBatches" runat="server" OnItemCommand="rptBatches_ItemCommand">
                    <ItemTemplate>
                        <div class="batch-card">
                            <div class="batch-info">
                                <span class="batch-number"><%# Eval("batch_number") %></span>
                                <span class="batch-expiry">Expires: <%# Convert.ToDateTime(Eval("expiration_date")).ToString("MMM dd, yyyy") %></span>
                                <span class="batch-stock <%# Convert.ToInt32(Eval("current_stock")) > 0 ? "green" : "red" %>">
                                    <%# Eval("current_stock") %> doses
                                </span>
                                <span class="badge <%# GetStockStatusClass(Eval("stock_status").ToString()) %>">
                                    <%# Eval("stock_status") %>
                                </span>
                            </div>
                            <div>
                                <asp:LinkButton ID="btnViewVials" runat="server" 
                                    CommandName="ViewVials"
                                    CommandArgument='<%# Eval("batch_id") %>'
                                    CssClass="text-indigo-600 font-semibold text-xs hover:text-indigo-800 transition">
                                    View Vials
                                </asp:LinkButton>
                            </div>
                        </div>
                    </ItemTemplate>
                    <EmptyDataTemplate>
                        <div class="p-6 text-center text-slate-400 text-sm">No batches available.</div>
                    </EmptyDataTemplate>
                </asp:Repeater>

                <%-- Vials --%>
                <asp:Panel ID="panelVials" runat="server" Visible="false" CssClass="mt-4 pt-4 border-t border-slate-200">
                    <div class="flex justify-between items-center mb-2">
                        <span class="section-title text-sm">
                            Vials — <asp:Label ID="lblSelectedBatch" runat="server" CssClass="text-indigo-600 font-mono" />
                        </span>
                        <asp:LinkButton ID="btnCloseVials" runat="server" OnClick="btnCloseVials_Click"
                            CssClass="text-slate-400 hover:text-red-500 font-bold text-sm transition">×</asp:LinkButton>
                    </div>

                    <div class="vial-summary">
                        <div class="vs-item"><span class="vs-dot green"></span>Sealed: <span class="vs-count"><asp:Literal ID="litSealedCount" runat="server" Text="0" /></span></div>
                        <div class="vs-item"><span class="vs-dot amber"></span>Open: <span class="vs-count"><asp:Literal ID="litOpenCount" runat="server" Text="0" /></span></div>
                        <div class="vs-item"><span class="vs-dot blue"></span>Empty: <span class="vs-count"><asp:Literal ID="litEmptyCount" runat="server" Text="0" /></span></div>
                        <div class="vs-item"><span class="vs-dot red"></span>Discarded: <span class="vs-count"><asp:Literal ID="litDiscardedCount" runat="server" Text="0" /></span></div>
                    </div>

                    <div class="vial-container">
                        <div class="vial-grid">
                            <asp:Repeater ID="rptVials" runat="server" OnItemCommand="rptVials_ItemCommand">
                                <ItemTemplate>
                                    <div class='vial-item <%# GetVialCssClass(Eval("vial_status").ToString()) %>'>
                                        <div class="vial-number"><%# Eval("vial_no") %></div>
                                        <div class="vial-status-text <%# Eval("vial_status").ToString().ToLower() %>">
                                            <%# Eval("vial_status") %>
                                        </div>
                                        <div class="vial-doses">
                                            <span><%# Eval("doses_used") %>/<%# Eval("doses_per_vial") %></span>
                                        </div>
                                        <div class="vial-actions">
                                            <asp:LinkButton ID="btnOpenVial" runat="server" 
                                                CommandName="OpenVial"
                                                CommandArgument='<%# Eval("vial_id") %>'
                                                Visible='<%# Eval("vial_status").ToString() == "Sealed" %>'
                                                CssClass="btn-sm open-btn">
                                                Open
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnUseDose" runat="server"
                                                CommandName="UseDose"
                                                CommandArgument='<%# Eval("vial_id") %>'
                                                Visible='<%# Eval("vial_status").ToString() == "Open" && Convert.ToInt32(Eval("doses_used")) < Convert.ToInt32(Eval("doses_per_vial")) %>'
                                                CssClass="btn-sm use-btn">
                                                Use
                                            </asp:LinkButton>
                                            <asp:LinkButton ID="btnDiscardVial" runat="server"
                                                CommandName="DiscardVial"
                                                CommandArgument='<%# Eval("vial_id") %>'
                                                Visible='<%# Eval("vial_status").ToString() == "Open" || Eval("vial_status").ToString() == "Sealed" %>'
                                                CssClass="btn-sm discard-btn">
                                                Discard
                                            </asp:LinkButton>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </asp:Panel>
            </div>
        </asp:Panel>

    </asp:Panel>

    <%-- ════════════════════════════════════════════════════════════
         TAB 2 — RECEIVE STOCK
         ════════════════════════════════════════════════════════════ --%>
    <asp:Panel ID="panelAddStock" runat="server" Visible="false">
        <div class="grid grid-cols-1 lg:grid-cols-5 gap-5">

            <div class="lg:col-span-2 bg-white border border-slate-200 rounded-xl shadow-sm p-6">
                <h2 class="text-xl font-bold mb-1 text-[#0b2a7a]">Receive New Batch</h2>
                <p class="text-sm text-slate-400 mb-4">Add incoming vaccine stock to inventory.</p>

                <div class="space-y-4">
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Vaccine <span class="text-red-500">*</span></label>
                        <asp:DropDownList ID="ddlVaccineName" runat="server"
                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white" />
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Expiry Date <span class="text-red-500">*</span></label>
                        <asp:TextBox ID="txtExpiryDate" runat="server" TextMode="Date"
                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Doses per Vial <span class="text-red-500">*</span></label>
                        <asp:DropDownList ID="ddlDosesPerVial" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                            <asp:ListItem Text="1 (Single)" Value="1" />
                            <asp:ListItem Text="2" Value="2" />
                            <asp:ListItem Text="5" Value="5" />
                            <asp:ListItem Text="10" Value="10" />
                        </asp:DropDownList>
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Number of Vials <span class="text-red-500">*</span></label>
                        <asp:TextBox ID="txtVialCount" runat="server" TextMode="Number"
                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="e.g. 10" />
                    </div>
                    <div class="bg-slate-50 rounded-lg p-3 flex justify-between items-center">
                        <span class="text-xs font-bold text-slate-500 uppercase tracking-wider">Total Doses</span>
                        <span id="totalDosesDisplay" class="text-2xl font-extrabold text-blue-600">0</span>
                    </div>

                    <div class="pt-2 flex gap-3">
                        <asp:Button ID="btnSaveStock" runat="server" Text="Save Batch" OnClick="btnSaveStock_Click"
                            CssClass="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2 rounded-lg font-bold cursor-pointer transition text-sm" />
                        <asp:Button ID="btnCancelStock" runat="server" Text="Cancel" OnClick="btnCancelStock_Click"
                            CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2 rounded-lg font-bold cursor-pointer transition text-sm" />
                    </div>
                </div>
            </div>

            <div class="lg:col-span-3 bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-4 py-3 border-b border-slate-200 bg-slate-50">
                    <span class="section-title">Recent Stock Receipts</span>
                </div>

                <asp:GridView ID="gvStockHistory" runat="server" AutoGenerateColumns="False"
                              CssClass="gv-table w-full text-sm" GridLines="None"
                              AllowPaging="True" PageSize="10"
                              OnPageIndexChanging="gvStockHistory_PageIndexChanging"
                              PagerStyle-CssClass="pager-wrap">
                    <HeaderStyle CssClass="text-left bg-slate-50 text-slate-400 border-b border-slate-200 uppercase text-xs font-bold" />
                    <RowStyle CssClass="border-b border-slate-50 transition-colors" />
                    <PagerStyle CssClass="pager-wrap" />
                    <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" PageButtonCount="5" />
                    <Columns>
                        <asp:BoundField DataField="transaction_date" HeaderText="Date"
                            DataFormatString="{0:MMM dd, yyyy}"
                            ItemStyle-CssClass="p-3 text-slate-500 text-xs whitespace-nowrap"
                            HeaderStyle-CssClass="p-3" />
                        <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine"
                            ItemStyle-CssClass="p-3 font-semibold text-slate-700"
                            HeaderStyle-CssClass="p-3" />
                        <asp:BoundField DataField="batch_number" HeaderText="Batch"
                            ItemStyle-CssClass="p-3 font-mono text-slate-500 text-xs"
                            HeaderStyle-CssClass="p-3" />
                        <asp:BoundField DataField="vials" HeaderText="Vials"
                            ItemStyle-CssClass="p-3 text-slate-600 text-center"
                            HeaderStyle-CssClass="p-3 text-center" />
                        <asp:TemplateField HeaderText="Doses" HeaderStyle-CssClass="p-3 text-center" ItemStyle-CssClass="p-3 text-center">
                            <ItemTemplate>
                                <span class="badge badge-in">+<%# Eval("quantity") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="updated_by" HeaderText="By"
                            ItemStyle-CssClass="p-3 text-slate-500 text-xs"
                            HeaderStyle-CssClass="p-3" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="p-8 text-center text-slate-400 text-sm">No stock history yet.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

        </div>
    </asp:Panel>

    <%-- ════════════════════════════════════════════════════════════
         TAB 3 — AUDIT LOG
         ════════════════════════════════════════════════════════════ --%>
    <asp:Panel ID="panelAudit" runat="server" Visible="false">
        <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-4 py-3 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-2">
                <span class="section-title">Audit Log</span>
                <div class="flex gap-2 flex-wrap">
                    <asp:TextBox ID="txtAuditSearch" runat="server" placeholder="Search…"
                        CssClass="border border-slate-300 rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:Button ID="btnAuditSearch" runat="server" Text="Filter" OnClick="btnAuditSearch_Click"
                        CssClass="bg-slate-800 text-white px-3 py-1.5 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                    <asp:Button ID="btnAuditClear" runat="server" Text="Clear" OnClick="btnAuditClear_Click"
                        CssClass="bg-white border border-slate-300 text-slate-600 px-3 py-1.5 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                </div>
            </div>

            <asp:GridView ID="gvAuditLog" runat="server" AutoGenerateColumns="False"
                          CssClass="gv-table w-full text-sm" GridLines="None"
                          AllowPaging="True" PageSize="15"
                          OnPageIndexChanging="gvAuditLog_PageIndexChanging"
                          PagerStyle-CssClass="pager-wrap">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-50 transition-colors" />
                <PagerStyle CssClass="pager-wrap" />
                <PagerSettings Mode="NumericFirstLast" FirstPageText="First" LastPageText="Last" PageButtonCount="5" />
                <Columns>
                    <asp:BoundField DataField="performed_at" HeaderText="Date/Time"
                        DataFormatString="{0:MMM dd, yyyy HH:mm}"
                        ItemStyle-CssClass="p-3 text-slate-500 text-xs whitespace-nowrap"
                        HeaderStyle-CssClass="p-3" />
                    <asp:BoundField DataField="table_name" HeaderText="Table"
                        ItemStyle-CssClass="p-3 font-mono text-xs text-slate-600"
                        HeaderStyle-CssClass="p-3" />
                    <asp:BoundField DataField="record_id" HeaderText="Record"
                        ItemStyle-CssClass="p-3 font-mono text-xs text-slate-600"
                        HeaderStyle-CssClass="p-3" />
                    <asp:TemplateField HeaderText="Action" HeaderStyle-CssClass="p-3 text-center" ItemStyle-CssClass="p-3 text-center">
                        <ItemTemplate>
                            <span class='badge <%# GetActionBadgeClass(Eval("action").ToString()) %>'>
                                <%# Eval("action") %>
                            </span>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:BoundField DataField="performed_by" HeaderText="User"
                        ItemStyle-CssClass="p-3 text-slate-600"
                        HeaderStyle-CssClass="p-3" />
                    <asp:BoundField DataField="ip_address" HeaderText="IP"
                        ItemStyle-CssClass="p-3 text-slate-400 text-xs font-mono"
                        HeaderStyle-CssClass="p-3" />
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-8 text-center text-slate-400 text-sm">No audit records found.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </div>
    </asp:Panel>

</div>

<%-- ── Notify Modal ──────────────────────────────────────────── --%>
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

    // Calculate total doses
    document.addEventListener('DOMContentLoaded', function() {
        var vialCount = document.getElementById('<%= txtVialCount.ClientID %>');
        var dosesPerVial = document.getElementById('<%= ddlDosesPerVial.ClientID %>');
        var display = document.getElementById('totalDosesDisplay');

        function updateTotal() {
            var vc = parseInt(vialCount.value) || 0;
            var dpv = parseInt(dosesPerVial.value) || 1;
            display.textContent = vc * dpv;
        }

        if (vialCount) vialCount.addEventListener('input', updateTotal);
        if (dosesPerVial) dosesPerVial.addEventListener('change', updateTotal);
        updateTotal();
    });
</script>
</asp:Content>