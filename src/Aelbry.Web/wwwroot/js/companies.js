window.Companies = (function () {
    let modal;

    async function loadAll() {
        const result = await Aelbry.api.get('/Company/GetAll');
        const rows = document.getElementById('companyRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((c) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${c.name}</td>
                <td>${c.legalTaxId ?? ''}</td>
                <td>${c.timeZone}</td>
                <td>${c.isActive ? '<span class="badge text-bg-success">Si</span>' : '<span class="badge text-bg-secondary">No</span>'}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-primary" onclick='Companies.openEdit(${JSON.stringify(c)})'>Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="Companies.remove(${c.companyId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    function openCreate() {
        document.getElementById('companyId').value = '';
        document.getElementById('companyName').value = '';
        document.getElementById('companyTaxId').value = '';
        document.getElementById('companyTimeZone').value = 'UTC';
    }

    function openEdit(company) {
        document.getElementById('companyId').value = company.companyId;
        document.getElementById('companyName').value = company.name;
        document.getElementById('companyTaxId').value = company.legalTaxId ?? '';
        document.getElementById('companyTimeZone').value = company.timeZone;
        modal = modal || new bootstrap.Modal(document.getElementById('companyModal'));
        modal.show();
    }

    async function save() {
        const id = document.getElementById('companyId').value;
        const payload = {
            companyId: id ? parseInt(id, 10) : 0,
            name: document.getElementById('companyName').value,
            legalTaxId: document.getElementById('companyTaxId').value,
            timeZone: document.getElementById('companyTimeZone').value,
            isActive: true,
        };

        const result = id
            ? await Aelbry.api.post('/Company/Update', payload)
            : await Aelbry.api.post('/Company/Create', payload);

        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('companyModal'))?.hide();
            loadAll();
        } else {
            alert(result.result);
        }
    }

    async function remove(companyId) {
        if (!confirm('Eliminar esta empresa?')) return;
        const result = await Aelbry.api.post(`/Company/Delete?companyId=${companyId}`);
        if (result.result === 'OK') loadAll();
        else alert(result.result);
    }

    document.addEventListener('DOMContentLoaded', loadAll);

    return { openCreate, openEdit, save, remove };
})();
