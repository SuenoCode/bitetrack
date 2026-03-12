<%@ Page Language="C#" MasterPageFile="~/Site1.Master" AutoEventWireup="true" CodeBehind="Settings.aspx.cs" Inherits="SBI.Settings" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <!-- Tailwind requirement: make sure Site1.Master includes:
         <script src="https://cdn.tailwindcss.com"></script>
    -->

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- Header -->
        <div class="flex flex-col gap-4 sm:flex-row sm:items-start sm:justify-between">
            <div>
                <h1 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">System Settings</h1>
                <p class="mt-1 text-base text-slate-600">Configure risk levels, thresholds, and system preferences</p>
            </div>

            <asp:Button ID="btnSaveChanges" runat="server" Text="Save Changes"
                CssClass="h-11 rounded-lg bg-[#0b2a7a] px-6 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition" />
        </div>

        <!-- Cards grid -->
        <div class="mt-6 grid grid-cols-1 gap-5 lg:grid-cols-2">

            <!-- Vaccine Stock Thresholds -->
            <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
                <div class="flex items-center gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                    <span class="inline-flex h-8 w-8 items-center justify-center rounded-xl bg-blue-100 text-blue-700 font-bold"></span>
                    <h3 class="text-base font-extrabold text-slate-900">Vaccine Stock Thresholds</h3>
                </div>

                <div class="px-5 py-5 space-y-5">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Low Stock Alert Threshold (doses)</label>
                        <asp:TextBox ID="txtLowStockThreshold" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            TextMode="Number" Text="20" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Critical Stock Threshold (doses)</label>
                        <asp:TextBox ID="txtCriticalStockThreshold" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            TextMode="Number" Text="10" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Expiry Warning (days before)</label>
                        <asp:TextBox ID="txtExpiryWarningDays" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            TextMode="Number" Text="30" />
                    </div>

                </div>
            </div>

            <!-- Security & Password Policy -->
            <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm lg:col-span-1">
                <div class="flex items-center gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                    <span class="inline-flex h-8 w-8 items-center justify-center rounded-xl bg-rose-100 text-rose-700 font-bold"></span>
                    <h3 class="text-base font-extrabold text-slate-900">Security &amp; Password Policy</h3>
                </div>

                <div class="px-5 py-5 space-y-6">

                    <!-- Item 1 -->
                    <div class="flex items-center justify-between gap-4">
                        <div>
                            <div class="text-sm font-extrabold text-slate-900">Force Password Change</div>
                            <div class="mt-1 text-sm text-slate-500">Every 90 days</div>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <asp:CheckBox ID="chkForcePasswordChange" runat="server" CssClass="peer sr-only" />
                            <span class="h-7 w-12 rounded-full bg-slate-300 transition peer-checked:bg-blue-600"></span>
                            <span class="absolute left-1 top-1 h-5 w-5 rounded-full bg-white transition peer-checked:translate-x-5"></span>
                        </label>
                    </div>

                    <!-- Item 2 -->
                    <div class="flex items-center justify-between gap-4">
                        <div>
                            <div class="text-sm font-extrabold text-slate-900">Session Timeout (30 min)</div>
                            <div class="mt-1 text-sm text-slate-500">Auto-logout on inactivity</div>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <asp:CheckBox ID="chkSessionTimeout" runat="server" CssClass="peer sr-only" />
                            <span class="h-7 w-12 rounded-full bg-slate-300 transition peer-checked:bg-blue-600"></span>
                            <span class="absolute left-1 top-1 h-5 w-5 rounded-full bg-white transition peer-checked:translate-x-5"></span>
                        </label>
                    </div>

                    <!-- Item 3 -->
                    <div class="flex items-center justify-between gap-4">
                        <div>
                            <div class="text-sm font-extrabold text-slate-900">Two-Factor Authentication</div>
                            <div class="mt-1 text-sm text-slate-500">SMS OTP required at login</div>
                        </div>
                        <label class="relative inline-flex items-center cursor-pointer">
                            <asp:CheckBox ID="chkTwoFactor" runat="server" CssClass="peer sr-only" />
                            <span class="h-7 w-12 rounded-full bg-slate-300 transition peer-checked:bg-blue-600"></span>
                            <span class="absolute left-1 top-1 h-5 w-5 rounded-full bg-white transition peer-checked:translate-x-5"></span>
                        </label>
                    </div>

                </div>
            </div>

            <!-- Empty spacer to keep 2-column balance (optional) -->
            <div class="hidden lg:block"></div>

        </div>
    </div>

</asp:Content>