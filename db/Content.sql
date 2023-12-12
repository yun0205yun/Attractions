CREATE TABLE Content (
    ContentId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    ContentText NVARCHAR(1000), -- Content 表的內容
    CreatedAt DATETIME DEFAULT GETDATE()
);