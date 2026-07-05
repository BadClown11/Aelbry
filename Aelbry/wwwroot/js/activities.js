window.Activities = (function () {
    const STATUS_LABELS = { 1: 'Pendiente', 2: 'En Progreso', 3: 'Bloqueada', 4: 'Completada', 5: 'Cancelada' };
    const PRIORITY_LABELS = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Critica' };
    const DEPENDENCY_LABELS = { 1: 'Fin a Inicio', 2: 'Inicio a Inicio', 3: 'Fin a Fin', 4: 'Inicio a Fin' };

    let activityModal, checklistModal, dependenciesModal, participantsModal, activityTagsModal, timeModal;
    let companyId = null;
    let companyTags = [];
    let companyUsers = [];
    let myTeamId = null;
    let myIsEmpleado = false;

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
        }

        const me = Aelbry.api.getCurrentUser();
        if (me) {
            myIsEmpleado = (me.roles ?? []).includes('Empleado');
            const meResult = await Aelbry.api.get(`/User/GetById?userId=${me.userId}`);
            myTeamId = meResult.result === 'OK' ? meResult.data.teamId : null;
        }

        const result = await Aelbry.api.get(`/Activity/GetTreeByProject?projectId=${pid}`);
        console.log('[Actividades] GetTreeByProject ->', result);
        const rows = document.getElementById('activityRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((a) => renderActivityRow(rows, a, 0));
    }

    function renderActivityRow(container, a, depth) {
        const tr = document.createElement('tr');
        const indent = '&nbsp;&nbsp;&nbsp;&nbsp;'.repeat(depth) + (depth > 0 ? '&#8627; ' : '');
        tr.innerHTML = `
            <td><span class="badge" style="background-color:${a.colorHex}">${a.code}</span></td>
            <td>${indent}${a.name}</td>
            <td>${STATUS_LABELS[a.status] ?? a.status}</td>
            <td>${PRIORITY_LABELS[a.priority] ?? a.priority}</td>
            <td>${a.responsibleName ?? ''}</td>
            <td>${a.weight}</td>
            <td>${a.progressPercentage ?? 0}%</td>
            <td class="text-end text-nowrap">
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.openCreate(${a.activityId})">+ Sub</button>
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.openChecklist(${a.activityId})">Checklist</button>
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.openTime(${a.activityId})">Tiempo</button>
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.openDependencies(${a.activityId})">Dependencias</button>
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.openParticipants(${a.activityId})">Participantes</button>
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.openActivityTags(${a.activityId})">Etiquetas</button>
                <button class="btn btn-sm btn-outline-secondary" onclick="Activities.duplicate(${a.activityId})">Duplicar</button>
                <button class="btn btn-sm btn-outline-primary" onclick="Activities.openEdit(${a.activityId})">Editar</button>
                <button class="btn btn-sm btn-outline-danger" onclick="Activities.remove(${a.activityId})">Eliminar</button>
            </td>`;
        container.appendChild(tr);

        (a.children ?? []).forEach((child) => renderActivityRow(container, child, depth + 1));
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
        document.getElementById('activityCategory').value = '';
        document.getElementById('activityStatus').value = '1';
        document.getElementById('activityPriority').value = '2';
        populateUserSelect(document.getElementById('activityResponsibleId'), '-- Selecciona un responsable --');
        setSelectValue(document.getElementById('activityResponsibleId'), '');
        document.getElementById('activityEstStart').value = '';
        document.getElementById('activityEstEnd').value = '';
        document.getElementById('activityActualStart').value = '';
        document.getElementById('activityActualEnd').value = '';
        document.getElementById('activityEstimatedHours').value = '0';
        document.getElementById('activityWorkedHours').value = '0';
        document.getElementById('activityProgressBanner').classList.add('d-none');

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
        document.getElementById('activityCategory').value = a.category ?? '';
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

        activityModal = activityModal || new bootstrap.Modal(document.getElementById('activityModal'));
        activityModal.show();
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
        loadAll, openCreate, openEdit, save, remove, duplicate,
        openChecklist, addChecklistItem, toggleChecklistItem, deleteChecklistItem,
        openTime, startTimer, stopTimer,
        openDependencies, addDependency, removeDependency,
        openParticipants, addParticipant, removeParticipant,
        openActivityTags, toggleActivityTag,
    };
})();

Aelbry.projectContext.autoLoad(Activities.loadAll);
