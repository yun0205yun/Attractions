CREATE TABLE Category (
  CateID INT IDENTITY(1,1) NOT NULL, -- 分類編號，自動編號，不可為空
  CategoryName NVARCHAR(20) NOT NULL, -- 分類名稱，最大長度20，不可為空
  CreatedAt DATETIME DEFAULT GETDATE() NOT NULL, -- 新增時間，預設為目前日期時間，不可為空
  PRIMARY KEY (CateID) -- 設定主鍵
);
INSERT INTO Category (CategoryName, CreatedAt)
VALUES 
  (N'北基宜', GETDATE()),
  (N'桃竹苗', GETDATE()),
  (N'中彰投', GETDATE()),
  (N'雲嘉南', GETDATE());
