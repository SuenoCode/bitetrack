<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SBI.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Morong Choropleth</title>

    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>

    <style>
        #map { height: 600px; width: 100%; border-radius: 12px; }
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
        <div class="flex gap-4">
            <button class="px-4 py-2 border border-blue-500 text-blue-500 rounded-lg hover:bg-blue-50">
                Date Range
            </button>
            <button class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">
                Generate Report
            </button>
        </div>
    </div>

    <!-- Map -->
    <div id="map" class="w-full h-[520px] rounded-xl shadow bg-white"></div>

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

    <!-- Charts -->
    <div class="grid lg:grid-cols-2 gap-4">
        <div class="bg-white rounded-xl shadow p-4">
            <h2 class="text-lg font-semibold mb-2">Cases Overview</h2>
            <div class="h-[220px]">
                <canvas id="casesChart"></canvas>
            </div>
        </div>
        <div class="bg-white rounded-xl shadow p-4">
            <h2 class="text-lg font-semibold mb-2">Vaccine Usage Trend (Weekly)</h2>
            <div class="h-[220px]">
                <canvas id="vaccineChart"></canvas>
            </div>
        </div>
    </div>

    <!-- Previous Cases -->
    <div class="bg-white rounded-xl shadow p-4">
        <h2 class="text-lg font-semibold mb-3">Previous Cases</h2>
        <asp:GridView
            ID="gvPreviousCases"
            runat="server"
            CssClass="w-full text-sm border border-gray-200"
            HeaderStyle-CssClass="bg-gray-100 text-left"
            RowStyle-CssClass="border-t"
            AutoGenerateColumns="true">
        </asp:GridView>
    </div>
    </div>

    <!-- IMPORTANT: load your GeoJSON file here so it's guaranteed available -->
    <script src="MapContent/morong.js"></script>

