<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Home.aspx.cs" Inherits="SBI.Home" %>

<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>SBI Medical Animal Bite Center</title>
    <!-- Tailwind -->
    <script src="https://cdn.tailwindcss.com"></script>
</head>

<body class="bg-gray-50 pt-24">
    <!-- TOP BAR -->
    <div class="bg-blue-900 text-white text-sm flex justify-between px-6 py-2 fixed top-0 w-full z-50">
        <span>SBI Medical Animal Bite Center & Vaccination Clinic - Morong Branch</span>
        <div class="space-x-4">
            <span>📞 +63-9286-052-684</span>
            <span>📍 Morong, Rizal, Philippines</span>
        </div>
    </div>

    <form id="form1" runat="server">
        <!-- HEADER -->
        <header class="bg-white shadow-md fixed top-8 w-full z-40">
            <div class="max-w-7xl mx-auto flex justify-between items-center px-8 py-4">
                <!-- LOGO -->
                <div class="flex items-center gap-3">
                    <div>
                        <h1 class="font-bold text-blue-900 text-lg">SBI Medical Animal Bite Center</h1>
                        <p class="text-xs text-gray-600">& Vaccination Clinic - Morong Branch</p>
                    </div>
                </div>

                <!-- NAVIGATION -->
                <nav class="flex items-center gap-6">
                    <button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="home">Home</button>
                    <button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="about">About</button>
                    <button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="services">Services</button>
                    <button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="contact">Contact</button>
                    <button type="button" onclick="window.location.href='Login.aspx'" class="bg-blue-700 text-white px-4 py-2 rounded-lg hover:bg-blue-800">Login</button>
                </nav>
            </div>
        </header>

        <!-- HERO -->
        <section id="home" class="bg-blue-800 text-white py-32" style="scroll-margin-top: 120px;">
            <div class="max-w-6xl mx-auto px-8 grid md:grid-cols-2 gap-12 items-center">
                <div>
                    <span class="bg-blue-600 px-4 py-1 rounded text-sm">DOH Accredited Facility</span>
                    <h2 class="text-5xl font-bold mt-6">Protecting Lives from <span class="text-yellow-300">Animal Bites</span></h2>
                    <p class="mt-6 text-blue-100 text-lg">Committed to delivering exceptional and compassionate care.</p>
                    <div class="mt-8 flex gap-4">
                    </div>
                </div>
                <div>
                    <img src="<%= ResolveUrl("~/Icons/logo.png") %>" class="rounded-xl" alt="Clinic">
                </div>
            </div>
        </section>

        <!-- ABOUT -->
        <section id="about" class="py-24 bg-white" style="scroll-margin-top: 120px;">
            <div class="max-w-6xl mx-auto px-8 text-center">
                <h2 class="text-3xl font-bold text-blue-900">Our Purpose</h2>
                <p class="text-gray-600 mt-2">Committed to excellence in rabies prevention and bite wound management.</p>
                <div class="grid md:grid-cols-2 gap-8 mt-12">
                    <div class="bg-blue-50 p-8 rounded-xl shadow">
                        <h3 class="text-xl font-bold text-blue-800 mb-4">MISSION</h3>
                        <p class="text-gray-700 text-justify">
                            Our mission is to provide high-quality, accessible, and economically affordable anti-rabies vaccines and comprehensive 
                            rabies prevention and animal bite care services that safeguard the health and well-being of our communities.
                        </p>
                    </div>
                    <div class="bg-blue-50 p-8 rounded-xl shadow">
                        <h3 class="text-xl font-bold text-blue-800 mb-4">VISION</h3>
                        <p class="text-gray-700 text-justify">
                            Our vision is to be the leading and most trusted animal bite treatment center in the Philippines, recognized for our strong 
                            commitment to eradicating human rabies, delivering exceptional patient care, and fostering a rabies-free nation.
                        </p>
                    </div>
                </div>
            </div>
        </section>

        <!-- SERVICES & WHY CHOOSE US -->
        <section id="services" class="py-24 bg-blue-900" style="scroll-margin-top: 120px;">
            <div class="max-w-6xl mx-auto px-8">
                <div class="grid md:grid-cols-2 gap-8">
                    <!-- Services Column -->
                    <div class="bg-white p-8 rounded-xl shadow h-full flex flex-col">
                        <h3 class="text-xl font-bold text-blue-800 mb-6 text-center">Our Services</h3>
                        <div class="space-y-5 flex-grow">
                            <div>
                                <h4 class="font-bold text-blue-700">Animal Bite Consultation & Management</h4>
                                <p class="text-sm text-gray-600 mt-1">Professional wound assessment and treatment.</p>
                            </div>
                            <div>
                                <h4 class="font-bold text-blue-700">Anti-Rabies Vaccination</h4>
                                <p class="text-sm text-gray-600 mt-1">For humans and pets, using WHO-approved vaccines.</p>
                            </div>
                            <div>
                                <h4 class="font-bold text-blue-700">Tetanus & General Vaccinations</h4>
                                <p class="text-sm text-gray-600 mt-1">Protection against tetanus and other vaccine-preventable diseases.</p>
                            </div>
                            <div>
                                <h4 class="font-bold text-blue-700">Public Health Education</h4>
                                <p class="text-sm text-gray-600 mt-1">Awareness programs for rabies prevention.</p>
                            </div>
                        </div>
                    </div>
                    <!-- Why Choose Us Column -->
                    <div class="bg-white p-8 rounded-xl shadow h-full flex flex-col">
                        <h3 class="text-xl font-bold text-blue-800 mb-6 text-center">Why Choose Us</h3>
                        <div class="space-y-5 flex-grow">
                            <div>
                                <h4 class="font-bold text-blue-700">DOH-Recognized & Certified</h4>
                                <p class="text-sm text-gray-600 mt-1">Accredited facility meeting national standards.</p>
                            </div>
                            <div>
                                <h4 class="font-bold text-blue-700">Affordable & Accessible</h4>
                                <p class="text-sm text-gray-600 mt-1">Quality care at reasonable prices.</p>
                            </div>
                            <div>
                                <h4 class="font-bold text-blue-700">Highly Trained Professionals</h4>
                                <p class="text-sm text-gray-600 mt-1">Experienced medical staff dedicated to your care.</p>
                            </div>
                            <div>
                                <h4 class="font-bold text-blue-700">Multiple Branches</h4>
                                <p class="text-sm text-gray-600 mt-1">Convenient locations across the Philippines.</p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>

        <!-- CORE VALUES -->
        <section class="py-24 bg-gray-50">
            <div class="max-w-6xl mx-auto px-8 text-center">
                <h2 class="text-3xl font-bold text-blue-900">Our Core Values</h2>
                <p class="text-gray-600 mt-2">The principles that guide us every day.</p>
                <div class="grid md:grid-cols-2 lg:grid-cols-4 gap-8 mt-12">
                    <div class="bg-white p-6 rounded-xl shadow hover:shadow-lg transition">
                        <h4 class="font-bold text-blue-800 text-xl">Quality & Excellence</h4>
                        <p class="text-sm text-gray-600 mt-3">Delivering exceptional and compassionate care through continuous learning and professional development.</p>
                    </div>
                    <div class="bg-white p-6 rounded-xl shadow hover:shadow-lg transition">
                        <h4 class="font-bold text-blue-800 text-xl">Accountability & Integrity</h4>
                        <p class="text-sm text-gray-600 mt-3">Upholding the highest standards of ethics and responsibility in all aspects of our service.</p>
                    </div>
                    <div class="bg-white p-6 rounded-xl shadow hover:shadow-lg transition">
                        <h4 class="font-bold text-blue-800 text-xl">Service-Oriented</h4>
                        <p class="text-sm text-gray-600 mt-3">Prioritizing public health awareness, prevention, and patient-centered care for every individual.</p>
                    </div>
                    <div class="bg-white p-6 rounded-xl shadow hover:shadow-lg transition">
                        <h4 class="font-bold text-blue-800 text-xl">Rabies-Free Philippines</h4>
                        <p class="text-sm text-gray-600 mt-3">Working tirelessly to prevent and manage rabies through education, vaccination, and innovative solutions.</p>
                    </div>
                </div>
                <p class="mt-12 text-blue-700 italic font-medium">
                    "Let these values guide us as we work together toward a healthier and rabies-free Philippines!
                    <br>
                    Because SBI Care Beyond Compare."
                </p>
            </div>
        </section>

        <!-- COMPACT BRANCHES SECTION -->
        <section class="py-12 bg-white">
            <div class="max-w-7xl mx-auto px-4 text-center">
                <h2 class="text-2xl font-bold text-blue-900 mb-6">Our Branches in Rizal Province</h2>
                <div class="flex flex-wrap justify-center gap-3">
                    <!-- Padilla Branch -->
                    <div class="flex-1 min-w-[180px] max-w-[200px] bg-gray-50 p-3 rounded-lg shadow text-left text-xs">
                        <h3 class="font-semibold text-blue-800 text-sm">Padilla</h3>
                        <p class="text-gray-600 truncate">📍 Lot 19 Blk 2, Langhaya</p>
                        <p class="text-gray-600">📌 In front of ADH</p>
                        <p class="text-gray-600">☎ 0998 867 7234</p>
                        <p class="text-gray-600">🕒 8AM–5PM daily</p>
                    </div>
                    <!-- Boso-Boso Branch -->
                    <div class="flex-1 min-w-[180px] max-w-[200px] bg-gray-50 p-3 rounded-lg shadow text-left text-xs">
                        <h3 class="font-semibold text-blue-800 text-sm">Boso-Boso</h3>
                        <p class="text-gray-600 truncate">📍 Inside SBI Lying-In</p>
                        <p class="text-gray-600">📌 30m from Arko</p>
                        <p class="text-gray-600">☎ 0928 605 2684</p>
                        <p class="text-gray-600">🕒 8AM–5PM daily</p>
                    </div>
                    <!-- Montalban Branch -->
                    <div class="flex-1 min-w-[180px] max-w-[200px] bg-gray-50 p-3 rounded-lg shadow text-left text-xs">
                        <h3 class="font-semibold text-blue-800 text-sm">Montalban</h3>
                        <p class="text-gray-600 truncate">📍 2F Clinica Burgos</p>
                        <p class="text-gray-600">📌 Near Ynares</p>
                        <p class="text-gray-600">☎ 0998 867 7235</p>
                        <p class="text-gray-600">🕒 8AM–8PM daily</p>
                    </div>
                    <!-- Morong Branch -->
                    <div class="flex-1 min-w-[180px] max-w-[200px] bg-gray-50 p-3 rounded-lg shadow text-left text-xs">
                        <h3 class="font-semibold text-blue-800 text-sm">Morong</h3>
                        <p class="text-gray-600 truncate">📍 81 Tomas Claudio St</p>
                        <p class="text-gray-600">📌 Beside Super8</p>
                        <p class="text-gray-600">☎ 0928 605 2684</p>
                        <p class="text-gray-600">🕒 Mon–Sat 8AM–8PM<br>
                            Sun 8AM–5PM</p>
                    </div>
                    <!-- Cainta Branch -->
                    <div class="flex-1 min-w-[180px] max-w-[200px] bg-gray-50 p-3 rounded-lg shadow text-left text-xs">
                        <h3 class="font-semibold text-blue-800 text-sm">Cainta</h3>
                        <p class="text-gray-600 truncate">📍 05 Parola St</p>
                        <p class="text-gray-600">📌 Behind CMH</p>
                        <p class="text-gray-600">☎ 0931 180 0996</p>
                        <p class="text-gray-600">🕒 8AM–8PM daily</p>
                    </div>
                    <!-- Tanay Branch -->
                    <div class="flex-1 min-w-[180px] max-w-[200px] bg-gray-50 p-3 rounded-lg shadow text-left text-xs">
                        <h3 class="font-semibold text-blue-800 text-sm">Tanay</h3>
                        <p class="text-gray-600 truncate">📍 21 Yujuico St</p>
                        <p class="text-gray-600">📌 In front of plaza</p>
                        <p class="text-gray-600">☎ +63 921 974 9679</p>
                        <p class="text-gray-600">🕒 8AM–5PM daily</p>
                    </div>
                </div>
            </div>
        </section>

        <!-- CONTACT SECTION -->
        <section id="contact" class="bg-blue-900 text-white py-20" style="scroll-margin-top: 120px;">
            <div class="max-w-6xl mx-auto px-8">
                <h3 class="text-2xl font-bold">Contact Information</h3>
                <p class="mt-4">📞 <a href="tel:+639286052684" class="hover:underline">0928 605 2684</a></p>
                <p>✉ <a href="mailto:sbimedicalanimalbitecenter@gmail.com" class="hover:underline">sbimedicalanimalbitecenter@gmail.com</a></p>
                <p>📍 81 Tomas Claudio St. Manila E. Rd, Morong, Rizal</p>
                <p class="mt-4">Monday-Sunday: 8:00 AM - 5:00 PM</p>
            </div>
        </section>

        <!-- FOOTER -->
        <footer class="bg-gray-900 text-white py-8 text-center">
            <p>SBI Medical Animal Bite Center</p>
            <p class="text-gray-400 text-sm mt-2">© 2026 All Rights Reserved</p>
        </footer>
    </form>

    <script>
        const navLinks = document.querySelectorAll('.nav-link');
        navLinks.forEach(btn => {
            btn.addEventListener('click', () => {
                const target = document.getElementById(btn.dataset.target);
                if (target) {
                    target.scrollIntoView({ behavior: 'smooth' });
                }
            });
        });
    </script>
</body>
</html>
