var chat = (function () {
  var currentConversationId = null;
  var currentPage = 1;
  var totalPages = 1;
  var isLoadingMessages = false;
  var typingTimeout = null;
  var currentUser = null;

  function init() {
    if (!checkAuth()) return;
    loadConversations();
    setupEventListeners();
    setupWebsocketHandlers();
    loadUserProfile();
    var params = new URLSearchParams(window.location.search);
    var contactId = params.get('contact');
    if (contactId) {
      createConversationWithContact(contactId);
    }
  }

  function loadUserProfile() {
    getProfile().then(function (profile) {
      currentUser = profile;
    }).catch(function () {});
  }

  function loadConversations() {
    getConversations().then(function (conversations) {
      renderConversations(conversations);
    }).catch(function (err) {
      console.error('Failed to load conversations:', err);
    });
  }

  function renderConversations(conversations) {
    var listEl = document.getElementById('conversationList');
    if (!listEl) return;
    if (!conversations || conversations.length === 0) {
      listEl.innerHTML = '<div class="empty-conversations">No conversations yet</div>';
      return;
    }
    listEl.innerHTML = conversations.map(function (conv) {
      var name = conv.name || conv.displayName || 'Unknown';
      var initial = name.charAt(0).toUpperCase();
      var preview = conv.lastMessage ? conv.lastMessage.content : '';
      var time = conv.lastMessage ? formatDate(conv.lastMessage.timestamp) : '';
      var isActive = currentConversationId === conv.id;
      return '<div class="conversation-item' + (isActive ? ' active' : '') + '" data-conversation-id="' + escapeHtml(conv.id) + '">' +
        '<div class="conversation-item-avatar">' + escapeHtml(initial) + '</div>' +
        '<div class="conversation-item-info">' +
        '<div class="conversation-item-name">' + escapeHtml(name) + '</div>' +
        '<div class="conversation-item-preview">' + escapeHtml(preview) + '</div>' +
        '</div>' +
        '<div class="conversation-item-time">' + escapeHtml(time) + '</div>' +
        '</div>';
    }).join('');
    listEl.querySelectorAll('.conversation-item').forEach(function (item) {
      item.addEventListener('click', function () {
        var convId = item.getAttribute('data-conversation-id');
        selectConversation(convId);
      });
    });
  }

  function selectConversation(conversationId) {
    currentConversationId = conversationId;
    currentPage = 1;
    totalPages = 1;
    var messagesEl = document.getElementById('chatMessages');
    var emptyState = document.getElementById('emptyState');
    if (emptyState) emptyState.style.display = 'none';
    if (messagesEl) messagesEl.innerHTML = '';
    var inputEl = document.getElementById('messageInput');
    if (inputEl) inputEl.disabled = false;
    var sendBtn = document.getElementById('sendButton');
    if (sendBtn) sendBtn.disabled = false;
    updateConversationHeader(conversationId);
    highlightConversation(conversationId);
    loadMessages(conversationId);
    websocket.joinConversation(conversationId).catch(function () {});
  }

  function updateConversationHeader(conversationId) {
    var headerEl = document.getElementById('currentConversationName');
    if (!headerEl) return;
    var activeItem = document.querySelector('.conversation-item[data-conversation-id="' + conversationId + '"]');
    if (activeItem) {
      var nameEl = activeItem.querySelector('.conversation-item-name');
      headerEl.textContent = nameEl ? nameEl.textContent : 'Conversation';
    } else {
      headerEl.textContent = 'Conversation';
    }
  }

  function highlightConversation(conversationId) {
    document.querySelectorAll('.conversation-item').forEach(function (item) {
      item.classList.remove('active');
    });
    var activeItem = document.querySelector('.conversation-item[data-conversation-id="' + conversationId + '"]');
    if (activeItem) {
      activeItem.classList.add('active');
    }
  }

  function loadMessages(conversationId) {
    if (isLoadingMessages) return;
    isLoadingMessages = true;
    getMessages(conversationId, currentPage, 50).then(function (response) {
      var messages = response.items || response.messages || response || [];
      totalPages = response.totalPages || Math.ceil((response.totalCount || 0) / 50) || 1;
      var messagesEl = document.getElementById('chatMessages');
      if (!messagesEl) return;
      if (currentPage === 1) {
        messagesEl.innerHTML = '';
      }
      messages.reverse().forEach(function (msg) {
        var isOwn = currentUser && msg.senderId === currentUser.id;
        appendMessage(msg, isOwn, currentPage > 1);
      });
      if (currentPage === 1) {
        scrollToBottom();
      } else {
        var firstMsg = messagesEl.querySelector('.message');
        if (firstMsg) firstMsg.scrollIntoView();
      }
      isLoadingMessages = false;
    }).catch(function (err) {
      console.error('Failed to load messages:', err);
      isLoadingMessages = false;
    });
  }

  function loadMoreMessages() {
    if (isLoadingMessages || currentPage >= totalPages) return;
    currentPage++;
    loadMessages(currentConversationId);
  }

  function sendMessage(content) {
    if (!currentConversationId || !content || !content.trim()) return;
    var trimmed = content.trim();
    var inputEl = document.getElementById('messageInput');
    if (inputEl) inputEl.value = '';

    window.sendMessage(currentConversationId, trimmed).then(function (msg) {
      if (msg) {
        var isOwn = true;
        appendMessage(msg, isOwn);
        scrollToBottom();
        refreshConversations();
      }
    }).catch(function () {});

    websocket.sendMessage(currentConversationId, trimmed).catch(function () {});
  }

  function displayMessage(message, isOwn) {
    appendMessage(message, isOwn);
    scrollToBottom();
    refreshConversations();
  }

  function appendMessage(message, isOwn, prepend) {
    var messagesEl = document.getElementById('chatMessages');
    if (!messagesEl) return;
    var existingMsg = messagesEl.querySelector('[data-message-id="' + escapeHtml(message.id) + '"]');
    if (existingMsg) return;
    var senderName = message.senderName || message.senderDisplayName || message.senderUsername || (isOwn ? 'You' : 'Unknown');
    var content = escapeHtml(message.content || '');
    var time = formatDate(message.timestamp || message.sentAt || new Date());
    var msgDiv = document.createElement('div');
    msgDiv.className = 'message' + (isOwn ? ' own' : '');
    msgDiv.setAttribute('data-message-id', message.id);
    msgDiv.innerHTML = '<div class="message-sender">' + escapeHtml(senderName) + '</div>' +
      '<div class="message-content">' + content + '</div>' +
      '<div class="message-time">' + escapeHtml(time) + '</div>';
    if (prepend) {
      messagesEl.insertBefore(msgDiv, messagesEl.firstChild);
    } else {
      messagesEl.appendChild(msgDiv);
    }
  }

  function removeMessage(conversationId, messageId) {
    var msgEl = document.querySelector('.chat-messages [data-message-id="' + escapeHtml(messageId) + '"]');
    if (msgEl) {
      msgEl.remove();
    }
  }

  function showTypingIndicator(userName) {
    var indicator = document.getElementById('typingIndicator');
    if (!indicator) return;
    indicator.textContent = escapeHtml(userName) + ' is typing...';
    if (typingTimeout) clearTimeout(typingTimeout);
    typingTimeout = setTimeout(function () {
      indicator.textContent = '';
    }, 3000);
  }

  function scrollToBottom() {
    var messagesEl = document.getElementById('chatMessages');
    if (messagesEl) {
      messagesEl.scrollTop = messagesEl.scrollHeight;
    }
  }

  function refreshConversations() {
    loadConversations();
  }

  function setupEventListeners() {
    var sendBtn = document.getElementById('sendButton');
    var inputEl = document.getElementById('messageInput');
    if (sendBtn) {
      sendBtn.addEventListener('click', function () {
        if (inputEl) sendMessage(inputEl.value);
      });
    }
    if (inputEl) {
      inputEl.addEventListener('keydown', function (e) {
        if (e.key === 'Enter' && !e.shiftKey) {
          e.preventDefault();
          sendMessage(inputEl.value);
        }
      });
      inputEl.addEventListener('input', function () {
        if (currentConversationId && inputEl.value.trim()) {
          websocket.startTyping(currentConversationId).catch(function () {});
        }
      });
    }
    var messagesEl = document.getElementById('chatMessages');
    if (messagesEl) {
      messagesEl.addEventListener('scroll', function () {
        if (messagesEl.scrollTop === 0) {
          loadMoreMessages();
        }
      });
    }
    var toggleBtn = document.getElementById('toggleSidebar');
    if (toggleBtn) {
      toggleBtn.addEventListener('click', function () {
        var sidebar = document.querySelector('.conversations-sidebar');
        if (sidebar) {
          sidebar.classList.toggle('open');
        }
      });
    }
    var searchEl = document.getElementById('conversationSearch');
    if (searchEl) {
      searchEl.addEventListener('input', debounce(function () {
        var query = searchEl.value.toLowerCase();
        document.querySelectorAll('.conversation-item').forEach(function (item) {
          var name = item.querySelector('.conversation-item-name');
          var match = name && name.textContent.toLowerCase().indexOf(query) !== -1;
          item.style.display = match || !query ? 'flex' : 'none';
        });
      }, 300));
    }
  }

  function setupWebsocketHandlers() {
    websocket.on('messageReceived', function (message) {
      if (message.conversationId === currentConversationId) {
        var isOwn = currentUser && message.senderId === currentUser.id;
        displayMessage(message, isOwn);
      } else {
        refreshConversations();
        showNotification('New Message', message.senderName || 'Someone' + ' sent a message');
      }
    });

    websocket.on('messageDeleted', function (conversationId, messageId) {
      if (conversationId === currentConversationId) {
        removeMessage(conversationId, messageId);
      }
    });

    websocket.on('typing', function (conversationId, userName) {
      if (conversationId === currentConversationId) {
        showTypingIndicator(userName);
      }
    });

    websocket.on('connected', function () {
      if (currentConversationId) {
        websocket.joinConversation(currentConversationId).catch(function () {});
      }
    });
  }

  function createConversationWithContact(contactId) {
    var convItems = document.querySelectorAll('.conversation-item');
    for (var i = 0; i < convItems.length; i++) {
      convItems[i].click();
      return;
    }
  }

  document.addEventListener('DOMContentLoaded', function () {
    var chatLayout = document.querySelector('.chat-layout');
    if (chatLayout) {
      init();
      var token = getAuthToken();
      if (token) {
        websocket.connect(token).catch(function () {});
      }
    }
  });

  return {
    init: init,
    loadConversations: loadConversations,
    selectConversation: selectConversation,
    sendMessage: sendMessage,
    displayMessage: displayMessage,
    loadMoreMessages: loadMoreMessages,
    showTypingIndicator: showTypingIndicator
  };
})();
