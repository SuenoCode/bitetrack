<%@ Page Language="C#" MasterPageFile="~/Front.master" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="SBI.Login" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<div class="font-heading2 relative w-full h-screen overflow-hidden flex items-center justify-center">

    <!-- Blurred background -->
    <div class="absolute inset-0 bg-auto bg-repeat bg-[right_top] filter blur-[12px] scale-[1.2] pointer-events-none z-0"
         style="background-image: url('<%= ResolveUrl("~/Icons/sbi-bg.jpg") %>');">
    </div>

    <!-- Dark overlay -->
    <div class="absolute inset-0 bg-black/40 pointer-events-none"></div>

    <!-- Login Card -->
    <div class="relative z-10 w-full max-w-[370px] px-4">
        <div class="bg-white rounded-2xl shadow-2xl overflow-hidden">

            <!-- Top accent -->
            <div class="h-2 bg-blue-600 w-full"></div>

            <!-- Header -->
            <div class="px-8 pt-8 pb-3 text-center">
                <div class="flex justify-center mb-3">
                    <img src="<%= ResolveUrl("~/Icons/logo.png") %>" class="h-20 w-auto" alt="SBI Logo" />
                </div>

                <h2 class="text-3xl font-bold text-gray-800 tracking-tight">
                    Bite Track
                </h2>

                <p class="text-gray-400 text-xs tracking-widest mt-1">
                    SBI Medical - Morong Branch
                </p>
            </div>

            <!-- Form -->
            <div class="px-9 pb-8 pt-2">

                <!-- Error message -->
                <div class="min-h-[28px] mb-2">
                    <asp:Label ID="errorMsg" runat="server"
                        Text="Invalid username or password."
                        Visible="false"
                        CssClass="text-sm text-red-600 bg-red-50 px-4 py-2 rounded-md block text-center">
                    </asp:Label>
                </div>

                <!-- Username -->
                <div class="mb-5">
                    <label class="block text-sm font-medium text-gray-700 mb-1.5">
                        Username
                    </label>

                    <asp:TextBox ID="txtUsername" runat="server"
                        CssClass="w-full px-4 py-3 border border-gray-300 rounded-lg text-sm
                        focus:border-blue-500 focus:ring-1 focus:ring-blue-500 outline-none"
                        Placeholder="Enter your username" />
                </div>

                <!-- Password -->
                <div class="mb-6">
                    <label class="block text-sm font-medium text-gray-700 mb-1.5">
                        Password
                    </label>

                    <div class="relative">
                        <asp:TextBox ID="txtPassword" runat="server"
                            TextMode="Password"
                            CssClass="w-full px-4 py-3 pr-12 border border-gray-300 rounded-lg text-sm
                            focus:border-blue-500 focus:ring-1 focus:ring-blue-500 outline-none"
                            Placeholder="Enter your password" />

                        <button type="button"
                            onclick="togglePassword()"
                            class="absolute inset-y-0 right-3 flex items-center justify-center">
                            <img id="eyeIcon"
                                 src="<%= ResolveUrl("~/Icons/eye.svg") %>"
                                 alt="Show password"
                                 class="w-5 h-5 opacity-70 hover:opacity-100" />
                        </button>
                    </div>
                </div>

                <!-- Login Button -->
                <asp:Button ID="btnLogin" runat="server"
                    Text="Sign in"
                    OnClick="btnLogin_Click"
                    CssClass="w-full bg-blue-600 hover:bg-blue-700 text-white
                    font-medium py-3 rounded-lg text-sm transition-colors
                    focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2" />

                <div class="text-center text-xs text-gray-400 mt-6">
                    Secure login · Session expires in 30 minutes
                </div>

            </div>
        </div>
    </div>
</div>

<script type="text/javascript">
    function togglePassword() {
        var passwordBox = document.getElementById('<%= txtPassword.ClientID %>');
        var eyeIcon = document.getElementById('eyeIcon');

        if (passwordBox.type === "password") {
            passwordBox.type = "text";
            eyeIcon.style.opacity = "1";
        } else {
            passwordBox.type = "password";
            eyeIcon.style.opacity = "0.7";
        }
    }
</script>

</asp:Content>