/*
    Modulo 6 (parte 2) - Chat Empresarial
    Motor: SQL Server
    Los canales de proyecto reutilizan la entidad Project (no se crea un catalogo de
    "canales" aparte); los mensajes directos usan ChatConversation (1 fila por par de usuarios).
*/

USE AelbryDb;
GO

IF OBJECT_ID('dbo.ChatMessageReaction', 'U') IS NOT NULL DROP TABLE dbo.ChatMessageReaction;
IF OBJECT_ID('dbo.ChatMessage', 'U') IS NOT NULL DROP TABLE dbo.ChatMessage;
IF OBJECT_ID('dbo.ChatConversation', 'U') IS NOT NULL DROP TABLE dbo.ChatConversation;
GO

-- Un solo hilo de DM por par de usuarios. UserAId siempre es el menor de los dos UserId
-- (se normaliza en el SP de alta) para que la restriccion UNIQUE evite duplicados.
CREATE TABLE dbo.ChatConversation
(
    ConversationId  INT IDENTITY(1,1) NOT NULL,
    UserAId         INT               NOT NULL,
    UserBId         INT               NOT NULL,
    CreatedDate     DATETIME2         NOT NULL CONSTRAINT DF_ChatConversation_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ChatConversation PRIMARY KEY CLUSTERED (ConversationId),
    CONSTRAINT FK_ChatConversation_UserA FOREIGN KEY (UserAId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT FK_ChatConversation_UserB FOREIGN KEY (UserBId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT UQ_ChatConversation_Pair UNIQUE (UserAId, UserBId),
    CONSTRAINT CK_ChatConversation_Order CHECK (UserAId < UserBId)
);
GO

-- Un mensaje pertenece a un canal de proyecto (ProjectId) XOR a una conversacion (ConversationId).
-- ParentMessageId habilita hilos de un solo nivel (responder a un mensaje).
CREATE TABLE dbo.ChatMessage
(
    ChatMessageId    INT IDENTITY(1,1) NOT NULL,
    ProjectId        INT               NULL,
    ConversationId   INT               NULL,
    SenderUserId     INT               NOT NULL,
    ParentMessageId  INT               NULL,
    Text             NVARCHAR(2000)    NOT NULL,
    CreatedDate      DATETIME2         NOT NULL CONSTRAINT DF_ChatMessage_CreatedDate DEFAULT (SYSUTCDATETIME()),
    IsDeleted        BIT               NOT NULL CONSTRAINT DF_ChatMessage_IsDeleted DEFAULT (0),
    CONSTRAINT PK_ChatMessage PRIMARY KEY CLUSTERED (ChatMessageId),
    CONSTRAINT FK_ChatMessage_Project FOREIGN KEY (ProjectId) REFERENCES dbo.Project (ProjectId),
    CONSTRAINT FK_ChatMessage_Conversation FOREIGN KEY (ConversationId) REFERENCES dbo.ChatConversation (ConversationId),
    CONSTRAINT FK_ChatMessage_Sender FOREIGN KEY (SenderUserId) REFERENCES dbo.[User] (UserId),
    CONSTRAINT FK_ChatMessage_Parent FOREIGN KEY (ParentMessageId) REFERENCES dbo.ChatMessage (ChatMessageId),
    CONSTRAINT CK_ChatMessage_ExactlyOneTarget CHECK ((ProjectId IS NULL AND ConversationId IS NOT NULL) OR (ProjectId IS NOT NULL AND ConversationId IS NULL))
);
GO

CREATE TABLE dbo.ChatMessageReaction
(
    ChatMessageId  INT           NOT NULL,
    UserId         INT           NOT NULL,
    Emoji          NVARCHAR(16)  NOT NULL,
    CreatedDate    DATETIME2     NOT NULL CONSTRAINT DF_ChatMessageReaction_CreatedDate DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_ChatMessageReaction PRIMARY KEY CLUSTERED (ChatMessageId, UserId, Emoji),
    CONSTRAINT FK_ChatMessageReaction_Message FOREIGN KEY (ChatMessageId) REFERENCES dbo.ChatMessage (ChatMessageId),
    CONSTRAINT FK_ChatMessageReaction_User FOREIGN KEY (UserId) REFERENCES dbo.[User] (UserId)
);
GO

CREATE NONCLUSTERED INDEX IX_ChatMessage_ProjectId ON dbo.ChatMessage (ProjectId);
CREATE NONCLUSTERED INDEX IX_ChatMessage_ConversationId ON dbo.ChatMessage (ConversationId);
CREATE NONCLUSTERED INDEX IX_ChatMessage_ParentMessageId ON dbo.ChatMessage (ParentMessageId);
CREATE NONCLUSTERED INDEX IX_ChatMessageReaction_ChatMessageId ON dbo.ChatMessageReaction (ChatMessageId);
GO
