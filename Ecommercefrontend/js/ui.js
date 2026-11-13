"use strict";

// Escape text before putting it inside an HTML string (prevents XSS).
function escapeHtml(value) {
    const map = { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" };
    return String(value ?? "").replace(/[&<>"']/g, (ch) => map[ch]);
}

const currencyFormat = new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" });
function formatPrice(amount) { return currencyFormat.format(Number(amount) || 0); } // 299 -> "$299.00"

function formatDate(value) {
    const date = new Date(value);
    if (Number.isNaN(date.getTime())) return "";
    return date.toLocaleDateString("en-US", { year: "numeric", month: "short", day: "numeric" });
}

// Replaces the content of a container with a loading / empty / error message.
// kind: "loading" | "empty" | "error"
function renderState(container, kind, message) {
    const role = kind === "error" ? "alert" : "status";
    container.innerHTML = `<p class="state state--${kind}" role="${role}">${escapeHtml(message)}</p>`;
}

// A small message that fades out after 3 seconds. kind: "info" | "error"
function showToast(message, kind = "info") {
    let toast = document.getElementById("toast");
    if (!toast) {
        toast = document.createElement("div");
        toast.id = "toast";
        toast.setAttribute("role", "status");
        document.body.appendChild(toast);
    }
    toast.className = `toast toast--${kind} is-visible`;
    toast.textContent = message;
    clearTimeout(showToast.timer);
    showToast.timer = setTimeout(() => toast.classList.remove("is-visible"), 3000);
}