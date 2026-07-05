window.Home = (function () {
    // GetWeeklyReport serializa Status/Priority como numero; GetKpiSummary (CountsByStatus,
    // Dictionary<ActivityStatus,int>) los serializa como el nombre del enum en texto. Se
    // manejan los dos formatos por separado para no adivinar cual usa cada endpoint.
    const STATUS_LABELS_BY_NUMBER = { 1: 'Pendiente', 2: 'En Progreso', 3: 'Bloqueada', 4: 'Completada', 5: 'Cancelada' };
    const STATUS_BADGE_BY_NUMBER = { 1: 'secondary', 2: 'primary', 3: 'warning', 4: 'success', 5: 'danger' };
    const STATUS_LABELS_BY_NAME = { Pending: 'Pendiente', InProgress: 'En Progreso', Blocked: 'Bloqueada', Completed: 'Completada', Cancelled: 'Cancelada' };
    const PRIORITY_LABELS = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Critica' };

    let isGlobalView = false;

    function toIsoDate(date) {
        return date.toISOString().substring(0, 10);
    }

    function initDefaults() {
        const user = Aelbry.api.getCurrentUser();
        isGlobalView = Aelbry.api.hasPermission('REPORTS_VIEW');

        document.getElementById('homeGreeting').textContent = `Hola, ${user?.fullName || ''}`;
        document.getElementById('homeSubtitle').textContent = isGlobalView
            ? 'Actividades de toda la empresa (proximas y recientes)'
            : 'Tus actividades asignadas (proximas y recientes)';
        document.getElementById('homeResponsibleHeader').classList.toggle('d-none', !isGlobalView);
        document.getElementById('homeAdvancedFilters').classList.toggle('d-none', !isGlobalView);

        const today = new Date();
        const start = new Date(today);
        start.setDate(start.getDate() - 7);
        const end = new Date(today);
        end.setDate(end.getDate() + 30);

        document.getElementById('homeStartDate').value = toIsoDate(start);
        document.getElementById('homeEndDate').value = toIsoDate(end);
    }

    function buildParams() {
        const user = Aelbry.api.getCurrentUser();
        const params = new URLSearchParams();
        params.set('startDate', document.getElementById('homeStartDate').value);
        params.set('endDate', document.getElementById('homeEndDate').value);

        if (isGlobalView) {
            params.set('companyId', user?.companyId ?? '0');
            const teamId = document.getElementById('homeFilterTeamId').value;
            const deptId = document.getElementById('homeFilterDeptId').value;
            const userId = document.getElementById('homeFilterUserId').value;
            if (teamId) params.set('teamId', teamId);
            if (deptId) params.set('departmentId', deptId);
            if (userId) params.set('userId', userId);
        }

        return params;
    }

    async function load() {
        const params = buildParams();

        // Sin REPORTS_VIEW (Empleado/Invitado) el endpoint general de Reportes esta bloqueado
        // por permisos; se usa GetMyActivities, que solo exige ACTIVITIES_VIEW y siempre
        // devuelve las propias, sin exponer KPIs agregados de toda la empresa.
        if (!isGlobalView) {
            const weeklyResult = await Aelbry.api.get(`/Report/GetMyActivities?${params.toString()}`);
            renderRows(weeklyResult.result === 'OK' ? weeklyResult.data : []);
            document.getElementById('homeKpiRow').innerHTML = '';
            return;
        }

        const [weeklyResult, kpiResult] = await Promise.all([
            Aelbry.api.get(`/Report/GetWeeklyReport?${params.toString()}`),
            Aelbry.api.get(`/Report/GetKpiSummary?${params.toString()}&weekStart=${mondayOfCurrentWeek()}`),
        ]);

        renderRows(weeklyResult.result === 'OK' ? weeklyResult.data : []);
        renderKpis(kpiResult.result === 'OK' ? kpiResult.data : null);
    }

    function mondayOfCurrentWeek() {
        const now = new Date();
        const day = now.getDay();
        const diff = (day === 0 ? -6 : 1) - day;
        now.setDate(now.getDate() + diff);
        return now.toISOString().substring(0, 10);
    }

    function renderKpis(kpi) {
        const container = document.getElementById('homeKpiRow');
        container.innerHTML = '';
        if (!kpi) return;

        Object.entries(kpi.countsByStatus).forEach(([status, count]) => {
            container.innerHTML += `
                <div class="col-6 col-md-2">
                    <div class="card text-center">
                        <div class="card-body py-2">
                            <div class="fs-5 fw-bold">${count}</div>
                            <div class="small text-muted">${STATUS_LABELS_BY_NAME[status] ?? status}</div>
                        </div>
                    </div>
                </div>`;
        });

        container.innerHTML += `
            <div class="col-6 col-md-3">
                <div class="card text-center">
                    <div class="card-body py-2">
                        <div class="fs-5 fw-bold">${kpi.completedThisWeek}/${kpi.dueThisWeek}</div>
                        <div class="small text-muted">Completadas / Programadas esta semana</div>
                    </div>
                </div>
            </div>`;
    }

    function renderRows(rows) {
        const tbody = document.getElementById('homeRows');
        tbody.innerHTML = '';

        document.getElementById('homeEmptyState').classList.toggle('d-none', rows.length > 0);

        const sorted = [...rows].sort((a, b) => {
            const da = a.estimatedEndDate ? new Date(a.estimatedEndDate) : new Date(8640000000000000);
            const db = b.estimatedEndDate ? new Date(b.estimatedEndDate) : new Date(8640000000000000);
            return da - db;
        });

        sorted.forEach((a) => {
            const tr = document.createElement('tr');
            tr.style.cursor = 'pointer';
            tr.title = 'Entrar al proyecto';
            if (a.isAtRisk) tr.classList.add('table-danger');
            tr.addEventListener('click', () => enterProject(a.projectId, a.projectName));

            tr.innerHTML = `
                <td>${a.estimatedEndDate ? new Date(a.estimatedEndDate).toLocaleDateString() : 'Sin fecha'}</td>
                <td>${a.code}</td>
                <td>${a.name}</td>
                <td>${a.projectName}</td>
                <td class="${isGlobalView ? '' : 'd-none'}">${a.responsibleName}</td>
                <td><span class="badge text-bg-${STATUS_BADGE_BY_NUMBER[a.status] ?? 'secondary'}">${STATUS_LABELS_BY_NUMBER[a.status] ?? a.status}</span></td>
                <td>${PRIORITY_LABELS[a.priority] ?? a.priority}</td>
                <td>${a.progressPercentage ?? 0}%</td>`;
            tbody.appendChild(tr);
        });
    }

    // Al elegir un proyecto (desde una fila del feed) se marca como "activo" para que el
    // sidebar se revele (ver _Sidebar.cshtml -> applyProjectGate) y se entra a su tablero.
    function enterProject(projectId, projectName) {
        localStorage.setItem('aelbry_active_project_id', projectId);
        localStorage.setItem('aelbry_active_project_name', projectName);
        window.location.href = `/Kanban/Index?projectId=${projectId}`;
    }

    document.addEventListener('DOMContentLoaded', () => {
        if (!document.getElementById('homeGreeting')) return;
        initDefaults();
        load();
    });

    return { load, enterProject };
})();
