-- �������� ���� ������
CREATE DATABASE [Bookstore-Management-System10];
GO

-- ������������� ���� ������
USE [Bookstore-Management-System10];
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
    CoverImage VARBINARY(MAX),
    PageCount INT,
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    YearPublished INT,
    PublishedDate DATETIME,
    CostPrice DECIMAL(18, 2),
    SellingPrice DECIMAL(18, 2),
    IsSequel BIT DEFAULT 0,
    SequelToBookId INT NULL,
    SalesCount INT DEFAULT 0,
	StockQuantity INT NOT NULL DEFAULT 0, -- ������� ����� �� ������
    CONSTRAINT FK_SequelBook FOREIGN KEY (SequelToBookId) REFERENCES Books(Id)
);

-- ������� ��� �����
CREATE TABLE Promotions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PromotionName NVARCHAR(100) NOT NULL,
    Discount DECIMAL(5, 2) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL
);

-- ������� ��� ����� ���� � �����
CREATE TABLE BookPromotions (
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    PromotionId INT FOREIGN KEY REFERENCES Promotions(Id),
    PRIMARY KEY (BookId, PromotionId)
);

-- ������� ��� �������������
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT
);

-- ������� ��� ����������
CREATE TABLE Managers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ManagerName NVARCHAR(100) NOT NULL,
    ManagerSurname NVARCHAR(100) NOT NULL,
    UserID INT FOREIGN KEY REFERENCES Users(Id)
);

-- ������� ��� �����������
CREATE TABLE Customers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerSurname NVARCHAR(100) NOT NULL,
	FullName AS (CONCAT(CustomerName, ' ', CustomerSurname)) PERSISTED,
    City NVARCHAR(100),
    Address NVARCHAR(255),
    Tel NVARCHAR(20),
    Email NVARCHAR(50),
    UserID INT FOREIGN KEY REFERENCES Users(Id)
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



-- ������� ��� �������
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL,
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
    TotalAmount DECIMAL(18, 2) NOT NULL,
	Status NVARCHAR(50) NOT NULL DEFAULT '��������' -- ������ ������ (��������, �������, ��������)
);

-- ������ ���������� � �������� � ������ (�����, ����������, ����).
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT FOREIGN KEY REFERENCES Orders(Id),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    Quantity INT NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
	Total AS (Quantity * Price) PERSISTED
);


-- ������� ��� ��������� ����
CREATE TABLE BookRatings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    RatingDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- ������� ��� ������������ �������
CREATE TABLE AuthorPopularity (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Author NVARCHAR(100) NOT NULL,
    SalesCount INT DEFAULT 0
);

-- ������� ��� ������������ ������
CREATE TABLE GenrePopularity (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    SalesCount INT DEFAULT 0
);

