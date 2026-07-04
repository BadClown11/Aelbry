window.AuditLogPage = (function () {
    const ACTION_LABELS = {
        CREATE: 'Creacion', UPDATE: 'Modificacion', DELETE: 'Eliminacion',
        STATUS_CHANGE: 'Cambio de estado', ASSIGN: 'Asignacion', REMOVE: 'Remocion',
    };

    let detailModal;
    let rowsCache = [];

    function prettyJson(text) {
        if (!text) return '(vacio)';
        try {
            return JSON.stringify(JSON.parse(text), null, 2);
        } catch {
            return text;
        }
    }

    async function loadAll() {
        const companyId = document.getElementById('filterCompanyId').value;
        if (!companyId) return;

        const params = new URLSearchParams();
        params.set('companyId', companyId);

        const module = document.getElementById('filterModule').value;
        const userId = document.getElementById('filterUserId').value;
        const startDate = document.getElementById('filterStartDate').value;
        const endDate = document.getElementById('filterEndDate').value;

        if (module) params.set('module', module);
        if (userId) params.set('userId', userId);
        if (startDate) params.set('startDate', startDate);
        if (endDate) params.set('endDate', endDate);

        const result = await Aelbry.api.get(`/AuditLog/GetByCompany?${params.toString()}`);
        const rows = document.getElementById('auditRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        rowsCache = result.data;

        result.data.forEach((a, index) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${new Date(a.createdDate).toLocaleString()}</td>
                <td>${a.userName} <span class="text-muted small">#${a.userId}</span></td>
                <td class="small text-muted">${a.ipAddress ?? ''}</td>
                <td>${a.module}</td>
                <td>${ACTION_LABELS[a.action] ?? a.action}</td>
                <td>${a.entityId ?? ''}</td>
                <td class="text-end"><button class="btn btn-sm btn-outline-secondary" onclick="AuditLogPage.openDetail(${index})">Ver detalle</button></td>`;
            rows.appendChild(tr);
        });
    }

    function openDetail(index) {
        const a = rowsCache[index];
        document.getElementById('detailBefore').textContent = prettyJson(a.dataBefore);
        document.getElementById('detailAfter').textContent = prettyJson(a.dataAfter);

        detailModal = detailModal || new bootstrap.Modal(document.getElementById('detailModal'));
        detailModal.show();
    }

    return { loadAll, openDetail };
})();
