// Cliente de chat en tiempo real con SignalR

(function () {
    var data = document.getElementById('chatData');
    if (!data) return;

    var urlHub = data.dataset.hub;
    var jwtToken = data.dataset.token;
    var currentUserId = parseInt(data.dataset.userid, 10);
    var isDoctor = data.dataset.isdoctor === '1';

    var connection = null;
    var startPromise = null;
    var currentRoom = null;

    var STATUS_CLOSED = 3;

    function statusLabel(name) {
        switch (name) {
            case 'Open': return 'Abierta';
            case 'InProgress': return 'En progreso';
            case 'Closed': return 'Cerrada';
            default: return name;
        }
    }

    // ─── Conexion SignalR ───────────────────────────────
    function buildConnection() {
        connection = new signalR.HubConnectionBuilder()
            .withUrl(urlHub + '?access_token=' + jwtToken)
            .withAutomaticReconnect()
            .build();

        connection.on('ReceiveMessage', function (msg) {
            // Solo se pinta si el mensaje pertenece a la conversacion abierta
            if (msg.consultationId !== currentRoom) return;
            appendMessage(msg);
        });
        connection.on('ConsultationUpdated', function (update) { handleConsultationUpdated(update); });

        connection.onreconnecting(function () {
            if (currentRoom) setStatus('Reconectando…', 'text-warning');
        });
        connection.onreconnected(function () {
            joinAllRooms();
            if (currentRoom) setStatus('Conectado', 'text-success');
        });
    }

    // Inicia la conexion una sola vez y reutiliza la promesa
    function ensureStarted() {
        if (!connection) buildConnection();
        if (!startPromise) {
            startPromise = connection.start().catch(function (e) {
                startPromise = null;
                throw e;
            });
        }
        return startPromise;
    }

    // Se une a todas las conversaciones accesibles para recibir sus avisos
    async function joinAllRooms() {
        var items = document.querySelectorAll('.consultation-item[data-id]');
        for (var i = 0; i < items.length; i++) {
            try { await connection.invoke('JoinRoom', parseInt(items[i].dataset.id, 10)); } catch (e) { /* sin acceso */ }
        }
    }

    // ─── Seleccion de sala ──────────────────────────────
    function bindConsultations() {
        document.querySelectorAll('.consultation-item').forEach(function (el) {
            el.addEventListener('click', function () {
                openRoom(parseInt(el.dataset.id, 10), el.dataset.interlocutor, parseInt(el.dataset.status, 10));
            });
        });
    }

    async function openRoom(consultationId, interlocutorName, statusId) {
        document.querySelectorAll('.consultation-item').forEach(function (e) { e.classList.remove('active'); });
        var item = document.querySelector('.consultation-item[data-id="' + consultationId + '"]');
        if (item) item.classList.add('active');

        currentRoom = consultationId;

        document.getElementById('chatHeader').style.removeProperty('display');
        // El placeholder vive dentro de messagesArea y se elimina al limpiarlo, por eso se valida
        var placeholder = document.getElementById('chatPlaceholder');
        if (placeholder) placeholder.style.display = 'none';
        document.getElementById('chatInterlocutor').textContent = interlocutorName || 'Conversación';
        setStatus('Conectando…', 'text-warning');

        setClosedUi(statusId === STATUS_CLOSED, consultationId);

        document.getElementById('messagesArea').innerHTML = '';
        renderContext(item);

        try {
            await ensureStarted();
            await connection.invoke('JoinRoom', consultationId);
            setStatus('Conectado', 'text-success');
        } catch (e) {
            setStatus('Sin conexión', 'text-danger');
            return;
        }

        await loadHistory(consultationId);
    }

    // Pinta el contexto de la consulta (motivo, descripcion e indicador) al abrirla
    function renderContext(item) {
        if (!item) return;

        var title = item.dataset.title || '';
        var description = item.dataset.description || '';
        var measure = item.dataset.measure || '';

        if (!title && !description && !measure) return;

        var card = document.createElement('div');
        card.className = 'alert alert-light border small mb-3';

        var html = '';
        if (title) html += '<div class="mb-1"><span class="fw-semibold">Motivo:</span> ' + escapeHtml(title) + '</div>';
        if (description) html += '<div class="mb-1"><span class="fw-semibold">Descripción:</span> ' + escapeHtml(description) + '</div>';
        if (measure) html += '<div><span class="fw-semibold">Contexto:</span> ' + escapeHtml(measure) + '</div>';

        card.innerHTML = html;
        document.getElementById('messagesArea').appendChild(card);
    }

    // Habilita o bloquea la caja de envio segun si la consulta esta cerrada
    function setClosedUi(closed, consultationId) {
        var input = document.getElementById('messageInput');
        var sendBtn = document.getElementById('sendButton');
        var finalForm = document.getElementById('finalizarForm');

        if (closed) {
            input.disabled = true;
            input.placeholder = 'Consulta finalizada';
            sendBtn.disabled = true;
            finalForm.classList.add('d-none');
        } else {
            input.disabled = false;
            input.placeholder = 'Escribe un mensaje…';
            sendBtn.disabled = false;
            document.getElementById('finalizarConsultationId').value = consultationId;
            finalForm.classList.remove('d-none');
        }
    }

    // ─── Actualizacion de consulta (asignada / finalizada) ──
    function handleConsultationUpdated(update) {
        var item = document.querySelector('.consultation-item[data-id="' + update.consultationId + '"]');

        if (item) {
            item.dataset.status = update.statusId;

            var statusEl = item.querySelector('.ci-status');
            if (statusEl) statusEl.textContent = statusLabel(update.statusName);

            // El paciente ve el nombre del doctor cuando la consulta es asignada
            if (!isDoctor && update.doctorName) {
                item.dataset.interlocutor = update.doctorName;
                var interEl = item.querySelector('.ci-interlocutor');
                if (interEl) interEl.textContent = update.doctorName;
            }
        }

        if (currentRoom === update.consultationId) {
            if (!isDoctor && update.doctorName) {
                document.getElementById('chatInterlocutor').textContent = update.doctorName;
            }
            if (update.statusId === STATUS_CLOSED) {
                setClosedUi(true, update.consultationId);
            }
        }
    }

    // ─── Historial via REST ─────────────────────────────
    async function loadHistory(consultationId) {
        var res = await fetch('/Contacto/ConsultarMensajes?consultationId=' + consultationId);
        if (!res.ok) return;

        var messages = await res.json();

        // El usuario ya cambio de conversacion: se descarta este historial
        if (currentRoom !== consultationId) return;

        messages.forEach(function (m) { appendMessage(m, false); });
        scrollDown();
    }

    // ─── Envio de mensajes ──────────────────────────────
    async function sendMessage() {
        var input = document.getElementById('messageInput');
        var text = input.value.trim();

        if (!text || !currentRoom || (connection && connection.state !== signalR.HubConnectionState.Connected)) return;

        input.value = '';
        await connection.invoke('SendMessage', currentRoom, text);
    }

    // ─── Render ─────────────────────────────────────────
    function appendMessage(msg, animate) {
        if (animate === undefined) animate = true;

        var own = msg.senderUserId === currentUserId;
        var area = document.getElementById('messagesArea');

        var wrapper = document.createElement('div');
        wrapper.className = 'd-flex mb-2 ' + (own ? 'justify-content-end' : 'justify-content-start');

        var time = new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        wrapper.innerHTML =
            '<div class="chat-bubble ' + (own ? 'bubble-own' : 'bubble-other') + '">' +
            (!own ? '<div class="chat-sender">' + escapeHtml(msg.senderName) + '</div>' : '') +
            '<div>' + escapeHtml(msg.content) + '</div>' +
            '<div class="chat-time">' + time + '</div>' +
            '</div>';

        if (animate) wrapper.classList.add('chat-new');
        area.appendChild(wrapper);
        scrollDown();
    }

    function setStatus(text, cls) {
        var el = document.getElementById('chatStatus');
        el.textContent = text;
        el.className = 'small ' + cls;
    }

    function scrollDown() {
        var area = document.getElementById('messagesArea');
        area.scrollTop = area.scrollHeight;
    }

    function escapeHtml(text) {
        return (text || '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    document.addEventListener('DOMContentLoaded', function () {
        bindConsultations();

        var sendBtn = document.getElementById('sendButton');
        if (sendBtn) sendBtn.addEventListener('click', sendMessage);

        var input = document.getElementById('messageInput');
        if (input) {
            input.addEventListener('keydown', function (e) {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    sendMessage();
                }
            });
        }

        // Conecta al cargar y se une a todas las conversaciones para recibir avisos en vivo
        ensureStarted()
            .then(joinAllRooms)
            .catch(function (e) { console.error('No se pudo conectar al chat', e); });
    });
})();
