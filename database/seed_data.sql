INSERT INTO [Users] ([Username], [Email], [PasswordHash], [DisplayName], [AvatarUrl], [CreatedAt], [IsOnline], [LastSeenAt])
VALUES
    ('alice', 'alice@example.com', 'AQAAAAIAAYagAAAAEJ6Kj8Xy9kL3mWn4R5tB7v2cF0dG1hI3jK4lM5nO6pQ7rS8tU9vW0xY1zA2bC3dE', 'Alice Smith', NULL, GETUTCDATE(), 0, NULL),
    ('bob', 'bob@example.com', 'AQAAAAIAAYagAAAAEK7LmN4oP5qR6sT8uV0wX2yZ4aB6cD8eF0gH2iJ4kL6mN8oP0qR2sT4uV6wX', 'Bob Johnson', NULL, GETUTCDATE(), 0, NULL),
    ('charlie', 'charlie@example.com', 'AQAAAAIAAYagAAAAFM9oQ1rS3tU5vW7xY9zB2cD4eF6gH8iJ0kL2mN4oP6qR8sT0uV2wX4yZ6aB', 'Charlie Brown', NULL, GETUTCDATE(), 0, NULL);

DECLARE @AliceId INT = (SELECT [Id] FROM [Users] WHERE [Username] = 'alice');
DECLARE @BobId INT = (SELECT [Id] FROM [Users] WHERE [Username] = 'bob');
DECLARE @CharlieId INT = (SELECT [Id] FROM [Users] WHERE [Username] = 'charlie');

INSERT INTO [Conversations] ([Title], [CreatedByUserId], [CreatedAt], [IsGroupChat])
VALUES
    (NULL, @AliceId, GETUTCDATE(), 0),
    ('General Chat', @AliceId, GETUTCDATE(), 1);

DECLARE @PrivateConvId INT = (SELECT [Id] FROM [Conversations] WHERE [CreatedByUserId] = @AliceId AND [IsGroupChat] = 0);
DECLARE @GroupConvId INT = (SELECT [Id] FROM [Conversations] WHERE [IsGroupChat] = 1);

INSERT INTO [Participants] ([ConversationId], [UserId], [JoinedAt], [Role])
VALUES
    (@PrivateConvId, @AliceId, GETUTCDATE(), 'member'),
    (@PrivateConvId, @BobId, GETUTCDATE(), 'member'),
    (@GroupConvId, @AliceId, GETUTCDATE(), 'admin'),
    (@GroupConvId, @BobId, GETUTCDATE(), 'member'),
    (@GroupConvId, @CharlieId, GETUTCDATE(), 'member');

INSERT INTO [Messages] ([SenderId], [ConversationId], [Content], [SentAt], [IsRead], [IsDeleted])
VALUES
    (@AliceId, @PrivateConvId, 'Hey Bob, how are you?', DATEADD(MINUTE, -30, GETUTCDATE()), 1, 0),
    (@BobId, @PrivateConvId, 'Hi Alice! I''m doing great, thanks!', DATEADD(MINUTE, -25, GETUTCDATE()), 1, 0),
    (@AliceId, @PrivateConvId, 'Want to grab lunch later?', DATEADD(MINUTE, -20, GETUTCDATE()), 0, 0),
    (@AliceId, @GroupConvId, 'Welcome to the group chat everyone!', DATEADD(MINUTE, -15, GETUTCDATE()), 1, 0),
    (@BobId, @GroupConvId, 'Thanks Alice! Happy to be here.', DATEADD(MINUTE, -10, GETUTCDATE()), 1, 0),
    (@CharlieId, @GroupConvId, 'Hey all! Great to see everyone.', DATEADD(MINUTE, -5, GETUTCDATE()), 0, 0);
