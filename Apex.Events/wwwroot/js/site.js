// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
// Lightweight API client + small UI helpers for Apex.Events
// - generic request wrapper for JSON API endpoints
// - wrappers for Catering endpoints used by the Razor Pages UI
// - simple table renderer and alert helper
// - auto-load helpers that run if target containers exist on the page

(function () {
  'use strict';

  // --- Configuration -------------------------------------------------------
  // If your Catering service runs on another origin/port, change these base paths.
  const config = {
    cateringBase: '/api', // e.g. "https://localhost:7012/api" or "/api"
    venuesBase: '/api'
  };

  // --- Utilities -----------------------------------------------------------
  function getRequestVerificationToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    if (input) return input.value;
    const meta = document.querySelector('meta[name="RequestVerificationToken"]');
    if (meta) return meta.content;
    return null;
  }

  function escapeHtml(s) {
    return String(s ?? '').replaceAll('&', '&amp;').replaceAll('<', '&lt;').replaceAll('>', '&gt;').replaceAll('"', '&quot;');
  }

  async function toJsonSafe(response) {
    const text = await response.text();
    return text ? JSON.parse(text) : null;
  }

  // --- HTTP / fetch wrapper -----------------------------------------------
  async function request(method, url, data = null, extraHeaders = {}) {
    const anti = getRequestVerificationToken();
    const headers = {
      'Accept': 'application/json',
      'Content-Type': 'application/json',
      ...extraHeaders
    };
    if (anti) headers['RequestVerificationToken'] = anti;

    const opts = {
      method,
      headers,
      credentials: 'same-origin' // send cookies if present
    };
    if (data !== null) opts.body = JSON.stringify(data);

    const res = await fetch(url, opts);
    if (!res.ok) {
      // try to parse problem details or text for better message
      let payload;
      try { payload = await toJsonSafe(res); } catch { payload = await res.text(); }
      const message = payload?.title ?? payload?.message ?? JSON.stringify(payload) ?? `${res.status} ${res.statusText}`;
      const err = new Error(message);
      err.status = res.status;
      err.payload = payload;
      throw err;
    }
    return await toJsonSafe(res);
  }

  // --- API wrappers (Catering & Venues) ----------------------------------
  const Api = {
    // Catering - FoodItems
    getFoodItems: () => request('GET', `${config.cateringBase}/FoodItems`),
    getFoodItem: (id) => request('GET', `${config.cateringBase}/FoodItems/${id}`),
    createFoodItem: (model) => request('POST', `${config.cateringBase}/FoodItems`, model),
    updateFoodItem: (id, model) => request('PUT', `${config.cateringBase}/FoodItems/${id}`, model),
    deleteFoodItem: (id) => request('DELETE', `${config.cateringBase}/FoodItems/${id}`),

    // Catering - Menus
    getMenus: () => request('GET', `${config.cateringBase}/Menus`),
    getMenu: (id) => request('GET', `${config.cateringBase}/Menus/${id}`),
    createMenu: (model) => request('POST', `${config.cateringBase}/Menus`, model),
    updateMenu: (id, model) => request('PUT', `${config.cateringBase}/Menus/${id}`, model),
    deleteMenu: (id) => request('DELETE', `${config.cateringBase}/Menus/${id}`),

    // Catering - MenuFoodItems
    getMenuFoodItems: () => request('GET', `${config.cateringBase}/MenuFoodItems`),
    addMenuFoodItem: (model) => request('POST', `${config.cateringBase}/MenuFoodItems`, model),
    deleteMenuFoodItem: (menuId, foodItemId) => request('DELETE', `${config.cateringBase}/MenuFoodItems/${menuId}/${foodItemId}`),

    // Catering - FoodBookings
    getFoodBookings: () => request('GET', `${config.cateringBase}/FoodBookings`),
    getFoodBooking: (id) => request('GET', `${config.cateringBase}/FoodBookings/${id}`),
    createFoodBooking: (model) => request('POST', `${config.cateringBase}/FoodBookings`, model),
    updateFoodBooking: (id, model) => request('PUT', `${config.cateringBase}/FoodBookings/${id}`, model),
    deleteFoodBooking: (id) => request('DELETE', `${config.cateringBase}/FoodBookings/${id}`),

    // Venues examples (if required)
    getEventTypes: () => request('GET', `${config.venuesBase}/EventTypes`),
    getAvailability: (query) => {
      // query: { eventType, beginDate, endDate? }
      const q = new URLSearchParams(query).toString();
      return request('GET', `${config.venuesBase}/Availability?${q}`);
    }
  };

  // --- Small UI helpers ---------------------------------------------------
  function showAlert(message, type = 'info', timeout = 6000) {
    const wrapper = document.createElement('div');
    wrapper.innerHTML = `<div class="alert alert-${escapeHtml(type)} alert-dismissible fade show" role="alert">
      ${escapeHtml(message)}
      <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    </div>`;
    document.body.prepend(wrapper);
    if (timeout > 0) setTimeout(() => wrapper.remove(), timeout);
  }

  function renderTable(containerOrSelector, columns, rows) {
    const container = (typeof containerOrSelector === 'string') ? document.querySelector(containerOrSelector) : containerOrSelector;
    if (!container) return;
    const table = document.createElement('table');
    table.className = 'table table-sm table-striped table-hover';
    const thead = document.createElement('thead');
    const headRow = document.createElement('tr');
    columns.forEach(c => {
      const th = document.createElement('th');
      th.textContent = c.label ?? c.key;
      headRow.appendChild(th);
    });
    thead.appendChild(headRow);
    const tbody = document.createElement('tbody');
    rows.forEach(row => {
      const tr = document.createElement('tr');
      columns.forEach(c => {
        const td = document.createElement('td');
        const value = (c.key.indexOf('.') === -1)
          ? row[c.key]
          : c.key.split('.').reduce((o, k) => (o ? o[k] : undefined), row);
        td.textContent = (typeof c.format === 'function') ? c.format(value, row) : (value ?? '');
        tr.appendChild(td);
      });
      tbody.appendChild(tr);
    });
    table.appendChild(thead);
    table.appendChild(tbody);
    container.innerHTML = '';
    container.appendChild(table);
  }

  // --- Page-oriented helpers (auto-run if containers exist) --------------
  async function loadFoodBookingsTable(selector = '#foodBookingsTable') {
    const el = document.querySelector(selector);
    if (!el) return;
    try {
      const bookings = await Api.getFoodBookings();
      renderTable(el, [
        { key: 'FoodBookingId', label: 'Id' },
        { key: 'Menus.MenuName', label: 'Menu' }, // depends on controller Include(m => m.Menus)
        { key: 'NumberOfGuests', label: 'Guests' }
      ], bookings);
    } catch (err) {
      console.error(err);
      showAlert('Failed to load food bookings: ' + (err.message ?? err), 'danger');
    }
  }

  async function loadFoodItemsTable(selector = '#foodItemsTable') {
    const el = document.querySelector(selector);
    if (!el) return;
    try {
      const items = await Api.getFoodItems();
      renderTable(el, [
        { key: 'FoodItemId', label: 'Id' },
        { key: 'Description', label: 'Description' },
        { key: 'UnitPrice', label: 'Price', format: v => v ? v.toFixed(2) : '' }
      ], items);
    } catch (err) {
      console.error(err);
      showAlert('Failed to load food items: ' + (err.message ?? err), 'danger');
    }
  }

  // Auto-run small loaders if page contains standard containers.
  document.addEventListener('DOMContentLoaded', () => {
    loadFoodBookingsTable();
    loadFoodItemsTable();
    // Add more auto-loaders as you create pages (e.g. menus, menuFoodItems, event types etc.)
  });

  // Expose to window for console and page scripts
  window.ApexSite = {
    Api,
    showAlert,
    renderTable,
    loadFoodBookingsTable,
    loadFoodItemsTable,
    configure: (opts) => Object.assign(config, opts)
  };
})();
