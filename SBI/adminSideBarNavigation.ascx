<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="adminSideBarNavigation.ascx.cs" Inherits="SBI.adminSideBarNavigation" %>

<div class="bg-sidebar text-white w-75 min-h-screen flex flex-col">

    <!-- LOGO -->
    <section class="bg-white h-35 py-4 flex items-center justify-center border-b border-gray-200">
        <img src="Icons/logo.png" class="h-20 w-auto object-contain" />
    </section>

    <!-- MENU -->
    <div class="p-3 space-y-2">
        <!-- MAIN NAVIGATION -->
        <h2 runat="server" ID="mainNavigation"  class="font-heading2 text-xs tracking-widest mb-2 px-2">MAIN NAVIGATION
        </h2>

        <asp:LinkButton ID="lnkDashboard" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnDashboard_Click">
    <img src="Icons/Dashboard.svg" class="h-5 w-5 flex-shrink-0" />
    <span class="font-heading2 tracking-widest text-sm">Dashboard</span>
        </asp:LinkButton>

        <asp:LinkButton ID="lnkPatients" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnPatientRegistration_Click">
    <img src="Icons/patientRegistration.svg" class="h-5 w-5 flex-shrink-0" />
    <span class="font-heading1 tracking-widest text-sm">Patient Registration</span>
        </asp:LinkButton>

        <asp:LinkButton ID="lnkCaseSurv" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnCaseSurveillance_Click">
    <img src="Icons/caseMonitoring.svg" class="h-5 w-5 flex-shrink-0" />
    <span class="font-heading2 tracking-widest text-sm">Case Monitoring</span>
        </asp:LinkButton>

        <asp:LinkButton ID="lnkVaccReg" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnVaccineManagement_Click">
    <img src="Icons/vaccineManagement.svg" class="h-5 w-5 flex-shrink-0" />
    <span class="font-heading2 tracking-widest text-sm">Vaccine Management</span>
        </asp:LinkButton>

        <asp:LinkButton ID="lnkReports" runat="server"
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnReports_Click">
    <img src="Icons/reports.svg" class="h-5 w-5 flex-shrink-0" />
    <span class="font-heading2 tracking-widest text-sm">Reports</span>
        </asp:LinkButton>


        <h2 runat="server" ID="adminHeader" class="font-heading2 text-xs tracking-widest mb-2 px-2 opacity-70" >ADMINISTRATION
        </h2> 

        <asp:LinkButton ID="lnkUserManagement" runat="server" 
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnUserManagement_Click">
            <img src="Icons/userManagement.svg" class="h-5 w-5 flex-shrink-0" />
            <span class="font-heading2 tracking-widest text-sm">User Management</span>
        </asp:LinkButton>

        <asp:LinkButton ID="lnkAudit" runat="server" 
            Style="display: flex; align-items: center; gap: 8px;"
            CssClass="flex items-center gap-3 px-4 py-2.5 rounded-lg hover:bg-blue-700 transition-colors w-full"
            OnClick="btnAudit_Click">
            <img src="Icons/Audit.svg" class="h-5 w-5 flex-shrink-0" />
            <span class="font-heading2 tracking-widest text-sm">Audit Logs</span>

        </asp:LinkButton>


       

    </div>
    <asp:LinkButton ID="lnkSignOut" runat="server"
        CssClass="flex items-center gap-3 px-4 py-5 rounded-lg hover:bg-red-600 transition-colors w-full mt-auto"
        OnClick="btnSignOut_Click">
    <img src="Icons/SignOut.svg" class="h-5 w-5 flex-shrink-0" />
    <span class="font-heading2 tracking-widest text-sm">Sign Out</span>
    </asp:LinkButton>
</div>
