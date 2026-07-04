window.TimeEntries = (function () {
    let tickHandle = null;
    let runningEntry = null;

    function formatElapsed(startTime) {
        const seconds = Math.max(0, Math.floor((Date.now() - new Date(startTime).getTime()) / 1000));
        const h = String(Math.floor(seconds / 3600)).padStart(2, '0');
        const m = String(Math.floor((seconds % 3600) / 60)).padStart(2, '0');
        const s = String(seconds % 60).padStart(2, '0');
        return `${h}:${m}:${s}`;
    }

    function showRunning(entry) {
        runningEntry = entry;
        document.getElementById('timerIdle').classList.add('d-none');
        document.getElementById('timerRunning').classList.remove('d-none');
        document.getElementById('timerRunning').classList.add('d-flex');
        document.getElementById('timerActivityLabel').textContent = `${entry.activityName} (actividad #${entry.activityId})`;

        if (tickHandle) clearInterval(tickHandle);
        tickHandle = setInterval(() => {
            document.getElementById('timerElapsed').textContent = formatElapsed(entry.startTime);
        }, 1000);
        document.getElementById('timerElapsed').textContent = formatElapsed(entry.startTime);
    }

    function showIdle() {
        runningEntry = null;
        if (tickHandle) { clearInterval(tickHandle); tickHandle = null; }
        document.getElementById('timerRunning').classList.add('d-none');
        document.getElementById('timerRunning').classList.remove('d-flex');
        document.getElementById('timerIdle').classList.remove('d-none');
    }

    async function checkRunning() {
        const result = await Aelbry.api.get('/TimeEntry/GetRunning');
        if (result.result === 'OK' && result.data) {
            showRunning(result.data);
        } else {
            showIdle();
        }
    }

    async function start() {
        const activityId = document.getElementById('timerActivityId').value;
        if (!activityId) return;

        const result = await Aelbry.api.post(`/TimeEntry/Start?activityId=${activityId}`);
        if (result.result === 'OK') {
            await checkRunning();
        } else {
            alert(result.result);
        }
    }

    async function stop() {
        if (!runningEntry) return;

        const result = await Aelbry.api.post(`/TimeEntry/Stop?timeEntryId=${runningEntry.timeEntryId}`);
        if (result.result === 'OK') {
            await checkRunning();
            await loadLog();
        } else {
            alert(result.result);
        }
    }

    async function createManual() {
        const payload = {
            activityId: parseInt(document.getElementById('manualActivityId').value, 10),
            startTime: document.getElementById('manualDate').value,
            durationHours: parseFloat(document.getElementById('manualHours').value || '0'),
            isOvertime: document.getElementById('manualOvertime').checked,
            notes: document.getElementById('manualNotes').value,
        };

        if (!payload.activityId || !payload.startTime || !payload.durationHours) {
            alert('Actividad, fecha y horas son obligatorias.');
            return;
        }

        const result = await Aelbry.api.post('/TimeEntry/CreateManual', payload);
        if (result.result === 'OK') {
            document.getElementById('manualHours').value = '';
            document.getElementById('manualNotes').value = '';
            document.getElementById('manualOvertime').checked = false;
            await loadLog();
        } else {
            alert(result.result);
        }
    }

    async function loadLog() {
        const startDate = document.getElementById('logStartDate').value;
        const endDate = document.getElementById('logEndDate').value;
        const query = new URLSearchParams();
        if (startDate) query.set('startDate', startDate);
        if (endDate) query.set('endDate', endDate);

        const result = await Aelbry.api.get(`/TimeEntry/GetMine?${query.toString()}`);
        const rows = document.getElementById('logRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((e) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${e.activityName} <span class="text-muted small">#${e.activityId}</span></td>
                <td>${new Date(e.startTime).toLocaleString()}</td>
                <td>${e.durationHours}</td>
                <td>${e.isManual ? 'Manual' : 'Cronometro'}${e.isOvertime ? ' <span class="badge text-bg-warning">Extra</span>' : ''}</td>
                <td>${e.notes ?? ''}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-primary" onclick='TimeEntries.edit(${JSON.stringify(e)})'>Editar</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="TimeEntries.remove(${e.timeEntryId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    async function edit(entry) {
        const hours = prompt('Horas:', entry.durationHours);
        if (hours === null) return;
        const overtime = confirm('¿Marcar como hora extra? Aceptar = Si, Cancelar = No');
        const notes = prompt('Notas:', entry.notes ?? '') ?? '';

        const result = await Aelbry.api.post(`/TimeEntry/Update?timeEntryId=${entry.timeEntryId}&durationHours=${parseFloat(hours)}&isOvertime=${overtime}&notes=${encodeURIComponent(notes)}`);
        if (result.result === 'OK') loadLog();
        else alert(result.result);
    }

    async function remove(timeEntryId) {
        if (!confirm('Eliminar este registro de tiempo?')) return;
        const result = await Aelbry.api.post(`/TimeEntry/Delete?timeEntryId=${timeEntryId}`);
        if (result.result === 'OK') loadLog();
        else alert(result.result);
    }

    document.addEventListener('DOMContentLoaded', () => {
        if (!document.getElementById('timerIdle')) return;
        document.getElementById('manualDate').value = new Date().toISOString().substring(0, 10);
        checkRunning();
        loadLog();
    });

    return { start, stop, createManual, loadLog, edit, remove };
})();
