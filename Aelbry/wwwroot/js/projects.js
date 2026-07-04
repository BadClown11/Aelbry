window.Projects = (function () {
    const PRIORITY_LABELS = { 1: 'Baja', 2: 'Media', 3: 'Alta', 4: 'Critica' };
    const RISK_LABELS = { 1: 'Bajo', 2: 'Medio', 3: 'Alto' };

    let modal, membersModal, projectTagsModal, statusModal, tagModal, templateModal;
    let duplicateProjectModal, templateSkeletonModal, applyTemplateModal;
    let companyTags = [];
    let projectTagsCache = [];

    function companyId() {
        return document.getElementById('filterCompanyId').value;
    }

    async function loadAll() {
        const cid = companyId();
        if (!cid) return;

        await Promise.all([loadStatuses(), loadTags(), loadTemplates()]);
        await loadProjects();
    }

    async function loadProjects() {
        const cid = companyId();
        const result = await Aelbry.api.get(`/Project/GetByCompany?companyId=${cid}`);
        const rows = document.getElementById('projectRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((p) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><span class="badge" style="background-color:${p.colorHex}">${p.code}</span></td>
                <td>${p.name}</td>
                <td>${p.projectStatusName ?? ''}</td>
                <td>${PRIORITY_LABELS[p.priority] ?? p.priority}</td>
                <td>${RISK_LABELS[p.riskLevel] ?? p.riskLevel}</td>
                <td>${p.projectManagerName ?? ''}</td>
                <td>${p.progressPercentage ?? 0}%</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-secondary" onclick="Projects.openMembers(${p.projectId})">Miembros</button>
                    <button class="btn btn-sm btn-outline-secondary" onclick="Projects.openProjectTags(${p.projectId})">Etiquetas</button>
                    <button class="btn btn-sm btn-outline-secondary" onclick="Projects.openDuplicate(${p.projectId}, '${p.code}', '${(p.name ?? '').replace(/'/g, "\\'")}')">Duplicar</button>
                    <button class="btn btn-sm btn-outline-primary" onclick="Projects.openEdit(${p.projectId})">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Projects.remove(${p.projectId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    function fillStatusSelect(statuses) {
        const select = document.getElementById('projectStatusId');
        select.innerHTML = statuses.map((s) => `<option value="${s.projectStatusId}">${s.name}</option>`).join('');
        Aelbry.ui.initSelect2(select);
    }

    async function loadStatuses() {
        const cid = companyId();
        const result = await Aelbry.api.get(`/ProjectStatus/GetByCompany?companyId=${cid}`);
        const rows = document.getElementById('statusRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        fillStatusSelect(result.data);

        result.data.forEach((s) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${s.sequence}</td>
                <td><span class="badge" style="background-color:${s.colorHex}">${s.name}</span></td>
                <td>${s.colorHex}</td>
                <td>${s.isFinal ? 'Si' : 'No'}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-primary" onclick='Projects.openEditStatus(${JSON.stringify(s)})'>Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Projects.removeStatus(${s.projectStatusId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    async function loadTags() {
        const cid = companyId();
        const result = await Aelbry.api.get(`/Tag/GetByCompany?companyId=${cid}`);
        const rows = document.getElementById('tagRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        companyTags = result.data;

        result.data.forEach((t) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><span class="badge" style="background-color:${t.colorHex}">${t.name}</span></td>
                <td>${t.colorHex}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-primary" onclick='Projects.openEditTag(${JSON.stringify(t)})'>Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Projects.removeTag(${t.tagId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    async function loadTemplates() {
        const cid = companyId();
        const result = await Aelbry.api.get(`/ProjectTemplate/GetByCompany?companyId=${cid}`);
        const rows = document.getElementById('templateRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((t) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${t.name}</td>
                <td>${t.description ?? ''}</td>
                <td>${PRIORITY_LABELS[t.defaultPriority] ?? t.defaultPriority}</td>
                <td>${t.defaultEstimatedHours}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-secondary" onclick="Projects.openSkeleton(${t.projectTemplateId})">Esqueleto</button>
                    <button class="btn btn-sm btn-outline-success" onclick="Projects.openApplyTemplate(${t.projectTemplateId})">Aplicar</button>
                    <button class="btn btn-sm btn-outline-primary" onclick='Projects.openEditTemplate(${JSON.stringify(t)})'>Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Projects.removeTemplate(${t.projectTemplateId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    // ---- Proyectos ----
    function openCreate() {
        document.getElementById('projectId').value = '';
        document.getElementById('projectCode').value = '';
        document.getElementById('projectName').value = '';
        document.getElementById('projectColor').value = '#4C6EF5';
        document.getElementById('projectClient').value = '';
        document.getElementById('projectManagerId').value = '';
        document.getElementById('projectPriority').value = '2';
        document.getElementById('projectRiskLevel').value = '1';
        document.getElementById('projectStartDate').value = '';
        document.getElementById('projectEndDate').value = '';
        document.getElementById('projectBudget').value = '';
        document.getElementById('projectEstimatedHours').value = '0';
    }

    async function openEdit(projectId) {
        const result = await Aelbry.api.get(`/Project/GetById?projectId=${projectId}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        const p = result.data;
        document.getElementById('projectId').value = p.projectId;
        document.getElementById('projectCode').value = p.code;
        document.getElementById('projectName').value = p.name;
        document.getElementById('projectColor').value = p.colorHex;
        document.getElementById('projectClient').value = p.clientName ?? '';
        document.getElementById('projectManagerId').value = p.projectManagerId;
        document.getElementById('projectStatusId').value = p.projectStatusId;
        document.getElementById('projectPriority').value = p.priority;
        document.getElementById('projectRiskLevel').value = p.riskLevel;
        document.getElementById('projectStartDate').value = p.startDate ? p.startDate.substring(0, 10) : '';
        document.getElementById('projectEndDate').value = p.endDate ? p.endDate.substring(0, 10) : '';
        document.getElementById('projectBudget').value = p.budget ?? '';
        document.getElementById('projectEstimatedHours').value = p.estimatedHours;

        modal = modal || new bootstrap.Modal(document.getElementById('projectModal'));
        modal.show();
    }

    async function save() {
        const id = document.getElementById('projectId').value;
        const payload = {
            projectId: id ? parseInt(id, 10) : 0,
            companyId: parseInt(companyId(), 10),
            code: document.getElementById('projectCode').value,
            name: document.getElementById('projectName').value,
            colorHex: document.getElementById('projectColor').value,
            clientName: document.getElementById('projectClient').value,
            projectManagerId: parseInt(document.getElementById('projectManagerId').value, 10),
            projectStatusId: parseInt(document.getElementById('projectStatusId').value, 10),
            priority: parseInt(document.getElementById('projectPriority').value, 10),
            riskLevel: parseInt(document.getElementById('projectRiskLevel').value, 10),
            startDate: document.getElementById('projectStartDate').value || null,
            endDate: document.getElementById('projectEndDate').value || null,
            budget: document.getElementById('projectBudget').value ? parseFloat(document.getElementById('projectBudget').value) : null,
            estimatedHours: parseFloat(document.getElementById('projectEstimatedHours').value || '0'),
            isActive: true,
        };

        const result = id
            ? await Aelbry.api.post('/Project/Update', payload)
            : await Aelbry.api.post('/Project/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('projectModal'))?.hide();
            loadProjects();
        } else {
            alert(result.result);
        }
    }

    async function remove(projectId) {
        if (!confirm('Eliminar este proyecto?')) return;
        const result = await Aelbry.api.post(`/Project/Delete?projectId=${projectId}`);
        if (result.result === 'OK') loadProjects();
        else alert(result.result);
    }

    // ---- Duplicacion profunda ----
    function openDuplicate(projectId, code, name) {
        document.getElementById('duplicateSourceProjectId').value = projectId;
        document.getElementById('duplicateNewCode').value = `${code}-COPIA`;
        document.getElementById('duplicateNewName').value = `${name} (copia)`;
        duplicateProjectModal = duplicateProjectModal || new bootstrap.Modal(document.getElementById('duplicateProjectModal'));
        duplicateProjectModal.show();
    }

    async function confirmDuplicate() {
        const projectId = document.getElementById('duplicateSourceProjectId').value;
        const newCode = document.getElementById('duplicateNewCode').value;
        const newName = document.getElementById('duplicateNewName').value;

        const result = await Aelbry.api.post(`/Project/Duplicate?projectId=${projectId}&newCode=${encodeURIComponent(newCode)}&newName=${encodeURIComponent(newName)}`);
        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('duplicateProjectModal'))?.hide();
            loadProjects();
        } else {
            alert(result.result);
        }
    }

    // ---- Miembros ----
    async function openMembers(projectId) {
        document.getElementById('membersProjectId').value = projectId;
        await reloadMembers();
        membersModal = membersModal || new bootstrap.Modal(document.getElementById('membersModal'));
        membersModal.show();
    }

    async function reloadMembers() {
        const projectId = document.getElementById('membersProjectId').value;
        const result = await Aelbry.api.get(`/Project/GetMembers?projectId=${projectId}`);
        const rows = document.getElementById('memberRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((m) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span>${m.firstName} ${m.lastName} <small class="text-muted">${m.email}</small></span>
                <button class="btn btn-sm btn-outline-danger" onclick="Projects.removeMember(${m.userId})">Quitar</button>`;
            rows.appendChild(li);
        });
    }

    async function addMember() {
        const projectId = document.getElementById('membersProjectId').value;
        const userId = document.getElementById('newMemberUserId').value;
        if (!userId) return;

        const result = await Aelbry.api.post(`/Project/AddMember?projectId=${projectId}&userId=${userId}`);
        if (result.result === 'OK') {
            document.getElementById('newMemberUserId').value = '';
            await reloadMembers();
        } else {
            alert(result.result);
        }
    }

    async function removeMember(userId) {
        const projectId = document.getElementById('membersProjectId').value;
        const result = await Aelbry.api.post(`/Project/RemoveMember?projectId=${projectId}&userId=${userId}`);
        if (result.result === 'OK') await reloadMembers();
        else alert(result.result);
    }

    // ---- Etiquetas del proyecto ----
    async function openProjectTags(projectId) {
        document.getElementById('projectTagsProjectId').value = projectId;

        const result = await Aelbry.api.get(`/Project/GetTags?projectId=${projectId}`);
        projectTagsCache = result.result === 'OK' ? result.data.map((t) => t.tagId) : [];

        const container = document.getElementById('projectTagsChecklist');
        container.innerHTML = companyTags.map((t) => `
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="ptag_${t.tagId}"
                       ${projectTagsCache.includes(t.tagId) ? 'checked' : ''}
                       onchange="Projects.toggleProjectTag(${t.tagId}, this.checked)" />
                <label class="form-check-label" for="ptag_${t.tagId}">
                    <span class="badge" style="background-color:${t.colorHex}">${t.name}</span>
                </label>
            </div>`).join('');

        projectTagsModal = projectTagsModal || new bootstrap.Modal(document.getElementById('projectTagsModal'));
        projectTagsModal.show();
    }

    async function toggleProjectTag(tagId, checked) {
        const projectId = document.getElementById('projectTagsProjectId').value;
        const url = checked ? '/Project/AddTag' : '/Project/RemoveTag';
        const result = await Aelbry.api.post(`${url}?projectId=${projectId}&tagId=${tagId}`);
        if (result.result === 'OK') {
            loadProjects();
        } else {
            alert(result.result);
        }
    }

    // ---- Estados ----
    function openCreateStatus() {
        document.getElementById('statusId').value = '';
        document.getElementById('statusName').value = '';
        document.getElementById('statusColor').value = '#6C757D';
        document.getElementById('statusSequence').value = '0';
        document.getElementById('statusIsFinal').checked = false;
    }

    function openEditStatus(status) {
        document.getElementById('statusId').value = status.projectStatusId;
        document.getElementById('statusName').value = status.name;
        document.getElementById('statusColor').value = status.colorHex;
        document.getElementById('statusSequence').value = status.sequence;
        document.getElementById('statusIsFinal').checked = status.isFinal;
        statusModal = statusModal || new bootstrap.Modal(document.getElementById('statusModal'));
        statusModal.show();
    }

    async function saveStatus() {
        const id = document.getElementById('statusId').value;
        const payload = {
            projectStatusId: id ? parseInt(id, 10) : 0,
            companyId: parseInt(companyId(), 10),
            name: document.getElementById('statusName').value,
            colorHex: document.getElementById('statusColor').value,
            sequence: parseInt(document.getElementById('statusSequence').value || '0', 10),
            isFinal: document.getElementById('statusIsFinal').checked,
            isActive: true,
        };

        const result = id
            ? await Aelbry.api.post('/ProjectStatus/Update', payload)
            : await Aelbry.api.post('/ProjectStatus/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('statusModal'))?.hide();
            loadStatuses();
        } else {
            alert(result.result);
        }
    }

    async function removeStatus(projectStatusId) {
        if (!confirm('Eliminar este estado?')) return;
        const result = await Aelbry.api.post(`/ProjectStatus/Delete?projectStatusId=${projectStatusId}`);
        if (result.result === 'OK') loadStatuses();
        else alert(result.result);
    }

    // ---- Etiquetas (catalogo) ----
    function openCreateTag() {
        document.getElementById('tagId').value = '';
        document.getElementById('tagName').value = '';
        document.getElementById('tagColor').value = '#868E96';
    }

    function openEditTag(tag) {
        document.getElementById('tagId').value = tag.tagId;
        document.getElementById('tagName').value = tag.name;
        document.getElementById('tagColor').value = tag.colorHex;
        tagModal = tagModal || new bootstrap.Modal(document.getElementById('tagModal'));
        tagModal.show();
    }

    async function saveTag() {
        const id = document.getElementById('tagId').value;
        const payload = {
            tagId: id ? parseInt(id, 10) : 0,
            companyId: parseInt(companyId(), 10),
            name: document.getElementById('tagName').value,
            colorHex: document.getElementById('tagColor').value,
            isActive: true,
        };

        const result = id
            ? await Aelbry.api.post('/Tag/Update', payload)
            : await Aelbry.api.post('/Tag/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('tagModal'))?.hide();
            loadTags();
        } else {
            alert(result.result);
        }
    }

    async function removeTag(tagId) {
        if (!confirm('Eliminar esta etiqueta?')) return;
        const result = await Aelbry.api.post(`/Tag/Delete?tagId=${tagId}`);
        if (result.result === 'OK') loadTags();
        else alert(result.result);
    }

    // ---- Plantillas ----
    function openCreateTemplate() {
        document.getElementById('templateId').value = '';
        document.getElementById('templateName').value = '';
        document.getElementById('templateDescription').value = '';
        document.getElementById('templateDefaultPriority').value = '2';
        document.getElementById('templateDefaultEstimatedHours').value = '0';
    }

    function openEditTemplate(template) {
        document.getElementById('templateId').value = template.projectTemplateId;
        document.getElementById('templateName').value = template.name;
        document.getElementById('templateDescription').value = template.description ?? '';
        document.getElementById('templateDefaultPriority').value = template.defaultPriority;
        document.getElementById('templateDefaultEstimatedHours').value = template.defaultEstimatedHours;
        templateModal = templateModal || new bootstrap.Modal(document.getElementById('templateModal'));
        templateModal.show();
    }

    async function saveTemplate() {
        const id = document.getElementById('templateId').value;
        const payload = {
            projectTemplateId: id ? parseInt(id, 10) : 0,
            companyId: parseInt(companyId(), 10),
            name: document.getElementById('templateName').value,
            description: document.getElementById('templateDescription').value,
            defaultPriority: parseInt(document.getElementById('templateDefaultPriority').value, 10),
            defaultEstimatedHours: parseFloat(document.getElementById('templateDefaultEstimatedHours').value || '0'),
            isActive: true,
        };

        const result = id
            ? await Aelbry.api.post('/ProjectTemplate/Update', payload)
            : await Aelbry.api.post('/ProjectTemplate/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('templateModal'))?.hide();
            loadTemplates();
        } else {
            alert(result.result);
        }
    }

    async function removeTemplate(projectTemplateId) {
        if (!confirm('Eliminar esta plantilla?')) return;
        const result = await Aelbry.api.post(`/ProjectTemplate/Delete?projectTemplateId=${projectTemplateId}`);
        if (result.result === 'OK') loadTemplates();
        else alert(result.result);
    }

    // ---- Esqueleto de plantilla ----
    async function openSkeleton(projectTemplateId) {
        document.getElementById('skeletonTemplateId').value = projectTemplateId;
        await reloadSkeleton();
        templateSkeletonModal = templateSkeletonModal || new bootstrap.Modal(document.getElementById('templateSkeletonModal'));
        templateSkeletonModal.show();
    }

    async function reloadSkeleton() {
        const projectTemplateId = document.getElementById('skeletonTemplateId').value;
        const result = await Aelbry.api.get(`/ProjectTemplate/GetActivities?projectTemplateId=${projectTemplateId}`);
        const rows = document.getElementById('skeletonRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((a) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span><strong>${a.name}</strong> <small class="text-muted">${a.description ?? ''} (${a.estimatedHours}h)</small></span>
                <button class="btn btn-sm btn-outline-danger" onclick="Projects.removeSkeletonActivity(${a.projectTemplateActivityId})">Quitar</button>`;
            rows.appendChild(li);
        });
    }

    async function addSkeletonActivity() {
        const projectTemplateId = document.getElementById('skeletonTemplateId').value;
        const name = document.getElementById('newSkeletonName').value;
        const description = document.getElementById('newSkeletonDescription').value;
        const hours = parseFloat(document.getElementById('newSkeletonHours').value || '0');
        if (!name) return;

        const result = await Aelbry.api.post(`/ProjectTemplate/AddActivity?projectTemplateId=${projectTemplateId}&name=${encodeURIComponent(name)}&description=${encodeURIComponent(description)}&estimatedHours=${hours}&sequence=0`);
        if (result.result === 'OK') {
            document.getElementById('newSkeletonName').value = '';
            document.getElementById('newSkeletonDescription').value = '';
            document.getElementById('newSkeletonHours').value = '';
            await reloadSkeleton();
        } else {
            alert(result.result);
        }
    }

    async function removeSkeletonActivity(projectTemplateActivityId) {
        const result = await Aelbry.api.post(`/ProjectTemplate/RemoveActivity?projectTemplateActivityId=${projectTemplateActivityId}`);
        if (result.result === 'OK') await reloadSkeleton();
        else alert(result.result);
    }

    // ---- Aplicar plantilla ----
    function openApplyTemplate(projectTemplateId) {
        document.getElementById('applyTemplateId').value = projectTemplateId;
        document.getElementById('applyCompanyId').value = companyId() || '';
        document.getElementById('applyCode').value = '';
        document.getElementById('applyName').value = '';
        document.getElementById('applyManagerId').value = '';
        document.getElementById('applyStatusId').innerHTML = '';
        if (companyId()) loadApplyStatuses();

        applyTemplateModal = applyTemplateModal || new bootstrap.Modal(document.getElementById('applyTemplateModal'));
        applyTemplateModal.show();
    }

    async function loadApplyStatuses() {
        const cid = document.getElementById('applyCompanyId').value;
        if (!cid) return;

        const result = await Aelbry.api.get(`/ProjectStatus/GetByCompany?companyId=${cid}`);
        const select = document.getElementById('applyStatusId');
        select.innerHTML = result.result === 'OK'
            ? result.data.map((s) => `<option value="${s.projectStatusId}">${s.name}</option>`).join('')
            : '';
        Aelbry.ui.initSelect2(select);
    }

    async function confirmApplyTemplate() {
        const templateId = document.getElementById('applyTemplateId').value;
        const companyIdValue = document.getElementById('applyCompanyId').value;
        const code = document.getElementById('applyCode').value;
        const name = document.getElementById('applyName').value;
        const statusId = document.getElementById('applyStatusId').value;
        const managerId = document.getElementById('applyManagerId').value;

        const result = await Aelbry.api.post(`/Project/CreateFromTemplate?projectTemplateId=${templateId}&code=${encodeURIComponent(code)}&name=${encodeURIComponent(name)}&companyId=${companyIdValue}&projectStatusId=${statusId}&projectManagerId=${managerId}`);
        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('applyTemplateModal'))?.hide();
            alert(`Proyecto #${result.data} creado a partir de la plantilla.`);
            if (companyId()) loadProjects();
        } else {
            alert(result.result);
        }
    }

    return {
        loadAll, openCreate, openEdit, save, remove,
        openMembers, addMember, removeMember,
        openProjectTags, toggleProjectTag,
        openDuplicate, confirmDuplicate,
        openCreateStatus, openEditStatus, saveStatus, removeStatus,
        openCreateTag, openEditTag, saveTag, removeTag,
        openCreateTemplate, openEditTemplate, saveTemplate, removeTemplate,
        openSkeleton, addSkeletonActivity, removeSkeletonActivity,
        openApplyTemplate, loadApplyStatuses, confirmApplyTemplate,
    };
})();
