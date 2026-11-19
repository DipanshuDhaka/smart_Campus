// SmartCampus LPU - Real-time SignalR Notification Client & Institutional UI Scripts

document.addEventListener("DOMContentLoaded", function () {
    // 1. Initialize SignalR Connection if user is authenticated and signalR library is available
    if (typeof signalR !== "undefined") {
        const connection = new signalR.HubConnectionBuilder()
            .withUrl("/hubs/notifications")
            .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
            .build();

        // Handler for Targeted User Notifications
        connection.on("ReceiveNotification", function (data) {
            showLiveToast(data.title, data.message, data.type, data.url);
        });

        // Handler for Campus-Wide Broadcast Alerts
        connection.on("ReceiveCampusAlert", function (data) {
            showLiveToast("CAMPUS BROADCAST: " + data.title, data.message, "Warning", null);
        });

        connection.start()
            .then(function () {
                console.log("[SmartCampus] SignalR live notification hub connected.");
            })
            .catch(function (err) {
                // In unauthenticated views, hub returns 401; log gracefully
                console.log("[SmartCampus] Live notification hub idle (anonymous session or connecting).");
            });
    }

    // 2. Helper to display an institutional toast
    function showLiveToast(title, message, type, url) {
        const container = document.getElementById("liveToastContainer");
        if (!container) return;

        const headerClass = type === "Danger" ? "bg-danger text-white" :
                           (type === "Warning" ? "bg-warning text-dark" : "bg-primary text-white");

        const toastId = "toast_" + Date.now();
        const actionHtml = url ? `<div class="mt-2 pt-2 border-top"><a href="${url}" class="btn btn-sm btn-outline-dark py-0 px-2" style="font-size: 0.75rem;">View Record</a></div>` : "";

        const html = `
            <div id="${toastId}" class="toast shadow-lg" role="alert" aria-live="assertive" aria-atomic="true" data-bs-autohide="true" data-bs-delay="8000">
                <div class="toast-header ${headerClass} py-2">
                    <strong class="me-auto" style="font-size: 0.82rem;"><i class="bi bi-bell-fill me-1"></i>${title}</strong>
                    <small>Just now</small>
                    <button type="button" class="btn-close ${type === 'Warning' ? '' : 'btn-close-white'}" data-bs-dismiss="toast" aria-label="Close"></button>
                </div>
                <div class="toast-body small text-dark bg-white">
                    ${message}
                    ${actionHtml}
                </div>
            </div>`;

        container.insertAdjacentHTML("beforeend", html);
        const toastEl = document.getElementById(toastId);
        if (typeof bootstrap !== "undefined" && bootstrap.Toast) {
            const toast = new bootstrap.Toast(toastEl);
            toast.show();
        }
    }
});