<script>
    document.addEventListener("DOMContentLoaded", function () {

        // ─── Check map div and GeoJSON ───
        const mapDiv = document.getElementById("map");
        if (!mapDiv || mapDiv.offsetHeight === 0) {
            alert("Map div has no height. Make sure #map { height: 600px; } is applied.");
            return;
        }

        if (typeof morongData === "undefined") {
            alert("morongData is undefined. Check if MapContent/morong.js is loading (404).");
            return;
        }

        // ─── Initialize map ───
        const map = L.map("map", {
            zoomControl: true,
            maxZoom: 18
        });

        L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", {
            attribution: "&copy; OpenStreetMap contributors"
        }).addTo(map);

        // ─── UTM → Lat/Lon reprojection ───
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
                                return [lon, lat]; // GeoJSON: [lng, lat]
                            })
                        )
                    }
                }))
            };
        }

        const geoData = reprojectFeatureCollection(morongData);

        // Get color based on case count (3 colors)
        function getColor(caseCount) {
            if (caseCount >= 5) {
                return '#dc2626'; // Red - High (5+ cases)
            } else if (caseCount >= 1) {
                return '#f97316'; // Orange - Medium (1-4 cases)
            } else {
                return '#22c55e'; // Green - Low/No cases
            }
        }

        // Add legend to map
        function addLegend() {
            const legend = L.control({ position: 'bottomright' });

            legend.onAdd = function () {
                const div = L.DomUtil.create('div', 'legend');
                div.style.backgroundColor = 'white';
                div.style.padding = '10px';
                div.style.borderRadius = '5px';
                div.style.boxShadow = '0 0 15px rgba(0,0,0,0.2)';
                div.style.lineHeight = '24px';

                div.innerHTML = `
                    <h4 style="margin:0 0 10px 0; font-weight:bold;">Cases per Barangay</h4>
                    <div style="display:flex; align-items:center; margin-bottom:5px;">
                        <div style="width:20px; height:20px; background:#22c55e; margin-right:8px; border:1px solid #999;"></div>
                        <span>Low (0 cases)</span>
                    </div>
                    <div style="display:flex; align-items:center; margin-bottom:5px;">
                        <div style="width:20px; height:20px; background:#f97316; margin-right:8px; border:1px solid #999;"></div>
                        <span>Medium (1-4 cases)</span>
                    </div>
                    <div style="display:flex; align-items:center; margin-bottom:5px;">
                        <div style="width:20px; height:20px; background:#dc2626; margin-right:8px; border:1px solid #999;"></div>
                        <span>High (5+ cases)</span>
                    </div>
                `;
                return div;
            };

            legend.addTo(map);
        }

        // Update map with case data
        function updateMapWithCaseData(caseData) {
            const caseMap = {};
            if (caseData && caseData.Barangays && caseData.CaseCounts) {
                for (let i = 0; i < caseData.Barangays.length; i++) {
                    caseMap[caseData.Barangays[i].toUpperCase().trim()] = caseData.CaseCounts[i];
                }
            }

            // Remove existing layer if any
            if (window.currentLayer) {
                map.removeLayer(window.currentLayer);
            }

            // Add new layer with case data
            window.currentLayer = L.geoJSON(geoData, {
                style: function (feature) {
                    const barangayName = feature?.properties?.adm4_en?.toUpperCase().trim() || "UNKNOWN";
                    const caseCount = caseMap[barangayName] || 0;

                    return {
                        color: "#111827",
                        weight: 1,
                        fillColor: getColor(caseCount),
                        fillOpacity: 0.7
                    };
                },
                onEachFeature: function (feature, layer) {
                    const name = feature?.properties?.adm4_en || "Unknown";
                    const caseCount = caseMap[name.toUpperCase().trim()] || 0;

                    layer.bindPopup(`
                        <b>${name}</b><br/>
                        <b style="color:${getColor(caseCount)}">Cases: ${caseCount}</b><br/>
                        Status: ${caseCount >= 5 ? 'HIGH ALERT' : caseCount >= 1 ? 'MODERATE' : 'CLEAR'}
                    `);

                    layer.on({
                        mouseover: function (e) {
                            const layer = e.target;
                            layer.setStyle({
                                weight: 3,
                                color: '#1e3a8a',
                                fillOpacity: 0.9
                            });
                            layer.bringToFront();
                        },
                        mouseout: function (e) {
                            window.currentLayer.resetStyle(e.target);
                        }
                    });
                }
            }).addTo(map);

            // Fit map to bounds
            const bounds = window.currentLayer.getBounds();
            map.fitBounds(bounds, { padding: [20, 20] });

            // Update total cases stat
            if (caseData && caseData.CaseCounts) {
                const total = caseData.CaseCounts.reduce((a, b) => a + b, 0);
                document.getElementById('caseValue').textContent = total;
            }
        }

        // Load data from database
        function loadCaseData() {
            console.log('Loading case data from database...');

            // Fetch barangay cases from server
            fetch('Dashboard.aspx/GetBarangayCases', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                }
            })
                .then(response => response.json())
                .then(result => {
                    console.log('Data received from server:', result);

                    if (result.d) {
                        if (result.d.Error) {
                            console.error('Server error:', result.d.Error);
                            // Show error but don't load sample data
                            alert('Error loading case data: ' + result.d.Error);
                        } else {
                            updateMapWithCaseData(result.d);
                            console.log('Real data loaded successfully');
                        }
                    }
                })
                .catch(error => {
                    console.error('Fetch error:', error);
                    alert('Failed to load case data. Please check your connection.');
                });
        }

        // Add legend to map
        addLegend();

        // Load the data
        loadCaseData();

        // ─── Restrict map to choropleth bounds ───
        // Create initial layer just to get bounds
        const tempLayer = L.geoJSON(geoData).addTo(map);
        const choroplethBounds = tempLayer.getBounds();
        map.fitBounds(choroplethBounds, { padding: [20, 20] });
        map.setMaxBounds(choroplethBounds);
        map.options.minZoom = map.getBoundsZoom(choroplethBounds);
        map.removeLayer(tempLayer);

        // ─── Initialize Charts ───
        new Chart(document.getElementById('casesChart'), {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [{
                    data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0],
                    borderColor: '#FF6B35',
                    backgroundColor: 'rgba(255,107,53,0.1)',
                    fill: true,
                    tension: 0.3
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });

        new Chart(document.getElementById('vaccineChart'), {
            type: 'bar',
            data: {
                labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
                datasets: [{
                    data: [0, 0, 0, 0],
                    backgroundColor: '#2563eb',
                    borderRadius: 6
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });

        // ─── Dashboard Stats ───
        document.getElementById('today-date').textContent = new Date().toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });

        document.getElementById('monthlyCaseValue').textContent = '0';
        document.getElementById('highRiskCaseValue').textContent = '0';
        document.getElementById('treatmentCountValue').textContent = '0';
        document.getElementById('completedCaseValue').textContent = '0';
        document.getElementById('stockValue').textContent = '0';
    });
</script>
</asp:Content>