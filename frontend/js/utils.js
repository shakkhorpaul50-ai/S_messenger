function formatDate(date) {
  if (typeof date === 'string') {
    date = new Date(date);
  }
  if (isNaN(date.getTime())) {
    return '';
  }
  var now = new Date();
  var diff = now - date;
  var oneDay = 86400000;
  if (diff < oneDay && date.getDate() === now.getDate()) {
    return date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
  if (diff < 2 * oneDay && date.getDate() === now.getDate() - 1) {
    return 'Yesterday ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
  if (date.getFullYear() === now.getFullYear()) {
    return date.toLocaleDateString([], { month: 'short', day: 'numeric' }) + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  }
  return date.toLocaleDateString([], { year: 'numeric', month: 'short', day: 'numeric' }) + ' ' + date.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
}

function escapeHtml(str) {
  if (typeof str !== 'string') return '';
  var div = document.createElement('div');
  div.appendChild(document.createTextNode(str));
  return div.innerHTML;
}

function debounce(fn, delay) {
  var timer = null;
  return function () {
    var context = this;
    var args = arguments;
    if (timer) {
      clearTimeout(timer);
    }
    timer = setTimeout(function () {
      fn.apply(context, args);
      timer = null;
    }, delay || 300);
  };
}

function showNotification(title, body) {
  if (!('Notification' in window)) {
    return;
  }
  if (Notification.permission === 'granted') {
    new Notification(title, { body: body });
  } else if (Notification.permission !== 'denied') {
    Notification.requestPermission().then(function (permission) {
      if (permission === 'granted') {
        new Notification(title, { body: body });
      }
    });
  }
}

function getElement(selector) {
  return document.querySelector(selector);
}

function getElementById(id) {
  return document.getElementById(id);
}