-- ������� ��� ���������� ����
CREATE TABLE BookViews (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    ViewDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- ������� ��� ������������ �������������
CREATE TABLE UserPreferences (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    Author NVARCHAR(100)
);

-- ������� ��� ����������� �������� �������������
CREATE TABLE UserActivityLog (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    ActivityType NVARCHAR(50) NOT NULL,
    ActivityDate DATETIME NOT NULL DEFAULT GETDATE(),
    BookId INT FOREIGN KEY REFERENCES Books(Id)
);




-- ������� ��� ��������� ������
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Author ON Books(Author);
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Sales_SaleDate ON Sales(SaleDate);
CREATE INDEX IX_Books_SalesCount ON Books(SalesCount);
CREATE INDEX IX_AuthorPopularity_SalesCount ON AuthorPopularity(SalesCount);
CREATE INDEX IX_GenrePopularity_SalesCount ON GenrePopularity(SalesCount);

-- ������� ��� ���������� ���������� ��� ������� ����� 
CREATE TRIGGER UpdatePopularityStats
ON Sales
AFTER INSERT
AS
BEGIN
    -- ��������� ���������� ������ ��� �����
    UPDATE b
    SET b.SalesCount = b.SalesCount + i.Quantity
    FROM Books b
    INNER JOIN inserted i ON b.Id = i.BookId;

    -- ��������� ���������� ������ ��� ������
    UPDATE ap
    SET ap.SalesCount = ap.SalesCount + i.Quantity
    FROM AuthorPopularity ap
    INNER JOIN Books b ON ap.Author = b.Author
    INNER JOIN inserted i ON b.Id = i.BookId;

    -- ��������� ���������� ������ ��� �����
    UPDATE gp
    SET gp.SalesCount = gp.SalesCount + i.Quantity
    FROM GenrePopularity gp
    INNER JOIN Books b ON gp.GenreId = b.GenreId
    INNER JOIN inserted i ON b.Id = i.BookId;
END;

-- ������� ��� ��������� �������� ������
CREATE TRIGGER UpdatePopularityStatsOnDelete
ON Sales
AFTER DELETE
AS
BEGIN
    -- ��������� ���������� ������ ��� �����
    UPDATE b
    SET b.SalesCount = b.SalesCount - d.Quantity
    FROM Books b
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- ��������� ���������� ������ ��� ������
    UPDATE ap
    SET ap.SalesCount = ap.SalesCount - d.Quantity
    FROM AuthorPopularity ap
    INNER JOIN Books b ON ap.Author = b.Author
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- ��������� ���������� ������ ��� �����
    UPDATE gp
    SET gp.SalesCount = gp.SalesCount - d.Quantity
    FROM GenrePopularity gp
    INNER JOIN Books b ON gp.GenreId = b.GenreId
    INNER JOIN deleted d ON b.Id = d.BookId;
END;

-- ������� ��� ��������� ���������� ������
CREATE TRIGGER UpdatePopularityStatsOnUpdate
ON Sales
AFTER UPDATE
AS
BEGIN
    -- ������������ ���������� ������ ��� �����
    UPDATE b
    SET b.SalesCount = b.SalesCount + (i.Quantity - d.Quantity)
    FROM Books b
    INNER JOIN inserted i ON b.Id = i.BookId
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- ������������ ���������� ������ ��� ������
    UPDATE ap
    SET ap.SalesCount = ap.SalesCount + (i.Quantity - d.Quantity)
    FROM AuthorPopularity ap
    INNER JOIN Books b ON ap.Author = b.Author
    INNER JOIN inserted i ON b.Id = i.BookId
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- ������������ ���������� ������ ��� �����
    UPDATE gp
    SET gp.SalesCount = gp.SalesCount + (i.Quantity - d.Quantity)
    FROM GenrePopularity gp
    INNER JOIN Books b ON gp.GenreId = b.GenreId
    INNER JOIN inserted i ON b.Id = i.BookId
    INNER JOIN deleted d ON b.Id = d.BookId;
END;


-- ���������� ������ ��������� �������
-- ������� ������ (���� ��� ��� ����������)
DELETE FROM UserActivityLog;
DELETE FROM UserPreferences;
DELETE FROM BookViews;
DELETE FROM BookRatings;
DELETE FROM AuthorPopularity;
DELETE FROM GenrePopularity;
DELETE FROM BookPromotions;
DELETE FROM Sales;
DELETE FROM Orders;
DELETE FROM ReservedBooks;
DELETE FROM Promotions;
DELETE FROM Books;
DELETE FROM Genres;
DELETE FROM Managers;
DELETE FROM Customers;
DELETE FROM Users;

-- ���������� �������� ������ � ������� Genres
INSERT INTO Genres (Name)
VALUES 
    ('����������'),
    ('��������'),
    ('�����'),
    ('������� ����������'),
    ('������������'),
    ('���������'),
    ('������'),
    ('�������'),
    ('�������'),
    ('��������');

-- ���������� �������� ������ � ������� Promotions
INSERT INTO Promotions (PromotionName, Discount, StartDate, EndDate)
VALUES
    ('������ �� ����������', 10.00, '2023-10-01', '2023-10-31'),
    ('������ �� ���������', 15.00, '2023-11-01', '2023-11-30'),
    ('������ �� ��������', 20.00, '2023-12-01', '2023-12-31'),
    ('������ �� �������', 12.00, '2023-10-15', '2023-10-30'),
    ('������ �������', 30.00, '2023-11-24', '2023-11-24'), -- ������ �������
    ('���������� ����������', 25.00, '2023-12-20', '2024-01-10'); -- ���������� ����������

-- ���������� �������� ������ � ������� Users
INSERT INTO Users (Username, PasswordHash, Role, IsActive)
VALUES
    ('admin', 'hash1', 'Admin', 1),
    ('manager1', 'hash2', 'Manager', 1),
    ('customer1', 'hash3', 'Customer', 1),
    ('customer2', 'hash4', 'Customer', 1),
    ('customer3', 'hash5', 'Customer', 1);

-- ���������� �������� ������ � ������� Books
INSERT INTO Books (Title, Author, Publisher, CoverImage, PageCount, GenreId, YearPublished, PublishedDate, CostPrice, SellingPrice, IsSequel, SequelToBookId, StockQuantity, SalesCount)
VALUES
    ('1984', '������ ������', '������������ X', NULL, 328, 1, 1949, '19490608', 5.00, 15.00, 0, NULL, 10, 0),
    ('������ � ���������', '������ ��������', '������������ Y', NULL, 384, 3, 1967, '19670101', 7.00, 20.00, 0, NULL,10, 0),
    ('����� ������ � ����������� ������', '����� �������', '������������ Z', NULL, 320, 9, 1997, '19970620', 6.00, 18.00, 0, NULL, 10, 0),
    ('������������ � ���������', '����� �����������', '������������ A', NULL, 430, 3, 1866, '18660101', 8.00, 22.00, 0, NULL, 10, 0),
    ('��������� �����', '��. �. �. ������', '������������ B', NULL, 1178, 9, 1954, '19540729', 10.00, 30.00, 0, NULL, 10, 0),
    ('������ �����: ������ �����������', '����� ����� ����', '������������ C', NULL, 256, 2, 1902, '19020101', 4.00, 12.00, 0, NULL,10, 0),
    ('451 ������ �� ����������', '��� ��������', '������������ D', NULL, 256, 1, 1953, '19531019', 5.00, 14.00, 0, NULL, 10, 0),
    ('���� ��������', '��� �������', '������������ E', NULL, 864, 3, 1877, '18770101', 9.00, 25.00, 0, NULL, 10, 0),
    ('���� ���������', '������ �. �. ������', '������������ F', NULL, 694, 9, 1996, '19960801', 12.00, 35.00, 0, NULL, 10, 0),
    ('������� ����', '������� ������', '������������ G', NULL, 352, 3, 1842, '18420101', 6.00, 16.00, 0, NULL, 10, 0);



-- ���������� �������� ������ � ������� BookPromotions
INSERT INTO BookPromotions (BookId, PromotionId)
VALUES
    (1, 1), -- 1984 ��������� � ����� "������ �� ����������"
    (6, 2), -- ������ ����������� ��������� � ����� "������ �� ���������"
    (10, 3), -- ������� ���� ��������� � ����� "������ �� ��������"
    (3, 4), -- ����� ������ ��������� � ����� "������ �� �������"
    (5, 5), -- ��������� ����� ��������� � ����� "������ �������"
    (8, 6); -- ���� �������� ��������� � ����� "���������� ����������"



-- ���������� �������� ������ � ������� Customers
INSERT INTO Customers (CustomerName, CustomerSurname, City, Address, Tel, Email, UserID)
VALUES
    ('����', '������', '������', '��. ������, 10', '+79991234567', 'ivan@example.com', 3),
    ('����', '������', '�����-���������', '��. �������, 5', '+79992345678', 'petr@example.com', 4),
    ('����', '��������', '������������', '��. ��������, 15', '+79993456789', 'anna@example.com', 5);

-- ���������� �������� ������ � ������� Managers
INSERT INTO Managers (ManagerName, ManagerSurname, UserID)
VALUES
    ('������', '�������', 2);

-- ���������� �������� ������ � ������� Sales
INSERT INTO Sales (BookId, SaleDate, Quantity, TotalAmount, ManagerId, CustomerId)
VALUES
    (1, '2023-10-05', 2, 30.00, 1, 1), -- ������� ����� 1984
    (2, '2023-10-06', 1, 20.00, 1, 2), -- ������� ����� ������ � ���������
    (3, '2023-10-07', 3, 54.00, 1, 3), -- ������� ����� ����� ������
    (4, '2023-10-08', 1, 22.00, 1, 1), -- ������� ����� ������������ � ���������
    (5, '2023-10-09', 2, 60.00, 1, 2); -- ������� ����� ��������� �����

-- ���������� �������� ������ � ������� Orders
INSERT INTO Orders (OrderDate, CustomerId, TotalAmount, Status)
VALUES
    ('2023-10-05', 1,  30.00 , '��������' ),
    ('2023-10-06', 2,  20.00, '��������'),
    ('2023-10-07', 3,  54.00, '��������' ),
    ('2023-10-08', 1,  22.00, '��������'),
    ('2023-10-09', 2,  60.00, '��������' );

-- ���������� �������� ������ � ������� ReservedBooks
INSERT INTO ReservedBooks (BookId, CustomerId, ReservationDate)
VALUES
    (6, 1, '2023-10-10'), -- ������ ����������� ���������������
    (7, 2, '2023-10-11'), -- 451 ������ �� ���������� ���������������
    (8, 3, '2023-10-12'); -- ���� �������� ���������������

-- ���������� �������� ������ � ������� BookRatings
INSERT INTO BookRatings (BookId, UserId, Rating, RatingDate)
VALUES
    (1, 3, 5, '2023-10-05'), -- ������������ 3 ������ ����� 1984 �� 5
    (2, 4, 4, '2023-10-06'), -- ������������ 4 ������ ����� ������ � ��������� �� 4
    (3, 5, 5, '2023-10-07'), -- ������������ 5 ������ ����� ����� ������ �� 5
    (4, 3, 3, '2023-10-08'), -- ������������ 3 ������ ����� ������������ � ��������� �� 3
    (5, 4, 5, '2023-10-09'); -- ������������ 4 ������ ����� ��������� ����� �� 5

-- ���������� �������� ������ � ������� AuthorPopularity
INSERT INTO AuthorPopularity (Author, SalesCount)
VALUES
    ('������ ������', 0),
    ('������ ��������', 0),
    ('����� �������', 0),
    ('����� �����������', 0),
    ('��. �. �. ������', 0),
    ('����� ����� ����', 0),
    ('��� ��������', 0),
    ('��� �������', 0),
    ('������ �. �. ������', 0),
    ('������� ������', 0);

-- ���������� �������� ������ � ������� GenrePopularity
INSERT INTO GenrePopularity (GenreId, SalesCount)
VALUES
    (1, 0),
    (2, 0),
    (3, 0),
    (4, 0),
    (5, 0),
    (6, 0),
    (7, 0),
    (8, 0),
    (9, 0),
    (10, 0);

-- ���������� �������� ������ � ������� BookViews
INSERT INTO BookViews (BookId, UserId, ViewDate)
VALUES
    (1, 3, '2023-10-05'), -- ������������ 3 ���������� ����� 1984
    (2, 4, '2023-10-06'), -- ������������ 4 ���������� ����� ������ � ���������
    (3, 5, '2023-10-07'), -- ������������ 5 ���������� ����� ����� ������
    (4, 3, '2023-10-08'), -- ������������ 3 ���������� ����� ������������ � ���������
    (5, 4, '2023-10-09'); -- ������������ 4 ���������� ����� ��������� �����

-- ���������� �������� ������ � ������� UserPreferences
INSERT INTO UserPreferences (UserId, GenreId, Author)
VALUES
    (3, 1, '������ ������'), -- ������������ 3 ������������ ���������� � ������� �������
    (4, 3, '������ ��������'), -- ������������ 4 ������������ ������ � ������� ���������
    (5, 9, '����� �������'); -- ������������ 5 ������������ ������� � ����� �������

-- ���������� �������� ������ � ������� UserActivityLog
INSERT INTO UserActivityLog (UserId, ActivityType, ActivityDate, BookId)
VALUES
    (3, 'View', '2023-10-05', 1), -- ������������ 3 ���������� ����� 1984
    (4, 'View', '2023-10-06', 2), -- ������������ 4 ���������� ����� ������ � ���������
    (5, 'View', '2023-10-07', 3), -- ������������ 5 ���������� ����� ����� ������
    (3, 'Purchase', '2023-10-05', 1), -- ������������ 3 ����� ����� 1984
    (4, 'Purchase', '2023-10-06', 2); -- ������������ 4 ����� ����� ������ � ���������