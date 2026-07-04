window.Kanban = (function () {
    const STATUSES = [
        { value: 1, label: 'Pendiente' },
        { value: 2, label: 'En Progreso' },
        { value: 3, label: 'Bloqueada' },
        { value: 4, label: 'Completada' },
        { value: 5, label: 'Cancelada' },
    ];

    let allActivities = [];
    let activityTags = {};
    let sortables = [];

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

    async function loadAll() {
        const pid = projectId();
        if (!pid) return;

        const projectResult = await Aelbry.api.get(`/Project/GetById?projectId=${pid}`);
        const tagSelect = document.getElementById('filterTagId');
        tagSelect.innerHTML = '<option value="">Todas</option>';

        if (projectResult.result === 'OK') {
            const tagsResult = await Aelbry.api.get(`/Tag/GetByCompany?companyId=${projectResult.data.companyId}`);
            if (tagsResult.result === 'OK') {
                tagsResult.data.forEach((t) => {
                    tagSelect.innerHTML += `<option value="${t.tagId}">${t.name}</option>`;
                });
            }
        }
        Aelbry.ui.initSelect2(tagSelect);

        const treeResult = await Aelbry.api.get(`/Activity/GetTreeByProject?projectId=${pid}`);
        allActivities = treeResult.result === 'OK' ? flatten(treeResult.data, []) : [];

        const tagEntries = await Promise.all(allActivities.map(async (a) => {
            const r = await Aelbry.api.get(`/Activity/GetTags?activityId=${a.activityId}`);
            return [a.activityId, r.result === 'OK' ? r.data.map((t) => t.tagId) : []];
        }));
        activityTags = Object.fromEntries(tagEntries);

        render();
    }

    function clearFilters() {
        document.getElementById('filterUserId').value = '';
        document.getElementById('filterTagId').value = '';
        document.getElementById('filterPriority').value = '';
        render();
    }

    function matchesFilters(activity) {
        const userId = document.getElementById('filterUserId').value;
        const tagId = document.getElementById('filterTagId').value;
        const priority = document.getElementById('filterPriority').value;

        if (userId && activity.responsibleUserId !== parseInt(userId, 10)) return false;
        if (priority && activity.priority !== parseInt(priority, 10)) return false;
        if (tagId && !(activityTags[activity.activityId] ?? []).includes(parseInt(tagId, 10))) return false;

        return true;
    }

    function render() {
        const board = document.getElementById('kanbanBoard');
        board.innerHTML = '';
        sortables.forEach((s) => s.destroy());
        sortables = [];

        const filtered = allActivities.filter(matchesFilters);

        STATUSES.forEach((status) => {
            const column = document.createElement('div');
            column.className = 'card';
            column.style.minWidth = '260px';
            column.style.width = '260px';

            const cardsInColumn = filtered.filter((a) => a.status === status.value);

            column.innerHTML = `
                <div class="card-header fw-semibold">${status.label} <span class="badge text-bg-secondary">${cardsInColumn.length}</span></div>
                <div class="card-body p-2 kanban-column" data-status="${status.value}" style="min-height: 400px;"></div>`;

            board.appendChild(column);

            const columnBody = column.querySelector('.kanban-column');
            cardsInColumn.forEach((a) => columnBody.appendChild(buildCard(a)));

            const sortable = Sortable.create(columnBody, {
                group: 'kanban',
                animation: 150,
                onEnd: async (evt) => {
                    const activityId = parseInt(evt.item.dataset.activityId, 10);
                    const newStatus = parseInt(evt.to.dataset.status, 10);
                    const result = await Aelbry.api.post(`/Activity/UpdateStatus?activityId=${activityId}&status=${newStatus}`);
                    if (result.result === 'OK') {
                        const activity = allActivities.find((a) => a.activityId === activityId);
                        if (activity) activity.status = newStatus;
                    } else {
                        alert(result.result);
                        render();
                    }
                },
            });
            sortables.push(sortable);
        });
    }

    function buildCard(a) {
        const card = document.createElement('div');
        card.className = 'card mb-2 shadow-sm';
        card.dataset.activityId = a.activityId;
        card.style.cursor = 'grab';
        card.style.borderLeft = `4px solid ${a.colorHex}`;
        card.innerHTML = `
            <div class="card-body p-2">
                <div class="small text-muted">${a.code}</div>
                <div class="fw-semibold">${a.name}</div>
                <div class="small text-muted">${a.responsibleName ?? ''}</div>
                <div class="progress mt-1" style="height: 4px;">
                    <div class="progress-bar" style="width: ${a.progressPercentage ?? 0}%"></div>
                </div>
            </div>`;
        return card;
    }

    return { loadAll, render, clearFilters };
})();
