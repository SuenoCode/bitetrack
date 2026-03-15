<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="SBI.UserManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>SBI User Management</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfEditMode" runat="server" Value="false" />
    <asp:HiddenField ID="hfEditUserId" runat="server" Value="" />

    <div class="p-6 font-heading2 text-slate-900">

        <%-- ── Page Header ─────────────────────────────────────────── --%>
        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl font-bold text-[#0b2a7a] font-hBruns">User Management</h1>
                <p class="text-slate-500 text-sm mt-1">Manage admin accounts, roles, and access</p>
            </div>
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">

            <%-- ── LEFT: Add / Edit User Form ─────────────────────── --%>
            <div class="lg:col-span-1 space-y-6">

                <asp:Panel ID="panelUserForm" runat="server"
                    CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">

                    <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                        <div>
                            <h3 class="font-extrabold text-slate-800">
                                <asp:Literal ID="litFormTitle" runat="server" Text="Add New User" />
                            </h3>
                            <p class="text-xs text-slate-400 mt-0.5">Fill in the details then click Save</p>
                        </div>
                    </div>

                    <div class="p-5 space-y-4">

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
                            <div class="relative">
                                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"
                                    CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 pr-10"
                                    placeholder="••••••••" />
                                <button type="button" onclick="togglePassword()"
                                    class="absolute right-3 top-1/2 -translate-y-1/2 text-slate-400 hover:text-slate-700 transition text-xs font-bold">
                                    Show
                                </button>
                            </div>
                            <%-- shown only in edit mode --%>
                            <asp:Panel ID="panelCurrentPassword" runat="server" Visible="false"
                                CssClass="mt-2 bg-slate-50 border border-slate-200 rounded-lg px-3 py-2 text-xs text-slate-500">
                                Current (hashed): <span class="font-mono break-all"><asp:Literal ID="litCurrentPassword" runat="server" /></span>
                            </asp:Panel>
                            <p class="text-[11px] text-slate-400 mt-1">Leave blank to keep existing password when editing.</p>
                        </div>

                        <div>
                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Role</label>
                            <asp:DropDownList ID="ddlRole" runat="server"
                                CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 bg-white">
                                <asp:ListItem Text="-- Select Role --"   Value=""  />
                                <asp:ListItem Text="Admin"               Value="A" />
                                <asp:ListItem Text="Admin Assistant"     Value="B" />
                                <asp:ListItem Text="Vaccinator"          Value="C" />
                            </asp:DropDownList>
                        </div>

                        <asp:Label ID="lblFormError" runat="server" Visible="false"
                            CssClass="block text-xs text-red-600 font-semibold" />

                        <div class="flex gap-3 pt-1">
                            <asp:Button ID="btnSave" runat="server" Text="Save User"
                                CssClass="flex-1 bg-blue-600 hover:bg-blue-700 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                OnClick="btnSave_Click" />
                            <asp:Button ID="btnCancelEdit" runat="server" Text="Cancel"
                                CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                OnClick="btnCancelEdit_Click" Visible="false" />
                        </div>

                    </div>
                </asp:Panel>

            </div>

            <%-- ── RIGHT: Users Table ──────────────────────────────── --%>
            <div class="lg:col-span-2 flex flex-col gap-6">

                <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">

                    <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex justify-between items-center">
                        <h3 class="font-extrabold text-slate-800">Users</h3>
                        <span class="text-slate-500 text-sm">Click <strong>Edit</strong> or <strong>Delete</strong> from the Actions column</span>
                    </div>

                    <asp:GridView ID="gvUsers" runat="server"
                        AutoGenerateColumns="False"
                        DataKeyNames="user_id"
                        OnRowCommand="gvUsers_RowCommand"
                        CssClass="w-full text-sm" GridLines="None">
                        <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                        <RowStyle CssClass="border-b border-slate-100 transition-colors hover:bg-slate-50" />
                        <AlternatingRowStyle CssClass="border-b border-slate-100" />
                        <Columns>
                            <asp:BoundField DataField="user_id"   HeaderText="ID"
                                ItemStyle-CssClass="p-4 text-slate-500 text-xs"
                                HeaderStyle-CssClass="p-4" />
                            <asp:BoundField DataField="full_name" HeaderText="Full Name"
                                ItemStyle-CssClass="p-4 font-bold text-slate-700"
                                HeaderStyle-CssClass="p-4" />
                            <asp:BoundField DataField="username"  HeaderText="Username"
                                ItemStyle-CssClass="p-4 text-slate-600"
                                HeaderStyle-CssClass="p-4" />
                            <asp:TemplateField HeaderText="Password" HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                <ItemTemplate>
                                    <div class="flex items-center gap-2">
                                        <span class="font-mono text-xs text-slate-400 password-mask">••••••••</span>
                                        <span class="font-mono text-xs text-slate-600 password-plain hidden"><%# Eval("password") %></span>
                                        <button type="button"
                                            onclick="toggleRowPassword(this)"
                                            class="text-[11px] text-blue-500 hover:text-blue-700 font-bold transition">
                                            Show
                                        </button>
                                    </div>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Role" HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                <ItemTemplate>
                                    <span class='<%# GetRoleBadgeClass(Eval("role").ToString()) %>'>
                                        <%# GetRoleLabel(Eval("role").ToString()) %>
                                    </span>
                                </ItemTemplate>
                            </asp:TemplateField>
                            <asp:TemplateField HeaderText="Actions" HeaderStyle-CssClass="p-4 text-right" ItemStyle-CssClass="p-4 text-right">
                                <ItemTemplate>
                                    <asp:Button ID="btnEdit" runat="server"
                                        CommandName="EditUser"
                                        CommandArgument='<%# Container.DataItemIndex %>'
                                        Text="Edit"
                                        CssClass="bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 font-bold py-1.5 px-3 rounded-lg text-xs transition cursor-pointer mr-1" />
                                    <asp:Button ID="btnDelete" runat="server"
                                        CommandName="DeleteUser"
                                        CommandArgument='<%# Container.DataItemIndex %>'
                                        Text="Delete"
                                        OnClientClick="return confirm('Delete this user?');"
                                        CssClass="bg-red-50 border border-red-200 hover:bg-red-100 text-red-600 font-bold py-1.5 px-3 rounded-lg text-xs transition cursor-pointer" />
                                </ItemTemplate>
                            </asp:TemplateField>
                        </Columns>
                        <EmptyDataTemplate>
                            <div class="p-10 text-center text-slate-400 text-sm">No users found.</div>
                        </EmptyDataTemplate>
                    </asp:GridView>

                    <div class="px-5 py-3 border-t border-slate-200 bg-slate-50">
                        <p class="text-slate-500 text-sm">Tip: Use <strong>Show</strong> to reveal a stored password inline.</p>
                    </div>

                </asp:Panel>

            </div>

        </div>
    </div>

    <script>
        // Toggle password in the Add/Edit form
        function togglePassword() {
            var tb = document.querySelector('#<%= txtPassword.ClientID %>');
            var btn = tb ? tb.nextElementSibling : null;
            if (!tb || !btn) return;
            if (tb.type === 'password') { tb.type = 'text'; btn.textContent = 'Hide'; }
            else { tb.type = 'password'; btn.textContent = 'Show'; }
        }

        // Toggle password in a grid row
        function toggleRowPassword(btn) {
            var cell = btn.parentElement;
            var mask = cell.querySelector('.password-mask');
            var plain = cell.querySelector('.password-plain');
            if (!mask || !plain) return;
            var hidden = plain.classList.contains('hidden');
            plain.classList.toggle('hidden', !hidden);
            mask.classList.toggle('hidden', hidden);
            btn.textContent = hidden ? 'Hide' : 'Show';
        }
    </script>

</asp:Content>