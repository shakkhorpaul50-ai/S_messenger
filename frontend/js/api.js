var API_BASE_URL = 'http://localhost:5000/api';

function setAuthToken(token) {
  localStorage.setItem('token', token);
}

function getAuthToken() {
  return localStorage.getItem('token');
}

function clearAuthToken() {
  localStorage.removeItem('token');
}

function request(method, endpoint, data) {
  var url = API_BASE_URL + endpoint;
  var options = {
    method: method,
    headers: {
      'Content-Type': 'application/json'
    }
  };
  var token = getAuthToken();
  if (token) {
    options.headers['Authorization'] = 'Bearer ' + token;
  }
  if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
    options.body = JSON.stringify(data);
  }
  return fetch(url, options).then(function (response) {
    if (response.status === 204) {
      return null;
    }
    return response.json().then(function (json) {
      if (!response.ok) {
        var error = new Error(json.message || json.error || 'Request failed');
        error.status = response.status;
        error.data = json;
        throw error;
      }
      return json;
    });
  });
}

function login(username, password) {
  return request('POST', '/auth/login', { username: username, password: password });
}

function register(data) {
  return request('POST', '/auth/register', data);
}

function getMessages(conversationId, page, pageSize) {
  var params = '?page=' + (page || 1) + '&pageSize=' + (pageSize || 50);
  return request('GET', '/conversations/' + conversationId + '/messages' + params);
}

function sendMessage(conversationId, content) {
  return request('POST', '/conversations/' + conversationId + '/messages', { content: content });
}

function deleteMessage(conversationId, messageId) {
  return request('DELETE', '/conversations/' + conversationId + '/messages/' + messageId);
}

function getConversations() {
  return request('GET', '/conversations');
}

function searchUsers(query) {
  return request('GET', '/users/search?q=' + encodeURIComponent(query));
}

function addContact(userId) {
  return request('POST', '/contacts', { userId: userId });
}

function getContacts() {
  return request('GET', '/contacts');
}

function getProfile() {
  return request('GET', '/users/profile');
}

function updateProfile(data) {
  return request('PUT', '/users/profile', data);
}
