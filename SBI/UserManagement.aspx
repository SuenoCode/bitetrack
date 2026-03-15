<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="SBI.UserManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>SBI User Management</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="p-6 font-heading2 text-slate-900">

        <%-- ── Page Header ───────────────────────────────────────────── --%>
        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl font-bold text-[#0b2a7a] font-hBruns">User Management</h1>
                <p class="text-slate-500 text-sm mt-1">Manage admin accounts, roles, and access</p>
            </div>
            <asp:Button ID="btnAddUser" runat="server"
                Text="+ Add User"
                UseSubmitBehavior="false"
                OnClientClick="openModal(); return false;"
                CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer" />
        </div>

        <%-- Users Table --%>
        <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                <h3 class="font-extrabold text-slate-800">Users</h3>
                <span class="text-xs font-semibold text-slate-400">Edit / Delete from the Actions column</span>
            </div>

            <asp:GridView ID="gvUsers" runat="server"
                AutoGenerateColumns="False"
                DataKeyNames="user_id"
                OnRowEditing="gvUsers_RowEditing"
                OnRowCancelingEdit="gvUsers_RowCancelingEdit"
                OnRowUpdating="gvUsers_RowUpdating"
                OnRowDeleting="gvUsers_RowDeleting"
                CssClass="w-full text-sm" GridLines="None">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                <AlternatingRowStyle CssClass="border-b border-slate-100 hover:bg-slate-50" />
                <Columns>
                    <asp:BoundField DataField="user_id"   HeaderText="Admin ID"  ReadOnly="True" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="full_name" HeaderText="Full Name"               ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="username"  HeaderText="Username"                ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="password"  HeaderText="Password"                ItemStyle-CssClass="p-4 font-mono text-slate-500 text-xs" HeaderStyle-CssClass="p-4" />
                    <asp:BoundField DataField="role"      HeaderText="Role"                    ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                    <asp:CommandField ShowEditButton="True" ShowDeleteButton="True"
                        ControlStyle-CssClass="font-bold text-blue-600 hover:text-blue-800 hover:underline transition mr-2" />
                </Columns>
                <EmptyDataTemplate>
                    <div class="p-10 text-center text-slate-400 text-sm">No users found.</div>
                </EmptyDataTemplate>
            </asp:GridView>

            <div class="px-5 py-3 border-t border-slate-200 bg-slate-50">
                <p class="text-xs font-semibold text-slate-400">Tip: Avoid displaying raw passwords in production — use hashed passwords.</p>
            </div>
        </asp:Panel>

    </div>

    <%-- ── Add User Modal ────────────────────────────────────────── --%>
    <div id="userModal" class="fixed inset-0 z-50 hidden items-center justify-center bg-slate-900/50 px-4">
        <div class="w-full max-w-md rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden">

            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                <div>
                    <h3 class="font-extrabold text-slate-900">Add New User</h3>
                    <p class="text-xs text-slate-400 mt-0.5">Fill in the details then click Save</p>
                </div>
                <button type="button" onclick="closeModal()"
                    class="text-slate-400 hover:text-red-500 font-bold text-lg leading-none transition">✕</button>
            </div>

            <div class="p-5 space-y-5">
                <div>
                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Full Name</label>
                    <asp:TextBox ID="txtFullName" runat="server"
                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="e.g. Juan Dela Cruz" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server"
                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="e.g. admin.juan" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"
                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                        placeholder="••••••••" />
                </div>
                <div>
                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Role</label>
                    <asp:DropDownList ID="ddlRole" runat="server"
                        CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                        <asp:ListItem Text="Select Role"       Value=""               />
                        <asp:ListItem Text="Admin"             Value="admin"          />
                        <asp:ListItem Text="Admin Assistant"   Value="adminAssistant" />
                        <asp:ListItem Text="Vaccinator"        Value="vaccinators"    />
                    </asp:DropDownList>
                </div>
            </div>

            <div class="px-5 py-4 bg-slate-50 border-t border-slate-200 flex justify-end gap-3">
                <asp:Button ID="btnCancel" runat="server"
                    Text="Cancel"
                    UseSubmitBehavior="false"
                    OnClientClick="closeModal(); return false;"
                    CssClass="h-11 rounded-lg bg-white border border-slate-300 px-6 font-bold text-slate-700 hover:bg-slate-50 transition cursor-pointer" />
                <asp:Button ID="btnSave" runat="server"
                    Text="Save"
                    OnClick="btnSave_Click"
                    CssClass="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer" />
            </div>

        </div>
    </div>

    <script>
        function openModal() {
            var m = document.getElementById('userModal');
            m.classList.remove('hidden'); m.classList.add('flex');
        }
        function closeModal() {
            var m = document.getElementById('userModal');
            m.classList.remove('flex'); m.classList.add('hidden');
        }
        document.addEventListener('click', function (e) {
            var m = document.getElementById('userModal');
            if (e.target === m) closeModal();
        });
    </script>

</asp:Content>
