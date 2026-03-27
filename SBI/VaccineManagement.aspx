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

    /* ── Modal animation ────────────────────────────────────────── */
    @keyframes fadeIn {
        from { opacity:0; transform:translateY(8px) scale(.98); }
        to   { opacity:1; transform:translateY(0) scale(1); }
    }
</style>

<div class="p-6 font-heading2 text-slate-900">

    <%-- ── Page Header ───────────────────────────────────────────── --%>
    <div class="flex justify-between items-start mb-6">
        <div>
            <h1 class="text-4xl text-[#0b2a7a] font-heading2 tracking-widest">Vaccine Management</h1>
            <p class="text-slate-500 text-sm mt-1">Inventory control and stock monitoring</p>
        </div>
        <asp:Button ID="btnOpenAddStock" runat="server" Text="+ Add New Stock" OnClick="btnOpenAddStock_Click"
            CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer" />
    </div>

    <%-- ── Tab Navigation ────────────────────────────────────────── --%>
    <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
        <asp:LinkButton ID="btnOverviewTab" runat="server" OnClick="btnOverviewTab_Click" />
        <asp:LinkButton ID="btnAddStockTab" runat="server" OnClick="btnAddStockTab_Click" />
    </div>

    <%-- ════════════════════════════════════════════════════════════
         TAB 1 — INVENTORY DASHBOARD
         ════════════════════════════════════════════════════════════ --%>
    <asp:Panel ID="panelOverview" runat="server" Visible="true" CssClass="space-y-6">

        <%-- Stat cards --%>
        <div class="grid grid-cols-1 sm:grid-cols-2 xl:grid-cols-4 gap-4">
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Available Doses</div>
                <div class="text-3xl font-extrabold text-emerald-600"><asp:Label ID="lblTotalDoses" runat="server" /></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Expiring (30d)</div>
                <div class="text-3xl font-extrabold text-amber-500"><asp:Label ID="lblExpiring30" runat="server" /></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Expired</div>
                <div class="text-3xl font-extrabold text-red-600"><asp:Label ID="lblExpired" runat="server" /></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Stock Entries (MTD)</div>
                <div class="text-3xl font-extrabold text-indigo-600"><asp:Label ID="lblAdministeredMTD" runat="server" /></div>
            </div>
        </div>

        <%-- Inventory grid --%>
        <asp:Panel ID="panelInventory" runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                <h3 class="font-extrabold text-slate-800">Current Stock by Vaccine</h3>
                <div class="flex gap-2">
                    <asp:TextBox ID="txtSearch" runat="server" placeholder="Search vaccine…"
                        CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    <asp:Button ID="btnSearch" runat="server" Text="Filter" OnClick="btnSearch_Click"
                        CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                    <asp:Button ID="btnClearSearch" runat="server" Text="Clear" OnClick="btnClearSearch_Click"
                        CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
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
                <PagerSettings Mode="NumericFirstLast" FirstPageText="«" LastPageText="»" PageButtonCount="5" />
                <Columns>
                    <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine"
                        ItemStyle-CssClass="p-4 font-bold text-slate-700"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="total_batches" HeaderText="Batches"
                        ItemStyle-CssClass="p-4 text-slate-600"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="total_stock" HeaderText="Total Qty"
                        ItemStyle-CssClass="p-4 font-extrabold text-slate-800"
                        HeaderStyle-CssClass="p-4" />
                    <asp:TemplateField HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                        <ItemTemplate>
                            <asp:LinkButton ID="btnView" runat="server" CommandName="ViewDetails"
                                CommandArgument='<%# Container.DataItemIndex %>'
                                CssClass="inline-flex items-center gap-1 text-blue-600 font-semibold text-xs hover:text-blue-800 hover:underline transition">
                                View Batches →
                            </asp:LinkButton>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-10 text-center text-slate-400 text-sm">No vaccine records found.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </asp:Panel>

        <%-- Batch details (inline expand) --%>
        <asp:Panel ID="panelBatchDetails" runat="server" Visible="false"
            CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                <h3 class="font-extrabold text-slate-800">
                    Batch Breakdown — <asp:Label ID="lblSelectedVaccine" runat="server" CssClass="text-blue-600" />
                </h3>
                <asp:LinkButton ID="btnCloseDetails" runat="server" OnClick="btnCloseDetails_Click"
                    CssClass="text-slate-400 hover:text-red-500 font-bold text-lg leading-none transition">✕</asp:LinkButton>
            </div>

            <asp:GridView ID="gvBatchDetails" runat="server" AutoGenerateColumns="False"
                          CssClass="gv-table w-full text-sm" GridLines="None"
                          AllowPaging="True" PageSize="10"
                          OnPageIndexChanging="gvBatchDetails_PageIndexChanging"
                          PagerStyle-CssClass="pager-wrap">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-100" />
                <PagerStyle CssClass="pager-wrap" />
                <PagerSettings Mode="NumericFirstLast" FirstPageText="«" LastPageText="»" PageButtonCount="5" />
                <Columns>
                    <asp:BoundField DataField="batch_number" HeaderText="Batch #"
                        ItemStyle-CssClass="p-4 font-mono text-slate-700"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="expiration_date" HeaderText="Expiry"
                        DataFormatString="{0:MMM dd, yyyy}"
                        ItemStyle-CssClass="p-4 text-slate-600"
                        HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="current_stock" HeaderText="Stock"
                        ItemStyle-CssClass="p-4 font-extrabold text-slate-800"
                        HeaderStyle-CssClass="p-4" />
                    <asp:TemplateField HeaderText="Status" HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                        <ItemTemplate>
                            <%# FormatStockStatus(Eval("stock_status").ToString()) %>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-10 text-center text-slate-400 text-sm">No batch data available.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </asp:Panel>

    </asp:Panel>

    <%-- ════════════════════════════════════════════════════════════
         TAB 2 — RECEIVE STOCK  (form + recent history side by side)
         ════════════════════════════════════════════════════════════ --%>
    <asp:Panel ID="panelAddStock" runat="server" Visible="false">
        <div class="grid grid-cols-1 xl:grid-cols-5 gap-6">

            <%-- LEFT: form (2/5) --%>
            <div class="xl:col-span-2 bg-white border border-slate-200 rounded-xl shadow-sm p-8">
                <h2 class="text-2xl font-bold mb-1 font-heading1 text-[#0b2a7a]">Receive New Batch</h2>
                <p class="text-sm text-slate-400 mb-6">Add incoming vaccine stock to the inventory.</p>

                <div class="space-y-5">
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Vaccine</label>
                        <asp:DropDownList ID="ddlVaccineName" runat="server"
                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white" />
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Expiry Date</label>
                        <asp:TextBox ID="txtExpiryDate" runat="server" TextMode="Date"
                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500" />
                    </div>
                    <div>
                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Quantity Received</label>
                        <asp:TextBox ID="txtQuantity" runat="server" TextMode="Number"
                            CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                            placeholder="e.g. 50" />
                    </div>

                    <div class="pt-2 flex gap-3">
                        <asp:Button ID="btnSaveStock" runat="server" Text="Save Batch" OnClick="btnSaveStock_Click"
                            CssClass="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm" />
                        <asp:Button ID="btnCancelStock" runat="server" Text="Cancel" OnClick="btnCancelStock_Click"
                            CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm" />
                    </div>
                </div>
            </div>

            <%-- RIGHT: recent history (3/5) --%>
            <div class="xl:col-span-3 bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Recent Stock Receipts</h3>
                    <p class="text-xs text-slate-400 mt-0.5">Last 50 stock-in transactions</p>
                </div>

                <asp:GridView ID="gvStockHistory" runat="server" AutoGenerateColumns="False"
                              CssClass="gv-table w-full text-sm" GridLines="None"
                              AllowPaging="True" PageSize="10"
                              OnPageIndexChanging="gvStockHistory_PageIndexChanging"
                              PagerStyle-CssClass="pager-wrap">
                    <HeaderStyle CssClass="text-left bg-slate-50 text-slate-400 border-b border-slate-200 uppercase text-xs font-bold" />
                    <RowStyle CssClass="border-b border-slate-50 transition-colors" />
                    <PagerStyle CssClass="pager-wrap" />
                    <PagerSettings Mode="NumericFirstLast" FirstPageText="«" LastPageText="»" PageButtonCount="5" />
                    <Columns>
                        <asp:BoundField DataField="transaction_date" HeaderText="Date"
                            DataFormatString="{0:MMM dd, yyyy}"
                            ItemStyle-CssClass="p-4 text-slate-500 text-xs whitespace-nowrap"
                            HeaderStyle-CssClass="p-4" />
                        <asp:BoundField DataField="vaccine_name" HeaderText="Vaccine"
                            ItemStyle-CssClass="p-4 font-semibold text-slate-700"
                            HeaderStyle-CssClass="p-4" />
                        <asp:BoundField DataField="batch_number" HeaderText="Batch #"
                            ItemStyle-CssClass="p-4 font-mono text-slate-500 text-xs"
                            HeaderStyle-CssClass="p-4" />
                        <%-- Single Qty column as badge only — removed the plain BoundField duplicate --%>
                        <asp:TemplateField HeaderText="Qty" HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                            <ItemTemplate>
                                <span class="badge badge-in">+<%# Eval("quantity") %></span>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:BoundField DataField="updated_by" HeaderText="By"
                            ItemStyle-CssClass="p-4 text-slate-500 text-xs"
                            HeaderStyle-CssClass="p-4" />
                    </Columns>
                    <EmptyDataTemplate>
                        <div class="p-10 text-center text-slate-400 text-sm">No stock history yet.</div>
                    </EmptyDataTemplate>
                </asp:GridView>
            </div>

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
</script>
</asp:Content>
