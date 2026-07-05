window.Departments = (function () {
    let departmentModal, teamModal;

    function companyId() {
        return document.getElementById('filterCompanyId').value;
    }

    async function loadAll() {
        const cid = companyId();
        if (!cid) return;

        const result = await Aelbry.api.get(`/Department/GetByCompany?companyId=${cid}`);
        const container = document.getElementById('departmentList');
        container.innerHTML = '';

        if (result.result !== 'OK') return;

        if (result.data.length === 0) {
            container.innerHTML = '<div class="aelbry-empty-state">No hay departamentos para esta empresa.</div>';
            return;
        }

        result.data.forEach((dept) => {
            const card = document.createElement('div');
            card.className = 'card mb-3';
            card.innerHTML = `
                <div class="card-header d-flex justify-content-between align-items-center">
                    <strong>${dept.name}</strong>
                    <div class="d-flex gap-2">
                        <button class="btn btn-sm btn-outline-secondary" onclick="Departments.openCreateTeam(${dept.departmentId})">+ Equipo</button>
                        <button class="btn btn-sm btn-outline-primary" onclick="Departments.openEditDepartment(${dept.departmentId})">Editar</button>
                        <button class="btn btn-sm btn-outline-danger" onclick="Departments.removeDepartment(${dept.departmentId})">Eliminar</button>
                    </div>
                </div>
                <div class="card-body">
                    <ul class="list-group" id="teamsFor${dept.departmentId}">
                        <li class="list-group-item text-muted small">Cargando equipos...</li>
                    </ul>
                </div>`;
            container.appendChild(card);
            loadTeams(dept.departmentId);
        });
    }

    async function loadTeams(departmentId) {
        const result = await Aelbry.api.get(`/Team/GetByDepartment?departmentId=${departmentId}`);
        const list = document.getElementById(`teamsFor${departmentId}`);
        if (!list) return;

        list.innerHTML = '';

        if (result.result !== 'OK' || result.data.length === 0) {
            list.innerHTML = '<li class="list-group-item text-muted small">Sin equipos todavia.</li>';
            return;
        }

        result.data.forEach((team) => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `
                <span>${team.name}</span>
                <div class="d-flex gap-2">
                    <button class="btn btn-sm btn-outline-primary" onclick="Departments.openEditTeam(${team.teamId}, ${departmentId})">Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Departments.removeTeam(${team.teamId}, ${departmentId})">Eliminar</button>
                </div>`;
            list.appendChild(li);
        });
    }

    function openCreateDepartment() {
        document.getElementById('departmentId').value = '';
        document.getElementById('departmentName').value = '';
        document.getElementById('departmentModalTitle').textContent = 'Nuevo departamento';
    }

    async function openEditDepartment(departmentId) {
        const result = await Aelbry.api.get(`/Department/GetById?departmentId=${departmentId}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        document.getElementById('departmentId').value = result.data.departmentId;
        document.getElementById('departmentName').value = result.data.name;
        document.getElementById('departmentModalTitle').textContent = 'Editar departamento';

        departmentModal = departmentModal || new bootstrap.Modal(document.getElementById('departmentModal'));
        departmentModal.show();
    }

    async function saveDepartment() {
        const id = document.getElementById('departmentId').value;
        const payload = {
            departmentId: id ? parseInt(id, 10) : 0,
            companyId: parseInt(companyId(), 10),
            name: document.getElementById('departmentName').value,
        };

        const result = id
            ? await Aelbry.api.post('/Department/Update', payload)
            : await Aelbry.api.post('/Department/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('departmentModal'))?.hide();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function removeDepartment(departmentId) {
        if (!confirm('Eliminar este departamento?')) return;
        const result = await Aelbry.api.post(`/Department/Delete?departmentId=${departmentId}`);
        if (result.result === 'OK') loadAll();
        else alert(result.result);
    }

    function openCreateTeam(departmentId) {
        document.getElementById('teamId').value = '';
        document.getElementById('teamDepartmentId').value = departmentId;
        document.getElementById('teamName').value = '';
        document.getElementById('teamModalTitle').textContent = 'Nuevo equipo';

        teamModal = teamModal || new bootstrap.Modal(document.getElementById('teamModal'));
        teamModal.show();
    }

    async function openEditTeam(teamId, departmentId) {
        const result = await Aelbry.api.get(`/Team/GetById?teamId=${teamId}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        document.getElementById('teamId').value = result.data.teamId;
        document.getElementById('teamDepartmentId').value = departmentId;
        document.getElementById('teamName').value = result.data.name;
        document.getElementById('teamModalTitle').textContent = 'Editar equipo';

        teamModal = teamModal || new bootstrap.Modal(document.getElementById('teamModal'));
        teamModal.show();
    }

    async function saveTeam() {
        const id = document.getElementById('teamId').value;
        const departmentId = document.getElementById('teamDepartmentId').value;
        const payload = {
            teamId: id ? parseInt(id, 10) : 0,
            departmentId: parseInt(departmentId, 10),
            name: document.getElementById('teamName').value,
        };

        const result = id
            ? await Aelbry.api.post('/Team/Update', payload)
            : await Aelbry.api.post('/Team/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('teamModal'))?.hide();
            loadTeams(departmentId);
        } else {
            alert(result.result);
        }
    }

    async function removeTeam(teamId, departmentId) {
        if (!confirm('Eliminar este equipo?')) return;
        const result = await Aelbry.api.post(`/Team/Delete?teamId=${teamId}`);
        if (result.result === 'OK') loadTeams(departmentId);
        else alert(result.result);
    }

    return {
        loadAll, openCreateDepartment, openEditDepartment, saveDepartment, removeDepartment,
        openCreateTeam, openEditTeam, saveTeam, removeTeam,
    };
})();
