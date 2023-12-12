CREATE TABLE City (
  CityID INT IDENTITY(1,1) NOT NULL, -- ����ID�A�۰ʽs���A���i����
  CityName NVARCHAR(20) NOT NULL, -- �����W�١A�̤j����20�A���i����
  CreateAt DATETIME DEFAULT GETDATE() NOT NULL, -- �s�W�ɶ��A�w�]���ثe����ɶ��A���i����
  CateID INT NOT NULL, -- ����ID�A��ơA���i����
  PRIMARY KEY (CityID), -- �]�w�D��
  FOREIGN KEY (CateID) REFERENCES Category(CateID) -- �i�H�ھڹ�ڱ��p�[�J�~�����
);
INSERT INTO City (CityName, CreateAt, CateID)
VALUES
  (N'�x�_', GETDATE(), 1),  -- �x�_�A�ݩ�_��y
  (N'��', GETDATE(), 1),  -- �򶩡A�ݩ�_��y
  (N'�y��', GETDATE(), 1),  -- �y���A�ݩ�_��y
  (N'�s�_', GETDATE(), 1),  -- �s�_�A�ݩ�_��y

  (N'���', GETDATE(), 2),  -- ���A�ݩ��˭]
  (N'�s��', GETDATE(), 2),  -- �s�ˡA�ݩ��˭]
  (N'�]��', GETDATE(), 2),  -- �]�ߡA�ݩ��˭]

  (N'�x��', GETDATE(), 3),  -- �x���A�ݩ󤤹���
  (N'����', GETDATE(), 3),  -- ���ơA�ݩ󤤹���
  (N'�n��', GETDATE(), 3),  -- �n��A�ݩ󤤹���

  (N'���L', GETDATE(), 4),  -- ���L�A�ݩ󶳹ūn
  (N'�Ÿq', GETDATE(), 4),  -- �Ÿq�A�ݩ󶳹ūn
  (N'�x�n', GETDATE(), 4);  -- �x�n�A�ݩ󶳹ūn