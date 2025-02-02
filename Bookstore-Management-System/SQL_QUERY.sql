-- �������� ���� ������
CREATE DATABASE [Bookstore-Management-System];
GO

-- ������������� ���� ������
USE [Bookstore-Management-System];
GO

-- ������� ��� ������
CREATE TABLE Genres (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL
);

-- ������� ��� ����
CREATE TABLE Books (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    Author NVARCHAR(100) NOT NULL,
    Publisher NVARCHAR(100),
	CoverImage varbinary(max), ---- ����������� ��������� ��������
    PageCount INT,
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    YearPublished INT,
	PublishedDate datetime,
    CostPrice DECIMAL(18, 2),
    SellingPrice DECIMAL(18, 2),
    IsSequel BIT DEFAULT 0, -- �������� �� ������������ (0 ��� 1)
    SequelToBookId INT NULL, -- ������ �� ������ ����� (���� ��� �����������)
    CONSTRAINT FK_SequelBook FOREIGN KEY (SequelToBookId) REFERENCES Books(Id)
);

-- ������� ��� �����
CREATE TABLE Promotions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PromotionName NVARCHAR(100) NOT NULL,
    Discount DECIMAL(5, 2) NOT NULL, -- ������ � ���������
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL
);

-- ������� ��� ����� ���� � ����� (������ �� ������)
CREATE TABLE BookPromotions (
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    PromotionId INT FOREIGN KEY REFERENCES Promotions(Id),
    PRIMARY KEY (BookId, PromotionId)
);

-- ������� ��� ����������
CREATE TABLE Managers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ManagerName NVARCHAR(100) NOT NULL,
    ManagerSurname NVARCHAR(100) NOT NULL
);

-- ������� ��� �����������
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerSurname NVARCHAR(100) NOT NULL,
    City NVARCHAR(100),
    Address NVARCHAR(255),
    Tel NVARCHAR(20),
    Email NVARCHAR(50)
);

-- ������� ��� ������
CREATE TABLE Sales (
    SaleId INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    SaleDate DATE NOT NULL,
    Quantity INT NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    ManagerId INT FOREIGN KEY REFERENCES Managers(Id),
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id)
);

-- ������� ��� ���������� ����
CREATE TABLE ReservedBooks (
    ReservationId INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
    ReservationDate DATE NOT NULL
);

-- ������� ��� �������������
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(100) NOT NULL, -- ��� ������
    Role NVARCHAR(50) NOT NULL -- ���� ������������ (��������, "Admin", "User")
);

CREATE TABLE Order (
	Id INT PRIMARY KEY IDENTITY,
	OrderDate DATE NOT NULL,
	CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
	BookId INT FOREIGN KEY REFERENCES Books(Id),
	Quantity INT NOT NULL,
	TotalAmount DECIMAL(18, 2) NOT NULL
	);

-- ������� ��� ��������� ������
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Author ON Books(Author);
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Sales_SaleDate ON Sales(SaleDate);


--- ������ ��� ���������� ������ ��������� �������
-- ������� ������ � ������� Genres
INSERT INTO Genres (Name)
VALUES
('�����'),
('�������'),
('����������'),
('��������'),
('������� ����������'),
('������������'),
('���������');

-- ������� ������ � ������� Books
INSERT INTO Books (Title, Author, Publisher, PageCount, GenreId, YearPublished, CostPrice, SellingPrice, IsSequel, SequelToBookId)
VALUES
('����� � ���', '��� �������', '�����', 1225, 1, 1869, 500.00, 1200.00, 0, NULL),
('������������ � ���������', 'Ը��� �����������', '���', 672, 1, 1866, 300.00, 950.00, 0, NULL),
('1984', '������ ������', '������', 328, 3, 1949, 200.00, 800.00, 0, NULL),
('������ � ���������', '������ ��������', '�����', 480, 1, 1967, 400.00, 1100.00, 0, NULL),
('����� ������ � ����������� ������', '����� �������', '������', 432, 2, 1997, 350.00, 900.00, 0, NULL),
('����� ������ � ������ �������', '����� �������', '������', 480, 2, 1998, 350.00, 900.00, 1, 5),
('��������� �����', '������ �� ����-��������', '�����', 96, 1, 1943, 150.00, 500.00, 0, NULL);

/*
- ������� ������ � ������� Books � �������������
INSERT INTO Books (Title, Author, Publisher, PageCount, GenreId, YearPublished, CostPrice, SellingPrice, IsSequel, SequelToBookId, CoverImage)
VALUES
('����� � ���', '��� �������', '�����', 1225, 1, 1869, 500.00, 1200.00, 0, NULL, (SELECT BulkColumn FROM OPENROWSET(BULK 'C:\path_to_image1.jpg', SINGLE_BLOB) AS img)),
('������������ � ���������', 'Ը��� �����������', '���', 672, 1, 1866, 300.00, 950.00, 0, NULL, (SELECT BulkColumn FROM OPENROWSET(BULK 'C:\path_to_image2.jpg', SINGLE_BLOB) AS img));
*/

-- ������� ������ � ������� Promotions
INSERT INTO Promotions (PromotionName, Discount, StartDate, EndDate)
VALUES
('���������� ����������', 10.00, '2023-12-20', '2024-01-10'),
('������ �������', 20.00, '2023-11-24', '2023-11-26');

-- ������� ������ � ������� BookPromotions
INSERT INTO BookPromotions (BookId, PromotionId)
VALUES
(1, 1), -- ����� � ��� � ���������� ����������
(5, 1), -- ����� ������ � ����������� ������ � ���������� ����������
(3, 2); -- 1984 � ������ �������

-- ������� ������ � ������� Managers
INSERT INTO Managers (ManagerName, ManagerSurname)
VALUES
('����', '������'),
('�����', '�������');

-- ������� ������ � ������� Customers
INSERT INTO Customers (CustomerName, CustomerSurname, City, Address, Tel, Email)
VALUES
('�������', '�������', '������', '��. ������, 10', '+79161234567', 'alex@example.com'),
('�����', '���������', '�����-���������', '��. �������, 5', '+79167654321', 'elena@example.com');

-- ������� ������ � ������� Sales
INSERT INTO Sales (BookId, SaleDate, Quantity, TotalAmount, ManagerId, CustomerId)
VALUES
(1, '2023-10-01', 2, 2400.00, 1, 1),
(5, '2023-10-02', 1, 900.00, 2, 2),
(3, '2023-10-03', 3, 2400.00, 1, 1);

-- ������� ������ � ������� ReservedBooks
INSERT INTO ReservedBooks (BookId, CustomerId, ReservationDate)
VALUES
(2, 1, '2023-10-05'), -- ������������ � ��������� �������� ��� �������
(4, 2, '2023-10-06'); -- ������ � ��������� �������� ��� �����

-- ������� ������ � ������� Users
INSERT INTO Users (Username, PasswordHash, Role)
VALUES
('admin', 'hashed_password_1', 'Admin'), -- ������ ������ ���� ���������
('user', 'hashed_password_2', 'Customer'), -- ������ ������ ���� ���������
('user2', 'hashed_password_3', 'Customer'), -- ������ ������ ���� ���������
('IvanovI', 'hashed_password_4', 'Managers'), -- ������ ������ ���� ���������
('PetrovaM', 'hashed_password_4', 'Managers'); -- ������ ������ ���� ���������


