# Database Schema

## ER Diagram

```
┌─────────────┐       ┌────────────────┐       ┌──────────────┐
│    Users    │       │  Conversations │       │ Participants │
├─────────────┤       ├────────────────┤       ├──────────────┤
│ Id (PK)     │──┐    │ Id (PK)        │──┐    │ Id (PK)      │
│ Username    │  │    │ Title          │  │    │ ConvId (FK)  │──┐
│ Email       │  │    │ CreatedByUserId│──┤    │ UserId (FK)  │  │
│ PasswordHash│  │    │ CreatedAt      │  │    │ JoinedAt     │  │
│ DisplayName │  │    │ IsGroupChat    │  │    │ Role         │  │
│ AvatarUrl   │  │    └────────────────┘  │    └──────┬───────┘  │
│ CreatedAt   │  │    ┌──────────────┐     │           │          │
│ IsOnline    │  │    │  Messages    │     │           │          │
│ LastSeenAt  │  │    ├──────────────┤     │           │          │
└─────────────┘  │    │ Id (PK)      │     │           │          │
                 │    │ SenderId(FK) │─────┘           │          │
                 │    │ ConvId (FK)  │─────────────────┘          │
                 └────┤ Content      │                            │
                      │ SentAt       │                            │
                      │ IsRead       │                            │
                      │ IsDeleted    │                            │
                      └──────────────┘                            │
```

## Table Definitions

### Users

Stores registered user accounts.

| Column | Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, IDENTITY | Unique user identifier |
| Username | NVARCHAR(50) | NOT NULL, UNIQUE | Login username |
| Email | NVARCHAR(255) | NOT NULL, UNIQUE | User email address |
| PasswordHash | NVARCHAR(500) | NOT NULL | BCrypt hash of password |
| DisplayName | NVARCHAR(100) | NOT NULL | Display name shown in UI |
| AvatarUrl | NVARCHAR(500) | NULL | URL to avatar image |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Account creation timestamp |
| IsOnline | BIT | NOT NULL, DEFAULT 0 | Online status flag |
| LastSeenAt | DATETIME2 | NULL | Last activity timestamp |

### Conversations

Represents private chats and group conversations.

| Column | Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, IDENTITY | Unique conversation identifier |
| Title | NVARCHAR(200) | NULL | Group chat title (NULL for private chats) |
| CreatedByUserId | INT | NOT NULL, FK -> Users(Id) | User who created the conversation |
| CreatedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Creation timestamp |
| IsGroupChat | BIT | NOT NULL, DEFAULT 0 | Whether this is a group conversation |

### Participants

Maps users to conversations with role information.

| Column | Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, IDENTITY | Unique participant record identifier |
| ConversationId | INT | NOT NULL, FK -> Conversations(Id) | Associated conversation |
| UserId | INT | NOT NULL, FK -> Users(Id) | Associated user |
| JoinedAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | When the user joined |
| Role | NVARCHAR(20) | NOT NULL, DEFAULT 'member' | Role: 'member' or 'admin' |

**Unique Constraint:** (ConversationId, UserId) ensures a user cannot be added twice to the same conversation.

### Messages

Stores all messages sent in conversations.

| Column | Type | Constraints | Description |
|---|---|---|---|
| Id | INT | PK, IDENTITY | Unique message identifier |
| SenderId | INT | NOT NULL, FK -> Users(Id) | User who sent the message |
| ConversationId | INT | NOT NULL, FK -> Conversations(Id) | Conversation the message belongs to |
| Content | NVARCHAR(MAX) | NOT NULL | Message text content |
| SentAt | DATETIME2 | NOT NULL, DEFAULT GETUTCDATE() | Timestamp when sent |
| IsRead | BIT | NOT NULL, DEFAULT 0 | Whether the message has been read |
| IsDeleted | BIT | NOT NULL, DEFAULT 0 | Soft delete flag |

## Relationships

| From | To | Type | Cascade |
|---|---|---|---|
| Conversations.CreatedByUserId | Users.Id | Many-to-One | NO ACTION |
| Participants.ConversationId | Conversations.Id | Many-to-One | CASCADE |
| Participants.UserId | Users.Id | Many-to-One | CASCADE |
| Messages.SenderId | Users.Id | Many-to-One | NO ACTION |
| Messages.ConversationId | Conversations.Id | Many-to-One | CASCADE |

## Indexes

| Index Name | Table | Columns | Description |
|---|---|---|---|
| IX_Messages_ConversationId_SentAt | Messages | ConversationId (ASC), SentAt (DESC) | Optimizes message retrieval per conversation in reverse chronological order |
| IX_Participants_UserId | Participants | UserId (ASC) | Optimizes lookup of all conversations for a user |

## Cascade Delete Behavior

- Deleting a **User** cascades to **Participants** (user removed from all conversations).
- Deleting a **Conversation** cascades to **Participants** and **Messages** (cleanup of related records).
- Deleting a **User** does NOT cascade to **Messages** or **Conversations** to preserve message history and conversation ownership.
