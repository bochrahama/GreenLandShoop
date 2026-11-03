/* app.js — shared header/footer, auth token, API helper */
const API = 'https://localhost:7208/api';   // set to e.g. 'http://localhost:8080' if your backend is on another origin
const $ = (s, r = document) => r.querySelector(s);
const money = n => '$' + Number(n || 0).toFixed(2);
const token = () => localStorage.getItem('token');

async function api(path, { method = 'GET', body, auth = true } = {}) {
  const res = await fetch(API + path, {
    method,
    headers: { 'Content-Type': 'application/json', ...(token() ? { Authorization: 'Bearer ' + token() } : {}) },
    body: body ? JSON.stringify(body) : undefined,
  });
  let data = null;
  try { data = await res.json(); } catch {}
  if (res.status === 401 && auth) {
    localStorage.removeItem('token');
    location.href = 'login.html?next=' + encodeURIComponent(location.pathname.split('/').pop() + location.search);
  }
  if (!res.ok) throw new Error(data?.message || data?.error || `Request failed (${res.status})`);
  return data;
}

/* Accept either an array or { items: [...] } and flatten item.product */
function cartItems(d) {
  return (Array.isArray(d) ? d : d?.items || []).map(it => {
    const p = it.product || it;
    return {
      id: it.id ?? it.productId ?? p.id,
      name: p.name || p.title || 'Product',
      price: Number(p.price ?? it.price ?? 0),
      image: p.image || p.imageUrl || '',
      quantity: Number(it.quantity ?? 1),
    };
  });
}

const esc = s => String(s ?? '').replace(/[&<>"']/g, c => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
const img = (src, alt = '') => src ? `<img src="${esc(src)}" alt="${esc(alt)}" loading="lazy">` : '';

const $$ = (s, r = document) => [...r.querySelectorAll(s)];
const setCount = n => $$('.count').forEach(c => c.textContent = n);
const bumpCount = n => setCount(Number($('.count')?.textContent || 0) + n);

const HEADER = `
<header class="bar">
  <a href="index.html"><img src="images/logo/logo.png" alt="GreenLand Shop logo"><b>GreenLand Shop</b></a>
  <nav aria-label="Main">
    <a href="index.html">Home</a>
    <a href="fashion.html">Fashion</a>
    <a href="skincare.html">Skincare</a>
    <a href="floor.html">Floor</a>
  </nav>
  <span><a aria-label="Search" href="index.html#best">Search</a><a aria-label="Cart" href="cart.html">Cart <i class="count">0</i></a><button aria-expanded="false" aria-label="Menu" class="menu">☰</button></span>
</header>`;

const FOOTER = `
<footer class="foot">
  <div><b>GREENLAND SHOP</b><p>Anything. Anywhere. On Earth. Sustainable products inspired by nature and made to last.</p></div>
  <nav aria-label="Shop"><h4>EXPLORE</h4><a href="fashion.html">Fashion</a><a href="skincare.html">Skincare</a><a href="floor.html">Floor Decor &amp; Living</a></nav>
  <nav aria-label="Account"><h4>ACCOUNT</h4><a href="login.html">Sign in</a><a href="register.html">Create account</a><a href="orders.html">Orders</a></nav>
  <form novalidate>
    <h4 class="join">Join our community</h4>
    <input name="email" type="email" placeholder="Email address" aria-label="Email address">
    <button>Subscribe</button><output></output>
  </form>
  <small><span>© 2026 GreenLand Shoop. All rights reserved.</span><span>Instagram · Facebook · Twitter</span></small>
</footer>`;

function layout() {
  // Pages without their own header (login, cart, ...) get the shared one
  if (!$('.bar')) {
    document.body.insertAdjacentHTML('afterbegin', HEADER);
    $('script[src="script.js"]')?.insertAdjacentHTML('beforebegin', FOOTER) || document.body.insertAdjacentHTML('beforeend', FOOTER);
  }
  // Every page: cart link, account link, cart count
  const cart = $('a[aria-label="Cart"]');
  if (cart) {
    cart.href = 'cart.html';
    cart.insertAdjacentHTML('beforebegin', token()
      ? '<a href="orders.html">Orders</a><a href="#" id="out">Sign out</a>'
      : '<a href="login.html">Sign in</a>');
  }
  $('#out')?.addEventListener('click', e => { e.preventDefault(); localStorage.removeItem('token'); location.href = 'index.html'; });
  if (token()) api('/api/cart').then(d => setCount(cartItems(d).reduce((a, i) => a + i.quantity, 0))).catch(() => {});
}

// Pages that need a logged-in user
if (!token() && /\/(cart|checkout|orders)\.html$/.test(location.pathname))
  location.replace('login.html?next=' + encodeURIComponent(location.pathname.split('/').pop()));

// "Add to Cart" on every product card (index, fashion, skincare, floor)
document.addEventListener('click', async e => {
  const btn = e.target.closest('.card button');
  const card = btn?.closest('.card[data-id]');
  if (!card) return;
  if (!token()) return location.href = 'login.html?next=' + encodeURIComponent(location.pathname.split('/').pop() + location.hash);
  const label = btn.textContent; btn.disabled = true;
  try {
    await api('/api/cart', { method: 'POST', body: { productId: Number(card.dataset.id), quantity: 1 } });
    bumpCount(1); btn.textContent = 'Added ✓';
  } catch (err) { btn.textContent = 'Try again'; console.error(err); }
  setTimeout(() => { btn.textContent = label; btn.disabled = false; }, 1500);
});

layout();
