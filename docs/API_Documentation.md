# API Documentation

## Authentication

### POST /api/auth/register

Register a new user account.

**Request:**
```json
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "SecurePass123!",
  "displayName": "Alice Smith"
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "username": "alice",
  "email": "alice@example.com",
  "displayName": "Alice Smith",
  "avatarUrl": null,
  "createdAt": "2026-06-22T12:00:00Z"
}
```

### POST /api/auth/login

Authenticate and receive a JWT token.

**Request:**
```json
{
  "username": "alice",
  "password": "SecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiresAt": "2026-06-23T12:00:00Z",
  "user": {
    "id": 1,
    "username": "alice",
    "displayName": "Alice Smith",
    "avatarUrl": null
  }
}
```

### POST /api/auth/logout

Invalidate the current session.

**Response (200 OK):**
```json
{
  "message": "Logged out successfully"
}
```

### GET /api/auth/me

Get the currently authenticated user's profile.

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "alice",
  "email": "alice@example.com",
  "displayName": "Alice Smith",
  "avatarUrl": null,
  "createdAt": "2026-06-22T12:00:00Z",
  "isOnline": true,
  "lastSeenAt": null
}
```

---

## Users

### GET /api/users/{id}

Get a user's profile by ID.

**Response (200 OK):**
```json
{
  "id": 2,
  "username": "bob",
  "displayName": "Bob Johnson",
  "avatarUrl": null,
  "isOnline": false,
  "lastSeenAt": "2026-06-22T11:30:00Z"
}
```

### GET /api/users/search?q={query}

Search for users by username or display name.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "username": "alice",
    "displayName": "Alice Smith",
    "avatarUrl": null,
    "isOnline": true
  },
  {
    "id": 3,
    "username": "charlie",
    "displayName": "Charlie Brown",
    "avatarUrl": null,
    "isOnline": false
  }
]
```

### PUT /api/users/profile

Update the current user's profile.

**Request:**
```json
{
  "displayName": "Alice S.",
  "avatarUrl": "https://example.com/avatars/alice.jpg"
}
```

**Response (200 OK):**
```json
{
  "id": 1,
  "username": "alice",
  "displayName": "Alice S.",
  "avatarUrl": "https://example.com/avatars/alice.jpg"
}
```

---

## Conversations

### GET /api/conversations

Get all conversations for the authenticated user.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": null,
    "isGroupChat": false,
    "lastMessage": {
      "id": 3,
      "content": "Want to grab lunch later?",
      "sentAt": "2026-06-22T11:40:00Z",
      "senderId": 1,
      "senderName": "Alice Smith"
    },
    "unreadCount": 1,
    "participants": [
      { "id": 1, "username": "alice", "displayName": "Alice Smith" },
      { "id": 2, "username": "bob", "displayName": "Bob Johnson" }
    ]
  }
]
```

### POST /api/conversations

Create a new conversation.

**Request:**
```json
{
  "title": "Project Discussion",
  "isGroupChat": true,
  "participantIds": [1, 2, 3]
}
```

**Response (201 Created):**
```json
{
  "id": 3,
  "title": "Project Discussion",
  "isGroupChat": true,
  "createdByUserId": 1,
  "createdAt": "2026-06-22T12:00:00Z",
  "participants": [
    { "id": 1, "username": "alice", "displayName": "Alice Smith" },
    { "id": 2, "username": "bob", "displayName": "Bob Johnson" },
    { "id": 3, "username": "charlie", "displayName": "Charlie Brown" }
  ]
}
```

### GET /api/conversations/{id}

Get conversation details by ID.

**Response (200 OK):**
```json
{
  "id": 2,
  "title": "General Chat",
  "isGroupChat": true,
  "createdByUserId": 1,
  "createdAt": "2026-06-22T12:00:00Z",
  "participants": [
    { "id": 1, "username": "alice", "displayName": "Alice Smith", "role": "admin" },
    { "id": 2, "username": "bob", "displayName": "Bob Johnson", "role": "member" },
    { "id": 3, "username": "charlie", "displayName": "Charlie Brown", "role": "member" }
  ]
}
```

---

## Messages

### GET /api/messages/conversation/{id}?page=1&pageSize=50

Get paginated messages for a conversation.

**Response (200 OK):**
```json
{
  "items": [
    {
      "id": 6,
      "senderId": 3,
      "senderName": "Charlie Brown",
      "senderAvatarUrl": null,
      "content": "Hey all! Great to see everyone.",
      "sentAt": "2026-06-22T11:55:00Z",
      "isRead": false,
      "isDeleted": false
    }
  ],
  "page": 1,
  "pageSize": 50,
  "totalCount": 6,
  "hasNextPage": false
}
```

### POST /api/messages

Send a new message.

**Request:**
```json
{
  "conversationId": 1,
  "content": "Sure, how about 1pm?"
}
```

**Response (201 Created):**
```json
{
  "id": 7,
  "senderId": 1,
  "senderName": "Alice Smith",
  "content": "Sure, how about 1pm?",
  "sentAt": "2026-06-22T12:01:00Z",
  "isRead": false,
  "isDeleted": false
}
```

### DELETE /api/messages/{id}

Soft-delete a message (only the sender can delete).

**Response (200 OK):**
```json
{
  "message": "Message deleted"
}
```

---

## SignalR Hub

**Endpoint:** `/hubs/chat`

### Connection

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/chat", { accessTokenFactory: () => token })
  .build();
```

### Server-to-Client Events

| Event | Payload | Description |
|---|---|---|
| `MessageReceived` | `{ message: { id, senderId, senderName, senderAvatarUrl, conversationId, content, sentAt, isRead } }` | A new message was sent |
| `MessageDeleted` | `{ messageId, conversationId }` | A message was deleted |
| `Typing` | `{ conversationId, userId, userName }` | A user started typing |
| `StopTyping` | `{ conversationId, userId }` | A user stopped typing |
| `MarkedAsRead` | `{ conversationId, userId, messageId }` | Messages were marked as read |

### Client-to-Server Methods

| Method | Parameters | Description |
|---|---|---|
| `JoinConversation` | `conversationId` | Join a conversation group to receive messages |
| `LeaveConversation` | `conversationId` | Leave a conversation group |
| `SendMessage` | `{ conversationId, content }` | Send a message to a conversation |
| `Typing` | `{ conversationId }` | Notify others that the user is typing |
| `StopTyping` | `{ conversationId }` | Notify others that the user stopped typing |
| `MarkAsRead` | `{ conversationId, messageId }` | Mark a message as read |

### Client Example

```javascript
// Join conversation
await connection.invoke("JoinConversation", conversationId);

// Send message
await connection.invoke("SendMessage", {
  conversationId: 1,
  content: "Hello!"
});

// Receive messages
connection.on("MessageReceived", (data) => {
  appendMessage(data.message);
});
```
