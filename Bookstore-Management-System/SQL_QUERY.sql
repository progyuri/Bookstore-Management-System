-- Создание базы данных
CREATE DATABASE [Bookstore-Management-System10];
GO

-- Использование базы данных
USE [Bookstore-Management-System10];
GO

-- Таблица для жанров
CREATE TABLE Genres (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL
);

-- Таблица для книг
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
	StockQuantity INT NOT NULL DEFAULT 0, -- остатки книги на складе
    CONSTRAINT FK_SequelBook FOREIGN KEY (SequelToBookId) REFERENCES Books(Id)
);

-- Таблица для акций
CREATE TABLE Promotions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    PromotionName NVARCHAR(100) NOT NULL,
    Discount DECIMAL(5, 2) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL
);

-- Таблица для связи книг и акций
CREATE TABLE BookPromotions (
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    PromotionId INT FOREIGN KEY REFERENCES Promotions(Id),
    PRIMARY KEY (BookId, PromotionId)
);

-- Таблица для пользователей
CREATE TABLE Users (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(100) NOT NULL,
    Role NVARCHAR(50) NOT NULL,
    IsActive BIT
);

-- Таблица для менеджеров
CREATE TABLE Managers (
    Id INT PRIMARY KEY IDENTITY(1,1),
    ManagerName NVARCHAR(100) NOT NULL,
    ManagerSurname NVARCHAR(100) NOT NULL,
    UserID INT FOREIGN KEY REFERENCES Users(Id)
);

-- Таблица для покупателей
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



-- Таблица для продаж
CREATE TABLE Sales (
    SaleId INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    SaleDate DATE NOT NULL,
    Quantity INT NOT NULL,
    TotalAmount DECIMAL(18, 2) NOT NULL,
    ManagerId INT FOREIGN KEY REFERENCES Managers(Id),
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id)
);

-- Таблица для отложенных книг
CREATE TABLE ReservedBooks (
    ReservationId INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
    ReservationDate DATE NOT NULL
);



-- Таблица для заказов
CREATE TABLE Orders (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderDate DATE NOT NULL,
    CustomerId INT FOREIGN KEY REFERENCES Customers(Id),
    TotalAmount DECIMAL(18, 2) NOT NULL,
	Status NVARCHAR(50) NOT NULL DEFAULT 'Ожидание' -- Статус заказа (Ожидание, Оплачен, Завершен)
);

-- Хранит информацию о позициях в заказе (книга, количество, цена).
CREATE TABLE OrderItems (
    Id INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT FOREIGN KEY REFERENCES Orders(Id),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    Quantity INT NOT NULL,
    Price DECIMAL(18, 2) NOT NULL,
	Total AS (Quantity * Price) PERSISTED
);


