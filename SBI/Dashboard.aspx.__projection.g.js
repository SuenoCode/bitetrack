
/* BEGIN EXTERNAL SOURCE */

        document.addEventListener("DOMContentLoaded", function () {

            document.getElementById('today-date').textContent =
                new Date().toLocaleDateString('en-PH', {
                    year: 'numeric',
                    month: 'long',
                    day: 'numeric'
                });

            const map = L.map('map').setView([14.545, 121.235], 13);

            L.tileLayer(
                'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
                { attribution: '&copy; OpenStreetMap contributors' }
            ).addTo(map);

            setTimeout(() => {
                map.invalidateSize();
            }, 200);

            new Chart(document.getElementById('casesChart'), {
                type: 'line',
                data: {
                    labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                    datasets: [{
                        data: [0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0], /*******************************************************************/
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
                        data: [0, 0, 0, 0], /************************/
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

            /************/ 
            caseValue.textContent = '0';
            monthlyCaseValue.textContent = '0';
            highRiskCaseValue.textContent = '0';
            treatmentCountValue.textContent = '0';
            completedCaseValue.textContent = '0';
            stockValue.textContent = '0';

        });
    
/* END EXTERNAL SOURCE */

/* BEGIN EXTERNAL SOURCE */

    document.addEventListener("DOMContentLoaded", function () {

        // ===== TODAY DATE =====
        document.getElementById('today-date').textContent =
            new Date().toLocaleDateString('en-PH', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });

        // ===== MAP =====
        const map = L.map('map').setView([14.545, 121.235], 13);

        L.tileLayer(
            'https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png',
            { attribution: '&copy; OpenStreetMap contributors' }
        ).addTo(map);

        const morongData = {
            "type": "Feature",
            "properties": { "name": "Morong", "risk": 1 },
            "geometry": {
                
            }
        };

        const geojson = L.geoJSON(morongData, {
            style: {
                color: '#007bff',
                weight: 3,
                fillOpacity: 0.15
            }
        }).addTo(map);

        const bounds = geojson.getBounds();
        map.fitBounds(bounds);
        map.setMaxBounds(bounds);
        map.options.maxBoundsViscosity = 1.0;

        // ===== CHART =====
        new Chart(document.getElementById('casesChart'), {
            type: 'line',
            data: {
                labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'],
                datasets: [{
                    label: 'Monthly Cases',
                    data: [12, 19, 15, 17, 24, 23, 21, 18, 16, 14, 11, 9],
                    fill: true,
                    tension: 0.3,
                    borderColor: '#FF6B35',
                    backgroundColor: 'rgba(255,107,53,0.1)'
                }]
            },
            options: {
                responsive: true,
                plugins: { legend: { display: false } },
                scales: { y: { beginAtZero: true } }
            }
        });

        // ===== SAMPLE DATA =====
        caseValue.textContent = '24';
        monthlyCaseValue.textContent = '189';
        highRiskCaseValue.textContent = '7';
        treatmentCountValue.textContent = '43';
        completedCaseValue.textContent = '156';
        stockValue.textContent = '3';

    });

/* END EXTERNAL SOURCE */
