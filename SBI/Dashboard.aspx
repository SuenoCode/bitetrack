<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SBI.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Morong Choropleth</title>

    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

    <style>
        #map {
            height: 600px;
            width: 100%;
            border-radius: 12px;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="bg-slate-100 min-h-full p-4 space-y-4">

        <!-- Header -->
        <div class="flex justify-between items-center">
            <div>
                <h1 class="text-2xl font-head1 font-bold text-slate-800">Dashboard</h1>
                <p class="text-gray-500 text-sm">
                    SBI Medical - Morong Branch Today:
                    <span id="today-date" class="font-medium"></span>
                </p>
            </div>

        </div>

        <!-- Stats -->
        <section class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-6 gap-4">
            <div class="bg-white p-4 rounded-xl shadow">
                <p class="text-gray-500 text-xs">Cases Today</p>
                <p class="text-2xl font-bold" id="caseValue">0</p>
            </div>
            <div class="bg-white p-4 rounded-xl shadow">
                <p class="text-gray-500 text-xs">Monthly Cases</p>
                <p class="text-2xl font-bold" id="monthlyCaseValue">0</p>
            </div>
            <div class="bg-white p-4 rounded-xl shadow">
                <p class="text-gray-500 text-xs">High-Risk Cases</p>
                <p class="text-2xl font-bold" id="highRiskCaseValue">0</p>
            </div>
            <div class="bg-white p-4 rounded-xl shadow">
                <p class="text-gray-500 text-xs">Ongoing Treatments</p>
                <p class="text-2xl font-bold" id="treatmentCountValue">0</p>
            </div>
            <div class="bg-white p-4 rounded-xl shadow">
                <p class="text-gray-500 text-xs">Completed Cases</p>
                <p class="text-2xl font-bold" id="completedCaseValue">0</p>
            </div>
            <div class="bg-white p-4 rounded-xl shadow">
                <p class="text-gray-500 text-xs">Stock Alert</p>
                <p class="text-2xl font-bold text-red-500" id="stockValue">0</p>
            </div>
        </section>

        <!-- Map -->
        <div id="map" class="w-full h-[520px] rounded-xl shadow bg-white"></div>

        <!-- Row 1: Monthly Cases + Vaccine Usage -->
        <div class="grid lg:grid-cols-2 gap-4">
            <div class="bg-white rounded-xl shadow p-4">
                <h2 class="text-base font-semibold text-slate-800 mb-1">Monthly Cases Overview</h2>
                <p class="text-xs text-slate-400 mb-3">Number of bite cases recorded per month</p>
                <div class="h-[220px]">
                    <canvas id="casesChart"></canvas>
                </div>
            </div>
            <div class="bg-white rounded-xl shadow p-4">
                <h2 class="text-base font-semibold text-slate-800 mb-1">Vaccine Usage Trend (Weekly)</h2>
                <p class="text-xs text-slate-400 mb-3">Doses administered per week this month</p>
                <div class="h-[220px]">
                    <canvas id="vaccineChart"></canvas>
                </div>
            </div>
        </div>

        <!-- Row 2: Cases by Category + Cases by Animal Type -->
        <div class="grid lg:grid-cols-2 gap-4">
            <div class="bg-white rounded-xl shadow p-4">
                <h2 class="text-base font-semibold text-slate-800 mb-1">Cases by Category</h2>
                <p class="text-xs text-slate-400 mb-3">Distribution of bite cases by WHO exposure category</p>
                <div class="h-[220px] flex items-center justify-center">
                    <canvas id="categoryChart"></canvas>
                </div>
            </div>
            <div class="bg-white rounded-xl shadow p-4">
                <h2 class="text-base font-semibold text-slate-800 mb-1">Cases by Animal Type</h2>
                <p class="text-xs text-slate-400 mb-3">Breakdown of biting animal per recorded case</p>
                <div class="h-[220px] flex items-center justify-center">
                    <canvas id="animalChart"></canvas>
                </div>
            </div>
        </div>

        <!-- Row 3: Cases by Exposure Type + Cases by Wound Type -->
        <div class="grid lg:grid-cols-2 gap-4">
            <div class="bg-white rounded-xl shadow p-4">
                <h2 class="text-base font-semibold text-slate-800 mb-1">Cases by Exposure Type</h2>
                <p class="text-xs text-slate-400 mb-3">Bite vs. Non-bite / Play bite exposures</p>
                <div class="h-[220px] flex items-center justify-center">
                    <canvas id="exposureChart"></canvas>
                </div>
            </div>
            <div class="bg-white rounded-xl shadow p-4">
                <h2 class="text-base font-semibold text-slate-800 mb-1">Cases by Wound Type</h2>
                <p class="text-xs text-slate-400 mb-3">Type of wound recorded across all bite cases</p>
                <div class="h-[220px]">
                    <canvas id="woundChart"></canvas>
                </div>
            </div>
        </div>

        
    </div>

    <script src="MapContent/morong.js"></script>

    <script>
        document.addEventListener("DOMContentLoaded", function () {

            // ─── Map setup ───
            const mapDiv = document.getElementById("map");
            if (!mapDiv || mapDiv.offsetHeight === 0) {
                alert("Map div has no height.");
                return;
            }
            if (typeof morongData === "undefined") {
                alert("morongData is undefined. Check MapContent/morong.js.");
                return;
            }

            const map = L.map("map", { zoomControl: true, maxZoom: 18 });
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
                attribution: "&copy; OpenStreetMap contributors"
            }).addTo(map);

            function utmToLatLon(easting, northing, zone = 51) {
                const a = 6378137.0, f = 1 / 298.257223563;
                const b = a * (1 - f);
                const e2 = 1 - (b / a) ** 2;
                const k0 = 0.9996, E0 = 500000, N0 = 0;
                const x = easting - E0, y = northing - N0;
                const M = y / k0;
                const mu = M / (a * (1 - e2 / 4 - 3 * e2 ** 2 / 64 - 5 * e2 ** 3 / 256));
                const e1 = (1 - Math.sqrt(1 - e2)) / (1 + Math.sqrt(1 - e2));
                let phi1 = mu
                    + (3 * e1 / 2 - 27 * e1 ** 3 / 32) * Math.sin(2 * mu)
                    + (21 * e1 ** 2 / 16 - 55 * e1 ** 4 / 32) * Math.sin(4 * mu)
                    + (151 * e1 ** 3 / 96) * Math.sin(6 * mu)
                    + (1097 * e1 ** 4 / 512) * Math.sin(8 * mu);
                const N1 = a / Math.sqrt(1 - e2 * Math.sin(phi1) ** 2);
                const T1 = Math.tan(phi1) ** 2;
                const C1 = e2 * Math.cos(phi1) ** 2 / (1 - e2);
                const R1 = a * (1 - e2) / (1 - e2 * Math.sin(phi1) ** 2) ** 1.5;
                const D = x / (N1 * k0);
                const lat = phi1 - (N1 * Math.tan(phi1) / R1) * (
                    D ** 2 / 2
                    - (5 + 3 * T1 + 10 * C1 - 4 * C1 ** 2 - 9 * e2) * D ** 4 / 24
                    + (61 + 90 * T1 + 298 * C1 + 45 * T1 ** 2 - 252 * e2 - 3 * C1 ** 2) * D ** 6 / 720
                );
                const lon0 = ((zone - 1) * 6 - 180 + 3) * Math.PI / 180;
                const lon = lon0 + (
                    D
                    - (1 + 2 * T1 + C1) * D ** 3 / 6
                    + (5 - 2 * C1 + 28 * T1 - 3 * C1 ** 2 + 8 * e2 + 24 * T1 ** 2) * D ** 5 / 120
                ) / Math.cos(phi1);
                return [lat * 180 / Math.PI, lon * 180 / Math.PI];
            }

            function reprojectFeatureCollection(fc) {
                return {
                    ...fc,
                    features: fc.features.map(f => ({
                        ...f,
                        geometry: {
                            ...f.geometry,
                            coordinates: f.geometry.coordinates.map(ring =>
                                ring.map(([e, n]) => {
                                    const [lat, lon] = utmToLatLon(e, n);
                                    return [lon, lat];
                                })
                            )
                        }
                    }))
                };
            }

            const geoData = reprojectFeatureCollection(morongData);

            function getColor(c) {
                return c >= 30 ? '#dc2626' : c >= 20 ? '#f97316' : c >= 10 ? '#ffce00' : c >= 1 ? '#eab308' : '#22c55e';
            }

            function addLegend() {
                const legend = L.control({ position: 'bottomright' });
                legend.onAdd = function () {
                    const div = L.DomUtil.create('div', 'legend');
                    div.style.cssText = 'background:white;padding:12px;border-radius:8px;box-shadow:0 0 15px rgba(0,0,0,0.2);line-height:24px;min-width:180px;font-family:Arial,sans-serif;';
                    div.innerHTML = `
                        <h4 style="margin:0 0 10px 0;font-weight:bold;border-bottom:1px solid #ddd;padding-bottom:5px;color:#333;">Cases per Barangay</h4>
                        ${[['#dc2626','30+ cases'],['#f97316','20–29 cases'],['#ffce00','10–19 cases'],['#eab308','1–9 cases'],['#22c55e','No cases']].map(([c,l]) =>
                        `<div style="display:flex;align-items:center;margin-bottom:8px;">
                            <div style="width:20px;height:20px;background:${c};margin-right:10px;border:1px solid #999;"></div>
                            <span><b>${l}</b></span>
                        </div>`).join('')}`;
                    return div;
                };
                legend.addTo(map);
            }

            function updateMapWithCaseData(caseData) {
                const caseMap = {};
                if (caseData && caseData.Barangays && caseData.CaseCounts) {
                    for (let i = 0; i < caseData.Barangays.length; i++) {
                        caseMap[caseData.Barangays[i].toUpperCase().trim()] = caseData.CaseCounts[i];
                    }
                }
                if (window.currentLayer) map.removeLayer(window.currentLayer);
                window.currentLayer = L.geoJSON(geoData, {
                    style: function (feature) {
                        const name = feature?.properties?.adm4_en?.toUpperCase().trim() || "UNKNOWN";
                        const count = caseMap[name] || 0;
                        return { color: "#111827", weight: 1, fillColor: getColor(count), fillOpacity: 0.7 };
                    },
                    onEachFeature: function (feature, layer) {
                        const name = feature?.properties?.adm4_en || "Unknown";
                        const count = caseMap[name.toUpperCase().trim()] || 0;
                        layer.bindPopup(`<b>${name}</b><br/><b style="color:${getColor(count)}">Cases: ${count}</b><br/>Status: ${count >= 30 ? 'HIGH' : count >= 20 ? 'MEDIUM-HIGH' : count >= 10 ? 'MEDIUM' : count >= 1 ? 'LOW' : 'NO CASES'}`);
                        layer.on({
                            mouseover: e => { e.target.setStyle({ weight: 3, color: '#1e3a8a', fillOpacity: 0.9 }); e.target.bringToFront(); },
                            mouseout: e => { window.currentLayer.resetStyle(e.target); }
                        });
                    }
                }).addTo(map);
                map.fitBounds(window.currentLayer.getBounds(), { padding: [20, 20] });
                if (caseData && caseData.CaseCounts) {
                    document.getElementById('caseValue').textContent = caseData.CaseCounts.reduce((a, b) => a + b, 0);
                }
            }

            function loadCaseData() {
                fetch('Dashboard.aspx/GetBarangayCases', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                })
                .then(r => r.json())
                .then(result => {
                    if (result.d) {
                        if (result.d.Error) console.error('Server error:', result.d.Error);
                        else updateMapWithCaseData(result.d);
                    }
                })
                .catch(err => console.error('Fetch error:', err));
            }

            addLegend();
            loadCaseData();

            const tempLayer = L.geoJSON(geoData).addTo(map);
            const choroplethBounds = tempLayer.getBounds();
            map.fitBounds(choroplethBounds, { padding: [20, 20] });
            map.setMaxBounds(choroplethBounds);
            map.options.minZoom = map.getBoundsZoom(choroplethBounds);
            map.removeLayer(tempLayer);

            // ─── Shared fetch helper ───
            function fetchChart(endpoint) {
                return fetch('Dashboard.aspx/' + endpoint, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' }
                }).then(r => r.json()).then(res => res.d);
            }

            // ─── Chart instances (kept for future update) ───
            let casesChartInst, vaccineChartInst, categoryChartInst, animalChartInst, exposureChartInst, woundChartInst;

            // 1. Monthly Cases Line Chart
            casesChartInst = new Chart(document.getElementById('casesChart'), {
                type: 'line',
                data: {
                    labels: ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],
                    datasets: [{
                        label: 'Cases',
                        data: [0,0,0,0,0,0,0,0,0,0,0,0],
                        borderColor: '#FF6B35',
                        backgroundColor: 'rgba(255,107,53,0.1)',
                        fill: true,
                        tension: 0.4,
                        pointBackgroundColor: '#FF6B35',
                        pointRadius: 4
                    }]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });

            fetchChart('GetMonthlyCases').then(d => {
                if (d && d.Months && d.Counts) {
                    casesChartInst.data.labels = d.Months;
                    casesChartInst.data.datasets[0].data = d.Counts;
                    casesChartInst.update();
                    const total = d.Counts.reduce((a, b) => a + b, 0);
                    document.getElementById('monthlyCaseValue').textContent = d.Counts[new Date().getMonth()] || 0;
                }
            }).catch(() => {});

            // 2. Weekly Vaccine Bar Chart
            vaccineChartInst = new Chart(document.getElementById('vaccineChart'), {
                type: 'bar',
                data: {
                    labels: ['Week 1','Week 2','Week 3','Week 4'],
                    datasets: [{
                        label: 'Doses',
                        data: [0,0,0,0],
                        backgroundColor: '#2563eb',
                        borderRadius: 6
                    }]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { y: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });

            fetchChart('GetWeeklyVaccineUsage').then(d => {
                if (d && d.Weeks && d.Counts) {
                    vaccineChartInst.data.labels = d.Weeks;
                    vaccineChartInst.data.datasets[0].data = d.Counts;
                    vaccineChartInst.update();
                }
            }).catch(() => {});

            // 3. Cases by Category Doughnut
            categoryChartInst = new Chart(document.getElementById('categoryChart'), {
                type: 'doughnut',
                data: {
                    labels: ['Category I', 'Category II', 'Category III'],
                    datasets: [{
                        data: [0, 0, 0],
                        backgroundColor: ['#22c55e', '#f59e0b', '#ef4444'],
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'right', labels: { font: { size: 12 }, padding: 16 } }
                    },
                    cutout: '60%'
                }
            });

            fetchChart('GetCasesByCategory').then(d => {
                if (d && d.Labels && d.Counts) {
                    categoryChartInst.data.labels = d.Labels;
                    categoryChartInst.data.datasets[0].data = d.Counts;
                    categoryChartInst.update();
                    const highRisk = d.Counts[d.Labels.indexOf('Category III')] || d.Counts[2] || 0;
                    document.getElementById('highRiskCaseValue').textContent = highRisk;
                }
            }).catch(() => {});

            // 4. Cases by Animal Type Doughnut
            animalChartInst = new Chart(document.getElementById('animalChart'), {
                type: 'doughnut',
                data: {
                    labels: ['Dog', 'Cat', 'Others'],
                    datasets: [{
                        data: [0, 0, 0],
                        backgroundColor: ['#3b82f6', '#f97316', '#a855f7'],
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'right', labels: { font: { size: 12 }, padding: 16 } }
                    },
                    cutout: '60%'
                }
            });

            fetchChart('GetCasesByAnimalType').then(d => {
                if (d && d.Labels && d.Counts) {
                    animalChartInst.data.labels = d.Labels;
                    animalChartInst.data.datasets[0].data = d.Counts;
                    animalChartInst.update();
                }
            }).catch(() => {});

            // 5. Cases by Exposure Type Doughnut
            exposureChartInst = new Chart(document.getElementById('exposureChart'), {
                type: 'doughnut',
                data: {
                    labels: ['Bite', 'Non-Bite / Play Bite'],
                    datasets: [{
                        data: [0, 0],
                        backgroundColor: ['#ef4444', '#64748b'],
                        borderWidth: 2,
                        borderColor: '#fff'
                    }]
                },
                options: {
                    responsive: true, maintainAspectRatio: false,
                    plugins: {
                        legend: { position: 'right', labels: { font: { size: 12 }, padding: 16 } }
                    },
                    cutout: '60%'
                }
            });

            fetchChart('GetCasesByExposureType').then(d => {
                if (d && d.Labels && d.Counts) {
                    exposureChartInst.data.labels = d.Labels;
                    exposureChartInst.data.datasets[0].data = d.Counts;
                    exposureChartInst.update();
                }
            }).catch(() => {});

            // 6. Cases by Wound Type Horizontal Bar
            woundChartInst = new Chart(document.getElementById('woundChart'), {
                type: 'bar',
                data: {
                    labels: ['Lacerated','Avulsion','Punctured','Abrasion','Scratches','Hematoma'],
                    datasets: [{
                        label: 'Cases',
                        data: [0, 0, 0, 0, 0, 0],
                        backgroundColor: '#0ea5e9',
                        borderRadius: 4
                    }]
                },
                options: {
                    indexAxis: 'y',
                    responsive: true, maintainAspectRatio: false,
                    plugins: { legend: { display: false } },
                    scales: { x: { beginAtZero: true, ticks: { precision: 0 } } }
                }
            });

            fetchChart('GetCasesByWoundType').then(d => {
                if (d && d.Labels && d.Counts) {
                    woundChartInst.data.labels = d.Labels;
                    woundChartInst.data.datasets[0].data = d.Counts;
                    woundChartInst.update();
                }
            }).catch(() => {});

            // ─── Remaining stats ───
            fetchChart('GetDashboardStats').then(d => {
                if (d) {
                    if (d.OngoingTreatments !== undefined) document.getElementById('treatmentCountValue').textContent = d.OngoingTreatments;
                    if (d.CompletedCases !== undefined)   document.getElementById('completedCaseValue').textContent = d.CompletedCases;
                    if (d.StockAlerts !== undefined)       document.getElementById('stockValue').textContent = d.StockAlerts;
                }
            }).catch(() => {});

            // ─── Date ───
            document.getElementById('today-date').textContent = new Date().toLocaleDateString('en-US', {
                year: 'numeric', month: 'long', day: 'numeric'
            });
        });
    </script>
</asp:Content>
