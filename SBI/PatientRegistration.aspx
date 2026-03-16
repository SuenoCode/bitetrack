<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="PatientRegistration.aspx.cs" Inherits="SBI.PatientRegistration" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <asp:HiddenField ID="hfActivePanel" runat="server" Value="viewPatientPanel" />
    <asp:HiddenField ID="hfSelectedPatientId" runat="server" Value="" />
    <asp:HiddenField ID="hfSelectedCaseId" runat="server" Value="" />
    <asp:HiddenField ID="hfEditMode" runat="server" Value="" />

    <style>
        /* ── Step progress bar ───────────────────────────────── */
        .step-bar { display:flex; align-items:center; margin-bottom:2rem; }
        .step-item { display:flex; flex-direction:column; align-items:center; position:relative; flex:1; }
        .step-item:not(:last-child)::after {
            content:''; position:absolute; top:18px; left:calc(50% + 22px);
            right:calc(-50% + 22px); height:2px;
            background:#e2e8f0; z-index:0; transition:background .4s;
        }
        .step-item.done:not(:last-child)::after { background:#2563eb; }
        .step-circle {
            width:36px; height:36px; border-radius:50%;
            display:flex; align-items:center; justify-content:center;
            font-size:13px; font-weight:800; border:2px solid #e2e8f0;
            background:#fff; color:#94a3b8; position:relative; z-index:1; transition:all .3s;
        }
        .step-item.done   .step-circle { background:#2563eb; border-color:#2563eb; color:#fff; }
        .step-item.active .step-circle { background:#fff; border-color:#2563eb; color:#2563eb; box-shadow:0 0 0 4px #dbeafe; }
        .step-label { font-size:11px; font-weight:700; color:#94a3b8; margin-top:6px; text-transform:uppercase; letter-spacing:.5px; text-align:center; }
        .step-item.done   .step-label { color:#2563eb; }
        .step-item.active .step-label { color:#1e40af; }
        /* ── Wizard steps ─────────────────────────────────── */
        .wizard-step { display:none; animation:stepIn .25s ease-out; }
        .wizard-step.active { display:block; }
        @keyframes stepIn  { from { opacity:0; transform:translateY(10px); } to { opacity:1; transform:translateY(0); } }
        @keyframes fadeIn  { from { opacity:0; transform:translateY(8px) scale(.98); } to { opacity:1; transform:translateY(0) scale(1); } }
        .wizard-nav { display:flex; justify-content:space-between; align-items:center;
            padding:16px 24px; border-top:1px solid #e2e8f0; background:#f8fafc;
            border-radius:0 0 12px 12px; margin-top:24px; }
        /* ── Field styling ────────────────────────────────── */
        .field-group { display:flex; flex-direction:column; gap:6px; }
        .field-label { font-size:11px; font-weight:700; color:#64748b; text-transform:uppercase; letter-spacing:.5px; }
        .field-required { color:#ef4444; margin-left:2px; }
        .field-input { width:100%; border:1px solid #cbd5e1; border-radius:8px; padding:10px 14px;
            font-size:14px; outline:none; transition:all .2s; background:#fff; }
        .field-input:focus { border-color:#2563eb; box-shadow:0 0 0 3px #dbeafe; }
        /* ── Section / sub cards ──────────────────────────── */
        .section-card { background:#fff; border:1px solid #e2e8f0; border-radius:12px; overflow:hidden; margin-bottom:20px; }
        .section-header { padding:16px 20px; background:#f8fafc; border-bottom:1px solid #e2e8f0; }
        .section-title { font-size:14px; font-weight:800; color:#1e293b; }
        .section-desc  { font-size:12px; color:#94a3b8; margin-top:2px; }
        .section-body  { padding:20px; }
        .sub-card { background:#f8fafc; border:1px solid #e2e8f0; border-radius:10px; padding:16px; margin-top:4px; }
        .sub-card-title { font-size:11px; font-weight:800; color:#475569; text-transform:uppercase; letter-spacing:.5px; margin-bottom:12px; }
        /* ── Review rows ──────────────────────────────────── */
        .review-row { display:flex; gap:8px; padding:8px 0; border-bottom:1px solid #f1f5f9; font-size:13px; }
        .review-row:last-child { border-bottom:none; }
        .review-key { font-weight:700; color:#64748b; min-width:160px; flex-shrink:0; }
        .review-val { color:#1e293b; }
        /* ── View panel layout ────────────────────────────── */
        @media (min-width: 1280px) {
            #viewLayout.with-preview { grid-template-columns: minmax(0,1.8fr) minmax(360px,440px); align-items:start; }
            #viewLayout.no-preview   { grid-template-columns: minmax(0,1fr); }
        }
        @media (max-width:1279px) {
            #viewLayout.with-preview, #viewLayout.no-preview { grid-template-columns:1fr; }
        }
    </style>

    <div class="p-6 font-heading2 text-slate-900">

        <div class="flex justify-between items-start mb-6">
            <div>
                <h1 class="text-4xl text-[#0b2a7a] font-hBruns tracking-widest">Patient Registration</h1>
                <p class="text-slate-500 text-sm mt-1">Register patients, manage bite cases, and update records.</p>
            </div>
        </div>

        <%-- Tab Nav --%>
        <div class="flex gap-2 border-b border-slate-200 pb-px mb-6">
            <button type="button" id="btnViewPanel"
                class="panel-tab h-11 rounded-lg px-6 font-bold text-sm transition cursor-pointer border"
                onclick="showPanel('viewPatientPanel')">
                View Patient / Case Details
            </button>
            <button type="button" id="btnAddPanel"
                class="panel-tab h-11 rounded-lg px-6 font-bold text-sm transition cursor-pointer border"
                onclick="showPanel('addPatientPanel')">
                Add New Patient / Case
            </button>
        </div>

        <%-- ══════════════════════════════════════════════════
             ADD PANEL — 5-step wizard
             ══════════════════════════════════════════════════ --%>
        <div id="addPatientPanel" class="panel hidden">

            <%-- Progress Steps --%>
            <div class="step-bar" id="stepBar">
                <div class="step-item active" id="stepItem1">
                    <div class="step-circle">1</div>
                    <div class="step-label">Personal Info</div>
                </div>
                <div class="step-item" id="stepItem2">
                    <div class="step-circle">2</div>
                    <div class="step-label">Address &amp; Contacts</div>
                </div>
                <div class="step-item" id="stepItem3">
                    <div class="step-circle">3</div>
                    <div class="step-label">Bite Incident</div>
                </div>
                <div class="step-item" id="stepItem4">
                    <div class="step-circle">4</div>
                    <div class="step-label">Animal &amp; Wound</div>
                </div>
                <div class="step-item" id="stepItem5">
                    <div class="step-circle">5</div>
                    <div class="step-label">Review &amp; Submit</div>
                </div>
            </div>

            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">

                <%-- STEP 1: Personal Information --%>
                <div class="wizard-step active" id="wizStep1">
                    <div class="section-header">
                        <div class="section-title">Step 1 — Personal Information</div>
                        <div class="section-desc">Basic demographics and contact details of the patient</div>
                    </div>
                    <div class="section-body">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="field-group">
                                <label class="field-label">First Name <span class="field-required">*</span></label>
                                <asp:TextBox ID="txtFirstName" runat="server" CssClass="field-input" placeholder="e.g. Maria" />
                            </div>
                            <div class="field-group">
                                <label class="field-label">Last Name <span class="field-required">*</span></label>
                                <asp:TextBox ID="txtLastName" runat="server" CssClass="field-input" placeholder="e.g. Santos" />
                            </div>
                            <div class="field-group">
                                <label class="field-label">Date of Birth <span class="field-required">*</span></label>
                                <asp:TextBox ID="txtDOB" runat="server" TextMode="Date" CssClass="field-input" />
                            </div>
                            <div class="field-group">
                                <label class="field-label">Gender <span class="field-required">*</span></label>
                                <asp:DropDownList ID="ddlGender" runat="server" CssClass="field-input">
                                    <asp:ListItem Text="Select Gender" Value="" />
                                    <asp:ListItem Text="Male" Value="M" />
                                    <asp:ListItem Text="Female" Value="F" />
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <label class="field-label">Civil Status</label>
                                <asp:DropDownList ID="ddlCivilStatus" runat="server" CssClass="field-input">
                                    <asp:ListItem Text="Select Status" Value="" />
                                    <asp:ListItem Text="Single" Value="Single" />
                                    <asp:ListItem Text="Married" Value="Married" />
                                    <asp:ListItem Text="Widowed" Value="Widowed" />
                                    <asp:ListItem Text="Separated" Value="Separated" />
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <label class="field-label">Contact No <span class="field-required">*</span></label>
                                <asp:TextBox ID="txtContactNo" runat="server" CssClass="field-input" placeholder="e.g. 09123456789" />
                            </div>
                            <div class="field-group md:col-span-2">
                                <label class="field-label">Occupation <span class="field-required">*</span></label>
                                <asp:DropDownList ID="ddlOccupation" runat="server" CssClass="field-input">
                                    <asp:ListItem Text="Select Occupation" Value="" />
                                    <asp:ListItem Text="Student" Value="Student" />
                                    <asp:ListItem Text="Employed" Value="Employed" />
                                    <asp:ListItem Text="Self-Employed" Value="Self-Employed" />
                                    <asp:ListItem Text="Unemployed" Value="Unemployed" />
                                    <asp:ListItem Text="Retired" Value="Retired" />
                                </asp:DropDownList>
                            </div>
                        </div>
                        <%-- Optional Vitals --%>
                        <div class="mt-6">
                            <button type="button" onclick="toggleVitals()"
                                class="flex items-center gap-2 text-sm font-bold text-blue-600 hover:text-blue-800 transition mb-3">
                                <span id="vitalsIcon">▶</span> Optional Vitals
                            </button>
                            <div id="optionalVitals" class="hidden sub-card">
                                <div class="sub-card-title">Vitals &amp; Visit Information</div>
                                <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                    <div class="field-group">
                                        <label class="field-label">Blood Pressure</label>
                                        <asp:TextBox ID="txtBloodPressure" runat="server" CssClass="field-input" placeholder="e.g. 120/80" />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">Temperature (°C)</label>
                                        <asp:TextBox ID="txtTemperature" runat="server" CssClass="field-input" placeholder="e.g. 36.5" />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">Weight (kg)</label>
                                        <asp:TextBox ID="txtWeight" runat="server" CssClass="field-input" placeholder="e.g. 65" />
                                    </div>
                                    <div class="field-group">
                                        <label class="field-label">Capillary Refill</label>
                                        <asp:TextBox ID="txtCapillaryRefill" runat="server" CssClass="field-input" placeholder="e.g. &lt; 2 sec" />
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="wizard-nav">
                        <span class="text-xs text-slate-400">Step 1 of 5</span>
                        <button type="button" onclick="goStep(2)"
                            class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer">
                            Next: Address &amp; Contacts →
                        </button>
                    </div>
                </div>

                <%-- STEP 2: Address & Emergency Contact --%>
                <div class="wizard-step" id="wizStep2">
                    <div class="section-header">
                        <div class="section-title">Step 2 — Address &amp; Emergency Contact</div>
                        <div class="section-desc">Patient's home address and emergency contact person</div>
                    </div>
                    <div class="section-body space-y-5">
                        <div class="sub-card">
                            <div class="sub-card-title">Home Address</div>
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div class="field-group">
                                    <label class="field-label">House No.</label>
                                    <asp:TextBox ID="txtHouseNo" runat="server" CssClass="field-input" placeholder="e.g. 123" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Subdivision / Street</label>
                                    <asp:TextBox ID="txtSubdivision" runat="server" CssClass="field-input" placeholder="Street or Subdivision" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Barangay <span class="field-required">*</span></label>
                                    <asp:TextBox ID="txtBarangay" runat="server" CssClass="field-input" placeholder="Barangay" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">City / Province <span class="field-required">*</span></label>
                                    <asp:TextBox ID="txtProvinceCity" runat="server" CssClass="field-input" placeholder="City / Province" />
                                </div>
                            </div>
                        </div>
                        <div class="sub-card">
                            <div class="sub-card-title">Emergency Contact</div>
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div class="field-group">
                                    <label class="field-label">Contact Person <span class="field-required">*</span></label>
                                    <asp:TextBox ID="txtEmergencyContactPerson" runat="server" CssClass="field-input" placeholder="Full name" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Contact Number <span class="field-required">*</span></label>
                                    <asp:TextBox ID="txtEmergencyContactNo" runat="server" CssClass="field-input" placeholder="e.g. 09123456789" />
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="wizard-nav">
                        <button type="button" onclick="goStep(1)"
                            class="bg-white border border-slate-300 text-slate-600 px-5 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer hover:bg-slate-50">
                            ← Back
                        </button>
                        <span class="text-xs text-slate-400">Step 2 of 5</span>
                        <button type="button" onclick="goStep(3)"
                            class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer">
                            Next: Bite Incident →
                        </button>
                    </div>
                </div>

                <%-- STEP 3: Bite Incident --%>
                <div class="wizard-step" id="wizStep3">
                    <div class="section-header">
                        <div class="section-title">Step 3 — History of Biting Incident</div>
                        <div class="section-desc">Date, time, and location of the bite exposure</div>
                    </div>
                    <div class="section-body space-y-5">
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="field-group md:col-span-2">
                                <label class="field-label">Date and Time of Bite <span class="field-required">*</span></label>
                                <asp:TextBox ID="txtBiteDateTime" runat="server" TextMode="DateTimeLocal" CssClass="field-input" />
                            </div>
                        </div>
                        <div class="sub-card">
                            <div class="sub-card-title">Place of Bite</div>
                            <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                <div class="field-group">
                                    <label class="field-label">House No.</label>
                                    <asp:TextBox ID="txtPlaceHouseNo" runat="server" CssClass="field-input" placeholder="House No." />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Street</label>
                                    <asp:TextBox ID="txtPlaceStreet" runat="server" CssClass="field-input" placeholder="Street" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Barangay <span class="field-required">*</span></label>
                                    <asp:TextBox ID="txtPlaceBarangay" runat="server" CssClass="field-input" placeholder="Barangay" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">City / Province <span class="field-required">*</span></label>
                                    <asp:TextBox ID="txtPlaceCity" runat="server" CssClass="field-input" placeholder="City / Province" />
                                </div>
                            </div>
                        </div>
                        <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
                            <div class="field-group">
                                <label class="field-label">Category <span class="field-required">*</span></label>
                                <asp:DropDownList ID="ddlCategory" runat="server" CssClass="field-input">
                                    <asp:ListItem Text="Select Category" Value="" />
                                    <asp:ListItem Text="Category I" Value="I" />
                                    <asp:ListItem Text="Category II" Value="II" />
                                    <asp:ListItem Text="Category III" Value="III" />
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <label class="field-label">Type of Exposure</label>
                                <asp:DropDownList ID="ddlExposureType" runat="server" CssClass="field-input">
                                    <asp:ListItem Text="Select Type" Value="" />
                                    <asp:ListItem Text="Bite" Value="Bite" />
                                    <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                                </asp:DropDownList>
                            </div>
                            <div class="field-group">
                                <label class="field-label">Manifestation</label>
                                <asp:DropDownList ID="ddlManifestation" runat="server" CssClass="field-input">
                                    <asp:ListItem Text="Select" Value="" />
                                    <asp:ListItem Text="Head Ache" Value="Head Ache" />
                                    <asp:ListItem Text="Fever" Value="Fever" />
                                    <asp:ListItem Text="Numbness on Site of Bite" Value="Numbness on Site of Bite" />
                                    <asp:ListItem Text="Tingling Sensation" Value="Tingling Sensation" />
                                    <asp:ListItem Text="None" Value="None" />
                                </asp:DropDownList>
                            </div>
                        </div>
                    </div>
                    <div class="wizard-nav">
                        <button type="button" onclick="goStep(2)"
                            class="bg-white border border-slate-300 text-slate-600 px-5 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer hover:bg-slate-50">
                            ← Back
                        </button>
                        <span class="text-xs text-slate-400">Step 3 of 5</span>
                        <button type="button" onclick="goStep(4)"
                            class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer">
                            Next: Animal &amp; Wound →
                        </button>
                    </div>
                </div>

                <%-- STEP 4: Animal & Wound --%>
                <div class="wizard-step" id="wizStep4">
                    <div class="section-header">
                        <div class="section-title">Step 4 — Animal &amp; Wound Details</div>
                        <div class="section-desc">Information about the biting animal and wound characteristics</div>
                    </div>
                    <div class="section-body space-y-5">
                        <div class="sub-card">
                            <div class="sub-card-title">Biting Animal</div>
                            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                                <div class="field-group">
                                    <label class="field-label">Animal Type <span class="field-required">*</span></label>
                                    <asp:DropDownList ID="ddlBitingAnimal" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select Animal" Value="" />
                                        <asp:ListItem Text="Dog" Value="Dog" />
                                        <asp:ListItem Text="Cat" Value="Cat" />
                                        <asp:ListItem Text="Others" Value="Others" />
                                    </asp:DropDownList>
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Ownership</label>
                                    <asp:DropDownList ID="ddlOwnership" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select Ownership" Value="" />
                                        <asp:ListItem Text="Owned" Value="Owned" />
                                        <asp:ListItem Text="Stray" Value="Stray" />
                                        <asp:ListItem Text="Leashed / Cage" Value="Leashed/Cage" />
                                    </asp:DropDownList>
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Animal Status</label>
                                    <asp:DropDownList ID="ddlAnimalStatus" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select Status" Value="" />
                                        <asp:ListItem Text="Alive / Healthy" Value="Alive/Healthy" />
                                        <asp:ListItem Text="Sick" Value="Sick" />
                                        <asp:ListItem Text="Died / Killed" Value="Died/Killed" />
                                        <asp:ListItem Text="Unknown" Value="Unknown" />
                                    </asp:DropDownList>
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Circumstance</label>
                                    <asp:DropDownList ID="ddlCircumstance" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select Circumstance" Value="" />
                                        <asp:ListItem Text="Provoked / Intentional" Value="Provoked" />
                                        <asp:ListItem Text="Unprovoked / Unintentional" Value="Unprovoked" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                        <div class="sub-card">
                            <div class="sub-card-title">Wound Details</div>
                            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                                <div class="field-group">
                                    <label class="field-label">Wound Location</label>
                                    <asp:TextBox ID="txtWoundLocation" runat="server" CssClass="field-input" placeholder="e.g. Left arm" />
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Wound Type</label>
                                    <asp:DropDownList ID="ddlWoundType" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select Type" Value="" />
                                        <asp:ListItem Text="Lacerated" Value="Lacerated" />
                                        <asp:ListItem Text="Avulsion" Value="Avulsion" />
                                        <asp:ListItem Text="Punctured" Value="Punctured" />
                                        <asp:ListItem Text="Abrasion" Value="Abrasion" />
                                        <asp:ListItem Text="Scratches" Value="Scratches" />
                                        <asp:ListItem Text="Hematoma" Value="Hematoma" />
                                    </asp:DropDownList>
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Bleeding</label>
                                    <asp:DropDownList ID="ddlBleeding" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select" Value="" />
                                        <asp:ListItem Text="No" Value="No" />
                                        <asp:ListItem Text="Yes" Value="Yes" />
                                    </asp:DropDownList>
                                </div>
                                <div class="field-group">
                                    <label class="field-label">Wound Washed</label>
                                    <asp:DropDownList ID="ddlWoundWashed" runat="server" CssClass="field-input">
                                        <asp:ListItem Text="Select" Value="" />
                                        <asp:ListItem Text="Washed (15 mins)" Value="Yes" />
                                        <asp:ListItem Text="Unwashed" Value="No" />
                                    </asp:DropDownList>
                                </div>
                            </div>
                        </div>
                    </div>
                    <div class="wizard-nav">
                        <button type="button" onclick="goStep(3)"
                            class="bg-white border border-slate-300 text-slate-600 px-5 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer hover:bg-slate-50">
                            ← Back
                        </button>
                        <span class="text-xs text-slate-400">Step 4 of 5</span>
                        <button type="button" onclick="goStep(5)"
                            class="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer">
                            Next: Review →
                        </button>
                    </div>
                </div>

                <%-- STEP 5: Review & Submit --%>
                <div class="wizard-step" id="wizStep5">
                    <div class="section-header">
                        <div class="section-title">Step 5 — Review &amp; Submit</div>
                        <div class="section-desc">Please verify all information before saving the record</div>
                    </div>
                    <div class="section-body space-y-4">
                        <div class="section-card">
                            <div class="section-header" style="padding:12px 16px;">
                                <div class="section-title" style="font-size:12px;">👤 Patient Information</div>
                            </div>
                            <div class="section-body" style="padding:12px 16px;">
                                <div id="reviewPersonal"></div>
                            </div>
                        </div>
                        <div class="section-card">
                            <div class="section-header" style="padding:12px 16px;">
                                <div class="section-title" style="font-size:12px;">🏠 Address &amp; Emergency Contact</div>
                            </div>
                            <div class="section-body" style="padding:12px 16px;">
                                <div id="reviewAddress"></div>
                            </div>
                        </div>
                        <div class="section-card">
                            <div class="section-header" style="padding:12px 16px;">
                                <div class="section-title" style="font-size:12px;">🦷 Bite Incident</div>
                            </div>
                            <div class="section-body" style="padding:12px 16px;">
                                <div id="reviewBite"></div>
                            </div>
                        </div>
                        <div class="section-card">
                            <div class="section-header" style="padding:12px 16px;">
                                <div class="section-title" style="font-size:12px;">🐾 Animal &amp; Wound</div>
                            </div>
                            <div class="section-body" style="padding:12px 16px;">
                                <div id="reviewAnimal"></div>
                            </div>
                        </div>
                        <div class="bg-blue-50 border border-blue-200 rounded-xl px-4 py-3 text-sm text-blue-800 font-semibold">
                            ℹ️ An Initial Visit record will automatically be created upon submission.
                        </div>
                    </div>
                    <div class="wizard-nav">
                        <button type="button" onclick="goStep(4)"
                            class="bg-white border border-slate-300 text-slate-600 px-5 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer hover:bg-slate-50">
                            ← Back
                        </button>
                        <span class="text-xs text-slate-400">Step 5 of 5</span>
                        <div class="flex gap-3">
                            <button type="button" onclick="resetWizard()"
                                class="bg-white border border-slate-300 text-slate-600 px-5 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer hover:bg-slate-50">
                                Clear All
                            </button>
                            <asp:Button ID="btnSave" runat="server" Text="✓ Submit Registration"
                                CssClass="bg-emerald-600 hover:bg-emerald-700 text-white px-6 py-2.5 rounded-lg font-bold text-sm transition cursor-pointer"
                                OnClick="btnSave_Click" />
                        </div>
                    </div>
                </div>

            </div>

            <%-- Hidden for code-behind compatibility --%>
            <asp:Button ID="btnUpdateRecord"   runat="server" Visible="false" OnClick="btnUpdateRecord_Click" />
            <asp:Button ID="btnCancelEditForm" runat="server" Visible="false" OnClick="btnCancelEditForm_Click" />
            <asp:Button ID="btnClear"          runat="server" Visible="false" OnClick="btnClear_Click" />

        </div>

        <%-- ══════════════════════════════════════════════════
             VIEW PANEL
             ══════════════════════════════════════════════════ --%>
        <div id="viewPatientPanel" class="panel space-y-6">
            <div id="viewLayout" class="grid grid-cols-1 gap-6 transition-all duration-300 no-preview">

                <div id="detailsPane" class="space-y-6 min-w-0">

                    <%-- Patient Grid --%>
                    <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                            <div>
                                <h3 class="font-extrabold text-slate-800">Patient Details</h3>
                                <p class="text-slate-500 text-sm mt-1">List of registered patients</p>
                            </div>
                            <div class="flex gap-2 flex-wrap items-center">
                                <asp:TextBox ID="txtSearchPatient" runat="server" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" placeholder="Search by Patient ID, Name, Contact, Address" />
                                <asp:TextBox ID="txtPatientDateFrom" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <span class="text-sm text-slate-400">to</span>
                                <asp:TextBox ID="txtPatientDateTo" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <asp:Button ID="btnSearchPatient" runat="server" Text="Filter" OnClick="btnSearchPatient_Click" CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                                <asp:Button ID="btnResetPatientSearch" runat="server" Text="Clear" OnClick="btnResetPatientSearch_Click" CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                            </div>
                        </div>
                        <asp:GridView ID="gvPatients" runat="server" AutoGenerateColumns="False" CssClass="w-full text-sm" GridLines="None" DataKeyNames="patient_id" OnRowCommand="gvPatients_RowCommand" OnRowDataBound="gvPatients_RowDataBound">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                            <Columns>
                                <asp:TemplateField HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditPatient" runat="server" Text="Edit" CommandName="EditPatient" CommandArgument='<%# Eval("patient_id") %>' CssClass="text-blue-600 font-semibold text-xs hover:underline" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="patient_id" HeaderText="Patient ID" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="fname" HeaderText="First Name" ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="lname" HeaderText="Last Name" ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="gender" HeaderText="Gender" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="contact_no" HeaderText="Contact No" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="address" HeaderText="Address" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="date_added" HeaderText="Date Added" DataFormatString="{0:MMM dd, yyyy}" ItemStyle-CssClass="p-4 text-slate-500 text-xs whitespace-nowrap" HeaderStyle-CssClass="p-4" />
                            </Columns>
                            <EmptyDataTemplate><div class="p-10 text-center text-slate-400 text-sm">No patient records found.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                    <%-- Case Grid --%>
                    <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex flex-wrap justify-between items-center gap-3">
                            <div>
                                <h3 class="font-extrabold text-slate-800">Case Details</h3>
                                <p class="text-slate-500 text-sm mt-1">Recorded bite exposure cases</p>
                            </div>
                            <div class="flex gap-2 flex-wrap items-center">
                                <asp:TextBox ID="txtSearchCase" runat="server" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" placeholder="Search by Case ID, Patient ID, Case No, Place, Category" />
                                <asp:TextBox ID="txtCaseDateFrom" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <span class="text-sm text-slate-400">to</span>
                                <asp:TextBox ID="txtCaseDateTo" runat="server" TextMode="Date" CssClass="border border-slate-300 rounded-lg px-3 py-2 text-sm" />
                                <asp:Button ID="btnSearchCase" runat="server" Text="Filter" OnClick="btnSearchCase_Click" CssClass="bg-slate-800 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-700 transition" />
                                <asp:Button ID="btnResetCaseSearch" runat="server" Text="Clear" OnClick="btnResetCaseSearch_Click" CssClass="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition" />
                            </div>
                        </div>
                        <asp:GridView ID="gvCases" runat="server" AutoGenerateColumns="False" CssClass="w-full text-sm" GridLines="None" DataKeyNames="case_id" OnRowCommand="gvCases_RowCommand" OnRowDataBound="gvCases_RowDataBound">
                            <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                            <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                            <Columns>
                                <asp:TemplateField HeaderStyle-CssClass="p-4" ItemStyle-CssClass="p-4">
                                    <ItemTemplate>
                                        <asp:LinkButton ID="btnEditCase" runat="server" Text="Edit" CommandName="EditCase" CommandArgument='<%# Eval("case_id") %>' CssClass="text-blue-600 font-semibold text-xs hover:underline" />
                                    </ItemTemplate>
                                </asp:TemplateField>
                                <asp:BoundField DataField="case_id" HeaderText="Case ID" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="patient_id" HeaderText="Patient ID" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="case_no" HeaderText="Case Number" ItemStyle-CssClass="p-4 font-bold text-slate-700" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="date_of_bite" HeaderText="Date of Bite" DataFormatString="{0:MMM dd, yyyy}" ItemStyle-CssClass="p-4 text-slate-500 text-xs whitespace-nowrap" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="place_of_bite" HeaderText="Place of Bite" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="type_of_exposure" HeaderText="Type of Exposure" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="site_of_bite" HeaderText="Site of Bite" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                                <asp:BoundField DataField="category" HeaderText="Category" ItemStyle-CssClass="p-4 text-slate-600" HeaderStyle-CssClass="p-4" />
                            </Columns>
                            <EmptyDataTemplate><div class="p-10 text-center text-slate-400 text-sm">No case records found.</div></EmptyDataTemplate>
                        </asp:GridView>
                    </asp:Panel>

                </div>

                <%-- Record Preview --%>
                <asp:Panel ID="pnlRecordPreviewContainer" runat="server" Visible="false" CssClass="min-w-0">
                    <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden lg:sticky lg:top-6 max-h-[85vh] flex flex-col">
                        <div class="px-5 py-4 border-b border-slate-200 bg-slate-50 flex-shrink-0">
                            <h3 class="font-extrabold text-slate-800">Record Preview</h3>
                            <p class="text-slate-500 text-sm mt-1">Edit the selected record here</p>
                        </div>
                        <div class="flex-1 overflow-y-auto p-5">

                            <%-- Patient Preview --%>
                            <asp:Panel ID="pnlPatientPreview" runat="server" Visible="false" CssClass="space-y-4">
                                <div class="rounded-xl bg-blue-50 border border-blue-100 px-4 py-3">
                                    <h5 class="font-extrabold text-blue-900 text-sm">Patient Information</h5>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                    <div class="md:col-span-2">
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Patient ID</label>
                                        <asp:TextBox ID="txtPreviewPatientId" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">First Name</label>
                                        <asp:TextBox ID="txtPreviewFirstName" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Last Name</label>
                                        <asp:TextBox ID="txtPreviewLastName" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date of Birth</label>
                                        <asp:TextBox ID="txtPreviewDOB" runat="server" TextMode="Date" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Gender</label>
                                        <asp:DropDownList ID="ddlPreviewGender" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Gender" Value="" />
                                            <asp:ListItem Text="Male" Value="M" />
                                            <asp:ListItem Text="Female" Value="F" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Civil Status</label>
                                        <asp:DropDownList ID="ddlPreviewCivilStatus" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Status" Value="" />
                                            <asp:ListItem Text="Single" Value="Single" />
                                            <asp:ListItem Text="Married" Value="Married" />
                                            <asp:ListItem Text="Widowed" Value="Widowed" />
                                            <asp:ListItem Text="Separated" Value="Separated" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact No</label>
                                        <asp:TextBox ID="txtPreviewContactNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Occupation</label>
                                        <asp:DropDownList ID="ddlPreviewOccupation" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Occupation" Value="" />
                                            <asp:ListItem Text="Student" Value="Student" />
                                            <asp:ListItem Text="Employed" Value="Employed" />
                                            <asp:ListItem Text="Self-Employed" Value="Self-Employed" />
                                            <asp:ListItem Text="Unemployed" Value="Unemployed" />
                                            <asp:ListItem Text="Retired" Value="Retired" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Address</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">House No.</label>
                                            <asp:TextBox ID="txtPreviewHouseNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="House No." />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Street</label>
                                            <asp:TextBox ID="txtPreviewStreet" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Street" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Barangay</label>
                                            <asp:TextBox ID="txtPreviewBarangay" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Barangay" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">City / Province</label>
                                            <asp:TextBox ID="txtPreviewCityProvince" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="City / Province" />
                                        </div>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Emergency Contact</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact Person</label>
                                            <asp:TextBox ID="txtPreviewEmergencyPerson" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Full name" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Contact Number</label>
                                            <asp:TextBox ID="txtPreviewEmergencyNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 09123456789" />
                                        </div>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Vitals</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Blood Pressure</label>
                                            <asp:TextBox ID="txtPreviewBP" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 120/80" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Temperature</label>
                                            <asp:TextBox ID="txtPreviewTemp" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 36.5" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Weight</label>
                                            <asp:TextBox ID="txtPreviewWeight" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. 65" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Capillary Refill</label>
                                            <asp:TextBox ID="txtPreviewCapillaryRefill" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="e.g. &lt; 2 seconds" />
                                        </div>
                                    </div>
                                </div>
                                <div>
                                    <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date Added</label>
                                    <asp:TextBox ID="txtPreviewDateAdded" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                </div>
                                <div class="pt-2 flex gap-3">
                                    <asp:Button ID="btnPreviewUpdatePatient" runat="server" Text="Update"
                                        CssClass="flex-1 bg-amber-500 hover:bg-amber-600 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                        OnClick="btnPreviewUpdatePatient_Click"
                                        UseSubmitBehavior="false"
                                        OnClientClick="showConfirmModal('patient'); return false;" />
                                    <asp:Button ID="btnPreviewCancelPatient" runat="server" Text="Cancel"
                                        CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                        OnClick="btnPreviewCancelPatient_Click" />
                                </div>
                            </asp:Panel>

                            <%-- Case Preview --%>
                            <asp:Panel ID="pnlCasePreview" runat="server" Visible="false" CssClass="space-y-4">
                                <div class="rounded-xl bg-emerald-50 border border-emerald-100 px-4 py-3">
                                    <h5 class="font-extrabold text-emerald-900 text-sm">Case Information</h5>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Case ID</label>
                                        <asp:TextBox ID="txtPreviewCaseId" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Patient ID</label>
                                        <asp:TextBox ID="txtPreviewCasePatientId" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Case No</label>
                                        <asp:TextBox ID="txtPreviewCaseNo" runat="server" ReadOnly="true" CssClass="w-full border border-slate-200 rounded-lg px-3 py-2.5 text-sm bg-slate-100 text-slate-500" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Date of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseDateOfBite" runat="server" TextMode="Date" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Time of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseTimeOfBite" runat="server" TextMode="Time" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Exposure Type</label>
                                        <asp:DropDownList ID="ddlPreviewCaseExposureType" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Type" Value="" />
                                            <asp:ListItem Text="Bite" Value="Bite" />
                                            <asp:ListItem Text="Non-Bite / Play Bite" Value="Non Bite" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="bg-slate-50 border border-slate-200 rounded-xl p-4 space-y-3">
                                    <h6 class="font-extrabold text-slate-800 text-sm">Place of Bite</h6>
                                    <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">House No.</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceHouseNo" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="House No." />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Street</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceStreet" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Street" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Barangay</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceBarangay" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="Barangay" />
                                        </div>
                                        <div>
                                            <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">City / Province</label>
                                            <asp:TextBox ID="txtPreviewCasePlaceCity" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white" placeholder="City / Province" />
                                        </div>
                                    </div>
                                </div>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-3">
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Wound Type</label>
                                        <asp:DropDownList ID="ddlPreviewCaseWoundType" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Type" Value="" />
                                            <asp:ListItem Text="Lacerated" Value="Lacerated" />
                                            <asp:ListItem Text="Avulsion" Value="Avulsion" />
                                            <asp:ListItem Text="Punctured" Value="Punctured" />
                                            <asp:ListItem Text="Abrasion" Value="Abrasion" />
                                            <asp:ListItem Text="Scratches" Value="Scratches" />
                                            <asp:ListItem Text="Hematoma" Value="Hematoma" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Bleeding</label>
                                        <asp:DropDownList ID="ddlPreviewCaseBleeding" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select" Value="" />
                                            <asp:ListItem Text="No" Value="No" />
                                            <asp:ListItem Text="Yes" Value="Yes" />
                                        </asp:DropDownList>
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Site of Bite</label>
                                        <asp:TextBox ID="txtPreviewCaseSiteOfBite" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm" />
                                    </div>
                                    <div>
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Category</label>
                                        <asp:DropDownList ID="ddlPreviewCaseCategory" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select Category" Value="" />
                                            <asp:ListItem Text="I" Value="I" />
                                            <asp:ListItem Text="II" Value="II" />
                                            <asp:ListItem Text="III" Value="III" />
                                        </asp:DropDownList>
                                    </div>
                                    <div class="md:col-span-2">
                                        <label class="block text-xs font-bold text-slate-500 uppercase tracking-wider mb-1">Washed</label>
                                        <asp:DropDownList ID="ddlPreviewCaseWashed" runat="server" CssClass="w-full border border-slate-300 rounded-lg px-3 py-2.5 text-sm bg-white">
                                            <asp:ListItem Text="Select" Value="" />
                                            <asp:ListItem Text="Yes" Value="Yes" />
                                            <asp:ListItem Text="No" Value="No" />
                                        </asp:DropDownList>
                                    </div>
                                </div>
                                <div class="pt-2 flex gap-3">
                                    <asp:Button ID="btnPreviewUpdateCase" runat="server" Text="Update"
                                        CssClass="flex-1 bg-amber-500 hover:bg-amber-600 text-white py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                        OnClick="btnPreviewUpdateCase_Click"
                                        UseSubmitBehavior="false"
                                        OnClientClick="showConfirmModal('case'); return false;" />
                                    <asp:Button ID="btnPreviewCancelCase" runat="server" Text="Cancel"
                                        CssClass="flex-1 bg-white border border-slate-300 hover:bg-slate-50 text-slate-700 py-2.5 rounded-lg font-bold cursor-pointer transition text-sm"
                                        OnClick="btnPreviewCancelCase_Click" />
                                </div>
                            </asp:Panel>

                        </div>
                    </div>
                </asp:Panel>

            </div>
        </div>

    </div>

    <%-- ── Confirm Modal ─────────────────────────────────────────── --%>
    <div id="confirmModal" class="fixed inset-0 z-[100] hidden items-center justify-center bg-slate-900/50 px-4">
        <div class="w-full max-w-md rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden" style="animation:fadeIn .2s ease-out;">
            <div class="px-6 py-4 border-b border-amber-100 bg-amber-50 flex items-center gap-3">
                <img src="<%= ResolveUrl("~/Icons/warning.svg") %>" alt="warning" class="w-6 h-6 flex-shrink-0" />
                <div>
                    <h3 class="font-extrabold text-amber-900">Confirm Update</h3>
                    <p id="confirmModalMessage" class="mt-0.5 text-sm text-amber-700">Are you sure you want to continue?</p>
                </div>
            </div>
            <div class="px-6 py-4 flex justify-end gap-3">
                <button type="button" onclick="hideConfirmModal()"
                    class="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition">
                    Cancel
                </button>
                <button type="button" onclick="confirmModalAction()"
                    class="bg-amber-500 hover:bg-amber-600 text-white px-4 py-2 rounded-lg text-sm font-bold cursor-pointer transition">
                    Confirm
                </button>
            </div>
        </div>
    </div>

    <%-- ── Notify Modal ──────────────────────────────────────────── --%>
    <div id="notifyModal" class="fixed inset-0 z-[200] hidden items-center justify-center bg-slate-900/50 px-4">
        <div class="w-full max-w-sm rounded-2xl bg-white shadow-2xl border border-slate-200 overflow-hidden" style="animation:fadeIn .2s ease-out;">
            <div id="notifyModalHeader" class="px-6 py-4 border-b flex items-start gap-4">
                <div id="notifyModalIconWrap" class="w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5">
                    <img id="notifyModalIcon"
                         src="<%= ResolveUrl("~/Icons/warning.svg") %>"
                         alt="icon" class="w-5 h-5" />
                </div>
                <div class="flex-1 min-w-0">
                    <h3 id="notifyModalTitle" class="font-extrabold text-slate-900 text-base"></h3>
                    <p id="notifyModalMessage" class="text-sm text-slate-600 mt-1 leading-relaxed whitespace-pre-line"></p>
                </div>
            </div>
            <div class="px-6 py-4 flex justify-end">
                <button type="button" onclick="hideNotifyModal()"
                    id="notifyModalBtn"
                    class="px-6 py-2 rounded-lg text-sm font-bold cursor-pointer transition text-white">
                    OK
                </button>
            </div>
        </div>
    </div>

    <script type="text/javascript">

        var _iconSrc = '<%= ResolveUrl("~/Icons/warning.svg") %>';

        // ── Notify Modal ─────────────────────────────────────────────
        function showNotifyModal(message, type) {
            type = type || 'info';
            var hdr  = document.getElementById('notifyModalHeader');
            var wrap = document.getElementById('notifyModalIconWrap');
            var icon = document.getElementById('notifyModalIcon');
            var ttl  = document.getElementById('notifyModalTitle');
            var msg  = document.getElementById('notifyModalMessage');
            var btn  = document.getElementById('notifyModalBtn');

            icon.src = _iconSrc;
            hdr.className  = 'px-6 py-4 border-b flex items-start gap-4';
            wrap.className = 'w-10 h-10 rounded-full flex items-center justify-center flex-shrink-0 mt-0.5';
            icon.className = 'w-5 h-5';
            btn.className  = 'px-6 py-2 rounded-lg text-sm font-bold cursor-pointer transition text-white';
            // restore default onclick
            btn.onclick = hideNotifyModal;

            if (type === 'success') {
                hdr.className  += ' bg-emerald-50 border-emerald-100';
                wrap.className += ' bg-emerald-100';
                icon.style.filter = 'invert(39%) sepia(98%) saturate(400%) hue-rotate(100deg) brightness(90%)';
                ttl.textContent = 'Success'; ttl.className = 'font-extrabold text-emerald-800 text-base';
                btn.className  += ' bg-emerald-600 hover:bg-emerald-700';
            } else if (type === 'error') {
                hdr.className  += ' bg-red-50 border-red-100';
                wrap.className += ' bg-red-100';
                icon.style.filter = 'invert(20%) sepia(90%) saturate(700%) hue-rotate(340deg) brightness(90%)';
                ttl.textContent = 'Error'; ttl.className = 'font-extrabold text-red-800 text-base';
                btn.className  += ' bg-red-600 hover:bg-red-700';
            } else if (type === 'warning') {
                hdr.className  += ' bg-amber-50 border-amber-100';
                wrap.className += ' bg-amber-100';
                icon.style.filter = 'invert(55%) sepia(90%) saturate(500%) hue-rotate(10deg) brightness(95%)';
                ttl.textContent = 'Warning'; ttl.className = 'font-extrabold text-amber-800 text-base';
                btn.className  += ' bg-amber-500 hover:bg-amber-600';
            } else {
                hdr.className  += ' bg-blue-50 border-blue-100';
                wrap.className += ' bg-blue-100';
                icon.style.filter = 'invert(28%) sepia(80%) saturate(600%) hue-rotate(195deg) brightness(95%)';
                ttl.textContent = 'Notice'; ttl.className = 'font-extrabold text-blue-800 text-base';
                btn.className  += ' bg-blue-600 hover:bg-blue-700';
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

        // ── Confirm Modal ────────────────────────────────────────────
        var pendingUpdateType = '';

        function showConfirmModal(type) {
            pendingUpdateType = type || '';
            var msg = document.getElementById('confirmModalMessage');
            if (msg) msg.textContent = type === 'patient'
                ? 'Are you sure you want to update this patient record?'
                : type === 'case'
                    ? 'Are you sure you want to update this case record?'
                    : 'Are you sure you want to continue?';
            var m = document.getElementById('confirmModal');
            if (m) { m.classList.remove('hidden'); m.classList.add('flex'); }
            document.body.classList.add('overflow-hidden');
        }

        function hideConfirmModal() {
            var m = document.getElementById('confirmModal');
            if (m) { m.classList.add('hidden'); m.classList.remove('flex'); }
            document.body.classList.remove('overflow-hidden');
        }

        function confirmModalAction() {
            var t = pendingUpdateType; hideConfirmModal();
            if (t === 'patient') __doPostBack('<%=btnPreviewUpdatePatient.UniqueID%>', '');
            else if (t === 'case') __doPostBack('<%=btnPreviewUpdateCase.UniqueID%>', '');
        }

        // ── Wizard ───────────────────────────────────────────────────
        var currentStep = 1;
        var totalSteps  = 5;

        function goStep(n) {
            if (n < 1 || n > totalSteps) return;
            if (n > currentStep && !validateStep(currentStep)) return;
            document.getElementById('wizStep' + currentStep).classList.remove('active');
            document.getElementById('wizStep' + n).classList.add('active');
            for (var i = 1; i <= totalSteps; i++) {
                var item = document.getElementById('stepItem' + i);
                item.classList.remove('active', 'done');
                if (i < n)        item.classList.add('done');
                else if (i === n) item.classList.add('active');
            }
            currentStep = n;
            if (n === 5) buildReview();
            var bar = document.getElementById('stepBar');
            if (bar) bar.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }

        function validateStep(step) {
            var errors = [];
            if (step === 1) {
                if (!getVal('<%=txtFirstName.ClientID%>'))  errors.push('First Name is required.');
                if (!getVal('<%=txtLastName.ClientID%>'))   errors.push('Last Name is required.');
                if (!getVal('<%=txtDOB.ClientID%>'))        errors.push('Date of Birth is required.');
                if (!getVal('<%=ddlGender.ClientID%>'))     errors.push('Gender is required.');
                if (!getVal('<%=txtContactNo.ClientID%>'))  errors.push('Contact No is required.');
                if (!getVal('<%=ddlOccupation.ClientID%>')) errors.push('Occupation is required.');
            }
            if (step === 2) {
                if (!getVal('<%=txtBarangay.ClientID%>'))               errors.push('Barangay (home address) is required.');
                if (!getVal('<%=txtProvinceCity.ClientID%>'))           errors.push('City / Province is required.');
                if (!getVal('<%=txtEmergencyContactPerson.ClientID%>')) errors.push('Emergency contact person is required.');
                if (!getVal('<%=txtEmergencyContactNo.ClientID%>'))     errors.push('Emergency contact number is required.');
            }
            if (step === 3) {
                if (!getVal('<%=txtBiteDateTime.ClientID%>'))  errors.push('Date and Time of Bite is required.');
                if (!getVal('<%=txtPlaceBarangay.ClientID%>')) errors.push('Barangay (place of bite) is required.');
                if (!getVal('<%=txtPlaceCity.ClientID%>'))     errors.push('City / Province (place of bite) is required.');
                if (!getVal('<%=ddlCategory.ClientID%>'))      errors.push('Category is required.');
            }
            if (step === 4) {
                if (!getVal('<%=ddlBitingAnimal.ClientID%>')) errors.push('Animal Type is required.');
            }
            if (errors.length > 0) {
                showNotifyModal(errors.join('\n'), 'warning');
                return false;
            }
            return true;
        }

        function getVal(id) {
            var el = document.getElementById(id);
            return el ? el.value.trim() : '';
        }

        function getLbl(id) {
            var el = document.getElementById(id);
            if (!el) return '—';
            if (el.tagName === 'SELECT') return el.options[el.selectedIndex] ? el.options[el.selectedIndex].text : '—';
            return el.value.trim() || '—';
        }

        function row(key, val) {
            return '<div class="review-row"><span class="review-key">' + key + '</span><span class="review-val">' + (val || '—') + '</span></div>';
        }

        function buildReview() {
            document.getElementById('reviewPersonal').innerHTML =
                row('First Name',       getVal('<%=txtFirstName.ClientID%>')) +
                row('Last Name',        getVal('<%=txtLastName.ClientID%>')) +
                row('Date of Birth',    getVal('<%=txtDOB.ClientID%>')) +
                row('Gender',           getLbl('<%=ddlGender.ClientID%>')) +
                row('Civil Status',     getLbl('<%=ddlCivilStatus.ClientID%>')) +
                row('Contact No',       getVal('<%=txtContactNo.ClientID%>')) +
                row('Occupation',       getLbl('<%=ddlOccupation.ClientID%>')) +
                row('Blood Pressure',   getVal('<%=txtBloodPressure.ClientID%>')) +
                row('Temperature',      getVal('<%=txtTemperature.ClientID%>')) +
                row('Weight',           getVal('<%=txtWeight.ClientID%>')) +
                row('Capillary Refill', getVal('<%=txtCapillaryRefill.ClientID%>'));

            document.getElementById('reviewAddress').innerHTML =
                row('House No.',        getVal('<%=txtHouseNo.ClientID%>')) +
                row('Street',           getVal('<%=txtSubdivision.ClientID%>')) +
                row('Barangay',         getVal('<%=txtBarangay.ClientID%>')) +
                row('City / Province',  getVal('<%=txtProvinceCity.ClientID%>')) +
                row('Emergency Person', getVal('<%=txtEmergencyContactPerson.ClientID%>')) +
                row('Emergency No.',    getVal('<%=txtEmergencyContactNo.ClientID%>'));

            document.getElementById('reviewBite').innerHTML =
                row('Date & Time',      getVal('<%=txtBiteDateTime.ClientID%>')) +
                row('Place Barangay',   getVal('<%=txtPlaceBarangay.ClientID%>')) +
                row('Place City',       getVal('<%=txtPlaceCity.ClientID%>')) +
                row('Category',         getLbl('<%=ddlCategory.ClientID%>')) +
                row('Type of Exposure', getLbl('<%=ddlExposureType.ClientID%>')) +
                row('Manifestation',    getLbl('<%=ddlManifestation.ClientID%>'));

            document.getElementById('reviewAnimal').innerHTML =
                row('Animal Type',    getLbl('<%=ddlBitingAnimal.ClientID%>')) +
                row('Ownership',      getLbl('<%=ddlOwnership.ClientID%>')) +
                row('Animal Status',  getLbl('<%=ddlAnimalStatus.ClientID%>')) +
                row('Circumstance',   getLbl('<%=ddlCircumstance.ClientID%>')) +
                row('Wound Location', getVal('<%=txtWoundLocation.ClientID%>')) +
                row('Wound Type',     getLbl('<%=ddlWoundType.ClientID%>')) +
                row('Bleeding',       getLbl('<%=ddlBleeding.ClientID%>')) +
                row('Wound Washed',   getLbl('<%=ddlWoundWashed.ClientID%>'));
        }

        function resetWizard() {
            showNotifyModal('Clear all fields and start over?', 'warning');
            document.getElementById('notifyModalBtn').onclick = function () {
                hideNotifyModal();
                document.getElementById('notifyModalBtn').onclick = hideNotifyModal;
                ['<%=txtFirstName.ClientID%>','<%=txtLastName.ClientID%>',
                 '<%=txtDOB.ClientID%>','<%=txtContactNo.ClientID%>',
                 '<%=txtBloodPressure.ClientID%>','<%=txtTemperature.ClientID%>',
                 '<%=txtWeight.ClientID%>','<%=txtCapillaryRefill.ClientID%>',
                 '<%=txtHouseNo.ClientID%>','<%=txtSubdivision.ClientID%>',
                 '<%=txtBarangay.ClientID%>','<%=txtProvinceCity.ClientID%>',
                 '<%=txtEmergencyContactPerson.ClientID%>','<%=txtEmergencyContactNo.ClientID%>',
                 '<%=txtBiteDateTime.ClientID%>','<%=txtPlaceHouseNo.ClientID%>',
                 '<%=txtPlaceStreet.ClientID%>','<%=txtPlaceBarangay.ClientID%>',
                 '<%=txtPlaceCity.ClientID%>','<%=txtWoundLocation.ClientID%>'
                ].forEach(function(id){ var el=document.getElementById(id); if(el) el.value=''; });
                ['<%=ddlGender.ClientID%>','<%=ddlCivilStatus.ClientID%>',
                 '<%=ddlOccupation.ClientID%>','<%=ddlCategory.ClientID%>',
                 '<%=ddlExposureType.ClientID%>','<%=ddlManifestation.ClientID%>',
                 '<%=ddlBitingAnimal.ClientID%>','<%=ddlOwnership.ClientID%>',
                 '<%=ddlAnimalStatus.ClientID%>','<%=ddlCircumstance.ClientID%>',
                 '<%=ddlWoundType.ClientID%>','<%=ddlBleeding.ClientID%>',
                 '<%=ddlWoundWashed.ClientID%>'
                ].forEach(function(id){ var el=document.getElementById(id); if(el) el.selectedIndex=0; });
                goStep(1);
            };
        }

        function toggleVitals() {
            var v = document.getElementById('optionalVitals'),
                icon = document.getElementById('vitalsIcon');
            if (v) { v.classList.toggle('hidden'); if (icon) icon.textContent = v.classList.contains('hidden') ? '▶' : '▼'; }
        }

        // ── Panel switching ──────────────────────────────────────────
        function setActivePanelButton(panelId) {
            ['btnViewPanel','btnAddPanel'].forEach(function(id) {
                var b = document.getElementById(id); if (!b) return;
                b.classList.remove('bg-blue-600','text-white','border-blue-600','shadow');
                b.classList.add('bg-white','text-slate-700','border-slate-300');
            });
            var a = document.getElementById(panelId === 'viewPatientPanel' ? 'btnViewPanel' : 'btnAddPanel');
            if (a) {
                a.classList.remove('bg-white','text-slate-700','border-slate-300');
                a.classList.add('bg-blue-600','text-white','border-blue-600','shadow');
            }
        }

        function updateViewLayout() {
            var vl = document.getElementById('viewLayout');
            var pr = document.getElementById('<%=pnlRecordPreviewContainer.ClientID%>');
            if (!vl || !pr) return;
            var vis = pr.style.display !== 'none' && !pr.hasAttribute('hidden') && pr.offsetParent !== null;
            vl.classList.remove('with-preview','no-preview');
            vl.classList.add(vis ? 'with-preview' : 'no-preview');
        }

        function showPanel(panelId) {
            document.querySelectorAll('.panel').forEach(function(p) { p.classList.add('hidden'); });
            var t = document.getElementById(panelId); if (t) t.classList.remove('hidden');
            setActivePanelButton(panelId);
            var hf = document.getElementById('<%=hfActivePanel.ClientID%>'); if (hf) hf.value = panelId;
            setTimeout(updateViewLayout, 50);
        }

        // ── Init ─────────────────────────────────────────────────────
        document.addEventListener('DOMContentLoaded', function () {
            var hf = document.getElementById('<%=hfActivePanel.ClientID%>');
            showPanel(hf && hf.value ? hf.value : 'viewPatientPanel');
            setTimeout(updateViewLayout, 100);
        });

        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') { hideConfirmModal(); hideNotifyModal(); }
        });
        document.addEventListener('click', function (e) {
            var cm = document.getElementById('confirmModal');
            if (cm && !cm.classList.contains('hidden') && e.target === cm) hideConfirmModal();
            var nm = document.getElementById('notifyModal');
            if (nm && !nm.classList.contains('hidden') && e.target === nm) hideNotifyModal();
        });
        window.addEventListener('resize', updateViewLayout);

    </script>

</asp:Content>