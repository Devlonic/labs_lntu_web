document.addEventListener("DOMContentLoaded", function () {

    const apiBaseUrl = "/api/hosts";

    let hostModal;

    $(document).ready(function () {
        hostModal = new bootstrap.Modal(document.getElementById("hostModal"));

        setInterval(() => {
            loadHosts();
        }, 1000);

        $(document).on("click", "#btnAdd", function () {
            $("#hostModalLabel").text("Create host");
            hostModal.show();
        });

        $(document).on("click", "#btnSaveHost", function () {
            saveHost();
        });

        $("#hostsTableBody").on("click", ".btn-edit", function () {
            const id = $(this).data("id");
            editHost(id);
        }).on("click", ".btn-delete", function () {
            const id = $(this).data("id");
            deleteHost(id);
        });
    });

    function fetchWithTimeout(url, options = {}, timeoutMs = 1000) {
        const controller = new AbortController();
        const timeoutId = setTimeout(() => {
            controller.abort();
        }, timeoutMs);

        const finalOptions = {
            ...options,
            signal: controller.signal
        };

        return fetch(url, finalOptions)
            .catch(err => {
                if (err.name === 'AbortError') {
                    $("#overloadStatus").removeClass("d-none");
                    throw new Error("Request timed out");
                }
            })
            .finally(() => {
                clearTimeout(timeoutId);
            });
    }

    function loadHosts() {
        const statusEl = $("#serverStatus");
        fetchWithTimeout(`${apiBaseUrl}/Statuses`)
            .then(r => {
                if (!r.ok) throw new Error("Failed to load hosts");
                return r.json();
            })
            .then(data => {
                statusEl.addClass("text-success");
                statusEl.removeClass("text-danger");
                statusEl.text("Online");
                renderTable(data);
            })
            .catch(err => {
                statusEl.removeClass("text-success");
                statusEl.addClass("text-danger");
                $("#serverStatus").text("Offline");
                console.error(err);
            });
    }

    function renderTable(hosts) {
        const $tbody = $("#hostsTableBody");
        $tbody.empty();
        let html = "";
        hosts.forEach(h => {
            const statusClass = h.hostStatusMessage == "Online" ? "text-success" :
                h.hostStatus == "Delayed" ? "text-warning" : "text-danger";
            const tr = `
                <tr>
                    <td>${h.id}</td>
                    <td>${escapeHtml(h.remoteAddress)}</td>
                    <td>${escapeHtml(h.name)}</td>
                    <td class='${statusClass}'>${h.hostStatusMessage}</td>
                    <td>${h.totalSuccess}</td>
                    <td>${h.totalFailure}</td>
                    <td>${h.totalRequests}</td>
                    <td>${h.averageTimeMilisec}</td>
                    <td>${h.roundTripTimeMs}</td>
                    <td>${h.sequenceOfFailures}</td>
                    <td>
                        <button class="btn btn-sm btn-outline-secondary btn-edit" data-id="${h.id}">Edit</button>
                        <button class="btn btn-sm btn-outline-danger btn-delete ms-1" data-id="${h.id}">Delete</button>
                    </td>
                </tr>`;
            html += tr;
        });
        $tbody.html(html);
    }

    function editHost(id) {
        fetchWithTimeout(`${apiBaseUrl}/${id}`)
            .then(r => {
                if (r.status === 404) throw new Error("Not found");
                if (!r.ok) throw new Error("Failed to load host");
                return r.json();
            })
            .then(h => {
                $("#hostModalLabel").text("Edit host");
                $("#hostId").val(h.id);
                $("#hostName").val(h.name);
                $("#hostAddress").val(h.remoteAddress);


                const enabledElem = $("#hostEnabled");
                enabledElem.val(h.enabled ? "on" : "off");
                if (!h.enabled)
                    enabledElem.removeAttr("checked");
                else
                    enabledElem.attr("checked", "checked");
                hostModal.show();
            })
            .catch(err => {
                console.error(err);
                alert("Error loading host");
            });
    }

    function saveHost() {
        const id = $("#hostId").val();
        const hostName = $("#hostName").val().trim();
        const remoteAddress = $("#hostAddress").val().trim();
        const enabled = $("#hostEnabled").val() == "on" ? true : false;

        $("#hostError").text("");

        if (!hostName || !remoteAddress) {
            $("#hostError").text("Name and Address are required");
            return;
        }

        const host = { id: id ? parseInt(id) : 0, hostName, remoteAddress, enabled };

        const isEdit = !!id;
        const url = isEdit ? `${apiBaseUrl}/${id}` : apiBaseUrl;
        const method = isEdit ? "PUT" : "POST";

        fetchWithTimeout(url, {
            method: method,
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify(host)
        })
            .then(r => {
                if (!r.ok) return r.text().then(t => { throw new Error(t || "Save error"); });
                return r.json();
            })
            .then(() => {
                hostModal.hide();
            })
            .catch(err => {
                console.error(err);
                $("#hostError").text("Error saving host");
            });
    }

    function deleteHost(id) {
        if (!confirm("Delete this host?")) return;

        fetchWithTimeout(`${apiBaseUrl}/${id}`, {
            method: "DELETE"
        })
            .then(r => {
                if (!r.ok && r.status !== 204) throw new Error("Delete error");
            })
            .catch(err => {
                console.error(err);
                alert("Error deleting host");
            });
    }

    //function show

    function escapeHtml(text) {
        if (text == null) return "";
        return text.toString()
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/"/g, "&#039;");
    }
});
