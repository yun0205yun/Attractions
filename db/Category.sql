CREATE TABLE Category (
  CateID INT IDENTITY(1,1) NOT NULL, -- �����s���A�۰ʽs���A���i����
  CategoryName NVARCHAR(20) NOT NULL, -- �����W�١A�̤j����20�A���i����
  CreatedAt DATETIME DEFAULT GETDATE() NOT NULL, -- �s�W�ɶ��A�w�]���ثe����ɶ��A���i����
  PRIMARY KEY (CateID) -- �]�w�D��
);
INSERT INTO Category (CategoryName, CreatedAt)
VALUES 
  (N'�_��y', GETDATE()),
  (N'��˭]', GETDATE()),
  (N'������', GETDATE()),
  (N'���ūn', GETDATE());
