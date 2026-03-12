<%@ Page Language="C#" MasterPageFile="~/Site1.master" AutoEventWireup="true" CodeBehind="PatientRegistration.aspx.cs" Inherits="SBI.PatientRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div class="px-3 py-6 font-sans text-slate-900">

        <!-- PAGE HEADER -->
        <div class="mb-5">
            <h2 class="text-4xl font-extrabold tracking-tight text-[#0b2a7a]">Patient Registration</h2>
            <p class="mt-1 text-base text-slate-600">
                Register patient details and document biting incident history
            </p>
        </div>

        <!-- A. PATIENT INFORMATION!!!! --> 
        <div class="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
            <div class="flex items-center justify-between gap-3 border-b border-slate-200 bg-slate-50 px-5 py-4">
                <h3 class="text-lg font-extrabold text-slate-900">A. Patient Information</h3>
                <span class="text-xs font-bold text-slate-500">Fields with <span class="text-red-500">*</span> are required</span>
            </div>

            <div class="px-5 py-5 space-y-5">

                <!-- Row 1 -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-3">

                    <!-- Patient ID -->
                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Patient ID
                            <span class="ml-2 inline-flex items-center rounded-full bg-[#1a4ed8] px-2 py-0.5 text-[10px] font-extrabold text-white">
                                AUTO
                            </span>
                        </label>

                        <asp:TextBox ID="txtPatientID" runat="server" ReadOnly="true"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-slate-50 px-3 text-[15px] font-semibold text-slate-700 outline-none focus:ring-2 focus:ring-blue-200"
                            Text="2025-285" />
                    </div>

                    <!-- Last Name -->
                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Last Name <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtLastName" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="e.g. Santos" />
                    </div>

                    <!-- First Name -->
                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            First Name <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtFirstName" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="e.g. Maria" />
                    </div>
                </div>

                <!-- Row 2 -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-3">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Middle Name</label>
                        <asp:TextBox ID="txtMiddleName" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="e.g. Cruz" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Date of Birth <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtDOB" runat="server" TextMode="Date"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Age
                            <span class="ml-2 inline-flex items-center rounded-full bg-[#1a4ed8] px-2 py-0.5 text-[10px] font-extrabold text-white">
                                AUTO
                            </span>
                        </label>
                        <asp:TextBox ID="txtAge" runat="server" ReadOnly="true"
                            CssClass="h-11 w-full rounded-lg border border-blue-100 bg-blue-50 px-3 text-[15px] font-extrabold text-[#0b2a7a] outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="Auto-calculated" />
                    </div>
                </div>

                <!-- Row 3 -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-2">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Gender</label>
                        <asp:DropDownList ID="ddlGender" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            <asp:ListItem Text="Select Gender" Value="" Selected="True" />
                            <asp:ListItem Text="Male" Value="Male" />
                            <asp:ListItem Text="Female" Value="Female" />
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Civil Status</label>
                        <asp:DropDownList ID="ddlCivilStatus" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-blue-100 bg-blue-50 px-3 text-[15px] font-bold text-[#0b2a7a] outline-none focus:ring-2 focus:ring-blue-200">
                            <asp:ListItem Text="Select Status" Value="" Selected="True" />
                            <asp:ListItem Text="Single" Value="single" />
                            <asp:ListItem Text="Married" Value="married" />
                            <asp:ListItem Text="Widowed" Value="widowed" />
                            <asp:ListItem Text="Separated" Value="separated" />
                            <asp:ListItem Text="Divorced" Value="divorced" />
                        </asp:DropDownList>
                    </div>
                </div>

                <!-- Row 4 -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-4">
                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">House No.</label>
                        <asp:TextBox ID="txtHouseNo" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="House Number" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Subdivision</label>
                        <asp:TextBox ID="txtSubdivision" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="Subdivision Name" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Barangay <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtBarangay" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="Barangay Name" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Province/City <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtProvinceCity" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="Province Name or City Name" />
                    </div>
                </div>

                <!-- Row 5 -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-3">

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Contact No <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtContactNo" runat="server" TextMode="SingleLine"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Occupation <span class="text-red-500">*</span>
                        </label>
                        <asp:DropDownList ID="ddlOccupation" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200">
                            <asp:ListItem Text="Select Occupation" Value="" Selected="True" />
                            <asp:ListItem Text="Student / Minor" Value="student" />
                            <asp:ListItem Text="Employed (Private/Govt)" Value="employed" />
                            <asp:ListItem Text="Self-Employed / Business" Value="self-employed" />
                            <asp:ListItem Text="Unemployed / Housewife" Value="unemployed" />
                            <asp:ListItem Text="Retired / Pensioner" Value="retired" />
                        </asp:DropDownList>
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Emergency Contact Person <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtEmergencyContactPerson" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="Name of Contact Person" />
                    </div>
                </div>

                <!-- Row 6 (Vitals etc.) -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-4 xl:grid-cols-7">

                    <div class="xl:col-span-1">
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Emergency Contact Number</label>
                        <asp:TextBox ID="txtEmergencyContactNo" runat="server" TextMode="SingleLine"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div class="xl:col-span-2">
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Chief Complaints <span class="text-red-500">*</span>
                        </label>
                        <asp:TextBox ID="txtChiefComplaints" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                            placeholder="Reason for Patient's Visit" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Blood Pressure 
                        </label>
                        <asp:TextBox ID="txtBloodPressure" runat="server" TextMode="Number"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Temperature 
                        </label>
                        <asp:TextBox ID="txtTemperature" runat="server" TextMode="Number"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Capillary Refill
                        <asp:TextBox ID="txtCapillaryRefill" runat="server" TextMode="Number"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Weight 
                        </label>
                        <asp:TextBox ID="txtWeight" runat="server" TextMode="Number"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>

                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">
                            Date <span class="ml-2 inline-flex rounded-full bg-[#1a4ed8] px-2 py-0.5 text-[10px] font-extrabold text-white">AUTO</span>
                        </label>
                        <asp:TextBox ID="txtDate" runat="server" TextMode="Date"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>
                </div>

            </div>
        </div>

        <!-- B. HISTORY OF BITING INCIDENT -->
        <div class="mt-6 overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm">
            <div class="border-b border-slate-200 bg-slate-50 px-5 py-4">
                <h3 class="text-lg font-extrabold text-slate-900">B. History of Biting Incident</h3>
            </div>

            <div class="px-5 py-5 space-y-5">

                <!-- Date/Time + Place -->
                <div class="grid grid-cols-1 gap-4 md:grid-cols-2">
                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Date and Time of Bite</label>
                        <asp:TextBox ID="txtBiteDateTime" runat="server" TextMode="DateTimeLocal"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>
                    <div>
                        <label class="mb-2 block text-sm font-semibold text-slate-700">Place of Exposure</label>
                        <asp:TextBox ID="txtPlaceExposure" runat="server"
                            CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200" />
                    </div>
                </div>

                <!-- Helper for radio groups -->
                <div class="grid grid-cols-1 gap-4 lg:grid-cols-2">
                    <!-- Biting Animal -->
                    <div class="rounded-xl border border-slate-200 p-4">
                        <div class="mb-3 text-sm font-extrabold text-slate-900">Biting Animal</div>
                        <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                            <asp:RadioButton ID="rbDog" runat="server" GroupName="AnimalType" Text="Dog" />
                            <asp:RadioButton ID="rbCat" runat="server" GroupName="AnimalType" Text="Cat" />
                            <asp:TextBox ID="txtOtherAnimal" runat="server"
                                CssClass="h-10 w-48 rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                                placeholder="Others" />
                        </div>
                    </div>

                    <!-- Ownership -->
                    <div class="rounded-xl border border-slate-200 p-4">
                        <div class="mb-3 text-sm font-extrabold text-slate-900">Ownership</div>
                        <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                            <asp:RadioButton ID="rbOwned" runat="server" GroupName="Ownership" Text="Owned" />
                            <asp:RadioButton ID="rbStray" runat="server" GroupName="Ownership" Text="Stray" />
                            <asp:RadioButton ID="rbLeashed" runat="server" GroupName="Ownership" Text="Leashed/Cage" />
                        </div>
                    </div>

                    <!-- Circumstance -->
                    <div class="rounded-xl border border-slate-200 p-4">
                        <div class="mb-3 text-sm font-extrabold text-slate-900">Circumstance</div>
                        <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                            <asp:RadioButton ID="rbProvoked" runat="server" GroupName="Circumstance" Text="Provoked / Intentional" />
                            <asp:RadioButton ID="rbUnprovoked" runat="server" GroupName="Circumstance" Text="Unprovoked / Unintentional" />
                        </div>
                    </div>

                    <!-- Status -->
                    <div class="rounded-xl border border-slate-200 p-4">
                        <div class="mb-3 text-sm font-extrabold text-slate-900">Status of Biting Animal</div>
                        <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                            <asp:RadioButton ID="rbAlive" runat="server" GroupName="AnimalStatus" Text="Alive / Healthy" />
                            <asp:RadioButton ID="rbSick" runat="server" GroupName="AnimalStatus" Text="Sick" />
                            <asp:RadioButton ID="rbDied" runat="server" GroupName="AnimalStatus" Text="Died / Killed" />
                            <asp:RadioButton ID="rbUnknown" runat="server" GroupName="AnimalStatus" Text="Unknown" />
                        </div>
                    </div>
                </div>

                <!-- Exposure Type -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <div class="mb-3 text-sm font-extrabold text-slate-900">Type of Exposure</div>
                    <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                        <asp:RadioButton ID="rbBite" runat="server" GroupName="ExposureType" Text="Bite" />
                        <asp:RadioButton ID="rbNonBite" runat="server" GroupName="ExposureType" Text="Non-Bite / Play Bite" />
                        <asp:CheckBox ID="cbWashed" runat="server" Text="Washing of Bite Wound 15 mins" />
                        <asp:CheckBox ID="cbUnwashed" runat="server" Text="Unwashed Wound" />
                    </div>
                </div>

                <!-- Wound Type -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <div class="mb-3 text-sm font-extrabold text-slate-900">Wound Type</div>
                    <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                        <asp:RadioButton ID="rbLacerated" runat="server" GroupName="WoundType" Text="Lacerated" />
                        <asp:RadioButton ID="rbAvulsion" runat="server" GroupName="WoundType" Text="Avulsion" />
                        <asp:RadioButton ID="rbPunctured" runat="server" GroupName="WoundType" Text="Punctured" />
                        <asp:RadioButton ID="rbAbrasion" runat="server" GroupName="WoundType" Text="Abrasion" />
                        <asp:RadioButton ID="rbScratches" runat="server" GroupName="WoundType" Text="Scratches" />
                        <asp:RadioButton ID="rbHematoma" runat="server" GroupName="WoundType" Text="Hematoma" />
                    </div>
                </div>

                <!-- Bleeding -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <div class="mb-3 text-sm font-extrabold text-slate-900">Bleeding</div>
                    <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                        <asp:RadioButton ID="rbBleedingYes" runat="server" GroupName="Bleeding" Text="(+)" />
                        <asp:RadioButton ID="rbBleedingNo" runat="server" GroupName="Bleeding" Text="(-)" />
                    </div>
                </div>

                <!-- Site of Bite -->
                <div>
                    <label class="mb-2 block text-sm font-semibold text-slate-700">Site of Bite</label>
                    <asp:TextBox ID="txtSiteOfBite" runat="server"
                        CssClass="h-11 w-full rounded-lg border border-slate-200 bg-white px-3 text-[15px] text-slate-900 outline-none focus:ring-2 focus:ring-blue-200"
                        placeHolder="Address/Location" />
                </div>

                <!-- Category -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <div class="mb-3 text-sm font-extrabold text-slate-900">Category</div>
                    <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                        <asp:RadioButton ID="rbCatI" runat="server" GroupName="Category" Text="I" />
                        <asp:RadioButton ID="rbCatII" runat="server" GroupName="Category" Text="II" />
                        <asp:RadioButton ID="rbCatIII" runat="server" GroupName="Category" Text="III" />
                    </div>
                </div>

                <!-- Manifestation -->
                <div class="rounded-xl border border-slate-200 p-4">
                    <div class="mb-3 text-sm font-extrabold text-slate-900">Manifestation</div>
                    <div class="flex flex-wrap items-center gap-5 text-sm font-semibold text-slate-700">
                        <asp:CheckBox ID="cbHeadache" runat="server" Text="Head Ache" />
                        <asp:CheckBox ID="cbFever" runat="server" Text="Fever" />
                        <asp:CheckBox ID="cbNumbness" runat="server" Text="Numbness on site of Bite" />
                        <asp:CheckBox ID="cbTingling" runat="server" Text="Tingling Sensation" />
                    </div>
                </div>

            </div>
        </div>

        <!-- OPTIONAL: ACTION BUTTONS (if you have Save/Cancel) -->
        <!--
        <div class="mt-6 flex flex-col gap-3 sm:flex-row sm:justify-end">
            <asp:Button ID="btnCancel" runat="server" Text="Cancel"
                CssClass="h-11 rounded-lg border border-slate-200 bg-white px-5 font-semibold text-slate-700 shadow-sm hover:shadow-md hover:-translate-y-[1px] transition" />
            <asp:Button ID="btnSave" runat="server" Text="Save Patient"
                CssClass="h-11 rounded-lg bg-[#1a4ed8] px-6 font-extrabold text-white shadow hover:brightness-110 hover:-translate-y-[1px] transition" />
        </div>
        -->

    </div>

</asp:Content>