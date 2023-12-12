CREATE TABLE Attraction (
  AttractionID INT IDENTITY(1,1) NOT NULL, -- 景點ID，自動編號，不可為空
  AttractionTitle NVARCHAR(50) NOT NULL, -- 景點標題，最大長度50，不可為空
  AttractioDesc NVARCHAR(MAX) NOT NULL, -- 景點內容，最大長度（MAX），不可為空
  CreateUserID INT NOT NULL, -- 新增人ID，整數，不可為空
  CreatedAt DATETIME DEFAULT GETDATE() NOT NULL, -- 新增時間，預設為目前日期時間，不可為空
  EditUserID INT NULL, -- 編輯人ID，整數，可為空
  EditAt DATETIME NULL, -- 編輯時間，可為空
  CityID INT NOT NULL, -- 城市ID，整數，不可為空
  Status BIT DEFAULT 1 NOT NULL, -- 資料狀態，位元，預設為1（開啟），不可為空
  PRIMARY KEY (AttractionID), -- 設定主鍵
  FOREIGN KEY (CreateUserID) REFERENCES Users(UserID), -- 可以根據實際情況加入外鍵約束
  FOREIGN KEY (EditUserID) REFERENCES Users(UserID), -- 可以根據實際情況加入外鍵約束
  FOREIGN KEY (CityID) REFERENCES City(CityID) -- 可以根據實際情況加入外鍵約束
);
