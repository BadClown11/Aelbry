/*
    Modulo 6 (parte 2) - Stored Procedures del Chat Empresarial.
*/

USE AelbryDb;
GO

--------------------------------------------------------------------------------------------
-- CHAT CONVERSATION (mensajes directos)
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_CHAT_CONVERSATION_GET_OR_CREATE
    @P_USER_ID_1 INT,
    @P_USER_ID_2 INT,
    @P_CONVERSATION_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DECLARE @UserA INT = CASE WHEN @P_USER_ID_1 < @P_USER_ID_2 THEN @P_USER_ID_1 ELSE @P_USER_ID_2 END;
        DECLARE @UserB INT = CASE WHEN @P_USER_ID_1 < @P_USER_ID_2 THEN @P_USER_ID_2 ELSE @P_USER_ID_1 END;

        SELECT @P_CONVERSATION_ID = ConversationId FROM dbo.ChatConversation WHERE UserAId = @UserA AND UserBId = @UserB;

        IF @P_CONVERSATION_ID IS NULL
        BEGIN
            INSERT INTO dbo.ChatConversation (UserAId, UserBId) VALUES (@UserA, @UserB);
            SET @P_CONVERSATION_ID = CAST(SCOPE_IDENTITY() AS INT);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_CONVERSATION_GET_BY_USER
    @P_USER_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT c.ConversationId AS CONVERSATION_ID,
           CASE WHEN c.UserAId = @P_USER_ID THEN c.UserBId ELSE c.UserAId END AS OTHER_USER_ID,
           CASE WHEN c.UserAId = @P_USER_ID THEN ub.FirstName + ' ' + ub.LastName ELSE ua.FirstName + ' ' + ua.LastName END AS OTHER_USER_NAME,
           c.CreatedDate AS CREATED_DATE
    FROM dbo.ChatConversation c
    JOIN dbo.[User] ua ON ua.UserId = c.UserAId
    JOIN dbo.[User] ub ON ub.UserId = c.UserBId
    WHERE c.UserAId = @P_USER_ID OR c.UserBId = @P_USER_ID
    ORDER BY c.CreatedDate DESC;
END
GO

--------------------------------------------------------------------------------------------
-- CHAT MESSAGE
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_GET_BY_ID
    @P_CHAT_MESSAGE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT m.ChatMessageId AS CHAT_MESSAGE_ID, m.ProjectId AS PROJECT_ID, m.ConversationId AS CONVERSATION_ID,
           m.SenderUserId AS SENDER_USER_ID, u.FirstName + ' ' + u.LastName AS SENDER_NAME,
           m.ParentMessageId AS PARENT_MESSAGE_ID, m.Text AS TEXT, m.CreatedDate AS CREATED_DATE, m.IsDeleted AS IS_DELETED
    FROM dbo.ChatMessage m
    JOIN dbo.[User] u ON u.UserId = m.SenderUserId
    WHERE m.ChatMessageId = @P_CHAT_MESSAGE_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_GET_BY_PROJECT
    @P_PROJECT_ID INT,
    @P_TOP INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@P_TOP) m.ChatMessageId AS CHAT_MESSAGE_ID, m.ProjectId AS PROJECT_ID, m.ConversationId AS CONVERSATION_ID,
           m.SenderUserId AS SENDER_USER_ID, u.FirstName + ' ' + u.LastName AS SENDER_NAME,
           m.ParentMessageId AS PARENT_MESSAGE_ID, m.Text AS TEXT, m.CreatedDate AS CREATED_DATE, m.IsDeleted AS IS_DELETED
    FROM dbo.ChatMessage m
    JOIN dbo.[User] u ON u.UserId = m.SenderUserId
    WHERE m.ProjectId = @P_PROJECT_ID
    ORDER BY m.CreatedDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_GET_BY_CONVERSATION
    @P_CONVERSATION_ID INT,
    @P_TOP INT = 50
