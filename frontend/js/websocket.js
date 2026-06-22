var websocket = (function () {
  var connection = null;
  var listeners = {};
  var reconnectTimeout = null;
  var isConnected = false;

  function connect(token) {
    if (connection && (connection.state === 'Connected' || connection.state === 'connecting')) {
      return;
    }
    if (typeof signalR === 'undefined') {
      console.warn('SignalR client not loaded');
      return;
    }
    connection = new signalR.HubConnectionBuilder()
      .withUrl(API_BASE_URL.replace('/api', '') + '/hubs/chat', {
        accessTokenFactory: function () { return token || getAuthToken(); }
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    connection.on('MessageReceived', function (message) {
      emit('messageReceived', message);
    });

    connection.on('MessageDeleted', function (conversationId, messageId) {
      emit('messageDeleted', conversationId, messageId);
    });

    connection.on('UserTyping', function (conversationId, userName) {
      emit('typing', conversationId, userName);
    });

    connection.onreconnecting(function () {
      isConnected = false;
      emit('reconnecting');
    });

    connection.onreconnected(function () {
      isConnected = true;
      emit('reconnected');
    });

    connection.onclose(function () {
      isConnected = false;
      emit('disconnected');
    });

    return connection.start().then(function () {
      isConnected = true;
      emit('connected');
      return connection;
    }).catch(function (err) {
      console.error('SignalR connection error:', err);
      isConnected = false;
      throw err;
    });
  }

  function disconnect() {
    if (reconnectTimeout) {
      clearTimeout(reconnectTimeout);
      reconnectTimeout = null;
    }
    if (connection) {
      connection.stop();
      connection = null;
    }
    isConnected = false;
  }

  function on(event, callback) {
    if (!listeners[event]) {
      listeners[event] = [];
    }
    listeners[event].push(callback);
  }

  function off(event, callback) {
    if (!listeners[event]) return;
    if (!callback) {
      delete listeners[event];
      return;
    }
    listeners[event] = listeners[event].filter(function (fn) {
      return fn !== callback;
    });
  }

  function emit(event) {
    var args = Array.prototype.slice.call(arguments, 1);
    var eventListeners = listeners[event];
    if (eventListeners) {
      eventListeners.forEach(function (fn) {
        try {
          fn.apply(null, args);
        } catch (e) {
          console.error('Event handler error:', e);
        }
      });
    }
  }

  function sendMessage(conversationId, content) {
    if (connection && isConnected) {
      return connection.invoke('SendMessage', conversationId, content);
    }
    return Promise.reject(new Error('Not connected'));
  }

  function joinConversation(conversationId) {
    if (connection && isConnected) {
      return connection.invoke('JoinConversation', conversationId);
    }
    return Promise.reject(new Error('Not connected'));
  }

  function startTyping(conversationId) {
    if (connection && isConnected) {
      return connection.invoke('StartTyping', conversationId);
    }
    return Promise.resolve();
  }

  function getConnectionState() {
    return connection ? connection.state : 'Disconnected';
  }

  return {
    connect: connect,
    disconnect: disconnect,
    on: on,
    off: off,
    emit: emit,
    sendMessage: sendMessage,
    joinConversation: joinConversation,
    startTyping: startTyping,
    getConnectionState: getConnectionState
  };
})();
