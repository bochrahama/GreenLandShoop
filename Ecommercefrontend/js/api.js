"use strict";
// ---- Configuration ----------------------------------------------------------
// [CONFIRM] Copy the https address from Properties/launchSettings.json (profile "https").
const API_BASE = "https://localhost:7208/api";
// Keys used in localStorage.
const TOKEN_KEY = "onearth_token";
const CART_COUNT_KEY = "onearth_cart_count";
//getToken is for getting the token from local storage, setToken is for setting the token in local storage,and clearToken is for removing the token from local storage.
// parseJwt is for decoding the JWT token to get its payload, isTokenExpired checks if the token has expired based on its "exp" claim, 
function getToken() {
    try { return localStorage.getItem(TOKEN_KEY); } catch { return null; }
}
function setToken(token) { localStorage.setItem(TOKEN_KEY, token); }
function clearToken() { localStorage.removeItem(TOKEN_KEY); }
// A JWT is three base64url parts separated by dots; the middle one is the JSON payload.
function parseJwt(token) {
    try {
        const base64 = token.split(".")[1].replace(/-/g, "+").replace(/_/g, "/");
        const bytes = Uint8Array.from(atob(base64), (c) => c.charCodeAt(0));
        return JSON.parse(new TextDecoder().decode(bytes)); // TextDecoder keeps accents in names
    } catch {
        return null;
    }
}
//isTokenExpired checks if the token has expired based on its "exp" claim,
// which is in seconds since the epoch. It multiplies by 1000 to convert to milliseconds for comparison with Date.now().
function isTokenExpired(token) {
    const payload = parseJwt(token);
    if (!payload || !payload.exp) return true;
    return payload.exp * 1000 <= Date.now();
}
function isLoggedIn() {
    const token = getToken();
    return Boolean(token) && !isTokenExpired(token);
}

//Error handling function to handle errors from fetch requests. It checks if the response is not ok, and if so, it throws an error with the status text. Otherwise, it returns the JSON response.

class ApiError extends Error {
    constructor(status, message, details = null) {
        super(message);
        this.name = "ApiError";
        this.status = status; // 0 means "no HTTP response": server down, CORS, certificate, mixed content
        this.details = details; // the parsed response body, if any further details are needed (e.g., validation errors)
    }
}
//extractErrorMessage is a utility function that extracts a human-readable error message from the response body of an API request.
// It handles different formats of error responses, including strings, arrays, and objects, and provides default messages for common HTTP status codes.
function extractErrorMessage(body, status) {
    if (typeof body === "string" && body.trim()) return body;// this line mean that if the body is a non-empty string, it will return that string as the error message. The trim() method is used to remove any leading or trailing whitespace from the string before checking if it's empty. If the body is a non-empty string, it is assumed to be a valid error message and is returned directly.
    if (Array.isArray(body)) { 
        const text = body.map((e) => e.description || e.message || "").filter(Boolean).join(" "); //the condition for filter(Boolean) is used to remove any empty strings from the array before joining them into a single string. This ensures that only non-empty error messages are included in the final error message.
        if (text) return text;
    }
    if (body && typeof body === "object") {
        if (body.errors && typeof body.errors === "object") {
            const text = Object.values(body.errors).flat().join(" "); //the condition for filter(Boolean) is used to remove any empty strings from the array before joining them into a single string. This ensures that only non-empty error messages are included in the final error message.
            if (text) return text;
        }
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

//apiFetch is a function that performs an API request using the Fetch API. It takes a path and an options object as parameters,
//constructs the request with appropriate headers (including authorization if needed), and handles the response. If the response is not ok,
//it throws an ApiError with the status and extracted error message.
// If the response is successful, it returns the parsed JSON data or null for a 204 No Content response.
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
function buildQuery(params) {
    const query = new URLSearchParams();
    Object.entries(params).forEach(([key, value]) => {
        if (value !== undefined && value !== null && value !== "") query.append(key, value);
    });
    const text = query.toString();
    return text ? `?${text}` : "";
}