-- Таблица для рейтингов книг
CREATE TABLE BookRatings (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    Rating INT NOT NULL CHECK (Rating BETWEEN 1 AND 5),
    RatingDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- Таблица для популярности авторов
CREATE TABLE AuthorPopularity (
    Id INT PRIMARY KEY IDENTITY(1,1),
    Author NVARCHAR(100) NOT NULL,
    SalesCount INT DEFAULT 0
);

-- Таблица для популярности жанров
CREATE TABLE GenrePopularity (
    Id INT PRIMARY KEY IDENTITY(1,1),
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    SalesCount INT DEFAULT 0
);

-- Таблица для просмотров книг
CREATE TABLE BookViews (
    Id INT PRIMARY KEY IDENTITY(1,1),
    BookId INT FOREIGN KEY REFERENCES Books(Id),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    ViewDate DATETIME NOT NULL DEFAULT GETDATE()
);

-- Таблица для предпочтений пользователей
CREATE TABLE UserPreferences (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    GenreId INT FOREIGN KEY REFERENCES Genres(Id),
    Author NVARCHAR(100)
);

-- Таблица для логирования действий пользователей
CREATE TABLE UserActivityLog (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT FOREIGN KEY REFERENCES Users(Id),
    ActivityType NVARCHAR(50) NOT NULL,
    ActivityDate DATETIME NOT NULL DEFAULT GETDATE(),
    BookId INT FOREIGN KEY REFERENCES Books(Id)
);




-- Индексы для ускорения поиска
CREATE INDEX IX_Books_Title ON Books(Title);
CREATE INDEX IX_Books_Author ON Books(Author);
CREATE INDEX IX_Customers_Email ON Customers(Email);
CREATE INDEX IX_Sales_SaleDate ON Sales(SaleDate);
CREATE INDEX IX_Books_SalesCount ON Books(SalesCount);
CREATE INDEX IX_AuthorPopularity_SalesCount ON AuthorPopularity(SalesCount);
CREATE INDEX IX_GenrePopularity_SalesCount ON GenrePopularity(SalesCount);

-- Триггер для обновления статистики при продаже книги 
CREATE TRIGGER UpdatePopularityStats
ON Sales
AFTER INSERT
AS
BEGIN
    -- Обновляем количество продаж для книги
    UPDATE b
    SET b.SalesCount = b.SalesCount + i.Quantity
    FROM Books b
    INNER JOIN inserted i ON b.Id = i.BookId;

    -- Обновляем количество продаж для автора
    UPDATE ap
    SET ap.SalesCount = ap.SalesCount + i.Quantity
    FROM AuthorPopularity ap
    INNER JOIN Books b ON ap.Author = b.Author
    INNER JOIN inserted i ON b.Id = i.BookId;

    -- Обновляем количество продаж для жанра
    UPDATE gp
    SET gp.SalesCount = gp.SalesCount + i.Quantity
    FROM GenrePopularity gp
    INNER JOIN Books b ON gp.GenreId = b.GenreId
    INNER JOIN inserted i ON b.Id = i.BookId;
END;

-- Триггер для обработки удаления продаж
CREATE TRIGGER UpdatePopularityStatsOnDelete
ON Sales
AFTER DELETE
AS
BEGIN
    -- Уменьшаем количество продаж для книги
    UPDATE b
    SET b.SalesCount = b.SalesCount - d.Quantity
    FROM Books b
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- Уменьшаем количество продаж для автора
    UPDATE ap
    SET ap.SalesCount = ap.SalesCount - d.Quantity
    FROM AuthorPopularity ap
    INNER JOIN Books b ON ap.Author = b.Author
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- Уменьшаем количество продаж для жанра
    UPDATE gp
    SET gp.SalesCount = gp.SalesCount - d.Quantity
    FROM GenrePopularity gp
    INNER JOIN Books b ON gp.GenreId = b.GenreId
    INNER JOIN deleted d ON b.Id = d.BookId;
END;

-- Триггер для обработки обновления продаж
CREATE TRIGGER UpdatePopularityStatsOnUpdate
ON Sales
AFTER UPDATE
AS
BEGIN
    -- Корректируем количество продаж для книги
    UPDATE b
    SET b.SalesCount = b.SalesCount + (i.Quantity - d.Quantity)
    FROM Books b
    INNER JOIN inserted i ON b.Id = i.BookId
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- Корректируем количество продаж для автора
    UPDATE ap
    SET ap.SalesCount = ap.SalesCount + (i.Quantity - d.Quantity)
    FROM AuthorPopularity ap
    INNER JOIN Books b ON ap.Author = b.Author
    INNER JOIN inserted i ON b.Id = i.BookId
    INNER JOIN deleted d ON b.Id = d.BookId;

    -- Корректируем количество продаж для жанра
    UPDATE gp
    SET gp.SalesCount = gp.SalesCount + (i.Quantity - d.Quantity)
    FROM GenrePopularity gp
    INNER JOIN Books b ON gp.GenreId = b.GenreId
    INNER JOIN inserted i ON b.Id = i.BookId
    INNER JOIN deleted d ON b.Id = d.BookId;
END;


-- Заполнение таблиц тестовыми данными
-- Очистка таблиц (если они уже существуют)
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

-- Добавление тестовых данных в таблицу Genres
INSERT INTO Genres (Name)
VALUES 
    ('Фантастика'),
    ('Детектив'),
    ('Роман'),
    ('Научная литература'),
    ('Исторический'),
    ('Биография'),
    ('Поэзия'),
    ('Триллер'),
    ('Фэнтези'),
    ('Классика');

-- Добавление тестовых данных в таблицу Promotions
INSERT INTO Promotions (PromotionName, Discount, StartDate, EndDate)
VALUES
    ('Скидка на фантастику', 10.00, '2023-10-01', '2023-10-31'),
    ('Скидка на детективы', 15.00, '2023-11-01', '2023-11-30'),
    ('Скидка на классику', 20.00, '2023-12-01', '2023-12-31'),
    ('Скидка на фэнтези', 12.00, '2023-10-15', '2023-10-30'),
    ('Черная пятница', 30.00, '2023-11-24', '2023-11-24'), -- Черная пятница
    ('Новогодняя распродажа', 25.00, '2023-12-20', '2024-01-10'); -- Новогодняя распродажа

-- Добавление тестовых данных в таблицу Users
INSERT INTO Users (Username, PasswordHash, Role, IsActive)
VALUES
    ('admin', 'hash1', 'Admin', 1),
    ('manager1', 'hash2', 'Manager', 1),
    ('customer1', 'hash3', 'Customer', 1),
    ('customer2', 'hash4', 'Customer', 1),
    ('customer3', 'hash5', 'Customer', 1);

-- Добавление тестовых данных в таблицу Books
INSERT INTO Books (Title, Author, Publisher, CoverImage, PageCount, GenreId, YearPublished, PublishedDate, CostPrice, SellingPrice, IsSequel, SequelToBookId, StockQuantity, SalesCount)
VALUES
    ('1984', 'Джордж Оруэлл', 'Издательство X', NULL, 328, 1, 1949, '19490608', 5.00, 15.00, 0, NULL, 10, 0),
    ('Мастер и Маргарита', 'Михаил Булгаков', 'Издательство Y', NULL, 384, 3, 1967, '19670101', 7.00, 20.00, 0, NULL,10, 0),
    ('Гарри Поттер и философский камень', 'Джоан Роулинг', 'Издательство Z', NULL, 320, 9, 1997, '19970620', 6.00, 18.00, 0, NULL, 10, 0),
    ('Преступление и наказание', 'Федор Достоевский', 'Издательство A', NULL, 430, 3, 1866, '18660101', 8.00, 22.00, 0, NULL, 10, 0),
    ('Властелин колец', 'Дж. Р. Р. Толкин', 'Издательство B', NULL, 1178, 9, 1954, '19540729', 10.00, 30.00, 0, NULL, 10, 0),
    ('Шерлок Холмс: Собака Баскервилей', 'Артур Конан Дойл', 'Издательство C', NULL, 256, 2, 1902, '19020101', 4.00, 12.00, 0, NULL,10, 0),
    ('451 градус по Фаренгейту', 'Рэй Брэдбери', 'Издательство D', NULL, 256, 1, 1953, '19531019', 5.00, 14.00, 0, NULL, 10, 0),
    ('Анна Каренина', 'Лев Толстой', 'Издательство E', NULL, 864, 3, 1877, '18770101', 9.00, 25.00, 0, NULL, 10, 0),
    ('Игра престолов', 'Джордж Р. Р. Мартин', 'Издательство F', NULL, 694, 9, 1996, '19960801', 12.00, 35.00, 0, NULL, 10, 0),
    ('Мертвые души', 'Николай Гоголь', 'Издательство G', NULL, 352, 3, 1842, '18420101', 6.00, 16.00, 0, NULL, 10, 0);



-- Добавление тестовых данных в таблицу BookPromotions
INSERT INTO BookPromotions (BookId, PromotionId)
VALUES
    (1, 1), -- 1984 участвует в акции "Скидка на фантастику"
    (6, 2), -- Собака Баскервилей участвует в акции "Скидка на детективы"
    (10, 3), -- Мертвые души участвуют в акции "Скидка на классику"
    (3, 4), -- Гарри Поттер участвует в акции "Скидка на фэнтези"
    (5, 5), -- Властелин колец участвует в акции "Черная пятница"
    (8, 6); -- Анна Каренина участвует в акции "Новогодняя распродажа"



-- Добавление тестовых данных в таблицу Customers
INSERT INTO Customers (CustomerName, CustomerSurname, City, Address, Tel, Email, UserID)
VALUES
    ('Иван', 'Иванов', 'Москва', 'ул. Ленина, 10', '+79991234567', 'ivan@example.com', 3),
    ('Петр', 'Петров', 'Санкт-Петербург', 'ул. Пушкина, 5', '+79992345678', 'petr@example.com', 4),
    ('Анна', 'Сидорова', 'Екатеринбург', 'ул. Гагарина, 15', '+79993456789', 'anna@example.com', 5);

-- Добавление тестовых данных в таблицу Managers
INSERT INTO Managers (ManagerName, ManagerSurname, UserID)
VALUES
    ('Сергей', 'Сергеев', 2);

-- Добавление тестовых данных в таблицу Sales
INSERT INTO Sales (BookId, SaleDate, Quantity, TotalAmount, ManagerId, CustomerId)
VALUES
    (1, '2023-10-05', 2, 30.00, 1, 1), -- Продажа книги 1984
    (2, '2023-10-06', 1, 20.00, 1, 2), -- Продажа книги Мастер и Маргарита
    (3, '2023-10-07', 3, 54.00, 1, 3), -- Продажа книги Гарри Поттер
    (4, '2023-10-08', 1, 22.00, 1, 1), -- Продажа книги Преступление и наказание
    (5, '2023-10-09', 2, 60.00, 1, 2); -- Продажа книги Властелин колец

-- Добавление тестовых данных в таблицу Orders
INSERT INTO Orders (OrderDate, CustomerId, TotalAmount, Status)
VALUES
    ('2023-10-05', 1,  30.00 , 'Ожидание' ),
    ('2023-10-06', 2,  20.00, 'Ожидание'),
    ('2023-10-07', 3,  54.00, 'Ожидание' ),
    ('2023-10-08', 1,  22.00, 'Ожидание'),
    ('2023-10-09', 2,  60.00, 'Ожидание' );

-- Добавление тестовых данных в таблицу ReservedBooks
INSERT INTO ReservedBooks (BookId, CustomerId, ReservationDate)
VALUES
    (6, 1, '2023-10-10'), -- Собака Баскервилей зарезервирована
    (7, 2, '2023-10-11'), -- 451 градус по Фаренгейту зарезервирована
    (8, 3, '2023-10-12'); -- Анна Каренина зарезервирована

-- Добавление тестовых данных в таблицу BookRatings
INSERT INTO BookRatings (BookId, UserId, Rating, RatingDate)
VALUES
    (1, 3, 5, '2023-10-05'), -- Пользователь 3 оценил книгу 1984 на 5
    (2, 4, 4, '2023-10-06'), -- Пользователь 4 оценил книгу Мастер и Маргарита на 4
    (3, 5, 5, '2023-10-07'), -- Пользователь 5 оценил книгу Гарри Поттер на 5
    (4, 3, 3, '2023-10-08'), -- Пользователь 3 оценил книгу Преступление и наказание на 3
    (5, 4, 5, '2023-10-09'); -- Пользователь 4 оценил книгу Властелин колец на 5

-- Добавление тестовых данных в таблицу AuthorPopularity
INSERT INTO AuthorPopularity (Author, SalesCount)
VALUES
    ('Джордж Оруэлл', 0),
    ('Михаил Булгаков', 0),
    ('Джоан Роулинг', 0),
    ('Федор Достоевский', 0),
    ('Дж. Р. Р. Толкин', 0),
    ('Артур Конан Дойл', 0),
    ('Рэй Брэдбери', 0),
    ('Лев Толстой', 0),
    ('Джордж Р. Р. Мартин', 0),
    ('Николай Гоголь', 0);

-- Добавление тестовых данных в таблицу GenrePopularity
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

-- Добавление тестовых данных в таблицу BookViews
INSERT INTO BookViews (BookId, UserId, ViewDate)
VALUES
    (1, 3, '2023-10-05'), -- Пользователь 3 просмотрел книгу 1984
    (2, 4, '2023-10-06'), -- Пользователь 4 просмотрел книгу Мастер и Маргарита
    (3, 5, '2023-10-07'), -- Пользователь 5 просмотрел книгу Гарри Поттер
    (4, 3, '2023-10-08'), -- Пользователь 3 просмотрел книгу Преступление и наказание
    (5, 4, '2023-10-09'); -- Пользователь 4 просмотрел книгу Властелин колец

-- Добавление тестовых данных в таблицу UserPreferences
INSERT INTO UserPreferences (UserId, GenreId, Author)
VALUES
    (3, 1, 'Джордж Оруэлл'), -- Пользователь 3 предпочитает фантастику и Джорджа Оруэлла
    (4, 3, 'Михаил Булгаков'), -- Пользователь 4 предпочитает романы и Михаила Булгакова
    (5, 9, 'Джоан Роулинг'); -- Пользователь 5 предпочитает фэнтези и Джоан Роулинг

-- Добавление тестовых данных в таблицу UserActivityLog
INSERT INTO UserActivityLog (UserId, ActivityType, ActivityDate, BookId)
VALUES
    (3, 'View', '2023-10-05', 1), -- Пользователь 3 просмотрел книгу 1984
    (4, 'View', '2023-10-06', 2), -- Пользователь 4 просмотрел книгу Мастер и Маргарита
    (5, 'View', '2023-10-07', 3), -- Пользователь 5 просмотрел книгу Гарри Поттер
    (3, 'Purchase', '2023-10-05', 1), -- Пользователь 3 купил книгу 1984
    (4, 'Purchase', '2023-10-06', 2); -- Пользователь 4 купил книгу Мастер и Маргарита