AS
BEGIN
    SET NOCOUNT ON;
    SELECT TOP (@P_TOP) m.ChatMessageId AS CHAT_MESSAGE_ID, m.ProjectId AS PROJECT_ID, m.ConversationId AS CONVERSATION_ID,
           m.SenderUserId AS SENDER_USER_ID, u.FirstName + ' ' + u.LastName AS SENDER_NAME,
           m.ParentMessageId AS PARENT_MESSAGE_ID, m.Text AS TEXT, m.CreatedDate AS CREATED_DATE, m.IsDeleted AS IS_DELETED
    FROM dbo.ChatMessage m
    JOIN dbo.[User] u ON u.UserId = m.SenderUserId
    WHERE m.ConversationId = @P_CONVERSATION_ID
    ORDER BY m.CreatedDate DESC;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_INSERT
    @P_PROJECT_ID INT = NULL,
    @P_CONVERSATION_ID INT = NULL,
    @P_SENDER_USER_ID INT,
    @P_PARENT_MESSAGE_ID INT = NULL,
    @P_TEXT NVARCHAR(2000),
    @P_NEW_CHAT_MESSAGE_ID INT OUTPUT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        INSERT INTO dbo.ChatMessage (ProjectId, ConversationId, SenderUserId, ParentMessageId, Text)
        VALUES (@P_PROJECT_ID, @P_CONVERSATION_ID, @P_SENDER_USER_ID, @P_PARENT_MESSAGE_ID, @P_TEXT);

        SET @P_NEW_CHAT_MESSAGE_ID = CAST(SCOPE_IDENTITY() AS INT);
        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_DELETE
    @P_CHAT_MESSAGE_ID INT,
    @P_USER_ID INT,
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ChatMessage WHERE ChatMessageId = @P_CHAT_MESSAGE_ID AND SenderUserId = @P_USER_ID)
        BEGIN
            RAISERROR('Solo puedes eliminar tus propios mensajes.', 16, 1);
        END

        UPDATE dbo.ChatMessage
        SET IsDeleted = 1, Text = ''
        WHERE ChatMessageId = @P_CHAT_MESSAGE_ID;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

--------------------------------------------------------------------------------------------
-- CHAT MESSAGE REACTION
--------------------------------------------------------------------------------------------
CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_GET_REACTIONS_BY_MESSAGE
    @P_CHAT_MESSAGE_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ChatMessageId AS CHAT_MESSAGE_ID, r.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           r.Emoji AS EMOJI, r.CreatedDate AS CREATED_DATE
    FROM dbo.ChatMessageReaction r
    JOIN dbo.[User] u ON u.UserId = r.UserId
    WHERE r.ChatMessageId = @P_CHAT_MESSAGE_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_GET_REACTIONS_BY_PROJECT
    @P_PROJECT_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ChatMessageId AS CHAT_MESSAGE_ID, r.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           r.Emoji AS EMOJI, r.CreatedDate AS CREATED_DATE
    FROM dbo.ChatMessageReaction r
    JOIN dbo.[User] u ON u.UserId = r.UserId
    JOIN dbo.ChatMessage m ON m.ChatMessageId = r.ChatMessageId
    WHERE m.ProjectId = @P_PROJECT_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_GET_REACTIONS_BY_CONVERSATION
    @P_CONVERSATION_ID INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT r.ChatMessageId AS CHAT_MESSAGE_ID, r.UserId AS USER_ID, u.FirstName + ' ' + u.LastName AS USER_NAME,
           r.Emoji AS EMOJI, r.CreatedDate AS CREATED_DATE
    FROM dbo.ChatMessageReaction r
    JOIN dbo.[User] u ON u.UserId = r.UserId
    JOIN dbo.ChatMessage m ON m.ChatMessageId = r.ChatMessageId
    WHERE m.ConversationId = @P_CONVERSATION_ID;
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_ADD_REACTION
    @P_CHAT_MESSAGE_ID INT,
    @P_USER_ID INT,
    @P_EMOJI NVARCHAR(16),
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NOT EXISTS (SELECT 1 FROM dbo.ChatMessageReaction WHERE ChatMessageId = @P_CHAT_MESSAGE_ID AND UserId = @P_USER_ID AND Emoji = @P_EMOJI)
        BEGIN
            INSERT INTO dbo.ChatMessageReaction (ChatMessageId, UserId, Emoji) VALUES (@P_CHAT_MESSAGE_ID, @P_USER_ID, @P_EMOJI);
        END

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO

CREATE OR ALTER PROCEDURE dbo.SP_CHAT_MESSAGE_REMOVE_REACTION
    @P_CHAT_MESSAGE_ID INT,
    @P_USER_ID INT,
    @P_EMOJI NVARCHAR(16),
    @OUT_RESULT NVARCHAR(400) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        DELETE FROM dbo.ChatMessageReaction
        WHERE ChatMessageId = @P_CHAT_MESSAGE_ID AND UserId = @P_USER_ID AND Emoji = @P_EMOJI;

        SET @OUT_RESULT = 'OK';
    END TRY
    BEGIN CATCH
        SET @OUT_RESULT = ERROR_MESSAGE();
    END CATCH
END
GO
