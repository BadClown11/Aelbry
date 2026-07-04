window.Reports = (function () {
    const STATUS_LABELS = { Pending: 'Pendiente', InProgress: 'En Progreso', Blocked: 'Bloqueada', Completed: 'Completada', Cancelled: 'Cancelada' };
    const PRIORITY_LABELS = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Critica' };
    const STATUS_BY_NUMBER = { 1: 'Pendiente', 2: 'En Progreso', 3: 'Bloqueada', 4: 'Completada', 5: 'Cancelada' };

    let progressChart = null;
    let priorityChart = null;
    let burndownChartInstance = null;

    function commonFilters() {
        const params = new URLSearchParams();
        params.set('companyId', document.getElementById('filterCompanyId').value || '0');

        const projectId = document.getElementById('filterProjectId').value;
        const userId = document.getElementById('filterUserId').value;
        const teamId = document.getElementById('filterTeamId').value;
        const departmentId = document.getElementById('filterDepartmentId').value;

        if (projectId) params.set('projectId', projectId);
        if (userId) params.set('userId', userId);
        if (teamId) params.set('teamId', teamId);
        if (departmentId) params.set('departmentId', departmentId);

        return params;
    }

    function mondayOfCurrentWeek() {
        const now = new Date();
        const day = now.getDay();
        const diff = (day === 0 ? -6 : 1) - day;
        now.setDate(now.getDate() + diff);
        return now.toISOString().substring(0, 10);
    }

    async function loadAll() {
        await Promise.all([loadWeekly(), loadKpis(), loadCharts()]);
    }

    async function loadWeekly() {
        const params = commonFilters();
        const start = document.getElementById('weeklyStartDate').value;
        const end = document.getElementById('weeklyEndDate').value;
        if (start) params.set('startDate', start);
        if (end) params.set('endDate', end);

        const result = await Aelbry.api.get(`/Report/GetWeeklyReport?${params.toString()}`);
        const rows = document.getElementById('weeklyRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((a) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${a.code}</td>
                <td>${a.name}</td>
                <td>${a.projectName}</td>
                <td>${a.responsibleName}</td>
                <td>${STATUS_LABELS[a.status] ?? a.status}</td>
                <td>${PRIORITY_LABELS[a.priority] ?? a.priority}</td>
                <td>${a.estimatedEndDate ? new Date(a.estimatedEndDate).toLocaleDateString() : ''}</td>
                <td>${a.daysElapsed ?? ''}</td>
                <td>${a.daysRemaining ?? ''}</td>
                <td>${a.isAtRisk ? '<span class="badge text-bg-danger">En riesgo</span>' : ''}</td>`;
            rows.appendChild(tr);
        });
    }

    async function loadKpis() {
        const params = commonFilters();
        params.set('weekStart', document.getElementById('kpiWeekStart').value || mondayOfCurrentWeek());

        const result = await Aelbry.api.get(`/Report/GetKpiSummary?${params.toString()}`);
        const container = document.getElementById('kpiCards');
        container.innerHTML = '';

        if (result.result !== 'OK') return;

        const k = result.data;
        const deltaClass = k.productivityDelta >= 0 ? 'text-success' : 'text-danger';
        const deltaSign = k.productivityDelta >= 0 ? '+' : '';

        const statusCardsHtml = Object.entries(k.countsByStatus).map(([status, count]) => `
            <div class="col-md-2">
                <div class="card text-center">
                    <div class="card-body">
                        <div class="fs-4 fw-bold">${count}</div>
                        <div class="small text-muted">${STATUS_LABELS[status] ?? status}</div>
                    </div>
                </div>
            </div>`).join('');

        container.innerHTML = `
            ${statusCardsHtml}
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <div class="fs-4 fw-bold">${k.completedThisWeek}/${k.dueThisWeek}</div>
                        <div class="small text-muted">Completadas / Programadas esta semana</div>
                    </div>
                </div>
            </div>
            <div class="col-md-3">
                <div class="card text-center">
                    <div class="card-body">
                        <div class="fs-4 fw-bold">${k.productivityThisWeek}%</div>
                        <div class="small ${deltaClass}">${deltaSign}${k.productivityDelta}% vs semana anterior</div>
                    </div>
                </div>
            </div>`;
    }

    async function loadCharts() {
        const params = commonFilters();

        const progressResult = await Aelbry.api.get(`/Report/GetProgressByUser?${params.toString()}`);
        if (progressResult.result === 'OK') {
            const labels = progressResult.data.map((p) => p.userName);
            const data = progressResult.data.map((p) => p.averageProgress);

            if (progressChart) progressChart.destroy();
            progressChart = new Chart(document.getElementById('progressByUserChart'), {
                type: 'bar',
                data: { labels, datasets: [{ label: '% Avance promedio', data, backgroundColor: '#4C6EF5' }] },
                options: { scales: { y: { beginAtZero: true, max: 100 } } },
            });
        }

        const priorityResult = await Aelbry.api.get(`/Report/GetPriorityDistribution?${params.toString()}`);
        if (priorityResult.result === 'OK') {
            const labels = priorityResult.data.map((p) => PRIORITY_LABELS[p.priority] ?? p.priority);
            const data = priorityResult.data.map((p) => p.count);

            if (priorityChart) priorityChart.destroy();
            priorityChart = new Chart(document.getElementById('priorityChart'), {
                type: 'doughnut',
                data: { labels, datasets: [{ data, backgroundColor: ['#868e96', '#4C6EF5', '#f59f00', '#e03131'] }] },
            });
        }
    }

    async function loadBurndown() {
        const companyId = document.getElementById('filterCompanyId').value;
        const projectId = document.getElementById('filterProjectId').value;
        const start = document.getElementById('burndownStartDate').value;
        const end = document.getElementById('burndownEndDate').value;

        if (!companyId || !projectId || !start || !end) {
            alert('Selecciona empresa, proyecto (en los filtros de arriba) y el rango de fechas.');
            return;
        }

        const result = await Aelbry.api.get(`/Report/GetBurndown?companyId=${companyId}&projectId=${projectId}&startDate=${start}&endDate=${end}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        const labels = result.data.map((p) => new Date(p.date).toLocaleDateString());
        const ideal = result.data.map((p) => p.idealRemainingHours);
        const actual = result.data.map((p) => p.actualRemainingHours);

        if (burndownChartInstance) burndownChartInstance.destroy();
        burndownChartInstance = new Chart(document.getElementById('burndownChart'), {
            type: 'line',
            data: {
                labels,
                datasets: [
                    { label: 'Ideal', data: ideal, borderColor: '#868e96', borderDash: [5, 5], fill: false },
                    { label: 'Real', data: actual, borderColor: '#e03131', fill: false },
                ],
            },
        });
    }

    async function exportReport(format) {
        const params = commonFilters();
        const start = document.getElementById('weeklyStartDate').value;
        const end = document.getElementById('weeklyEndDate').value;
        if (start) params.set('startDate', start);
        if (end) params.set('endDate', end);

        const extensions = { Excel: 'xlsx', Word: 'docx', Pdf: 'pdf' };
        await Aelbry.ui.downloadBlob(`/Report/Export${format}?${params.toString()}`, `ReporteSemanal.${extensions[format]}`);
    }

    document.addEventListener('DOMContentLoaded', () => {
        if (!document.getElementById('kpiWeekStart')) return;
        document.getElementById('kpiWeekStart').value = mondayOfCurrentWeek();
    });

    return { loadAll, loadWeekly, loadKpis, loadCharts, loadBurndown, exportReport };
})();
