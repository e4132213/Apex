// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Lightweight API client + small UI helpers for Apex.Events
// - generic request wrapper for JSON API endpoints
// - wrappers for Catering endpoints used by the Razor Pages UI
// - simple table / list renderer and alert helper
// - auto-load helpers that run if target containers exist on the page

(function () {
  'use strict';

  // --- Configuration -------------------------------------------------------
  const config = {
    cateringBase: '/api', // Update if Catering API runs elsewhere
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
      credentials: 'same-origin'
    };
    if (data !== null) opts.body = JSON.stringify(data);

    const res = await fetch(url, opts);
    if (!res.ok) {
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
    // FoodItems
    getFoodItems: () => request('GET', `${config.cateringBase}/FoodItems`),
    getFoodItem: (id) => request('GET', `${config.cateringBase}/FoodItems/${id}`),
    createFoodItem: (model) => request('POST', `${config.cateringBase}/FoodItems`, model),
    updateFoodItem: (id, model) => request('PUT', `${config.cateringBase}/FoodItems/${id}`, model),
    deleteFoodItem: (id) => request('DELETE', `${config.cateringBase}/FoodItems/${id}`),

    // Menus
    getMenus: () => request('GET', `${config.cateringBase}/Menus`),
    getMenu: (id) => request('GET', `${config.cateringBase}/Menus/${id}`),
    createMenu: (model) => request('POST', `${config.cateringBase}/Menus`, model),
    updateMenu: (id, model) => request('PUT', `${config.cateringBase}/Menus/${id}`, model),
    deleteMenu: (id) => request('DELETE', `${config.cateringBase}/Menus/${id}`),

    // MenuFoodItems (join)
    getMenuFoodItems: () => request('GET', `${config.cateringBase}/MenuFoodItems`),
    addMenuFoodItem: (model) => request('POST', `${config.cateringBase}/MenuFoodItems`, model),
    deleteMenuFoodItem: (menuId, foodItemId) => request('DELETE', `${config.cateringBase}/MenuFoodItems/${menuId}/${foodItemId}`),

    // FoodBookings
    getFoodBookings: () => request('GET', `${config.cateringBase}/FoodBookings`),
    getFoodBooking: (id) => request('GET', `${config.cateringBase}/FoodBookings/${id}`),
    createFoodBooking: (model) => request('POST', `${config.cateringBase}/FoodBookings`, model),
    updateFoodBooking: (id, model) => request('PUT', `${config.cateringBase}/FoodBookings/${id}`, model),
    deleteFoodBooking: (id) => request('DELETE', `${config.cateringBase}/FoodBookings/${id}`),

    // Venues examples
    getEventTypes: () => request('GET', `${config.venuesBase}/EventTypes`),
    getAvailability: (query) => {
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

  // --- Menu / MenuItem management helpers ---------------------------------
  async function populateMenuSelect(selector = '.menu-select', includePlaceholder = true, selectedId = null) {
    const nodes = document.querySelectorAll(selector);
    if (!nodes || nodes.length === 0) return;
    let menus;
    try {
      menus = await Api.getMenus();
    } catch (err) {
      console.error(err);
      showAlert('Failed to load menus: ' + (err.message ?? err), 'danger');
      return;
    }
    nodes.forEach(el => {
      if (el.tagName !== 'SELECT') return;
      el.innerHTML = '';
      if (includePlaceholder) {
        const ph = document.createElement('option');
        ph.value = '';
        ph.textContent = '-- Select menu --';
        ph.disabled = true;
        ph.selected = !selectedId;
        el.appendChild(ph);
      }
      menus.forEach(m => {
        const opt = document.createElement('option');
        opt.value = m.MenuId;
        opt.textContent = m.MenuName;
        if (selectedId && m.MenuId === selectedId) opt.selected = true;
        el.appendChild(opt);
      });
    });
  }

  async function populateFoodItemSelect(selector = '.fooditem-select', includePlaceholder = true, selectedId = null) {
    const nodes = document.querySelectorAll(selector);
    if (!nodes || nodes.length === 0) return;
    let items;
    try {
      items = await Api.getFoodItems();
    } catch (err) {
      console.error(err);
      showAlert('Failed to load food items: ' + (err.message ?? err), 'danger');
      return;
    }
    nodes.forEach(el => {
      if (el.tagName !== 'SELECT') return;
      el.innerHTML = '';
      if (includePlaceholder) {
        const ph = document.createElement('option');
        ph.value = '';
        ph.textContent = '-- Select food item --';
        ph.disabled = true;
        ph.selected = !selectedId;
        el.appendChild(ph);
      }
      items.forEach(it => {
        const opt = document.createElement('option');
        opt.value = it.FoodItemId;
        opt.textContent = `${it.Description} (${(it.UnitPrice ?? 0).toFixed ? it.UnitPrice.toFixed(2) : it.UnitPrice})`;
        if (selectedId && it.FoodItemId === selectedId) opt.selected = true;
        el.appendChild(opt);
      });
    });
  }

  async function loadMenusList(containerSelector = '#menusList') {
    const container = document.querySelector(containerSelector);
    if (!container) return;
    try {
      const menus = await Api.getMenus();
      container.innerHTML = '';

      menus.forEach(menu => {
        const card = document.createElement('div');
        card.className = 'card mb-2';
        card.innerHTML = `<div class="card-body">
          <h5 class="card-title">${escapeHtml(menu.MenuName)} <small class="text-muted">#${menu.MenuId}</small></h5>
          <div class="mb-2" data-menu-id="${menu.MenuId}"></div>
        </div>`;
        container.appendChild(card);

        const itemsContainer = card.querySelector('[data-menu-id]');
        // render list of items with remove buttons
        const list = document.createElement('ul');
        list.className = 'list-group list-group-flush';
        const menuItems = (menu.MenuFoodItems ?? []).map(mfi => mfi.FoodItems).filter(Boolean);
        if (menuItems.length === 0) {
          const li = document.createElement('li');
          li.className = 'list-group-item text-muted';
          li.textContent = 'No items';
          list.appendChild(li);
        } else {
          menuItems.forEach(fi => {
            const li = document.createElement('li');
            li.className = 'list-group-item d-flex justify-content-between align-items-center';
            li.innerHTML = `<span>${escapeHtml(fi.Description)} <small class="text-muted">(${(fi.UnitPrice ?? 0).toFixed ? fi.UnitPrice.toFixed(2) : fi.UnitPrice})</small></span>`;
            const btn = document.createElement('button');
            btn.className = 'btn btn-sm btn-outline-danger';
            btn.textContent = 'Remove';
            btn.addEventListener('click', async () => {
              try {
                await Api.deleteMenuFoodItem(menu.MenuId, fi.FoodItemId);
                showAlert('Removed item from menu', 'success');
                await loadMenusList(containerSelector);
                // refresh selects
                populateMenuSelect('.menu-select');
              } catch (err) {
                console.error(err);
                showAlert('Failed to remove item: ' + (err.message ?? err), 'danger');
              }
            });
            li.appendChild(btn);
            list.appendChild(li);
          });
        }
        itemsContainer.appendChild(list);
      });
    } catch (err) {
      console.error(err);
      showAlert('Failed to load menus: ' + (err.message ?? err), 'danger');
    }
  }

  async function handleCreateMenuForm(formSelector = '#createMenuForm') {
    const form = document.querySelector(formSelector);
    if (!form) return;
    form.addEventListener('submit', async (ev) => {
      ev.preventDefault();
      const fm = new FormData(form);
      const name = (fm.get('MenuName') ?? '').toString().trim();
      if (!name) {
        showAlert('Menu name is required', 'warning');
        return;
      }
      try {
        const created = await Api.createMenu({ MenuName: name });
        showAlert('Menu created', 'success');
        form.reset();
        await populateMenuSelect('.menu-select');
        await loadMenusList('#menusList');
      } catch (err) {
        console.error(err);
        showAlert('Failed to create menu: ' + (err.message ?? err), 'danger');
      }
    });
  }

  async function handleAddMenuItemForm(formSelector = '#addMenuItemForm') {
    const form = document.querySelector(formSelector);
    if (!form) return;
    form.addEventListener('submit', async (ev) => {
      ev.preventDefault();
      const fm = new FormData(form);
      const menuId = parseInt(fm.get('MenuId')) || 0;
      const foodItemId = parseInt(fm.get('FoodItemId')) || 0;
      if (!menuId || !foodItemId) {
        showAlert('Select both menu and food item', 'warning');
        return;
      }
      try {
        await Api.addMenuFoodItem({ MenuId: menuId, FoodItemId: foodItemId });
        showAlert('Item added to menu', 'success');
        form.reset();
        // Refresh list and selects
        await loadMenusList('#menusList');
        await populateMenuSelect('.menu-select');
      } catch (err) {
        console.error(err);
        showAlert('Failed to add item: ' + (err.message ?? err), 'danger');
      }
    });
  }

  // --- Page-oriented helpers (auto-run if containers exist) --------------
  async function loadFoodBookingsTable(selector = '#foodBookingsTable') {
    const el = document.querySelector(selector);
    if (!el) return;
    try {
      const bookings = await Api.getFoodBookings();
      renderTable(el, [
        { key: 'FoodBookingId', label: 'Id' },
        { key: 'Menus.MenuName', label: 'Menu' },
        { key: 'NumberOfGuests', label: 'Guests' }
      ], bookings);
    } catch (err) {
      console.error(err);
      showAlert('Failed to load food bookings: ' + (err.message ?? err), 'danger');
    }
  }

  // existing booking edit helpers (if present) - preserved
  async function populateBookingSelect(selector = '#bookingSelect') {
    const node = document.querySelector(selector);
    if (!node) return;
    let bookings;
    try {
      bookings = await Api.getFoodBookings();
    } catch (err) {
      console.error(err);
      showAlert('Failed to load bookings: ' + (err.message ?? err), 'danger');
      return;
    }

    node.innerHTML = '';
    const ph = document.createElement('option');
    ph.value = '';
    ph.textContent = '-- Select booking to edit --';
    ph.disabled = true;
    ph.selected = true;
    node.appendChild(ph);

    bookings.forEach(b => {
      const opt = document.createElement('option');
      opt.value = b.FoodBookingId;
      const menuName = (b.Menus && b.Menus.MenuName) ? b.Menus.MenuName : `Menu ${b.MenuId}`;
      opt.textContent = `#${b.FoodBookingId} — ${menuName} (${b.NumberOfGuests} guests)`;
      node.appendChild(opt);
    });

    node.addEventListener('change', () => {
      const id = parseInt(node.value);
      if (!isNaN(id)) loadBookingIntoForm(id);
    });
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

  // --- preserved booking edit helpers (used by EditFoodBooking page) ------
  async function loadBookingIntoForm(bookingId, formSelector = '#editBookingForm') {
    const form = document.querySelector(formSelector);
    if (!form) return;
    try {
      const booking = await Api.getFoodBooking(bookingId);
      await populateMenuSelect('.menu-select', true, booking.MenuId);

      const idInput = form.querySelector('input[name="FoodBookingId"]');
      if (idInput) idInput.value = booking.FoodBookingId ?? '';

      const menuSelect = form.querySelector('select[name="MenuId"]');
      if (menuSelect) menuSelect.value = booking.MenuId?.toString() ?? '';

      const guestsInput = form.querySelector('input[name="NumberOfGuests"]');
      if (guestsInput) guestsInput.value = booking.NumberOfGuests ?? 1;

      const clientRef = form.querySelector('input[name="ClientReferenceId"]');
      if (clientRef) clientRef.value = booking.ClientReferenceId ?? 0;
    } catch (err) {
      console.error(err);
      showAlert('Failed to load booking: ' + (err.message ?? err), 'danger');
    }
  }

  async function handleEditFoodBookingForm(formSelector = '#editBookingForm') {
    const form = document.querySelector(formSelector);
    if (!form) return;

    form.addEventListener('submit', async (ev) => {
      ev.preventDefault();
      const formData = new FormData(form);
      const id = parseInt(formData.get('FoodBookingId')) || 0;
      if (!id) {
        showAlert('Select a booking to edit first.', 'warning');
        return;
      }

      const payload = {
        FoodBookingId: id,
        MenuId: parseInt(formData.get('MenuId')) || 0,
        NumberOfGuests: parseInt(formData.get('NumberOfGuests')) || 0,
        ClientReferenceId: parseInt(formData.get('ClientReferenceId') ?? '0') || 0
      };

      try {
        await Api.updateFoodBooking(id, payload);
        showAlert('Booking updated successfully.', 'success');
        await populateBookingSelect('#bookingSelect');
        if (typeof loadFoodBookingsTable === 'function') loadFoodBookingsTable();
      } catch (err) {
        console.error(err);
        showAlert('Failed to update booking: ' + (err.message ?? err), 'danger');
      }
    });
  }

  // Auto-run small loaders if page contains standard containers.
  document.addEventListener('DOMContentLoaded', () => {
    loadFoodBookingsTable();
    loadFoodItemsTable();

    // Menu management UI
    if (document.querySelector('#menusList')) {
      populateMenuSelect('.menu-select');
      populateFoodItemSelect('.fooditem-select');
      handleCreateMenuForm('#createMenuForm');
      handleAddMenuItemForm('#addMenuItemForm');
      loadMenusList('#menusList');
    }

    // existing edit booking form wiring
    if (document.querySelector('#editBookingForm')) {
      populateMenuSelect('.menu-select');
      populateBookingSelect('#bookingSelect');
      handleEditFoodBookingForm('#editBookingForm');
    }
  });

  // Expose to window for console and page scripts
  window.ApexSite = {
    Api,
    showAlert,
    renderTable,
    loadFoodBookingsTable,
    loadFoodItemsTable,
    populateMenuSelect,
    populateFoodItemSelect,
    loadMenusList,
    handleCreateMenuForm,
    handleAddMenuItemForm,
    populateBookingSelect,
    loadBookingIntoForm,
    handleEditFoodBookingForm,
    configure: (opts) => Object.assign(config, opts)
  };
})();
