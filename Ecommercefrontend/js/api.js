"use strict";
// ---- Configuration ----------------------------------------------------------
// [CONFIRM] 
const API_BASE = "https://localhost:7208";
10
// Keys used in localStorage. A prefix avoids clashing with other sites on localhost.
const TOKEN_KEY = "onearth_token";
const CART_COUNT_KEY = "onearth_cart_count";
// ---- Token storage ----------------------------------------------------------
function getToken() {
    try { return localStorage.getItem(TOKEN_KEY); } catch { return null; }
}
//settoken is used to store the token in local storage, and clearToken is used to remove it.
function setToken(token) { localStorage.setItem(TOKEN_KEY, token); }
function clearToken() { localStorage.removeItem(TOKEN_KEY); }
// parseJwt decodes the JWT payload, which is base64url-encoded JSON. It returns null if the token is invalid.
//base64url-encoded JSON is a way of encoding JSON data in a compact format that can be safely transmitted in URLs.
//The parseJwt function takes a JWT token as input, decodes the payload, and returns the parsed JSON object.
//If the token is invalid or cannot be decoded, it returns null.
function parseJwt(token) {
    try {
        const base64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
        const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
        return JSON.parse(new TextDecoder().decode(bytes)); // TextDecoder keeps accents in names
    } catch {
        return null;
    }
}
// "exp" is in seconds since 1970. This is only a convenience check: the server is the real judge.
//isTokenExpired checks if the JWT token has expired by comparing the expiration time
//in the token's payload with the current time. It returns true if the token is expired or invalid, and false otherwise.
function isTokenExpired(token) {
    const payload = parseJwt(token);
    if (!payload || !payload.exp) return true;
    return payload.exp * 1000 <= Date.now();
}
function isLoggedIn() {
    const token = getToken();
    return Boolean(token) && !isTokenExpired(token);
}

// ---- Errors -----------------
//ApiError is a custom error class that extends the built-in Error class. It is used to represent errors that occur during API requests.
//The constructor takes three parameters: status (the HTTP status code), message (a descriptive error message), and details (optional additional information about the error).
// The class sets the name property to "ApiError" and assigns the provided values to the corresponding properties.
class ApiError extends Error {
    constructor(status, message, details = null) {
        super(message);
        this.name = "ApiError";
        this.status = status; // 0 means "no HTTP response": server down, CORS, certificate, mixed content
        this.details = details; // the parsed response body, if any
    }
}
// Your API returns errors in four different shapes. This function understands all of them:
// "Invalid email or password" (BadRequest(string))
// [ { "code": "...", "description": "..." } ] (Identity errors from register)
// { "title": "...", "errors": { "Field": ["msg"] } } (automatic [ApiController] validation)
// { "message": "..." } (custom exception middleware)
function extractErrorMessage(body, status) {
//if string mean s the body is a string and it is not empty, return the body as the error message.
    if (typeof body === "string" && body.trim()) return body;
    if (Array.isArray(body)) {
        const text = body.map((e) => e.description || e.message || "").filter(Boolean).join(" ");
        if (text) return text;
    }
//if the body is an object, check for different properties that may contain error messages and return the first one found.
    if (body && typeof body === "object") {
        if (body.errors && typeof body.errors === "object") {
            const text = Object.values(body.errors).flat().join(" ");
            if (text) return text;
        }
//otherwise, check for message, title, or detail properties and return the first one found.
        if (body.message) return body.message;
        if (body.title) return body.title;
        if (body.detail) return body.detail;
    }
    const defaults = {
        400: "The request was not valid.",
        401: "Please log in to continue.",
        403: "You are not allowed to do that.",
        404: "Not found.",
        500: "Server error. Please try again later.",
    };
    return defaults[status] || `Request failed (${status}).`;
}
function redirectToLogin(reason) {
    clearToken();
    const page = location.pathname.split("/").pop() || "index.html";
    if (page === "login.html") return; // avoid a redirect loop
    const next = encodeURIComponent(page + location.search);
    location.href = `login.html?next=${next}` + (reason ? `&reason=${reason}` : "");
}
// ---- The one function that talks HTTP ---------------------------------------
// options.auth send "Authorization: Bearer <token>" (default false, like the original plan)
// options.redirectOn401 on 401, clear the token and go to login.html (default true)

//apiFetch is an asynchronous function that performs an HTTP request to the specified API endpoint. It takes two parameters:
// path (the API endpoint path) and options (an optional object containing request options).
async function apiFetch(path, options = {}) {
    const { method = "GET", body, auth = false, redirectOn401 = true } = options;
    const headers = { Accept: "application/json" };
    if (body !== undefined) headers["Content-Type"] = "application/json";
    const token = auth ? getToken() : null;
    if (token) headers.Authorization = `Bearer ${token}`;
    let response;
    try {
        response = await fetch(API_BASE + path, {
            method,
            headers,
            body: body !== undefined ? JSON.stringify(body) : undefined,
        });
    } catch {
        // fetch() only rejects when NO response arrived. Open the browser console for the real reason.
        throw new ApiError(0, "Cannot reach the server. Is the API running? (See the console for CORS or certificate errors.)");
    }
    if (response.status === 204) return null; // No Content: nothing to parse
    const text = await response.text();
    let data = null;
    if (text) {
        try { data = JSON.parse(text); } catch { data = text; } // plain-text errors are not JSON
    }
    if (!response.ok) {
        if (response.status === 401 && auth && redirectOn401) redirectToLogin("expired");
        throw new ApiError(response.status, extractErrorMessage(data, response.status), data);
    }
    return data;
}
// Short helpers so page code reads like a sentence.
const api = {
    get: (path, opts = {}) => apiFetch(path, { ...opts, method: "GET" }),
    post: (path, body, opts = {}) => apiFetch(path, { ...opts, method: "POST", body }),
    put: (path, body, opts = {}) => apiFetch(path, { ...opts, method: "PUT", body }),
    del: (path, opts = {}) => apiFetch(path, { ...opts, method: "DELETE" }),
};
// buildQuery({ search: "oil", categoryId: 2, minPrice: "" }) -> "?search=oil&categoryId=2"
function buildQuery(params) {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") query.append(key, value);
    });
    const text = query.toString();
    return text ? `?${text}` : "";
}