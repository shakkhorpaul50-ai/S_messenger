function handleLogin(formData) {
  var username = formData.username;
  var password = formData.password;
  var errorEl = document.getElementById('loginError');
  if (errorEl) errorEl.textContent = '';
  return login(username, password).then(function (response) {
    setAuthToken(response.token);
    window.location.replace('chat.html');
  }).catch(function (err) {
    if (errorEl) {
      errorEl.textContent = err.message || 'Login failed. Please try again.';
    }
  });
}

function handleRegister(formData) {
  var errorEl = document.getElementById('registerError');
  if (errorEl) errorEl.textContent = '';
  if (formData.password !== formData.confirmPassword) {
    if (errorEl) errorEl.textContent = 'Passwords do not match';
    return Promise.reject(new Error('Passwords do not match'));
  }
  var data = {
    username: formData.username,
    email: formData.email,
    password: formData.password,
    displayName: formData.displayName
  };
  return register(data).then(function (response) {
    setAuthToken(response.token);
    window.location.replace('chat.html');
  }).catch(function (err) {
    if (errorEl) {
      errorEl.textContent = err.message || 'Registration failed. Please try again.';
    }
  });
}

function logout() {
  clearAuthToken();
  websocket.disconnect();
  window.location.replace('login.html');
}

function checkAuth() {
  var token = getAuthToken();
  if (!token) {
    window.location.replace('login.html');
    return false;
  }
  return true;
}

document.addEventListener('DOMContentLoaded', function () {
  var loginForm = document.getElementById('loginForm');
  if (loginForm) {
    loginForm.addEventListener('submit', function (e) {
      e.preventDefault();
      var formData = {
        username: document.getElementById('username').value,
        password: document.getElementById('password').value
      };
      handleLogin(formData);
    });
  }

  var registerForm = document.getElementById('registerForm');
  if (registerForm) {
    registerForm.addEventListener('submit', function (e) {
      e.preventDefault();
      var formData = {
        username: document.getElementById('username').value,
        email: document.getElementById('email').value,
        password: document.getElementById('password').value,
        confirmPassword: document.getElementById('confirmPassword').value,
        displayName: document.getElementById('displayName').value
      };
      handleRegister(formData);
    });
  }

  var logoutBtn = document.getElementById('logoutBtn');
  if (logoutBtn) {
    logoutBtn.addEventListener('click', function (e) {
      e.preventDefault();
      logout();
    });
  }
});
