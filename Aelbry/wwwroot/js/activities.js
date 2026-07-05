window.Activities = (function () {
    const STATUS_LABELS = { 1: 'Pendiente', 2: 'En Progreso', 3: 'Bloqueada', 4: 'Completada', 5: 'Cancelada' };
    const STATUS_BADGES = { 1: 'text-bg-secondary', 2: 'text-bg-primary', 3: 'text-bg-danger', 4: 'text-bg-success', 5: 'text-bg-dark' };
    const PRIORITY_LABELS = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Critica' };
    const PRIORITY_BADGES = { 1: 'text-bg-secondary', 2: 'text-bg-info', 3: 'text-bg-warning', 4: 'text-bg-danger' };
    const DEPENDENCY_LABELS = { 1: 'Fin a Inicio', 2: 'Inicio a Inicio', 3: 'Fin a Fin', 4: 'Inicio a Fin' };
    const AVATAR_PALETTE = ['#4C6EF5', '#2F9E44', '#F59F00', '#7048E8', '#E03131', '#12B886', '#E8590C', '#1971C2'];

    let activityModal, checklistModal, dependenciesModal, participantsModal, activityTagsModal, timeModal;
    let companyId = null;
    let companyTags = [];
    let companyUsers = [];
    let companyCategories = [];
    let myTeamId = null;
    let myIsEmpleado = false;
    let lastTreeData = [];
    const collapsedIds = new Set();

    function defaultDates() {
        const start = new Date();
        const end = new Date();
        end.setDate(end.getDate() + 7);
        return { start: start.toISOString().substring(0, 10), end: end.toISOString().substring(0, 10) };
    }

    function avatarColor(name) {
        let hash = 0;
        for (let i = 0; i < (name ?? '').length; i++) {
            hash = (hash * 31 + name.charCodeAt(i)) % AVATAR_PALETTE.length;
        }
        return AVATAR_PALETTE[Math.abs(hash)];
    }

    function initials(name) {
        return (name ?? '')
            .split(' ')
            .filter(Boolean)
            .slice(0, 2)
            .map((p) => p[0].toUpperCase())
            .join('');
    }

    function projectId() {
        return document.getElementById('filterProjectId').value;
    }

    function populateUserSelect(select, placeholder, list) {
        const users = list ?? companyUsers;
        const previousValue = select.value;
        select.innerHTML = `<option value="">${placeholder}</option>`;
        users.forEach((u) => {
            const opt = document.createElement('option');
            opt.value = u.userId;
            opt.textContent = `${u.firstName} ${u.lastName}`;
            select.appendChild(opt);
        });
        select.value = previousValue;
        Aelbry.ui.initSelect2(select);
    }

    // Select2 no refleja los cambios hechos con select.value = x directamente:
    // hay que disparar 'change' en jQuery para que la caja visible se actualice.
    function setSelectValue(select, value) {
        select.value = value ?? '';
        if (window.jQuery) jQuery(select).trigger('change');
    }

    // La categoria sigue siendo un texto libre en Activity.Category (sin cambios
    // de esquema/DAL), pero las opciones del select vienen de ActivityCategory
    // (catalogo por empresa) en vez de que cada quien la escriba a mano distinto
    // cada vez ("Backend", "backend", "Back-end", ...).
    async function loadCategories() {
        const result = await Aelbry.api.get(`/ActivityCategory/GetByCompany?companyId=${companyId}`);
        companyCategories = result.result === 'OK' ? result.data : [];
    }

    function populateCategorySelect(select, extraValue) {
        const previousValue = select.value;
        select.innerHTML = '<option value="">-- Sin categoria --</option>';
        const names = companyCategories.map((c) => c.name);
        if (extraValue && !names.includes(extraValue)) names.push(extraValue);
        names.forEach((name) => {
            const opt = document.createElement('option');
            opt.value = name;
            opt.textContent = name;
            select.appendChild(opt);
        });
        select.value = previousValue;
        Aelbry.ui.initSelect2(select);
    }

    function populateResponsibleFilter() {
        const select = document.getElementById('filterResponsibleId');
        const previousValue = select.value;
        select.innerHTML = '<option value="">Todos los responsables</option>';
        companyUsers.forEach((u) => {
            const opt = document.createElement('option');
            opt.value = u.userId;
            opt.textContent = `${u.firstName} ${u.lastName}`;
            select.appendChild(opt);
        });
        select.value = previousValue;
        Aelbry.ui.initSelect2(select);
    }

    async function promptNewCategory() {
        const name = (prompt('Nombre de la nueva categoria:') ?? '').trim();
        if (!name) return;

        const result = await Aelbry.api.post('/ActivityCategory/Create', { companyId, name });
        if (result.result !== 'OK') { alert(result.result); return; }

        await loadCategories();
        const select = document.getElementById('activityCategory');
        populateCategorySelect(select);
        setSelectValue(select, name);
    }

    async function loadAll() {
        const pid = projectId();
        if (!pid) return;

        const projectResult = await Aelbry.api.get(`/Project/GetById?projectId=${pid}`);
        if (projectResult.result === 'OK') {
            companyId = projectResult.data.companyId;
            const tagsResult = await Aelbry.api.get(`/Tag/GetByCompany?companyId=${companyId}`);
            companyTags = tagsResult.result === 'OK' ? tagsResult.data : [];
            const usersResult = await Aelbry.api.get(`/User/GetByCompany?companyId=${companyId}`);
            companyUsers = usersResult.result === 'OK' ? usersResult.data : [];
            await loadCategories();
            populateResponsibleFilter();
        }

        const me = Aelbry.api.getCurrentUser();
        if (me) {
            myIsEmpleado = (me.roles ?? []).includes('Empleado');
            const meResult = await Aelbry.api.get(`/User/GetById?userId=${me.userId}`);
            myTeamId = meResult.result === 'OK' ? meResult.data.teamId : null;
        }

        const result = await Aelbry.api.get(`/Activity/GetTreeByProject?projectId=${pid}`);
        console.log('[Actividades] GetTreeByProject ->', result);

        if (result.result !== 'OK') { lastTreeData = []; renderTree(); return; }

        lastTreeData = result.data;
        renderTree();
    }

    function nodeMatchesFilter(node, nameQuery, responsibleId) {
        const nameMatch = !nameQuery || (node.name ?? '').toLowerCase().includes(nameQuery);
        const responsibleMatch = !responsibleId || String(node.responsibleUserId ?? '') === responsibleId;
        return nameMatch && responsibleMatch;
    }

    // Si una subactividad calza con el filtro pero su padre no, igual se
    // muestra el padre (para no perder el contexto de la jerarquia).
    function filterTree(nodes, nameQuery, responsibleId) {
        return nodes.reduce((acc, node) => {
            const filteredChildren = filterTree(node.children ?? [], nameQuery, responsibleId);
            if (nodeMatchesFilter(node, nameQuery, responsibleId) || filteredChildren.length > 0) {
                acc.push({ ...node, children: filteredChildren });
            }
            return acc;
        }, []);
    }

    function applyFilters() {
        renderTree();
    }

    function renderTree() {
        const rows = document.getElementById('activityRows');
        rows.innerHTML = '';

        const nameQuery = (document.getElementById('filterActivityName')?.value ?? '').trim().toLowerCase();
        const responsibleId = document.getElementById('filterResponsibleId')?.value ?? '';
        const data = (nameQuery || responsibleId) ? filterTree(lastTreeData, nameQuery, responsibleId) : lastTreeData;

        data.forEach((a, index) => renderActivityRow(rows, a, 0, `${index + 1}`));
    }

    function toggleCollapse(activityId) {
        if (collapsedIds.has(activityId)) {
            collapsedIds.delete(activityId);
        } else {
            collapsedIds.add(activityId);
        }
        renderTree();
    }

    function renderActivityRow(container, a, depth, numberPath) {
        const hasChildren = (a.children ?? []).length > 0;
        const isCollapsed = collapsedIds.has(a.activityId);
        const progress = a.progressPercentage ?? 0;
        const responsibleName = a.responsibleName ?? '';

        const indent = '<span class="wbs-indent"></span>'.repeat(depth);
        const toggle = hasChildren
            ? `<button class="wbs-toggle" onclick="event.stopPropagation(); Activities.toggleCollapse(${a.activityId})" title="${isCollapsed ? 'Expandir' : 'Colapsar'}">${isCollapsed ? '&#9656;' : '&#9662;'}</button>`
            : '<span class="wbs-indent"></span>';

        const tr = document.createElement('tr');
        tr.title = 'Doble click para editar';
        tr.ondblclick = () => openEdit(a.activityId);
        tr.innerHTML = `
            <td class="ps-3">
                <div class="wbs-tree-cell">
                    ${indent}${toggle}
                    <span class="wbs-color-dot" style="background-color:${a.colorHex}"></span>
                    <span class="wbs-number">${numberPath}</span>
                </div>
            </td>
            <td>
                <div class="wbs-name-cell">
                    <span class="wbs-name">${a.name}</span>
                    ${a.category ? `<span class="badge rounded-pill wbs-category-badge">${a.category}</span>` : ''}
                </div>
            </td>
            <td><span class="badge rounded-pill wbs-badge ${STATUS_BADGES[a.status] ?? 'text-bg-secondary'}">${STATUS_LABELS[a.status] ?? a.status}</span></td>
            <td><span class="badge rounded-pill wbs-badge ${PRIORITY_BADGES[a.priority] ?? 'text-bg-secondary'}">${PRIORITY_LABELS[a.priority] ?? a.priority}</span></td>
            <td>
                ${responsibleName ? `
                <div class="wbs-responsible">
                    <span class="wbs-avatar" style="background-color:${avatarColor(responsibleName)}">${initials(responsibleName)}</span>
                    <span>${responsibleName}</span>
                </div>` : '<span class="text-muted small">Sin asignar</span>'}
            </td>
            <td class="text-center">${a.weight}</td>
            <td>
                <div class="wbs-progress">
                    <div class="progress">
                        <div class="progress-bar bg-success" style="width:${progress}%"></div>
                    </div>
                    <span class="wbs-progress-label">${Math.round(progress)}%</span>
                </div>
            </td>
            <td class="text-end pe-3 wbs-actions">
                <div class="dropdown">
                    <button class="btn btn-sm" data-bs-toggle="dropdown" aria-expanded="false" onclick="event.stopPropagation()">&#8942;</button>
                    <ul class="dropdown-menu dropdown-menu-end">
                        <li><a class="dropdown-item" href="#" onclick="Activities.openCreate(${a.activityId});return false;">+ Subactividad</a></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.openChecklist(${a.activityId});return false;">Checklist</a></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.openTime(${a.activityId});return false;">Tiempo</a></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.openDependencies(${a.activityId});return false;">Dependencias</a></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.openParticipants(${a.activityId});return false;">Participantes</a></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.openActivityTags(${a.activityId});return false;">Etiquetas</a></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.duplicate(${a.activityId});return false;">Duplicar</a></li>
                        <li><hr class="dropdown-divider" /></li>
                        <li><a class="dropdown-item" href="#" onclick="Activities.openEdit(${a.activityId});return false;">Editar</a></li>
                        <li><a class="dropdown-item text-danger" href="#" onclick="Activities.remove(${a.activityId});return false;">Eliminar</a></li>
                    </ul>
                </div>
            </td>`;
        container.appendChild(tr);

        if (hasChildren && !isCollapsed) {
            a.children.forEach((child, index) => renderActivityRow(container, child, depth + 1, `${numberPath}.${index + 1}`));
        }
    }

    // ---- Actividad ----
    function openCreate(parentActivityId) {
        document.getElementById('activityId').value = '';
        document.getElementById('activityParentId').value = parentActivityId ?? '';
        document.getElementById('activityModalTitle').textContent = parentActivityId ? 'Nueva subactividad' : 'Nueva actividad raiz';
        document.getElementById('activityName').value = '';
        document.getElementById('activityColor').value = '#4C6EF5';
        document.getElementById('activityWeight').value = '1';
        document.getElementById('activityDescription').value = '';
        populateCategorySelect(document.getElementById('activityCategory'));
        setSelectValue(document.getElementById('activityCategory'), '');
        document.getElementById('activityStatus').value = '1';
        document.getElementById('activityPriority').value = '2';
        populateUserSelect(document.getElementById('activityResponsibleId'), '-- Selecciona un responsable --');
        setSelectValue(document.getElementById('activityResponsibleId'), '');
        const dates = defaultDates();
        document.getElementById('activityEstStart').value = dates.start;
        document.getElementById('activityEstEnd').value = dates.end;
        document.getElementById('activityActualStart').value = '';
        document.getElementById('activityActualEnd').value = '';
        document.getElementById('activityEstimatedHours').value = '0';
        document.getElementById('activityWorkedHours').value = '0';
        document.getElementById('activityProgressBanner').classList.add('d-none');
        document.getElementById('activitySubtasksSection').classList.add('d-none');

        activityModal = activityModal || new bootstrap.Modal(document.getElementById('activityModal'));
        activityModal.show();
    }

    async function openEdit(activityId) {
        const result = await Aelbry.api.get(`/Activity/GetById?activityId=${activityId}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        const a = result.data;
        document.getElementById('activityId').value = a.activityId;
        document.getElementById('activityParentId').value = a.parentActivityId ?? '';
        document.getElementById('activityModalTitle').textContent = `Editar ${a.code}`;
        document.getElementById('activityName').value = a.name;
        document.getElementById('activityColor').value = a.colorHex;
        document.getElementById('activityWeight').value = a.weight;
        document.getElementById('activityDescription').value = a.description ?? '';
        populateCategorySelect(document.getElementById('activityCategory'), a.category);
        setSelectValue(document.getElementById('activityCategory'), a.category ?? '');
        document.getElementById('activityStatus').value = a.status;
        document.getElementById('activityPriority').value = a.priority;
        populateUserSelect(document.getElementById('activityResponsibleId'), '-- Selecciona un responsable --');
        setSelectValue(document.getElementById('activityResponsibleId'), a.responsibleUserId);
        document.getElementById('activityEstStart').value = a.estimatedStartDate ? a.estimatedStartDate.substring(0, 10) : '';
        document.getElementById('activityEstEnd').value = a.estimatedEndDate ? a.estimatedEndDate.substring(0, 10) : '';
        document.getElementById('activityActualStart').value = a.actualStartDate ? a.actualStartDate.substring(0, 10) : '';
        document.getElementById('activityActualEnd').value = a.actualEndDate ? a.actualEndDate.substring(0, 10) : '';
        document.getElementById('activityEstimatedHours').value = a.estimatedHours;
        document.getElementById('activityWorkedHours').value = a.workedHours;

        const progress = a.progressPercentage ?? 0;
        document.getElementById('activityProgressBanner').classList.remove('d-none');
        document.getElementById('activityProgressLabel').textContent = `${Math.round(progress)}%`;
        document.getElementById('activityProgressBar').style.width = `${progress}%`;

        document.getElementById('activitySubtasksSection').classList.remove('d-none');
        document.getElementById('activityInlineSubtaskForm').classList.add('d-none');
        document.getElementById('activitySubtaskToggleBtn').textContent = '+ Agregar subactividad';
        renderSubtasksSection(a.activityId);

        activityModal = activityModal || new bootstrap.Modal(document.getElementById('activityModal'));
        activityModal.show();
    }

    // ---- Subactividades (dentro del modal de editar) ----
    function findNode(nodes, activityId) {
        for (const n of nodes) {
            if (n.activityId === activityId) return n;
            const found = findNode(n.children ?? [], activityId);
            if (found) return found;
        }
        return null;
    }

    function renderSubtasksSection(activityId) {
        const node = findNode(lastTreeData, activityId);
        const children = node?.children ?? [];
        const rows = document.getElementById('activitySubtaskRows');
        rows.innerHTML = '';

        if (children.length === 0) {
            rows.innerHTML = '<li class="list-group-item text-muted small">Todavia no tiene subactividades.</li>';
            return;
        }

        children.forEach((c) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <div class="d-flex align-items-center gap-2">
                    <span class="badge rounded-pill wbs-badge ${STATUS_BADGES[c.status] ?? 'text-bg-secondary'}">${STATUS_LABELS[c.status] ?? c.status}</span>
                    <span>${c.name}</span>
                    <span class="text-muted small">${Math.round(c.progressPercentage ?? 0)}%</span>
                </div>
                <button type="button" class="btn btn-sm btn-outline-secondary" onclick="Activities.openEdit(${c.activityId})">Editar</button>`;
            rows.appendChild(li);
        });
    }

    function toggleInlineSubtaskForm() {
        const form = document.getElementById('activityInlineSubtaskForm');
        const btn = document.getElementById('activitySubtaskToggleBtn');
        const opening = form.classList.contains('d-none');

        if (opening) {
            document.getElementById('subtaskName').value = '';
            document.getElementById('subtaskWeight').value = '1';
            populateCategorySelect(document.getElementById('subtaskCategory'));
            setSelectValue(document.getElementById('subtaskCategory'), '');
            populateUserSelect(document.getElementById('subtaskResponsibleId'), '-- Selecciona un responsable --');
            setSelectValue(document.getElementById('subtaskResponsibleId'), '');
            form.classList.remove('d-none');
            btn.classList.add('d-none');
        } else {
            form.classList.add('d-none');
            btn.classList.remove('d-none');
        }
    }

    async function saveInlineSubtask() {
        const parentActivityId = parseInt(document.getElementById('activityId').value, 10);
        const name = document.getElementById('subtaskName').value.trim();
        const responsibleId = document.getElementById('subtaskResponsibleId').value;

        if (!name) { alert('Ponle un nombre a la subactividad.'); return; }
        if (!responsibleId) { alert('Selecciona un responsable.'); return; }

        const dates = defaultDates();
        const payload = {
            activityId: 0,
            projectId: parseInt(projectId(), 10),
            parentActivityId,
            name,
            colorHex: '#4C6EF5',
            weight: parseFloat(document.getElementById('subtaskWeight').value || '1'),
            description: '',
            category: document.getElementById('subtaskCategory').value,
            status: 1,
            priority: 2,
            responsibleUserId: parseInt(responsibleId, 10),
            estimatedStartDate: dates.start,
            estimatedEndDate: dates.end,
            actualStartDate: null,
            actualEndDate: null,
            estimatedHours: 0,
            workedHours: 0,
            isActive: true,
        };

        const result = await Aelbry.api.post('/Activity/Create', payload);
        if (result.result !== 'OK') { alert(result.result); return; }

        await loadAll();
        renderSubtasksSection(parentActivityId);
        toggleInlineSubtaskForm();
    }

    async function save() {
        const id = document.getElementById('activityId').value;
        const parentId = document.getElementById('activityParentId').value;

        const payload = {
            activityId: id ? parseInt(id, 10) : 0,
            projectId: parseInt(projectId(), 10),
            parentActivityId: parentId ? parseInt(parentId, 10) : null,
            name: document.getElementById('activityName').value,
            colorHex: document.getElementById('activityColor').value,
            weight: parseFloat(document.getElementById('activityWeight').value || '1'),
            description: document.getElementById('activityDescription').value,
            category: document.getElementById('activityCategory').value,
            status: parseInt(document.getElementById('activityStatus').value, 10),
            priority: parseInt(document.getElementById('activityPriority').value, 10),
            responsibleUserId: parseInt(document.getElementById('activityResponsibleId').value, 10),
            estimatedStartDate: document.getElementById('activityEstStart').value || null,
            estimatedEndDate: document.getElementById('activityEstEnd').value || null,
            actualStartDate: document.getElementById('activityActualStart').value || null,
            actualEndDate: document.getElementById('activityActualEnd').value || null,
            estimatedHours: parseFloat(document.getElementById('activityEstimatedHours').value || '0'),
            workedHours: parseFloat(document.getElementById('activityWorkedHours').value || '0'),
            isActive: true,
        };

        const result = id
            ? await Aelbry.api.post('/Activity/Update', payload)
            : await Aelbry.api.post('/Activity/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('activityModal'))?.hide();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function remove(activityId) {
        if (!confirm('Eliminar esta actividad?')) return;
        const result = await Aelbry.api.post(`/Activity/Delete?activityId=${activityId}`);
        if (result.result === 'OK') loadAll();
        else alert(result.result);
    }

    async function duplicate(activityId) {
        if (!confirm('Duplicar esta actividad (con sus subactividades, checklist y etiquetas)?')) return;
        const result = await Aelbry.api.post(`/Activity/Duplicate?activityId=${activityId}`);
        if (result.result === 'OK') loadAll();
        else alert(result.result);
    }

    // ---- Checklist ----
    async function openChecklist(activityId) {
        document.getElementById('checklistActivityId').value = activityId;
        await reloadChecklist();
        checklistModal = checklistModal || new bootstrap.Modal(document.getElementById('checklistModal'));
        checklistModal.show();
    }

    async function reloadChecklist() {
        const activityId = document.getElementById('checklistActivityId').value;
        const result = await Aelbry.api.get(`/Activity/GetChecklistItems?activityId=${activityId}`);
        const rows = document.getElementById('checklistRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((item) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <div class="form-check">
                    <input class="form-check-input" type="checkbox" ${item.isChecked ? 'checked' : ''}
                           onchange="Activities.toggleChecklistItem(${item.checklistItemId}, this.checked)" />
                    <label class="form-check-label ${item.isChecked ? 'text-decoration-line-through text-muted' : ''}">${item.text}</label>
                </div>
                <button class="btn btn-sm btn-outline-danger" onclick="Activities.deleteChecklistItem(${item.checklistItemId})">Quitar</button>`;
            rows.appendChild(li);
        });
    }

    async function addChecklistItem() {
        const activityId = document.getElementById('checklistActivityId').value;
        const text = document.getElementById('newChecklistText').value;
        if (!text) return;

        const result = await Aelbry.api.post(`/Activity/AddChecklistItem?activityId=${activityId}&text=${encodeURIComponent(text)}&sequence=0`);
        if (result.result === 'OK') {
            document.getElementById('newChecklistText').value = '';
            await reloadChecklist();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function toggleChecklistItem(checklistItemId, isChecked) {
        const activityId = document.getElementById('checklistActivityId').value;
        const result = await Aelbry.api.post(`/Activity/ToggleChecklistItem?checklistItemId=${checklistItemId}&activityId=${activityId}&isChecked=${isChecked}`);
        if (result.result === 'OK') {
            await reloadChecklist();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function deleteChecklistItem(checklistItemId) {
        const activityId = document.getElementById('checklistActivityId').value;
        const result = await Aelbry.api.post(`/Activity/DeleteChecklistItem?checklistItemId=${checklistItemId}&activityId=${activityId}`);
        if (result.result === 'OK') {
            await reloadChecklist();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    // ---- Tiempo ----
    async function openTime(activityId) {
        document.getElementById('timeActivityId').value = activityId;
        await reloadTimeControls();
        await reloadTimeRows();
        timeModal = timeModal || new bootstrap.Modal(document.getElementById('timeModal'));
        timeModal.show();
    }

    async function reloadTimeControls() {
        const activityId = document.getElementById('timeActivityId').value;
        const runningResult = await Aelbry.api.get('/TimeEntry/GetRunning');
        const running = runningResult.result === 'OK' ? runningResult.data : null;
        const controls = document.getElementById('timeTimerControls');

        if (running && running.activityId === parseInt(activityId, 10)) {
            controls.innerHTML = `<button class="btn btn-danger btn-sm" onclick="Activities.stopTimer(${running.timeEntryId})">Detener cronometro</button>`;
        } else {
            controls.innerHTML = `<button class="btn btn-success btn-sm" onclick="Activities.startTimer(${activityId})">Iniciar cronometro para mi</button>`;
        }
    }

    async function startTimer(activityId) {
        const result = await Aelbry.api.post(`/TimeEntry/Start?activityId=${activityId}`);
        if (result.result === 'OK') {
            await reloadTimeControls();
            await reloadTimeRows();
        } else {
            alert(result.result);
        }
    }

    async function stopTimer(timeEntryId) {
        const result = await Aelbry.api.post(`/TimeEntry/Stop?timeEntryId=${timeEntryId}`);
        if (result.result === 'OK') {
            await reloadTimeControls();
            await reloadTimeRows();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function reloadTimeRows() {
        const activityId = document.getElementById('timeActivityId').value;
        const result = await Aelbry.api.get(`/TimeEntry/GetByActivity?activityId=${activityId}`);
        const rows = document.getElementById('timeRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((e) => {
            const li = document.createElement('li');
            li.className = 'list-group-item';
            li.innerHTML = `
                <div class="d-flex justify-content-between">
                    <span>${e.userName}</span>
                    <span>${e.durationHours}h ${e.isOvertime ? '<span class="badge text-bg-warning">Extra</span>' : ''}</span>
                </div>
                <div class="small text-muted">${new Date(e.startTime).toLocaleString()} ${e.endTime ? '- ' + new Date(e.endTime).toLocaleString() : '(en curso)'}</div>`;
            rows.appendChild(li);
        });
    }

    // ---- Dependencias ----
    async function openDependencies(activityId) {
        document.getElementById('dependenciesActivityId').value = activityId;
        await reloadDependencies();
        dependenciesModal = dependenciesModal || new bootstrap.Modal(document.getElementById('dependenciesModal'));
        dependenciesModal.show();
    }

    async function reloadDependencies() {
        const activityId = document.getElementById('dependenciesActivityId').value;
        const result = await Aelbry.api.get(`/Activity/GetDependencies?activityId=${activityId}`);
        const rows = document.getElementById('dependencyRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((d) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span>${d.dependsOnActivityName} <small class="text-muted">(${DEPENDENCY_LABELS[d.dependencyType] ?? d.dependencyType})</small></span>
                <button class="btn btn-sm btn-outline-danger" onclick="Activities.removeDependency(${d.activityDependencyId})">Quitar</button>`;
            rows.appendChild(li);
        });
    }

    async function addDependency() {
        const activityId = document.getElementById('dependenciesActivityId').value;
        const dependsOnActivityId = document.getElementById('newDependsOnActivityId').value;
        const dependencyType = document.getElementById('newDependencyType').value;
        if (!dependsOnActivityId) return;

        const result = await Aelbry.api.post(`/Activity/AddDependency?activityId=${activityId}&dependsOnActivityId=${dependsOnActivityId}&dependencyType=${dependencyType}`);
        if (result.result === 'OK') {
            document.getElementById('newDependsOnActivityId').value = '';
            await reloadDependencies();
        } else {
            alert(result.result);
        }
    }

    async function removeDependency(activityDependencyId) {
        const result = await Aelbry.api.post(`/Activity/RemoveDependency?activityDependencyId=${activityDependencyId}`);
        if (result.result === 'OK') await reloadDependencies();
        else alert(result.result);
    }

    // ---- Participantes ----
    // Una subactividad solo puede tener como participantes a quienes ya
    // participan en su actividad padre; un Empleado normal (sin permisos
    // de gestion mas amplios) solo puede elegir gente de su propio equipo
    // (esto incluye a su lider, ya que comparte el mismo equipo) al asignar
    // participantes de una actividad raiz.
    async function getParticipantCandidates(activityId) {
        const detailResult = await Aelbry.api.get(`/Activity/GetById?activityId=${activityId}`);
        const parentActivityId = detailResult.result === 'OK' ? detailResult.data.parentActivityId : null;

        if (parentActivityId) {
            const parentResult = await Aelbry.api.get(`/Activity/GetParticipants?activityId=${parentActivityId}`);
            return parentResult.result === 'OK' ? parentResult.data : [];
        }

        if (myIsEmpleado && myTeamId) {
            const teamResult = await Aelbry.api.get(`/User/GetByTeam?teamId=${myTeamId}`);
            return teamResult.result === 'OK' ? teamResult.data : [];
        }

        return companyUsers;
    }

    async function openParticipants(activityId) {
        document.getElementById('participantsActivityId').value = activityId;
        const candidates = await getParticipantCandidates(activityId);
        populateUserSelect(document.getElementById('newParticipantUserId'), '-- Selecciona una persona --', candidates);
        await reloadParticipants();
        participantsModal = participantsModal || new bootstrap.Modal(document.getElementById('participantsModal'));
        participantsModal.show();
    }

    async function reloadParticipants() {
        const activityId = document.getElementById('participantsActivityId').value;
        const result = await Aelbry.api.get(`/Activity/GetParticipants?activityId=${activityId}`);
        const rows = document.getElementById('participantRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((m) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span>${m.firstName} ${m.lastName} <small class="text-muted">${m.email}</small></span>
                <button class="btn btn-sm btn-outline-danger" onclick="Activities.removeParticipant(${m.userId})">Quitar</button>`;
            rows.appendChild(li);
        });
    }

    async function addParticipant() {
        const activityId = document.getElementById('participantsActivityId').value;
        const userId = document.getElementById('newParticipantUserId').value;
        if (!userId) return;

        const result = await Aelbry.api.post(`/Activity/AddParticipant?activityId=${activityId}&userId=${userId}`);
        if (result.result === 'OK') {
            setSelectValue(document.getElementById('newParticipantUserId'), '');
            await reloadParticipants();
        } else {
            alert(result.result);
        }
    }

    async function removeParticipant(userId) {
        const activityId = document.getElementById('participantsActivityId').value;
        const result = await Aelbry.api.post(`/Activity/RemoveParticipant?activityId=${activityId}&userId=${userId}`);
        if (result.result === 'OK') await reloadParticipants();
        else alert(result.result);
    }

    // ---- Etiquetas ----
    async function openActivityTags(activityId) {
        document.getElementById('activityTagsActivityId').value = activityId;

        const result = await Aelbry.api.get(`/Activity/GetTags?activityId=${activityId}`);
        const activeTagIds = result.result === 'OK' ? result.data.map((t) => t.tagId) : [];

        const container = document.getElementById('activityTagsChecklist');
        container.innerHTML = companyTags.map((t) => `
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="atag_${t.tagId}"
                       ${activeTagIds.includes(t.tagId) ? 'checked' : ''}
                       onchange="Activities.toggleActivityTag(${t.tagId}, this.checked)" />
                <label class="form-check-label" for="atag_${t.tagId}">
                    <span class="badge" style="background-color:${t.colorHex}">${t.name}</span>
                </label>
            </div>`).join('');

        activityTagsModal = activityTagsModal || new bootstrap.Modal(document.getElementById('activityTagsModal'));
        activityTagsModal.show();
    }

    async function toggleActivityTag(tagId, checked) {
        const activityId = document.getElementById('activityTagsActivityId').value;
        const url = checked ? '/Activity/AddTag' : '/Activity/RemoveTag';
        const result = await Aelbry.api.post(`${url}?activityId=${activityId}&tagId=${tagId}`);
        if (result.result !== 'OK') alert(result.result);
    }

    return {
        loadAll, applyFilters, toggleCollapse, openCreate, openEdit, save, remove, duplicate,
        promptNewCategory, toggleInlineSubtaskForm, saveInlineSubtask,
        openChecklist, addChecklistItem, toggleChecklistItem, deleteChecklistItem,
        openTime, startTimer, stopTimer,
        openDependencies, addDependency, removeDependency,
        openParticipants, addParticipant, removeParticipant,
        openActivityTags, toggleActivityTag,
    };
})();

Aelbry.projectContext.autoLoad(Activities.loadAll);
