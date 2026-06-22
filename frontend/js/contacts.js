function loadContacts() {
  if (!checkAuth()) return;
  return getContacts().then(function (contacts) {
    renderContactList(contacts);
    return contacts;
  }).catch(function (err) {
    console.error('Failed to load contacts:', err);
  });
}

function searchContacts(query) {
  if (!query || query.trim().length < 2) {
    document.getElementById('searchResults').innerHTML = '';
    return;
  }
  searchUsers(query.trim()).then(function (users) {
    var resultsEl = document.getElementById('searchResults');
    if (!users || users.length === 0) {
      resultsEl.innerHTML = '<p class="no-results">No users found</p>';
      return;
    }
    resultsEl.innerHTML = users.map(function (user) {
      return '<div class="contact-item" data-user-id="' + escapeHtml(user.id) + '">' +
        '<div class="contact-item-info">' +
        '<span class="contact-item-name">' + escapeHtml(user.displayName || user.username) + '</span>' +
        '<span class="contact-item-username">@' + escapeHtml(user.username) + '</span>' +
        '</div>' +
        '<button class="btn btn-sm btn-primary add-contact-btn" data-user-id="' + escapeHtml(user.id) + '">Add</button>' +
        '</div>';
    }).join('');
    resultsEl.querySelectorAll('.add-contact-btn').forEach(function (btn) {
      btn.addEventListener('click', function (e) {
        e.stopPropagation();
        handleAddContact(btn.getAttribute('data-user-id'));
      });
    });
  }).catch(function (err) {
    console.error('Search failed:', err);
  });
}

function handleAddContact(userId) {
  return window.addContact(userId).then(function () {
    loadContacts();
    var resultsEl = document.getElementById('searchResults');
    if (resultsEl) resultsEl.innerHTML = '';
    var searchEl = document.getElementById('contactSearch');
    if (searchEl) searchEl.value = '';
  }).catch(function (err) {
    alert(err.message || 'Failed to add contact');
  });
}

function renderContactList(contacts) {
  var listEl = document.getElementById('contactsList');
  if (!listEl) return;
  if (!contacts || contacts.length === 0) {
    listEl.innerHTML = '<p class="no-results">No contacts yet. Search for users to add.</p>';
    return;
  }
  listEl.innerHTML = contacts.map(function (contact) {
    var name = contact.displayName || contact.username;
    return '<div class="contact-item" data-user-id="' + escapeHtml(contact.id) + '">' +
      '<div class="contact-item-info">' +
      '<span class="contact-item-name">' + escapeHtml(name) + '</span>' +
      '<span class="contact-item-username">@' + escapeHtml(contact.username) + '</span>' +
      '</div>' +
      '<button class="btn btn-sm btn-secondary message-contact-btn">Message</button>' +
      '</div>';
  }).join('');
  listEl.querySelectorAll('.contact-item').forEach(function (item) {
    item.addEventListener('click', function () {
      openConversation(item.getAttribute('data-user-id'));
    });
  });
}

function openConversation(contactId) {
  window.location.href = 'chat.html?contact=' + encodeURIComponent(contactId);
}

document.addEventListener('DOMContentLoaded', function () {
  var contactsPage = document.getElementById('contactsList');
  if (contactsPage) {
    loadContacts();
    var searchEl = document.getElementById('contactSearch');
    if (searchEl) {
      var debouncedSearch = debounce(function () {
        searchContacts(searchEl.value);
      }, 400);
      searchEl.addEventListener('input', debouncedSearch);
    }
  }
});
