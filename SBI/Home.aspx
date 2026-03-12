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
<span>+63-9286-052-684</span>
<span>Morong, Rizal, Philippines</span>
</div>
</div>

<form id="form1" runat="server">

<!-- HEADER -->
<header class="bg-white shadow-md fixed top-8 w-full z-40">

<div class="max-w-7xl mx-auto flex justify-between items-center px-8 py-4">

<!-- LOGO -->
<div class="flex items-center gap-3">

<div class="w-12 h-12 bg-gray-200 rounded-full"></div>

<div>
<h1 class="font-bold text-blue-900 text-lg">
SBI Medical Animal Bite Center
</h1>

<p class="text-xs text-gray-600">
& Vaccination Clinic - Morong Branch
</p>
</div>

</div>

<!-- NAVIGATION -->
<nav class="flex items-center gap-6">

<button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="home">Home</button>

<button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="about">About</button>

<button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="services">Services</button>

<button type="button" class="nav-link text-gray-700 hover:text-blue-700 font-medium" data-target="contact">Contact</button>

<button type="button"
onclick="window.location.href='Login.aspx'"
class="bg-blue-700 text-white px-4 py-2 rounded-lg hover:bg-blue-800">
Login
</button>

</nav>

</div>
</header>

<!-- HERO -->
<section id="home" class="bg-blue-800 text-white py-32">

<div class="max-w-6xl mx-auto px-8 grid md:grid-cols-2 gap-12 items-center" img src>

<div>

<span class="bg-blue-600 px-4 py-1 rounded text-sm">
DOH Accredited Facility
</span>

<h2 class="text-5xl font-bold mt-6">
Protecting Lives from
<span class="text-yellow-300">Animal Bites</span>
</h2>

<p class="mt-6 text-blue-100 text-lg">
Providing safe and effective anti-rabies vaccination and professional bite wound treatment to protect communities from rabies.
</p>

<div class="mt-8 flex gap-4">

<button class="bg-yellow-400 text-blue-900 px-6 py-3 rounded-lg font-semibold hover:bg-yellow-300">
Get Treatment
</button>

<button class="border border-white px-6 py-3 rounded-lg hover:bg-white hover:text-blue-800">
Learn More
</button>

</div>

</div>

<div>

<img src="https://images.unsplash.com/photo-1580281657527-47f249e0c3b1"
class="rounded-xl shadow-lg">

</div>

</div>
</section>

<!-- ABOUT -->
<section id="about" class="py-24 bg-white">

<div class="max-w-6xl mx-auto px-8 text-center">

<h2 class="text-3xl font-bold text-blue-900">
Our Purpose
</h2>

<p class="text-gray-600 mt-2">
Committed to excellence in rabies prevention and bite wound management.
</p>

<div class="grid md:grid-cols-2 gap-8 mt-12">

<div class="bg-blue-50 p-8 rounded-xl shadow">
<h3 class="text-xl font-bold text-blue-800 mb-4">MISSION</h3>

<p class="text-gray-700">
To provide high-quality and affordable anti-rabies vaccines while ensuring fast and compassionate medical care for all animal bite victims.
</p>
</div>

<div class="bg-blue-50 p-8 rounded-xl shadow">
<h3 class="text-xl font-bold text-blue-800 mb-4">VISION</h3>

<p class="text-gray-700">
To be the most trusted animal bite treatment center in the Philippines dedicated to eliminating rabies-related fatalities.
</p>
</div>

</div>
</div>
</section>

<!-- SERVICES -->
<section id="services" class="py-24 bg-gray-50">

<div class="max-w-6xl mx-auto px-8 text-center">

<h2 class="text-3xl font-bold text-blue-900">
Our Services
</h2>

<div class="grid md:grid-cols-4 gap-8 mt-12">

<div class="bg-white p-6 rounded-xl shadow hover:shadow-lg">
<div class="text-3xl">🩺</div>
<h4 class="font-bold mt-3">Animal Bite Consultation</h4>
<p class="text-sm text-gray-600 mt-2">Professional wound assessment.</p>
</div>

<div class="bg-white p-6 rounded-xl shadow hover:shadow-lg">
<div class="text-3xl">💉</div>
<h4 class="font-bold mt-3">Anti-Rabies Vaccination</h4>
<p class="text-sm text-gray-600 mt-2">WHO-approved vaccines.</p>
</div>

<div class="bg-white p-6 rounded-xl shadow hover:shadow-lg">
<div class="text-3xl">🔬</div>
<h4 class="font-bold mt-3">Post Exposure Prophylaxis</h4>
<p class="text-sm text-gray-600 mt-2">Complete PEP protocols.</p>
</div>

