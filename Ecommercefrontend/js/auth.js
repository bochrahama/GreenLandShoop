"use strict";

//Claim names used in JWT tokens for extracting user information. CLAIM_NAME is an array of possible claim names for the user's name,
//and CLAIM_ROLE is an array of possible claim names for the user's role.
// These claim names are used to retrieve the corresponding values from the decoded JWT payload.
const CLAIM_NAME = ["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name", "unique_name", "name"];
const CLAIM_ROLE = ["http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "role"];

//read claim is for reading specific claims from the JWT token payload.
// It takes an array of keys and returns the value of the first matching claim found in the payload. If no matching claim is found, it returns undefined.
function readClaim(keys) {
    const payload = parseJwt(getToken() || "");
    if (!payload) return undefined;
    for (const key of keys) if (payload[key] !== undefined) return payload[key];
    return undefined;
}
//getUserName retrieves the user's name from the JWT token by reading the CLAIM_NAME claim. If no name is found, it returns an empty string.
function getUserName() { return readClaim(CLAIM_NAME) || ""; }
function getUserRoles() {
    const role = readClaim(CLAIM_ROLE);
    return Array.isArray(role) ? role : role ? [role] : [];
}


async function login(email, password) {
    const result = await api.post("/auth/login", { email, password }); // public call
    setToken(result.token);
}
// Your register endpoint returns { message } and NO token, so we log in right after it.
async function register(fullName, email, password) {
    await api.post("/auth/register", { fullName, email, password });
    await login(email, password);
}
function logout() {
    clearToken();
    localStorage.removeItem(CART_COUNT_KEY);
    window.location.href = "index.html";
}
// Call at the top of any protected page. Returns false (and redirects) when not logged in.
function requireLogin() {
    if (isLoggedIn()) return true;
    redirectToLogin(getToken() ? "expired" : "required");
    return false;
}
//safe Next is a function that validates the "next" parameter in the URL to ensure it is a safe and valid path
//for redirection after login or registration.It checks if the value matches
// a specific pattern(a valid HTML file name with optional query parameters) and returns the value if valid; otherwise, it defaults to "index.html".
function safeNext(value) {
    return /^[a-z0-9_-]+\.html(\?[\w=&%.-]*)?$/i.test(value || "") ? value : "index.html";
}

function updateNavbar() {
    const loggedIn = isLoggedIn();
    document.querySelectorAll("[data-guest]").forEach((el) => { el.hidden = loggedIn; });
    document.querySelectorAll("[data-user]").forEach((el) => { el.hidden = !loggedIn; });
    const logoutLink = document.getElementById("logout-link");
    if (logoutLink && !logoutLink.dataset.bound) { // attach the listener only once
        logoutLink.dataset.bound = "1";
        logoutLink.addEventListener("click", (event) => {
            event.preventDefault();
            logout();
        });
    }
}







function setFormError(box, message) {
    box.textContent = message;
    box.hidden = !message;
}
//it's for extracting error messages from API responses based on the status code.
//It takes the response data and status code as parameters and returns a user-friendly error message.
async function submitAuthForm(form, action) {
    const box = document.getElementById("form-error");
    const button = form.querySelector("button[type=submit]");

    const params = new URLSearchParams(location.search);// this line means extract all the query parameters from the current URL
    button.disabled = true;
    setFormError(box, "");
    try {
        await action();
        location.href = safeNext(params.get("next"));
    } catch (error) {
        setFormError(box, error.message);
        button.disabled = false;
    }
    //initLoginForm initializes the login form on the page. It checks for the presence of the login form element and sets up event listeners for form submission.
    //It also handles displaying error messages based on query parameters in the URL (e.g., "expired" or "required") and redirects logged-in users to the appropriate page.

    function initLoginForm() {
        const form = document.getElementById("login-form");
        if (!form) return;
        const params = new URLSearchParams(location.search);
        const box = document.getElementById("form-error");
        if (params.get("reason") === "expired") setFormError(box, "Your session expired. Please log in again.");
        if (params.get("reason") === "required") setFormError(box, "Please log in to continue.");
        if (isLoggedIn()) { location.href = safeNext(params.get("next")); return; }
        clearToken(); // remove any stale token
        form.addEventListener("submit", (event) => {
            event.preventDefault();
            submitAuthForm(form, () => login(form.email.value.trim(), form.password.value));
        });
    }
    function initRegisterForm() {
        const form = document.getElementById("register-form");
        if (!form) return;
        form.addEventListener("submit", (event) => {
            event.preventDefault();
            const box = document.getElementById("form-error");
            if (form.password.value !== form.confirm.value) {
                setFormError(box, "The two passwords do not match.");
                return;
            }
            submitAuthForm(form, () =>
                register(form.fullName.value.trim(), form.email.value.trim(), form.password.value));
        });
    }
    document.addEventListener("DOMContentLoaded", () => {
        updateNavbar();
        initLoginForm();
        initRegisterForm();
    }
    );
}