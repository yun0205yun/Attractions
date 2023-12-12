CREATE TABLE Content (
    ContentId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(UserId),
    ContentText NVARCHAR(1000), -- Content �����e
    CreatedAt DATETIME DEFAULT GETDATE()
);