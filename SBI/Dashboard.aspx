<%@ Page Language="C#" MasterPageFile="~/Admin.master" AutoEventWireup="true" CodeBehind="Dashboard.aspx.cs" Inherits="SBI.Dashboard" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <title>Dashboard — SBI Medical</title>
    <link rel="stylesheet" href="https://unpkg.com/leaflet@1.9.4/dist/leaflet.css" />
    <script src="https://unpkg.com/leaflet@1.9.4/dist/leaflet.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>#map { height: 520px; width: 100%; border-radius: 12px; }</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

    <div class="p-6 font-heading2 text-slate-900 space-y-6">

        <%-- ── Page Header ───────────────────────────────────────────── --%>
        <div class="flex justify-between items-start">
            <div>
                <h1 class="text-4xl text-[#0b2a7a] font-hBruns tracking-widest">Dashboard</h1>
                <p class="text-slate-500 text-sm mt-1">
                    SBI Medical — Morong Branch &nbsp;·&nbsp;
                    <span id="today-date" class="font-medium text-slate-600"></span>
                </p>
            </div>
            <div class="flex gap-2">
                <button class="bg-white border border-slate-300 text-slate-600 px-4 py-2 rounded-lg text-sm font-bold cursor-pointer hover:bg-slate-50 transition">
                    Date Range
                </button>
                <button class="h-11 rounded-lg bg-blue-600 px-6 font-bold text-white shadow hover:bg-blue-700 transition cursor-pointer">
                    Generate Report
                </button>
            </div>
        </div>

        <%-- ── Stat Cards ─────────────────────────────────────────────── --%>
        <div class="grid grid-cols-2 md:grid-cols-3 xl:grid-cols-6 gap-4">
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Cases Today</div>
                <div class="text-3xl font-extrabold text-slate-800" id="caseValue">0</div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Monthly Cases</div>
                <div class="text-3xl font-extrabold text-indigo-600" id="monthlyCaseValue">0</div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">High-Risk Cases</div>
                <div class="text-3xl font-extrabold text-amber-500" id="highRiskCaseValue">0</div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Ongoing Treatments</div>
                <div class="text-3xl font-extrabold text-blue-600" id="treatmentCountValue">0</div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Completed Cases</div>
                <div class="text-3xl font-extrabold text-emerald-600" id="completedCaseValue">0</div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
                <div class="text-xs font-bold text-slate-400 uppercase tracking-widest mb-1">Stock Alert</div>
                <div class="text-3xl font-extrabold text-red-600" id="stockValue">0</div>
            </div>
        </div>

        <%-- ── Map ───────────────────────────────────────────────────── --%>
        <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                <h3 class="font-extrabold text-slate-800">Barangay Case Map — Morong</h3>
                <p class="text-slate-500 text-sm mt-1">Choropleth showing bite case density per barangay</p>
            </div>
            <div class="p-4"><div id="map"></div></div>
        </div>

        <%-- ── Charts Row 1 ──────────────────────────────────────────── --%>
        <div class="grid lg:grid-cols-2 gap-4">
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Monthly Cases Overview</h3>
                    <p class="text-slate-500 text-sm mt-1">Number of bite cases recorded per month</p>
                </div>
                <div class="p-5 h-[260px]"><canvas id="casesChart"></canvas></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Vaccine Usage Trend (Weekly)</h3>
                    <p class="text-slate-500 text-sm mt-1">Doses administered per week this month</p>
                </div>
                <div class="p-5 h-[260px]"><canvas id="vaccineChart"></canvas></div>
            </div>
        </div>

        <%-- ── Charts Row 2 ──────────────────────────────────────────── --%>
        <div class="grid lg:grid-cols-2 gap-4">
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Cases by Category</h3>
                    <p class="text-slate-500 text-sm mt-1">Distribution of bite cases by WHO exposure category</p>
                </div>
                <div class="p-5 h-[260px] flex items-center justify-center"><canvas id="categoryChart"></canvas></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Cases by Animal Type</h3>
                    <p class="text-slate-500 text-sm mt-1">Breakdown of biting animal per recorded case</p>
                </div>
                <div class="p-5 h-[260px] flex items-center justify-center"><canvas id="animalChart"></canvas></div>
            </div>
        </div>

        <%-- ── Charts Row 3 ──────────────────────────────────────────── --%>
        <div class="grid lg:grid-cols-2 gap-4">
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Cases by Exposure Type</h3>
                    <p class="text-slate-500 text-sm mt-1">Bite vs. Non-bite / Play bite exposures</p>
                </div>
                <div class="p-5 h-[260px] flex items-center justify-center"><canvas id="exposureChart"></canvas></div>
            </div>
            <div class="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
                <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                    <h3 class="font-extrabold text-slate-800">Cases by Wound Type</h3>
                    <p class="text-slate-500 text-sm mt-1">Type of wound recorded across all bite cases</p>
                </div>
                <div class="p-5 h-[260px]"><canvas id="woundChart"></canvas></div>
            </div>
        </div>

        <%-- ── Previous Cases Table ──────────────────────────────────── --%>
        <asp:Panel runat="server" CssClass="bg-white border border-slate-200 rounded-xl shadow-sm overflow-hidden">
            <div class="px-5 py-4 border-b border-slate-200 bg-slate-50">
                <h3 class="font-extrabold text-slate-800">Previous Cases</h3>
                <p class="text-slate-500 text-sm mt-1">Recently recorded bite cases</p>
            </div>
            <asp:GridView ID="gvPreviousCases" runat="server"
                CssClass="w-full text-sm" GridLines="None"
                AutoGenerateColumns="true">
                <HeaderStyle CssClass="text-left bg-slate-50 text-slate-500 border-b border-slate-200 uppercase text-xs font-bold" />
                <RowStyle CssClass="border-b border-slate-100 transition-colors" />
                <EmptyDataTemplate>
                    <div class="p-10 text-center text-slate-400 text-sm">No previous cases on record.</div>
                </EmptyDataTemplate>
            </asp:GridView>
        </asp:Panel>

    </div>

    <script src="MapContent/morong.js"></script>
    <script>
        document.addEventListener("DOMContentLoaded", function () {
            const mapDiv = document.getElementById("map");
            if (!mapDiv || mapDiv.offsetHeight === 0 || typeof morongData === "undefined") return;

            const map = L.map("map", { zoomControl: true, maxZoom: 18 });
            L.tileLayer("https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png", { attribution: "&copy; OpenStreetMap contributors" }).addTo(map);

            function utmToLatLon(easting, northing, zone = 51) {
                const a=6378137.0,f=1/298.257223563,b=a*(1-f),e2=1-(b/a)**2,k0=0.9996,E0=500000,x=easting-E0,y=northing;
                const M=y/k0,mu=M/(a*(1-e2/4-3*e2**2/64-5*e2**3/256));
                const e1=(1-Math.sqrt(1-e2))/(1+Math.sqrt(1-e2));
                let p=mu+(3*e1/2-27*e1**3/32)*Math.sin(2*mu)+(21*e1**2/16-55*e1**4/32)*Math.sin(4*mu)+(151*e1**3/96)*Math.sin(6*mu)+(1097*e1**4/512)*Math.sin(8*mu);
                const N1=a/Math.sqrt(1-e2*Math.sin(p)**2),T1=Math.tan(p)**2,C1=e2*Math.cos(p)**2/(1-e2),R1=a*(1-e2)/(1-e2*Math.sin(p)**2)**1.5,D=x/(N1*k0);
                const lat=p-(N1*Math.tan(p)/R1)*(D**2/2-(5+3*T1+10*C1-4*C1**2-9*e2)*D**4/24+(61+90*T1+298*C1+45*T1**2-252*e2-3*C1**2)*D**6/720);
                const lon0=((zone-1)*6-180+3)*Math.PI/180,lon=lon0+(D-(1+2*T1+C1)*D**3/6+(5-2*C1+28*T1-3*C1**2+8*e2+24*T1**2)*D**5/120)/Math.cos(p);
                return [lat*180/Math.PI,lon*180/Math.PI];
            }

            const geoData = { ...morongData, features: morongData.features.map(f => ({ ...f, geometry: { ...f.geometry,
                coordinates: f.geometry.coordinates.map(ring => ring.map(([e,n]) => { const [lat,lon]=utmToLatLon(e,n); return [lon,lat]; }))
            }}))};

            function getColor(c) { return c>=30?'#dc2626':c>=20?'#f97316':c>=10?'#ffce00':c>=1?'#eab308':'#22c55e'; }

            const legend = L.control({ position: 'bottomright' });
            legend.onAdd = function() {
                const div = L.DomUtil.create('div');
                div.style.cssText = 'background:white;padding:12px;border-radius:8px;box-shadow:0 0 15px rgba(0,0,0,0.2);line-height:24px;min-width:180px;font-family:Arial,sans-serif;';
                div.innerHTML = '<h4 style="margin:0 0 8px;font-weight:bold;border-bottom:1px solid #ddd;padding-bottom:5px;color:#333;">Cases per Barangay</h4>'
                    + [['#dc2626','30+ cases'],['#f97316','20–29'],['#ffce00','10–19'],['#eab308','1–9'],['#22c55e','No cases']]
                    .map(([c,l])=>`<div style="display:flex;align-items:center;margin-bottom:6px;"><div style="width:18px;height:18px;background:${c};margin-right:8px;border:1px solid #999;border-radius:2px;"></div><span style="font-size:13px;">${l}</span></div>`).join('');
                return div;
            };
            legend.addTo(map);

            function loadMap(caseData) {
                const caseMap = {};
                if (caseData && caseData.Barangays) for (let i=0;i<caseData.Barangays.length;i++) caseMap[caseData.Barangays[i].toUpperCase().trim()]=caseData.CaseCounts[i];
                if (window.currentLayer) map.removeLayer(window.currentLayer);
                window.currentLayer = L.geoJSON(geoData, {
                    style: f => { const c=caseMap[(f?.properties?.adm4_en||'').toUpperCase().trim()]||0; return {color:'#111827',weight:1,fillColor:getColor(c),fillOpacity:0.7}; },
                    onEachFeature: (f, layer) => {
                        const n=f?.properties?.adm4_en||'Unknown', c=caseMap[n.toUpperCase().trim()]||0;
                        layer.bindPopup(`<b>${n}</b><br/><b style="color:${getColor(c)}">Cases: ${c}</b>`);
                        layer.on({ mouseover: e=>{e.target.setStyle({weight:3,color:'#1e3a8a',fillOpacity:0.9});e.target.bringToFront();}, mouseout: e=>window.currentLayer.resetStyle(e.target) });
                    }
                }).addTo(map);
                map.fitBounds(window.currentLayer.getBounds(), {padding:[20,20]});
                if (caseData && caseData.CaseCounts) document.getElementById('caseValue').textContent = caseData.CaseCounts.reduce((a,b)=>a+b,0);
            }

            const tmp = L.geoJSON(geoData).addTo(map);
            const bnds = tmp.getBounds(); map.fitBounds(bnds,{padding:[20,20]}); map.setMaxBounds(bnds); map.options.minZoom=map.getBoundsZoom(bnds); map.removeLayer(tmp);

            const api = ep => fetch('Dashboard.aspx/'+ep,{method:'POST',headers:{'Content-Type':'application/json'}}).then(r=>r.json()).then(r=>r.d);

            api('GetBarangayCases').then(d=>{ if(d&&!d.Error) loadMap(d); }).catch(()=>{});

            // Charts
            const casesChart = new Chart(document.getElementById('casesChart'),{type:'line',data:{labels:['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'],datasets:[{label:'Cases',data:[0,0,0,0,0,0,0,0,0,0,0,0],borderColor:'#FF6B35',backgroundColor:'rgba(255,107,53,0.1)',fill:true,tension:0.4,pointBackgroundColor:'#FF6B35',pointRadius:4}]},options:{responsive:true,maintainAspectRatio:false,plugins:{legend:{display:false}},scales:{y:{beginAtZero:true,ticks:{precision:0}}}}});
            api('GetMonthlyCases').then(d=>{ if(d&&d.Months){ casesChart.data.labels=d.Months; casesChart.data.datasets[0].data=d.Counts; casesChart.update(); document.getElementById('monthlyCaseValue').textContent=d.Counts[new Date().getMonth()]||0; }}).catch(()=>{});

            const vaccineChart = new Chart(document.getElementById('vaccineChart'),{type:'bar',data:{labels:['Week 1','Week 2','Week 3','Week 4'],datasets:[{label:'Doses',data:[0,0,0,0],backgroundColor:'#2563eb',borderRadius:6}]},options:{responsive:true,maintainAspectRatio:false,plugins:{legend:{display:false}},scales:{y:{beginAtZero:true,ticks:{precision:0}}}}});
            api('GetWeeklyVaccineUsage').then(d=>{ if(d&&d.Weeks){ vaccineChart.data.labels=d.Weeks; vaccineChart.data.datasets[0].data=d.Counts; vaccineChart.update(); }}).catch(()=>{});

            const donutOpts = (pos='right') => ({responsive:true,maintainAspectRatio:false,plugins:{legend:{position:pos,labels:{font:{size:12},padding:16}}},cutout:'60%'});
            const categoryChart = new Chart(document.getElementById('categoryChart'),{type:'doughnut',data:{labels:['Category I','Category II','Category III'],datasets:[{data:[0,0,0],backgroundColor:['#22c55e','#f59e0b','#ef4444'],borderWidth:2,borderColor:'#fff'}]},options:donutOpts()});
            api('GetCasesByCategory').then(d=>{ if(d&&d.Labels){ categoryChart.data.labels=d.Labels; categoryChart.data.datasets[0].data=d.Counts; categoryChart.update(); document.getElementById('highRiskCaseValue').textContent=d.Counts[d.Labels.indexOf('Category III')]||0; }}).catch(()=>{});

            const animalChart = new Chart(document.getElementById('animalChart'),{type:'doughnut',data:{labels:['Dog','Cat','Others'],datasets:[{data:[0,0,0],backgroundColor:['#3b82f6','#f97316','#a855f7'],borderWidth:2,borderColor:'#fff'}]},options:donutOpts()});
            api('GetCasesByAnimalType').then(d=>{ if(d&&d.Labels){ animalChart.data.labels=d.Labels; animalChart.data.datasets[0].data=d.Counts; animalChart.update(); }}).catch(()=>{});

            const exposureChart = new Chart(document.getElementById('exposureChart'),{type:'doughnut',data:{labels:['Bite','Non-Bite / Play Bite'],datasets:[{data:[0,0],backgroundColor:['#ef4444','#64748b'],borderWidth:2,borderColor:'#fff'}]},options:donutOpts()});
            api('GetCasesByExposureType').then(d=>{ if(d&&d.Labels){ exposureChart.data.labels=d.Labels; exposureChart.data.datasets[0].data=d.Counts; exposureChart.update(); }}).catch(()=>{});

            const woundChart = new Chart(document.getElementById('woundChart'),{type:'bar',data:{labels:['Lacerated','Avulsion','Punctured','Abrasion','Scratches','Hematoma'],datasets:[{label:'Cases',data:[0,0,0,0,0,0],backgroundColor:'#0ea5e9',borderRadius:4}]},options:{indexAxis:'y',responsive:true,maintainAspectRatio:false,plugins:{legend:{display:false}},scales:{x:{beginAtZero:true,ticks:{precision:0}}}}});
            api('GetCasesByWoundType').then(d=>{ if(d&&d.Labels){ woundChart.data.labels=d.Labels; woundChart.data.datasets[0].data=d.Counts; woundChart.update(); }}).catch(()=>{});

            api('GetDashboardStats').then(d=>{ if(d){ if(d.OngoingTreatments!=null) document.getElementById('treatmentCountValue').textContent=d.OngoingTreatments; if(d.CompletedCases!=null) document.getElementById('completedCaseValue').textContent=d.CompletedCases; if(d.StockAlerts!=null) document.getElementById('stockValue').textContent=d.StockAlerts; }}).catch(()=>{});

            document.getElementById('today-date').textContent = new Date().toLocaleDateString('en-US',{year:'numeric',month:'long',day:'numeric'});
        });
    </script>

</asp:Content>
