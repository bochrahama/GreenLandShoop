
(function () {
  "use strict";

  function ready(fn) {
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", fn, { once: true });
    else fn();
  }

  // ---- 1. Mobile menu ------------------------------------------------------------------
  // style.css shows the nav on small screens when it has the class "open" (.bar nav.open).
  function initMenu() {
    var header = document.querySelector("header.bar");
    if (!header || header.hasAttribute("data-shared-layout")) return; // injected headers wire themselves (js/ui.js)
    var button = header.querySelector(".menu");
    var nav = header.querySelector("nav");
    if (!button || !nav) return;

    function setOpen(open) {
      nav.classList.toggle("open", open);
      button.setAttribute("aria-expanded", String(open));
    }

    button.addEventListener("click", function () {
      setOpen(!nav.classList.contains("open"));
    });
    nav.addEventListener("click", function (event) {   // choosing a link closes the menu
      if (event.target.closest("a")) setOpen(false);
    });
    document.addEventListener("keydown", function (event) {
      if (event.key === "Escape" && nav.classList.contains("open")) {
        setOpen(false);
        button.focus();
      }
    });
  }

  // The backend has no newsletter endpoint yet, so this only validates the address and thanks the visitor.
  // When an endpoint exists, call it where marked below.
  function initNewsletter() {
    document.querySelectorAll("footer form").forEach(function (form) {
      if (form.closest("[data-shared-layout]")) return; // already wired by js/ui.js
      var input = form.querySelector('input[type="email"], input[name="email"]');
      var output = form.querySelector("output");
      if (!input || !output) return;

      form.addEventListener("submit", function (event) {
        event.preventDefault();
        var email = input.value.trim();
        if (!/^\S+@\S+\.\S+$/.test(email)) {
          output.textContent = "Please enter a valid email address.";
          input.focus();
          return;
        }
        // TODO: POST the address to the API here once a newsletter endpoint exists.
        output.textContent = "Thanks for subscribing!";
        form.reset();
      });
    });
  }

  ready(function () {
    initMenu();
    initNewsletter();
  });
})();
