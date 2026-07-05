window.CalendarViewPage = (function () {
    const STATUS_LABELS = { 1: 'Pendiente', 2: 'En Progreso', 3: 'Bloqueada', 4: 'Completada', 5: 'Cancelada' };

    let calendar = null;
    let activitiesById = {};

    function projectId() {
        return document.getElementById('filterProjectId').value;
    }

    function flatten(nodes, list) {
        nodes.forEach((n) => {
            list.push(n);
            flatten(n.children ?? [], list);
        });
        return list;
    }

    function addOneDay(dateStr) {
        const d = new Date(dateStr);
        d.setDate(d.getDate() + 1);
        return d.toISOString().substring(0, 10);
    }

    async function loadAll() {
        const pid = projectId();
        if (!pid) return;

        const treeResult = await Aelbry.api.get(`/Activity/GetTreeByProject?projectId=${pid}`);
        console.log('[Calendario] GetTreeByProject ->', treeResult);
        const activities = treeResult.result === 'OK' ? flatten(treeResult.data, []) : [];

        activitiesById = Object.fromEntries(activities.map((a) => [String(a.activityId), a]));

        const withDates = activities.filter((a) => a.estimatedStartDate || a.estimatedEndDate);
        const withoutDatesCount = activities.length - withDates.length;
        document.getElementById('noDateCount').textContent = withoutDatesCount > 0
            ? `${withoutDatesCount} actividad(es) sin fecha estimada no se muestran en el calendario.`
            : '';

        const events = withDates.map((a) => {
            const start = (a.estimatedStartDate ?? a.estimatedEndDate).substring(0, 10);
            const end = a.estimatedEndDate ? addOneDay(a.estimatedEndDate.substring(0, 10)) : addOneDay(start);

            return {
                id: String(a.activityId),
                title: `${a.code} - ${a.name}`,
                start,
                end,
                color: a.colorHex,
            };
        });

        if (calendar) {
            calendar.destroy();
        }

        calendar = new FullCalendar.Calendar(document.getElementById('calendar'), {
            initialView: 'dayGridMonth',
            headerToolbar: {
                left: 'prev,next today',
                center: 'title',
                right: 'dayGridMonth,timeGridWeek,timeGridDay,listWeek',
            },
            events,
            eventClick: (info) => showDetails(info.event.id),
        });
        calendar.render();

        document.getElementById('eventDetails').classList.add('d-none');
    }

    function showDetails(activityId) {
        const a = activitiesById[activityId];
        if (!a) return;

        document.getElementById('eventDetailsBody').innerHTML = `
            <h6>${a.code} - ${a.name}</h6>
            <div>${a.description ?? ''}</div>
            <div class="small text-muted mt-2">
                Estado: ${STATUS_LABELS[a.status] ?? a.status} &middot;
                Responsable: ${a.responsibleName ?? ''} &middot;
                Avance: ${a.progressPercentage ?? 0}%
            </div>`;
        document.getElementById('eventDetails').classList.remove('d-none');
    }

    Aelbry.projectContext.autoLoad(loadAll);

    return { loadAll };
})();