<div class="bg-white p-6 rounded-xl shadow hover:shadow-lg">
<div class="text-3xl">📢</div>
<h4 class="font-bold mt-3">Rabies Awareness</h4>
<p class="text-sm text-gray-600 mt-2">Community education programs.</p>
</div>

</div>
</div>
</section>

<!-- WHY CHOOSE US -->
<section class="py-24 bg-white">

<div class="max-w-6xl mx-auto px-8 text-center">

<h2 class="text-3xl font-bold text-blue-900">
Why Choose Us
</h2>

<div class="grid md:grid-cols-4 gap-10 mt-12">

<div>
<div class="text-4xl">👨‍⚕️</div>
<h4 class="font-semibold mt-3">Experienced Doctors</h4>
</div>

<div>
<div class="text-4xl">💉</div>
<h4 class="font-semibold mt-3">Safe Vaccines</h4>
</div>

<div>
<div class="text-4xl">⚡</div>
<h4 class="font-semibold mt-3">Fast Treatment</h4>
</div>

<div>
<div class="text-4xl">🏥</div>
<h4 class="font-semibold mt-3">Modern Facility</h4>
</div>

</div>
</div>
</section>

<!-- PROCESS -->
<section class="py-24 bg-gray-50">

<div class="max-w-6xl mx-auto px-8 text-center">

<h2 class="text-3xl font-bold text-blue-900">
Rabies Treatment Process
</h2>

<div class="grid md:grid-cols-4 gap-10 mt-16">

<div>
<div class="text-3xl font-bold text-blue-700">1</div>
<p>Wound Cleaning</p>
</div>

<div>
<div class="text-3xl font-bold text-blue-700">2</div>
<p>Medical Assessment</p>
</div>

<div>
<div class="text-3xl font-bold text-blue-700">3</div>
<p>Vaccination</p>
</div>

<div>
<div class="text-3xl font-bold text-blue-700">4</div>
<p>Follow Up Doses</p>
</div>

</div>
</div>
</section>

<!-- STATS -->
<section class="bg-blue-900 text-white py-20">

<div class="max-w-6xl mx-auto grid md:grid-cols-4 text-center gap-10">

<div>
<h3 class="text-4xl font-bold">5000+</h3>
<p>Patients Treated</p>
</div>

<div>
<h3 class="text-4xl font-bold">10+</h3>
<p>Years of Service</p>
</div>

<div>
<h3 class="text-4xl font-bold">98%</h3>
<p>Patient Satisfaction</p>
</div>

<div>
<h3 class="text-4xl font-bold">24/7</h3>
<p>Support</p>
</div>

</div>
</section>

<!-- TESTIMONIALS -->
<section class="py-24 bg-white">

<div class="max-w-6xl mx-auto px-8 text-center">

<h2 class="text-3xl font-bold text-blue-900">
Patient Testimonials
</h2>

<div class="grid md:grid-cols-3 gap-8 mt-16">

<div class="bg-gray-50 p-6 rounded-xl shadow">
<p>"Very accommodating staff and fast vaccination."</p>
<span class="text-sm text-gray-500">— Maria L.</span>
</div>

<div class="bg-gray-50 p-6 rounded-xl shadow">
<p>"Highly recommended clinic in Morong."</p>
<span class="text-sm text-gray-500">— John D.</span>
</div>

<div class="bg-gray-50 p-6 rounded-xl shadow">
<p>"Clean facility and professional doctors."</p>
<span class="text-sm text-gray-500">— Anna P.</span>
</div>

</div>
</div>
</section>

<!-- CONTACT -->
<section id="contact" class="bg-blue-900 text-white py-20">

<div class="max-w-6xl mx-auto px-8">

<h3 class="text-2xl font-bold">Contact Information</h3>

<p class="mt-4">📞 +63-912-345-6789</p>
<p>✉ sbimedicalanimalbitecenter@gmail.com</p>
<p>📍 Morong, Rizal Philippines</p>

<p class="mt-4">Mon-Sat: 7:00 AM - 5:00 PM</p>
<p>Sun: 8:00 AM - 12:00 PM</p>

</div>
</section>

<!-- FOOTER -->
<footer class="bg-gray-900 text-white py-8 text-center">

<p>SBI Medical Animal Bite Center</p>

<p class="text-gray-400 text-sm mt-2">
© 2026 All Rights Reserved
</p>

</footer>

</form>

<script>

    const navLinks = document.querySelectorAll('.nav-link');

    navLinks.forEach(btn => {

        btn.addEventListener('click', () => {

            const target = document.getElementById(btn.dataset.target);

            target.scrollIntoView({
                behavior: 'smooth'
            });

        });

    });

</script>

</body>
</html>