window.Notifications = (function () {
    let connection = null;
    let items = [];

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text ?? '';
        return div.innerHTML;
    }

    function render() {
        const list = document.getElementById('notificationList');
        if (!list) return;

        list.innerHTML = items.length
            ? items.map((n) => `
                <li class="list-group-item small ${n.isRead ? '' : 'fw-semibold'}" style="cursor:pointer" onclick="Notifications.open(${n.notificationId}, ${JSON.stringify(n.link ?? '')})">
                    <div>${escapeHtml(n.title)}</div>
                    <div class="text-muted fw-normal">${escapeHtml(n.message)}</div>
                    <div class="text-muted fw-normal" style="font-size: 0.75rem;">${new Date(n.createdDate).toLocaleString()}</div>
                </li>`).join('')
            : '<li class="list-group-item small text-muted">Sin notificaciones</li>';

        const unreadCount = items.filter((n) => !n.isRead).length;
        const badge = document.getElementById('notificationBadge');
        badge.textContent = unreadCount;
        badge.classList.toggle('d-none', unreadCount === 0);
    }

    async function loadMine() {
        const result = await Aelbry.api.get('/Notification/GetMine');
        if (result.result === 'OK') {
            items = result.data;
            render();
        }
    }

    async function open(notificationId, link) {
        await Aelbry.api.post(`/Notification/MarkAsRead?notificationId=${notificationId}`);
        const item = items.find((n) => n.notificationId === notificationId);
        if (item) item.isRead = true;
        render();

        if (link) window.location.href = link;
    }

    async function markAllAsRead() {
        await Aelbry.api.post('/Notification/MarkAllAsRead');
        items.forEach((n) => { n.isRead = true; });
        render();
    }

    async function init() {
        if (!document.getElementById('notificationBell') || !Aelbry.api.isAuthenticated()) return;

        await loadMine();

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/notifications', { accessTokenFactory: () => Aelbry.api.getAccessToken() })
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveNotification', (notification) => {
            items.unshift(notification);
            render();
        });

        try {
            await connection.start();
        } catch {
            // Sin conexion en tiempo real, la campanita sigue funcionando via polling manual (recarga de pagina).
        }
    }

    document.addEventListener('DOMContentLoaded', init);

    return { open, markAllAsRead };
})();
