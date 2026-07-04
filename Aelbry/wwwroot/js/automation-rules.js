window.AutomationRules = (function () {
    const TRIGGER_TYPE_LABELS = { 1: 'Avance de Actividad', 2: 'Estado de Actividad', 3: 'Avance de Proyecto' };
    const ACTION_TYPE_LABELS = { 1: 'Cambiar estado de Actividad', 2: 'Cambiar estado de Proyecto', 3: 'Enviar notificacion' };
    const STATUS_LABELS = { 1: 'Pendiente', 2: 'En Progreso', 3: 'Bloqueada', 4: 'Completada', 5: 'Cancelada' };

    let ruleModal, logModal;

    function companyId() {
        return document.getElementById('filterCompanyId').value;
    }

    function describeTrigger(r) {
        if (r.triggerType === 1) return `Actividad #${r.triggerActivityId} avance >= ${r.triggerThresholdPercent}%`;
        if (r.triggerType === 2) return `Actividad #${r.triggerActivityId} cambia a ${STATUS_LABELS[r.triggerStatus] ?? r.triggerStatus}`;
        if (r.triggerType === 3) return `Proyecto #${r.triggerProjectId} avance >= ${r.triggerThresholdPercent}%`;
        return TRIGGER_TYPE_LABELS[r.triggerType] ?? '';
    }

    function describeAction(r) {
        if (r.actionType === 1) return `Actividad #${r.actionTargetActivityId} -> ${STATUS_LABELS[r.actionNewActivityStatus] ?? r.actionNewActivityStatus}`;
        if (r.actionType === 2) return `Proyecto #${r.actionTargetProjectId} -> estado #${r.actionNewProjectStatusId}`;
        if (r.actionType === 3) return `Notificar a usuario #${r.actionNotificationUserId}: "${r.actionNotificationMessage ?? ''}"`;
        return ACTION_TYPE_LABELS[r.actionType] ?? '';
    }

    async function loadAll() {
        const cid = companyId();
        if (!cid) return;

        const result = await Aelbry.api.get(`/AutomationRule/GetByCompany?companyId=${cid}`);
        const rows = document.getElementById('ruleRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((r) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${r.name}</td>
                <td>${describeTrigger(r)}</td>
                <td>${describeAction(r)}</td>
                <td>${r.isActive ? '<span class="badge text-bg-success">Si</span>' : '<span class="badge text-bg-secondary">No</span>'}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-secondary" onclick="AutomationRules.openLogs(${r.automationRuleId})">Bitacora</button>
                    <button class="btn btn-sm btn-outline-primary" onclick="AutomationRules.openEdit(${r.automationRuleId})">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="AutomationRules.remove(${r.automationRuleId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    function onTriggerTypeChange() {
        const type = parseInt(document.getElementById('triggerType').value, 10);
        document.getElementById('triggerActivityFields').classList.toggle('d-none', type === 3);
        document.getElementById('triggerProjectFields').classList.toggle('d-none', type !== 3);
        document.getElementById('triggerThresholdFields').classList.toggle('d-none', type === 2);
        document.getElementById('triggerStatusFields').classList.toggle('d-none', type !== 2);
    }

    function onActionTypeChange() {
        const type = parseInt(document.getElementById('actionType').value, 10);
        document.getElementById('actionActivityFields').classList.toggle('d-none', type !== 1);
        document.getElementById('actionProjectFields').classList.toggle('d-none', type !== 2);
        document.getElementById('actionNotificationFields').classList.toggle('d-none', type !== 3);
    }

    function openCreate() {
        document.getElementById('ruleId').value = '';
        document.getElementById('ruleName').value = '';
        document.getElementById('triggerType').value = '1';
        document.getElementById('triggerActivityId').value = '';
        document.getElementById('triggerProjectId').value = '';
        document.getElementById('triggerThresholdPercent').value = '100';
        document.getElementById('triggerStatus').value = '4';
        document.getElementById('actionType').value = '1';
        document.getElementById('actionTargetActivityId').value = '';
        document.getElementById('actionTargetProjectId').value = '';
        document.getElementById('actionNewActivityStatus').value = '4';
        document.getElementById('actionNewProjectStatusId').value = '';
        document.getElementById('actionNotificationUserId').value = '';
        document.getElementById('actionNotificationMessage').value = '';
        document.getElementById('ruleIsActive').checked = true;
        onTriggerTypeChange();
        onActionTypeChange();
    }

    async function openEdit(automationRuleId) {
        const result = await Aelbry.api.get(`/AutomationRule/GetById?automationRuleId=${automationRuleId}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        const r = result.data;
        document.getElementById('ruleId').value = r.automationRuleId;
        document.getElementById('ruleName').value = r.name;
        document.getElementById('triggerType').value = r.triggerType;
        document.getElementById('triggerActivityId').value = r.triggerActivityId ?? '';
        document.getElementById('triggerProjectId').value = r.triggerProjectId ?? '';
        document.getElementById('triggerThresholdPercent').value = r.triggerThresholdPercent ?? '';
        document.getElementById('triggerStatus').value = r.triggerStatus ?? '4';
        document.getElementById('actionType').value = r.actionType;
        document.getElementById('actionTargetActivityId').value = r.actionTargetActivityId ?? '';
        document.getElementById('actionTargetProjectId').value = r.actionTargetProjectId ?? '';
        document.getElementById('actionNewActivityStatus').value = r.actionNewActivityStatus ?? '4';
        document.getElementById('actionNewProjectStatusId').value = r.actionNewProjectStatusId ?? '';
        document.getElementById('actionNotificationUserId').value = r.actionNotificationUserId ?? '';
        document.getElementById('actionNotificationMessage').value = r.actionNotificationMessage ?? '';
        document.getElementById('ruleIsActive').checked = r.isActive;
        onTriggerTypeChange();
        onActionTypeChange();

        ruleModal = ruleModal || new bootstrap.Modal(document.getElementById('ruleModal'));
        ruleModal.show();
    }

    async function save() {
        const id = document.getElementById('ruleId').value;
        const val = (elId) => document.getElementById(elId).value;
        const numOrNull = (elId) => (val(elId) ? parseInt(val(elId), 10) : null);
        const floatOrNull = (elId) => (val(elId) ? parseFloat(val(elId)) : null);

        const payload = {
            automationRuleId: id ? parseInt(id, 10) : 0,
            companyId: parseInt(companyId(), 10),
            name: val('ruleName'),
            triggerType: parseInt(val('triggerType'), 10),
            triggerActivityId: numOrNull('triggerActivityId'),
            triggerProjectId: numOrNull('triggerProjectId'),
            triggerThresholdPercent: floatOrNull('triggerThresholdPercent'),
            triggerStatus: parseInt(val('triggerStatus'), 10),
            actionType: parseInt(val('actionType'), 10),
            actionTargetActivityId: numOrNull('actionTargetActivityId'),
            actionTargetProjectId: numOrNull('actionTargetProjectId'),
            actionNewActivityStatus: parseInt(val('actionNewActivityStatus'), 10),
            actionNewProjectStatusId: numOrNull('actionNewProjectStatusId'),
            actionNotificationUserId: numOrNull('actionNotificationUserId'),
            actionNotificationMessage: val('actionNotificationMessage'),
            isActive: document.getElementById('ruleIsActive').checked,
        };

        const result = id
            ? await Aelbry.api.post('/AutomationRule/Update', payload)
            : await Aelbry.api.post('/AutomationRule/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('ruleModal'))?.hide();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function remove(automationRuleId) {
        if (!confirm('Eliminar esta regla?')) return;
        const result = await Aelbry.api.post(`/AutomationRule/Delete?automationRuleId=${automationRuleId}`);
        if (result.result === 'OK') loadAll();
        else alert(result.result);
    }

    async function openLogs(automationRuleId) {
        const result = await Aelbry.api.get(`/AutomationRule/GetLogs?automationRuleId=${automationRuleId}`);
        const rows = document.getElementById('logRows');
        rows.innerHTML = '';

        if (result.result === 'OK') {
            result.data.forEach((l) => {
                const li = document.createElement('li');
                li.className = 'list-group-item';
                li.innerHTML = `
                    <div class="d-flex justify-content-between">
                        <span>${new Date(l.triggeredDate).toLocaleString()}</span>
                        ${l.success ? '<span class="badge text-bg-success">OK</span>' : '<span class="badge text-bg-danger">Error</span>'}
                    </div>
                    <div class="small text-muted">${l.details ?? ''}</div>`;
                rows.appendChild(li);
            });
        }

        logModal = logModal || new bootstrap.Modal(document.getElementById('logModal'));
        logModal.show();
    }

    return { loadAll, onTriggerTypeChange, onActionTypeChange, openCreate, openEdit, save, remove, openLogs };
})();
