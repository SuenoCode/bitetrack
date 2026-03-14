<%@ Page Language="C#" MasterPageFile="~/Admin.Master" AutoEventWireup="true" CodeBehind="UserManagement.aspx.cs" Inherits="SBI.UserManagement" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>SBI User Management</title>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="mx-auto max-w-6xl px-4 py-6 font-sans text-slate-900">

        <!-- Header -->
        <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-end sm:justify-between">
            <div>
                <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Admin User Management</h1>
                <p class="mt-1 text-base text-slate-600">Manage admin accounts, roles, and access</p>
            </div>

            <asp:Button ID="btnAddUser" runat="server"
                Text="Add User"
                UseSubmitBehavior="false"
                OnClientClick="openModal(); return false;"
                CssClass="h-11 rounded-lg bg-[#1a4ed8] px-5 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition" />
        </div>

        <!-- Table Card -->
        <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">

            <div class="flex items-center justify-between gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                <h2 class="text-lg font-extrabold text-slate-900">Users</h2>
                <span class="text-sm font-semibold text-slate-500">Edit / Delete from the Actions column</span>
            </div>

            <div class="w-full overflow-x-auto">
                <asp:GridView ID="gvUsers" runat="server"
    AutoGenerateColumns="False"
    DataKeyNames="user_id"
    OnRowEditing="gvUsers_RowEditing"
    OnRowCancelingEdit="gvUsers_RowCancelingEdit"
    OnRowUpdating="gvUsers_RowUpdating"
    OnRowDeleting="gvUsers_RowDeleting"
    CssClass="min-w-[900px] w-full text-sm"
    HeaderStyle-CssClass="bg-slate-50 text-slate-700 font-extrabold border-b border-slate-200"
    RowStyle-CssClass="border-b border-slate-100"
    AlternatingRowStyle-CssClass="bg-white"
    GridLines="None">

    <Columns>
        <asp:BoundField DataField="user_id"   HeaderText="Admin ID"  ReadOnly="True" />
        <asp:BoundField DataField="full_name" HeaderText="Full Name" />
        <asp:BoundField DataField="username"  HeaderText="Username"  />
        <asp:BoundField DataField="password"  HeaderText="Password"  />
        <asp:BoundField DataField="role"      HeaderText="Role"      />

        <asp:CommandField ShowEditButton="True" ShowDeleteButton="True"
            ControlStyle-CssClass="font-bold text-[#1a4ed8] hover:text-[#0b2a7a]" />
    </Columns>
</asp:GridView>
            </div>

            <!-- Optional footer space -->
            <div class="border-t border-slate-200 bg-slate-50 px-5 py-3 text-xs font-semibold text-slate-500">
                Tip: Avoid displaying raw passwords in production (use hashed passwords).
            </div>

        </div>

    </div>

    <!-- MODAL -->
    <div id="userModal" class="fixed inset-0 z-50 hidden items-center justify-center bg-black/40 p-4">

        <div class="w-full max-w-md overflow-hidden rounded-2xl bg-white shadow-2xl">

            <!-- Modal header -->
            <div class="flex items-center justify-between border-b border-slate-200 bg-slate-50 px-5 py-4">
                <div>
                    <h2 class="text-lg font-extrabold text-slate-900">Add New User</h2>
                    <p class="mt-0.5 text-sm font-semibold text-slate-500">Fill in the details then click Save</p>
                </div>

                <button type="button" onclick="closeModal()"
                    class="rounded-lg px-3 py-2 text-slate-500 hover:bg-slate-100 hover:text-slate-700 transition">
                    ✕
                </button>
            </div>

            <!-- Modal body -->
            <div class="px-5 py-5 space-y-4">

                <div>
                    <label class="mb-2 block text-sm font-semibold text-slate-700">Full Name</label>
                    <asp:TextBox ID="txtFullName" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        placeholder="e.g. Juan Dela Cruz" />
                </div>

                <div>
                    <label class="mb-2 block text-sm font-semibold text-slate-700">Username</label>
                    <asp:TextBox ID="txtUsername" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        placeholder="e.g. admin.juan" />
                </div>

                <div>
                    <label class="mb-2 block text-sm font-semibold text-slate-700">Password</label>
                    <asp:TextBox ID="txtPassword" runat="server" TextMode="Password"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        placeholder="••••••••" />
                </div>

                <div>
                    <label class="mb-2 block text-sm font-semibold text-slate-700">Role</label>
                    <asp:DropDownList ID="ddlRole" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                        <asp:ListItem Text="Select Role"       Value=""               />
                        <asp:ListItem Text="Admin"             Value="admin"          />
                        <asp:ListItem Text="Admin Assistant"   Value="adminAssistant" />
                        <asp:ListItem Text="Vaccinator"        Value="vaccinators"    />
                    </asp:DropDownList>
                </div>

            </div>

            <!-- Modal footer -->
            <div class="flex flex-col gap-3 border-t border-slate-200 bg-slate-50 px-5 py-4 sm:flex-row sm:justify-end">

                <asp:Button ID="btnCancel" runat="server"
                    Text="Cancel"
                    UseSubmitBehavior="false"
                    OnClientClick="closeModal(); return false;"
                    CssClass="h-11 rounded-lg border border-slate-200 bg-white px-5 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition" />

                <asp:Button ID="btnSave" runat="server"
                Text="Save"
                OnClick="btnSave_Click"
                CssClass="h-11 rounded-lg bg-[#1a4ed8] px-6 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition" />

            </div>

        </div>
    </div>

    <script>
        function openModal() {
            const modal = document.getElementById("userModal");
            modal.classList.remove("hidden");
            modal.classList.add("flex");
        }

        function closeModal() {
            const modal = document.getElementById("userModal");
            modal.classList.remove("flex");
            modal.classList.add("hidden");
        }

        document.addEventListener("click", function (e) {
            const modal = document.getElementById("userModal");
            if (e.target === modal) closeModal();
        });
    </script>

</asp:Content>