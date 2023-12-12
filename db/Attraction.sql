CREATE TABLE Attraction (
  AttractionID INT IDENTITY(1,1) NOT NULL, -- ���IID�A�۰ʽs���A���i����
  AttractionTitle NVARCHAR(50) NOT NULL, -- ���I���D�A�̤j����50�A���i����
  AttractioDesc NVARCHAR(MAX) NOT NULL, -- ���I���e�A�̤j���ס]MAX�^�A���i����
  CreateUserID INT NOT NULL, -- �s�W�HID�A��ơA���i����
  CreatedAt DATETIME DEFAULT GETDATE() NOT NULL, -- �s�W�ɶ��A�w�]���ثe����ɶ��A���i����
  EditUserID INT NULL, -- �s��HID�A��ơA�i����
  EditAt DATETIME NULL, -- �s��ɶ��A�i����
  CityID INT NOT NULL, -- ����ID�A��ơA���i����
  Status BIT DEFAULT 1 NOT NULL, -- ��ƪ��A�A�줸�A�w�]��1�]�}�ҡ^�A���i����
  PRIMARY KEY (AttractionID), -- �]�w�D��
  FOREIGN KEY (CreateUserID) REFERENCES Users(UserID), -- �i�H�ھڹ�ڱ��p�[�J�~�����
  FOREIGN KEY (EditUserID) REFERENCES Users(UserID), -- �i�H�ھڹ�ڱ��p�[�J�~�����
  FOREIGN KEY (CityID) REFERENCES City(CityID) -- �i�H�ھڹ�ڱ��p�[�J�~�����
);
