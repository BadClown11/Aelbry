window.ChatPage = (function () {
    const NAME_ID_CLAIM = 'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier';
    const EMOJIS = ['👍', '❤️', '😂', '🎉'];

    let connection = null;
    let currentUserId = null;
    let currentChannel = null; // { type: 'project'|'conversation', id }
    let replyingToId = null;
    let typingTimeout = null;
    const onlineUsers = new Set();

    function decodeCurrentUserId() {
        const token = Aelbry.api.getAccessToken();
        if (!token) return null;

        try {
            const payloadBase64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/');
            const payload = JSON.parse(decodeURIComponent(atob(payloadBase64).split('').map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2)).join('')));
            return parseInt(payload[NAME_ID_CLAIM] ?? payload.nameid ?? payload.sub, 10);
        } catch {
            return null;
        }
    }

    function escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text ?? '';
        return div.innerHTML;
    }

    async function init() {
        currentUserId = decodeCurrentUserId();

        connection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/chat', { accessTokenFactory: () => Aelbry.api.getAccessToken() })
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveMessage', (message) => appendMessage(message));
        connection.on('MessageDeleted', (chatMessageId) => {
            const el = document.querySelector(`[data-message-id="${chatMessageId}"]`);
            if (el) el.remove();
        });
        connection.on('ReactionsUpdated', (chatMessageId, reactions) => renderReactions(chatMessageId, reactions));
        connection.on('UserOnline', (userId) => { onlineUsers.add(userId); updateOnlineIndicator(); });
        connection.on('UserOffline', (userId) => { onlineUsers.delete(userId); updateOnlineIndicator(); });
        connection.on('UserTyping', (userId, isTyping) => {
            document.getElementById('typingIndicator').textContent = isTyping ? `Usuario #${userId} esta escribiendo...` : '';
        });

        await connection.start();
        updateOnlineIndicator();
        await loadConversations();
    }

    function updateOnlineIndicator() {
        document.getElementById('onlineIndicator').textContent = `En linea: ${onlineUsers.size} usuario(s)`;
    }

    async function loadConversations() {
        const result = await Aelbry.api.get('/Chat/GetConversations');
        const rows = document.getElementById('conversationRows');
        rows.innerHTML = '';

        if (result.result !== 'OK') return;

        result.data.forEach((c) => {
            const li = document.createElement('li');
            li.className = 'list-group-item list-group-item-action';
            li.style.cursor = 'pointer';
            li.textContent = c.otherUserName;
            li.onclick = () => enterChannel('conversation', c.conversationId, c.otherUserName);
            rows.appendChild(li);
        });
    }

    async function openProjectChannel() {
        const projectId = document.getElementById('channelProjectId').value;
        if (!projectId) return;
        await enterChannel('project', parseInt(projectId, 10), `Proyecto #${projectId}`);
    }

    async function openDirectMessage() {
        const otherUserId = document.getElementById('dmUserId').value;
        if (!otherUserId) return;

        const result = await Aelbry.api.post(`/Chat/GetOrCreateConversation?otherUserId=${otherUserId}`);
        if (result.result !== 'OK') { alert(result.result); return; }

        await enterChannel('conversation', result.data, `Usuario #${otherUserId}`);
        await loadConversations();
    }

    async function enterChannel(type, id, label) {
        if (currentChannel) {
            const leaveMethod = currentChannel.type === 'project' ? 'LeaveProjectChannel' : 'LeaveConversation';
            await connection.invoke(leaveMethod, currentChannel.id);
        }

        currentChannel = { type, id };
        replyingToId = null;
        document.getElementById('replyingTo').classList.add('d-none');
        document.getElementById('channelTitle').textContent = label;

        const joinMethod = type === 'project' ? 'JoinProjectChannel' : 'JoinConversation';
        await connection.invoke(joinMethod, id);

        const historyUrl = type === 'project'
            ? `/Chat/GetProjectMessages?projectId=${id}`
            : `/Chat/GetDirectMessages?conversationId=${id}`;

        const result = await Aelbry.api.get(historyUrl);
        const container = document.getElementById('chatMessages');
        container.innerHTML = '';

        if (result.result === 'OK') {
            result.data.forEach((m) => appendMessage(m, false));
        }
        container.scrollTop = container.scrollHeight;
    }

    function appendMessage(message, scroll = true) {
        if (!currentChannel) return;
        const belongsHere = currentChannel.type === 'project'
            ? message.projectId === currentChannel.id
            : message.conversationId === currentChannel.id;
        if (!belongsHere) return;

        const container = document.getElementById('chatMessages');
        const isOwn = message.senderUserId === currentUserId;
        const div = document.createElement('div');
        div.className = `chat-bubble ${isOwn ? 'own' : ''}`;
        div.dataset.messageId = message.chatMessageId;

        const reactionButtons = EMOJIS.map((e) => `<button class="chat-reaction-btn" onclick="ChatPage.toggleReaction(${message.chatMessageId}, '${e}')">${e}</button>`).join('');

        div.innerHTML = `
            <div class="d-flex justify-content-between gap-2">
                <span><strong>${escapeHtml(message.senderName)}</strong> <span class="text-muted small">${new Date(message.createdDate).toLocaleTimeString()}</span></span>
                ${isOwn ? `<button class="btn btn-sm btn-link text-danger p-0 chat-actions" onclick="ChatPage.deleteMessage(${message.chatMessageId})">Eliminar</button>` : ''}
            </div>
            <div>${escapeHtml(message.text)}</div>
            <div class="small chat-actions">
                ${reactionButtons}
                <button class="btn btn-sm btn-link p-0 ms-2" onclick="ChatPage.replyTo(${message.chatMessageId})">Responder</button>
            </div>
            <div class="small" id="reactions-${message.chatMessageId}"></div>`;

        container.appendChild(div);
        renderReactions(message.chatMessageId, message.reactions ?? []);

        if (scroll) container.scrollTop = container.scrollHeight;
    }

    function renderReactions(chatMessageId, reactions) {
        const el = document.getElementById(`reactions-${chatMessageId}`);
        if (!el) return;

        const byEmoji = {};
        reactions.forEach((r) => {
            byEmoji[r.emoji] = byEmoji[r.emoji] ?? [];
            byEmoji[r.emoji].push(r);
        });

        el.innerHTML = Object.entries(byEmoji).map(([emoji, list]) => `
            <span class="badge ${list.some((r) => r.userId === currentUserId) ? 'text-bg-primary' : 'text-bg-light border'}">${emoji} ${list.length}</span>
        `).join(' ');
    }

    async function toggleReaction(chatMessageId, emoji) {
        const reactedBefore = Array.from(document.querySelectorAll(`#reactions-${chatMessageId} .badge`))
            .some((b) => b.textContent.trim().startsWith(emoji) && b.classList.contains('text-bg-primary'));

        if (reactedBefore) {
            await connection.invoke('RemoveReaction', chatMessageId, emoji);
        } else {
            await connection.invoke('AddReaction', chatMessageId, emoji);
        }
    }

    function replyTo(chatMessageId) {
        replyingToId = chatMessageId;
        const box = document.getElementById('replyingTo');
        box.textContent = `Respondiendo al mensaje #${chatMessageId} (click para cancelar)`;
        box.classList.remove('d-none');
        box.onclick = () => { replyingToId = null; box.classList.add('d-none'); };
    }

    async function deleteMessage(chatMessageId) {
        if (!confirm('Eliminar este mensaje?')) return;
        await connection.invoke('DeleteMessage', chatMessageId);
    }

    async function sendMessage() {
        if (!currentChannel) return;
        const input = document.getElementById('chatInput');
        const text = input.value.trim();
        if (!text) return;

        const method = currentChannel.type === 'project' ? 'SendProjectMessage' : 'SendDirectMessage';
        await connection.invoke(method, currentChannel.id, text, replyingToId);

        input.value = '';
        replyingToId = null;
        document.getElementById('replyingTo').classList.add('d-none');
    }

    function onInputKeyDown(event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            sendMessage();
        }
    }

    function onInputTyping() {
        if (!currentChannel) return;
        const method = currentChannel.type === 'project' ? 'NotifyTypingInProject' : 'NotifyTypingInConversation';

        connection.invoke(method, currentChannel.id, true);

        if (typingTimeout) clearTimeout(typingTimeout);
        typingTimeout = setTimeout(() => connection.invoke(method, currentChannel.id, false), 2000);
    }

    document.addEventListener('DOMContentLoaded', () => {
        if (document.getElementById('chatMessages')) init();
    });

    return {
        openProjectChannel, openDirectMessage, sendMessage, onInputKeyDown, onInputTyping,
        toggleReaction, replyTo, deleteMessage,
    };
})();
