window.Users = (function () {
    async function loadAll() {
        const companyId = document.getElementById('filterCompanyId').value;
        if (!companyId) return;

        const result = await Aelbry.api.get(`/User/GetByCompany?companyId=${companyId}`);
        const rows = document.getElementById('userRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((u) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${u.firstName} ${u.lastName}</td>
                <td>${u.email}</td>
                <td>${u.jobTitle ?? ''}</td>
                <td>${u.isActive ? '<span class="badge text-bg-success">Si</span>' : '<span class="badge text-bg-secondary">No</span>'}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-danger" onclick="Users.remove(${u.userId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    function openCreate() {
        document.getElementById('userCompanyId').value = document.getElementById('filterCompanyId').value || '';
        document.getElementById('userFirstName').value = '';
        document.getElementById('userLastName').value = '';
        document.getElementById('userEmail').value = '';
        document.getElementById('userJobTitle').value = '';
        document.getElementById('userPassword').value = '';
    }

    async function save() {
        const payload = {
            user: {
                companyId: parseInt(document.getElementById('userCompanyId').value, 10),
                firstName: document.getElementById('userFirstName').value,
                lastName: document.getElementById('userLastName').value,
                email: document.getElementById('userEmail').value,
                jobTitle: document.getElementById('userJobTitle').value,
                timeZone: 'UTC',
                isActive: true,
            },
            password: document.getElementById('userPassword').value,
        };

        const result = await Aelbry.api.post('/User/Create', payload);
        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('userModal'))?.hide();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function remove(userId) {
        if (!confirm('Eliminar este usuario?')) return;
        const result = await Aelbry.api.post(`/User/Delete?userId=${userId}`);
        if (result.result === 'OK') loadAll();
        else alert(result.result);
    }

    return { loadAll, openCreate, save, remove };
})();
