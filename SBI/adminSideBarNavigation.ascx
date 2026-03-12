<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="adminSideBarNavigation.ascx.cs" Inherits="SBI.adminSideBarNavigation" %>

<div class="bg-sidebar text-white w-64 min-h-screen flex flex-col">

    <!-- LOGO -->
    <section class="bg-white h-35 py-4 flex items-center justify-center border-b border-gray-200">
        <img src="Icons/logo.png" class="h-20 w-auto object-contain" />
    </section>

    <!-- MENU -->
    <div class="p-3 space-y-2">

        <h2 class="font-heading2 text-xs tracking-[0.2em] mb-2 px-2 opacity-70">ADMINISTRATION
        </h2>

        <asp:LinkButton ID="lnkUserManagement" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnUserManagement_Click">

            <img src="Icons/userManagement.svg" class="h-5 w-5 flex-shrink-0" />
            <span class="font-medium text-sm">User Management</span>

        </asp:LinkButton>

        <asp:LinkButton ID="lnkSettings" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnSettings_Click">

            <img src="Icons/settings.svg" class="h-5 w-5 flex-shrink-0" />
            <span class="font-medium text-sm">Settings</span>

        </asp:LinkButton>

        <asp:LinkButton ID="lnkSignOut" runat="server"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full mt-8"
            OnClick="btnSignOut_Click">

            <img src="Icons/SignOut.svg" class="h-5 w-5 flex-shrink-0" />
            <span class="font-medium text-sm">Sign Out</span>

        </asp:LinkButton>

    </div>

</div>
