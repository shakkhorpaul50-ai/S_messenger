CREATE TABLE [Users] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Username] NVARCHAR(50) NOT NULL,
    [Email] NVARCHAR(255) NOT NULL,
    [PasswordHash] NVARCHAR(500) NOT NULL,
    [DisplayName] NVARCHAR(100) NOT NULL,
    [AvatarUrl] NVARCHAR(500) NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsOnline] BIT NOT NULL DEFAULT 0,
    [LastSeenAt] DATETIME2 NULL,
    CONSTRAINT [UQ_Users_Username] UNIQUE ([Username]),
    CONSTRAINT [UQ_Users_Email] UNIQUE ([Email])
);

CREATE TABLE [Conversations] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Title] NVARCHAR(200) NULL,
    [CreatedByUserId] INT NOT NULL,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsGroupChat] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Conversations_CreatedByUserId] FOREIGN KEY ([CreatedByUserId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Participants] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [ConversationId] INT NOT NULL,
    [UserId] INT NOT NULL,
    [JoinedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [Role] NVARCHAR(20) NOT NULL DEFAULT 'member',
    CONSTRAINT [FK_Participants_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_Participants_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE,
    CONSTRAINT [UQ_Participants_ConversationId_UserId] UNIQUE ([ConversationId], [UserId])
);

CREATE TABLE [Messages] (
    [Id] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [SenderId] INT NOT NULL,
    [ConversationId] INT NOT NULL,
    [Content] NVARCHAR(MAX) NOT NULL,
    [SentAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [IsRead] BIT NOT NULL DEFAULT 0,
    [IsDeleted] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [FK_Messages_SenderId] FOREIGN KEY ([SenderId]) REFERENCES [Users]([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Messages_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [Conversations]([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Messages_ConversationId_SentAt] ON [Messages] ([ConversationId], [SentAt] DESC);
CREATE INDEX [IX_Participants_UserId] ON [Participants] ([UserId]);
