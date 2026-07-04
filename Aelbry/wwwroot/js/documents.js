window.DocFiles = (function () {
    let documentModal, folderModal, historyModal;
    let currentDocumentId = null;
    let currentIsFile = false;

    function projectId() {
        return document.getElementById('filterProjectId').value;
    }

    function formatBytes(bytes) {
        if (!bytes) return '0 B';
        const units = ['B', 'KB', 'MB', 'GB'];
        let i = 0;
        let value = bytes;
        while (value >= 1024 && i < units.length - 1) {
            value /= 1024;
            i++;
        }
        return `${value.toFixed(value < 10 && i > 0 ? 1 : 0)} ${units[i]}`;
    }

    async function loadAll() {
        const pid = projectId();
        if (!pid) return;

        closeEditor();
        await loadDocuments();
        await loadFolders();
        await loadFiles();
    }

    async function loadDocuments() {
        const pid = projectId();
        const result = await Aelbry.api.get(`/Document/GetByProject?projectId=${pid}`);
        const rows = document.getElementById('documentRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((d) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${d.title}</td>
                <td>v${d.latestVersionNumber}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-primary" onclick="DocFiles.openDocument(${d.documentId})">Abrir</button>
                    <button class="btn btn-sm btn-outline-danger" onclick="DocFiles.deleteDocument(${d.documentId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    async function createDocument() {
        const title = document.getElementById('newDocumentTitle').value.trim();
        if (!title) return;

        const result = await Aelbry.api.post('/Document/Create', { projectId: parseInt(projectId(), 10), title, contentMarkdown: `# ${title}\n` });
        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('newDocumentModal'))?.hide();
            document.getElementById('newDocumentTitle').value = '';
            await loadDocuments();
            openDocument(result.data.documentId);
        } else {
            alert(result.result);
        }
    }

    async function deleteDocument(documentId) {
        if (!confirm('Eliminar este documento?')) return;
        const result = await Aelbry.api.post(`/Document/Delete?documentId=${documentId}`);
        if (result.result === 'OK') loadDocuments();
        else alert(result.result);
    }

    async function openDocument(documentId) {
        const docResult = await Aelbry.api.get(`/Document/GetById?documentId=${documentId}`);
        const versionResult = await Aelbry.api.get(`/Document/GetLatestVersion?documentId=${documentId}`);
        if (docResult.result !== 'OK' || versionResult.result !== 'OK') { alert(docResult.result || versionResult.result); return; }

        currentDocumentId = documentId;
        document.getElementById('editorTitle').value = docResult.data.title;
        document.getElementById('editorContent').value = versionResult.data.contentMarkdown;
        renderPreview();

        document.getElementById('documentListView').classList.add('d-none');
        document.getElementById('documentEditorView').classList.remove('d-none');
    }

    function closeEditor() {
        currentDocumentId = null;
        document.getElementById('documentEditorView').classList.add('d-none');
        document.getElementById('documentListView').classList.remove('d-none');
    }

    function renderPreview() {
        const content = document.getElementById('editorContent').value;
        document.getElementById('editorPreview').innerHTML = window.marked ? marked.parse(content) : content;
    }

    async function saveVersion() {
        if (!currentDocumentId) return;

        const title = document.getElementById('editorTitle').value.trim();
        const content = document.getElementById('editorContent').value;

        await Aelbry.api.post('/Document/UpdateTitle', { documentId: currentDocumentId, title });
        const result = await Aelbry.api.post('/Document/SaveNewVersion', { documentId: currentDocumentId, contentMarkdown: content });

        if (result.result === 'OK') {
            alert(`Guardado como version ${result.data.versionNumber}`);
        } else {
            alert(result.result);
        }
    }

    async function openHistory() {
        currentIsFile = false;
        if (!currentDocumentId) return;

        const result = await Aelbry.api.get(`/Document/GetVersions?documentId=${currentDocumentId}`);
        const rows = document.getElementById('historyRows');
        rows.innerHTML = '';

        if (result.result === 'OK') {
            result.data.forEach((v) => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `<span>Version ${v.versionNumber} - ${new Date(v.createdDate).toLocaleString()}</span>
                    <button class="btn btn-sm btn-outline-secondary">Ver</button>`;
                li.querySelector('button').addEventListener('click', () => {
                    document.getElementById('editorContent').value = v.contentMarkdown;
                    renderPreview();
                    historyModal.hide();
                });
                rows.appendChild(li);
            });
        }

        historyModal = historyModal || new bootstrap.Modal(document.getElementById('historyModal'));
        historyModal.show();
    }

    // ---------------- Archivos ----------------

    function folderId() {
        const val = document.getElementById('filterFolderId').value;
        return val ? parseInt(val, 10) : null;
    }

    function buildFolderOptions(folders, parentId, depth, select) {
        folders.filter((f) => (f.parentFolderId ?? null) === parentId).forEach((f) => {
            const opt = document.createElement('option');
            opt.value = f.fileFolderId;
            opt.textContent = `${'  '.repeat(depth)}${f.name}`;
            select.appendChild(opt);
            buildFolderOptions(folders, f.fileFolderId, depth + 1, select);
        });
    }

    async function loadFolders() {
        const pid = projectId();
        const result = await Aelbry.api.get(`/File/GetFolders?projectId=${pid}`);
        const select = document.getElementById('filterFolderId');
        const previousValue = select.value;
        select.innerHTML = '<option value="">Raiz</option>';

        if (result.result === 'OK') {
            buildFolderOptions(result.data, null, 0, select);
        }

        select.value = previousValue;
    }

    async function createFolder() {
        const name = document.getElementById('newFolderName').value.trim();
        if (!name) return;

        const result = await Aelbry.api.post('/File/CreateFolder', { projectId: parseInt(projectId(), 10), parentFolderId: folderId(), name });
        if (result.result === 'OK') {
            bootstrap.Modal.getInstance(document.getElementById('newFolderModal'))?.hide();
            document.getElementById('newFolderName').value = '';
            await loadFolders();
        } else {
            alert(result.result);
        }
    }

    async function loadFiles() {
        const pid = projectId();
        if (!pid) return;

        const fid = folderId();
        const url = fid ? `/File/GetAttachments?projectId=${pid}&fileFolderId=${fid}` : `/File/GetAttachments?projectId=${pid}`;
        const result = await Aelbry.api.get(url);
        const rows = document.getElementById('fileRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((f) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td>${f.fileName}</td>
                <td>v${f.latestVersionNumber}</td>
                <td>${formatBytes(f.latestFileSizeBytes)}</td>
                <td class="text-end">
                    <button class="btn btn-sm btn-outline-secondary" onclick="DocFiles.downloadFile(${f.fileAttachmentId})">Descargar</button>
                    <button class="btn btn-sm btn-outline-secondary" onclick="DocFiles.openFileHistory(${f.fileAttachmentId})">Historial</button>
                    <label class="btn btn-sm btn-outline-primary mb-0">
                        Nueva version
                        <input type="file" class="d-none" onchange="DocFiles.uploadNewVersion(${f.fileAttachmentId}, this)" />
                    </label>
                    <button class="btn btn-sm btn-outline-danger" onclick="DocFiles.deleteFile(${f.fileAttachmentId})">Eliminar</button>
                </td>`;
            rows.appendChild(tr);
        });
    }

    async function downloadBlob(url, suggestedName) {
        const token = Aelbry.api.getAccessToken();
        const response = await fetch(url, { headers: token ? { Authorization: `Bearer ${token}` } : {} });
        if (!response.ok) { alert('No se pudo descargar el archivo'); return; }

        const blob = await response.blob();
        const objectUrl = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = objectUrl;
        a.download = suggestedName || 'archivo';
        document.body.appendChild(a);
        a.click();
        a.remove();
        URL.revokeObjectURL(objectUrl);
    }

    function downloadFile(fileAttachmentId) {
        downloadBlob(`/File/DownloadLatest?fileAttachmentId=${fileAttachmentId}`);
    }

    async function uploadFile() {
        const input = document.getElementById('uploadFileInput');
        const file = input.files[0];
        if (!file) return;

        const pid = projectId();
        const fid = folderId();
        const formData = new FormData();
        formData.append('file', file);

        let url = `/File/Upload?projectId=${pid}`;
        if (fid) url += `&fileFolderId=${fid}`;

        const result = await Aelbry.api.postForm(url, formData);
        input.value = '';

        if (result.result === 'OK') loadFiles();
        else alert(result.result);
    }

    async function uploadNewVersion(fileAttachmentId, inputEl) {
        const file = inputEl.files[0];
        if (!file) return;

        const formData = new FormData();
        formData.append('file', file);

        const result = await Aelbry.api.postForm(`/File/UploadNewVersion?fileAttachmentId=${fileAttachmentId}`, formData);
        inputEl.value = '';

        if (result.result === 'OK') loadFiles();
        else alert(result.result);
    }

    async function deleteFile(fileAttachmentId) {
        if (!confirm('Eliminar este archivo?')) return;
        const result = await Aelbry.api.post(`/File/DeleteAttachment?fileAttachmentId=${fileAttachmentId}`);
        if (result.result === 'OK') loadFiles();
        else alert(result.result);
    }

    async function openFileHistory(fileAttachmentId) {
        currentIsFile = true;
        const result = await Aelbry.api.get(`/File/GetVersions?fileAttachmentId=${fileAttachmentId}`);
        const rows = document.getElementById('historyRows');
        rows.innerHTML = '';

        if (result.result === 'OK') {
            result.data.forEach((v) => {
                const li = document.createElement('li');
                li.className = 'list-group-item d-flex justify-content-between align-items-center';
                li.innerHTML = `<span>Version ${v.versionNumber} - ${v.originalFileName} - ${formatBytes(v.fileSizeBytes)} - ${new Date(v.uploadedDate).toLocaleString()}</span>
                    <button class="btn btn-sm btn-outline-secondary">Descargar</button>`;
                li.querySelector('button').addEventListener('click', () => {
                    downloadBlob(`/File/Download?fileAttachmentVersionId=${v.fileAttachmentVersionId}`, v.originalFileName);
                });
                rows.appendChild(li);
            });
        }

        historyModal = historyModal || new bootstrap.Modal(document.getElementById('historyModal'));
        historyModal.show();
    }

    return {
        loadAll, createDocument, deleteDocument, openDocument, closeEditor, renderPreview, saveVersion, openHistory,
        loadFiles, createFolder, downloadFile, uploadFile, uploadNewVersion, deleteFile, openFileHistory,
    };
})();
