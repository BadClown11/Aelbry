window.AiAssistant = (function () {
    let lastSuggestion = null;
    let lastExcelToken = null;
    let lastExcelColumns = [];

    const MAPPING_FIELDS = [
        { id: 'mapName', label: 'Nombre (obligatorio)' },
        { id: 'mapDescription', label: 'Descripcion' },
        { id: 'mapEstimatedHours', label: 'Horas estimadas' },
        { id: 'mapCategory', label: 'Categoria' },
        { id: 'mapResponsible', label: 'Responsable (ID o email)' },
    ];

    function toggleTarget() {
        const isNew = document.getElementById('targetNew').checked;
        document.getElementById('targetExistingFields').classList.toggle('d-none', isNew);
        document.getElementById('targetNewFields').classList.toggle('d-none', !isNew);
    }

    async function loadStatuses() {
        const companyId = document.getElementById('aiNewCompanyId').value;
        if (!companyId) return;

        const result = await Aelbry.api.get(`/ProjectStatus/GetByCompany?companyId=${companyId}`);
        const select = document.getElementById('aiNewProjectStatusId');
        select.innerHTML = result.result === 'OK'
            ? result.data.map((s) => `<option value="${s.projectStatusId}">${s.name}</option>`).join('')
            : '';
    }

    function appendMessage(html, alignRight) {
        const container = document.getElementById('chatMessages');
        const wrapper = document.createElement('div');
        wrapper.className = `d-flex mb-2 ${alignRight ? 'justify-content-end' : 'justify-content-start'}`;
        wrapper.innerHTML = `<div class="p-2 rounded ${alignRight ? 'bg-primary text-white' : 'bg-light border'}" style="max-width: 80%;">${html}</div>`;
        container.appendChild(wrapper);
        container.scrollTop = container.scrollHeight;
        return wrapper;
    }

    async function sendPrompt() {
        const promptBox = document.getElementById('chatPrompt');
        const prompt = promptBox.value.trim();
        if (!prompt) return;

        appendMessage(escapeHtml(prompt), true);
        promptBox.value = '';
        const loadingBubble = appendMessage('Pensando...', false);

        const result = await Aelbry.api.post('/AiAssistant/Suggest', { prompt });

        loadingBubble.remove();

        if (result.result !== 'OK') {
            appendMessage(`<span class="text-danger">${escapeHtml(result.result)}</span>`, false);
            return;
        }

        lastSuggestion = result.data;
        const treeHtml = renderPhases(lastSuggestion.phases);
        appendMessage(`
            <div><strong>${escapeHtml(lastSuggestion.suggestedProjectName ?? '')}</strong></div>
            <div class="small text-muted mb-2">${escapeHtml(lastSuggestion.suggestedProjectDescription ?? '')}</div>
            ${treeHtml}
            <button class="btn btn-sm btn-success mt-2" onclick="AiAssistant.insertSelection()">Insertar seleccionadas</button>
        `, false);
    }

    function renderPhases(phases) {
        return phases.map((phase, pIndex) => renderNode(phase, `${pIndex}`)).join('');
    }

    function renderNode(node, path) {
        const childrenHtml = (node.subtasks ?? []).map((child, i) => renderNode(child, `${path}.${i}`)).join('');
        return `
            <div class="form-check">
                <input class="form-check-input" type="checkbox" id="ainode_${path}" checked
                       onchange="AiAssistant.toggleNode('${path}', this.checked)" />
                <label class="form-check-label" for="ainode_${path}">
                    <strong>${escapeHtml(node.name)}</strong>
                    ${node.estimatedHours ? `<span class="text-muted small"> (${node.estimatedHours}h${node.suggestedRole ? ', ' + escapeHtml(node.suggestedRole) : ''})</span>` : ''}
                </label>
            </div>
            <div class="ms-4">${childrenHtml}</div>`;
    }

    function getNodeByPath(path) {
        const indexes = path.split('.').map((n) => parseInt(n, 10));
        let node = lastSuggestion.phases[indexes[0]];
        for (let i = 1; i < indexes.length; i++) {
            node = node.subtasks[indexes[i]];
        }
        return node;
    }

    function setDescendantsCheckbox(node, path, checked) {
        (node.subtasks ?? []).forEach((child, i) => {
            const childPath = `${path}.${i}`;
            child.selected = checked;
            const checkbox = document.getElementById(`ainode_${childPath}`);
            if (checkbox) {
                checkbox.checked = checked;
                checkbox.disabled = !checked;
            }
            setDescendantsCheckbox(child, childPath, checked);
        });
    }

    function toggleNode(path, checked) {
        const node = getNodeByPath(path);
        node.selected = checked;
        setDescendantsCheckbox(node, path, checked);
    }

    async function insertSelection() {
        if (!lastSuggestion) return;

        const isNew = document.getElementById('targetNew').checked;
        const payload = {
            selectedPhases: lastSuggestion.phases,
        };

        if (isNew) {
            payload.companyId = parseInt(document.getElementById('aiNewCompanyId').value, 10);
            payload.newProjectCode = document.getElementById('aiNewProjectCode').value;
            payload.newProjectName = document.getElementById('aiNewProjectName').value;
            payload.newProjectStatusId = parseInt(document.getElementById('aiNewProjectStatusId').value, 10);
        } else {
            payload.projectId = parseInt(document.getElementById('aiExistingProjectId').value, 10);
        }

        const result = await Aelbry.api.post('/AiAssistant/InsertSuggestions', payload);

        if (result.result === 'OK') {
            appendMessage(`Se crearon ${result.data.insertedActivitiesCount} actividades en el proyecto #${result.data.projectId}.`, false);
            lastSuggestion = null;
        } else {
            appendMessage(`<span class="text-danger">${escapeHtml(result.result)}</span>`, false);
        }
    }

    async function bulkCreate() {
        const projectId = parseInt(document.getElementById('bulkProjectId').value, 10);
        const parentActivityIdRaw = document.getElementById('bulkParentActivityId').value;
        const lines = document.getElementById('bulkLines').value.split('\n');

        const payload = {
            projectId,
            parentActivityId: parentActivityIdRaw ? parseInt(parentActivityIdRaw, 10) : null,
            lines,
        };

        const result = await Aelbry.api.post('/Activity/BulkCreate', payload);
        const resultBox = document.getElementById('bulkResult');

        if (result.result === 'OK') {
            resultBox.innerHTML = `<div class="alert alert-success">Se crearon ${result.data.length} actividades.</div>`;
        } else {
            resultBox.innerHTML = `<div class="alert alert-danger">${escapeHtml(result.result)}</div>`;
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text ?? '';
        return div.innerHTML;
    }

    // ---- Importacion desde Excel ----
    async function previewExcel() {
        const fileInput = document.getElementById('excelFile');
        const resultBox = document.getElementById('excelResult');
        resultBox.innerHTML = '';

        if (!fileInput.files.length) {
            resultBox.innerHTML = '<div class="alert alert-warning">Selecciona un archivo .xlsx.</div>';
            return;
        }

        const formData = new FormData();
        formData.append('file', fileInput.files[0]);

        const result = await Aelbry.api.postForm('/Activity/ImportExcelPreview', formData);

        if (result.result !== 'OK') {
            resultBox.innerHTML = `<div class="alert alert-danger">${escapeHtml(result.result)}</div>`;
            return;
        }

        lastExcelToken = result.data.token;
        lastExcelColumns = result.data.columns;

        document.getElementById('excelRowCount').textContent = `${result.data.totalRows} filas detectadas. Elige que columna corresponde a cada campo:`;

        const optionsHtml = `<option value="">(no usar)</option>` + lastExcelColumns.map((c) => `<option value="${escapeHtml(c)}">${escapeHtml(c)}</option>`).join('');
        document.getElementById('excelMappingFields').innerHTML = MAPPING_FIELDS.map((f) => `
            <div class="col-md-4 mb-2">
                <label class="form-label">${f.label}</label>
                <select class="form-select form-select-sm" id="${f.id}">${optionsHtml}</select>
            </div>`).join('');

        const sampleTable = document.getElementById('excelSampleTable');
        const headerHtml = `<thead><tr>${lastExcelColumns.map((c) => `<th>${escapeHtml(c)}</th>`).join('')}</tr></thead>`;
        const bodyHtml = `<tbody>${result.data.sampleRows.map((row) => `<tr>${lastExcelColumns.map((c) => `<td>${escapeHtml(row[c])}</td>`).join('')}</tr>`).join('')}</tbody>`;
        sampleTable.innerHTML = headerHtml + bodyHtml;

        document.getElementById('excelMappingSection').classList.remove('d-none');
    }

    async function commitExcel() {
        const payload = {
            token: lastExcelToken,
            projectId: parseInt(document.getElementById('excelProjectId').value, 10),
            parentActivityId: document.getElementById('excelParentActivityId').value
                ? parseInt(document.getElementById('excelParentActivityId').value, 10)
                : null,
            mapping: {
                nameColumn: document.getElementById('mapName').value,
                descriptionColumn: document.getElementById('mapDescription').value,
                estimatedHoursColumn: document.getElementById('mapEstimatedHours').value,
                categoryColumn: document.getElementById('mapCategory').value,
                responsibleColumn: document.getElementById('mapResponsible').value,
            },
        };

        const result = await Aelbry.api.post('/Activity/ImportExcelCommit', payload);
        const resultBox = document.getElementById('excelResult');

        if (result.result === 'OK') {
            const warnings = result.data.warnings.length
                ? `<ul>${result.data.warnings.map((w) => `<li>${escapeHtml(w)}</li>`).join('')}</ul>`
                : '';
            resultBox.innerHTML = `<div class="alert alert-success">Se crearon ${result.data.createdCount} actividades.${warnings}</div>`;
            document.getElementById('excelMappingSection').classList.add('d-none');
        } else {
            resultBox.innerHTML = `<div class="alert alert-danger">${escapeHtml(result.result)}</div>`;
        }
    }

    return {
        toggleTarget, loadStatuses, sendPrompt, toggleNode, insertSelection, bulkCreate,
        previewExcel, commitExcel,
    };
})();
