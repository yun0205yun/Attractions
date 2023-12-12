CREATE TABLE City (
  CityID INT IDENTITY(1,1) NOT NULL, -- 縣市ID，自動編號，不可為空
  CityName NVARCHAR(20) NOT NULL, -- 縣市名稱，最大長度20，不可為空
  CreateAt DATETIME DEFAULT GETDATE() NOT NULL, -- 新增時間，預設為目前日期時間，不可為空
  CateID INT NOT NULL, -- 分類ID，整數，不可為空
  PRIMARY KEY (CityID), -- 設定主鍵
  FOREIGN KEY (CateID) REFERENCES Category(CateID) -- 可以根據實際情況加入外鍵約束
);
INSERT INTO City (CityName, CreateAt, CateID)
VALUES
  (N'台北', GETDATE(), 1),  -- 台北，屬於北基宜
  (N'基隆', GETDATE(), 1),  -- 基隆，屬於北基宜
  (N'宜蘭', GETDATE(), 1),  -- 宜蘭，屬於北基宜
  (N'新北', GETDATE(), 1),  -- 新北，屬於北基宜

  (N'桃園', GETDATE(), 2),  -- 桃園，屬於桃竹苗
  (N'新竹', GETDATE(), 2),  -- 新竹，屬於桃竹苗
  (N'苗栗', GETDATE(), 2),  -- 苗栗，屬於桃竹苗

  (N'台中', GETDATE(), 3),  -- 台中，屬於中彰投
  (N'彰化', GETDATE(), 3),  -- 彰化，屬於中彰投
  (N'南投', GETDATE(), 3),  -- 南投，屬於中彰投

  (N'雲林', GETDATE(), 4),  -- 雲林，屬於雲嘉南
  (N'嘉義', GETDATE(), 4),  -- 嘉義，屬於雲嘉南
  (N'台南', GETDATE(), 4);  -- 台南，屬於雲嘉南